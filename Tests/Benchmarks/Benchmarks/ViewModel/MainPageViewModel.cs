using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Xamarin.Forms;

namespace Benchmarks.ViewModel
{
    public class MainPageViewModel : BaseViewModel
    {
        public ICommand RunBenchmarksCommand { get; private set; }

        private string benchmarkResults;

        public string BenchmarkResults
        {
            get => benchmarkResults;
            private set => SetProperty(ref benchmarkResults, value);
        }

        string[] args;

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
                }

                var filterArgumentIndex = Array.IndexOf(args, "-f");
                if (filterArgumentIndex >= 0)
                {
                    //This means that the filter needs to be the last argument
                    filterPatterns = args[(filterArgumentIndex + 1)..args.Length];
                }

                var join = args.Contains("--join");

                await RunBenchmarks(true, join, filterPatterns, artifactPath);

                TerminateApp();

                Console.WriteLine("Application should be closed by now..."); //TODO for testing
            }
        }

        public void TerminateApp()
        {
            Thread.CurrentThread.Abort();
        }

        //What to do
        // - Need to be sure that the application quits after running the tests
        // - Make the IConfig in common between Xamarin and the rest

        async Task RunBenchmarks(bool headless = false, bool join = true,
            string[] filterPatterns = null, string artifactsPath = null)
        {
            var defaultConfig = DefaultConfig.Instance;
            IConfig config = new ManualConfig()
                .AddColumnProvider(defaultConfig.GetColumnProviders().ToArray())
                .AddLogger(defaultConfig.GetLoggers().ToArray())
                .AddAnalyser(defaultConfig.GetAnalysers().ToArray())
                .AddValidator(defaultConfig.GetValidators().ToArray())
                .WithUnionRule(defaultConfig.UnionRule)
                .WithSummaryStyle(defaultConfig.SummaryStyle)
                .AddDiagnoser(MemoryDiagnoser.Default)
                .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.Method, MethodOrderPolicy.Alphabetical))
                .AddExporter(MarkdownExporter.GitHub, JsonExporter.FullCompressed)
                .WithOption(ConfigOptions.JoinSummary, join)
                .WithOption(ConfigOptions.DisableOptimizationsValidator, true);  //TODO this should be removed, it's only so we can run it in debug

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
                var logger = new AccumulationLogger();

                //TODO For testing. We need to decide if it even makes sense to run it from UI
                var summary = benchmarkResults[0];

                MarkdownExporter.Console.ExportToLog(summary, logger);
                ConclusionHelper.Print(logger,
                        summary.BenchmarksCases
                               .SelectMany(benchmark => benchmark.Config.GetCompositeAnalyser().Analyse(summary))
                               .Distinct()
                               .ToList());

                BenchmarkResults = logger.GetLog();
            }
        }

    }
}
