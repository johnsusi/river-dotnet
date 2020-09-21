using System;
using System.Runtime.CompilerServices;
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
        var task = await Task.WhenAny(_completion, Task.Delay(Timeout.Infinite, cancellationToken));
        if (task == _completion) await task;
        else throw new Exception("Actor still running");
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