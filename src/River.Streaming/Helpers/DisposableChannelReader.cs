using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace River.Streaming.Helpers
{

  public class DisposableChannelReader<T> : ChannelReader<T>, IDisposable
  {

    private readonly ChannelReader<T> _reader;
    private int _refCount = 1;

    public DisposableChannelReader(ChannelReader<T> reader) => _reader = reader;

    internal void Retain() => Interlocked.Increment(ref _refCount);
    internal bool Release() => Interlocked.Decrement(ref _refCount) == 0;

    public void Dispose() => Release();

    public override bool TryRead(out T item)
      => _reader.TryRead(out item);

    public override ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default)
      => _reader.WaitToReadAsync(cancellationToken);

  }
}