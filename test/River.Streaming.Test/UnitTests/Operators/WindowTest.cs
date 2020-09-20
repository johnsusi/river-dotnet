using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using River.Streaming.Test.Helpers;
using Xunit;

namespace River.Streaming.Test
{
  public class WindowTest
  {
    [Theory]
    [InlineData(10, 5)]
    [InlineData(10, 9)]
    [InlineData(10, 10)]
    [InlineData(10, 1)]
    public async Task Window_Should_Close_After_N_Messages(int count, int windowSize)
    {
      var numbers = Enumerable.Range(1, count);
      var expected =
        numbers
          .Select((value, index) => (value, index))
          .GroupBy(x => x.index / windowSize, x => x.value)
          .Select(x => x.ToList());
      var producer = numbers.AsProducer();
      var consumers = new List<TestConsumer<int>>();

      var windows =
        producer
          .Outbox
          .Window(size: windowSize);
      producer.Start();

      await foreach (var window in windows)
      {
        var consumer = new TestConsumer<int>();
        window.LinkTo(consumer.Inbox);
        consumers.Add(consumer);
        consumer.Start();
      }

      var tasks =
        consumers
          .Select(consumer => consumer.Completion)
          .Append(producer);

      await Task.WhenAll(tasks);

      var actual = consumers.SelectMany(consumer => consumer.Values);


      // Assert.Equal(expected, actual);
      Assert.Equal(windowCount(count, windowSize), consumers.Count());


    }

    static int windowCount(int messages, int windowSize) => (messages + windowSize - 1) / windowSize;

    // [Fact]
    // public async Task Window_With_Negative_Size_Should_Throw()
    // {
    //   await Assert.ThrowsAsync<Exception>(async () =>

    //     await Enumerable
    //       .Range(1, 1000)
    //       .AsProducer()
    //       .Outbox
    //       .Window(size: -1)
    //       .Buffer()
    //       .Concat()
    //       .ToListAsync()
    //   );
    // }
  }
}
