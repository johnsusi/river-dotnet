using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using River.Streaming.Helpers;
using River.Streaming.Test.Helpers;
using Xunit;

namespace River.Streaming.Test
{

  public class TransformTest
  {

    [Fact]
    public async Task Identity_Transform_Should_Leave_Data_Unchanged()
    {
      var expected = Enumerable.Range(1, 1000);
      var producer = expected.AsProducer();
      var consumer = new TestConsumer<int>();

      producer
        .Outbox
        .Transform(x => x)
        .LinkTo(consumer.Inbox);

      await Task.WhenAll(
        producer,
        consumer
      );

      var actual = consumer.Values;
      Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Transform_Should_Be_Able_To_Change_Type()
    {
      var expected = Enumerable.Range(1, 1000);
      var producer = expected.AsProducer();
      var consumer = new TestConsumer<string>();

      producer
        .Outbox
        .Transform(x => x.ToString())
        .LinkTo(consumer.Inbox);

      await Task.WhenAll(
        producer,
        consumer
      );

      var actual = consumer.Values.Select(int.Parse);
      Assert.Equal(expected, actual);
    }

  }
}
