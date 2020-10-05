using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace River.Streaming.Actors
{
  internal class TransformActor<TIn, TOut> : ActionActor
  {
    public Consumer<TIn> Inbox { get; }
    public Producer<TOut> Outbox { get; } = new Producer<TOut>();

    public TransformActor(Consumer<TIn> inbox, Func<ChannelReader<TIn>, ChannelWriter<TOut>, CancellationToken, Task> transform)
    {
      Inbox = inbox;
      Action = async cancellationToken =>
      {
        using (Outbox)
        using (Inbox)
        {
          await Outbox.WaitToWriteAsync(cancellationToken);
          await Inbox.WaitToReadAsync(cancellationToken);
          await transform(Inbox, Outbox, cancellationToken);
        }
      };
    }

    public TransformActor(Func<ChannelReader<TIn>, ChannelWriter<TOut>, CancellationToken, Task> transform)
      : this(new Consumer<TIn>(), transform)
    {
    }

    public TransformActor(Func<TIn, CancellationToken, ValueTask<TOut>> transform)
      : this(async (reader, writer, cancellationToken) =>
        {
          await foreach (var msg in reader.ReadAllAsync(cancellationToken))
            await writer.WriteAsync(await transform(msg, cancellationToken), cancellationToken);
        })
    {
    }

    public TransformActor(Func<TIn, TOut> transform)
      : this((x, _) => new ValueTask<TOut>(transform(x)))
    {
    }

  }
}