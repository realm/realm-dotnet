// ***********************************************************************
// Copyright (c) 2015 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using NUnit.Runner.Helpers;
using NUnit.Runner.View;

using NUnit.Runner.Services;

using Xamarin.Forms;

namespace NUnit.Runner.ViewModel
{
    class SummaryViewModel : BaseViewModel
    {
        readonly TestPackage _testPackage;
        ResultSummary _results;
        bool _running;
        TestResultProcessor _resultProcessor;

        public SummaryViewModel()
        {
            _testPackage = new TestPackage();
            RunTestsCommand = new Command(async o => await ExecuteTestsAync(), o => !Running);
            ViewAllResultsCommand = new Command(
                async o => await Navigation.PushAsync(new ResultsView(new ResultsViewModel(_results.GetTestResults(), true))),
                o => !HasResults);
            ViewFailedResultsCommand = new Command(
                async o => await Navigation.PushAsync(new ResultsView(new ResultsViewModel(_results.GetTestResults(), false))),
                o => !HasResults);
        }

        private TestOptions options;

        /// <summary>
        /// User options for the test suite.
        /// </summary>
        public TestOptions Options {
            get
            {
                if(options == null)
                {
                    options = new TestOptions();
                }
                return options;
            }
            set
            {
                options = value;
            }
        }
        
        /// <summary>
        /// Called from the view when the view is appearing
        /// </summary>
        public void OnAppearing()
        {
            if(Options.AutoRun)
            {
                // Don't rerun if we navigate back
                Options.AutoRun = false;
                RunTestsCommand.Execute(null);
            }
        }

        /// <summary>
        /// The overall test results
        /// </summary>
        public ResultSummary Results
        {
            get { return _results; }
            set
            {
                if (Equals(value, _results)) return;
                _results = value;
                OnPropertyChanged();
                OnPropertyChanged("HasResults");
            }
        }

        /// <summary>
        /// True if tests are currently running
        /// </summary>
        public bool Running
        {
            get { return _running; }
            set
            {
                if (value.Equals(_running)) return;
                _running = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// True if we have test results to display
        /// </summary>
        public bool HasResults => Results != null;

        public ICommand RunTestsCommand { set; get; }
        public ICommand ViewAllResultsCommand { set; get; }
        public ICommand ViewFailedResultsCommand { set; get; }

        /// <summary>
        /// Adds an assembly to be tested.
        /// </summary>
        /// <param name="testAssembly">The test assembly.</param>
        /// <returns></returns>
        internal void AddTest(Assembly testAssembly)
        {
            _testPackage.AddAssembly(testAssembly);
        }

        async Task ExecuteTestsAync()
        {
            Running = true;
            Results = null;
            TestRunResult results = await _testPackage.ExecuteTests();
            ResultSummary summary = new ResultSummary(results);

            _resultProcessor = TestResultProcessor.BuildChainOfResponsability(Options);
            await _resultProcessor.Process(summary).ConfigureAwait(false);

            Device.BeginInvokeOnMainThread(
                () =>
                    {
                        Results = summary;
                        Running = false;

                    if (Options.TerminateAfterExecution)
                        TerminateWithSuccess();
                });
        }

        public static void TerminateWithSuccess()
        {
#if __IOS__
            var selector = new ObjCRuntime.Selector("terminateWithSuccess");
            UIKit.UIApplication.SharedApplication.PerformSelector(selector, UIKit.UIApplication.SharedApplication, 0);
#elif __DROID__
            System.Environment.Exit(0);
#elif WINDOWS_UWP
            Windows.UI.Xaml.Application.Current.Exit();
#endif
        }
    }
}
