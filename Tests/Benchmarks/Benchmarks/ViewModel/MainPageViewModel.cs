using System;
using System.Windows.Input;
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

        public MainPageViewModel()
        {
            InitCommands();
        }

        void InitCommands()
        {
            RunBenchmarksCommand = new Command(RunBenchmarks);
        }

        void RunBenchmarks()
        {
        }
    }
}
