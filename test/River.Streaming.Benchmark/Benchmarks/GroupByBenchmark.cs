using System;
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
  public class GroupByBenchmark
  {

    [Params(1000000)]
    public int Messages;

    [Params(1, 10, 100, 1000, 10_000, 100_000, 1000_000)]
    public int Groups;

    private IConsumer<int> _consumer;
    private IProducer<int> _producer;

    [GlobalSetup]
    public void Setup()
    {
      _producer = new Producer<int>();
      _consumer = new Consumer<int>();

      _producer
        .GroupBy(x => x % Groups)
        .Merge()
        .LinkTo(_consumer);

    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
      var writer = await _producer.GetWriterAsync();
      writer.Dispose();
      var reader = await _consumer.GetReaderAsync();
      reader.Dispose();
    }


    [Benchmark]
    public async Task WriteBeforeRead()
    {

      await write();
      await read();

      async Task read()
      {
        var reader = await _consumer.GetReaderAsync();
        for (int i = 0;i < Messages; ++i)
          await reader.ReadAsync();
      }

      async Task write()
      {
        var writer = await _producer.GetWriterAsync();
        for (int i = 0; i < Messages; ++i)
          await writer.WriteAsync(i);
      }
    }

    [Benchmark]
    public async Task Concurrent()
    {

      await Task.WhenAll(
        read(),
        write()
      );

      async Task read()
      {
        var reader = await _consumer.GetReaderAsync();
        for (int i = 0;i < Messages; ++i)
          await reader.ReadAsync();
      }

      async Task write()
      {
        var writer = await _producer.GetWriterAsync();
        for (int i = 0; i < Messages; ++i)
          await writer.WriteAsync(i);
      }
    }


  }

}