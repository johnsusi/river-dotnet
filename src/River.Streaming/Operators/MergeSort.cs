using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using River.Streaming.Actors;
using River.Streaming.Helpers;

namespace River.Streaming
{
  public static partial class Operators
  {

    public static IProducer<T> MergeSort<T>(this IProducer<T> producer, IProducer<T> other)
    {
      var actor = new TransformActor<T, T>(x => new ValueTask<T>(x));
      producer.LinkTo(actor.Inbox);
      other.LinkTo(actor.Inbox);
      return actor.Outbox;
    }

  }
}