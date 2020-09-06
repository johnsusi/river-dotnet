using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using River.Streaming.Helpers;

namespace River.Streaming.Actors
{
  public class ConsumerActor<T> : ActionActor
  {
    public IConsumer<T> Inbox { get; } = new Consumer<T>();
    public ConsumerActor(Func< ChannelReader<T>, CancellationToken, Task> action)
    {
      Action = async cancellationToken =>
      {
        using var reader = await Inbox.GetReaderAsync(cancellationToken);
        await action(reader, cancellationToken);
      };
    }
  }
}