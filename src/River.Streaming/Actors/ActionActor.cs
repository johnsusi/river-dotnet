
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace River.Streaming.Actors
{

  public class ActionActor : AbstractActor
  {
    private Func<CancellationToken, Task>? _action;
    public Func<CancellationToken, Task> Action
    {
      get => _action ?? throw new Exception("Action is not set");
      set => _action = value;
    }
    public ActionActor() {}
    public ActionActor(Func<CancellationToken, Task> action)
    {
      _action = action;
    }
    protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
      await Action(cancellationToken);
    }

  }
}
