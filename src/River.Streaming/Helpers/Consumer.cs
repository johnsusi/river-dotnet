using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using River.Streaming;

[assembly: InternalsVisibleTo("River.Streaming.Test")]
namespace River.Streaming.Helpers
{
  public class Consumer<T> : IConsumer<T>
  {
    private TaskCompletionSource<Channel<T>> _source = new TaskCompletionSource<Channel<T>>(TaskCreationOptions.RunContinuationsAsynchronously);

    TaskCompletionSource<Channel<T>> IConsumer<T>.Source => _source;

    public async Task<DisposableChannelReader<T>> GetReaderAsync(CancellationToken cancellationToken = default)
    {
      return await Task.Run(async () =>
      {
        var result = await _source.Task;
        if (result.Reader is DisposableChannelReader<T> reader) return reader;
        throw new Exception("Not an instant of DisposableChannelReader");
      }, cancellationToken);

    }

  }
}