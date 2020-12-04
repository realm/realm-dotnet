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

using System.Linq;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

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
                .AddJob(Job.Default.WithToolchain(InProcessEmitToolchain.Instance))
                .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.Method, MethodOrderPolicy.Alphabetical))
                .AddExporter(MarkdownExporter.GitHub, JsonExporter.FullCompressed);

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
        }
    }
}
