using System;
using System.Threading;
using System.Threading.Tasks;

namespace River.Streaming.Actors
{

  public abstract class AbstractActor : IActor, IDisposable
  {
    private Task? _completion;
    public Task Completion
    {
      get => _completion ??= ExecuteAsync(_cancel.Token);
    }
    private readonly CancellationTokenSource _cancel = new CancellationTokenSource();

    public void Start() => _completion ??= ExecuteAsync(_cancel.Token);

    public async Task CancelAsync(CancellationToken cancellationToken = default)
    {
      if (_completion is null) return;
      try
      {
        _cancel.Cancel();
      }
      finally
      {        
        var timeout = Task.Delay(Timeout.Infinite, cancellationToken);
        await Task.WhenAny(_completion, timeout);
        cancellationToken.ThrowIfCancellationRequested();
      }
    }

    protected AbstractActor() {}

    protected abstract Task ExecuteAsync(CancellationToken cancellationToken = default);

    public void Dispose()
    {
      _cancel.Dispose();
    }

    public static implicit operator Task(AbstractActor actor) => actor.Completion;

  }
}