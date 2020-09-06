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

    public static async IAsyncEnumerable<IWindowProducer<T>> Window<T>(this IProducer<T> producer, int size = 0, TimeSpan? duration = null, ChannelOptions? options = null, [EnumeratorCancellation]CancellationToken cancellationToken = default)
    {
      var consumer = new Consumer<T>();
      producer.LinkTo(consumer, options);
      using var reader = await consumer.GetReaderAsync();
      int count = 0;
      int index = 0;
      WindowProducer<T>? windowProducer = null;
      DisposableChannelWriter<T>? writer = null;
      var intervalSource = Channel.CreateBounded<DateTime>(1);
      var intervals = intervalSource.Reader.ReadAllAsync(cancellationToken);
      TimerCallback callback = state => {};
      // var timer = new Timer(new TimerCallback(callback), null, TimeSpan.Zero, duration);

      duration ??= TimeSpan.MaxValue;

      try
      {
        await foreach (var item in reader.ReadAllAsync(cancellationToken))
        {
          if (count == 0)
          {
            var old = writer;
            windowProducer = new WindowProducer<T>(index++, duration.Value, size);
            yield return windowProducer;
            writer = await windowProducer.GetWriterAsync(cancellationToken);
            old?.Dispose();
          }
          if (writer != null)
            await writer.WriteAsync(item, cancellationToken);
          if (++count >= size)
          {
            count = 0;
          }
        }
      }
      finally
      {
        writer?.Dispose();
      }

    }
  }
}