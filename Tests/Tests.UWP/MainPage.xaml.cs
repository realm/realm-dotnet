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
using System.IO;
using NUnit.Runner.Services;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Xaml.Navigation;

namespace Realms.Tests.UWP
{
    public sealed partial class MainPage
    {
        private NUnit.Runner.App? _nunit;

        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            TestHelpers.IsUWP = true;

            var outputWriter = new StreamWriter(Path.Combine(ApplicationData.Current.LocalFolder.Path, "TestRunOutput.txt"));
            TestHelpers.Output = outputWriter;

            try
            {
                _nunit = new NUnit.Runner.App(outputWriter);
                _nunit.AddTestAssembly(typeof(TestHelpers).Assembly);

                _nunit.Options = new TestOptions
                {
                    LogToOutput = true
                };

                if (e.Parameter != null && e.Parameter is string launchParams)
                {
                    var args = TestHelpers.SplitArguments(launchParams);
                    if (TestHelpers.IsHeadlessRun(args))
                    {
                        _nunit.Options.AutoRun = true;
                        _nunit.Options.CreateXmlResultFile = true;

                        var resultPath = TestHelpers.GetResultsPath(args);
                        _nunit.Options.ResultFilePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, resultPath);
                        _nunit.Options.OnCompletedCallback = async () =>
                        {
                            await TestHelpersUWP.TransformTestResults(_nunit.Options.ResultFilePath);
                            outputWriter.WriteLine("Test finished, reporting results");
                            outputWriter.Dispose();
                            CoreApplication.Exit();
                        };
                    }
                }

                LoadApplication(_nunit);
            }
            catch (Exception ex)
            {
                outputWriter.WriteLine($"An error occurred in OnNavigatedTo: {ex}");
                outputWriter.Flush();
                Environment.FailFast("An error occurred in OnNavigatedTo", ex);
            }
        }
    }
}
