using System;

namespace River.Streaming.Helpers
{
  public class WindowProducer<T> : Producer<T>, IWindowProducer<T>
  {
    public int Index { get; private set; }
    public TimeSpan Duration { get; private set; }
    public int MaxSize { get; private set; }

    public WindowProducer(int index, TimeSpan duration, int size)
    {
      Index = index;
      Duration = duration;
      MaxSize = size;
    }
  }
}