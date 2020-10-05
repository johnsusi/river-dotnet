namespace River.Streaming
{
  public static partial class Operators
  {
    public static void Multicast<T>(this Producer<T> producer, params Consumer<T>[] consumers)
    {
      
    }
  }
}