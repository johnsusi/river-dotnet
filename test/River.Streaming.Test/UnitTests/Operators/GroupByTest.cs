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

  public class GroupByTest
  {

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

    // [Fact]
    // public async Task GroupBy_Should_()
    // {
    //   static bool isEven(int x) => x % 2 == 0;
    //   static bool isOdd(int x) => x % 2 != 0;

    //   var expected = Enumerable.Range(1, 10);
    //   var even = expected.Where(isEven);
    //   var odd = expected.Where(isOdd);
    //   var producer = expected.AsProducer();
    //   var evenConsumer = new TestConsumer<int>();
    //   var oddConsumer = new TestConsumer<int>();

    //   producer
    //     .Outbox
    //     .GroupBy(isEven)
    //     .Merge()
    //     .Consume(g =>
    //     {
    //       var consumer = g.Key ? evenConsumer : oddConsumer;
    //       g.LinkTo(consumer.Inbox);
    //     });


    //   await Task.WhenAll(
    //     producer,
    //     evenConsumer,
    //     oddConsumer
    //   );

    //   Assert.Equal(even, evenConsumer.Values);
    //   Assert.Equal(odd, oddConsumer.Values);;
    // }
  }
}
