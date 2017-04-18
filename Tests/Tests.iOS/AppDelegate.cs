﻿////////////////////////////////////////////////////////////////////////////
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
using Foundation;
using NUnit.Runner;
using NUnit.Runner.Services;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace Tests
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

            var arguments = NSProcessInfo.ProcessInfo.Arguments;
            if (arguments.Any("--headless".Equals))
            {
                options.AutoRun = true;
                options.CreateXmlResultFile = true;
                options.TerminateAfterExecution = true;

                var hasResultsPath = false;
                for (var i = 0; i < arguments.Length; i++)
                {
                    if (arguments[i] == "--resultpath")
                    {
                        options.ResultFilePath = arguments[i + 1];
                        hasResultsPath = true;
                        break;
                    }
                }

                if (!hasResultsPath)
                {
                    throw new Exception("You must provide path to store test results with --resultpath path/to/results.xml");
                }

                options.XmlTransformFile = "nunit3-junit.xslt";
            }

            nunit.Options = options;
            LoadApplication(nunit);

            return base.FinishedLaunching(uiApplication, launchOptions);
        }
    }
}
