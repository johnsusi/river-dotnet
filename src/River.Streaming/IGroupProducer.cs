using System;

namespace River.Streaming
{
  public interface IGroupProducer<TKey, T> : IProducer<T>
  {
    int Index { get; }

    TKey Key { get; }

  }
}