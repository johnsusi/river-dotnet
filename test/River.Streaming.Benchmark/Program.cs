using BenchmarkDotNet.Running;

namespace River.Streaming.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
          BenchmarkSwitcher
            .FromAssembly(typeof(Program).Assembly)
            .Run(args);
        }
    }
}
