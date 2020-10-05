using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;

namespace River.Streaming
{
    public static partial class Operators
  {
    public static async IAsyncEnumerable<T> ToIAsyncEnumerable<T>(this Producer<T> producer, [EnumeratorCancellation]CancellationToken cancellationToken = default, ChannelOptions? options = null)
    {
      using var consumer = new Consumer<T>();
      producer.LinkTo(consumer, options);
      await foreach (var item in consumer.ReadAllAsync(cancellationToken))
        yield return item;
    }
  }
}