using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace River.Streaming
{
    public static partial class Operators
  {
    public static async Task<IList<T>> ToListAsync<T>(this Producer<T> producer, CancellationToken cancellationToken = default, ChannelOptions? options = null)
    {
      using var consumer = new Consumer<T>();
      producer.LinkTo(consumer, options);
      var result = new List<T>();
      await foreach (var item in consumer.ReadAllAsync(cancellationToken))
        result.Add(item);
      return result;
    }
  }
}