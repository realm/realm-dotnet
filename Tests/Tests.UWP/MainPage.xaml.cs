////////////////////////////////////////////////////////////////////////////
//
// Copyright 2017 Realm Inc.
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

using System.Reflection;
using NUnit.Runner.Services;
using Windows.UI.Xaml.Navigation;
using System.IO;
using Windows.ApplicationModel;

namespace Tests.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var nunit = new NUnit.Runner.App();
            nunit.AddTestAssembly(typeof(MainPage).GetTypeInfo().Assembly);

            var options = new TestOptions
            {
                LogToOutput = true
            };

            if (e.Parameter is string arguments && arguments.Contains("--headless"))
            {
                options.AutoRun = true;
                options.CreateXmlResultFile = true;
                options.TerminateAfterExecution = true;
                options.XmlTransformFile = Path.Combine(Package.Current.InstalledLocation.Path, "nunit3-junit.xslt");
            }

            nunit.Options = options;
            LoadApplication(nunit);
        }
    }
}
