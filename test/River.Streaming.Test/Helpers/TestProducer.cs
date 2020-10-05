using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using River.Streaming.Actors;
using River.Streaming.Helpers;

namespace River.Streaming.Test.Helpers
{
  internal static class TestProducer
  {
    internal static TestProducer<T> Create<T>(IEnumerable<T> data, AsyncBarrier barrier = null) => new TestProducer<T>(data, barrier);

    internal static TestProducer<T> AsProducer<T>(this IEnumerable<T> data, AsyncBarrier barrier = null) => new TestProducer<T>(data, barrier);

    internal static async IAsyncEnumerable<Producer<T>> CreateMany<T>(IEnumerable<T> data, int count, AsyncBarrier barrier = null)
    {
      for (int i = 0; i < count; ++i)
      {
        yield return data.AsProducer(barrier).Outbox;
        await Task.Delay(0);
      }
    }

  }

  internal class TestProducer<T> : ActionActor
  {
    public Producer<T> Outbox { get; } = new Producer<T>();
    internal TestProducer(IEnumerable<T> items, AsyncBarrier barrier = null)
    {
      Action = async cancellationToken =>
      {
        using (Outbox)
        {
          await Outbox.WaitToWriteAsync(cancellationToken);
          foreach (var item in items)
          {
            cancellationToken.ThrowIfCancellationRequested();
            await Outbox.WriteAsync(item, cancellationToken);
            if (barrier != null) await barrier.SignalAndWait();
          }
        }
      };
      Start();
    }
  }
}