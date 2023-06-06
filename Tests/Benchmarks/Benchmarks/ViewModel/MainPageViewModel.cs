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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Benchmarks.Model;
using Realms;
using Xamarin.Forms;

namespace Benchmarks.ViewModel
{
    public class MainPageViewModel : BaseViewModel
    {
        private const string DryRunJob = "Dry run";
        private const string ShortJob = "Short job";
        private const string DefaultJob = "Default job";

        private static readonly string ResultsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "benchmarks-ios");

        private readonly string[] _args;

        private bool _isRunning;
        private string? _resultsLocation;

        public ObservableCollection<BenchmarkResult> BenchmarkResults { get; private set; }

        public IList<string> JobTypes { get; } = new[] { DefaultJob, ShortJob, DryRunJob };

        public BenchmarkConfig Config { get; }

        public bool IsRunning
        {
            get => _isRunning;
            private set => SetProperty(ref _isRunning, value);
        }

        public ICommand RunBenchmarksCommand { get; }

        public string? ResultsLocation
        {
            get => _resultsLocation;
            private set => SetProperty(ref _resultsLocation, value);
        }

        public MainPageViewModel(string[] args)
        {
            Directory.CreateDirectory(ResultsFolder);

            var config = new RealmConfiguration(Path.Combine(ResultsFolder, "config.realm"))
            {
                Schema = new[] { typeof(BenchmarkConfig) }
            };

            var realm = Realm.GetInstance(config);
            Config = realm.Write(() =>
            {
                return realm.All<BenchmarkConfig>().SingleOrDefault() ?? realm.Add(new BenchmarkConfig
                {
                    SelectedJob = DefaultJob
                });
            });

            _args = args;
            RunBenchmarksCommand = new Command(async () => await RunBenchmarks());
            BenchmarkResults = new ObservableCollection<BenchmarkResult>();
        }

        public override async void OnAppearing()
        {
            base.OnAppearing();

            if (_args.Contains("--headless"))
            {
                string? artifactPath = null;
                string[]? filterPatterns = null;

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
            string[]? filterPatterns = null, string? artifactsPath = null)
        {
            var config = PerformanceTests.Program.GetCustomConfig();

            var job = headless ? Job.Default : Config.SelectedJob switch
            {
                DryRunJob => Job.Dry,
                ShortJob => Job.ShortRun,
                DefaultJob => Job.Default,
                _ => throw new NotSupportedException($"Invalid job: {Config.SelectedJob}")
            };

            config = config.WithOption(ConfigOptions.JoinSummary, join)
                           .AddJob(job);

            artifactsPath ??= Path.Combine(ResultsFolder, DateTimeOffset.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss"));
            if (!string.IsNullOrEmpty(artifactsPath))
            {
                config = config.WithArtifactsPath(artifactsPath);
            }

            filterPatterns ??= Config.Filters?.Split(";");
            if (filterPatterns?.Any() == true)
            {
                config = config.AddFilter(new GlobFilter(filterPatterns));
            }

            var summaries = Array.Empty<Summary>();

            IsRunning = true;

            await Task.Run(() =>
            {
                summaries = BenchmarkRunner.Run(typeof(PerformanceTests.Program).Assembly, config);
            });

            IsRunning = false;

            ResultsLocation = artifactsPath;

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
