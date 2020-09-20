using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using River.Streaming.Actors;
using River.Streaming.Helpers;

namespace River.Streaming
{

  public class RegulatePressureActor<T, TFeedback> : AbstractActor
  {
    public IConsumer<T> Inbox { get; } = new Consumer<T>();
    public IProducer<T> Outbox { get; } = new Producer<T>();
    public IConsumer<TFeedback> Feedback { get; } = new Consumer<TFeedback>();

    protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
      using var reader = await Inbox.GetReaderAsync(cancellationToken);
      using var writer = await Outbox.GetWriterAsync(cancellationToken);
      using var feedback = await Feedback.GetReaderAsync(cancellationToken);

      // int pressure = 0;
      // while (!cancellationToken.IsCancellationRequested)
      // {


      // }

    }
  }

  public static partial class Operators
  {
    public static IProducer<T> RegulatePressure<T, TFeedback>(this IProducer<T> producer, IProducer<TFeedback> feedback, Func<int, TFeedback, int> pressure, ChannelOptions? options = null)
    {
      var actor = new RegulatePressureActor<T, TFeedback>();
      producer.LinkTo(actor.Inbox, options);
      actor.Start();
      return actor.Outbox;
    }

  }
}