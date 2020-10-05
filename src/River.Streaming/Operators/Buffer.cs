using System.Collections.Generic;
using System.Threading.Channels;

namespace River.Streaming
{
    public static partial class Operators
  {
    public static Producer<IList<T>> Buffer<T>(this Producer<T> producer, int? capacity = null, ChannelOptions? options = null)
    {
      return producer.Transform<T, IList<T>>(async (reader, writer, cancellationToken) =>
      {
        var result = capacity.HasValue ? new List<T>(capacity.Value) : new List<T>();
        await foreach (var item in reader.ReadAllAsync(cancellationToken))
          result.Add(item);
        await writer.WriteAsync(result, cancellationToken);
      }, options);
    }
  }
}