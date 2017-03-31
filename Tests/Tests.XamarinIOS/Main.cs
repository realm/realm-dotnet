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

using System;
using System.IO;
using System.Linq;

using Foundation;
using UIKit;

namespace IntegrationTests.XamarinIOS
{
    public class Application
    {
        // This is the main entry point of the application.
        private static void Main(string[] args)
        {
            if (NSProcessInfo.ProcessInfo.Arguments.Any("--headless".Equals))
            {
                using (var output = File.OpenWrite(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TestResults.iOS.xml")))
                {
                    IntegrationTests.TestRunner.Run("iOS", output);
                }

                return;
            }

            // if you want to use a different Application Delegate class from "UnitTestAppDelegate"
            // you can specify it here.
            UIApplication.Main(args, null, "UnitTestAppDelegate");
        }
    }
}
