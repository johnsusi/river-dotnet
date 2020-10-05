using System;
using System.Threading.Tasks;
using River.Streaming.Actors;
using Xunit;

namespace River.Streaming.Test.UnitTests.Actors
{

  public class PressureControlActorTest
  {

    [Fact]
    public async Task RegulatePressure_WithLimitAndFeedback_ShouldThrowOnNegativePressure()
    {
      var actor = new PressureControlActor<int, int>(1, (_1, _2, _3) => -2);

      using var producer = new Producer<int>();
      using var feedback = new Producer<int>();
      using var consumer = new Consumer<int>();

      producer.LinkTo(actor.Inbox);
      feedback.LinkTo(actor.Feedback);
      actor.Outbox.LinkTo(consumer);

      await producer.WriteAsync(1);
      await producer.WriteAsync(2);
      await feedback.WriteAsync(1);

      actor.Start();
      
      await Assert.ThrowsAsync<Exception>(async () => await actor.Completion);
    }
  }
}