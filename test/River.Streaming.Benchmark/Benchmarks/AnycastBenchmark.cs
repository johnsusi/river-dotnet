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
  [Orderer(SummaryOrderPolicy.Declared, MethodOrderPolicy.Declared)]
  // [ReturnValueValidator] // Does not work when using attributes
  public class AnycastBenchmark
  {
    [Params(1000_000)]
    public int Messages;

    [Params(1, 10, 100, 1000, 10_000, 100_000, 1000_000)]
    public int Consumers;

    private Producer<int> _producer;
    private List<Consumer<int>> _consumers;

    [GlobalSetup]
    public void Setup()
    {
      var options = new UnboundedChannelOptions();
      _producer = new Producer<int>();
      _consumers = new List<Consumer<int>>();
      for (int i = 0;i < Consumers;++i)
        _consumers.Add(new Consumer<int>());

      foreach (var consumer in _consumers)
        _producer.LinkTo(consumer, options);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
      _producer.Dispose();
      foreach (var consumer in _consumers)
      {
        consumer.Dispose();
      }
    }

    [Benchmark]
    public async Task WriteBeforeRead()
    {
      await write();
      await readAll();

      Task readAll() => Task.WhenAll(_consumers.Select(consumer => read(consumer, Messages / Consumers)));

      static async Task read(Consumer<int> consumer, int readCount)
      {
        for (int i = 0;i < readCount;++i)
          await consumer.ReadAsync();
      }

      async Task write()
      {
        for (int i = 0;i < Messages;++i)
          await _producer.WriteAsync(i);
      }
    }

    [Benchmark]
    public async Task Concurrent()
    {
      await Task.WhenAll(
        readAll(),
        write()
      );


      Task readAll() => Task.WhenAll(_consumers.Select(consumer => read(consumer, Messages / Consumers)));

      static async Task read(Consumer<int> consumer, int readCount)
      {
        for (int i = 0;i < readCount;++i)
          await consumer.ReadAsync();
      }

      async Task write()
      {
        
        for (int i = 0;i < Messages;++i)
          await _producer.WriteAsync(i);
      }
    }
  }
}