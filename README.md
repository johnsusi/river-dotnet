# River.Streaming

River.Streaming is a library for writing concurrent streaming applications. Concurrency is achieved using actors which consume and produce messages.

## Actor

An actor is the unit of concurrency.

```c#
  interface IActor
  {
    Task Completion { get; }
    void Start();
    Task CancelAsync(CancellationToken cancellationToken = default);
  }
```

Much like a real actor on stage, an Actor can be started (Action!), cancelled (Cut!) and completed (That's a wrap!).

Actors model concurrency:

```c#

  class MinimalActor : AbstractActor
  {
    protected Task ExecuteAsync(CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();
      return Task.CompletedTask;
    }
  }

```

## Producer

```c#

Producers produce messages. The actual implementation is based on channels.

  public interface IProducer<T>
  {
    Task<DisposableChannelWriter<T>> GetWriterAsync(CancellationToken cancellationToken = default);
  }

```

The API might look a bit confusing at first, but you will soon appreciate that wiring together actors is usualy an asynchronous step that happens after the actor is created. The actor itself can just await its producers and consumers before the actual processing starts.

A minimalistic producer actor:

```c#

class Actor : AbstractActor
{
    public IProducer<int> Outbox { get; } = new Producer<int>();

    protected async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var writer = await Outbox.GetWriterAsync(cancellationToken);
        await writer.WriteAsync(42, cancellationToken);
    }
}

```

When the actor completes the ChannelWriter will be closed and thus signalling any linked consumers that there will be no more data flowing through the stream.

## Consumer

```c#

Consumers consume messages.

  public interface IConsumer<T>
  {
    Task<DisposableChannelReader<T>> GetReaderAsync(CancellationToken cancellationToken = default);
  }

```

Consumers mirrors the producer API.

## LinkTo

LinkTo is the basic operation that connects a producer with a consumer.

```c#

  class Hello
  {
    public string Name { get; set; }
  }

  class Source : AbstractActor
  {
    public IProducer<Hello> Outbox { get; } = new Producer<Hello>();
    protected async Task ExecuteAsync(CancellationToken cancellationToken)
    {
      using var writer = await Outbox.GetWriterAsync(cancellationToken);
      await writer.WriteAsync(new Hello { Name = "World" }, cancellationToken);
    }
  }

  class Sink : AbstractActor
  {
    public IConsumer<Hello> Inbox { get; } = new Consumer<Hello>();
    protected async Task ExecuteAsync(CancellationToken cancellationToken)
    {
      using var reader = await Inbox.GetReaderAsync(cancellationToken);
      await foreach (var hello in reader.ReadAllAsync(cancellationToken))
        Console.WriteLine($"Hello, {hello.Name}!");
    }
  }

  public static void Main()
  {
    using var source = new Source();
    using var sink = new Sink();
    source.Outbox.LinkTo(sink.Inbox);
    await Task.WhenAll(source, sink);
  }

```