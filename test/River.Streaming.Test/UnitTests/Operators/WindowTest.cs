using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using River.Streaming.Test.Helpers;
using Xunit;

namespace River.Streaming.Test
{
  public class WindowTest
  {

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public async Task Window_WithNegativeSize_ShouldThrow(int size)
    {
      await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        await Enumerable
          .Range(1, 10)
          .AsProducer()
          .Outbox
          .Window(size)
          .ToListAsync());
    }

    [Theory]
    [InlineData(10, 5)]
    [InlineData(10, 9)]
    [InlineData(10, 10)]
    [InlineData(10, 1)]
    [InlineData(10, int.MaxValue)]
    public async Task Window_Should_Close_After_N_Messages(int count, int windowSize)
    {
      var numbers = Enumerable.Range(1, count);
      var expected =
        numbers
          .Select((value, index) => (value, index))
          .GroupBy(x => x.index / windowSize, x => x.value)
          .Select(x => x.ToList());

      var actual = await numbers
        .AsProducer()
        .Outbox
        .Window(size: windowSize)
        .BufferMany()
        .Concat()
        .ToListAsync();

      Assert.Equal(expected, actual);

    }

    [Fact]
    public async Task WindowTest2()
    {
      var numbers = Enumerable.Range(0, 1000);
      var expected = numbers
        .GroupBy( i => i / 10)
        .Select(group => group.ToList());
      var producer = numbers.AsProducer();
      var consumer = new TestConsumer<IList<int>>();
      var actual = consumer.Values;

      producer.Outbox
        .Window(10, options: new BoundedChannelOptions(1))
        .Select(window => window.Buffer())
        .Concat()
        .LinkTo(consumer.Inbox);

      await Task.WhenAll(producer, consumer);

      Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Window_ShouldContainIndex()
    {
      var numbers = Enumerable.Range(0, 1000);
      var expected = numbers
        .GroupBy( i => i / 10)
        .Select(group => group.Key + 1);
      var producer = numbers.AsProducer();
      var consumer = new Consumer<int>();

      var actual = await producer.Outbox
        .Window(10)
        .Select(window =>
        {
          window.LinkTo(new Consumer<int>());
          return window.Index;
        })
        .ToListAsync();

      Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Window_ShouldContainMaxSize()
    {
      var numbers = Enumerable.Range(0, 1000);
      var expected = numbers
        .GroupBy( i => i / 10)
        .Select(group => 10);
      var producer = numbers.AsProducer();
      var consumer = new Consumer<int>();

      var actual = await producer.Outbox
        .Window(10)
        .Select(window =>
        {
          window.LinkTo(new Consumer<int>());
          return window.MaxSize;
        })
        .ToListAsync();

      Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Window_ShouldContainDuration()
    {
      var numbers = Enumerable.Range(0, 1000);
      var expected = numbers
        .GroupBy( i => i / 10)
        .Select(group => 10);
      var producer = numbers.AsProducer();
      var consumer = new Consumer<int>();

      var actual = await producer.Outbox
        .Window(10)
        .Select(window =>
        {
          window.LinkTo(new Consumer<int>());
          return window.Duration;
        })
        .ToListAsync();

      foreach (var duration in actual)
        Assert.True(duration > TimeSpan.Zero);

    }


  }
}
