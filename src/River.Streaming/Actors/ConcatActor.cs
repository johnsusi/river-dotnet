using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace River.Streaming.Actors
{

  internal class ConcatActor<T> : AbstractActor
  {
    private readonly IAsyncEnumerable<Producer<T>> _producers;
    private readonly ChannelOptions? _options = null;
    public Producer<T> Outbox { get; } = new Producer<T>();

    public ConcatActor(IAsyncEnumerable<Producer<T>> producers, ChannelOptions? options = null)
    {
      _producers = producers;
      _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
      using (Outbox)
      {
        var channel = Channel.CreateUnbounded<Consumer<T>>();
        var task = Write(channel, Outbox, cancellationToken);
        try
        {
          await foreach (var producer in _producers)
          {
            var consumer = new Consumer<T>();
            producer.LinkTo(consumer, _options);
            await channel.Writer.WriteAsync(consumer, cancellationToken);
          }
        }
        finally
        {
          channel.Writer.Complete();
        }
        await task;
      }
    }

    private static async Task Write(ChannelReader<Consumer<T>> consumers, ChannelWriter<T> writer, CancellationToken cancellationToken)
    {
      await foreach (var reader in consumers.ReadAllAsync(cancellationToken))
        await foreach (var item in reader.ReadAllAsync(cancellationToken))
          await writer.WriteAsync(item, cancellationToken);
    }
  }
}
