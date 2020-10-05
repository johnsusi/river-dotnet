using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using River.Streaming.Helpers;

[assembly: InternalsVisibleTo("River.Streaming.Test")]
namespace River.Streaming.Actors
{

  internal class WindowActor<T> : AbstractActor
  {
    public Consumer<T> Inbox { get; } = new Consumer<T>();
    public Producer<WindowProducer<T>> Outbox { get; } = new Producer<WindowProducer<T>>();
    public int WindowSize { get; }
    public TimeSpan WindowDuration { get; }

    public WindowActor(int size = int.MaxValue, TimeSpan? duration = null)
    {
      if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size), "Window size must be larger than 0");
      WindowSize = size;
      WindowDuration = duration ?? TimeSpan.MaxValue;
    }

    private static Func<int, DateTime, bool> NewWindow(int startCount, DateTime startTime, int windowSize, TimeSpan windowDuration)
    {
      return (count, now) => (count - startCount) >= windowSize || (now - startTime) >= windowDuration;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
      using (Outbox)
      using (Inbox)
      {
        int index = 0, count = 0;
        var windows = new List<(Func<int, DateTime, bool>, Producer<T>)>();
        try
        {
          var now = DateTime.UtcNow;
          await foreach (var item in Inbox.ReadAllAsync(cancellationToken))
          {
            for (int i = windows.Count() - 1; i >= 0; --i)
            {
              var (shouldClose, writer) = windows[i];
              if (shouldClose(count, now))
              {
                writer.Dispose();
                windows.RemoveAt(i);
              }
            }
            if (windows.Count() == 0)
              windows.Add(await CreateWindow(++index, count, now, cancellationToken));
            foreach (var (_, writer) in windows)
              await writer.WriteAsync(item, cancellationToken);
            ++count;
          }
        }
        finally
        {
          foreach (var (_, writer) in windows) writer.Dispose();
        }
      }
    }

    protected async Task<(Func<int, DateTime, bool>, Producer<T>)> CreateWindow(int index, int count, DateTime now, CancellationToken cancellationToken)
    {
      var windowProducer = new WindowProducer<T>(index, WindowDuration, WindowSize);
      await Outbox.WriteAsync(windowProducer, cancellationToken);
      return (NewWindow(count, now, WindowSize, WindowDuration), windowProducer);
    }
  }
}



