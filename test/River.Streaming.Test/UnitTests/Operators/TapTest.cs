using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using River.Streaming.Actors;
using River.Streaming.Helpers;
using River.Streaming.Test.Helpers;
using Xunit;

namespace River.Streaming.Test.Operators
{
  public class TapTest
  {


    [Fact]
    public async Task Tap_Should_Tap_Into_Stream()
    {

      var producer = Enumerable.Range(1, 10).AsProducer();
      var consumer = new TestConsumer<int>();
      var expected = producer.Outbox;
      object actual = null;

      producer
        .Outbox
        .Tap(x => actual = x)
        .LinkTo(consumer.Inbox);

      await Task.WhenAll(producer,consumer);

      Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Tap_Should_Not_Change_Stream()
    {

      var expected = Enumerable.Range(1, 10);
      var producer = expected.AsProducer();
      var consumer = new TestConsumer<int>();

      producer
        .Outbox
        .Tap(x => {})
        .LinkTo(consumer.Inbox);

      await Task.WhenAll(producer,consumer);

      var actual = consumer.Values;

      Assert.Equal(expected, actual);
    }

  }
}
