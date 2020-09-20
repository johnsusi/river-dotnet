using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using River.Streaming.Helpers;


[assembly: InternalsVisibleTo("River.Streaming.Test")]
namespace River.Streaming
{
  public interface IConsumer<T>
  {
    Task<DisposableChannelReader<T>> GetReaderAsync(CancellationToken cancellationToken = default);
    internal TaskCompletionSource<Channel<T>> Source { get; }

  }
}