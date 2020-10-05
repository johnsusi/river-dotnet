using System;
using River.Streaming.Actors;

namespace River.Streaming
{
    public static partial class Operators
  {

    public static Producer<TOut> Zip<TIn, TOther, TOut>(this Producer<TIn> producer, Producer<TOther> other, Func<TIn, TOther, TOut> action)
    {
      var actor = new TransformActor<TIn, TOut>(async (readerIn, writer, ct) =>
      {
        using var consumer = new Consumer<TOther>();
        other.LinkTo(consumer);

        await writer.WriteAsync(
          action(
            await readerIn.ReadAsync(ct),
            await consumer.ReadAsync(ct)
          ),
          ct
        );

      });
      return actor.Outbox;
    }
  }
}