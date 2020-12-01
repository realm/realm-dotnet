using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using PerformanceTests;
using Xamarin.Forms;

namespace Benchmarks
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        async void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {
                var logger = new AccumulationLogger();
                await Task.Run(() =>
                {
                    var summary = BenchmarkRunner.Run<QueryTests>();
                    MarkdownExporter.Console.ExportToLog(summary, logger);
                    ConclusionHelper.Print(logger,
                            summary.BenchmarksCases
                                   .SelectMany(benchmark => benchmark.Config.GetCompositeAnalyser().Analyse(summary))
                                   .Distinct()
                                   .ToList());
                });
                SetSummary(logger.GetLog());
            }
            catch (Exception exc)
            {
                await DisplayAlert("Error", exc.Message, "Ok");
            }

            void SetSummary(string text)
            {
                benchmarkLabel.Text = text;
                var size = benchmarkLabel.Measure(double.MaxValue, double.MaxValue).Request;
                benchmarkLabel.WidthRequest = size.Width;
                benchmarkLabel.HeightRequest = size.Height;
            }
        }
    }
}
