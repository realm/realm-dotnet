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

using System.Linq;
using System.Threading.Tasks;
using Foundation;
using NUnit.Runner;
using NUnit.Runner.Services;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace Realms.Tests.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {
            Forms.Init();

            TestHelpers.IsAOTTarget = true;

            var nunit = new App();
            nunit.AddTestAssembly(typeof(TestHelpers).Assembly);
            var options = new TestOptions
            {
                LogToOutput = true
            };

            var arguments = NSProcessInfo.ProcessInfo.Arguments
                                         .Select(a => a.Replace("-app-arg=", string.Empty))
                                         .ToArray();

            if (TestHelpers.IsHeadlessRun(arguments))
            {
                options.AutoRun = true;
                options.CreateXmlResultFile = true;
                options.ResultFilePath = TestHelpers.GetResultsPath(arguments);
                options.OnCompletedCallback = () =>
                {
                    TestHelpers.TransformTestResults(options.ResultFilePath);

                    var selector = new ObjCRuntime.Selector("terminateWithSuccess");
                    UIApplication.SharedApplication.PerformSelector(selector, UIApplication.SharedApplication, 0);

                    return Task.CompletedTask;
                };
            }

            nunit.Options = options;
            LoadApplication(nunit);

            return base.FinishedLaunching(uiApplication, launchOptions);
        }
    }
}
