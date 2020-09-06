using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using River.Streaming.Helpers;

namespace River.Streaming.Actors
{
  internal class TransformActor<TIn, TOut> : ActionActor
  {
    public IConsumer<TIn> Inbox { get; } = new Consumer<TIn>();
    public IProducer<TOut> Outbox { get; } = new Producer<TOut>();

    public TransformActor(Func<ChannelReader<TIn>, ChannelWriter<TOut>, CancellationToken, Task> transform)
    {
      Action = async cancellationToken =>
      {
        using var reader = await Inbox.GetReaderAsync(cancellationToken);
        using var writer = await Outbox.GetWriterAsync(cancellationToken);
        await transform(reader, writer, cancellationToken);
      };
    }

    public TransformActor(Func<TIn, ValueTask<TOut>> transform)
      : this(async (reader, writer, cancellationToken) =>
        {
          await foreach (var msg in reader.ReadAllAsync(cancellationToken))
            await writer.WriteAsync(await transform(msg), cancellationToken);
        })
    {
    }

    public TransformActor(Func<TIn, TOut> transform)
      : this(x => new ValueTask<TOut>(transform(x)))
    {
    }

  }
}