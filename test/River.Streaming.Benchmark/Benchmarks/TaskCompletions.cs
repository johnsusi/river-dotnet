using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using River.Streaming.Actors;
using River.Streaming.Helpers;

namespace River.Streaming.Benchmarks
{
  [ShortRunJob]
  [ThreadingDiagnoser]
  [MemoryDiagnoser]
  public class TaskCompletionBenchmark
  {


    [Params(1, 10, 100, 1000)]
    public int Consumers;



    [Benchmark]
    public async Task Raw()
    {
      var channel = Channel.CreateUnbounded<int>();

      var tasks = new List<Task> { write() };
      for (int i = 0;i < Consumers;++i)
        tasks.Add(read());

      await Task.WhenAll(tasks);

      async Task write()
      {
        for (int i = 0;i < Consumers;++i)
          await channel.Writer.WriteAsync(i);
      }

      async Task read()
      {
        await channel.Reader.ReadAsync();
      }


    }

    [Benchmark]
    public async Task TaskCompletionSource()
    {
      var channel = Channel.CreateUnbounded<int>(new UnboundedChannelOptions
      {
        SingleReader = false,
        SingleWriter = true,
        AllowSynchronousContinuations = true
      });

      var completions = new List<TaskCompletionSource<Channel<int>>>();
      for (int i = 0;i < Consumers;++i)
      {
        var tcs = new TaskCompletionSource<Channel<int>>(TaskCreationOptions.RunContinuationsAsynchronously);
        completions.Add(tcs);
        tcs.SetResult(channel);
      }

      var tasks = new List<Task>();
      for (int i = 0;i < Consumers;++i)
      {
        tasks.Add(read(completions[i]));
      }
      tasks.Add(write());
      await Task.WhenAll(tasks);

      async Task write()
      {
        for (int i = 0;i < Consumers;++i)
          await channel.Writer.WriteAsync(i);
      }

      static async Task read(TaskCompletionSource<Channel<int>> source)
      {
        var _channel = await source.Task;
        await _channel.Reader.ReadAsync();
      }


    }
  }

}