using System.Threading;
using System.Threading.Tasks;

namespace River.Streaming
{
  public interface IActor
  {
    Task Completion { get; }
    void Start();
    Task CancelAsync(CancellationToken cancellationToken = default);
  }
}