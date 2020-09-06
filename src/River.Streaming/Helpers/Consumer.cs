using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using River.Streaming;

namespace River.Streaming.Helpers
{
  public class Consumer<T> : IConsumer<T>
  {
    private TaskCompletionSource<Channel<T>> _source = new TaskCompletionSource<Channel<T>>(TaskCreationOptions.RunContinuationsAsynchronously);

    TaskCompletionSource<Channel<T>> IConsumer<T>.Source => _source;

    public async Task<DisposableChannelReader<T>> GetReaderAsync(CancellationToken cancellationToken = default)
    {
      var result = await _source.Task;
      return result.Reader as DisposableChannelReader<T> ?? throw new Exception("Reader is null");
    }

  }
}