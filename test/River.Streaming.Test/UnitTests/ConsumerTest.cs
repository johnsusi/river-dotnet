using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace River.Streaming.Test.UnitTests
{
  public class ConsumerTest  
  {

    [Fact]
    public void Dispose_WithUnlinkedChannel_ShouldComplete()
    {
      var consumer = new Consumer<object>();
      var actual = Record.Exception(consumer.Dispose);
      Assert.Null(actual);
    }

    [Fact]
    public async Task WaitToReadAsync_WithoutLink_ShouldThrowIfCancelled()
    {
      var consumer = new Consumer<object>();
      var cancel = new CancellationTokenSource();
      cancel.Cancel();
      await Assert.ThrowsAsync<OperationCanceledException>(async () => await consumer.WaitToReadAsync(cancel.Token));
    }

  }
}