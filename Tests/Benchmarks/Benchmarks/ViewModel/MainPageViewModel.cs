using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Xamarin.Forms;

namespace Benchmarks.ViewModel
{
    public class MainPageViewModel : BaseViewModel
    {
        string[] args;

        private string benchmarkResults;

        public string BenchmarkResults
        {
            get => benchmarkResults;
            private set => SetProperty(ref benchmarkResults, value);
        }

        public ICommand RunBenchmarksCommand { get; private set; }

        public MainPageViewModel(string[] args)
        {
            this.args = args;

            InitCommands();
        }

        void InitCommands()
        {
            RunBenchmarksCommand = new Command(async () => await RunBenchmarks());
        }

        public override async void OnAppearing()
        {
            base.OnAppearing();

            if (args.Contains("--headless"))
            {
                string artifactPath = null;
                string[] filterPatterns = null;

                var artifactArgumentIndex = Array.IndexOf(args, "--artifacts");
                if (artifactArgumentIndex >= 0)
                {
                    artifactPath = args[artifactArgumentIndex + 1];
                    Console.WriteLine("PATTTTTTH    " + artifactPath);
                }

                var filterArgumentIndex = Array.IndexOf(args, "-f");
                if (filterArgumentIndex >= 0)
                {
                    //The filter needs to be the last argument
                    var extractedPatterns = args[(filterArgumentIndex + 1)..args.Length];
                    filterPatterns = extractedPatterns.Contains("\"*\"") ? null : extractedPatterns;
                }

                var join = args.Contains("--join");

                await RunBenchmarks(true, join, filterPatterns, artifactPath);

                TerminateApp();
            }
        }

        public void TerminateApp()
        {
            Thread.CurrentThread.Abort();
        }

        async Task RunBenchmarks(bool headless = false, bool join = true,
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

            Summary[] benchmarkResults = await Task.Run(() =>
            {
                return BenchmarkRunner.Run(typeof(PerformanceTests.Program).Assembly, config);
            });

            if (!headless)
            {
                //In this case it is run from the UI
                //Does it make sense to show results on the simulator?
            }
        }

    }
}
