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


  internal class NotImplementedAbstractActor : AbstractActor
  {
    protected override Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
      throw new NotImplementedException();
    }
  }

  internal class InfiniteAbstractActor : AbstractActor
  {
    protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
      await Task.Delay(Timeout.Infinite, cancellationToken);
    }
  }

  public class AbstractActorTest : UnitTest
  {
    [Fact]
    public void AbstractActor_CancelAsync_Should_Complete_If_Not_Started()
    {
      var actor = new NotImplementedAbstractActor();
      var task = actor.CancelAsync();
      Assert.True(task.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task AbstractActor_CancelAsync_Should_Complete_With_Cancel_Exception()
    {
      var actor = new InfiniteAbstractActor();
      actor.Start();
      var cancel = new CancellationTokenSource();
      cancel.Cancel();
      await Assert.ThrowsAsync<TaskCanceledException>(async () => await actor.CancelAsync(cancel.Token));
    }


  }
}
