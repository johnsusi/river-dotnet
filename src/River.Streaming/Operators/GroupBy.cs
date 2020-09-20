using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using River.Streaming.Actors;
using River.Streaming.Helpers;

namespace River.Streaming
{
  public static partial class Operators
  {
    public static async IAsyncEnumerable<IGroupProducer<TKey, T>> GroupBy<TKey, T>(this IProducer<T> producer, Func<T, TKey> selector, ChannelOptions? options = null, [EnumeratorCancellation]CancellationToken cancellationToken = default)
    {
      var actor = new GroupByActor<TKey, T>(selector);
      producer.LinkTo(actor.Inbox, options);
      var consumer = new Consumer<IGroupProducer<TKey, T>>();
      actor.Outbox.LinkTo(consumer);
      actor.Start();
      using var reader = await consumer.GetReaderAsync(cancellationToken);
      await foreach (var group in reader.ReadAllAsync(cancellationToken))
        yield return group;
    }


    public static async IAsyncEnumerable<IProducer<TOut>> GroupBy<TKey, TIn, TOut>(this IProducer<TIn> producer, Func<TIn, TKey> selector, Func<IGroupProducer<TKey, TIn>, IProducer<TOut>> builder)
    {
      await foreach (var group in GroupBy(producer, selector))
        yield return builder(group);
    }

    public static async Task GroupBy<TKey, TIn>(this IProducer<TIn> producer, Func<TIn, TKey> selector, Action<IGroupProducer<TKey, TIn>> builder, ChannelOptions? options = null, CancellationToken cancellationToken = default)
    {
      await foreach (var group in GroupBy(producer, selector))
        builder(group);
    }
  }
}

