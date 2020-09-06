namespace River.Streaming
{
  public static partial class Operators
  {
    public static void Multicast<T>(this IProducer<T> producer, params IConsumer<T>[] consumers)
    {

    }
  }
}