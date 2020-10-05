using System;

namespace River.Streaming
{

  public static partial class Operators
  {
    public static Producer<T> Tap<T>(this Producer<T> producer, Action<Producer<T>> action)
    {
      action(producer);
      return producer;
    }
  }
}