using System.Runtime.CompilerServices;

namespace River.Streaming
{
  public static class IActorExtensions
  {
    public static TaskAwaiter GetAwaiter(this IActor actor) => actor.Completion.GetAwaiter();

  }
}