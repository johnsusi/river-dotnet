using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using River.Streaming.Actors;

namespace River.Streaming
{

  public static partial class Operators
  {
    public static Task Consume<T>(this Producer<T> producer, Action<T> consumer, ChannelOptions? options = null)
      => Consume(producer, t => { consumer(t); return new ValueTask(); }, options);

    public static Task Consume<T>(this Producer<T> producer, Func<T, ValueTask> consumer, ChannelOptions? options = null)
    {
      var actor = new ConsumerActor<T>(async (reader, ct) =>
      {
        await foreach (var item in reader.ReadAllAsync(ct))
          await consumer(item);

      });
      producer.LinkTo(actor.Inbox, options);
      actor.Start();
      return actor.Completion;
    }
  }
}