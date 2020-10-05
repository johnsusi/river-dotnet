using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using River.Streaming.Actors;

namespace River.Streaming
{
  public static partial class Operators
  {

    public static Producer<TOut> Transform<TIn, TOut>(this Producer<TIn> producer, Func<TIn, Task<TOut>> transform, ChannelOptions? options = null)
    {
      var actor = new TransformActor<TIn, TOut>((x, _) => new ValueTask<TOut>(transform(x)));
      producer.LinkTo(actor.Inbox, options);
      actor.Start();
      return actor.Outbox;
    }

    public static Producer<TOut> Transform<TIn, TOut>(this Producer<TIn> producer, Func<TIn, TOut> transform, ChannelOptions? options = null)
    {
      var actor = new TransformActor<TIn, TOut>((x, _) => new ValueTask<TOut>(transform(x)));
      producer.LinkTo(actor.Inbox, options);
      actor.Start();
      return actor.Outbox;
    }

    public static Producer<TOut> Transform<TIn, TOut>(this Producer<TIn> producer, Func<ChannelReader<TIn>, ChannelWriter<TOut>, CancellationToken, Task> transform, ChannelOptions? options = null)
    {
      var actor = new TransformActor<TIn, TOut>(transform);
      producer.LinkTo(actor.Inbox, options);
      actor.Start();
      return actor.Outbox;
    }

    public static Producer<TOut> Transform<TIn, TOut>(this Producer<TIn> producer, Func<TIn, CancellationToken, ValueTask<TOut>> transform, ChannelOptions? options = null)
    {
      var actor = new TransformActor<TIn, TOut>(transform);
      producer.LinkTo(actor.Inbox, options);
      actor.Start();
      return actor.Outbox;
    }



    public static Producer<TOut> Transform<TIn, TOut>(this Consumer<TIn> consumer, Func<ChannelReader<TIn>, ChannelWriter<TOut>, CancellationToken, Task> transform)
    {
      var actor = new TransformActor<TIn, TOut>(consumer, transform);
      actor.Start();
      return actor.Outbox;
    }

  }
}