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

using System;
using System.IO;
using Microsoft.UI.Xaml;

namespace Tests.Maui.WinUI;

public partial class App : MauiWinUIApplication
{
    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        File.AppendAllText(@"D:\a\realm-dotnet\realm-dotnet\args.txt", args.Arguments);

        base.OnLaunched(args);

        // Try to attach to a parent process's console, for logging
        PInvoke.Kernel32.AttachConsole(-1);
    }

    protected override MauiApp CreateMauiApp()
    {
        var args = Environment.GetCommandLineArgs().Skip(1).ToArray();

        return MauiProgram.CreateMauiApp(args);
    }
}

