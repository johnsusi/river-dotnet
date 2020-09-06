using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using River.Streaming.Actors;
using River.Streaming.Helpers;

namespace River.Streaming
{
  public static partial class Operators
  {

    // public static IProducer< TOut > Join<TIn, TOther, TOut>(this IProducer<TIn> producer, IProducer<TOther> other, Func<TIn, TOther, TOut> action);

    // public class MergeProducer<T> : IProducer<T>
    // {
    //   public Task<DisposableChannelWriter<T>> GetWriterAsync(CancellationToken cancellationToken = default)
    //   {
    //     throw new System.NotImplementedException();
    //   }
    // }

    public static IProducer<T> Merge<T>(this IProducer<T> producer, IProducer<T> other)
    {
      var actor = new TransformActor<T, T>(x => new ValueTask<T>(x));
      producer.LinkTo(actor.Inbox);
      other.LinkTo(actor.Inbox);
      return actor.Outbox;
    }

    public static IProducer<T> Merge<T>(this IAsyncEnumerable<IProducer<T>> producers, ChannelOptions? options = null)
    {
      var actor = new TransformActor<T, T>(x => new ValueTask<T>(x));
      var _ = Task.Run(async () =>
      {
        actor.Start();
        await foreach (var producer in producers)
          producer.LinkTo(actor.Inbox);

        await actor;
      });
      return actor.Outbox;
    }
    // public static IProducer<T> Merge<TKey, T>(this IProducer<IGroupProducer<TKey, T>> producer, ChannelOptions? options = null)
    //   => Merge(producer.Transform(x => (IProducer<T>)x), options);
    // public static IProducer<T> Merge<TKey, T>(this IProducer<IWindowProducer<T>> producer, ChannelOptions? options = null)
    //   => Merge((IProducer<IProducer<T>>)producer, options);
  }
}