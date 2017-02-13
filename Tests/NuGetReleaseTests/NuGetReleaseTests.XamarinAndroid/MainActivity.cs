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

using System.Reflection;

using Android.App;
using Android.OS;
using Xamarin.Android.NUnitLite;

namespace Tests.XamarinAndroid
{
    [Activity (Label = "NuGetReleaseTests.XamarinAndroid", MainLauncher = true)]
    public class MainActivity : TestSuiteActivity
    {
        protected override void OnCreate (Bundle bundle)
        {
            // tests can be inside the main assembly
            AddTest (Assembly.GetExecutingAssembly ());
            // or in any reference assemblies
            // AddTest (typeof (Your.Library.TestClass).Assembly);

            // Once you called base.OnCreate(), you cannot add more assemblies.
            base.OnCreate (bundle);
        }
    }
}

