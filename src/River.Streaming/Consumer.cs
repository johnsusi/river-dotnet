using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace River.Streaming
{

  public class Consumer<T> : ChannelReader<T>, IDisposable
  {

    private readonly TaskCompletionSource<RefCountChannel<T>> _source = new TaskCompletionSource<RefCountChannel<T>>(TaskCreationOptions.RunContinuationsAsynchronously);

    internal TaskCompletionSource<RefCountChannel<T>> Source => _source;

    private RefCountChannel<T>? _channel = null;
    private ChannelReader<T>? _reader = null;

    public void Dispose()
    {
    }

    private readonly Task _completion;

    public override Task Completion => _completion;

    public Consumer()
    {
      _completion = Task.Run(async () =>
      {
        var channel = await _source.Task;
        await channel.Reader.Completion;
      });
    }

    public override bool TryRead(out T item)
    {
      if (_reader is null)
      {
        #nullable disable
        item = default;
        #nullable restore
        return false;
      }
      return _reader.TryRead(out item);
    }

    public override async ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default)
    {
      if (_reader is null)
      {
        var timeout = Task.Delay(Timeout.Infinite, cancellationToken);
        var task = await Task.WhenAny(_source.Task, timeout);
        if (task == timeout)
          cancellationToken.ThrowIfCancellationRequested();
        _channel = await _source.Task;
        _reader = _channel.Reader;
      }
      return await _reader.WaitToReadAsync(cancellationToken);
    }
  }
}