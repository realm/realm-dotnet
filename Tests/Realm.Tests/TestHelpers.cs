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
using System.Threading;
#if NETCOREAPP || NETFRAMEWORK
using System.Runtime.InteropServices;
#endif
using System.Threading.Tasks;
using MongoDB.Bson;
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

        public static byte[] GetBytes(int size, byte? value = null)
        {
            var result = new byte[size];
            if (value == null)
            {
                Random.NextBytes(result);
            }
            else
            {
                for (var i = 0; i < size; i++)
                {
                    result[i] = value.Value;
                }
            }

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
            destPath = RealmConfigurationBase.GetPathToRealm(destPath);  // any relative subdir or filename works
            return TransformHelpers.CopyBundledFileToDocuments(realmName, destPath);
        }

        public static async Task EnsureObjectsAreCollected(Func<object[]> objectsGetter)
        {
            var references = new Func<WeakReference[]>(() =>
            {
                var objects = objectsGetter();
                var result = new WeakReference[objects.Length];

                for (var i = 0; i < objects.Length; i++)
                {
                    result[i] = new WeakReference(objects[i]);
                }

                return result;
            })();

            await WaitUntilReferencesAreCollected(10000, references);

            Assert.That(references.All(r => !r.IsAlive), "Expected all references to be GC-ed within 10 seconds but they weren't");
        }

        private static async Task WaitUntilReferencesAreCollected(int milliseconds, params WeakReference[] references)
        {
            using var cts = new CancellationTokenSource(milliseconds);

            try
            {
                await Task.Run(async () =>
                {
                    while (references.Any(r => r.IsAlive))
                    {
                        cts.Token.ThrowIfCancellationRequested();

                        await Task.Yield();

                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                });
            }
            catch (OperationCanceledException)
            {
            }
        }

        public static async Task EnsurePreserverKeepsObjectAlive<T>(Func<(T Preserver, WeakReference Reference)> func, Action<(T Preserver, WeakReference Reference)> assertReferenceIsAlive = null)
        {
            WeakReference reference = null;
            WeakReference preserverReference = null;
            await new Func<Task>(async () =>
            {
                T preserver;
                (preserver, reference) = func();
                await WaitUntilReferencesAreCollected(2000, reference);

                Assert.That(reference.IsAlive, "Preserver hasn't been disposed so expected object to still be alive.");

                assertReferenceIsAlive?.Invoke((preserver, reference));

                if (preserver is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                preserverReference = new WeakReference(preserver);
                preserver = default;
            })();

            await WaitUntilReferencesAreCollected(10000, reference, preserverReference);

            Assert.That(preserverReference.IsAlive, Is.False, "Expected the preserver instance to be GC-ed but it wasn't.");
            Assert.That(reference.IsAlive, Is.False, "Expected object to be GC-ed but it wasn't.");
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

        public static bool IsAOTTarget
        {
            get
            {
#if __IOS__
                return true;
#else
                return false;
#endif
            }
        }

        public static void IgnoreOnAOT(string message)
        {
            if (IsAOTTarget)
            {
                Assert.Ignore(message);
            }
        }

        public static ObjectId GenerateRepetitiveObjectId(byte value) => new ObjectId(Enumerable.Range(0, 12).Select(_ => value).ToArray());

        public static RealmInteger<T>[] ToInteger<T>(this T[] values)
            where T : struct, IComparable<T>, IFormattable, IConvertible, IEquatable<T>
        {
            return values?.Select(v => new RealmInteger<T>(v)).ToArray();
        }

        public static RealmInteger<T>?[] ToInteger<T>(this T?[] values)
            where T : struct, IComparable<T>, IFormattable, IConvertible, IEquatable<T>
        {
            return values?.Select(v => v == null ? (RealmInteger<T>?)null : new RealmInteger<T>(v.Value)).ToArray();
        }

        public static (TKey, RealmInteger<TValue>)[] ToIntegerTuple<TKey, TValue>(this (TKey, TValue)[] values)
            where TValue : struct, IComparable<TValue>, IFormattable, IConvertible, IEquatable<TValue>
        {
            return values?.Select(kvp => (kvp.Item1, new RealmInteger<TValue>(kvp.Item2))).ToArray();
        }

        public static (TKey, RealmInteger<TValue>?)[] ToIntegerTuple<TKey, TValue>(this (TKey, TValue?)[] values)
            where TValue : struct, IComparable<TValue>, IFormattable, IConvertible, IEquatable<TValue>
        {
            return values?.Select(kvp => (kvp.Item1, kvp.Item2 == null ? (RealmInteger<TValue>?)null : new RealmInteger<TValue>(kvp.Item2.Value))).ToArray();
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

        public static void RunAsyncTest(Func<Task> testFunc, int timeout = 30000, Task errorTask = null)
        {
            AsyncContext.Run(async () =>
            {
                await (errorTask == null ? testFunc() : Task.WhenAny(testFunc(), errorTask)).Timeout(timeout);
            });
        }

        public static async Task<T> AssertThrows<T>(Func<Task> function)
            where T : Exception
        {
            try
            {
                await function().Timeout(5000);
            }
            catch (T ex)
            {
                return ex;
            }

            Assert.Fail($"Exception of type {typeof(T)} expected.");
            return null;
        }

        public static void TransformTestResults(string resultPath)
        {
            var transformFile = CopyBundledFileToDocuments("nunit3-junit.xslt", "nunit3-junit.xslt");
            TransformHelpers.TransformTestResults(resultPath, transformFile);
        }

        public static string ByteArrayToTestDescription<T>(T arr)
        {
            var byteArr = (byte[])(object)arr;

            if (byteArr == null)
            {
                return "<null>";
            }

            if (byteArr.Length == 0)
            {
                return "<empty>";
            }

            return $"<{byteArr[0]}>";
        }

        public static IDisposable Subscribe<T>(this IObservable<T> observable, Action<T> onNext)
        {
            var observer = new FunctionObserver<T>(onNext);
            return observable.Subscribe(observer);
        }

        private class FunctionObserver<T> : IObserver<T>
        {
            private readonly Action<T> _onNext;

            public FunctionObserver(Action<T> onNext)
            {
                _onNext = onNext;
            }

            public void OnCompleted()
            {
            }

            public void OnError(Exception error)
            {
            }

            public void OnNext(T value)
            {
                _onNext?.Invoke(value);
            }
        }
    }
}
