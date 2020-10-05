using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace River.Streaming
{

  public static partial class Operators
  {
    private static Channel<T> CreateChannel<T>(ChannelOptions? options)
      => options switch
      {
        UnboundedChannelOptions opts => Channel.CreateUnbounded<T>(opts),
        BoundedChannelOptions opts => Channel.CreateBounded<T>(opts),
        _ => Channel.CreateUnbounded<T>()
      };

    public static void LinkTo<T>(this Producer<T> producer, Consumer<T> consumer, ChannelOptions? options = default)
    {
      if (producer.Source.Task.IsCompletedSuccessfully)
      {
        if (producer.Source.Task.Result is RefCountChannel<T> channel)
        {

          if (!consumer.Source.Task.IsCompleted)
          {
            if (!consumer.Source.TrySetResult(channel))
            {
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
        if (consumer.Source.Task.Result is RefCountChannel<T> channel)
        {
          channel.Retain();
          if (!producer.Source.TrySetResult(channel))
          {
            channel.Release();
            throw new Exception("Did not work");
          }
        }
      }
      else
      {
        var channel = new RefCountChannel<T>(CreateChannel<T>(options));
        if (!producer.Source.TrySetResult(channel) || !consumer.Source.TrySetResult(channel))
          throw new Exception("Did not work");
      }
    }

    public static void LinkTo<T>(this IAsyncEnumerable<Producer<T>> producer, Consumer<T> consumer, ChannelOptions? options = null)
    {
      var _ = Task.Run(async () =>
      {
        await foreach (var p in producer)
        {
          p.LinkTo(consumer, options);
        }
      });
    }
  }
}