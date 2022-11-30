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
using Android.Runtime;
using Realms.Tests;

namespace Tests.Maui;

[Application]
public class MainApplication : MauiApplication
{
    internal string[] Args { get; set; } = Array.Empty<string>();

    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    protected override MauiApp CreateMauiApp()
    {
        TestHelpers.TestHttpHandlerFactory = () => new Xamarin.Android.Net.AndroidMessageHandler();
        return MauiProgram.CreateMauiApp(Args);
    }
}
