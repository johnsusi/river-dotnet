using System.Threading;
using System.Threading.Tasks;

namespace River.Streaming.Helpers
{
  public class GroupProducer<TKey, T> : Producer<T>, IGroupProducer<TKey, T>
  {
    public int Index { get; protected set; }

    public TKey Key { get; protected set; }

    public GroupProducer(int index, TKey key)
    {
      Index = index;
      Key = key;
    }

  }
}