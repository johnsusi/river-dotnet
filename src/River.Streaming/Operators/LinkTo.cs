using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using River.Streaming.Helpers;

namespace River.Streaming
{

  internal class InternalChannel<T> : Channel<T>
  {
    private readonly Channel<T> _channel;
    private readonly DisposableChannelReader<T> _reader;
    private readonly DisposableChannelWriter<T> _writer;

    internal InternalChannel(Channel<T> channel)
    {
      _channel = channel;
      _reader = new DisposableChannelReader<T>(_channel);
      _writer = new DisposableChannelWriter<T>(_channel);
      Reader = _reader;
      Writer = _writer;
    }
  }

  public static partial class Operators
  {

    private static Channel<T> CreateChannel<T>(ChannelOptions? options)
      => options switch
      {
        UnboundedChannelOptions opts => Channel.CreateUnbounded<T>(opts),
        BoundedChannelOptions opts => Channel.CreateBounded<T>(opts),
        _ => Channel.CreateUnbounded<T>()
      };


    public static void LinkTo<T>(this IProducer<T> producer, IConsumer<T> consumer, ChannelOptions? options = default)
    {
      if (producer.Source.Task.IsCompletedSuccessfully)
      {

        var channel = producer.Source.Task.Result;
        if (channel.Reader is DisposableChannelReader<T> reader)
        {
          reader.Retain();

          if (!consumer.Source.Task.IsCompleted)
          {
            if (!consumer.Source.TrySetResult(channel))
            {
              reader.Release();
              throw new Exception("Did not work");
            }
          }
          else if (consumer.Source.Task.IsCompletedSuccessfully && channel != consumer.Source.Task.Result)
            throw new Exception("Invalid configuration");
          else throw new Exception("Unhandled condition");
        }
        else
          throw new Exception("Invalid configuration");

      }
      else if (consumer.Source.Task.IsCompletedSuccessfully)
      {
        var channel = consumer.Source.Task.Result;
        if (channel.Writer is DisposableChannelWriter<T> writer)
        {
          writer.Retain();
          if (!producer.Source.TrySetResult(channel))
          {
            writer.Release();
            throw new Exception("Did not work");
          }
        }
      }
      else
      {
        var channel = new InternalChannel<T>(CreateChannel<T>(options));
        if (!producer.Source.TrySetResult(channel) || !consumer.Source.TrySetResult(channel))
          throw new Exception("Did not work");
      }

    }

    // public static void LinkTo<TKey, T>(this IGroupProducer<TKey, T> producer, IConsumer<T> consumer, ChannelOptions? options = null)
    //   => LinkTo(producer as IProducer<T>, consumer, options);
    public static void LinkTo<T>(this IAsyncEnumerable<IProducer<T>> producer, IConsumer<T> consumer, ChannelOptions? options = null)
    {
      var _ = Task.Run(async () =>
      {
        await foreach (var p in producer)
        {
          p.LinkTo(consumer, options);
        }
      });
      // producer.Consume(p => p.LinkTo(consumer, options));
    }

    public static void LinkTo<TKey, T>(this IProducer<IGroupProducer<TKey, T>> producer, IConsumer<T> consumer, ChannelOptions? options = null)
    {
      producer.Consume(p => p.LinkTo(consumer));
    }

  }
}