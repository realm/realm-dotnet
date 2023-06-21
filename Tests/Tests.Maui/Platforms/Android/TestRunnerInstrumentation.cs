////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
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

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;

namespace Tests.Maui.Platforms.Android
{
    [Instrumentation(Name = "io.realm.mauitests.TestRunner")]
    public class TestRunnerInstrumentation : Instrumentation
    {
        private List<string> _args = new() {
            "--headless",
            "--result=/storage/emulated/0/Documents/TestResults.Android.xml"
        };

        public TestRunnerInstrumentation(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public override void OnCreate(Bundle? arguments)
        {
            base.OnCreate(arguments);

            var args = arguments?.GetString("args");
            if (args != null)
            {
                _args.AddRange(Realms.Tests.TestHelpers.SplitArguments(args));
            }

            Start();
        }

        public override void CallApplicationOnCreate(global::Android.App.Application? app)
        {
            ((MainApplication)app!).Args = _args.ToArray();
            base.CallApplicationOnCreate(app);
        }

        public override void OnStart()
        {
            var intent = new Intent(Context!, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.NewTask);
            StartActivitySync(intent);
        }
    }
}

