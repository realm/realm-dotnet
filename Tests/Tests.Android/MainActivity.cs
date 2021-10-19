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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using NUnit.Runner;
using NUnit.Runner.Services;
using Realms.Tests.Sync;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using Environment = Android.OS.Environment;

namespace Realms.Tests.Android
{
    [Activity(Label = "Realm Tests", MainLauncher = true)]
    public class MainActivity : FormsApplicationActivity
    {
        public Action<Result> OnFinished { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Forms.Init(this, savedInstanceState);

            var nunit = new App();
            nunit.AddTestAssembly(typeof(TestHelpers).Assembly);
            var options = new TestOptions
            {
                LogToOutput = true,
            };

            var arguments = TestHelpers.SplitArguments(Intent.GetStringExtra("args") ?? string.Empty);
            arguments = SyncTestHelpers.ExtractBaasSettings(arguments);

            if (TestHelpers.IsHeadlessRun(arguments))
            {
                options.AutoRun = true;
                options.CreateXmlResultFile = true;
                options.ResultFilePath = TestHelpers.GetResultsPath(arguments);
                options.OnCompletedCallback = () =>
                {
                    TestHelpers.TransformTestResults(options.ResultFilePath);
                    Console.WriteLine("Activity finished...");
                    OnFinished(Result.Ok);
                    Finish();
                    return Task.CompletedTask;
                };
            }

            nunit.Options = options;

            LoadApplication(nunit);
        }
    }
}
