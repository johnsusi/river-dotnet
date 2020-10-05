using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using River.Streaming.Test.Helpers;
using Xunit;

namespace River.Streaming.Test
{

  public class ConcatTest : UnitTest
  {

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(100)]
    public async Task Concat_Should_Append_Multiple_Producers_In_Order(int count)
    {
      var numbers = Enumerable.Range(1, 1000);
      var producers = TestProducer.CreateMany(numbers, count);

      var expected =
        Enumerable
          .Range(1, count)
          .Select(_ => numbers)
          .SelectMany(x => x);

      var actual =
        await producers
          .Concat()
          .ToListAsync();

      Assert.Equal(expected, actual);
    }

   [Fact]
    public async Task Concat_WithConcurrentProducers_ShouldLinkWhenProducerArrives()
    {
      var expected = new [] { 1, 2, 3};
      var consumer = new Consumer<int>();
      var producer1 = new Producer<int>();
      var producer2 = new Producer<int>();
      var barrier = new AsyncBarrier(2);
      var producers = new []
      {
        producer1,
        producer2,
      }.ToAsyncEnumerable();

      producers.Concat().LinkTo(consumer);

      var actual = new List<int>();

      await producer1.WriteAsync(1);
      await producer2.WriteAsync(3);

      actual.Add(await consumer.ReadAsync());
      Assert.False(consumer.TryRead(out _));

      await producer1.WriteAsync(2);

      actual.Add(await consumer.ReadAsync());
      Assert.False(consumer.TryRead(out _));

      producer1.Dispose();
      actual.Add(await consumer.ReadAsync());

      producer2.Dispose();
      consumer.Dispose();
      Assert.Equal(expected, actual);
    }
  }
}
