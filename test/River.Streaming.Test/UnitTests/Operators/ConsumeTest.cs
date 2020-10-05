using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using River.Streaming.Test.Helpers;
using Xunit;

namespace River.Streaming.Test
{

  public class ConsumeTest : UnitTest
  {

    [Fact]
    public async Task Consume_ShouldComplete()
    {
      var expected = Enumerable.Range(1, 1000);
      using var producer = expected.AsProducer();
      var actual = new List<int>();
      await producer
        .Outbox
        .Consume(actual.Add);

      Assert.Equal(expected, actual);
    }
  }
}
