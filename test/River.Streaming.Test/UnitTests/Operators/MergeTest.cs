using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using River.Streaming.Test.Helpers;
using Xunit;

namespace River.Streaming.Test
{

  public class MergeTest : UnitTest
  {


    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(100)]
    public async Task Merge_Should_Interleave_Multiple_Producers(int count)
    {
      var barrier = new AsyncBarrier(count);
      var numbers = Enumerable.Range(1, 1000);
      var producers = TestProducer.CreateMany(numbers, count, barrier);

      var expected =
        numbers
          .Select(x => Enumerable.Repeat(x, count))
          .SelectMany(x => x);


      var actual =
        await producers
          .Merge()
          .ToListAsync();

      Assert.Equal(expected, actual);
    }
  }
}
