using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace River.Streaming.Helpers
{

  // public class ChannelWriterDecorator<TIn, TOut> : DisposableChannelWriter<TIn>
  // {
  //   private readonly ChannelWriter<TOut> _writer;

  //   private Func<TIn, TOut> _transform;

  //   public ChannelWriterDecorator(ChannelWriter<TOut> writer, Func<TIn, TOut> transform)
  //   {
  //     _writer = writer;
  //     _transform = transform;
  //   }

  //   public override bool TryComplete(Exception error = null)
  //   {
  //     return _writer.TryComplete(error);
  //   }

  //   public override bool TryWrite(TIn item)
  //   {
  //     return _writer.TryWrite(_transform(item));
  //   }

  //   public override ValueTask<bool> WaitToWriteAsync(CancellationToken cancellationToken = default)
  //   {
  //     return WaitToWriteAsync(cancellationToken);
  //   }

  //   public override ValueTask WriteAsync(TIn item, CancellationToken cancellationToken = default)
  //   {
  //     return _writer.WriteAsync(_transform(item), cancellationToken);
  //   }
  // }

  // public class TransformDecorator<TIn, TOut> : IProducer<TOut>
  // {

  //   internal class FakeChannel : Channel<TIn>
  //   {
  //     internal FakeChannel(ChannelReader<TIn> reader, ChannelWriter<TIn> writer)
  //     {
  //       Reader = reader;
  //       Writer = writer;
  //     }
  //   }

  //   private readonly IProducer<TIn> _producer;

  //   public TransformDecorator(IProducer<TIn> producer, Func<TIn, TOut> transform)
  //   {
  //     _producer = producer;
  //     Task.Run(async () =>
  //     {
  //       var c = await _source.Task;
  //       var channel = new FakeChannel(null, new DisposableChannelWriter<TIn>(new ChannelWriterDecorator<TIn, TOut>(c.Writer, transform)));
  //       _producer.Source.TrySetResult(channel);
  //     });
  //   }

  //   Task<DisposableChannelWriter<TOut>> IProducer<TOut>.GetWriterAsync(CancellationToken cancellationToken)
  //   {
  //     throw new NotImplementedException();
  //   }
  //   private TaskCompletionSource<Channel<TOut>> _source = new TaskCompletionSource<Channel<TOut>>();
  //   TaskCompletionSource<Channel<TOut>> IProducer<TOut>.Source => _source;

  // }

  public class Producer<T> : IProducer<T>
  {

    private TaskCompletionSource<Channel<T>> _source = new TaskCompletionSource<Channel<T>>(TaskCreationOptions.RunContinuationsAsynchronously);

    TaskCompletionSource<Channel<T>> IProducer<T>.Source => _source;

    public async Task<DisposableChannelWriter<T>> GetWriterAsync(CancellationToken cancellationToken = default)
    {
      return await Task.Run(async () =>
      {
        var result = await _source.Task;
        if (result.Writer is DisposableChannelWriter<T> writer)
        {
          return writer;
        }
        throw new Exception("Writer is null");
      }, cancellationToken);
    }
  }
}