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

    public static IProducer<T> Merge<T>(this IAsyncEnumerable<IProducer<T>> producers, ChannelOptions? options = null)
    {
      var actor = new MergeActor<T>(producers, options);
      actor.Start();
      return actor.Outbox;
    }
  }
}