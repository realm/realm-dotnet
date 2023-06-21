////////////////////////////////////////////////////////////////////////////
//
// Copyright 2023 Realm Inc.
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
using System.Linq;
using Foundation;
using UIKit;

namespace Realms.Tests.XamarinTVOS
{
    public class Application
    {
        public static string[] Args { get; private set; } = Array.Empty<string>();

        static void Main(string[] args)
        {
            if (!args.Any())
            {
                // First argument is the executable when launched from the command line.
                // For some reason, the command line arguments don't show up as args, so
                // we need to extract them from NSProcessInfo.
                args = NSProcessInfo.ProcessInfo.Arguments
                                    .Skip(1)
                                    .Select(a => a.Replace("-app-arg=", string.Empty))
                                    .ToArray();
            }

            Args = Sync.SyncTestHelpers.ExtractBaasSettings(args);
            UIApplication.Main(args, null, typeof(AppDelegate));
        }
    }
}

