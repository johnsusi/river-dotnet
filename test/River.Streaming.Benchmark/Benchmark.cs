// using System;
// using System.Linq;
// using System.Threading;
// using System.Threading.Channels;
// using System.Threading.Tasks;
// using BenchmarkDotNet.Attributes;
// using BenchmarkDotNet.Order;
// using River.Streaming.Actors;
// using River.Streaming.Helpers;

// namespace River.Streaming.Benchmarks
// {
//   [ShortRunJob]
//   [ThreadingDiagnoser]
//   [MemoryDiagnoser]
//   [Orderer(SummaryOrderPolicy.Declared, MethodOrderPolicy.Declared)]
//   // [ReturnValueValidator] // Does not work when using attributes
//   public class LinkToBenchmark
//   {

//     [Params(1000000)]
//     public int Messages;

//     [Params(0)]
//     public int ChannelCapacity;


//     class BenchProducer : ActionActor
//     {
//       public IProducer<int> Outbox { get; set; } = new Producer<int>();
//       public int Limit { get; set; }

//       public BenchProducer()
//       {
//         Action = async cancellationToken =>
//         {
//           using var writer = await Outbox.GetWriterAsync(cancellationToken);
//           for (int i = 0; i < Limit; ++i)
//             await writer.WriteAsync(i, cancellationToken);
//         };
//       }
//     }

//     class BenchConsumer : ActionActor
//     {
//       public long Sum { get; set; } = 0;
//       public IConsumer<int> Inbox { get; set; } = new Consumer<int>();

//       public BenchConsumer()
//       {
//         Action = async cancellationToken =>
//         {
//           using var reader = await Inbox.GetReaderAsync(cancellationToken);
//           await foreach (var item in reader.ReadAllAsync(cancellationToken))
//             Sum += item;
//         };
//       }
//     }

//     private ChannelOptions ChannelOptions =>
//       ChannelCapacity == 0
//         ? null
//         : new BoundedChannelOptions(ChannelCapacity);

//     [Benchmark(Baseline = true)]
//     [Arguments(1)]
//     public async Task<long> Unicast(int count)
//     {

//       var producer = new BenchProducer { Limit = Messages };
//       var consumer = new BenchConsumer { };


//       producer.Outbox.LinkTo(consumer.Inbox, ChannelOptions);

//       await Task.WhenAll(
//         producer,
//         consumer
//       );

//       var result = consumer.Sum;
//       return result;

//     }

//     [Benchmark]
//     // [Arguments(1)]
//     // [Arguments(10)]
//     // [Arguments(100)]
//     // [Arguments(1000)]
//     // [Arguments(10000)]
//     [Arguments(100000)]
//     // [Arguments(1000000)]
//     public async Task<long> Anycast(int count)
//     {

//       var producer = new BenchProducer { Limit = Messages };
//       var consumers = Enumerable.Range(0, count)
//         .Select(index => new BenchConsumer())
//         .ToArray();


//       foreach (var consumer in consumers)
//         producer.Outbox.LinkTo(consumer.Inbox, ChannelOptions);


//       var tasks =
//         consumers
//           .Select(c => c.Completion)
//           .Append(producer.Completion)
//           .ToList();

//       while (tasks.Count() > 0)
//       {
//         var task = await Task.WhenAny(tasks);
//         try
//         {
//           await task;
//         }
//         finally
//         {
//           tasks.Remove(task);
//         }
//       }

//       var result = consumers.Select(c => c.Sum).Sum();
//       return result;
//     }

//     // [Benchmark]
//     // [Arguments(1)]
//     // [Arguments(10)]
//     // [Arguments(100)]
//     // public async Task<long> Multicast(int count)
//     // {

//     //   var producer = new Producer { Limit = Messages };
//     //   var consumers = Enumerable.Range(0, count)
//     //     .Select(index => new Consumer())
//     //     .ToArray();

//     //   producer.Multicast(multicast =>
//     //   {
//     //     foreach (var consumer in consumers)
//     //       multicast.LinkTo(consumer);
//     //   }, ChannelOptions);

//     //   var tasks = consumers
//     //     .Select(consumer => consumer.ExecuteAsync())
//     //     .Append(producer.ExecuteAsync()).ToArray();
//     //   await Task.WhenAll(tasks);

//     //   var result = consumers.Select(c => c.Sum).Sum() / count;
//     //   return result;
//     // }

//     // [Benchmark]
//     // [Arguments(1)]
//     // [Arguments(10)]
//     // [Arguments(100)]
//     // [Arguments(1000)]
//     // [Arguments(10000)]
//     // [Arguments(100000)]
//     // [Arguments(1000000)]
//     // public async Task<long> GroupBy_Merge(int count)
//     // {

//     //   var producer = new Producer { Limit = Messages };
//     //   var consumer = new Consumer { };

//     //   producer
//     //     .GroupBy(x => x % count,
//     //     group => group.LinkTo(consumer, ChannelOptions),
//     //     ChannelOptions);


//     //   await Task.WhenAll(
//     //     producer.ExecuteAsync(),
//     //     consumer.ExecuteAsync()
//     //   );

//     //   var result = consumer.Sum;
//     //   return result;
//     // }

//     // [Benchmark]
//     // [Arguments(1)]
//     // [Arguments(10)]
//     // [Arguments(100)]
//     // public async Task<long> Transform(int count)
//     // {

//     //   var producer = new Producer { Limit = Messages };
//     //   var consumer = new Consumer { };


//     //   IProducer<int> p = producer;
//     //   for (int i = 0; i < count; ++i)
//     //     p = p.Transform(x => x);
//     //   p.LinkTo(consumer, ChannelOptions);

//     //   await Task.WhenAll(
//     //     producer.ExecuteAsync(),
//     //     consumer.ExecuteAsync()
//     //   );

//     //   var result = consumer.Sum;
//     //   return result;
//     // }

//   }

// }