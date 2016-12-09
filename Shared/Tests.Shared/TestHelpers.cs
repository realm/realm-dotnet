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
using System.IO;
#if __IOS__
using Foundation;
#endif
using Realms;

namespace IntegrationTests
{
    public static class TestHelpers
    {
        public static object GetPropertyValue(object o, string propName)
        {
            return o.GetType().GetProperty(propName).GetValue(o, null);
        }

        public static void SetPropertyValue(object o, string propName, object propertyValue)
        {
            o.GetType().GetProperty(propName).SetValue(o, propertyValue);
        }

        public static T GetPropertyValue<T>(this object obj, string propertyName)
        {
            return (T)GetPropertyValue(obj, propertyName);
        }

        public static void CopyBundledDatabaseToDocuments(string realmName, string destPath = null, bool overwrite = true)
        {
            destPath = RealmConfigurationBase.GetPathToRealm(destPath);  // any relative subdir or filename works

#if __ANDROID__
            using (var asset = Android.App.Application.Context.Assets.Open(realmName))
            using (var destination = File.OpenWrite(destPath))
            {
                asset.CopyTo(destination);
            }

            return;
#endif

            string sourceDir = string.Empty;
#if __IOS__
            sourceDir = NSBundle.MainBundle.BundlePath;
#endif
            // TODO add cases for Windows setting sourcedir for bundled files

            File.Copy(Path.Combine(sourceDir, realmName), destPath, overwrite);
        }
    }
}
