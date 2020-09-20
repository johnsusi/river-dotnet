using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using River.Streaming.Actors;
using River.Streaming.Helpers;

namespace River.Streaming.Test.Helpers
{
  internal class TestConsumer<T> : ActionActor
  {
    public Consumer<T> Inbox { get; } = new Consumer<T>();
    public List<T> Values { get; set; } = new List<T>();

    public TestConsumer(AsyncBarrier barrier = null)
    {
      Action = async cancellationToken =>
      {
        using var reader = await Inbox.GetReaderAsync(cancellationToken);
        await foreach (var item in reader.ReadAllAsync(cancellationToken))
        {
          Values.Add(item);
          if (barrier != null) await barrier.SignalAndWait();
        }
      };
      Start();
    }
  }
}