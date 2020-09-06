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

    private IProducer<int> _producer;
    private List<IConsumer<int>> _consumers;

    [GlobalSetup]
    public void Setup()
    {
      var options = new UnboundedChannelOptions();
      _producer = new Producer<int>();
      _consumers = new List<IConsumer<int>>();
      for (int i = 0;i < Consumers;++i)
        _consumers.Add(new Consumer<int>());

      foreach (var consumer in _consumers)
        _producer.LinkTo(consumer, options);

    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
      var writer = await _producer.GetWriterAsync();
      writer.Dispose();
      foreach (var consumer in _consumers)
      {
        var reader = await consumer.GetReaderAsync();
        reader.Dispose();
      }
    }

    [Benchmark]
    public async Task WriteBeforeRead()
    {
      await write();
      await readAll();

      Task readAll() => Task.WhenAll(_consumers.Select(consumer => read(consumer, Messages / Consumers)));

      async Task read(IConsumer<int> consumer, int readCount)
      {
        var reader = await consumer.GetReaderAsync();
        for (int i = 0;i < readCount;++i)
          await reader.ReadAsync();
      }

      async Task write()
      {
        var writer = await _producer.GetWriterAsync();
        for (int i = 0;i < Messages;++i)
          await writer.WriteAsync(i);
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

      async Task read(IConsumer<int> consumer, int readCount)
      {
        var reader = await consumer.GetReaderAsync();
        for (int i = 0;i < readCount;++i)
          await reader.ReadAsync();
      }

      async Task write()
      {
        var writer = await _producer.GetWriterAsync();
        for (int i = 0;i < Messages;++i)
          await writer.WriteAsync(i);
      }
    }
  }
}