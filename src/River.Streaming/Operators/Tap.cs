using System;

namespace River.Streaming
{

  public static partial class Operators
  {
    public static IProducer<T> Tap<T>(this IProducer<T> producer, Action<IProducer<T>> action)
    {
      action(producer);
      return producer;
    }
  }
}