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
using System.Diagnostics;
using System.IO;
using NUnit.Runner.Services;
using Windows.Storage;
using Windows.UI.Xaml.Navigation;

namespace Realms.Tests.UWP
{
    public sealed partial class MainPage
    {
        private NUnit.Runner.App _nunit;
        public MainPage()
        {
            InitializeComponent();

            _nunit = new NUnit.Runner.App();
            _nunit.AddTestAssembly(typeof(TestHelpers).Assembly);

            _nunit.Options = new TestOptions()
            {
                LogToOutput = true
            };
        }

        // TODO check a workaround to avoid "async void"
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            #region TO REMOVE BECAUSE OF TESTING
            //_nunit.Options.AutoRun = true;
            //_nunit.Options.CreateXmlResultFile = true;
            //_nunit.Options.ResultFilePath = ".\\UwpTestsResults.xml";//Path.Combine("C:\\Users\\andre\\Development\\realm-dotnet", "UwpTestsResults.xml");//Path.Combine(storageFolder.Path, "UwpTestsResults.xml");//xmlResultFile.Path;//GetResultPath(launchParams);
            //Debug.WriteLine($"Path = {_nunit.Options.ResultFilePath}");
            //_nunit.Options.OnCompletedCallback = () =>
            //{
            //    if (!string.IsNullOrEmpty(_nunit.Options.ResultFilePath))
            //    {
            //        TestHelpers.TransformTestResults(_nunit.Options.ResultFilePath);
            //    }
            //    Console.WriteLine("Test finished, reporting results");
            //    return Task.CompletedTask;
            //};
            #endregion

            if (e.Parameter != null && e.Parameter is string launchParams)
            {
                if (launchParams.Contains("--headless"))
                {
                    _nunit.Options.AutoRun = true;
                    _nunit.Options.CreateXmlResultFile = true;
                    var extractedPath = TestHelpersUWP.GetResultPath(launchParams);
                    if (extractedPath == string.Empty)
                    {
                        extractedPath = "UWP_results.xml";
                        Debug.WriteLine($"No \"--result\" property was found in the cmd args, so \"{extractedPath}\" filename was used");
                    }

                    _nunit.Options.ResultFilePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, extractedPath);
                    _nunit.Options.OnCompletedCallback = async () =>
                    {
                        await TestHelpersUWP.TransformTestResults(_nunit.Options.ResultFilePath);
                        Console.WriteLine("Test finished, reporting results");
                    };
                }
            }

            LoadApplication(_nunit);

            base.OnNavigatedTo(e);
        }
    }
}
