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
using System.IO;
using Android.App;
using Android.OS;
using NUnit.Runner;
using NUnit.Runner.Services;
using Realms;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace Tests.Android
{
    [Activity(Label = Constants.ActivityLabel, MainLauncher = true)]
    public class MainActivity : FormsApplicationActivity
    {
        public Action<Result> OnFinished { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Forms.Init(this, savedInstanceState);

            var nunit = new App();

            var options = new TestOptions
            {
                LogToOutput = true,
            };

            if (Intent.GetBooleanExtra("headless", false))
            {
                TestHelpers.CopyBundledDatabaseToDocuments("nunit3-junit.xslt", "nunit3-junit.xslt");
                var transformPath = RealmConfigurationBase.GetPathToRealm("nunit3-junit.xslt");
                options.XmlTransformFile = transformPath;
                options.AutoRun = true;
                options.CreateXmlResultFile = true;
                options.OnCompletedCallback = () => 
                {
                    Console.WriteLine("Activity finished...");
                    OnFinished(Result.Ok);
                    Finish();
                };
                options.ResultFilePath = Intent.GetStringExtra("resultPath");
            }

            nunit.Options = options;

            LoadApplication(nunit);
        }
    }
}