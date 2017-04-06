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

using Foundation;
using UIKit;
using NUnit.Runner.Services;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms;
using NUnit.Runner;
using System.Linq;
using System.IO;
using System;

namespace Tests.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {
            Forms.Init();

            var nunit = new App();
            var options = new TestOptions
            {
                LogToOutput = true
            };

            if (NSProcessInfo.ProcessInfo.Arguments.Any("--headless".Equals))
            {
                options.AutoRun = true;
                options.CreateXmlResultFile = true;
                options.TerminateAfterExecution = true;
                options.ResultFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TestResults.iOS.xml");
                options.LogToOutput = false;
            }

            nunit.Options = options;
            LoadApplication(nunit);

            return base.FinishedLaunching(uiApplication, launchOptions);
        }
    }
}
