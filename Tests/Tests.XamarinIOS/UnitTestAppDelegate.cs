////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
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
using NUnit.Runner.Services;
using UIKit;

namespace IntegrationTests.XamarinIOS
{
    [Register("UnitTestAppDelegate")]
    public class UnitTestAppDelegate : Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            global::Xamarin.Forms.Forms.Init();

            // This will load all tests within the current project
            var runner = new NUnit.Runner.App();
            runner.AddTestAssembly(System.Reflection.Assembly.GetExecutingAssembly());

            // Do you want to automatically run tests when the app starts?
            runner.Options = new TestOptions { AutoRun = false };

            LoadApplication(runner);

            return base.FinishedLaunching(application, launchOptions);
        }
    }
}