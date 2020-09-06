using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace River.Streaming.Helpers
{

  public class DisposableChannelWriter<T> : ChannelWriter<T>, IDisposable
  {

    private readonly ChannelWriter<T> _writer;
    private int _refCount = 1;

    public DisposableChannelWriter(ChannelWriter<T> writer) => _writer = writer;

    internal void Retain() => Interlocked.Increment(ref _refCount);
    internal bool Release() => Interlocked.Decrement(ref _refCount) == 0;

    public void Dispose()
    {
      if (Release())
        _writer.TryComplete();
    }

    public override bool TryWrite(T item) => _writer.TryWrite(item);

    public override async ValueTask<bool> WaitToWriteAsync(CancellationToken cancellationToken = default)
      => await _writer.WaitToWriteAsync(cancellationToken);
  }


}