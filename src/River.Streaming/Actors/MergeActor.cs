using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace River.Streaming.Actors
{

  internal class MergeActor<T> : AbstractActor
  {
    private readonly IAsyncEnumerable<Producer<T>> _producers;
    private readonly ChannelOptions? _options;
    public Producer<T> Outbox { get; } = new Producer<T>();

    public MergeActor(IAsyncEnumerable<Producer<T>> producers, ChannelOptions? options = null)
    {
      _producers = producers;
      _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
      using (Outbox)
      {
        var dummy = new Producer<T>();
        var inbox = new Consumer<T>();
        dummy.LinkTo(inbox, _options);
        var task = Task.Run(async () =>
        {
          await foreach (var item in inbox.ReadAllAsync(cancellationToken))
            await Outbox.WriteAsync(item, cancellationToken);
        }, cancellationToken);
        await foreach (var producer in _producers)
          producer.LinkTo(inbox, _options);
        dummy.Dispose();
        await task;
      }
    }

  }
}
