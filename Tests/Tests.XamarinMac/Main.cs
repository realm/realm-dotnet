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

using System.Linq;
using AppKit;

namespace Realms.Tests.XamarinMac
{
    internal static class MainClass
    {
        private const string HeadlessArg = "--headless";

        public static string[] NUnitArgs { get; private set; }

        public static bool Headless { get; private set; }

        public static void Main(string[] args)
        {
            Headless = args.Contains(HeadlessArg);
            NUnitArgs = args.Where(a => a != HeadlessArg).ToArray();
            NSApplication.Init();
            NSApplication.Main(args);
        }
    }
}
