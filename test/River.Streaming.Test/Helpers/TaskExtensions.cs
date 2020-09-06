using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace River.Streaming.Test.Helpers
{
  public static partial class TaskHelpers
  {
    public static async Task WhenAll(this IEnumerable<Task> tasks, CancellationTokenSource cancel)
    {
      var executing = tasks.ToList();
      while (executing.Count() > 0)
      {
        var task = await Task.WhenAny(executing);
        try
        {
          await task;
        }
        catch(Exception)
        {
          cancel.Cancel();
          await Task.WhenAll(executing); // This will rethrow the exception
        }
        finally
        {
          executing.Remove(task);
        }
      }

    }
  }

}