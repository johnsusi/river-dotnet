using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using River.Streaming.Helpers;

namespace River.Streaming
{
  public interface IProducer<T>
  {
    Task<DisposableChannelWriter<T>> GetWriterAsync(CancellationToken cancellationToken = default);
    internal TaskCompletionSource<Channel<T>> Source { get; }

  }
}