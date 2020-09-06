using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using River.Streaming.Actors;
using River.Streaming.Helpers;

namespace River.Streaming
{
  public static partial class Operators
  {

    public static IProducer<TOut> Zip<TIn, TOther, TOut>(this IProducer<TIn> producer, IProducer<TOther> other, Func<TIn, TOther, TOut> action)
    {
      var consumer = new Consumer<TOther>();
      other.LinkTo(consumer);
      var actor = new TransformActor<TIn, TOut>(async (readerIn, writer, ct) =>
      {
        var readerOther = await consumer.GetReaderAsync(ct);

        await writer.WriteAsync(
          action(
            await readerIn.ReadAsync(ct),
            await readerOther.ReadAsync(ct)
          ),
          ct
        );

      });

      return actor.Outbox;

    }

 }
}