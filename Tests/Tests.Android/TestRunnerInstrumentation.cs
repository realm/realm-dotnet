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
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;

namespace Realms.Tests.Android
{
    [Instrumentation(Name = "io.realm.xamarintests.TestRunner")]
    public class TestRunnerInstrumentation : Instrumentation
    {
        private List<string> _args = new()
        {
            "--headless",
            "--result=/storage/emulated/0/Documents/TestResults.Android.xml"
        };

        public TestRunnerInstrumentation(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public override void OnCreate(Bundle arguments)
        {
            base.OnCreate(arguments);

            var args = arguments.GetString("args");
            if (args != null)
            {
                _args.AddRange(TestHelpers.SplitArguments(args));
            }

            Start();
        }

        public override void OnStart()
        {
            var intent = new Intent(Context!, typeof(MainActivity));
            intent.PutExtra("args", _args.ToArray());
            intent.SetFlags(ActivityFlags.NewTask);
            var activity = (MainActivity)StartActivitySync(intent);
            activity.OnFinished = result =>
            {
                Console.WriteLine("Instrumentation finished...");
                Finish(result, null);
            };
        }
    }
}
