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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework.Interfaces;

namespace NUnit.Runner.ViewModel
{
    class ResultsViewModel : BaseViewModel
    {
        /// <summary>
        /// Constructs the view model
        /// </summary>
        /// <param name="results">The package of all results in run</param>
        /// <param name="viewAll">If true, views all tests, otherwise only shows those
        /// that did not pass</param>
        public ResultsViewModel(IReadOnlyCollection<ITestResult> results, bool viewAll)
        {
            Results = new ObservableCollection<ResultViewModel>();
            foreach (var result in results)
                AddTestResults(result, viewAll);
        }

        /// <summary>
        /// A list of tests that did not pass
        /// </summary>
        public ObservableCollection<ResultViewModel> Results { get; private set; }

        /// <summary>
        /// Add all tests that did not pass to the Results collection
        /// </summary>
        /// <param name="result"></param>
        /// <param name="viewAll"></param>
        private void AddTestResults(ITestResult result, bool viewAll)
        {
            if (result.Test.IsSuite)
            {
                foreach (var childResult in result.Children) 
                    AddTestResults(childResult, viewAll);
            }
            else if (viewAll || result.ResultState.Status != TestStatus.Passed)
            {
                Results.Add(new ResultViewModel(result));
            }
        }
    }
}
