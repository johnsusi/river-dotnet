using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using River.Streaming.Test.Helpers;
using Xunit;

namespace River.Streaming.Test
{

  public class BufferTest : UnitTest
  {

    [Fact]
    public async Task Buffer_Should_Create_List_From_Stream()
    {
      var expected = Enumerable.Range(1, 1000);
      using var producer = expected.AsProducer();
      using var consumer = new TestConsumer<IList<int>>();

      producer
        .Outbox
        .Buffer()
        .LinkTo(consumer.Inbox);

      await Task.WhenAll(
        producer,
        consumer
      );

      var actual = consumer.Values.FirstOrDefault();
      Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(1000, 1)]
    [InlineData(1000, 9)]
    [InlineData(1000, 10)]
    [InlineData(1000, 1000)]
    public async Task BufferMany_Should_Create_Lists_From_Streams(int count, int windowSize)
    {
      var messages = Enumerable.Range(1, count);
      var expected =
        messages
          .Select( (value, index) => (value, index))
          .GroupBy( x => x.index / windowSize, x => x.value)
          .Select(x => x.ToList());

      using var producer = messages.AsProducer();

      var actual =
        await producer
          .Outbox
          .Window(windowSize)
          .BufferMany()
          .Concat()
          .ToListAsync();

      Assert.Equal(expected, actual);
    }
  }
}
