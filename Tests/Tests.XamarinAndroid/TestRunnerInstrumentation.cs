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

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;

namespace IntegrationTests.XamarinAndroid
{
    [Instrumentation(Name = "io.realm.xamarintests.TestRunner")]
    public class TestRunnerInstrumentation : Instrumentation
    {
        public TestRunnerInstrumentation(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public override void OnCreate(Bundle arguments)
        {
            base.OnCreate(arguments);

            this.Start();
        }

        public override void OnStart()
        {
            NativeMethods.ALooper_prepare(0);

            using (var output = Context.OpenFileOutput("TestResults.Android.xml", FileCreationMode.WorldReadable))
            {
                IntegrationTests.TestRunner.Run("Android", output);
            }

            this.Finish(Result.Ok, null);
        }

        private static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("android")]
            internal static extern IntPtr ALooper_prepare(int opts);
        }
    }
}