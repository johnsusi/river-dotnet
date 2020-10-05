using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace River.Streaming
{

  public class RefCountChannel<T>
  {

    private int _refCount = 1;
    public ChannelReader<T> Reader { get; }
    public ChannelWriter<T> Writer { get; }
    public RefCountChannel(Channel<T> channel)
    {
      Reader = channel;
      Writer = channel;
    }

    public void Retain() => Interlocked.Increment(ref _refCount);
    public bool Release() => Interlocked.Decrement(ref _refCount) == 0;

  }

  interface IProducer<T> : IDisposable
  {
    bool TryWrite(T item);
    ValueTask<bool> WaitToWriteAsync(CancellationToken cancellationToken = default);
    bool TryComplete(Exception? error = null);
    ValueTask WriteAsync(T item, CancellationToken cancellationToken = default);

  }

  public class Producer<T> : ChannelWriter<T>, /*IProducer<T>, */IDisposable
  {

    private readonly TaskCompletionSource<RefCountChannel<T>> _source = new TaskCompletionSource<RefCountChannel<T>>(TaskCreationOptions.RunContinuationsAsynchronously);

    internal TaskCompletionSource<RefCountChannel<T>> Source => _source;

    private RefCountChannel<T>? _channel = null;
    private ChannelWriter<T>? _writer = null;


    private bool _disposed = false;
    ~Producer() => Dispose(false);

    protected virtual void Dispose(bool disposing)
    {
      if (_disposed) return;

      if (_channel is null && _source.Task.IsCompletedSuccessfully)
      {
        _channel = _source.Task.Result;
        _writer = _channel.Writer;
      }
      if (_channel?.Release() ?? false)
        _writer?.TryComplete();

      _channel = null;
      _writer = null;
      _disposed = true;
    }


    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    public override bool TryWrite(T item) => !(_writer is null) && _writer.TryWrite(item);

    public override async ValueTask<bool> WaitToWriteAsync(CancellationToken cancellationToken = default)
    {
      if (_writer is null)
      {
        var timeout = Task.Delay(Timeout.Infinite, cancellationToken);
        var task = await Task.WhenAny(_source.Task, timeout);
        if (task == timeout)
          cancellationToken.ThrowIfCancellationRequested();
        _channel = await _source.Task;
        _writer = _channel.Writer;
      }
      return await _writer.WaitToWriteAsync(cancellationToken);
    }

  }

  // public class DecoratedProducer<T> : IProducer<T>
  // {
  //   private readonly Producer<T> _component;

  //   public class DecoratedProducer<T> _component

  //   private override void Dispose(bool disposing) => parent.Dispose(disposing);
  // }
}