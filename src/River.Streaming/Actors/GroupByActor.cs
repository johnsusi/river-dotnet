using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using River.Streaming.Helpers;

namespace River.Streaming.Actors
{
  public class GroupByActor<TKey, T> : AbstractActor
  {
    private readonly Func<T, TKey> _selector;

    public Consumer<T> Inbox { get; } = new Consumer<T>();
    public Producer<GroupProducer<TKey, T>> Outbox { get; } = new Producer<GroupProducer<TKey, T>>();

    public GroupByActor(Func<T, TKey> selector)
    {
      _selector = selector;

    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
      using (Outbox)
      using (Inbox)
      {

        var groups = new Dictionary<TKey, GroupProducer<TKey, T>>();
        await foreach (var item in Inbox.ReadAllAsync(cancellationToken))
        {
          var key = _selector(item);
          if (!groups.TryGetValue(key, out var producer))
          {
            producer = new GroupProducer<TKey, T>(groups.Count + 1, key);
            await Outbox.WriteAsync(producer, cancellationToken);
            groups.Add(key, producer);
          }

          await producer.WriteAsync(item, cancellationToken);
        }

        foreach (var groupWriter in groups.Values)
          groupWriter.Dispose();
      }
    }
  }
}