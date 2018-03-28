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
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using Realms;

namespace Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class ListOfPrimitivesTests : RealmInstanceTest
    {
        private readonly Random _random = new Random();

        private ListsObject _managedListsObject;

        protected override void CustomSetUp()
        {
            base.CustomSetUp();

            _realm.Write(() =>
            {
                _managedListsObject = _realm.Add(new ListsObject());
            });
        }

        #region TestCaseSources

        private static readonly IEnumerable<bool?[]> _booleanValues = new[]
        {
            new bool?[] { true },
            new bool?[] { null },
            new bool?[] { true, true, null, false },
        };

        public static IEnumerable<object> BooleanTestValues()
        {
            yield return new object[] { null };
            var values = _booleanValues.Select(v => v.Where(b => b.HasValue).Select(b => b.Value).ToArray());
            foreach (var value in values.Where(a => a.Any()))
            {
                yield return new object[] { value.ToArray() };
            }
        }

        public static IEnumerable<object> NullableBooleanTestValues()
        {
            yield return new object[] { null };
            foreach (var item in _booleanValues)
            {
                yield return new object[] { item };
            }
        }

        private static readonly IEnumerable<byte?[]> _byteValues = new[]
        {
            new byte?[] { 0 },
            new byte?[] { null },
            new byte?[] { byte.MinValue, byte.MaxValue, 0 },
            new byte?[] { 1, 2, 3, null },
        };

        public static IEnumerable<object> ByteTestValues()
        {
            yield return new object[] { null };
            var values = _byteValues.Select(v => v.Where(b => b.HasValue).Select(b => b.Value).ToArray());
            foreach (var value in values.Where(a => a.Any()))
            {
                yield return new object[] { value.ToArray() };
            }
        }

        public static IEnumerable<object> NullableByteTestValues()
        {
            yield return new object[] { null };
            foreach (var value in _byteValues)
            {
                yield return new object[] { value.ToArray() };
            }
        }

        private static readonly IEnumerable<char?[]> _charValues = new[]
        {
            new char?[] { 'a' },
            new char?[] { null },
            new char?[] { char.MinValue, char.MaxValue },
            new char?[] { 'a', 'b', 'c', 'b', null }
        };

        public static IEnumerable<object> CharTestValues()
        {
            yield return new object[] { null };
            var values = _charValues.Select(v => v.Where(b => b.HasValue).Select(b => b.Value).ToArray());
            foreach (var value in values.Where(a => a.Any()))
            {
                yield return new object[] { value.ToArray() };
            }
        }

        public static IEnumerable<object> NullableCharTestValues()
        {
            yield return new object[] { null };
            foreach (var value in _charValues)
            {
                yield return new object[] { value.ToArray() };
            }
        }

        private static readonly IEnumerable<double?[]> _doubleValues = new[]
        {
            new double?[] { 1.4 },
            new double?[] { null },
            new double?[] { double.MinValue, double.MaxValue, 0 },
            new double?[] { -1, 3.4, null, 5.3, 9 }
        };

        public static IEnumerable<object> DoubleTestValues()
        {
            yield return new object[] { null };
            var values = _doubleValues.Select(v => v.Where(b => b.HasValue).Select(b => b.Value).ToArray());
            foreach (var value in values.Where(a => a.Any()))
            {
                yield return new object[] { value.ToArray() };
            }
        }

        public static IEnumerable<object> NullableDoubleTestValues()
        {
            yield return new object[] { null };
            foreach (var value in _doubleValues)
            {
                yield return new object[] { value.ToArray() };
            }
        }

        private static readonly IEnumerable<short?[]> _shortValues = new[]
        {
            new short?[] { 1 },
            new short?[] { null },
            new short?[] { short.MaxValue, short.MinValue, 0 },
            new short?[] { 3, -1, null, 45, null },
        };

        public static IEnumerable<object> Int16TestValues()
        {
            yield return new object[] { null };
            var values = _shortValues.Select(v => v.Where(b => b.HasValue).Select(b => b.Value).ToArray());
            foreach (var value in values.Where(a => a.Any()))
            {
                yield return new object[] { value.ToArray() };
            }
        }

        public static IEnumerable<object> NullableInt16TestValues()
        {
            yield return new object[] { null };
            foreach (var value in _shortValues)
            {
                yield return new object[] { value.ToArray() };
            }
        }

        private static readonly IEnumerable<int?[]> _intValues = new[]
        {
            new int?[] { 1 },
            new int?[] { null },
            new int?[] { int.MaxValue, int.MinValue, 0 },
            new int?[] { -5, 3, 9, null, 350 },
        };

        public static IEnumerable<object> Int32TestValues()
        {
            yield return new object[] { null };
            var values = _intValues.Select(v => v.Where(b => b.HasValue).Select(b => b.Value).ToArray());
            foreach (var value in values.Where(a => a.Any()))
            {
                yield return new object[] { value.ToArray() };
            }
        }

        public static IEnumerable<object> NullableInt32TestValues()
        {
            yield return new object[] { null };
            foreach (var value in _intValues)
            {
                yield return new object[] { value.ToArray() };
            }
        }

        private static readonly IEnumerable<long?[]> _longValues = new[]
        {
            new long?[] { 1 },
            new long?[] { null },
            new long?[] { long.MaxValue, long.MinValue, 0 },
            new long?[] { 4, -39, 81, null, -69324 },
        };

        public static IEnumerable<object> Int64TestValues()
        {
            yield return new object[] { null };
            var values = _longValues.Select(v => v.Where(b => b.HasValue).Select(b => b.Value).ToArray());
            foreach (var value in values.Where(a => a.Any()))
            {
                yield return new object[] { value.ToArray() };
            }
        }

        public static IEnumerable<object> NullableInt64TestValues()
        {
            yield return new object[] { null };
            foreach (var value in _longValues)
            {
                yield return new object[] { value.ToArray() };
            }
        }

        private static readonly IEnumerable<DateTimeOffset?[]> _dateValues = new[]
        {
            new DateTimeOffset?[] { DateTimeOffset.UtcNow.AddDays(-4) },
            new DateTimeOffset?[] { null },
            new DateTimeOffset?[] { DateTimeOffset.MinValue, DateTimeOffset.MaxValue, DateTimeOffset.UtcNow },
            new DateTimeOffset?[] { DateTimeOffset.UtcNow.AddDays(5), null, DateTimeOffset.UtcNow.AddDays(-39), DateTimeOffset.UtcNow.AddDays(81), DateTimeOffset.UtcNow.AddDays(-69324) },
        };

        public static IEnumerable<object> DateTestValues()
        {
            yield return new object[] { null };
            var values = _dateValues.Select(v => v.Where(b => b.HasValue).Select(b => b.Value).ToArray());
            foreach (var value in values.Where(a => a.Any()))
            {
                yield return new object[] { value.ToArray() };
            }
        }

        public static IEnumerable<object> NullableDateTestValues()
        {
            yield return new object[] { null };
            foreach (var value in _dateValues)
            {
                yield return new object[] { value.ToArray() };
            }
        }

        public static IEnumerable<object> StringTestValues()
        {
            yield return new object[] { null };
            yield return new object[] { new string[] { string.Empty } };
            yield return new object[] { new string[] { null } };
            yield return new object[] { new string[] { " " } };
            yield return new object[] { new string[] { "abc", "cdf", "az" } };
            yield return new object[] { new string[] { "a", null, "foo", "bar", null } };
        }

        public static IEnumerable<object> ByteArrayTestValues()
        {
            yield return new object[] { null };
            yield return new object[] { new byte[][] { new byte[0] } };
            yield return new object[] { new byte[][] { null } };
            yield return new object[] { new byte[][] { new byte[] { 0 } } };
            yield return new object[] { new byte[][] { new byte[] { 0, byte.MinValue, byte.MaxValue } } };
            yield return new object[] { new byte[][] { TestHelpers.GetBytes(3), TestHelpers.GetBytes(5), TestHelpers.GetBytes(7) } };
            yield return new object[] { new byte[][] { TestHelpers.GetBytes(1), null, TestHelpers.GetBytes(3), TestHelpers.GetBytes(3), null } };
        }

        #endregion

        #region Managed Tests

        [TestCaseSource(nameof(BooleanTestValues))]
        public void Test_ManagedBooleanList(bool[] values)
        {
            RunManagedTests(_managedListsObject.BooleanList, values);
        }

        [TestCaseSource(nameof(ByteTestValues))]
        public void Test_ManagedByteCounterList(byte[] values)
        {
            RunManagedTests(_managedListsObject.ByteCounterList, values.ToInteger());
        }

        [TestCaseSource(nameof(ByteTestValues))]
        public void Test_ManagedByteList(byte[] values)
        {
            RunManagedTests(_managedListsObject.ByteList, values);
        }

        [TestCaseSource(nameof(CharTestValues))]
        public void Test_ManagedCharList(char[] values)
        {
            RunManagedTests(_managedListsObject.CharList, values);
        }

        [TestCaseSource(nameof(DoubleTestValues))]
        public void Test_ManagedDoubleList(double[] values)
        {
            RunManagedTests(_managedListsObject.DoubleList, values);
        }

        [TestCaseSource(nameof(Int16TestValues))]
        public void Test_ManagedInt16CounterList(short[] values)
        {
            RunManagedTests(_managedListsObject.Int16CounterList, values.ToInteger());
        }

        [TestCaseSource(nameof(Int16TestValues))]
        public void Test_ManagedInt16List(short[] values)
        {
            RunManagedTests(_managedListsObject.Int16List, values);
        }

        [TestCaseSource(nameof(Int32TestValues))]
        public void Test_ManagedInt32CounterList(int[] values)
        {
            RunManagedTests(_managedListsObject.Int32CounterList, values.ToInteger());
        }

        [TestCaseSource(nameof(Int32TestValues))]
        public void Test_ManagedInt32List(int[] values)
        {
            RunManagedTests(_managedListsObject.Int32List, values);
        }

        [TestCaseSource(nameof(Int64TestValues))]
        public void Test_ManagedInt64CounterList(long[] values)
        {
            RunManagedTests(_managedListsObject.Int64CounterList, values.ToInteger());
        }

        [TestCaseSource(nameof(Int64TestValues))]
        public void Test_ManagedInt64List(long[] values)
        {
            RunManagedTests(_managedListsObject.Int64List, values);
        }

        [TestCaseSource(nameof(DateTestValues))]
        public void Test_ManagedDateTimeOffsetList(DateTimeOffset[] values)
        {
            RunManagedTests(_managedListsObject.DateTimeOffsetList, values);
        }

        [TestCaseSource(nameof(NullableBooleanTestValues))]
        public void Test_ManagedNullableBooleanList(bool?[] values)
        {
            RunManagedTests(_managedListsObject.NullableBooleanList, values);
        }

        [TestCaseSource(nameof(NullableByteTestValues))]
        public void Test_ManagedNullableByteCounterList(byte?[] values)
        {
            RunManagedTests(_managedListsObject.NullableByteCounterList, values.ToInteger());
        }

        [TestCaseSource(nameof(NullableByteTestValues))]
        public void Test_ManagedNullableByteList(byte?[] values)
        {
            RunManagedTests(_managedListsObject.NullableByteList, values);
        }

        [TestCaseSource(nameof(NullableCharTestValues))]
        public void Test_ManagedNullableCharList(char?[] values)
        {
            RunManagedTests(_managedListsObject.NullableCharList, values);
        }

        [TestCaseSource(nameof(NullableDoubleTestValues))]
        public void Test_ManagedNullableDoubleList(double?[] values)
        {
            RunManagedTests(_managedListsObject.NullableDoubleList, values);
        }

        [TestCaseSource(nameof(NullableInt16TestValues))]
        public void Test_ManagedNullableInt16CounterList(short?[] values)
        {
            RunManagedTests(_managedListsObject.NullableInt16CounterList, values.ToInteger());
        }

        [TestCaseSource(nameof(NullableInt16TestValues))]
        public void Test_ManagedNullableInt16List(short?[] values)
        {
            RunManagedTests(_managedListsObject.NullableInt16List, values);
        }

        [TestCaseSource(nameof(NullableInt32TestValues))]
        public void Test_ManagedNullableInt32CounterList(int?[] values)
        {
            RunManagedTests(_managedListsObject.NullableInt32CounterList, values.ToInteger());
        }

        [TestCaseSource(nameof(NullableInt32TestValues))]
        public void Test_ManagedNullableInt32List(int?[] values)
        {
            RunManagedTests(_managedListsObject.NullableInt32List, values);
        }

        [TestCaseSource(nameof(NullableInt64TestValues))]
        public void Test_ManagedNullableInt64CounterList(long?[] values)
        {
            RunManagedTests(_managedListsObject.NullableInt64CounterList, values.ToInteger());
        }

        [TestCaseSource(nameof(NullableInt64TestValues))]
        public void Test_ManagedNullableInt64List(long?[] values)
        {
            RunManagedTests(_managedListsObject.NullableInt64List, values);
        }

        [TestCaseSource(nameof(NullableDateTestValues))]
        public void Test_ManagedNullableDateTimeOffsetList(DateTimeOffset?[] values)
        {
            RunManagedTests(_managedListsObject.NullableDateTimeOffsetList, values);
        }

        [TestCaseSource(nameof(StringTestValues))]
        public void Test_ManagedStringList(string[] values)
        {
            RunManagedTests(_managedListsObject.StringList, values);
        }

        [TestCaseSource(nameof(ByteArrayTestValues))]
        public void Test_ManagedByteArrayList(byte[][] values)
        {
            RunManagedTests(_managedListsObject.ByteArrayList, values);
        }

        private void RunManagedTests<T>(IList<T> items, T[] toAdd)
        {
            AsyncContext.Run(() => RunManagedTestsAsync(items, toAdd).Timeout(5000));
        }

        private async Task RunManagedTestsAsync<T>(IList<T> items, T[] toAdd)
        {
            if (toAdd == null)
            {
                toAdd = new T[0];
            }

            var notifications = new List<ChangeSet>();
            var token = items.SubscribeForNotifications((sender, changes, error) =>
            {
                if (changes != null)
                {
                    notifications.Add(changes);
                }
            });

            // Test add
            _realm.Write(() =>
            {
                foreach (var item in toAdd)
                {
                    items.Add(item);
                }
            });

            // Test notifications
            if (toAdd.Any())
            {
                VerifyNotifications(notifications, () =>
                {
                    Assert.That(notifications[0].InsertedIndices, Is.EquivalentTo(Enumerable.Range(0, toAdd.Length)));
                });
            }

            // Test iterating
            var iterator = 0;
            foreach (var item in items)
            {
                Assert.That(item, Is.EqualTo(toAdd[iterator++]));
            }

            // Test access by index
            for (var i = 0; i < items.Count; i++)
            {
                Assert.That(items[i], Is.EqualTo(toAdd[i]));
            }

            Assert.That(() => items[-1], Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => items[items.Count], Throws.TypeOf<ArgumentOutOfRangeException>());

            // Test indexOf
            foreach (var item in toAdd)
            {
                Assert.That(items.IndexOf(item), Is.EqualTo(Array.IndexOf(toAdd, item)));
            }

            // Test threadsafe reference
            var reference = ThreadSafeReference.Create(items);
            await Task.Run(() =>
            {
                using (var realm = Realm.GetInstance(_realm.Config))
                {
                    var backgroundList = realm.ResolveReference(reference);
                    for (var i = 0; i < backgroundList.Count; i++)
                    {
                        Assert.That(backgroundList[i], Is.EqualTo(toAdd[i]));
                    }
                }
            });

            if (toAdd.Any())
            {
                // Test insert
                var toInsert = toAdd[_random.Next(0, toAdd.Length)];
                _realm.Write(() =>
                {
                    items.Insert(0, toInsert);
                    items.Insert(items.Count, toInsert);

                    Assert.That(() => items.Insert(-1, toInsert), Throws.TypeOf<ArgumentOutOfRangeException>());
                    Assert.That(() => items.Insert(items.Count + 1, toInsert), Throws.TypeOf<ArgumentOutOfRangeException>());
                });

                Assert.That(items.First(), Is.EqualTo(toInsert));
                Assert.That(items.Last(), Is.EqualTo(toInsert));

                // Test notifications
                VerifyNotifications(notifications, () =>
                {
                    Assert.That(notifications[0].InsertedIndices, Is.EquivalentTo(new[] { 0, items.Count - 1 }));
                });

                // Test remove
                _realm.Write(() =>
                {
                    items.Remove(toInsert);
                    items.RemoveAt(items.Count - 1);

                    Assert.That(() => items.RemoveAt(-1), Throws.TypeOf<ArgumentOutOfRangeException>());
                    Assert.That(() => items.RemoveAt(items.Count + 1), Throws.TypeOf<ArgumentOutOfRangeException>());
                });

                CollectionAssert.AreEqual(items, toAdd);

                // Test notifications
                VerifyNotifications(notifications, () =>
                {
                    Assert.That(notifications[0].DeletedIndices, Is.EquivalentTo(new[] { 0, items.Count + 1 }));
                });

                // Test set
                var indexToSet = TestHelpers.Random.Next(0, items.Count);
                var previousValue = items[indexToSet];
                var valueToSet = toAdd[TestHelpers.Random.Next(0, toAdd.Length)];
                _realm.Write(() =>
                {
                    items[indexToSet] = valueToSet;

                    Assert.That(() => items[-1] = valueToSet, Throws.TypeOf<ArgumentOutOfRangeException>());
                    Assert.That(() => items[items.Count] = valueToSet, Throws.TypeOf<ArgumentOutOfRangeException>());
                });

                VerifyNotifications(notifications, () =>
                {
                    Assert.That(notifications[0].ModifiedIndices, Is.EquivalentTo(new[] { indexToSet }));
                });

                _realm.Write(() => items[indexToSet] = previousValue);

                VerifyNotifications(notifications, () =>
                {
                    Assert.That(notifications[0].ModifiedIndices, Is.EquivalentTo(new[] { indexToSet }));
                });

                // Test move
                var from = TestHelpers.Random.Next(0, items.Count);
                var to = TestHelpers.Random.Next(0, items.Count);

                _realm.Write(() =>
                {
                    items.Move(from, to);

                    Assert.That(() => items.Move(-1, to), Throws.TypeOf<ArgumentOutOfRangeException>());
                    Assert.That(() => items.Move(from, -1), Throws.TypeOf<ArgumentOutOfRangeException>());
                    Assert.That(() => items.Move(items.Count + 1, to), Throws.TypeOf<ArgumentOutOfRangeException>());
                    Assert.That(() => items.Move(from, items.Count + 1), Throws.TypeOf<ArgumentOutOfRangeException>());
                });

                Assert.That(items[to], Is.EqualTo(toAdd[from]));

                // Test notifications
                if (from != to)
                {
                    VerifyNotifications(notifications, () =>
                    {
                        Assert.That(notifications[0].Moves.Length, Is.EqualTo(1));
                        var move = notifications[0].Moves[0];

                        // Moves may be reported with swapped from/to arguments if the elements are adjacent
                        if (move.From == to)
                        {
                            Assert.That(move.From, Is.EqualTo(to));
                            Assert.That(move.To, Is.EqualTo(from));
                        }
                        else
                        {
                            Assert.That(move.From, Is.EqualTo(from));
                            Assert.That(move.To, Is.EqualTo(to));
                        }
                    });
                }
            }

            // Test Clear
            _realm.Write(() =>
            {
                items.Clear();
            });

            Assert.That(items, Is.Empty);

            // Test notifications
            if (toAdd.Any())
            {
                VerifyNotifications(notifications, () =>
                {
                    // TODO: verify notifications contains the expected Deletions collection
                });
            }

            token.Dispose();
        }

        private void VerifyNotifications(List<ChangeSet> notifications, Action verifier)
        {
            _realm.Refresh();
            Assert.That(notifications.Count, Is.EqualTo(1));
            verifier();
            notifications.Clear();
        }

        #endregion

        #region Unmanaged Tests

        [TestCaseSource(nameof(BooleanTestValues))]
        public void Test_UnmanagedBooleanList(bool[] values)
        {
            RunUnmanagedTests(o => o.BooleanList, values);
        }

        [TestCaseSource(nameof(ByteTestValues))]
        public void Test_UnmanagedByteCounterList(byte[] values)
        {
            RunUnmanagedTests(o => o.ByteCounterList, values.ToInteger());
        }

        [TestCaseSource(nameof(ByteTestValues))]
        public void Test_UnmanagedByteList(byte[] values)
        {
            RunUnmanagedTests(o => o.ByteList, values);
        }

        [TestCaseSource(nameof(CharTestValues))]
        public void Test_UnmanagedCharList(char[] values)
        {
            RunUnmanagedTests(o => o.CharList, values);
        }

        [TestCaseSource(nameof(DoubleTestValues))]
        public void Test_UnmanagedDoubleList(double[] values)
        {
            RunUnmanagedTests(o => o.DoubleList, values);
        }

        [TestCaseSource(nameof(Int16TestValues))]
        public void Test_UnmanagedInt16CounterList(short[] values)
        {
            RunUnmanagedTests(o => o.Int16CounterList, values.ToInteger());
        }

        [TestCaseSource(nameof(Int16TestValues))]
        public void Test_UnmanagedInt16List(short[] values)
        {
            RunUnmanagedTests(o => o.Int16List, values);
        }

        [TestCaseSource(nameof(Int32TestValues))]
        public void Test_UnmanagedInt32CounterList(int[] values)
        {
            RunUnmanagedTests(o => o.Int32CounterList, values.ToInteger());
        }

        [TestCaseSource(nameof(Int32TestValues))]
        public void Test_UnmanagedInt32List(int[] values)
        {
            RunUnmanagedTests(o => o.Int32List, values);
        }

        [TestCaseSource(nameof(Int64TestValues))]
        public void Test_UnmanagedInt64CounterList(long[] values)
        {
            RunUnmanagedTests(o => o.Int64CounterList, values.ToInteger());
        }

        [TestCaseSource(nameof(Int64TestValues))]
        public void Test_UnmanagedInt64List(long[] values)
        {
            RunUnmanagedTests(o => o.Int64List, values);
        }

        [TestCaseSource(nameof(DateTestValues))]
        public void Test_UnmanagedDateTimeOffsetList(DateTimeOffset[] values)
        {
            RunUnmanagedTests(o => o.DateTimeOffsetList, values);
        }

        [TestCaseSource(nameof(NullableBooleanTestValues))]
        public void Test_UnmanagedNullableBooleanList(bool?[] values)
        {
            RunUnmanagedTests(o => o.NullableBooleanList, values);
        }

        [TestCaseSource(nameof(NullableByteTestValues))]
        public void Test_UnmanagedNullableByteCounterList(byte?[] values)
        {
            RunUnmanagedTests(o => o.NullableByteCounterList, values.ToInteger());
        }

        [TestCaseSource(nameof(NullableByteTestValues))]
        public void Test_UnmanagedNullableByteList(byte?[] values)
        {
            RunUnmanagedTests(o => o.NullableByteList, values);
        }

        [TestCaseSource(nameof(NullableCharTestValues))]
        public void Test_UnmanagedNullableCharList(char?[] values)
        {
            RunUnmanagedTests(o => o.NullableCharList, values);
        }

        [TestCaseSource(nameof(NullableDoubleTestValues))]
        public void Test_UnmanagedNullableDoubleList(double?[] values)
        {
            RunUnmanagedTests(o => o.NullableDoubleList, values);
        }

        [TestCaseSource(nameof(NullableInt16TestValues))]
        public void Test_UnmanagedNullableInt16CounterList(short?[] values)
        {
            RunUnmanagedTests(o => o.NullableInt16CounterList, values.ToInteger());
        }

        [TestCaseSource(nameof(NullableInt16TestValues))]
        public void Test_UnmanagedNullableInt16List(short?[] values)
        {
            RunUnmanagedTests(o => o.NullableInt16List, values);
        }

        [TestCaseSource(nameof(NullableInt32TestValues))]
        public void Test_UnmanagedNullableInt32CounterList(int?[] values)
        {
            RunUnmanagedTests(o => o.NullableInt32CounterList, values.ToInteger());
        }

        [TestCaseSource(nameof(NullableInt32TestValues))]
        public void Test_UnmanagedNullableInt32List(int?[] values)
        {
            RunUnmanagedTests(o => o.NullableInt32List, values);
        }

        [TestCaseSource(nameof(NullableInt64TestValues))]
        public void Test_UnmanagedNullableInt64CounterList(long?[] values)
        {
            RunUnmanagedTests(o => o.NullableInt64CounterList, values.ToInteger());
        }

        [TestCaseSource(nameof(NullableInt64TestValues))]
        public void Test_UnmanagedNullableInt64List(long?[] values)
        {
            RunUnmanagedTests(o => o.NullableInt64List, values);
        }

        [TestCaseSource(nameof(NullableDateTestValues))]
        public void Test_UnmanagedNullableDateTimeOffsetList(DateTimeOffset?[] values)
        {
            RunUnmanagedTests(o => o.NullableDateTimeOffsetList, values);
        }

        [TestCaseSource(nameof(StringTestValues))]
        public void Test_UnmanagedStringList(string[] values)
        {
            RunUnmanagedTests(o => o.StringList, values);
        }

        [TestCaseSource(nameof(ByteArrayTestValues))]
        public void Test_UnmanagedByteArrayList(byte[][] values)
        {
            RunUnmanagedTests(o => o.ByteArrayList, values);
        }

        private void RunUnmanagedTests<T>(Func<ListsObject, IList<T>> accessor, T[] toAdd)
        {
            if (toAdd == null)
            {
                toAdd = new T[0];
            }

            var listsObject = new ListsObject();
            var list = accessor(listsObject);

            foreach (var item in toAdd)
            {
                list.Add(item);
            }

            CollectionAssert.AreEqual(list, toAdd);

            _realm.Write(() => _realm.Add(listsObject));

            var managedList = accessor(listsObject);

            CollectionAssert.AreEqual(managedList, toAdd);
        }

        #endregion
    }
}
