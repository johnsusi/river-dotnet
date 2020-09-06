using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using River.Streaming.Actors;
using River.Streaming.Helpers;

namespace River.Streaming
{
  public static partial class Operators
  {
    public static IProducer<TOut> Transform<TIn, TOut>(this IProducer<TIn> producer, Func<TIn, TOut> transform)
    {
      var actor = new TransformActor<TIn, TOut>(x => new ValueTask<TOut>(transform(x)));
      producer.LinkTo(actor.Inbox);
      actor.Start();
      return actor.Outbox;
    }

    public static IProducer<TOut> Transform<TIn, TOut>(this IProducer<TIn> producer, Func<ChannelReader<TIn>, ChannelWriter<TOut>, CancellationToken, Task> transform)
    {
      var actor = new TransformActor<TIn, TOut>(transform);
      producer.LinkTo(actor.Inbox);
      actor.Start();
      return actor.Outbox;
    }

  }
}