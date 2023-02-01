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

            _ = RunTests();
        }

        private async Task RunTests()
        {
            try
            {
                StateField.StringValue = "Running tests...";

                await Task.Delay(50);

                var result = new AutoRun(typeof(TestHelpers).Assembly).Execute(MainClass.Args.Where(a => a != "--headless").ToArray());

                StateField.StringValue = $"Test run complete. Failed: {result}";

                if (TestHelpers.IsHeadlessRun(MainClass.Args))
                {
                    var resultPath = TestHelpers.GetResultsPath(MainClass.Args);
                    TestHelpers.TransformTestResults(resultPath);
                    NSApplication.SharedApplication.Terminate(this);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while running the tests: {ex}");
                NSApplication.SharedApplication.Terminate(this);
            }
        }
    }
}
