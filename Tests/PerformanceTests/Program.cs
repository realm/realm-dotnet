////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Portability.Cpu;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using Perfolizer.Horology;

namespace PerformanceTests
{
    public static class Program
    {
        public static void Main(string[] args)
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
                .AddJob(Job.ShortRun.WithToolchain(InProcessEmitToolchain.Instance))
                .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.Method, MethodOrderPolicy.Alphabetical))
                .AddExporter(new JenkinsHtmlExporter(), MarkdownExporter.GitHub, JsonExporter.Full);

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
        }

        // This is needed because the Jenkins user does not have permissions to use System.Management.ManagementObjectSearcher.
        // There's no public API to prevent BenchmarkDotNet from looking it up, so when building the summary, we get an access
        // denied exception. This uses reflection to replace the HostEnvironmentInfo.CpuInfo lazy with one that returns a mock
        // CpuInfo. Since this is the first exporter being run, it will modify the summary for the rest, so we don't need to
        // fiddle with them. Once we move to Github actions, this can be removed.
        private class JenkinsHtmlExporter : HtmlExporter
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
