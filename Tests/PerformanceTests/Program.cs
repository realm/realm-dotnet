using System;
using System.Linq;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Portability.Cpu;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Perfolizer.Horology;

namespace PerformanceTests
{
    class Program
    {
        static void Main(string[] args)
        {
            var defaultConfig = DefaultConfig.Instance;
            var config = new ManualConfig()
                .AddColumnProvider(defaultConfig.GetColumnProviders().ToArray())
                .AddLogger(defaultConfig.GetLoggers().ToArray())
                .AddAnalyser(defaultConfig.GetAnalysers().ToArray())
                .AddValidator(defaultConfig.GetValidators().ToArray())
                .WithUnionRule(defaultConfig.UnionRule)
                .WithSummaryStyle(defaultConfig.SummaryStyle)
                .WithArtifactsPath(defaultConfig.ArtifactsPath)
                .AddDiagnoser(MemoryDiagnoser.Default)
                .AddExporter(new JenkinsHtmlExporter(), MarkdownExporter.GitHub, JsonExporter.Full);

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
        }

        class JenkinsHtmlExporter : HtmlExporter
        {
            private CpuInfo _mockCpuInfo = new CpuInfo("MockIntel(R) Core(TM) i7-6700HQ CPU 2.60GHz",
                                              physicalProcessorCount: 1,
                                              physicalCoreCount: 4,
                                              logicalCoreCount: 8,
                                              nominalFrequency: Frequency.FromMHz(3100),
                                              maxFrequency: Frequency.FromMHz(3100),
                                              minFrequency: Frequency.FromMHz(3100));
            public override void ExportToLog(Summary summary, ILogger logger)
            {
                var cpuInfoProp = typeof(HostEnvironmentInfo).GetProperty(nameof(HostEnvironmentInfo.CpuInfo));
                cpuInfoProp.SetValue(summary.HostEnvironmentInfo, new Lazy<CpuInfo>(() => _mockCpuInfo));
                base.ExportToLog(summary, logger);
            }
        }
    }
}
