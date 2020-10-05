using System;
using System.Threading.Tasks;
using River.Streaming.Actors;
using River.Streaming.Helpers;
using River.Streaming.Test.Helpers;
using Xunit;

namespace River.Streaming.Test.Actors
{

  public class WindowActorTest : UnitTest
  {
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public async Task WindowActor_WithNonPositiveSize_ShouldThrow(int size)
    {
      await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => new WindowActor<int>(size));
    }

    [Fact]
    public async Task WindowActor_SingleObject_ShouldPropagateAndComplete()
    {
      var expected = new object();
      var actor = new WindowActor<object>(1);
      var producer = new Producer<object>();
      var consumer = new Consumer<WindowProducer<object>>();

      producer.LinkTo(actor.Inbox);
      actor.Outbox.LinkTo(consumer);
      actor.Start();
      await producer.WriteAsync(expected);
      var windowProducer = await consumer.ReadAsync();
      var windowConsumer = new Consumer<object>();
      windowProducer.LinkTo(windowConsumer);
      var actual =  await windowConsumer.ReadAsync();
      producer.Dispose();
      await actor;

      Assert.Equal(expected, actual);
    }
  }
}
