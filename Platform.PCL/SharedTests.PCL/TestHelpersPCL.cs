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
using System.Reflection;
using Realms;

namespace IntegrationTests
{
    // Copy for PCL use only, main TestHelpers.cs is included directly in device builds via TestsDeviceOnly.shared
    public static class TestHelpers
    {
        public static object GetPropertyValue(object o, string propName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        public static void SetPropertyValue(object o, string propName, object propertyValue)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public static T GetPropertyValue<T>(this object obj, string propertyName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return default(T);
        }

        public static void CopyBundledDatabaseToDocuments(string realmName, string destPath = null, bool overwrite = true)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        public static string GetTempFileName()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return string.Empty;
        }

        public static long FileLength(string path)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0;
        }

        public static bool FileExists(string path)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }

        public static IEnumerable<PropertyInfo> GetTypeProperties(Type type)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        public static PropertyInfo GetTypeProperty(object obj, string propName)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        public static MethodInfo GetTypeMethod(object obj, string methName, Type[] types)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        public static string DocumentsFolder()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return string.Empty;
        }

        public static char DirectorySeparatorChar()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return ' ';
        }

        public static void reset_for_testing()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

    }
}
