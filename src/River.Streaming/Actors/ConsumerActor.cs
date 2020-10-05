using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace River.Streaming.Actors
{
  public class ConsumerActor<T> : ActionActor
  {
    public Consumer<T> Inbox { get; } = new Consumer<T>();
    public ConsumerActor(Func< ChannelReader<T>, CancellationToken, Task> action)
    {
      Action = async cancellationToken =>
      {
        await action(Inbox, cancellationToken);
      };
    }
  }
}