using System;
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

  public class GroupByTest : UnitTest
  {


    [Fact]
    public async Task GroupBy_Should_Partition_Stream()
    {
      var numbers = Enumerable.Range(1, 1000);
      var expected = numbers.GroupBy(x => x % 2).Select(x => (IEnumerable<int>)x);
      var producer = numbers.AsProducer();

      var actual =
        await
          producer
            .Outbox
            .GroupBy(x => x % 2)
            .Buffer()
            .Merge()
            .ToListAsync();

      Assert.Equal(expected, actual.OrderBy(x => x[0]));

    }

    [Fact]
    public async Task GroupBy_Merge_Will_Should_()
    {
      var timeout = new CancellationTokenSource();
      var expected = Enumerable.Range(1, 10);
      var producer = expected.AsProducer();
      var consumer = new TestConsumer<int>();

      producer
        .Outbox
        .GroupBy(x => x, cancellationToken: timeout.Token)
        .Merge()
        .LinkTo(consumer.Inbox);

      await Task.WhenAll(
        producer,
        consumer);

      var actual = consumer.Values.OrderBy(x => x);
      Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GroupBy_Merge_Will_Should_2()
    {
      var expected = Enumerable.Range(1, 10);
      var producer = expected.AsProducer();
      var consumer = new TestConsumer<int>();

      var cancel = new CancellationTokenSource();

      var tasks = new List<Task> {
        producer
          .Outbox
          .GroupBy<int, int>(
            x => x,
            p => p.LinkTo(consumer.Inbox),
            null,
            cancel.Token),
        producer,
        consumer
      };

      await tasks.WhenAll(cancel);

      var actual = consumer.Values.OrderBy(x => x);
      Assert.Equal(expected, actual);
    }
    
  }
}
