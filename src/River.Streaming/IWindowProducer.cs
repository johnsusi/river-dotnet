using System;

namespace River.Streaming
{
  public interface IWindowProducer<T> : IProducer<T>
  {
    int Index { get; }
    TimeSpan Duration { get; }
    int MaxSize { get; }
  }
}