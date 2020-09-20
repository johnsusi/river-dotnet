using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using River.Streaming.Actors;

namespace River.Streaming
{
  public static partial class Operators
  {
    public static IProducer<T> Concat<T>(this IAsyncEnumerable<IProducer<T>> producers, ChannelOptions? options = null)
    {
      var actor = new ConcatActor<T>(producers);
      actor.Start();
      return actor.Outbox;
    }
  }
}