

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using River.Streaming.Helpers;

namespace River.Streaming.Actors
{

  internal class MergeActor<T> : AbstractActor
  {
    private readonly IAsyncEnumerable<IProducer<T>> _producers;
    private readonly ChannelOptions? _options;
    public IProducer<T> Outbox { get; } = new Producer<T>();

    public MergeActor(IAsyncEnumerable<IProducer<T>> producers, ChannelOptions? options = null)
    {
      _producers = producers;
      _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
      using var writer = await Outbox.GetWriterAsync(cancellationToken);
      var consumer = new Consumer<T>();
      var task = Task.Run(async () =>
      {
        using var reader = await consumer.GetReaderAsync(cancellationToken);
        await foreach (var item in reader.ReadAllAsync(cancellationToken))
          await writer.WriteAsync(item, cancellationToken);
      }, cancellationToken);
      await foreach (var producer in _producers)
        producer.LinkTo(consumer, _options);
      await task;
    }

  }
}
