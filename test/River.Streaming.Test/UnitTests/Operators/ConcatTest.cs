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
  }
}
