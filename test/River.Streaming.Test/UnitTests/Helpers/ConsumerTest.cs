using System;
using System.Threading;
using System.Threading.Tasks;
using River.Streaming.Helpers;
using River.Streaming.Test.Helpers;
using Xunit;

namespace River.Streaming.Test.UnitTests.Helpers
{
  public class ConsumerTest : UnitTest
  {

    [Fact]
    public async Task Consumer_GetReaderAsync_WhenConnected_ShouldReturn()
    {

      var producer = new Producer<object>();
      var consumer = new Consumer<object>();
      var task = consumer.GetReaderAsync();
      producer.LinkTo(consumer);
      await task;
      Assert.True(task.IsCompletedSuccessfully);

    }

    [Fact]
    public async Task Consumer_GetReaderAsync_ShouldCancel()
    {
      using var cancel = new CancellationTokenSource();
      cancel.Cancel();
      var consumer = new Consumer<object>();
      var task = consumer.GetReaderAsync(cancel.Token);
      if (!task.IsCompleted)
      {
        try { await task; }
        catch(OperationCanceledException) {}
      }
      Assert.True(task.IsCanceled);
    }


  }
}