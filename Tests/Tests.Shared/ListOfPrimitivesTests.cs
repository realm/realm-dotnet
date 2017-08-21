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
using NUnit.Framework;
using Realms;

namespace Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class ListOfPrimitivesTests : RealmInstanceTest
    {
        private readonly Random _random = new Random();

        private ListsObject _listsObject;

        protected override void CustomSetUp()
        {
            base.CustomSetUp();

            _realm.Write(() =>
            {
                _listsObject = _realm.Add(new ListsObject());
            });
        }

        [Test]
        public void Test_BooleanList()
        {
            SmokeTest(_listsObject.BooleanList);
            SmokeTest(_listsObject.BooleanList, true);
            SmokeTest(_listsObject.BooleanList, true, true, false);
        }

        [Test]
        public void Test_ByteCounterList()
        {
            SmokeTest(_listsObject.ByteCounterList);
            SmokeTest<RealmInteger<byte>>(_listsObject.ByteCounterList, 0);
            SmokeTest<RealmInteger<byte>>(_listsObject.ByteCounterList, byte.MinValue, byte.MaxValue, 0);
            SmokeTest<RealmInteger<byte>>(_listsObject.ByteCounterList, 1, 2, 0);
        }

        [Test]
        public void Test_ByteList()
        {
            SmokeTest(_listsObject.ByteList);
            SmokeTest<byte>(_listsObject.ByteList, 0);
            SmokeTest<byte>(_listsObject.ByteList, byte.MinValue, byte.MaxValue, 0);
            SmokeTest<byte>(_listsObject.ByteList, 1, 2, 0);
        }

        [Test]
        public void Test_CharList()
        {
            SmokeTest(_listsObject.CharList);
            SmokeTest(_listsObject.CharList, 'a');
            SmokeTest(_listsObject.CharList, char.MinValue, char.MaxValue);
            SmokeTest(_listsObject.CharList, 'a', 'b', 'c', 'b');
        }

        [Test]
        public void Test_DoubleList()
        {
            SmokeTest(_listsObject.DoubleList);
            SmokeTest(_listsObject.DoubleList, 1.4);
            SmokeTest(_listsObject.DoubleList, double.MinValue, double.MaxValue, 0);
            SmokeTest(_listsObject.DoubleList, -1, 3.4, 5.3, 9);
        }

        [Test]
        public void Test_Int16CounterList()
        {
            SmokeTest(_listsObject.Int16CounterList);
            SmokeTest<RealmInteger<short>>(_listsObject.Int16CounterList, 1);
            SmokeTest<RealmInteger<short>>(_listsObject.Int16CounterList, short.MaxValue, short.MinValue, 0);
            SmokeTest<RealmInteger<short>>(_listsObject.Int16CounterList, 3, -1, 45);
        }

        [Test]
        public void Test_Int16List()
        {
            SmokeTest(_listsObject.Int16List);
            SmokeTest<short>(_listsObject.Int16List, 1);
            SmokeTest<short>(_listsObject.Int16List, short.MaxValue, short.MinValue, 0);
            SmokeTest<short>(_listsObject.Int16List, 3, -1, 45);
        }

        [Test]
        public void Test_Int32CounterList()
        {
            SmokeTest(_listsObject.Int32CounterList);
            SmokeTest(_listsObject.Int32CounterList, 1);
            SmokeTest(_listsObject.Int32CounterList, int.MinValue, int.MaxValue, 0);
            SmokeTest(_listsObject.Int32CounterList, -5, 3, 9, 350);
        }

        [Test]
        public void Test_Int32List()
        {
            SmokeTest(_listsObject.Int32List);
            SmokeTest(_listsObject.Int32List, 1);
            SmokeTest(_listsObject.Int32List, int.MinValue, int.MaxValue, 0);
            SmokeTest(_listsObject.Int32List, -5, 3, 9, 350);
        }

        [Test]
        public void Test_Int64CounterList()
        {
            SmokeTest(_listsObject.Int64CounterList);
            SmokeTest(_listsObject.Int64CounterList, 4);
            SmokeTest(_listsObject.Int64CounterList, long.MinValue, long.MaxValue, 0);
            SmokeTest(_listsObject.Int64CounterList, 4, -39, 81L, -69324);
        }

        [Test]
        public void Test_Int64List()
        {
            SmokeTest(_listsObject.Int64List);
            SmokeTest(_listsObject.Int64List, 4);
            SmokeTest(_listsObject.Int64List, long.MinValue, long.MaxValue, 0);
            SmokeTest(_listsObject.Int64List, 4, -39, 81L, -69324);
        }

        [Test]
        public void Test_DateTimeOffsetList()
        {
            SmokeTest(_listsObject.DateTimeOffsetList);
            SmokeTest(_listsObject.DateTimeOffsetList, DateTimeOffset.UtcNow.AddDays(-4));
            SmokeTest(_listsObject.DateTimeOffsetList, DateTimeOffset.MinValue, DateTimeOffset.MaxValue, DateTimeOffset.UtcNow);
            SmokeTest(_listsObject.DateTimeOffsetList, DateTimeOffset.UtcNow.AddDays(5), DateTimeOffset.UtcNow.AddDays(-39), DateTimeOffset.UtcNow.AddDays(81), DateTimeOffset.UtcNow.AddDays(-69324));
        }

        [Test]
        public void Test_NullableBooleanList()
        {
            SmokeTest(_listsObject.NullableBooleanList);
            SmokeTest(_listsObject.NullableBooleanList, true);
            SmokeTest(_listsObject.NullableBooleanList, (bool?)null);
            SmokeTest(_listsObject.NullableBooleanList, true, true, null, false);
        }

        [Test]
        public void Test_NullableByteCounterList()
        {
            SmokeTest(_listsObject.NullableByteCounterList);
            SmokeTest<RealmInteger<byte>?>(_listsObject.NullableByteCounterList, 0);
            SmokeTest(_listsObject.NullableByteCounterList, (byte?)null);
            SmokeTest<RealmInteger<byte>?>(_listsObject.NullableByteCounterList, byte.MinValue, byte.MaxValue, 0);
            SmokeTest<RealmInteger<byte>?>(_listsObject.NullableByteCounterList, 1, 2, 0, null);
        }

        [Test]
        public void Test_NullableByteList()
        {
            SmokeTest(_listsObject.NullableByteList);
            SmokeTest<byte?>(_listsObject.NullableByteList, 0);
            SmokeTest(_listsObject.NullableByteList, (byte?)null);
            SmokeTest<byte?>(_listsObject.NullableByteList, byte.MinValue, byte.MaxValue, 0);
            SmokeTest<byte?>(_listsObject.NullableByteList, 1, 2, 0, null);
        }

        [Test]
        public void Test_NullableCharList()
        {
            SmokeTest(_listsObject.NullableCharList);
            SmokeTest(_listsObject.NullableCharList, 'a');
            SmokeTest(_listsObject.NullableCharList, (char?)null);
            SmokeTest(_listsObject.NullableCharList, char.MinValue, char.MaxValue);
            SmokeTest(_listsObject.NullableCharList, 'a', 'b', 'c', 'b');
        }

        [Test]
        public void Test_NullableDoubleList()
        {
            SmokeTest(_listsObject.NullableDoubleList);
            SmokeTest(_listsObject.NullableDoubleList, 1.4);
            SmokeTest(_listsObject.NullableDoubleList, (double?)null);
            SmokeTest(_listsObject.NullableDoubleList, double.MinValue, double.MaxValue, 0);
            SmokeTest(_listsObject.NullableDoubleList, -1, 3.4, 5.3, 9);
        }

        [Test]
        public void Test_NullableInt16CounterList()
        {
            SmokeTest(_listsObject.NullableInt16CounterList);
            SmokeTest<RealmInteger<short>?>(_listsObject.NullableInt16CounterList, 1);
            SmokeTest(_listsObject.NullableInt16CounterList, (short?)null);
            SmokeTest<RealmInteger<short>?>(_listsObject.NullableInt16CounterList, short.MaxValue, short.MinValue, 0);
            SmokeTest<RealmInteger<short>?>(_listsObject.NullableInt16CounterList, 3, -1, null, 45, null);
        }

        [Test]
        public void Test_NullableInt16List()
        {
            SmokeTest(_listsObject.NullableInt16List);
            SmokeTest<short?>(_listsObject.NullableInt16List, 1);
            SmokeTest(_listsObject.NullableInt16List, (short?)null);
            SmokeTest<short?>(_listsObject.NullableInt16List, short.MaxValue, short.MinValue, 0);
            SmokeTest<short?>(_listsObject.NullableInt16List, 3, -1, null, 45, null);
        }

        [Test]
        public void Test_NullableInt32CounterList()
        {
            SmokeTest(_listsObject.NullableInt32CounterList);
            SmokeTest(_listsObject.NullableInt32CounterList, 1);
            SmokeTest(_listsObject.NullableInt32CounterList, (int?)null);
            SmokeTest(_listsObject.NullableInt32CounterList, int.MinValue, int.MaxValue, 0);
            SmokeTest(_listsObject.NullableInt32CounterList, -5, 3, null, 9, 350);
        }

        [Test]
        public void Test_NullableInt32List()
        {
            SmokeTest(_listsObject.NullableInt32List);
            SmokeTest(_listsObject.NullableInt32List, 1);
            SmokeTest(_listsObject.NullableInt32List, (int?)null);
            SmokeTest(_listsObject.NullableInt32List, int.MinValue, int.MaxValue, 0);
            SmokeTest(_listsObject.NullableInt32List, -5, 3, null, 9, 350);
        }

        [Test]
        public void Test_NullableInt64CounterList()
        {
            SmokeTest(_listsObject.NullableInt64CounterList);
            SmokeTest(_listsObject.NullableInt64CounterList, 4);
            SmokeTest(_listsObject.NullableInt64CounterList, (long?)null);
            SmokeTest(_listsObject.NullableInt64CounterList, long.MinValue, long.MaxValue, 0);
            SmokeTest(_listsObject.NullableInt64CounterList, 4, -39, 81L, null, -69324);
        }

        [Test]
        public void Test_NullableInt64List()
        {
            SmokeTest(_listsObject.NullableInt64List);
            SmokeTest(_listsObject.NullableInt64List, 4);
            SmokeTest(_listsObject.NullableInt64List, (long?)null);
            SmokeTest(_listsObject.NullableInt64List, long.MinValue, long.MaxValue, 0);
            SmokeTest(_listsObject.NullableInt64List, 4, -39, 81L, null, -69324);
        }

        [Test]
        public void Test_NullableDateTimeOffsetList()
        {
            SmokeTest(_listsObject.NullableDateTimeOffsetList);
            SmokeTest(_listsObject.NullableDateTimeOffsetList, DateTimeOffset.UtcNow.AddDays(-4));
            SmokeTest(_listsObject.NullableDateTimeOffsetList, (DateTimeOffset?)null);
            SmokeTest(_listsObject.NullableDateTimeOffsetList, DateTimeOffset.MinValue, DateTimeOffset.MaxValue, DateTimeOffset.UtcNow);
            SmokeTest(_listsObject.NullableDateTimeOffsetList, DateTimeOffset.UtcNow.AddDays(5), null, DateTimeOffset.UtcNow.AddDays(-39), DateTimeOffset.UtcNow.AddDays(81), DateTimeOffset.UtcNow.AddDays(-69324));
        }

        [Test]
        public void Test_StringList()
        {
            SmokeTest(_listsObject.StringList);
            SmokeTest(_listsObject.StringList, string.Empty);
            SmokeTest(_listsObject.StringList, (string)null);
            SmokeTest(_listsObject.StringList, " ");
            SmokeTest(_listsObject.StringList, "abc", "cdf", "az");
            SmokeTest(_listsObject.StringList, "a", null, "foo", "bar", null);
        }

        [Test]
        public void Test_ByteArrayList()
        {
            SmokeTest(_listsObject.ByteArrayList);
            SmokeTest(_listsObject.ByteArrayList, new byte[0]);
            SmokeTest(_listsObject.ByteArrayList, (byte[])null);
            SmokeTest(_listsObject.ByteArrayList, new byte[1] { 0 });
            SmokeTest(_listsObject.ByteArrayList, new byte[3] { 0, byte.MinValue, byte.MaxValue });
            SmokeTest(_listsObject.ByteArrayList, TestHelpers.GetBytes(3), TestHelpers.GetBytes(5), TestHelpers.GetBytes(7));
            SmokeTest(_listsObject.ByteArrayList, TestHelpers.GetBytes(1), null, TestHelpers.GetBytes(3), TestHelpers.GetBytes(3), null);
        }

        private void SmokeTest<T>(IList<T> items, params T[] toAdd)
        {
            if (toAdd == null)
            {
                toAdd = new T[0];
            }

            // Test add
            _realm.Write(() =>
            {
                items.Clear();

                foreach (var item in toAdd)
                {
                    items.Add(item);
                }
            });

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

            // Test indexOf
            foreach (var item in toAdd)
            {
                Assert.That(items.IndexOf(item), Is.EqualTo(Array.IndexOf(toAdd, item)));
            }

            if (toAdd.Any())
            {
                // Test insert
                var toInsert = toAdd[_random.Next(0, toAdd.Length)];
                _realm.Write(() =>
                {
                    items.Insert(0, toInsert);
                    items.Insert(items.Count, toInsert);
                });

                Assert.That(items.First(), Is.EqualTo(toInsert));
                Assert.That(items.Last(), Is.EqualTo(toInsert));
            }
        }
    }
}
