////////////////////////////////////////////////////////////////////////////
//
// Copyright 2021 Realm Inc.
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Benchmarks.Model;
using Xamarin.Forms;

namespace Benchmarks.ViewModel
{
    public class MainPageViewModel : BaseViewModel
    {
        private readonly string[] _args;

        public ObservableCollection<BenchmarkResult> BenchmarkResults { get; private set; }

        private bool _isRunning;

        public bool IsRunning
        {
            get => _isRunning;
            private set => SetProperty(ref _isRunning, value);
        }

        public ICommand RunBenchmarksCommand { get; }

        public MainPageViewModel(string[] args)
        {
            _args = args;
            RunBenchmarksCommand = new Command(async () => await RunBenchmarks());
            BenchmarkResults = new ObservableCollection<BenchmarkResult>();
        }

        public override async void OnAppearing()
        {
            base.OnAppearing();

            if (_args.Contains("--headless"))
            {
                string artifactPath = null;
                string[] filterPatterns = null;

                var artifactArgumentIndex = Array.IndexOf(_args, "--artifacts");
                if (artifactArgumentIndex >= 0)
                {
                    artifactPath = _args[artifactArgumentIndex + 1];
                }

                var filterArgumentIndex = Array.IndexOf(_args, "-f");
                if (filterArgumentIndex >= 0)
                {
                    // The filter needs to be the last argument
                    var extractedPatterns = _args[(filterArgumentIndex + 1).._args.Length];
                    filterPatterns = extractedPatterns.Contains("\"*\"") ? null : extractedPatterns;
                }

                var join = _args.Contains("--join");

                await RunBenchmarks(true, join, filterPatterns, artifactPath);

                TerminateApp();
            }
        }

        public void TerminateApp()
        {
            Thread.CurrentThread.Abort();
        }

        private async Task RunBenchmarks(bool headless = false, bool join = false,
            string[] filterPatterns = null, string artifactsPath = null)
        {
            var config = PerformanceTests.Program.GetCustomConfig();

            config = config.WithOption(ConfigOptions.JoinSummary, join);

            if (!string.IsNullOrEmpty(artifactsPath))
            {
                config = config.WithArtifactsPath(artifactsPath);
            }

            if (filterPatterns?.Any() == true)
            {
                config = config.AddFilter(new GlobFilter(filterPatterns));
            }

            Summary[] summaries = null;

            IsRunning = true;

            await Task.Run(() =>
            {
                summaries = BenchmarkRunner.Run(typeof(PerformanceTests.Program).Assembly, config);
            });

            IsRunning = false;

            if (!headless)
            {
                BenchmarkResults.Clear();

                foreach (var summary in summaries)
                {
                    var logger = new AccumulationLogger();

                    HtmlExporter.Default.ExportToLog(summary, logger);

                    ConclusionHelper.Print(logger,
                            summary.BenchmarksCases
                                   .SelectMany(benchmark => benchmark.Config.GetCompositeAnalyser().Analyse(summary))
                                   .Distinct()
                                   .ToList());

                    BenchmarkResults.Add(new BenchmarkResult(summary.Title, logger.GetLog()));
                }
            }
        }
    }
}
