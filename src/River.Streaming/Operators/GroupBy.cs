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
      var consumer = new Consumer<T>();
      producer.LinkTo(consumer, options);

      using var reader = await consumer.GetReaderAsync(cancellationToken);
      int index = 0;
      var groups = new Dictionary<TKey, DisposableChannelWriter<T>>();

      try
      {
        await foreach (var msg in reader.ReadAllAsync(cancellationToken))
        {

          var key = selector(msg);
          if (!groups.TryGetValue(key, out var writer))
          {
            var groupProducer = new GroupProducer<TKey, T>(index, key);
            yield return groupProducer;
            writer = await groupProducer.GetWriterAsync(cancellationToken);
            groups.Add(key, writer);
          }

          await writer.WriteAsync(msg, cancellationToken);

        }
      }
      finally
      {
        foreach (var writer in groups.Values)
        {
          writer.Dispose();
        }
      }

      // var actor = new GroupActor<TKey, T>(selector);
      // producer.LinkTo(actor.Inbox);
      // var _ = actor.ExecuteAsync();
      // return actor.Outbox;
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

