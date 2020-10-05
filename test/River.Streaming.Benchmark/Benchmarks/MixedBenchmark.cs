using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace River.Streaming.Benchmarks
{
  [ShortRunJob]
  [ThreadingDiagnoser]
  [MemoryDiagnoser]
  [Orderer(SummaryOrderPolicy.Declared, MethodOrderPolicy.Declared)]
  // [ReturnValueValidator] // Does not work when using attributes
  public class MixedBenchmark
  {

    [Params(1000_000)]
    public int Messages;


    [Benchmark(Baseline = true)]
    public async Task UnboundedChannel()
    {
      var channel = Channel.CreateUnbounded<int>();

      await Task.WhenAll(write(), read());

      async Task write()
      {
        for (int i = 0; i < Messages; ++i)
          await channel.Writer.WriteAsync(i);
        channel.Writer.TryComplete();
      }
      async Task read()
      {
        await foreach (var _ in channel.Reader.ReadAllAsync())
          ;
      }
    }

    [Benchmark]
    public async Task Unicast()
    {
      var producer = new Producer<int>();
      var consumer = new Consumer<int>();
      producer.LinkTo(consumer);

      await Task.WhenAll(write(), read());

      async Task write()
      {
        for (int i = 0; i < Messages; ++i)
          await producer.WriteAsync(i);
        producer.Dispose();
      }
      async Task read()
      {
        await foreach (var _ in consumer.ReadAllAsync())
          ;
      }
    }

    [Benchmark]
    public async Task GroupBy()
    {
      var producer = new Producer<int>();
      var consumer = new Consumer<int>();
      producer
        .GroupBy(x => 1)
        .LinkTo(consumer);

      await Task.WhenAll(write(), read());

      async Task write()
      {
        for (int i = 0; i < Messages; ++i)
          await producer.WriteAsync(i);
        producer.Dispose();
      }
      async Task read()
      {
        long sum = 0;
        await foreach (var msg in consumer.ReadAllAsync())
          sum += msg;
      }
    }

    [Benchmark]
    public async Task GroupBy1k()
    {
      var producer = new Producer<int>();
      var consumer = new Consumer<int>();
      producer
        .GroupBy(x => x % 1000)
        .LinkTo(consumer);

      await Task.WhenAll(write(), read());

      async Task write()
      {
        for (int i = 0; i < Messages; ++i)
          await producer.WriteAsync(i);
        producer.Dispose();
      }
      async Task read()
      {
        long sum = 0;
        await foreach (var msg in consumer.ReadAllAsync())
          sum += msg;
      }
    }

    [Benchmark]
    public async Task Window()
    {
      var producer = new Producer<int>();
      var consumer = new Consumer<int>();
      producer
        .Window(Messages)
        .Merge()
        .LinkTo(consumer);

      await Task.WhenAll(write(), read());

      async Task write()
      {
        for (int i = 0; i < Messages; ++i)
          await producer.WriteAsync(i);
        producer.Dispose();
      }
      async Task read()
      {
        long sum = 0;
        await foreach (var msg in consumer.ReadAllAsync())
          sum += msg;
      }
    }

    [Benchmark]
    public async Task Window1k()
    {
      var producer = new Producer<int>();
      var consumer = new Consumer<int>();
      producer
        .Window(Messages / 1000)
        .Merge()
        .LinkTo(consumer);

      await Task.WhenAll(write(), read());

      async Task write()
      {
        for (int i = 0; i < Messages; ++i)
          await producer.WriteAsync(i);
        producer.Dispose();
      }
      async Task read()
      {
        long sum = 0;
        await foreach (var msg in consumer.ReadAllAsync())
          sum += msg;
      }
    }

    // [Benchmark]
    // public async Task GroupBy1kWindow1k()
    // {
    //   var producer = new Producer<int>();
    //   var consumer = new Consumer<int>();
    //   producer
    //     .GroupBy(x => x % 1000)
    //     .Window(1)
    //     .LinkTo(consumer);

    //   await Task.WhenAll(write(), read());

    //   async Task write()
    //   {
    //     for (int i = 0; i < Messages; ++i)
    //       await producer.WriteAsync(i);
    //     producer.Dispose();
    //   }
    //   async Task read()
    //   {
    //     long sum = 0;
    //     await foreach (var msg in consumer.ReadAllAsync())
    //       sum += msg;
    //   }
    // }



  }
}