using System;
using System.Threading;
using System.Threading.Tasks;
using River.Streaming.Actors;
using River.Streaming.Test.Helpers;
using Xunit;

namespace River.Streaming.Test.Actors
{

  public class ActionActorTest : UnitTest
  {
    internal class ExpectedException : Exception {}

    [Fact]
    public async Task ActionActor_Should_Stop_When_Cancelled()
    {
      var actor = new ActionActor(async cancellationToken => { await Task.Delay(Timeout.Infinite, cancellationToken); });
      actor.Start();
      try
      {
        await actor.CancelAsync();
      }
      catch(OperationCanceledException) {}
      Assert.True(actor.Completion.IsCompleted);
    }

    [Fact]
    public async Task ActionActor_Should_Propagate_Exceptions()
    {
      var actor = new ActionActor(cancellationToken => throw new ExpectedException());
      await Assert.ThrowsAsync<ExpectedException>(async () => await actor);
    }

    // [Fact]
    // public async Task ActionActor_Should_Propagate_Exception_When_Cancelled()
    // {
    //   var actor = new ActionActor(async cancellationToken =>
    //   {
    //     try { await Task.Delay(Timeout.Infinite, cancellationToken); }
    //     catch (TaskCanceledException) {}
    //     throw new ExpectedException();

    //   });
    //   actor.Start();
    //   await Assert.ThrowsAsync<ExpectedException>(async () => await actor.CancelAsync());
    // }

  }
}
