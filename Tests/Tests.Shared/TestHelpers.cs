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
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NUnit.Framework;
using Realms;
using Realms.Helpers;
#if __ANDROID__
using Application = Android.App.Application;
#endif

namespace Tests
{
    public static class TestHelpers
    {
        public static readonly Random Random = new Random();

        public static byte[] GetBytes(int size)
        {
            var result = new byte[size];
            Random.NextBytes(result);
            return result;
        }

        public static byte[] GetEncryptionKey(params byte[] bytes)
        {
            var result = new byte[64];
            for (var i = 0; i < bytes.Length; i++)
            {
                result[i] = bytes[i];
            }

            return result;
        }

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

        public static string CopyBundledDatabaseToDocuments(string realmName, string destPath = null, bool overwrite = true)
        {
            destPath = RealmConfigurationBase.GetPathToRealm(destPath);  // any relative subdir or filename works

#if __ANDROID__
            using (var asset = Application.Context.Assets.Open(realmName))
            using (var destination = File.OpenWrite(destPath))
            {
                asset.CopyTo(destination);
            }
#else
#if __IOS__
            var sourceDir = Foundation.NSBundle.MainBundle.BundlePath;
#elif WINDOWS_UWP
            var sourceDir = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
#elif __MACOS__
            var sourceDir = Foundation.NSBundle.MainBundle.ResourcePath;
#else
            var sourceDir = NUnit.Framework.TestContext.CurrentContext.TestDirectory;
#endif

            File.Copy(Path.Combine(sourceDir, realmName), destPath, overwrite);
#endif

            return destPath;
        }

        public static bool IsWindows
        {
            get
            {
#if WINDOWS
                return true;
#elif NETCOREAPP1_1
                return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#else
                return false;
#endif
            }
        }

        public static bool IsMacOS
        {
            get
            {
#if __MACOS__
                return true;
#elif NETCOREAPP1_1
                return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
#else
                return false;
#endif
            }
        }

        public static bool IsLinux
        {
            get
            {
#if NETCOREAPP1_1
                return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
#else
                return false;
#endif
            }
        }

        public static void IgnoreOnWindows(string message)
        {
            if (IsWindows)
            {
                Assert.Ignore(message);
            }
        }

        public static RealmInteger<T>[] ToInteger<T>(this T[] values)
            where T : struct, IComparable<T>, IFormattable
        {
            return values?.Select(v => new RealmInteger<T>(v)).ToArray();
        }

        public static RealmInteger<T>?[] ToInteger<T>(this T?[] values)
            where T : struct, IComparable<T>, IFormattable
        {
            return values?.Select(v => v == null ? (RealmInteger<T>?)null : new RealmInteger<T>(v.Value)).ToArray();
        }

        public static Task<TEventArgs> EventToTask<TEventArgs>(Action<EventHandler<TEventArgs>> subscribe, Action<EventHandler<TEventArgs>> unsubscribe)
        {
            Argument.NotNull(subscribe, nameof(subscribe));
            Argument.NotNull(unsubscribe, nameof(unsubscribe));

            var tcs = new TaskCompletionSource<TEventArgs>();
            EventHandler<TEventArgs> handler = null;
            handler = (sender, args) =>
            {
                unsubscribe(handler);
                tcs.TrySetResult(args);
            };
            subscribe(handler);

            return tcs.Task;
        }
    }
}
