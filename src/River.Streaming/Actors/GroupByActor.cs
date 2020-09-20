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

    public IConsumer<T> Inbox { get; } = new Consumer<T>();
    public IProducer<IGroupProducer<TKey, T>> Outbox { get; } = new Producer<IGroupProducer<TKey, T>>();

    public GroupByActor(Func<T, TKey> selector)
    {
      _selector = selector;

    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
      using var reader = await Inbox.GetReaderAsync(cancellationToken);
      using var writer = await Outbox.GetWriterAsync(cancellationToken);

      var groups = new Dictionary<TKey, DisposableChannelWriter<T>>();
      await foreach (var item in reader.ReadAllAsync(cancellationToken))
      {
        var key = _selector(item);
        if (!groups.TryGetValue(key, out var groupWriter))
        {
          var producer = new GroupProducer<TKey, T>(groups.Count+1, key);
          await writer.WriteAsync(producer, cancellationToken);
          groupWriter = await producer.GetWriterAsync(cancellationToken);
          groups.Add(key, groupWriter);
        }

        await groupWriter.WriteAsync(item, cancellationToken);
      }

      foreach (var groupWriter in groups.Values)
        groupWriter.Dispose();

    }
  }
}