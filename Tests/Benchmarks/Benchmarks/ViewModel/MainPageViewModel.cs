using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.ConsoleArguments;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using PerformanceTests;
using Xamarin.Forms;

namespace Benchmarks.ViewModel
{
    public class MainPageViewModel : BaseViewModel
    {
        private string benchmarkResults;

        public ICommand RunBenchmarksCommand { get; private set; }

        public string BenchmarkResults
        {
            get => benchmarkResults;
            private set => SetProperty(ref benchmarkResults, value);
        }

        private string[] args;

        public MainPageViewModel(string[] args)
        {
            this.args = args;

            InitCommands();

            Console.WriteLine("CONSTRUCTOR " + string.Join(" ", args));

            if (args.Contains("--headless"))
            {
                RunBenchmarksCommand.Execute(null);
                App.Current.Quit();
            }

        }

        void InitCommands()
        {
            RunBenchmarksCommand = new Command(async () => await RunBenchmarks());
        }

        async Task RunBenchmarks()
        {
            var path = "/Users/ferdinando.papale/MongoDB/realm-dotnet/Tests/Benchmarks/Benchmarks.iOS";
            var defaultConfig = DefaultConfig.Instance;
            IConfig config = new ManualConfig()
                .AddColumnProvider(defaultConfig.GetColumnProviders().ToArray())
                .AddLogger(defaultConfig.GetLoggers().ToArray())
                .AddAnalyser(defaultConfig.GetAnalysers().ToArray())
                .AddValidator(defaultConfig.GetValidators().ToArray())
                .WithUnionRule(defaultConfig.UnionRule)
                .WithSummaryStyle(defaultConfig.SummaryStyle)
                .WithArtifactsPath(path)
                .AddDiagnoser(MemoryDiagnoser.Default)
                //.AddJob(Job.Default.WithToolchain(InProcessEmitToolchain.Instance))  //TODO FP this uses reflection, and so it can't be used on iOS
                .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.Method, MethodOrderPolicy.Alphabetical))
                .AddExporter(MarkdownExporter.GitHub, JsonExporter.FullCompressed)
                .WithOption(ConfigOptions.JoinSummary, true)
                .WithOption(ConfigOptions.DisableOptimizationsValidator, true);

            var logger = new AccumulationLogger();
            await Task.Run(() =>
            {
                var summaries = BenchmarkRunner.Run<QueryTests>(config);
                var summary = summaries;

                MarkdownExporter.Console.ExportToLog(summary, logger);
                ConclusionHelper.Print(logger,
                        summary.BenchmarksCases
                               .SelectMany(benchmark => benchmark.Config.GetCompositeAnalyser().Analyse(summary))
                               .Distinct()
                               .ToList());
            });

            BenchmarkResults = logger.GetLog();
        }

    }
}
