using System.Collections.Generic;
using System.Threading.Channels;
using River.Streaming.Actors;

namespace River.Streaming
{
    public static partial class Operators
  {

    public static Producer<T> Merge<T>(this IAsyncEnumerable<Producer<T>> producers, ChannelOptions? options = null)
    {
      var actor = new MergeActor<T>(producers, options);
      actor.Start();
      return actor.Outbox;
    }
  }
}