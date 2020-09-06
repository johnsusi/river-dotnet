using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using River.Streaming.Test.Helpers;
using Xunit;

namespace River.Streaming.Test
{

  public class BufferTest
  {

    [Fact]
    public async Task Buffer_Should_Create_List_From_Stream()
    {
      var expected = Enumerable.Range(1, 1000);
      var producer = expected.AsProducer();
      var consumer = new TestConsumer<IList<int>>();

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

    [Fact]
    public async Task Buffer_Should_Create_Lists_From_Streams()
    {
      var expected = Enumerable.Range(1, 1000);
      var producer = expected.AsProducer();
      var consumer = new TestConsumer<IList<int>>();


      producer
        .Outbox
        .Window(10)
        .Buffer()
        .LinkTo(consumer.Inbox);

      await Task.WhenAll(producer, consumer);

      var actual =
        consumer.Values
          .SelectMany(x => x)
          .OrderBy(x => x)
          .ToList();

      Assert.Equal(expected, actual);
    }


  }
}
