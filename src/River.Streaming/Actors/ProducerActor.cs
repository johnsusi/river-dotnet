using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using River.Streaming.Helpers;

namespace River.Streaming.Actors
{

  public class ProducerActor<T> : ActionActor
  {
    public IProducer<T> Outbox { get; } = new Producer<T>();
    public ProducerActor(Func<ChannelWriter<T>, CancellationToken, Task> action)
    {
      Action = async cancellationToken =>
      {
        using var writer = await Outbox.GetWriterAsync(cancellationToken);
        await action(writer, cancellationToken);
      };
    }
  }

  public static class ProducerActor
  {
    public static ProducerActor<T> Create<T>(Func<ChannelWriter<T>, CancellationToken, Task> action)
      => new ProducerActor<T>(action);

    // public static ProducerActor<T> Create<T>(Func<CancellationToken, ValueTask<(bool, T)>> action)
    //   => new ProducerActor<T>(action);
  }
}