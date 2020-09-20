using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using River.Streaming.Actors;

namespace River.Streaming
{

  public static partial class Operators
  {

    public static TConsumer Consume<T, TConsumer>(this IProducer<T> producer)
      where TConsumer : IConsumer<T>, new()
    {
      var consumer = new TConsumer();
      producer.LinkTo(consumer);
      return consumer;
    }

    public static void Consume<T>(this IProducer<T> producer, Action<T> consumer, ChannelOptions? options = null)
      => Consume(producer, t => { consumer(t); return new ValueTask(); }, options);

    public static void Consume<T>(this IProducer<T> producer, Func<T, ValueTask> consumer, ChannelOptions? options = null)
    {
      var actor = new ConsumerActor<T>(async (reader, ct) =>
      {
        await foreach (var item in reader.ReadAllAsync(ct))
          await consumer(item);

      });
      producer.LinkTo(actor.Inbox, options);
      actor.Start();
    }
  }
}