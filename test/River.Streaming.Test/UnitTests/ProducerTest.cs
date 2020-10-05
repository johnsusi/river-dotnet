using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace River.Streaming.Test.UnitTests
{
  public class ProducerTest  
  {
    [Fact]
    public void Dispose_WithUnlinkedChannel_ShouldComplete()
    {
      var producer = new Producer<object>();
      var actual = Record.Exception(producer.Dispose);
      Assert.Null(actual);
    }

    [Fact]
    public async Task Dispose_WithLinkedChannel_ShouldBeComplete()
    {
      var producer = new Producer<object>();
      var consumer = new Consumer<object>();
      producer.LinkTo(consumer);
      producer.Dispose();
      var timeout = Task.Delay(TimeSpan.FromSeconds(15));
      var task = await Task.WhenAny(consumer.Completion, timeout);
      Assert.NotEqual(timeout, task);
      Assert.True(consumer.Completion.IsCompletedSuccessfully, "Consumer should be completed when producer is disposed");
    }

    [Fact]
    public async Task Dispose_WithMultiLinkedChannel_ShouldNotComplete()
    {
      var producer = new Producer<object>();
      var other = new Producer<object>();
      var consumer = new Consumer<object>();
      producer.LinkTo(consumer);
      other.LinkTo(consumer);
      producer.Dispose();
      Assert.False(consumer.Completion.IsCompleted, "Consumer should not be completed when producer is disposed if there are more producers alive");
      other.Dispose();
      var timeout = Task.Delay(TimeSpan.FromSeconds(15));
      var task = await Task.WhenAny(consumer.Completion, timeout);
      Assert.NotEqual(timeout, task);
      Assert.True(consumer.Completion.IsCompletedSuccessfully, "Consumer should be completed when producer is disposed");
    }

    [Fact]
    public void TryWrite_WithoutLink_ShouldFail()
    {
      var producer = new Producer<object>();
      var actual = producer.TryWrite(default);
      Assert.False(actual);
    }

    [Fact]
    public void WaitToWriteAsync_WithoutLink_ShouldWait()
    {
      var producer = new Producer<object>();
      var actual = producer.WaitToWriteAsync(default);      
      Assert.False(actual.IsCompleted);
    }

    [Fact]
    public async Task WaitToWriteAsync_WithoutLink_ShouldSucceedWhenLinked()
    {
      var producer = new Producer<object>();
      var task = producer.WaitToWriteAsync(default);      
      var consumer = new Consumer<object>();
      producer.LinkTo(consumer);
      var actual = await task;
      Assert.True(actual);
    }

    [Fact]
    public async Task WaitToWriteAsync_WithLink_ShouldSucceed()
    {
      var producer = new Producer<object>();
      var consumer = new Consumer<object>();
      producer.LinkTo(consumer);
      var actual = await producer.WaitToWriteAsync(default);
      Assert.True(actual);
    }

    [Fact]
    public async Task WaitToWriteAsync_WithoutLink_ShouldThrowIfCancelled()
    {
      var producer = new Producer<object>();
      var cancel = new CancellationTokenSource();
      cancel.Cancel();
      await Assert.ThrowsAsync<OperationCanceledException>(async () => await producer.WaitToWriteAsync(cancel.Token));
    }

  }
}