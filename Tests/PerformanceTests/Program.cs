using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Running;

namespace PerformanceTests
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = DefaultConfig.Instance
                                      .AddDiagnoser(MemoryDiagnoser.Default)
                                      .AddExporter(JsonExporter.Full);

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
        }
    }
}
