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
    public static async IAsyncEnumerable<GroupProducer<TKey, T>> GroupBy<TKey, T>(this Producer<T> producer, Func<T, TKey> selector, ChannelOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
      var actor = new GroupByActor<TKey, T>(selector);
      producer.LinkTo(actor.Inbox, options);
      var consumer = new Consumer<GroupProducer<TKey, T>>();
      actor.Outbox.LinkTo(consumer);
      actor.Start();
      await foreach (var group in consumer.ReadAllAsync(cancellationToken))
        yield return group;
    }


    public static async IAsyncEnumerable<Producer<TOut>> GroupBy<TKey, TIn, TOut>(this Producer<TIn> producer, Func<TIn, TKey> selector, Func<GroupProducer<TKey, TIn>, Producer<TOut>> builder, ChannelOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
      await foreach (var group in GroupBy(producer, selector, options, cancellationToken))
        yield return builder(group);
    }

    public static async Task GroupBy<TKey, TIn>(this Producer<TIn> producer, Func<TIn, TKey> selector, Action<GroupProducer<TKey, TIn>> builder, ChannelOptions? options = null, CancellationToken cancellationToken = default)
    {
      await foreach (var group in GroupBy(producer, selector, options, cancellationToken))
        builder(group);
    }
  }
}

