using System;
using System.Threading.Channels;
using River.Streaming.Actors;

namespace River.Streaming
{

  public static partial class Operators
  {
    public static Producer<T> RegulatePressure<T, TFeedback>(this Producer<T> producer, int limit, Producer<TFeedback> feedback, Func<int, int, TFeedback, int>? regulate = null, ChannelOptions? options = null)
    {
      regulate ??= (pressure, _2, _3) => pressure - 1;
      var actor = new PressureControlActor<T, TFeedback>(limit, regulate);
      producer.LinkTo(actor.Inbox, options);
      feedback.LinkTo(actor.Feedback, options);
      actor.Start();
      return actor.Outbox;
    }
  }

}
