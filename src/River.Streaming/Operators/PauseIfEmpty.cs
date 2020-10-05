using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using River.Streaming.Actors;

namespace River.Streaming
{
  public static partial class Operators
  {
    public static Producer<T> PauseIfEmpty<T>(this Producer<T> producer, TimeSpan pause, ChannelOptions? options = null)
    {
      var actor = new TransformActor<T, T>( async (reader, writer, cancellationToken) =>
      {
        while (!cancellationToken.IsCancellationRequested)
        {
          while (reader.TryRead(out var item))
            await writer.WriteAsync(item, cancellationToken);
          await Task.Delay(pause);
          if (!await reader.WaitToReadAsync(cancellationToken)) break;
        }
      });
      producer.LinkTo(actor.Inbox, options);
      return actor.Outbox;
    }
  }
}