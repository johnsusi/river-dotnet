using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using River.Streaming.Actors;

namespace River.Streaming
{
  public static partial class Operators
  {
    public static IProducer<T> PauseIfEmpty<T>(this IProducer<T> producer, TimeSpan pause, ChannelOptions? options = null)
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

      return actor.Outbox;
    }
  }
}