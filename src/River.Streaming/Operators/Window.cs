using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using River.Streaming.Actors;
using River.Streaming.Helpers;

namespace River.Streaming
{
  public static partial class Operators
  {

    public static async IAsyncEnumerable<IWindowProducer<T>> Window<T>(this IProducer<T> producer, int size = int.MaxValue, TimeSpan? duration = null, ChannelOptions? options = null, [EnumeratorCancellation]CancellationToken cancellationToken = default)
    {
      var actor = new WindowActor<T>(size, duration);
      producer.LinkTo(actor.Inbox, options);
      var consumer = new Consumer<IWindowProducer<T>>();
      actor.Outbox.LinkTo(consumer);
      using var reader = await consumer.GetReaderAsync();
      actor.Start();
      await foreach (var window in reader.ReadAllAsync(cancellationToken))
        yield return window;
      await actor;
    }
  }
}