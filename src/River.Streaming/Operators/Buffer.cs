using System.Collections.Generic;
using System.Threading.Channels;

namespace River.Streaming
{
  public static partial class Operators
  {
    public static IProducer<IList<T>> Buffer<T>(this IProducer<T> producer, ChannelOptions? options = null)
    {
      return producer.Transform<T, IList<T>>(async (reader, writer, cancellationToken) =>
      {
        var result = new List<T>();
        await foreach (var item in reader.ReadAllAsync(cancellationToken))
          result.Add(item);
        await writer.WriteAsync(result, cancellationToken);
      });
    }

    public static async IAsyncEnumerable<IProducer<IList<T>>> Buffer<T>(this IAsyncEnumerable<IProducer<T>> producers, ChannelOptions? options = null)
    {
      await foreach (var producer in producers)
        yield return producer.Buffer(options);
    }
  }
}