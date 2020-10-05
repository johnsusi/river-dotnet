using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace River.Streaming
{
  public static partial class Operators
  {
    public static Producer<T> Decorate<T>(this Producer<T> producer, Action<T> decorator)
    {
      return producer.Transform(msg =>
      {
        decorator(msg);
        return msg;
      }, new BoundedChannelOptions(1));
    }
  }

  public static partial class Operators
  {
    public static Producer<T> Decorate<T>(this Producer<T> producer, Func<T, Task> decorator)
    {
      return producer.Transform(async msg =>
      {
        await decorator(msg);
        return msg;
      }, new BoundedChannelOptions(1));
    }
  }
}