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
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using Realms.Helpers;
#if __ANDROID__
using Application = Android.App.Application;
#endif

namespace Realms.Tests
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

        public static string CopyBundledFileToDocuments(string realmName, string destPath = null)
        {
            var assembly = typeof(TestHelpers).Assembly;
            var resourceName = assembly.GetManifestResourceNames().SingleOrDefault(s => s.EndsWith(realmName));
            if (resourceName == null)
            {
                throw new Exception($"Couldn't find embedded resource '{realmName}' in the RealmTests assembly");
            }

            destPath = RealmConfigurationBase.GetPathToRealm(destPath);  // any relative subdir or filename works

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var destination = new FileStream(destPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                stream.CopyTo(destination);
            }

            return destPath;
        }

        public static bool IsWindows
        {
            get
            {
#if NETCOREAPP || NETFRAMEWORK
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
#elif NETCOREAPP || NETFRAMEWORK
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
#if NETCOREAPP || NETFRAMEWORK
                return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
#else
                return false;
#endif
            }
        }

        public static bool IgnoreOnWindows(string message)
        {
            if (IsWindows)
            {
                Assert.Ignore(message);
                return true;
            }

            return false;
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

            subscribe(handler);

            return tcs.Task;

            void handler(object sender, TEventArgs args)
            {
                unsubscribe(handler);
                tcs.TrySetResult(args);
            }
        }

        public static Task WaitForConditionAsync(Func<bool> testFunc, int retryDelay = 100, int attempts = 100)
        {
            return WaitForConditionAsync(testFunc, b => b, retryDelay, attempts);
        }

        public static async Task<T> WaitForConditionAsync<T>(Func<T> producer, Func<T, bool> tester, int retryDelay = 100, int attempts = 100)
        {
            var value = producer();
            var success = tester(value);
            var timeout = retryDelay * attempts;
            while (!success && attempts > 0)
            {
                await Task.Delay(retryDelay);
                value = producer();
                success = tester(value);
                attempts--;
            }

            if (!success)
            {
                throw new TimeoutException($"Failed to meet condition after {timeout} ms.");
            }

            return value;
        }

        public static void RunAsyncTest(Func<Task> testFunc, int timeout = 30000)
        {
            AsyncContext.Run(() => testFunc().Timeout(timeout));
        }

        public static async Task AssertThrows<T>(Func<Task> function, Action<T> exceptionAsserts = null)
            where T : Exception
        {
            try
            {
                await function().Timeout(5000);
                Assert.Fail($"Exception of type {typeof(T)} expected.");
            }
            catch (T ex)
            {
                exceptionAsserts?.Invoke(ex);
            }
        }
    }
}
