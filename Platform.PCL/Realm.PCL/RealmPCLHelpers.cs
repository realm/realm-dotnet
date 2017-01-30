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
using System.Linq;
using System.Reflection;

namespace Realms
{
    internal static class RealmPCLHelpers
    {
        private static readonly Lazy<bool> _shouldThrow = new Lazy<bool>(() =>
        {
            var getEntryAssembly = typeof(Assembly).GetRuntimeMethods()
                                                   .FirstOrDefault(m => m.Name == "GetEntryAssembly");

            if (getEntryAssembly == null)
            {
                return false;
            }

            var entryAssembly = (Assembly)getEntryAssembly.Invoke(null, null);

            return entryAssembly.GetCustomAttributes<RealmTestingSetup>()
                                .FirstOrDefault()
                                ?.ThrowPCLExceptions != false;
        });

        internal static void ThrowProxyShouldNeverBeUsed()
        {
            if (_shouldThrow.Value)
            {
                throw new PlatformNotSupportedException("The PCL build of Realm is being linked which probably means you need to use NuGet or otherwise link a platform-specific Realm.dll to your main application.");
            }
        }
    }
}