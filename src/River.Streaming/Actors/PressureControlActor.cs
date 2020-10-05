using System;
using System.Threading;
using System.Threading.Tasks;

namespace River.Streaming.Actors
{

    public class PressureControlActor<T, TFeedback> : AbstractActor
  {


    private readonly int _limit;
    private readonly Func<int, int, TFeedback, int> _control;

    public Consumer<T> Inbox { get; } = new Consumer<T>();
    public Producer<T> Outbox { get; } = new Producer<T>();
    public Consumer<TFeedback> Feedback { get; } = new Consumer<TFeedback>();


    public PressureControlActor(int limit, Func<int, int, TFeedback, int> control)
    {
      _limit = limit;
      _control = control;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
      using (Inbox)
      using (Outbox)
      using (Feedback)
      {

        int pressure = 0;

        while (await Inbox.WaitToReadAsync(cancellationToken))
        {
          if (pressure >= _limit)
          {
            if (!await Feedback.WaitToReadAsync(cancellationToken))
              throw new Exception("Feedback channel closed prematurely");
            while (Feedback.TryRead(out var x))
              pressure = _control(pressure, _limit, x);
            if (pressure < 0) throw new Exception("Feedback channel has to many elements");
          }

          while (pressure < _limit && Inbox.TryRead(out var item))
          {
            await Outbox.WriteAsync(item, cancellationToken);
            ++pressure;
          }

        }
        // await foreach (var item in Feedback.ReadAllAsync(cancellationToken))
        //   pressure = _control(pressure, _limit, item);
        // if (pressure < 0) throw new Exception("Feedback channel has to many elements");
      }
    }
  }
}