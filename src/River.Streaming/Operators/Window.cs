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

    public static async IAsyncEnumerable<WindowProducer<T>> Window<T>(this Producer<T> producer, int size = int.MaxValue, TimeSpan? duration = null, ChannelOptions? options = null, [EnumeratorCancellation]CancellationToken cancellationToken = default)
    {
      using var actor = new WindowActor<T>(size, duration);
      producer.LinkTo(actor.Inbox, options);
      using var consumer = new Consumer<WindowProducer<T>>();
      actor.Outbox.LinkTo(consumer);
      actor.Start();
      await foreach (var window in consumer.ReadAllAsync(cancellationToken))
        yield return window;
      await actor;
    }

    public static async IAsyncEnumerable<WindowProducer<T>> Window<T>(this IAsyncEnumerable<Producer<T>> producers, int size = int.MaxValue, TimeSpan? duration = null, ChannelOptions? options = null, [EnumeratorCancellation]CancellationToken cancellationToken = default)
    {
      await foreach (var producer in producers)
        await foreach (var window in producer.Window(size, duration, options, cancellationToken))
          yield return window;
    }

  }
}