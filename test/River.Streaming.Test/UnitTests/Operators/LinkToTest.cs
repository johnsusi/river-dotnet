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

  public class LinkToTest
  {

    public static IEnumerable<object[]> Data =>
      new List<object[]>
      {
        new object[] { Enumerable.Empty<int>(), new UnboundedChannelOptions() },
        new object[] { Enumerable.Empty<int>(), new BoundedChannelOptions(1) },
        new object[] { Enumerable.Range(1, 1000), new UnboundedChannelOptions() },
        new object[] { Enumerable.Range(1, 1000), new BoundedChannelOptions(1) },
      };

    [Theory]
    [MemberData(nameof(Data))]
    public async Task Unicast(IEnumerable<int> expected, ChannelOptions options)
    {
      var producer = expected.AsProducer();
      var consumer = new TestConsumer<int>();

      producer
        .Outbox
        .LinkTo(consumer.Inbox, options);

      await Task.WhenAll(
        producer,
        consumer
      );

      var actual = consumer.Values;
      Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Anycast_Should_Deliver_To_Random_Consumer()
    {
      var expected = Enumerable.Range(1, 10);
      var producer = expected.AsProducer();
      var barrier = new AsyncBarrier(expected.Count());
      var consumers =
        expected
          .Select(_ => new TestConsumer<int>(barrier))
          .ToArray();

      foreach (var consumer in consumers)
        producer
          .Outbox
          .LinkTo(consumer.Inbox);


      var tasks = consumers
          .Select(consumer => consumer.Completion)
          .Append(producer.Completion);


      await Task.WhenAll(tasks);

      foreach (var consumer in consumers)
        Assert.Single(consumer.Values);

      var actual =
        consumers
          .SelectMany(consumer => consumer.Values)
          .OrderBy(x => x);

      Assert.Equal(expected, actual);
    }

  }
}
