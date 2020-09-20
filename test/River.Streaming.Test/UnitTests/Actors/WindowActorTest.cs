using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using River.Streaming.Actors;
using River.Streaming.Helpers;
using River.Streaming.Test.Helpers;
using Xunit;

namespace River.Streaming.Test.Actors
{



  public class WindowActorTest : UnitTest
  {
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]  
    public async Task WindowActor_WithNonPositiveSize_ShouldThrow(int size)
    {
      await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => new WindowActor<int>(size));
    }

  }
}