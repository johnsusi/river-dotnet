using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace River.Streaming.Test.Operators
{
  public class RegulatePressureTest
  {

    [Fact]
    public async Task RegulatePressure_WithLimit_ShouldLimitPressure()
    {
      int limit = 7;
      var expected = Enumerable.Range(1, limit);
      using var producer = new Producer<int>();
      using var feedback = new Producer<int>();
      using var consumer = new Consumer<int>();

      producer
        .RegulatePressure(limit, feedback)
        .LinkTo(consumer);

      foreach (var number in expected)
        await producer.WriteAsync(number);

      await producer.WriteAsync(-1);

      var actual = new List<int>();
      for (int i = 0;i < limit;++i)
        actual.Add(await consumer.ReadAsync());

      Assert.Equal(expected, actual);
      Assert.False(consumer.TryRead(out _));
    }

    [Fact]
    public async Task RegulatePressure_WithLimitAndFeedback_ShouldRestorePressure()
    {
      int limit = 7;
      var expected = Enumerable.Range(1, limit);
      using var producer = new Producer<int>();
      using var feedback = new Producer<int>();
      using var consumer = new Consumer<int>();

      producer
        .RegulatePressure(limit - 1, feedback)
        .LinkTo(consumer);

      foreach (var number in expected)
        await producer.WriteAsync(number);

      var actual = new List<int>();
      for (int i = 0;i < limit - 1;++i)
        actual.Add(await consumer.ReadAsync());

      Assert.False(consumer.TryRead(out _));

      await feedback.WriteAsync(1);
      actual.Add(await consumer.ReadAsync());

      Assert.Equal(expected, actual);
    }
  }
}
