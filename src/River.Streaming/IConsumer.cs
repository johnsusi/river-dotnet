using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using River.Streaming.Helpers;

namespace River.Streaming
{
  public interface IConsumer<T>
  {
    Task<DisposableChannelReader<T>> GetReaderAsync(CancellationToken cancellationToken = default);
    internal TaskCompletionSource<Channel<T>> Source { get; }

  }
}