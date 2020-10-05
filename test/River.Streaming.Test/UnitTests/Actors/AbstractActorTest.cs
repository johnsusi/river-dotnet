using System;
using System.Threading;
using System.Threading.Tasks;
using River.Streaming.Actors;
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


  internal class ThrowOnCancelAbstractActor : AbstractActor
  {
    internal class InternalException : Exception {}
    protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
      try
      {
        await Task.Delay(Timeout.Infinite, cancellationToken);
      }
      catch (TaskCanceledException)
      {
        throw new InternalException();
      }
    }
  }

  public class AbstractActorTest : UnitTest
  {
    [Fact]
    public void CancelAsync_ShouldComplete()
    {
      using var actor = new NotImplementedAbstractActor();
      var task = actor.CancelAsync();
      Assert.True(task.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task CancelAsync_ShouldThrowIfCancelled()
    {
      using var cancel = new CancellationTokenSource();
      cancel.Cancel();
      using var actor = new InfiniteAbstractActor();
      actor.Start();
      await Assert.ThrowsAsync<OperationCanceledException>(async () => await actor.CancelAsync(cancel.Token));
    }

  }
}
