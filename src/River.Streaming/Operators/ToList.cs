using System;
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
    public static async Task<IList<T>> ToListAsync<T>(this IProducer<T> producer, CancellationToken cancellationToken = default, ChannelOptions? options = null)
    {
      var consumer = new Consumer<T>();
      producer.LinkTo(consumer, options);
      using var reader = await consumer.GetReaderAsync(cancellationToken);
      var result = new List<T>();
      await foreach (var item in reader.ReadAllAsync(cancellationToken))
        result.Add(item);
      return result;
    }
  }
}