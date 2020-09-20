

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using River.Streaming.Helpers;

[assembly: InternalsVisibleTo("River.Streaming.Test")]
namespace River.Streaming.Actors
{

  public interface IIntervalSource
  {
    Task NextInterval { get; }
    void Reset();
  }

  public class IntervalSource : IIntervalSource
  {
    private readonly Channel<DateTime> _channel;
    private readonly Timer _timer;
    private readonly TimeSpan _duration;

    public IntervalSource(TimeSpan duration)
    {
      _duration = duration;
      _channel = Channel.CreateBounded<DateTime>(new BoundedChannelOptions(1)
      {
        FullMode = BoundedChannelFullMode.DropOldest
      });
      _timer = new Timer(new TimerCallback(state =>
      {
        _channel.Writer.TryWrite(DateTime.Now);
      }), null, duration, duration);
    }

    public Task NextInterval => Task.Run(async () => await _channel.Reader.ReadAsync());

    public void Reset()
    {
      _timer.Change(Timeout.Infinite, Timeout.Infinite);
      _timer.Change(_duration, _duration);
    }
  }
  internal class WindowActor<T> : AbstractActor
  {
    public IConsumer<T> Inbox { get; } = new Consumer<T>();
    public IProducer<IWindowProducer<T>> Outbox { get; } = new Producer<IWindowProducer<T>>();

    public int WindowSize { get; }

    public TimeSpan WindowDuration { get; }

    public WindowActor(int size = int.MaxValue, TimeSpan? duration = null)
    {
      if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size), "Window size must be larger than 0");
      WindowSize = size;
      if (duration.HasValue)
        WindowDuration = duration.Value;
      else
        WindowDuration = Timeout.InfiniteTimeSpan;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
      using var reader = await Inbox.GetReaderAsync();
      using var writer = await Outbox.GetWriterAsync();
      int count = 0;
      WindowProducer<T>? windowProducer = null;
      DisposableChannelWriter<T>? windowWriter = null;
      var intervalSource = new IntervalSource(WindowDuration);
      try
      {
        await foreach (var item in reader.ReadAllAsync(cancellationToken))
        {
          if (count == 0)
          {
            try { windowWriter?.Dispose(); }
            finally { windowWriter = null; }
            windowProducer = new WindowProducer<T>(windowProducer?.Index ?? 0 + 1, WindowDuration, WindowSize);
            await writer.WriteAsync(windowProducer, cancellationToken);
            windowWriter = await windowProducer.GetWriterAsync(cancellationToken);
            intervalSource.Reset();
          }
          if (windowWriter != null) await windowWriter.WriteAsync(item, cancellationToken);
          if (++count >= WindowSize) count = 0;
          if (intervalSource.NextInterval.IsCompleted) count = 0;
        }
      }
      finally
      {
        windowWriter?.Dispose();
      }

    }
  }
}
