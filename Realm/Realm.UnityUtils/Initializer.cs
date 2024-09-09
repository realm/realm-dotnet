////////////////////////////////////////////////////////////////////////////
//
// Copyright 2021 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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
using System.Threading;
using Realms;
using UnityEngine;

namespace UnityUtils
{
    [Obfuscation(ApplyToMembers = true, Exclude = true)]
    internal static class Initializer
    {
        private static int _isInitialized;

        [Preserve]
        public static void Initialize()
        {
            if (Interlocked.CompareExchange(ref _isInitialized, 1, 0) == 0)
            {
                InteropConfig.AddPotentialStorageFolder(FileHelper.GetStorageFolder());
                Realms.Logging.RealmLogger.Console = new UnityLogger();
                Application.quitting += () =>
                {
                    NativeCommon.CleanupNativeResources("Application is exiting");
                };
            }
        }

        [Preserve]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void OnSubsystemRegistration()
        {
            NativeCommon.Initialize();
        }
    }
}
