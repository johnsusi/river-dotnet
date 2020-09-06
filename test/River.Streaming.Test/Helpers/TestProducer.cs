using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using River.Streaming.Actors;
using River.Streaming.Helpers;

namespace River.Streaming.Test.Helpers
{
  internal static class TestProducer
  {
    internal static TestProducer<T> Create<T>(IEnumerable<T> data) => new TestProducer<T>(data);

    internal static TestProducer<T> AsProducer<T>(this IEnumerable<T> data) => new TestProducer<T>(data);

  }

  internal class TestProducer<T> : ActionActor
  {
    public IProducer<T> Outbox { get; } = new Producer<T>();
    internal TestProducer(IEnumerable<T> items)
    {
      Action = async cancellationToken =>
      {
        using var writer = await Outbox.GetWriterAsync(cancellationToken);
        foreach (var item in items)
        {
          cancellationToken.ThrowIfCancellationRequested();
          await writer.WriteAsync(item, cancellationToken);
        }
      };
    }
  }
}