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

using System;
using System.Linq;
using System.Threading.Tasks;
using AppKit;
using NUnitLite;

namespace Realms.Tests.XamarinMac
{
    public partial class ViewController : NSViewController
    {
        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidAppear()
        {
            base.ViewDidAppear();

            RunTests();
        }

        private async Task RunTests()
        {
            StateField.StringValue = "Running tests...";

            await Task.Delay(50);

            var result = new AutoRun(typeof(TestHelpers).Assembly).Execute(MainClass.NUnitArgs);

            StateField.StringValue = $"Test run complete. Failed: {result}";

            if (MainClass.Headless)
            {
                var resultPath = MainClass.NUnitArgs.FirstOrDefault(a => a.StartsWith("--result="))?.Replace("--result=", "");
                if (!string.IsNullOrEmpty(resultPath))
                {
                    TestHelpers.TransformTestResults(resultPath);
                }

                NSApplication.SharedApplication.Terminate(this);
            }
        }
    }
}