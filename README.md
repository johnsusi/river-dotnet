# River.Streaming

River.Streaming is a library for writing hybrid dataflow applications. Concurrency is achieved using actors which consume and produce messages.

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

Producers produce messages. The actual implementation is based on channels.

```c#

  public interface IProducer<T> : IDisposable
  {
    bool TryWrite(T item);
    ValueTask<bool> WaitToWriteAsync(CancellationToken cancellationToken = default);
    bool TryComplete(Exception? error = null);
    ValueTask WriteAsync(T item, CancellationToken cancellationToken = default);
  }

```

`Producer<T>` decorates a `ChannelWriter<T>` with reference counting and lazy assignment. Use `Dispose` to properly close the channel. `TryComplete` will be called on the underlying `ChannelWriter<T>` when the reference count hits zero.


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
      using (Outbox)
      {
        await Outbox.WriteAsync(new Hello { Name = "World" }, cancellationToken);
      }
    }
  }

  class Sink : AbstractActor
  {
    public IConsumer<Hello> Inbox { get; } = new Consumer<Hello>();
    protected async Task ExecuteAsync(CancellationToken cancellationToken)
    {
      using (Inbox)
      {
        await foreach (var hello in Inbox.ReadAllAsync(cancellationToken))
          Console.WriteLine($"Hello, {hello.Name}!");
      }
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
