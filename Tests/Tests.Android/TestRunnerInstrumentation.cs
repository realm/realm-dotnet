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
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;

using Environment = Android.OS.Environment;

namespace Tests.Android
{
    [Instrumentation(Name = Constants.PackageName + ".TestRunner")]
    public class TestRunnerInstrumentation : Instrumentation
    {
        public TestRunnerInstrumentation(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public override void OnCreate(Bundle arguments)
        {
            base.OnCreate(arguments);

            Start();
        }

        public override void OnStart()
        {
            var resultPath = Path.Combine(Environment.ExternalStorageDirectory.AbsolutePath, "RealmTests", "TestResults.Android.xml");
            Console.WriteLine($"Test Result file: {resultPath}");
            var intent = new Intent(Context, typeof(MainActivity));
            intent.PutExtra("headless", true);
            intent.SetFlags(ActivityFlags.NewTask);
            intent.PutExtra("resultPath", resultPath);
            var activity = (MainActivity)StartActivitySync(intent);
            activity.OnFinished = result =>
            {
                Console.WriteLine("Instrumentation finished...");
                Finish(result, null);
            };
        }
    }
}