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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Running;
using Xamarin.Forms;

namespace Benchmarks.ViewModel
{
    public class MainPageViewModel : BaseViewModel
    {
        private readonly string[] _args;

        private string _benchmarkResults;

        public string BenchmarkResults
        {
            get => _benchmarkResults;
            private set => SetProperty(ref _benchmarkResults, value);
        }

        public ICommand RunBenchmarksCommand { get; }

        public MainPageViewModel(string[] args)
        {
            _args = args;
            RunBenchmarksCommand = new Command(async () => await RunBenchmarks());
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

        private async Task RunBenchmarks(bool headless = false, bool join = true,
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

            await Task.Run(() =>
            {
                return BenchmarkRunner.Run(typeof(PerformanceTests.Program).Assembly, config);
            });

            if (!headless)
            {
                // In this case it is run from the UI
                // Does it make sense to show results on the simulator?
            }
        }

    }
}
