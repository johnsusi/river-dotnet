using System.Collections.Generic;
using System.Threading.Channels;

namespace River.Streaming
{
    public static partial class Operators
  {
    public static async IAsyncEnumerable<Producer<IList<T>>> BufferMany<T>(this IAsyncEnumerable<Producer<T>> producers, int? capacity = null, ChannelOptions? options = null)
    {
      await foreach (var producer in producers)
        yield return producer.Buffer(capacity, options);
    }
  }
}