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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using Nito.AsyncEx;
using NUnit.Framework;
using Realms.Helpers;
using Realms.Schema;

namespace Realms.Tests
{
    public static class TestHelpers
    {
        public static readonly Random Random = new();

        public static TextWriter Output { get; set; } = Console.Out;

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

        public static object? GetPropertyValue(object o, string propName)
        {
            return o.GetType().GetProperty(propName)!.GetValue(o, null);
        }

        public static void SetPropertyValue(object o, string propName, object? propertyValue)
        {
            o.GetType().GetProperty(propName)!.SetValue(o, propertyValue);
        }

        public static string CopyBundledFileToDocuments(string realmName, string? destPath = null)
        {
            destPath = RealmConfigurationBase.GetPathToRealm(destPath);  // any relative subdir or filename works
            TransformHelpers.ExtractBundledFile(realmName, destPath);
            return destPath;
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

        public static async Task WaitUntilReferencesAreCollected(int milliseconds, params WeakReference[] references)
        {
            IgnoreOnUnity("Waiting on GC seems to lock up on Unity on Linux.", OSPlatform.Linux);

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

        public static async Task EnsurePreserverKeepsObjectAlive<T>(
            Func<(T Preserver, WeakReference Reference)> func,
            Action<(T Preserver, WeakReference Reference)>? assertReferenceIsAlive = null)
        {
            WeakReference reference = null!;
            WeakReference preserverReference = null!;
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
                preserver = default!;
            })();

            await WaitUntilReferencesAreCollected(10000, reference, preserverReference);

            Assert.That(preserverReference.IsAlive, Is.False, "Expected the preserver instance to be GC-ed but it wasn't.");
            Assert.That(reference.IsAlive, Is.False, "Expected object to be GC-ed but it wasn't.");
        }

        public static bool IsAOTTarget;

        public static void IgnoreOnAOT(string message)
        {
            if (IsAOTTarget)
            {
                Assert.Ignore(message);
            }
        }

        public static bool IsUWP { get; set; }

        public static bool IsUnity
        {
            get
            {
#if UNITY
                return true;
#else
                return false;
#endif
            }
        }

        [System.Diagnostics.Conditional("UNITY")]
        public static void IgnoreOnUnity(string message = "dynamic is not supported on Unity", OSPlatform? platform = null)
        {
            if (platform == null || RuntimeInformation.IsOSPlatform(platform.Value))
            {
                Assert.Ignore(message);
            }
        }

        public static Func<HttpMessageHandler> TestHttpHandlerFactory = () => new HttpClientHandler();

        private static readonly decimal _decimalValue = 1.23456789M;

        public static readonly Action _preserveAction;

        static TestHelpers()
        {
            // Preserve the >= and <= operators on System.decimal as IL2CPP will strip them otherwise.
            _ = decimal.MaxValue >= _decimalValue;
            _ = decimal.MinValue <= _decimalValue;

            _preserveAction = () =>
            {
                // Preserve all the realm.Find<T> overloads
                using var r = Realm.GetInstance(Guid.NewGuid().ToString());
                _ = r.Find<PrimaryKeyStringObject>(string.Empty);
                _ = r.Find<PrimaryKeyObjectIdObject>(ObjectId.GenerateNewId());
                _ = r.Find<PrimaryKeyGuidObject>(Guid.NewGuid());
                _ = r.Find<PrimaryKeyInt64Object>(123L);
            };
        }

        public static ObjectId GenerateRepetitiveObjectId(byte value) => new(Enumerable.Range(0, 12).Select(_ => value).ToArray());

        public static (TKey, RealmInteger<TValue>)[] ToIntegerTuple<TKey, TValue>(this (TKey, TValue)[] values)
            where TValue : struct, IComparable<TValue>, IFormattable, IConvertible, IEquatable<TValue>
        {
            return values.Select(kvp => (kvp.Item1, new RealmInteger<TValue>(kvp.Item2))).ToArray();
        }

        public static (TKey, RealmInteger<TValue>?)[] ToIntegerTuple<TKey, TValue>(this (TKey, TValue?)[] values)
            where TValue : struct, IComparable<TValue>, IFormattable, IConvertible, IEquatable<TValue>
        {
            return values.Select(kvp => (kvp.Item1, kvp.Item2 == null ? (RealmInteger<TValue>?)null : new RealmInteger<TValue>(kvp.Item2.Value))).ToArray();
        }

        public static Task WaitForConditionAsync(Func<bool> testFunc, int retryDelay = 100, int attempts = 100, string? errorMessage = null)
        {
            return WaitForConditionAsync(testFunc, b => b, retryDelay, attempts, errorMessage);
        }

        public static async Task WaitForEventAsync<T>(this IEnumerable<T> collection, Func<IRealmCollection<T>, ChangeSet?, bool> testFunc)
        {
            var tcs = new TaskCompletionSource();
            if (collection is not IRealmCollection<T> realmCollection)
            {
                throw new NotSupportedException();
            }

            using var token = realmCollection.SubscribeForNotifications((sender, changes) =>
            {
                if (testFunc(sender, changes))
                {
                    tcs.TrySetResult();
                }
            });

            await tcs.Task;
        }

        public static Task<T> WaitForConditionAsync<T>(Func<T> producer, Func<T, bool> tester, int retryDelay = 100, int attempts = 100, string? errorMessage = null)
            => WaitForConditionAsync<T>(() => Task.FromResult(producer()), item => Task.FromResult(tester(item)), retryDelay, attempts, errorMessage);

        public static async Task<T> WaitForConditionAsync<T>(Func<Task<T>> producer, Func<T, Task<bool>> tester, int retryDelay = 100, int attempts = 100, string? errorMessage = null)
        {
            var value = await producer();
            var success = await tester(value);
            var timeout = retryDelay * attempts;
            while (!success && attempts > 0)
            {
                await Task.Delay(retryDelay);
                value = await producer();
                success = await tester(value);
                attempts--;
            }

            if (!success)
            {
                var message = $"Failed to meet condition after {timeout} ms" + (errorMessage == null ? "." : $": {errorMessage}");
                throw new TimeoutException(message);
            }

            return value;
        }

        public static void RunAsyncTest(Func<Task> testFunc, int timeout = 30000, Task? errorTask = null)
        {
            AsyncContext.Run(async () =>
            {
                await testFunc().Timeout(timeout, errorTask);
            });
        }

        public static async Task<T> AssertThrows<T>(Func<Task> function, int timeout = 5000)
            where T : Exception
        {
            try
            {
                await function().Timeout(timeout);
            }
            catch (T ex)
            {
                return ex;
            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception of type {typeof(T)} expected. Got {ex.GetType()}: {ex}");
            }

            Assert.Fail($"Exception of type {typeof(T)} expected but method didn't throw.");
            return null!;
        }

        public static void TransformTestResults(string resultPath)
        {
            var transformFile = CopyBundledFileToDocuments("nunit3-junit.xslt", $"{Guid.NewGuid()}.xslt");
            TransformHelpers.TransformTestResults(resultPath, transformFile);
        }

        public static string ByteArrayToTestDescription<T>(T arr)
        {
            var byteArr = (byte[]?)(object?)arr;

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

        public static void DrainQueue<T>(this ConcurrentQueue<StrongBox<T>> queue, Action<T> action)
        {
            while (queue.TryDequeue(out var result))
            {
                action(result.Value!);
                result.Value = default;
            }
        }

        public static void DrainQueueAsync<T>(this ConcurrentQueue<StrongBox<T>> queue, Func<T, Task> action)
        {
            AsyncContext.Run(async () =>
            {
                await Task.Run(async () =>
                {
                    while (queue.TryDequeue(out var result))
                    {
                        await action(result.Value!);
                        result.Value = default;
                    }
                }).Timeout(20_000, detail: $"Failed to drain queue: {queue.GetType().Name}");
            });
        }

        public static IDisposable Subscribe<T>(this IObservable<T> observable, Action<T> onNext)
        {
            var observer = new FunctionObserver<T>(onNext);
            return observable.Subscribe(observer);
        }

        public static string Join<T>(this IEnumerable<T> collection, string separator = ", ") => string.Join(separator, collection);

        public static string[] SplitArguments(string commandLine)
        {
            var paramChars = commandLine.ToCharArray();

            var inSingleQuote = false;
            var inDoubleQuote = false;
            for (var index = 0; index < paramChars.Length; index++)
            {
                if (paramChars[index] == '"' && !inSingleQuote)
                {
                    inDoubleQuote = !inDoubleQuote;
                    paramChars[index] = '\n';
                }

                if (paramChars[index] == '\'' && !inDoubleQuote)
                {
                    inSingleQuote = !inSingleQuote;
                    paramChars[index] = '\n';
                }

                if (!inSingleQuote && !inDoubleQuote && paramChars[index] == ' ')
                {
                    paramChars[index] = '\n';
                }
            }

            return new string(paramChars).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static bool IsHeadlessRun(string[] args) => args.Contains("--headless");

        public static string GetResultsPath(string[] args)
            => args.FirstOrDefault(a => a.StartsWith("--result="))?.Replace("--result=", string.Empty) ??
                throw new Exception("You must provide path to store test results with --result path/to/results.xml");

        public static void AssertRegex(string testString, Regex regex)
        {
            Assert.That(regex.IsMatch(testString), $"Expected {testString} to match {regex}");
        }

        public static void AssertMatchesBsonDocument(BsonDocument? actual, IRealmObjectBase? expected, string? message = null,
            [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            var locationString = $"in {memberName} - {sourceFilePath}:{sourceLineNumber}" + (message == null ? string.Empty : $" - {message}");

            if (expected is null)
            {
                Assert.That(actual, Is.Null, $"Expected {actual} to be null {locationString}");
                return;
            }
            else
            {
                Assert.That(actual, Is.Not.Null, $"Expected {actual} to not be null {locationString}");
            }

            Assert.That(expected.ObjectSchema, Is.Not.Null, $"This method should only be used with SG-generated classes, but the ObjectSchema was null {locationString}");

            foreach (var prop in expected.ObjectSchema!)
            {
                // TODO: handle collections
                if (prop.Type.IsCollection(out _))
                {
                    continue;
                }

                var value = actual![prop.Name];

                switch (prop.Type)
                {
                    case PropertyType.Object:
                        // TODO: handle objects
                        break;
                    case PropertyType.Int:
                        AssertAreEqual(value.ToInt64(), expected.GetProperty<long>(prop));
                        break;
                    case PropertyType.Bool:
                        AssertAreEqual(value.AsBoolean, expected.GetProperty<bool>(prop));
                        break;
                    case PropertyType.String:
                        AssertAreEqual(value.AsString, expected.GetProperty<string?>(prop));
                        break;
                    case PropertyType.NullableString:
                        AssertAreEqual(value.IsBsonNull ? null : value.AsString, expected.GetProperty<string?>(prop));
                        break;
                    case PropertyType.Data:
                        AssertAreEqual(value.AsBsonBinaryData.Bytes, expected.GetProperty<byte[]>(prop));
                        break;
                    case PropertyType.NullableData:
                        AssertAreEqual(value.IsBsonNull ? null : value.AsBsonBinaryData.Bytes, expected.GetProperty<byte[]>(prop));
                        break;
                    case PropertyType.Date:
                        AssertAreEqual(new DateTimeOffset(value.ToUniversalTime()), expected.GetProperty<DateTimeOffset>(prop));
                        break;
                    case PropertyType.Float:
                    case PropertyType.Double:
                        AssertAreEqual(value.AsDouble, expected.GetProperty<double>(prop));
                        break;
                    case PropertyType.RealmValue:
                        break;
                    case PropertyType.ObjectId:
                        AssertAreEqual(value.AsObjectId, expected.GetProperty<ObjectId>(prop));
                        break;
                    case PropertyType.Decimal:
                        AssertAreEqual(value.AsDecimal128, expected.GetProperty<Decimal128>(prop));
                        break;
                    case PropertyType.Guid:
                        AssertAreEqual(value.AsGuid, expected.GetProperty<Guid>(prop));
                        break;
                    case PropertyType.NullableInt:
                        AssertAreEqual(value.IsBsonNull ? null : value.ToInt64(), expected.GetProperty<long?>(prop));
                        break;
                    case PropertyType.NullableBool:
                        AssertAreEqual(value.AsNullableBoolean, expected.GetProperty<bool?>(prop));
                        break;
                    case PropertyType.NullableDate:
                        AssertAreEqual(value.IsBsonNull ? null : new DateTimeOffset(value.ToUniversalTime()), expected.GetProperty<DateTimeOffset?>(prop));
                        break;
                    case PropertyType.NullableFloat:
                    case PropertyType.NullableDouble:
                        AssertAreEqual(value.AsNullableDouble, expected.GetProperty<double?>(prop));
                        break;
                    case PropertyType.NullableObjectId:
                        AssertAreEqual(value.AsNullableObjectId, expected.GetProperty<ObjectId?>(prop));
                        break;
                    case PropertyType.NullableDecimal:
                        AssertAreEqual(value.AsNullableDecimal128, expected.GetProperty<Decimal128?>(prop));
                        break;
                    case PropertyType.NullableGuid:
                        AssertAreEqual(value.AsNullableGuid, expected.GetProperty<Guid?>(prop));
                        break;
                }
            }
        }

        public static void AssertAreEqual(object? actual, object? expected, string? message = null,
            [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            var locationString = $"in {memberName} - {sourceFilePath}:{sourceLineNumber}" + (message == null ? string.Empty : $" - {message}");

            if (expected is null)
            {
                Assert.That(actual, Is.Null, $"Expected {actual} to be null {locationString}");
            }
            else
            {
                Assert.That(actual, Is.Not.Null, $"Expected {actual} to not be null {locationString}");
            }

            if (expected is IRealmObjectBase robjExpected)
            {
                Assert.That(robjExpected.ObjectSchema, Is.Not.Null, $"This method should only be used with SG-generated classes, but the ObjectSchema was null {locationString}");

                var robjActual = actual as IRealmObjectBase;
                Assert.That(robjActual, Is.Not.Null, $"Expected {actual} to be a RealmObject {locationString}");
                foreach (var prop in robjExpected.ObjectSchema!.Where(p => !p.Type.IsComputed()))
                {
                    var expectedProp = robjExpected.GetProperty<object?>(prop);
                    var actualProp = robjActual!.GetProperty<object?>(prop);

                    AssertAreEqual(actualProp, expectedProp, $"property: {prop.Name}");
                }
            }
            else if (expected is IEnumerable enumerableExpected and not string and not byte[])
            {
                Assert.That(actual is IEnumerable, $"Expected {actual} to be a collection {locationString}");
                Assert.That(actual, Is.EquivalentTo(enumerableExpected).Using((object a, object e) => AreValuesEqual(a, e)), $"Expected collections to match {locationString}");
            }
            else
            {
                Assert.That(AreValuesEqual(actual, expected), $"Expected {actual} to equal {expected} {locationString}");
            }
        }

        public static bool AreValuesEqual(object? actual, object? expected)
        {
            if (actual is null || expected is null)
            {
                return actual is null && expected is null;
            }

            var expectedType = expected.GetType();
            if (expectedType.IsClosedGeneric(typeof(KeyValuePair<,>), out _))
            {
                var keyPi = expectedType.GetProperty("Key")!;
                var valuePi = expectedType.GetProperty("Value")!;

                // For kvp elements, we need to compare the keys and the values
                return actual.GetType() == expectedType
                    && (string)keyPi.GetValue(actual)! == (string)keyPi.GetValue(expected)!
                    && AreValuesEqual(valuePi.GetValue(actual), valuePi.GetValue(expected));
            }

            if (expected is RealmValue rvExpected)
            {
                return actual is RealmValue rvActual && rvExpected.Type switch
                {
                    // float is not representable in json, so gets serialized as double
                    RealmValueType.Float => rvActual.Type == RealmValueType.Double && rvActual.AsDouble() == (double)rvExpected.AsFloat(),

                    // for binary, we compare the sequences rather than the addresses
                    RealmValueType.Data => rvActual.Type == RealmValueType.Data && AreValuesEqual(rvActual.AsData(), rvExpected.AsData()),

                    RealmValueType.Date => rvActual.Type == RealmValueType.Date && AreValuesEqual(rvActual.AsDate(), rvExpected.AsDate()),
                    _ => rvExpected == rvActual,
                };
            }

            if (expected is byte[] dataExpected)
            {
                return actual is byte[] dataActual && dataActual.SequenceEqual(dataExpected);
            }

            if (expected is DateTimeOffset dateExpected)
            {
                return actual is DateTimeOffset dateActual && dateExpected.ToUnixTimeMilliseconds() == dateActual.ToUnixTimeMilliseconds();
            }

            return actual.Equals(expected);
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

        public class StrongBox<T>
        {
            public T? Value { get; set; }

            public static implicit operator StrongBox<T>(T value) => new() { Value = value };
        }

        public static TestCaseData<T> CreateTestCase<T>(string description, T value) => new(description, value);

        public static TestCaseData<Property> CreateTestCase(Property prop)
        {
            var propType = $"{prop.Type.UnderlyingType()}{(prop.Type.IsNullable() ? "?" : string.Empty)}";
            if (prop.Type.IsCollection(out var collection))
            {
                propType = $"{collection}<{propType}>";
            }

            return new(propType, prop);
        }

        public static T GetProperty<T>(this IRealmObjectBase o, Property property)
        {
            var pi = o.GetType().GetProperty(property.ManagedName, BindingFlags.Public | BindingFlags.Instance)!;

#pragma warning disable CS8600, CS8603 // Caller needs to ensure T is nullable if property may be null
            return Operator.Convert<T>(pi.GetValue(o));
#pragma warning restore CS8600, CS8603
        }

        public class TestCaseData<T>
        {
            private readonly string _description;

            public T Value { get; }

            public TestCaseData(string description, T value)
            {
                _description = description;
                Value = value;
            }

            public override string ToString() => _description;
        }
    }
}
