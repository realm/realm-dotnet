////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using NUnit.Framework;
using Realms.Exceptions;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class RealmSetTests : RealmInstanceTest
    {
        #region Boolean

        public static IEnumerable<TestCaseData<bool>> BoolTestValues()
        {
            yield return new TestCaseData<bool>(new bool[] { true }, new bool[] { false });
            yield return new TestCaseData<bool>(new bool[] { true, false }, new bool[] { true, false });
            yield return new TestCaseData<bool>(new bool[] { true }, Array.Empty<bool>());
            yield return new TestCaseData<bool>(Array.Empty<bool>(), new bool[] { false });
            yield return new TestCaseData<bool>(new bool[] { true, true, true, true, true }, new bool[] { true });
            yield return new TestCaseData<bool>(new bool[] { true, true, true, true }, new bool[] { true, false });
            yield return new TestCaseData<bool>(new bool[] { true, true, true, true, true, true, false, false, true }, new bool[] { false });
        }

        public static IEnumerable<TestCaseData<bool?>> NullableBoolTestValues()
        {
            yield return new TestCaseData<bool?>(new bool?[] { true }, new bool?[] { false });
            yield return new TestCaseData<bool?>(new bool?[] { true, false, null }, new bool?[] { true, false, null });
            yield return new TestCaseData<bool?>(new bool?[] { true }, Array.Empty<bool?>());
            yield return new TestCaseData<bool?>(Array.Empty<bool?>(), new bool?[] { null });
            yield return new TestCaseData<bool?>(new bool?[] { true, true, true }, new bool?[] { false, true, false, true, false });
            yield return new TestCaseData<bool?>(new bool?[] { true, true, true, true, true }, new bool?[] { true, true });
            yield return new TestCaseData<bool?>(new bool?[] { true, true, true, true, true, true }, new bool?[] { true, true, false });
            yield return new TestCaseData<bool?>(new bool?[] { true, true, true, true, true, true, false, false, true }, new bool?[] { true, true });
            yield return new TestCaseData<bool?>(new bool?[] { true, true, false, true, null, null, null }, new bool?[] { true, null });
            yield return new TestCaseData<bool?>(new bool?[] { null }, new bool?[] { null, null });
            yield return new TestCaseData<bool?>(new bool?[] { null, null }, new bool?[] { null, false });
        }

        [TestCaseSource(nameof(BoolTestValues))]
        public void RealmSet_WhenUnmanaged_Bool(TestCaseData<bool> testData)
        {
            RunUnmanagedTests(o => o.BooleanSet, testData);
        }

        [TestCaseSource(nameof(NullableBoolTestValues))]
        public void RealmSet_WhenUnmanaged_NullableBool(TestCaseData<bool?> testData)
        {
            RunUnmanagedTests(o => o.NullableBooleanSet, testData);
        }

        [TestCaseSource(nameof(BoolTestValues))]
        public void RealmSet_WhenManaged_Bool(TestCaseData<bool> testData)
        {
            RunManagedTests(o => o.BooleanSet, o => o.BooleanList, o => o.BooleanDict, testData);
        }

        [TestCaseSource(nameof(NullableBoolTestValues))]
        public void RealmSet_WhenManaged_NullableBool(TestCaseData<bool?> testData)
        {
            RunManagedTests(o => o.NullableBooleanSet, o => o.NullableBooleanList, o => o.NullableBooleanDict, testData);
        }

        [Test]
        public void RealmSet_WhenManaged_Bool_Notifications()
        {
            var testData = new TestCaseData<bool>(true);
            RunManagedNotificationsTests(o => o.BooleanSet, testData, newValue: false);
        }

        [Test]
        public void RealmSet_WhenManaged_NullableBool_Notifications()
        {
            var testData = new TestCaseData<bool?>(true);
            RunManagedNotificationsTests(o => o.NullableBooleanSet, testData, newValue: null);
        }

        #endregion

        #region Byte

        public static IEnumerable<TestCaseData<byte>> ByteTestValues()
        {
            yield return new TestCaseData<byte>(new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6 });
            yield return new TestCaseData<byte>(new byte[] { 1, 2, 3 }, new byte[] { 1, 2, 3 });
            yield return new TestCaseData<byte>(new byte[] { byte.MinValue, byte.MaxValue }, new byte[] { 0 });
            yield return new TestCaseData<byte>(new byte[] { 2, 0, 1 }, Array.Empty<byte>());
            yield return new TestCaseData<byte>(Array.Empty<byte>(), new byte[] { 0 });
            yield return new TestCaseData<byte>(new byte[] { 4, 6, 8 }, new byte[] { 12, 43, 2, 5, 6, 4, 8 });
            yield return new TestCaseData<byte>(new byte[] { 1, 1, 1, 1, 1, 1, 1 }, new byte[] { 1, 1, 1 });
            yield return new TestCaseData<byte>(new byte[] { 1, 1, 1, 1, 1, 1, 1 }, new byte[] { 1, 2, 1 });
            yield return new TestCaseData<byte>(new byte[] { 1, 2, 2, 1, 1, 1, 1 }, new byte[] { 1, 1, 1 });
        }

        public static IEnumerable<TestCaseData<byte?>> NullableByteTestValues()
        {
            yield return new TestCaseData<byte?>(new byte?[] { 1, 2, 3 }, new byte?[] { 4, 5, 6 });
            yield return new TestCaseData<byte?>(new byte?[] { 1, 2, 3, null }, new byte?[] { 1, 2, 3, null });
            yield return new TestCaseData<byte?>(new byte?[] { byte.MinValue, byte.MaxValue }, new byte?[] { 0 });
            yield return new TestCaseData<byte?>(new byte?[] { 2, 0, 1 }, Array.Empty<byte?>());
            yield return new TestCaseData<byte?>(Array.Empty<byte?>(), new byte?[] { 0 });
            yield return new TestCaseData<byte?>(new byte?[] { 4, 6, 8 }, new byte?[] { 12, 43, 2, 5, 6, 4, 8 });
            yield return new TestCaseData<byte?>(new byte?[] { 1, 1, 1, 1, 1, 1, 1 }, new byte?[] { 1, 1, 1 });
            yield return new TestCaseData<byte?>(new byte?[] { 1, 1, 1, 1, 1, 1, 1 }, new byte?[] { 1, 2, 1 });
            yield return new TestCaseData<byte?>(new byte?[] { 1, 2, 2, 1, 1, 1, 1 }, new byte?[] { 1, 1, 1 });
            yield return new TestCaseData<byte?>(new byte?[] { 1, 2, 2, 1, null, null, null }, new byte?[] { 1, 1, 1, null });
            yield return new TestCaseData<byte?>(new byte?[] { null }, new byte?[] { null, null });
            yield return new TestCaseData<byte?>(new byte?[] { null, null }, new byte?[] { null, 6 });
        }

        [TestCaseSource(nameof(ByteTestValues))]
        public void RealmSet_WhenUnmanaged_Byte(TestCaseData<byte> testData)
        {
            RunUnmanagedTests(o => o.ByteSet, testData);
        }

        [TestCaseSource(nameof(ByteTestValues))]
        public void RealmSet_WhenUnmanaged_ByteCounter(TestCaseData<byte> testData)
        {
            RunUnmanagedTests(o => o.ByteCounterSet, ToInteger(testData));
        }

        [TestCaseSource(nameof(NullableByteTestValues))]
        public void RealmSet_WhenUnmanaged_NullableByte(TestCaseData<byte?> testData)
        {
            RunUnmanagedTests(o => o.NullableByteSet, testData);
        }

        [TestCaseSource(nameof(NullableByteTestValues))]
        public void RealmSet_WhenUnmanaged_NullableByteCounter(TestCaseData<byte?> testData)
        {
            RunUnmanagedTests(o => o.NullableByteCounterSet, ToInteger(testData));
        }

        [TestCaseSource(nameof(ByteTestValues))]
        public void RealmSet_WhenManaged_Byte(TestCaseData<byte> testData)
        {
            RunManagedTests(o => o.ByteSet, o => o.ByteList, o => o.ByteDict, testData);
        }

        [TestCaseSource(nameof(ByteTestValues))]
        public void RealmSet_WhenManaged_ByteCounter(TestCaseData<byte> testData)
        {
            RunManagedTests(o => o.ByteCounterSet, o => o.ByteCounterList, o => o.ByteCounterDict, ToInteger(testData));
        }

        [TestCaseSource(nameof(NullableByteTestValues))]
        public void RealmSet_WhenManaged_NullableByte(TestCaseData<byte?> testData)
        {
            RunManagedTests(o => o.NullableByteSet, o => o.NullableByteList, o => o.NullableByteDict, testData);
        }

        [TestCaseSource(nameof(NullableByteTestValues))]
        public void RealmSet_WhenManaged_NullableByteCounter(TestCaseData<byte?> testData)
        {
            RunManagedTests(o => o.NullableByteCounterSet, o => o.NullableByteCounterList, o => o.NullableByteCounterDict, ToInteger(testData));
        }

        [Test]
        public void RealmSet_WhenManaged_Byte_Notifications()
        {
            var testData = new TestCaseData<byte>(123, 99);
            RunManagedNotificationsTests(o => o.ByteSet, testData, newValue: (byte)111);
        }

        [Test]
        public void RealmSet_WhenManaged_NullableByte_Notifications()
        {
            var testData = new TestCaseData<byte?>(123, null, 99);
            RunManagedNotificationsTests(o => o.NullableByteSet, testData, newValue: (byte)111);
        }

        #endregion

        #region Int16

        public static IEnumerable<TestCaseData<short>> Int16TestValues()
        {
            yield return new TestCaseData<short>(new short[] { 1, 2, 3 }, new short[] { 4, 5, 6 });
            yield return new TestCaseData<short>(new short[] { 1, 2, 3 }, new short[] { 1, 2, 3 });
            yield return new TestCaseData<short>(new short[] { short.MinValue, short.MaxValue }, new short[] { 0 });
            yield return new TestCaseData<short>(new short[] { -1, 0, 1 }, Array.Empty<short>());
            yield return new TestCaseData<short>(Array.Empty<short>(), new short[] { 0 });
            yield return new TestCaseData<short>(new short[] { 4, 6, 8 }, new short[] { 12, 43, 2, 5, 6, 4, 8 });
            yield return new TestCaseData<short>(new short[] { 1, 1, 1, 1, 1, 1, 1 }, new short[] { 1, 1, 1 });
            yield return new TestCaseData<short>(new short[] { 1, 1, 1, 1, 1, 1, 1 }, new short[] { 1, 2, 1 });
            yield return new TestCaseData<short>(new short[] { 1, 2, 2, 1, 1, 1, 1 }, new short[] { 1, 1, 1 });
        }

        public static IEnumerable<TestCaseData<short?>> NullableInt16TestValues()
        {
            yield return new TestCaseData<short?>(new short?[] { 1, 2, 3 }, new short?[] { 4, 5, 6 });
            yield return new TestCaseData<short?>(new short?[] { 1, 2, 3, null }, new short?[] { 1, 2, 3, null });
            yield return new TestCaseData<short?>(new short?[] { short.MinValue, short.MaxValue }, new short?[] { 0 });
            yield return new TestCaseData<short?>(new short?[] { -1, 0, 1 }, Array.Empty<short?>());
            yield return new TestCaseData<short?>(Array.Empty<short?>(), new short?[] { 0 });
            yield return new TestCaseData<short?>(new short?[] { 4, 6, 8 }, new short?[] { 12, 43, 2, 5, 6, 4, 8 });
            yield return new TestCaseData<short?>(new short?[] { 1, 1, 1, 1, 1, 1, 1 }, new short?[] { 1, 1, 1 });
            yield return new TestCaseData<short?>(new short?[] { 1, 1, 1, 1, 1, 1, 1 }, new short?[] { 1, 2, 1 });
            yield return new TestCaseData<short?>(new short?[] { 1, 2, 2, 1, 1, 1, 1 }, new short?[] { 1, 1, 1 });
            yield return new TestCaseData<short?>(new short?[] { 1, 2, 2, 1, null, null, null }, new short?[] { 1, 1, 1, null });
            yield return new TestCaseData<short?>(new short?[] { null }, new short?[] { null, null });
            yield return new TestCaseData<short?>(new short?[] { null, null }, new short?[] { null, 6 });
        }

        [TestCaseSource(nameof(Int16TestValues))]
        public void RealmSet_WhenUnmanaged_Int16(TestCaseData<short> testData)
        {
            RunUnmanagedTests(o => o.Int16Set, testData);
        }

        [TestCaseSource(nameof(Int16TestValues))]
        public void RealmSet_WhenUnmanaged_Int16Counter(TestCaseData<short> testData)
        {
            RunUnmanagedTests(o => o.Int16CounterSet, ToInteger(testData));
        }

        [TestCaseSource(nameof(NullableInt16TestValues))]
        public void RealmSet_WhenUnmanaged_NullableInt16(TestCaseData<short?> testData)
        {
            RunUnmanagedTests(o => o.NullableInt16Set, testData);
        }

        [TestCaseSource(nameof(NullableInt16TestValues))]
        public void RealmSet_WhenUnmanaged_NullableInt16Counter(TestCaseData<short?> testData)
        {
            RunUnmanagedTests(o => o.NullableInt16CounterSet, ToInteger(testData));
        }

        [TestCaseSource(nameof(Int16TestValues))]
        public void RealmSet_WhenManaged_Int16(TestCaseData<short> testData)
        {
            RunManagedTests(o => o.Int16Set, o => o.Int16List, o => o.Int16Dict, testData);
        }

        [TestCaseSource(nameof(Int16TestValues))]
        public void RealmSet_WhenManaged_Int16Counter(TestCaseData<short> testData)
        {
            RunManagedTests(o => o.Int16CounterSet, o => o.Int16CounterList, o => o.Int16CounterDict, ToInteger(testData));
        }

        [TestCaseSource(nameof(NullableInt16TestValues))]
        public void RealmSet_WhenManaged_NullableInt16(TestCaseData<short?> testData)
        {
            RunManagedTests(o => o.NullableInt16Set, o => o.NullableInt16List, o => o.NullableInt16Dict, testData);
        }

        [TestCaseSource(nameof(NullableInt16TestValues))]
        public void RealmSet_WhenManaged_NullableInt16Counter(TestCaseData<short?> testData)
        {
            RunManagedTests(o => o.NullableInt16CounterSet, o => o.NullableInt16CounterList, o => o.NullableInt16CounterDict, ToInteger(testData));
        }

        [Test]
        public void RealmSet_WhenManaged_Int16_Notifications()
        {
            var testData = new TestCaseData<short>(999, 99);
            RunManagedNotificationsTests(o => o.Int16Set, testData, newValue: (short)111);
        }

        [Test]
        public void RealmSet_WhenManaged_NullableInt16_Notifications()
        {
            var testData = new TestCaseData<short?>(999, null, 99);
            RunManagedNotificationsTests(o => o.NullableInt16Set, testData, newValue: (short)111);
        }

        #endregion

        #region Int32

        public static IEnumerable<TestCaseData<int>> Int32TestValues()
        {
            yield return new TestCaseData<int>(new int[] { 1, 2, 3 }, new int[] { 4, 5, 6 });
            yield return new TestCaseData<int>(new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 });
            yield return new TestCaseData<int>(new int[] { int.MinValue, int.MaxValue }, new int[] { 0 });
            yield return new TestCaseData<int>(new int[] { -1, 0, 1 }, Array.Empty<int>());
            yield return new TestCaseData<int>(Array.Empty<int>(), new int[] { 0 });
            yield return new TestCaseData<int>(new int[] { 4, 6, 8 }, new int[] { 12, 43, 2, 5, 6, 4, 8 });
            yield return new TestCaseData<int>(new int[] { 1, 1, 1, 1, 1, 1, 1 }, new int[] { 1, 1, 1 });
            yield return new TestCaseData<int>(new int[] { 1, 1, 1, 1, 1, 1, 1 }, new int[] { 1, 2, 1 });
            yield return new TestCaseData<int>(new int[] { 1, 2, 2, 1, 1, 1, 1 }, new int[] { 1, 1, 1 });
        }

        public static IEnumerable<TestCaseData<int?>> NullableInt32TestValues()
        {
            yield return new TestCaseData<int?>(new int?[] { 1, 2, 3 }, new int?[] { 4, 5, 6 });
            yield return new TestCaseData<int?>(new int?[] { 1, 2, 3, null }, new int?[] { 1, 2, 3, null });
            yield return new TestCaseData<int?>(new int?[] { int.MinValue, int.MaxValue }, new int?[] { 0 });
            yield return new TestCaseData<int?>(new int?[] { -1, 0, 1 }, Array.Empty<int?>());
            yield return new TestCaseData<int?>(Array.Empty<int?>(), new int?[] { 0 });
            yield return new TestCaseData<int?>(new int?[] { 4, 6, 8 }, new int?[] { 12, 43, 2, 5, 6, 4, 8 });
            yield return new TestCaseData<int?>(new int?[] { 1, 1, 1, 1, 1, 1, 1 }, new int?[] { 1, 1, 1 });
            yield return new TestCaseData<int?>(new int?[] { 1, 1, 1, 1, 1, 1, 1 }, new int?[] { 1, 2, 1 });
            yield return new TestCaseData<int?>(new int?[] { 1, 2, 2, 1, 1, 1, 1 }, new int?[] { 1, 1, 1 });
            yield return new TestCaseData<int?>(new int?[] { 1, 2, 2, 1, null, null, null }, new int?[] { 1, 1, 1, null });
            yield return new TestCaseData<int?>(new int?[] { null }, new int?[] { null, null });
            yield return new TestCaseData<int?>(new int?[] { null, null }, new int?[] { null, 6 });
        }

        [TestCaseSource(nameof(Int32TestValues))]
        public void RealmSet_WhenUnmanaged_Int32(TestCaseData<int> testData)
        {
            RunUnmanagedTests(o => o.Int32Set, testData);
        }

        [TestCaseSource(nameof(Int32TestValues))]
        public void RealmSet_WhenUnmanaged_Int32Counter(TestCaseData<int> testData)
        {
            RunUnmanagedTests(o => o.Int32CounterSet, ToInteger(testData));
        }

        [TestCaseSource(nameof(NullableInt32TestValues))]
        public void RealmSet_WhenUnmanaged_NullableInt32(TestCaseData<int?> testData)
        {
            RunUnmanagedTests(o => o.NullableInt32Set, testData);
        }

        [TestCaseSource(nameof(NullableInt32TestValues))]
        public void RealmSet_WhenUnmanaged_NullableInt32Counter(TestCaseData<int?> testData)
        {
            RunUnmanagedTests(o => o.NullableInt32CounterSet, ToInteger(testData));
        }

        [TestCaseSource(nameof(Int32TestValues))]
        public void RealmSet_WhenManaged_Int32(TestCaseData<int> testData)
        {
            RunManagedTests(o => o.Int32Set, o => o.Int32List, o => o.Int32Dict, testData);
        }

        [TestCaseSource(nameof(Int32TestValues))]
        public void RealmSet_WhenManaged_Int32Counter(TestCaseData<int> testData)
        {
            RunManagedTests(o => o.Int32CounterSet, o => o.Int32CounterList, o => o.Int32CounterDict, ToInteger(testData));
        }

        [TestCaseSource(nameof(NullableInt32TestValues))]
        public void RealmSet_WhenManaged_NullableInt32(TestCaseData<int?> testData)
        {
            RunManagedTests(o => o.NullableInt32Set, o => o.NullableInt32List, o => o.NullableInt32Dict, testData);
        }

        [TestCaseSource(nameof(NullableInt32TestValues))]
        public void RealmSet_WhenManaged_NullableInt32Counter(TestCaseData<int?> testData)
        {
            RunManagedTests(o => o.NullableInt32CounterSet, o => o.NullableInt32CounterList, o => o.NullableInt32CounterDict, ToInteger(testData));
        }

        [Test]
        public void RealmSet_WhenManaged_Int32_Notifications()
        {
            var testData = new TestCaseData<int>(123456789, 99);
            RunManagedNotificationsTests(o => o.Int32Set, testData, newValue: int.MinValue);
        }

        [Test]
        public void RealmSet_WhenManaged_NullableInt32_Notifications()
        {
            var testData = new TestCaseData<int?>(123, null, 99);
            RunManagedNotificationsTests(o => o.NullableInt32Set, testData, newValue: int.MaxValue);
        }

        #endregion

        #region Int64

        public static IEnumerable<TestCaseData<long>> Int64TestValues()
        {
            yield return new TestCaseData<long>(new long[] { 1, 2, 3 }, new long[] { 4, 5, 6 });
            yield return new TestCaseData<long>(new long[] { 1, 2, 3 }, new long[] { 1, 2, 3 });
            yield return new TestCaseData<long>(new long[] { long.MinValue, long.MaxValue }, new long[] { 0 });
            yield return new TestCaseData<long>(new long[] { -1, 0, 1 }, Array.Empty<long>());
            yield return new TestCaseData<long>(Array.Empty<long>(), new long[] { 0 });
            yield return new TestCaseData<long>(new long[] { 4, 6, 8 }, new long[] { 12, 43, 2, 5, 6, 4, 8 });
            yield return new TestCaseData<long>(new long[] { 1, 1, 1, 1, 1, 1, 1 }, new long[] { 1, 1, 1 });
            yield return new TestCaseData<long>(new long[] { 1, 1, 1, 1, 1, 1, 1 }, new long[] { 1, 2, 1 });
            yield return new TestCaseData<long>(new long[] { 1, 2, 2, 1, 1, 1, 1 }, new long[] { 1, 1, 1 });
        }

        public static IEnumerable<TestCaseData<long?>> NullableInt64TestValues()
        {
            yield return new TestCaseData<long?>(new long?[] { 1, 2, 3 }, new long?[] { 4, 5, 6 });
            yield return new TestCaseData<long?>(new long?[] { 1, 2, 3, null }, new long?[] { 1, 2, 3, null });
            yield return new TestCaseData<long?>(new long?[] { long.MinValue, long.MaxValue }, new long?[] { 0 });
            yield return new TestCaseData<long?>(new long?[] { -1, 0, 1 }, Array.Empty<long?>());
            yield return new TestCaseData<long?>(Array.Empty<long?>(), new long?[] { 0 });
            yield return new TestCaseData<long?>(new long?[] { 4, 6, 8 }, new long?[] { 12, 43, 2, 5, 6, 4, 8 });
            yield return new TestCaseData<long?>(new long?[] { 1, 1, 1, 1, 1, 1, 1 }, new long?[] { 1, 1, 1 });
            yield return new TestCaseData<long?>(new long?[] { 1, 1, 1, 1, 1, 1, 1 }, new long?[] { 1, 2, 1 });
            yield return new TestCaseData<long?>(new long?[] { 1, 2, 2, 1, 1, 1, 1 }, new long?[] { 1, 1, 1 });
            yield return new TestCaseData<long?>(new long?[] { 1, 2, 2, 1, null, null, null }, new long?[] { 1, 1, 1, null });
            yield return new TestCaseData<long?>(new long?[] { null }, new long?[] { null, null });
            yield return new TestCaseData<long?>(new long?[] { null, null }, new long?[] { null, 6 });
        }

        [TestCaseSource(nameof(Int64TestValues))]
        public void RealmSet_WhenUnmanaged_Int64(TestCaseData<long> testData)
        {
            RunUnmanagedTests(o => o.Int64Set, testData);
        }

        [TestCaseSource(nameof(Int64TestValues))]
        public void RealmSet_WhenUnmanaged_Int64Counter(TestCaseData<long> testData)
        {
            RunUnmanagedTests(o => o.Int64CounterSet, ToInteger(testData));
        }

        [TestCaseSource(nameof(NullableInt64TestValues))]
        public void RealmSet_WhenUnmanaged_NullableInt64(TestCaseData<long?> testData)
        {
            RunUnmanagedTests(o => o.NullableInt64Set, testData);
        }

        [TestCaseSource(nameof(NullableInt64TestValues))]
        public void RealmSet_WhenUnmanaged_NullableInt64Counter(TestCaseData<long?> testData)
        {
            RunUnmanagedTests(o => o.NullableInt64CounterSet, ToInteger(testData));
        }

        [TestCaseSource(nameof(Int64TestValues))]
        public void RealmSet_WhenManaged_Int64(TestCaseData<long> testData)
        {
            RunManagedTests(o => o.Int64Set, o => o.Int64List, o => o.Int64Dict, testData);
        }

        [TestCaseSource(nameof(Int64TestValues))]
        public void RealmSet_WhenManaged_Int64Counter(TestCaseData<long> testData)
        {
            RunManagedTests(o => o.Int64CounterSet, o => o.Int64CounterList, o => o.Int64CounterDict, ToInteger(testData));
        }

        [TestCaseSource(nameof(NullableInt64TestValues))]
        public void RealmSet_WhenManaged_NullableInt64(TestCaseData<long?> testData)
        {
            RunManagedTests(o => o.NullableInt64Set, o => o.NullableInt64List, o => o.NullableInt64Dict, testData);
        }

        [TestCaseSource(nameof(NullableInt64TestValues))]
        public void RealmSet_WhenManaged_NullableInt64Counter(TestCaseData<long?> testData)
        {
            RunManagedTests(o => o.NullableInt64CounterSet, o => o.NullableInt64CounterList, o => o.NullableInt64CounterDict, ToInteger(testData));
        }

        [Test]
        public void RealmSet_WhenManaged_Int64_Notifications()
        {
            var testData = new TestCaseData<long>(123, 99);
            RunManagedNotificationsTests(o => o.Int64Set, testData, newValue: long.MinValue);
        }

        [Test]
        public void RealmSet_WhenManaged_NullableInt64_Notifications()
        {
            var testData = new TestCaseData<long?>(123, null, 99);
            RunManagedNotificationsTests(o => o.NullableInt64Set, testData, newValue: long.MaxValue);
        }

        #endregion

        #region Float

        public static IEnumerable<TestCaseData<float>> FloatTestValues()
        {
            yield return new TestCaseData<float>(new float[] { 1.1f, 2.2f, 3.3f }, new float[] { 4.4f, 5.5f, 6.6f });
            yield return new TestCaseData<float>(new float[] { 1, 2.3f, 3 }, new float[] { 1, 2.3f, 3 });
            yield return new TestCaseData<float>(new float[] { float.MinValue, float.MaxValue }, new float[] { 0 });
            yield return new TestCaseData<float>(new float[] { -1, 0, 1.5f }, Array.Empty<float>());
            yield return new TestCaseData<float>(Array.Empty<float>(), new float[] { 0 });
            yield return new TestCaseData<float>(new float[] { 4, 6.6f, 8 }, new float[] { 12, 43, 2.2f, 5, 6.6f, 4, 8 });
            yield return new TestCaseData<float>(new float[] { 1, 1, 1, 1, 1, 1, 1.0f }, new float[] { 1, 1, 1 });
            yield return new TestCaseData<float>(new float[] { 1, 1, 1, 1, 1, 1, 1 }, new float[] { 1.0f, 2, 1.0f });
            yield return new TestCaseData<float>(new float[] { 1, 2f, 2f, 1, 1, 1, 1 }, new float[] { 1, 1f, 1 });
        }

        public static IEnumerable<TestCaseData<float?>> NullableFloatTestValues()
        {
            yield return new TestCaseData<float?>(new float?[] { 1.1f, 2.2f, 3.3f }, new float?[] { 1.1f, 2.2f, 3.3f });
            yield return new TestCaseData<float?>(new float?[] { 1, 2.3f, 3, null }, new float?[] { 1, 2.3f, 3, null });
            yield return new TestCaseData<float?>(new float?[] { float.MinValue, float.MaxValue }, new float?[] { 0 });
            yield return new TestCaseData<float?>(new float?[] { -1, 0, 1.5f }, Array.Empty<float?>());
            yield return new TestCaseData<float?>(Array.Empty<float?>(), new float?[] { 0 });
            yield return new TestCaseData<float?>(new float?[] { 4, 6.6f, 8 }, new float?[] { 12, 43, 2.2f, 5, 6.6f, 4, 8 });
            yield return new TestCaseData<float?>(new float?[] { 1, 1, 1, 1, 1, 1, 1.0f }, new float?[] { 1, 1, 1 });
            yield return new TestCaseData<float?>(new float?[] { 1, 1, 1, 1, 1, 1, 1 }, new float?[] { 1.0f, 2, 1.0f });
            yield return new TestCaseData<float?>(new float?[] { 1, 2, 2, 1, 1, 1, 1 }, new float?[] { 1f, 1f, 1f });
            yield return new TestCaseData<float?>(new float?[] { 1, 2, 2, 1, null, null, null }, new float?[] { 1.0f, 1.0f, 1.0f, null });
            yield return new TestCaseData<float?>(new float?[] { null }, new float?[] { null, null });
            yield return new TestCaseData<float?>(new float?[] { null, null }, new float?[] { null, 6 });
        }

        [TestCaseSource(nameof(FloatTestValues))]
        public void RealmSet_WhenUnmanaged_Float(TestCaseData<float> testData)
        {
            RunUnmanagedTests(o => o.SingleSet, testData);
        }

        [TestCaseSource(nameof(NullableFloatTestValues))]
        public void RealmSet_WhenUnmanaged_NullableFloat(TestCaseData<float?> testData)
        {
            RunUnmanagedTests(o => o.NullableSingleSet, testData);
        }

        [TestCaseSource(nameof(FloatTestValues))]
        public void RealmSet_WhenManaged_Float(TestCaseData<float> testData)
        {
            RunManagedTests(o => o.SingleSet, o => o.SingleList, o => o.SingleDict, testData);
        }

        [TestCaseSource(nameof(NullableFloatTestValues))]
        public void RealmSet_WhenManaged_NullableFloat(TestCaseData<float?> testData)
        {
            RunManagedTests(o => o.NullableSingleSet, o => o.NullableSingleList, o => o.NullableSingleDict, testData);
        }

        [Test]
        public void RealmSet_WhenManaged_Float_Notifications()
        {
            var testData = new TestCaseData<float>(123.456f, 99);
            RunManagedNotificationsTests(o => o.SingleSet, testData, newValue: float.MinValue);
        }

        [Test]
        public void RealmSet_WhenManaged_NullableFloat_Notifications()
        {
            var testData = new TestCaseData<float?>(123.567f, null, 99);
            RunManagedNotificationsTests(o => o.NullableSingleSet, testData, newValue: float.MaxValue);
        }

        #endregion

        #region Double

        public static IEnumerable<TestCaseData<double>> DoubleTestValues()
        {
            yield return new TestCaseData<double>(new double[] { 1.1, 2.2, 3.3 }, new double[] { 4.4, 5.5, 6.6 });
            yield return new TestCaseData<double>(new double[] { 1, 2.3, 3 }, new double[] { 1, 2.3, 3 });
            yield return new TestCaseData<double>(new double[] { double.MinValue, double.MaxValue }, new double[] { 0 });
            yield return new TestCaseData<double>(new double[] { -1, 0, 1.5 }, Array.Empty<double>());
            yield return new TestCaseData<double>(Array.Empty<double>(), new double[] { 0 });
            yield return new TestCaseData<double>(new double[] { 4, 6.6, 8 }, new double[] { 12, 43, 2.2, 5, 6.6, 4, 8 });
            yield return new TestCaseData<double>(new double[] { 1, 1, 1, 1, 1, 1, 1.0 }, new double[] { 1, 1, 1 });
            yield return new TestCaseData<double>(new double[] { 1, 1, 1, 1, 1, 1, 1 }, new double[] { 1.0, 2, 1.0 });
            yield return new TestCaseData<double>(new double[] { 1, 2, 2, 1, 1, 1, 1 }, new double[] { 1, 1, 1 });
        }

        public static IEnumerable<TestCaseData<double?>> NullableDoubleTestValues()
        {
            yield return new TestCaseData<double?>(new double?[] { 1.1, 2.2, 3.3 }, new double?[] { 1.1, 2.2, 3.3 });
            yield return new TestCaseData<double?>(new double?[] { 1, 2.3, 3, null }, new double?[] { 1, 2.3, 3, null });
            yield return new TestCaseData<double?>(new double?[] { double.MinValue, double.MaxValue }, new double?[] { 0 });
            yield return new TestCaseData<double?>(new double?[] { -1, 0, 1.5 }, Array.Empty<double?>());
            yield return new TestCaseData<double?>(Array.Empty<double?>(), new double?[] { 0 });
            yield return new TestCaseData<double?>(new double?[] { 4, 6.6, 8 }, new double?[] { 12, 43, 2.2, 5, 6.6, 4, 8 });
            yield return new TestCaseData<double?>(new double?[] { 1, 1, 1, 1, 1, 1, 1.0 }, new double?[] { 1, 1, 1 });
            yield return new TestCaseData<double?>(new double?[] { 1, 1, 1, 1, 1, 1, 1 }, new double?[] { 1.0, 2, 1.0 });
            yield return new TestCaseData<double?>(new double?[] { 1, 2, 2, 1, 1, 1, 1 }, new double?[] { 1, 1, 1 });
            yield return new TestCaseData<double?>(new double?[] { 1, 2, 2, 1, null, null, null }, new double?[] { 1.0, 1.0, 1.0, null });
            yield return new TestCaseData<double?>(new double?[] { null }, new double?[] { null, null });
            yield return new TestCaseData<double?>(new double?[] { null, null }, new double?[] { null, 6 });
        }

        [TestCaseSource(nameof(DoubleTestValues))]
        public void RealmSet_WhenUnmanaged_Double(TestCaseData<double> testData)
        {
            RunUnmanagedTests(o => o.DoubleSet, testData);
        }

        [TestCaseSource(nameof(NullableDoubleTestValues))]
        public void RealmSet_WhenUnmanaged_NullableDouble(TestCaseData<double?> testData)
        {
            RunUnmanagedTests(o => o.NullableDoubleSet, testData);
        }

        [TestCaseSource(nameof(DoubleTestValues))]
        public void RealmSet_WhenManaged_Double(TestCaseData<double> testData)
        {
            RunManagedTests(o => o.DoubleSet, o => o.DoubleList, o => o.DoubleDict, testData);
        }

        [TestCaseSource(nameof(NullableDoubleTestValues))]
        public void RealmSet_WhenManaged_NullableDouble(TestCaseData<double?> testData)
        {
            RunManagedTests(o => o.NullableDoubleSet, o => o.NullableDoubleList, o => o.NullableDoubleDict, testData);
        }

        [Test]
        public void RealmSet_WhenManaged_Double_Notifications()
        {
            var testData = new TestCaseData<double>(123.9999, 99);
            RunManagedNotificationsTests(o => o.DoubleSet, testData, newValue: double.MinValue);
        }

        [Test]
        public void RealmSet_WhenManaged_NullableDouble_Notifications()
        {
            var testData = new TestCaseData<double?>(123.1111, null, 99);
            RunManagedNotificationsTests(o => o.NullableDoubleSet, testData, newValue: double.MaxValue);
        }

        #endregion

        #region Decimal

        public static IEnumerable<TestCaseData<decimal>> DecimalTestValues()
        {
            yield return new TestCaseData<decimal>(new decimal[] { 1.1m, 2.2m, 3.3m }, new decimal[] { 4.4m, 5.5m, 6.6m });
            yield return new TestCaseData<decimal>(new decimal[] { 1, 2.3m, 3 }, new decimal[] { 1, 2.3m, 3 });
            yield return new TestCaseData<decimal>(new decimal[] { decimal.MinValue, decimal.MaxValue }, new decimal[] { 0 });
            yield return new TestCaseData<decimal>(new decimal[] { -1, 0, 1.5m }, Array.Empty<decimal>());
            yield return new TestCaseData<decimal>(Array.Empty<decimal>(), new decimal[] { 0 });
            yield return new TestCaseData<decimal>(new decimal[] { 4, 6.6m, 8 }, new decimal[] { 12, 43, 2.2m, 5, 6.6m, 4, 8 });
            yield return new TestCaseData<decimal>(new decimal[] { 1, 1, 1, 1, 1, 1, 1.0m }, new decimal[] { 1, 1, 1 });
            yield return new TestCaseData<decimal>(new decimal[] { 1, 1, 1, 1, 1, 1, 1 }, new decimal[] { 1.0m, 2, 1.0m });
            yield return new TestCaseData<decimal>(new decimal[] { 1, 2, 2, 1, 1, 1, 1 }, new decimal[] { 1, 1, 1 });
            yield return new TestCaseData<decimal>(new decimal[] { 1.9357683758257382523m }, new decimal[] { 1.9357683758257382524m, 1.9357683758257382522m });
            yield return new TestCaseData<decimal>(new decimal[] { 1.9357683758257382523m, 3.5743857348m, 8.75878832943928m }, new decimal[] { 1.9357683758257382523m, 8.75878832943928m });
        }

        public static IEnumerable<TestCaseData<decimal?>> NullableDecimalTestValues()
        {
            yield return new TestCaseData<decimal?>(new decimal?[] { 1.1m, 2.2m, 3.3m }, new decimal?[] { 4.4m, 5.5m, 6.6m });
            yield return new TestCaseData<decimal?>(new decimal?[] { 1, 2.3m, 3, null }, new decimal?[] { 1, 2.3m, 3, null });
            yield return new TestCaseData<decimal?>(new decimal?[] { decimal.MinValue, decimal.MaxValue }, new decimal?[] { 0 });
            yield return new TestCaseData<decimal?>(new decimal?[] { -1, 0, 1.5m }, Array.Empty<decimal?>());
            yield return new TestCaseData<decimal?>(Array.Empty<decimal?>(), new decimal?[] { 0 });
            yield return new TestCaseData<decimal?>(new decimal?[] { 4, 6.6m, 8 }, new decimal?[] { 12, 43, 2.2m, 5, 6.6m, 4, 8 });
            yield return new TestCaseData<decimal?>(new decimal?[] { 1, 1, 1, 1, 1, 1, 1.0m }, new decimal?[] { 1, 1, 1 });
            yield return new TestCaseData<decimal?>(new decimal?[] { 1, 1, 1, 1, 1, 1, 1 }, new decimal?[] { 1.0m, 2, 1.0m });
            yield return new TestCaseData<decimal?>(new decimal?[] { 1, 2, 2, 1, 1, 1, 1 }, new decimal?[] { 1, 1, 1 });
            yield return new TestCaseData<decimal?>(new decimal?[] { 1, 2, 2, 1, null, null, null }, new decimal?[] { 1.0m, 1.0m, 1.0m, null });
            yield return new TestCaseData<decimal?>(new decimal?[] { null }, new decimal?[] { null, null });
            yield return new TestCaseData<decimal?>(new decimal?[] { null, null }, new decimal?[] { null, 6 });
            yield return new TestCaseData<decimal?>(new decimal?[] { 1.9357683758257382523m }, new decimal?[] { 1.9357683758257382524m, 1.9357683758257382522m });
            yield return new TestCaseData<decimal?>(new decimal?[] { 1.9357683758257382523m, null, 8.75878832943928m }, new decimal?[] { 1.9357683758257382523m, 8.75878832943928m });
        }

        [TestCaseSource(nameof(DecimalTestValues))]
        public void RealmSet_WhenUnmanaged_Decimal(TestCaseData<decimal> testData)
        {
            RunUnmanagedTests(o => o.DecimalSet, testData);
        }

        [TestCaseSource(nameof(NullableDecimalTestValues))]
        public void RealmSet_WhenUnmanaged_NullableDecimal(TestCaseData<decimal?> testData)
        {
            RunUnmanagedTests(o => o.NullableDecimalSet, testData);
        }

        [TestCaseSource(nameof(DecimalTestValues))]
        public void RealmSet_WhenManaged_Decimal(TestCaseData<decimal> testData)
        {
            RunManagedTests(o => o.DecimalSet, o => o.DecimalList, o => o.DecimalDict, testData);
        }

        [TestCaseSource(nameof(NullableDecimalTestValues))]
        public void RealmSet_WhenManaged_NullableDecimal(TestCaseData<decimal?> testData)
        {
            RunManagedTests(o => o.NullableDecimalSet, o => o.NullableDecimalList, o => o.NullableDecimalDict, testData);
        }

        [Test]
        public void RealmSet_WhenManaged_Decimal_Notifications()
        {
            var testData = new TestCaseData<decimal>(123.7777777m, 99);
            RunManagedNotificationsTests(o => o.DecimalSet, testData, newValue: decimal.MinValue);
        }

        [Test]
        public void RealmSet_WhenManaged_NullableDecimal_Notifications()
        {
            var testData = new TestCaseData<decimal?>(123.999999999m, null, 99);
            RunManagedNotificationsTests(o => o.NullableDecimalSet, testData, newValue: long.MaxValue);
        }

        #endregion

        #region Decimal128

        public static IEnumerable<TestCaseData<Decimal128>> Decimal128TestValues()
        {
            yield return new TestCaseData<Decimal128>(new Decimal128[] { 1.1m, 2.2m, 3.3m }, new Decimal128[] { 4.4m, 5.5m, 6.6m });
            yield return new TestCaseData<Decimal128>(new Decimal128[] { 1, 2.3m, 3 }, new Decimal128[] { 1, 2.3m, 3 });
            yield return new TestCaseData<Decimal128>(new Decimal128[] { Decimal128.MinValue, Decimal128.MaxValue }, new Decimal128[] { 0 });
            yield return new TestCaseData<Decimal128>(new Decimal128[] { -1, 0, 1.5m }, Array.Empty<Decimal128>());
            yield return new TestCaseData<Decimal128>(Array.Empty<Decimal128>(), new Decimal128[] { 0 });
            yield return new TestCaseData<Decimal128>(new Decimal128[] { 4, 6.6m, 8 }, new Decimal128[] { 12, 43, 2.2m, 5, 6.6m, 4, 8 });

            // There is a bug in the Decimal128 implementation of GetHashCode, so we can't use the tests below. https://jira.mongodb.org/browse/CSHARP-3288
            // Once fixed, we should uncomment.
            // yield return new TestCaseData<Decimal128>(new Decimal128[] { 1, 1, 1, 1, 1, 1, 1.0m }, new Decimal128[] { 1, 1, 1 });
            // yield return new TestCaseData<Decimal128>(new Decimal128[] { 1, 1, 1, 1, 1, 1, 1 }, new Decimal128[] { 1.0m, 2, 1.0m });
            yield return new TestCaseData<Decimal128>(new Decimal128[] { 1, 1, 1, 1, 1, 1, 1 }, new Decimal128[] { 1, 1, 1 });
            yield return new TestCaseData<Decimal128>(new Decimal128[] { 1, 1, 1, 1, 1, 1, 1 }, new Decimal128[] { 1, 2, 1 });
            yield return new TestCaseData<Decimal128>(new Decimal128[] { 1, 2, 2, 1, 1, 1, 1 }, new Decimal128[] { 1, 1, 1 });
            yield return new TestCaseData<Decimal128>(new Decimal128[] { 1.9357683758257382523m }, new Decimal128[] { 1.9357683758257382524m, 1.9357683758257382522m });
            yield return new TestCaseData<Decimal128>(new Decimal128[] { 1.9357683758257382523m, 3.5743857348m, 8.75878832943928m }, new Decimal128[] { 1.9357683758257382523m, 8.75878832943928m });
        }

        public static IEnumerable<TestCaseData<Decimal128?>> NullableDecimal128TestValues()
        {
            yield return new TestCaseData<Decimal128?>(new Decimal128?[] { 1.1m, 2.2m, 3.3m }, new Decimal128?[] { 4.4m, 5.5m, 6.6m });
            yield return new TestCaseData<Decimal128?>(new Decimal128?[] { 1, 2.3m, 3, null }, new Decimal128?[] { 1, 2.3m, 3, null });
            yield return new TestCaseData<Decimal128?>(new Decimal128?[] { Decimal128.MinValue, Decimal128.MaxValue }, new Decimal128?[] { 0 });
            yield return new TestCaseData<Decimal128?>(new Decimal128?[] { -1, 0, 1.5m }, Array.Empty<Decimal128?>());
            yield return new TestCaseData<Decimal128?>(Array.Empty<Decimal128?>(), new Decimal128?[] { 0 });
            yield return new TestCaseData<Decimal128?>(new Decimal128?[] { 4, 6.6m, 8 }, new Decimal128?[] { 12, 43, 2.2m, 5, 6.6m, 4, 8 });

            // There is a bug in the Decimal128 implementation of GetHashCode, so we can't use the tests below. https://jira.mongodb.org/browse/CSHARP-3288
            // Once fixed, we should uncomment.
            // yield return new TestCaseData<Decimal128?>(new Decimal128?[] { 1, 1, 1, 1, 1, 1, 1.0m }, new Decimal128?[] { 1, 1, 1 });
            // yield return new TestCaseData<Decimal128?>(new Decimal128?[] { 1, 1, 1, 1, 1, 1, 1 }, new Decimal128?[] { 1.0m, 2, 1.0m });
            // yield return new TestCaseData<Decimal128?>(new Decimal128?[] { 1, 2, 2, 1, null, null, null }, new Decimal128?[] { 1.0m, 1.0m, 1.0m, null });
            yield return new TestCaseData<Decimal128?>(new Decimal128?[] { 1, 1, 1, 1, 1, 1, 1 }, new Decimal128?[] { 1, 1, 1 });
            yield return new TestCaseData<Decimal128?>(new Decimal128?[] { 1, 1, 1, 1, 1, 1, 1 }, new Decimal128?[] { 1, 2, 1 });
            yield return new TestCaseData<Decimal128?>(new Decimal128?[] { 1, 2, 2, 1, null, null, null }, new Decimal128?[] { 1, 1, 1, null });
            yield return new TestCaseData<Decimal128?>(new Decimal128?[] { 1, 2, 2, 1, 1, 1, 1 }, new Decimal128?[] { 1, 1, 1 });
            yield return new TestCaseData<Decimal128?>(new Decimal128?[] { null }, new Decimal128?[] { null, null });
            yield return new TestCaseData<Decimal128?>(new Decimal128?[] { null, null }, new Decimal128?[] { null, 6 });
            yield return new TestCaseData<Decimal128?>(new Decimal128?[] { 1.9357683758257382523m }, new Decimal128?[] { 1.9357683758257382524m, 1.9357683758257382522m });
            yield return new TestCaseData<Decimal128?>(new Decimal128?[] { 1.9357683758257382523m, null, 8.75878832943928m }, new Decimal128?[] { 1.9357683758257382523m, 8.75878832943928m });
        }

        [TestCaseSource(nameof(Decimal128TestValues))]
        public void RealmSet_WhenUnmanaged_Decimal128(TestCaseData<Decimal128> testData)
        {
            RunUnmanagedTests(o => o.Decimal128Set, testData);
        }

        [TestCaseSource(nameof(NullableDecimal128TestValues))]
        public void RealmSet_WhenUnmanaged_NullableDecimal128(TestCaseData<Decimal128?> testData)
        {
            RunUnmanagedTests(o => o.NullableDecimal128Set, testData);
        }

        [TestCaseSource(nameof(Decimal128TestValues))]
        public void RealmSet_WhenManaged_Decimal128(TestCaseData<Decimal128> testData)
        {
            RunManagedTests(o => o.Decimal128Set, o => o.Decimal128List, o => o.Decimal128Dict, testData);
        }

        [TestCaseSource(nameof(NullableDecimal128TestValues))]
        public void RealmSet_WhenManaged_NullableDecimal128(TestCaseData<Decimal128?> testData)
        {
            RunManagedTests(o => o.NullableDecimal128Set, o => o.NullableDecimal128List, o => o.NullableDecimal128Dict, testData);
        }

        [Test]
        public void RealmSet_WhenManaged_Decimal128_Notifications()
        {
            var testData = new TestCaseData<Decimal128>(123.9999999999m, 99);
            RunManagedNotificationsTests(o => o.Decimal128Set, testData, newValue: Decimal128.MinValue);
        }

        [Test]
        public void RealmSet_WhenManaged_NullableDecimal128_Notifications()
        {
            var testData = new TestCaseData<Decimal128?>(123, null, 99.123456789m);
            RunManagedNotificationsTests(o => o.NullableDecimal128Set, testData, newValue: Decimal128.MaxValue);
        }

        #endregion

        #region ObjectId

        private static readonly ObjectId ObjectId0 = TestHelpers.GenerateRepetitiveObjectId(0);
        private static readonly ObjectId ObjectId1 = TestHelpers.GenerateRepetitiveObjectId(1);
        private static readonly ObjectId ObjectId2 = TestHelpers.GenerateRepetitiveObjectId(2);
        private static readonly ObjectId ObjectId3 = TestHelpers.GenerateRepetitiveObjectId(3);
        private static readonly ObjectId ObjectId4 = TestHelpers.GenerateRepetitiveObjectId(4);
        private static readonly ObjectId ObjectId5 = TestHelpers.GenerateRepetitiveObjectId(5);
        private static readonly ObjectId ObjectId6 = TestHelpers.GenerateRepetitiveObjectId(6);
        private static readonly ObjectId ObjectIdMax = TestHelpers.GenerateRepetitiveObjectId(3);

        public static IEnumerable<TestCaseData<ObjectId>> ObjectIdTestValues()
        {
            yield return new TestCaseData<ObjectId>(new ObjectId[] { ObjectId1, ObjectId2, ObjectId3 }, new ObjectId[] { ObjectId4, ObjectId5, ObjectId6 });
            yield return new TestCaseData<ObjectId>(new ObjectId[] { ObjectId1, ObjectId2, ObjectId3 }, new ObjectId[] { ObjectId1, ObjectId2, ObjectId3 });
            yield return new TestCaseData<ObjectId>(new ObjectId[] { ObjectIdMax }, new ObjectId[] { ObjectId0 });
            yield return new TestCaseData<ObjectId>(new ObjectId[] { ObjectId0, ObjectId1, ObjectIdMax }, Array.Empty<ObjectId>());
            yield return new TestCaseData<ObjectId>(Array.Empty<ObjectId>(), new ObjectId[] { ObjectId0 });
            yield return new TestCaseData<ObjectId>(new ObjectId[] { ObjectId1, ObjectId2 }, new ObjectId[] { ObjectIdMax, ObjectId.GenerateNewId(), ObjectId1, ObjectId2, ObjectId3, ObjectId2 });
            yield return new TestCaseData<ObjectId>(new ObjectId[] { ObjectId1, ObjectId1, ObjectId1, ObjectId1, ObjectId1, ObjectId1, ObjectId1 }, new ObjectId[] { ObjectId1, ObjectId1 });
            yield return new TestCaseData<ObjectId>(new ObjectId[] { ObjectId1, ObjectId1, ObjectId1, ObjectId1, ObjectId1, ObjectId1, ObjectId1 }, new ObjectId[] { ObjectId1, ObjectId2 });
            yield return new TestCaseData<ObjectId>(new ObjectId[] { ObjectId1, ObjectId1, ObjectId1, ObjectId1, ObjectId2, ObjectId2 }, new ObjectId[] { ObjectId1, ObjectId1 });
        }

        public static IEnumerable<TestCaseData<ObjectId?>> NullableObjectIdTestValues()
        {
            yield return new TestCaseData<ObjectId?>(new ObjectId?[] { ObjectId1, ObjectId2, ObjectId3 }, new ObjectId?[] { ObjectId4, ObjectId5, ObjectId6 });
            yield return new TestCaseData<ObjectId?>(new ObjectId?[] { ObjectId1, ObjectId2, ObjectId3, null }, new ObjectId?[] { ObjectId1, ObjectId2, ObjectId3, null });
            yield return new TestCaseData<ObjectId?>(new ObjectId?[] { ObjectIdMax }, new ObjectId?[] { ObjectId0 });
            yield return new TestCaseData<ObjectId?>(new ObjectId?[] { ObjectId0, ObjectId1, ObjectIdMax }, Array.Empty<ObjectId?>());
            yield return new TestCaseData<ObjectId?>(Array.Empty<ObjectId?>(), new ObjectId?[] { ObjectId0 });
            yield return new TestCaseData<ObjectId?>(new ObjectId?[] { ObjectId1, ObjectId2 }, new ObjectId?[] { ObjectIdMax, ObjectId.GenerateNewId(), ObjectId1, ObjectId2, ObjectId3, ObjectId2 });
            yield return new TestCaseData<ObjectId?>(new ObjectId?[] { ObjectId1, ObjectId1, ObjectId1, ObjectId1, ObjectId1, ObjectId1, ObjectId1 }, new ObjectId?[] { ObjectId1, ObjectId1 });
            yield return new TestCaseData<ObjectId?>(new ObjectId?[] { ObjectId1, ObjectId1, ObjectId1, ObjectId1, ObjectId1, ObjectId1, ObjectId1 }, new ObjectId?[] { ObjectId1, ObjectId2 });
            yield return new TestCaseData<ObjectId?>(new ObjectId?[] { ObjectId1, ObjectId1, ObjectId1, ObjectId1, ObjectId2, ObjectId2 }, new ObjectId?[] { ObjectId1, ObjectId1 });
            yield return new TestCaseData<ObjectId?>(new ObjectId?[] { ObjectId1, ObjectId2, ObjectId1, ObjectId2, null, null, null }, new ObjectId?[] { ObjectId1, ObjectId1, null });
            yield return new TestCaseData<ObjectId?>(new ObjectId?[] { null }, new ObjectId?[] { null, null });
            yield return new TestCaseData<ObjectId?>(new ObjectId?[] { null, null }, new ObjectId?[] { null, ObjectId6 });
        }

        [TestCaseSource(nameof(ObjectIdTestValues))]
        public void RealmSet_WhenUnmanaged_ObjectId(TestCaseData<ObjectId> testData)
        {
            RunUnmanagedTests(o => o.ObjectIdSet, testData);
        }

        [TestCaseSource(nameof(NullableObjectIdTestValues))]
        public void RealmSet_WhenUnmanaged_NullableObjectId(TestCaseData<ObjectId?> testData)
        {
            RunUnmanagedTests(o => o.NullableObjectIdSet, testData);
        }

        [TestCaseSource(nameof(ObjectIdTestValues))]
        public void RealmSet_WhenManaged_ObjectId(TestCaseData<ObjectId> testData)
        {
            RunManagedTests(o => o.ObjectIdSet, o => o.ObjectIdList, o => o.ObjectIdDict, testData);
        }

        [TestCaseSource(nameof(NullableObjectIdTestValues))]
        public void RealmSet_WhenManaged_NullableObjectId(TestCaseData<ObjectId?> testData)
        {
            RunManagedTests(o => o.NullableObjectIdSet, o => o.NullableObjectIdList, o => o.NullableObjectIdDict, testData);
        }

        [Test]
        public void RealmSet_WhenManaged_ObjectId_Notifications()
        {
            var testData = new TestCaseData<ObjectId>(TestHelpers.GenerateRepetitiveObjectId(1), TestHelpers.GenerateRepetitiveObjectId(2));
            RunManagedNotificationsTests(o => o.ObjectIdSet, testData, newValue: TestHelpers.GenerateRepetitiveObjectId(255));
        }

        [Test]
        public void RealmSet_WhenManaged_NullableObjectId_Notifications()
        {
            var testData = new TestCaseData<ObjectId?>(TestHelpers.GenerateRepetitiveObjectId(1), TestHelpers.GenerateRepetitiveObjectId(2));
            RunManagedNotificationsTests(o => o.NullableObjectIdSet, testData, newValue: null);
        }

        #endregion

        #region DateTimeOffset

        private static readonly DateTimeOffset Date0 = new DateTimeOffset(0, TimeSpan.Zero);
        private static readonly DateTimeOffset Date1 = new DateTimeOffset(1999, 3, 4, 5, 30, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset Date2 = new DateTimeOffset(2030, 1, 3, 9, 25, 34, TimeSpan.FromHours(3));
        private static readonly DateTimeOffset Date3 = new DateTimeOffset(2000, 9, 7, 12, 39, 24, TimeSpan.FromHours(12));
        private static readonly DateTimeOffset Date4 = new DateTimeOffset(1975, 6, 9, 11, 59, 59, TimeSpan.Zero);
        private static readonly DateTimeOffset Date5 = new DateTimeOffset(2034, 12, 24, 4, 0, 14, TimeSpan.FromHours(-3));
        private static readonly DateTimeOffset Date6 = new DateTimeOffset(2020, 10, 11, 19, 17, 10, TimeSpan.Zero);

        public static IEnumerable<TestCaseData<DateTimeOffset>> DateTimeOffsetTestValues()
        {
            yield return new TestCaseData<DateTimeOffset>(new DateTimeOffset[] { Date1, Date2, Date3 }, new DateTimeOffset[] { Date4, Date5, Date6 });
            yield return new TestCaseData<DateTimeOffset>(new DateTimeOffset[] { Date1, Date2, Date3 }, new DateTimeOffset[] { Date1, Date2, Date3 });
            yield return new TestCaseData<DateTimeOffset>(new DateTimeOffset[] { DateTimeOffset.MaxValue, DateTimeOffset.MinValue }, new DateTimeOffset[] { Date0 });
            yield return new TestCaseData<DateTimeOffset>(new DateTimeOffset[] { Date0, Date1, DateTimeOffset.MaxValue }, Array.Empty<DateTimeOffset>());
            yield return new TestCaseData<DateTimeOffset>(Array.Empty<DateTimeOffset>(), new DateTimeOffset[] { Date0 });
            yield return new TestCaseData<DateTimeOffset>(new DateTimeOffset[] { Date1, Date2 }, new DateTimeOffset[] { DateTimeOffset.MaxValue, DateTimeOffset.UtcNow, Date1, Date2, Date3, Date2 });
            yield return new TestCaseData<DateTimeOffset>(new DateTimeOffset[] { Date1, Date1, Date1, Date1, Date1, Date1, Date1 }, new DateTimeOffset[] { Date1, Date1 });
            yield return new TestCaseData<DateTimeOffset>(new DateTimeOffset[] { Date1, Date1, Date1, Date1, Date1, Date1, Date1 }, new DateTimeOffset[] { Date1, Date2 });
            yield return new TestCaseData<DateTimeOffset>(new DateTimeOffset[] { Date1, Date1, Date1, Date1, Date2, Date2 }, new DateTimeOffset[] { Date1, Date1 });
        }

        public static IEnumerable<TestCaseData<DateTimeOffset?>> NullableDateTimeOffsetTestValues()
        {
            yield return new TestCaseData<DateTimeOffset?>(new DateTimeOffset?[] { Date1, Date2, Date3 }, new DateTimeOffset?[] { Date4, Date5, Date6 });
            yield return new TestCaseData<DateTimeOffset?>(new DateTimeOffset?[] { Date1, Date2, Date3, null }, new DateTimeOffset?[] { Date1, Date2, Date3, null });
            yield return new TestCaseData<DateTimeOffset?>(new DateTimeOffset?[] { DateTimeOffset.MaxValue, DateTimeOffset.MinValue }, new DateTimeOffset?[] { Date0 });
            yield return new TestCaseData<DateTimeOffset?>(new DateTimeOffset?[] { Date0, Date1, DateTimeOffset.MaxValue }, Array.Empty<DateTimeOffset?>());
            yield return new TestCaseData<DateTimeOffset?>(Array.Empty<DateTimeOffset?>(), new DateTimeOffset?[] { Date0 });
            yield return new TestCaseData<DateTimeOffset?>(new DateTimeOffset?[] { Date1, Date2 }, new DateTimeOffset?[] { DateTimeOffset.MaxValue, DateTimeOffset.UtcNow, Date1, Date2, Date3, Date2 });
            yield return new TestCaseData<DateTimeOffset?>(new DateTimeOffset?[] { Date1, Date1, Date1, Date1, Date1, Date1, Date1 }, new DateTimeOffset?[] { Date1, Date1 });
            yield return new TestCaseData<DateTimeOffset?>(new DateTimeOffset?[] { Date1, Date1, Date1, Date1, Date1, Date1, Date1 }, new DateTimeOffset?[] { Date1, Date2 });
            yield return new TestCaseData<DateTimeOffset?>(new DateTimeOffset?[] { Date1, Date1, Date1, Date1, Date2, Date2 }, new DateTimeOffset?[] { Date1, Date1 });
            yield return new TestCaseData<DateTimeOffset?>(new DateTimeOffset?[] { Date1, Date2, Date1, Date2, null, null, null }, new DateTimeOffset?[] { Date1, Date1, null });
            yield return new TestCaseData<DateTimeOffset?>(new DateTimeOffset?[] { null }, new DateTimeOffset?[] { null, null });
            yield return new TestCaseData<DateTimeOffset?>(new DateTimeOffset?[] { null, null }, new DateTimeOffset?[] { null, Date6 });
        }

        [TestCaseSource(nameof(DateTimeOffsetTestValues))]
        public void RealmSet_WhenUnmanaged_DateTimeOffset(TestCaseData<DateTimeOffset> testData)
        {
            RunUnmanagedTests(o => o.DateTimeOffsetSet, testData);
        }

        [TestCaseSource(nameof(NullableDateTimeOffsetTestValues))]
        public void RealmSet_WhenUnmanaged_NullableDateTimeOffset(TestCaseData<DateTimeOffset?> testData)
        {
            RunUnmanagedTests(o => o.NullableDateTimeOffsetSet, testData);
        }

        [TestCaseSource(nameof(DateTimeOffsetTestValues))]
        public void RealmSet_WhenManaged_DateTimeOffset(TestCaseData<DateTimeOffset> testData)
        {
            RunManagedTests(o => o.DateTimeOffsetSet, o => o.DateTimeOffsetList, o => o.DateTimeOffsetDict, testData);
        }

        [TestCaseSource(nameof(NullableDateTimeOffsetTestValues))]
        public void RealmSet_WhenManaged_NullableDateTimeOffset(TestCaseData<DateTimeOffset?> testData)
        {
            RunManagedTests(o => o.NullableDateTimeOffsetSet, o => o.NullableDateTimeOffsetList, o => o.NullableDateTimeOffsetDict, testData);
        }

        [Test]
        public void RealmSet_WhenManaged_DateTimeOffset_Notifications()
        {
            var testData = new TestCaseData<DateTimeOffset>(Date0, Date1);
            RunManagedNotificationsTests(o => o.DateTimeOffsetSet, testData, newValue: Date5);
        }

        [Test]
        public void RealmSet_WhenManaged_NullableDateTimeOffset_Notifications()
        {
            var testData = new TestCaseData<DateTimeOffset?>(Date0, null, Date2);
            RunManagedNotificationsTests(o => o.NullableDateTimeOffsetSet, testData, newValue: Date6);
        }

        #endregion

        #region String

        public static IEnumerable<TestCaseData<string>> StringTestValues()
        {
            yield return new TestCaseData<string>(new string[] { "a", "b", "c" }, new string[] { "d", "e", "f" });
            yield return new TestCaseData<string>(new string[] { "a", "b", "c" }, new string[] { "a", "b", "c" });
            yield return new TestCaseData<string>(new string[] { "bla bla bla", string.Empty }, new string[] { " " });
            yield return new TestCaseData<string>(new string[] { " ", "a", "bla bla bla" }, Array.Empty<string>());
            yield return new TestCaseData<string>(Array.Empty<string>(), new string[] { " " });
            yield return new TestCaseData<string>(new string[] { "a", "b" }, new string[] { "bla bla bla", "a", "b", "c", "b" });
            yield return new TestCaseData<string>(new string[] { "a", "a", "a", "a", "a", "a", "a" }, new string[] { "a", "a" });
            yield return new TestCaseData<string>(new string[] { "a", "a", "a", "a", "a", "a", "a" }, new string[] { "a", "b" });
            yield return new TestCaseData<string>(new string[] { "a", "a", "a", "a", "b", "b" }, new string[] { "a", "a" });
        }

        public static IEnumerable<TestCaseData<string>> NullableStringTestValues()
        {
            yield return new TestCaseData<string>(new string[] { "a", "b", "c" }, new string[] { "d", "e", "f" });
            yield return new TestCaseData<string>(new string[] { "a", "b", "c", null }, new string[] { "a", "b", "c", null });
            yield return new TestCaseData<string>(new string[] { "bla bla bla" }, new string[] { " " });
            yield return new TestCaseData<string>(new string[] { " ", "a", "bla bla bla" }, Array.Empty<string>());
            yield return new TestCaseData<string>(Array.Empty<string>(), new string[] { " " });
            yield return new TestCaseData<string>(new string[] { "a", "b" }, new string[] { "bla bla bla", "a", "b", "c", "b" });
            yield return new TestCaseData<string>(new string[] { "a", "a", "a", "a", "a", "a", "a" }, new string[] { "a", "a" });
            yield return new TestCaseData<string>(new string[] { "a", "a", "a", "a", "a", "a", "a" }, new string[] { "a", "b" });
            yield return new TestCaseData<string>(new string[] { "a", "a", "a", "a", "b", "b" }, new string[] { "a", "a" });
            yield return new TestCaseData<string>(new string[] { "a", "b", "a", "b", null, null, null }, new string[] { "a", "a", null });
            yield return new TestCaseData<string>(new string[] { null }, new string[] { null, null });
            yield return new TestCaseData<string>(new string[] { null, null }, new string[] { null, "f" });
        }

        [TestCaseSource(nameof(StringTestValues))]
        public void RealmSet_WhenUnmanaged_String(TestCaseData<string> testData)
        {
            RunUnmanagedTests(o => o.StringSet, testData);
        }

        [TestCaseSource(nameof(NullableStringTestValues))]
        public void RealmSet_WhenUnmanaged_NullableString(TestCaseData<string> testData)
        {
            RunUnmanagedTests(o => o.NullableStringSet, testData);
        }

        [TestCaseSource(nameof(StringTestValues))]
        public void RealmSet_WhenManaged_String(TestCaseData<string> testData)
        {
            RunManagedTests(o => o.StringSet, o => o.StringList, o => o.StringDict, testData);
        }

        [TestCaseSource(nameof(NullableStringTestValues))]
        public void RealmSet_WhenManaged_NullableString(TestCaseData<string> testData)
        {
            RunManagedTests(o => o.NullableStringSet, o => o.NullableStringList, o => o.NullableStringDict, testData);
        }

        [Test]
        public void RealmSet_WhenManaged_String_Notifications()
        {
            var testData = new TestCaseData<string>("abc", "cde");
            RunManagedNotificationsTests(o => o.StringSet, testData, newValue: string.Empty);
        }

        [Test]
        public void RealmSet_WhenManaged_NullableString_Notifications()
        {
            var testData = new TestCaseData<string>("fge", null, "zzzz");
            RunManagedNotificationsTests(o => o.NullableStringSet, testData, newValue: "new string");
        }

        #endregion

        #region Binary

        private static byte[] Binary0 => TestHelpers.GetBytes(5, 0);

        private static byte[] Binary1 => TestHelpers.GetBytes(5, 1);

        private static byte[] Binary2 => TestHelpers.GetBytes(5, 2);

        private static byte[] Binary3 => TestHelpers.GetBytes(5, 3);

        private static byte[] Binary4 => TestHelpers.GetBytes(5, 4);

        private static byte[] Binary5 => TestHelpers.GetBytes(5, 5);

        private static byte[] Binary6 => TestHelpers.GetBytes(5, 6);

        private static byte[] BinaryMax => TestHelpers.GetBytes(5, 255);

        public static IEnumerable<TestCaseData<byte[]>> BinaryTestValues()
        {
            yield return new TestCaseData<byte[]>(new byte[][] { Binary1, Binary2, Binary3 }, new byte[][] { Binary4, Binary5, Binary6 });
            yield return new TestCaseData<byte[]>(new byte[][] { Binary1, Binary2, Binary3 }, new byte[][] { Binary1, Binary2, Binary3 });
            yield return new TestCaseData<byte[]>(new byte[][] { BinaryMax }, new byte[][] { Binary0 });
            yield return new TestCaseData<byte[]>(new byte[][] { Binary0, Binary1, BinaryMax }, Array.Empty<byte[]>());
            yield return new TestCaseData<byte[]>(Array.Empty<byte[]>(), new byte[][] { Binary0 });
            yield return new TestCaseData<byte[]>(new byte[][] { Binary1, Binary2 }, new byte[][] { BinaryMax, Binary1, Binary2, Binary3, Binary2 });
            yield return new TestCaseData<byte[]>(new byte[][] { Binary1, Binary1, Binary1, Binary1, Binary1, Binary1, Binary1 }, new byte[][] { Binary1, Binary1 });
            yield return new TestCaseData<byte[]>(new byte[][] { Binary1, Binary1, Binary1, Binary1, Binary1, Binary1, Binary1 }, new byte[][] { Binary1, Binary2 });
            yield return new TestCaseData<byte[]>(new byte[][] { Binary1, Binary1, Binary1, Binary1, Binary2, Binary2 }, new byte[][] { Binary1, Binary1 });
        }

        public static IEnumerable<TestCaseData<byte[]>> NullableBinaryTestValues()
        {
            yield return new TestCaseData<byte[]>(new byte[][] { Binary1, Binary2, Binary3 }, new byte[][] { Binary4, Binary5, Binary6 });
            yield return new TestCaseData<byte[]>(new byte[][] { Binary1, Binary2, Binary3, null }, new byte[][] { Binary1, Binary2, Binary3, null });
            yield return new TestCaseData<byte[]>(new byte[][] { BinaryMax }, new byte[][] { Binary0 });
            yield return new TestCaseData<byte[]>(new byte[][] { Binary0, Binary1, BinaryMax }, Array.Empty<byte[]>());
            yield return new TestCaseData<byte[]>(Array.Empty<byte[]>(), new byte[][] { Binary0 });
            yield return new TestCaseData<byte[]>(new byte[][] { Binary1, Binary2 }, new byte[][] { BinaryMax, Binary1, Binary2, Binary3, Binary2 });
            yield return new TestCaseData<byte[]>(new byte[][] { Binary1, Binary1, Binary1, Binary1, Binary1, Binary1, Binary1 }, new byte[][] { Binary1, Binary1 });
            yield return new TestCaseData<byte[]>(new byte[][] { Binary1, Binary1, Binary1, Binary1, Binary1, Binary1, Binary1 }, new byte[][] { Binary1, Binary2 });
            yield return new TestCaseData<byte[]>(new byte[][] { Binary1, Binary1, Binary1, Binary1, Binary2, Binary2 }, new byte[][] { Binary1, Binary1 });
            yield return new TestCaseData<byte[]>(new byte[][] { Binary1, Binary2, Binary1, Binary2, null, null, null }, new byte[][] { Binary1, Binary1, null });
            yield return new TestCaseData<byte[]>(new byte[][] { null }, new byte[][] { null, null });
            yield return new TestCaseData<byte[]>(new byte[][] { null, null }, new byte[][] { null, Binary6 });
        }

        [TestCaseSource(nameof(BinaryTestValues))]
        public void RealmSet_WhenUnmanaged_Binary(TestCaseData<byte[]> testData)
        {
            RunUnmanagedTests(o => o.ByteArraySet, testData);
        }

        [TestCaseSource(nameof(NullableBinaryTestValues))]
        public void RealmSet_WhenUnmanaged_NullableBinary(TestCaseData<byte[]> testData)
        {
            RunUnmanagedTests(o => o.NullableByteArraySet, testData);
        }

        [TestCaseSource(nameof(BinaryTestValues))]
        public void RealmSet_WhenManaged_Binary(TestCaseData<byte[]> testData)
        {
            RunManagedTests(o => o.ByteArraySet, o => o.ByteArrayList, o => o.ByteArrayDict, testData);
        }

        [TestCaseSource(nameof(NullableBinaryTestValues))]
        public void RealmSet_WhenManaged_NullableBinary(TestCaseData<byte[]> testData)
        {
            RunManagedTests(o => o.NullableByteArraySet, o => o.NullableByteArrayList, o => o.NullableByteArrayDict, testData);
        }

        [Test]
        public void RealmSet_WhenManaged_Binary_Notifications()
        {
            var testData = new TestCaseData<byte[]>(Binary0, Binary1);
            RunManagedNotificationsTests(o => o.ByteArraySet, testData, newValue: BinaryMax);
        }

        [Test]
        public void RealmSet_WhenManaged_NullableBinary_Notifications()
        {
            var testData = new TestCaseData<byte[]>(Binary1, null, Binary5);
            RunManagedNotificationsTests(o => o.NullableByteArraySet, testData, newValue: Binary2);
        }

        #endregion

        #region IntPropertyObject

        public static IEnumerable<TestCaseData<IntPropertyObject>> ObjectTestValues()
        {
            var objs = GenerateObjects(1, 2, 3, 4, 5, 6);
            yield return new TestCaseData<IntPropertyObject>(new IntPropertyObject[] { objs[0], objs[1], objs[2] }, new IntPropertyObject[] { objs[3], objs[4], objs[5] });

            objs = GenerateObjects(1, 2, 3);
            yield return new TestCaseData<IntPropertyObject>(new IntPropertyObject[] { objs[0], objs[1], objs[2] }, new IntPropertyObject[] { objs[0], objs[1], objs[2] });

            objs = GenerateObjects(0, int.MaxValue);
            yield return new TestCaseData<IntPropertyObject>(new IntPropertyObject[] { objs[1] }, new IntPropertyObject[] { objs[0] });

            objs = GenerateObjects(0, int.MaxValue);
            yield return new TestCaseData<IntPropertyObject>(new IntPropertyObject[] { objs[0], objs[0], objs[1] }, Array.Empty<IntPropertyObject>());

            objs = GenerateObjects(1);
            yield return new TestCaseData<IntPropertyObject>(Array.Empty<IntPropertyObject>(), new IntPropertyObject[] { objs[0] });

            objs = GenerateObjects(1, 2, 3, int.MaxValue);
            yield return new TestCaseData<IntPropertyObject>(new IntPropertyObject[] { objs[0], objs[1] }, new IntPropertyObject[] { objs[3], objs[0], objs[1], objs[2], objs[1] });

            objs = GenerateObjects(1);
            yield return new TestCaseData<IntPropertyObject>(new IntPropertyObject[] { objs[0], objs[0], objs[0], objs[0], objs[0], objs[0], objs[0] }, new IntPropertyObject[] { objs[0], objs[0] });

            objs = GenerateObjects(1, 2);
            yield return new TestCaseData<IntPropertyObject>(new IntPropertyObject[] { objs[0], objs[0], objs[0], objs[0], objs[0], objs[0], objs[0] }, new IntPropertyObject[] { objs[0], objs[1] });

            objs = GenerateObjects(1, 2);
            yield return new TestCaseData<IntPropertyObject>(new IntPropertyObject[] { objs[0], objs[0], objs[0], objs[0], objs[1], objs[1] }, new IntPropertyObject[] { objs[0], objs[0] });
        }

        private static IntPropertyObject[] GenerateObjects(params int[] values)
        {
            var result = new IntPropertyObject[values.Length];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = new IntPropertyObject { Int = values[i] };
            }

            return result;
        }

        [TestCaseSource(nameof(ObjectTestValues))]
        public void RealmSet_WhenUnmanaged_Object(TestCaseData<IntPropertyObject> testData)
        {
            RunUnmanagedTests(o => o.ObjectSet, testData);
        }

        [TestCaseSource(nameof(ObjectTestValues))]
        public void RealmSet_WhenManaged_Object(TestCaseData<IntPropertyObject> testData)
        {
            RunManagedTests(o => o.ObjectSet, o => o.ObjectList, o => o.ObjectDict, testData);
        }

        [Test]
        public void RealmSet_WhenManaged_Object_Notifications()
        {
            var testData = new TestCaseData<IntPropertyObject>(GenerateObjects(1, 2, 3));
            RunManagedNotificationsTests(o => o.ObjectSet, testData, newValue: GenerateObjects(5).Single());
        }

        #endregion

        #region RealmValue

        public static IEnumerable<TestCaseData<RealmValue>> RealmTestValues()
        {
            var rv0 = RealmValue.Null;
            var rv1 = RealmValue.Create(10, RealmValueType.Int);
            var rv2 = RealmValue.Create(true, RealmValueType.Bool);
            var rv3 = RealmValue.Create("abc", RealmValueType.String);
            var rv4 = RealmValue.Create(new byte[] { 0, 1, 2 }, RealmValueType.Data);
            var rv5 = RealmValue.Create(DateTimeOffset.FromUnixTimeSeconds(1616137641), RealmValueType.Date);
            var rv6 = RealmValue.Create(1.5f, RealmValueType.Float);
            var rv7 = RealmValue.Create(2.5d, RealmValueType.Double);
            var rv8 = RealmValue.Create(5m, RealmValueType.Decimal128);
            var rv9 = RealmValue.Create(new ObjectId("5f63e882536de46d71877979"), RealmValueType.ObjectId);
            var rv10 = RealmValue.Create(new Guid("{F2952191-A847-41C3-8362-497F92CB7D24}"), RealmValueType.Guid);
            var rv11 = GetRealmValueObject();

            yield return new TestCaseData<RealmValue>(new[] { rv0, rv1, rv2, rv3, rv4, rv5, rv6, rv7, rv8, rv9, rv10, rv11 }, new[] { rv0, rv1, rv2, rv3, rv4, rv5, rv6, rv7, rv8, rv9, rv10, rv11 });

            rv11 = GetRealmValueObject();

            yield return new TestCaseData<RealmValue>(new[] { rv0, rv1, rv2, rv3, rv4, rv5 }, new[] { rv6, rv7, rv8, rv9, rv10, rv11 });

            rv11 = GetRealmValueObject();

            yield return new TestCaseData<RealmValue>( Array.Empty<RealmValue>(), new[] { rv0, rv1, rv2, rv3, rv4, rv5, rv6, rv7, rv8, rv9, rv10, rv11 });

            static RealmValue GetRealmValueObject() => RealmValue.Create(new IntPropertyObject { Int = 10 }, RealmValueType.Object);

            var i1 = RealmValue.Create(1, RealmValueType.Int);
            var i2 = RealmValue.Create(1d, RealmValueType.Double);
            var i3 = RealmValue.Create(1f, RealmValueType.Float);
            var i4 = RealmValue.Create(true, RealmValueType.Bool);
            var i5 = RealmValue.Create(1m, RealmValueType.Decimal128);

            yield return new TestCaseData<RealmValue>(new[] { i1, i2, i3, i4, i5 }, new[] { i1, i2, i3, i4, i5 });

            var s1 = RealmValue.Create(string.Empty, RealmValueType.String);
            var s2 = RealmValue.Create(0, RealmValueType.Int);
            var s3 = RealmValue.Create(Guid.Empty, RealmValueType.Guid);
            var s4 = RealmValue.Null;

            yield return new TestCaseData<RealmValue>(new[] { s1, s2, s3, s4 }, new[] { s1, s2, s3, s4 });

            var d1 = RealmValue.Create(1m, RealmValueType.Decimal128);
            var d2 = RealmValue.Create(1f, RealmValueType.Decimal128);
            var d3 = RealmValue.Create(1d, RealmValueType.Decimal128);
            var d4 = RealmValue.Create(1, RealmValueType.Decimal128);

            yield return new TestCaseData<RealmValue>(new[] { d1, d2, d3, d4 }, new[] { d1, d2, d3, d4 });
        }

        [TestCaseSource(nameof(RealmTestValues))]
        public void RealmSet_WhenUnmanaged_RealmValue(TestCaseData<RealmValue> testData)
        {
            RunUnmanagedTests(o => o.RealmValueSet, testData);
        }

        [TestCaseSource(nameof(RealmTestValues))]
        public void RealmSet_WhenManaged_RealmValue(TestCaseData<RealmValue> testData)
        {
            RunManagedTests(o => o.RealmValueSet, o => o.RealmValueList, o => o.RealmValueDict, testData);
        }

        [Test]
        public void RealmSet_WhenManaged_RealmValue_Notifications()
        {
            var testData = new TestCaseData<RealmValue>(new RealmValue[] {
                RealmValue.Null,
                RealmValue.Create(10, RealmValueType.Int),
                RealmValue.Create(true, RealmValueType.Bool),
                RealmValue.Create("abc", RealmValueType.String),
                RealmValue.Create(new byte[] { 0, 1, 2 }, RealmValueType.Data),
                RealmValue.Create(DateTimeOffset.FromUnixTimeSeconds(1616137641), RealmValueType.Date),
                RealmValue.Create(1.5f, RealmValueType.Float),
                RealmValue.Create(2.5d, RealmValueType.Double),
                RealmValue.Create(5m, RealmValueType.Decimal128),
                RealmValue.Create(new ObjectId("5f63e882536de46d71877979"), RealmValueType.ObjectId),
                RealmValue.Create(new Guid("{F2952191-A847-41C3-8362-497F92CB7D24}"), RealmValueType.Guid),
                RealmValue.Create(new IntPropertyObject { Int = 10 }, RealmValueType.Object) });

            RunManagedNotificationsTests(o => o.RealmValueSet, testData, newValue: "newValue");
        }

        #endregion

        private static void RunUnmanagedTests<T>(Func<CollectionsObject, ISet<T>> accessor, TestCaseData<T> testData)
        {
            var testObject = new CollectionsObject();
            var set = accessor(testObject);

            // We can't freeze unmanaged sets.
            Assert.Throws<RealmException>(() => set.Freeze());

            testData.AssertCount(set);
            testData.AssertExceptWith(set);
            testData.AssertIntersectWith(set);
            testData.AssertIsProperSubsetOf(set);
            testData.AssertIsProperSupersetOf(set);
            testData.AssertIsSubsetOf(set);
            testData.AssertIsSupersetOf(set);
            testData.AssertOverlaps(set);
            testData.AssertSymmetricExceptWith(set);
            testData.AssertUnionWith(set);
        }

        private void RunManagedTests<T>(Func<CollectionsObject, ISet<T>> accessor, Func<CollectionsObject, IList<T>> listAccessor, Func<CollectionsObject, IDictionary<string, T>> dictAccessor, TestCaseData<T> testData)
        {
            var testObject = new CollectionsObject();
            var set = accessor(testObject);

            testData.Seed(set);

            _realm.Write(() =>
            {
                _realm.Add(testObject);
            });

            var managedSet = accessor(testObject);
            Assert.That(set, Is.Not.SameAs(managedSet));

            // Now we're testing set operations on RealmSet/HashSet
            testData.AssertCount(managedSet);
            testData.AssertExceptWith(managedSet);
            testData.AssertIntersectWith(managedSet);
            testData.AssertIsProperSubsetOf(managedSet);
            testData.AssertIsProperSupersetOf(managedSet);
            testData.AssertIsSubsetOf(managedSet);
            testData.AssertIsSupersetOf(managedSet);
            testData.AssertOverlaps(managedSet);
            testData.AssertSymmetricExceptWith(managedSet);
            testData.AssertUnionWith(managedSet);

            // Now we're testing set operations on RealmSet/RealmSet
            var otherSet = _realm.Write(() =>
            {
                var otherObj = _realm.Add(new CollectionsObject());
                var result = accessor(otherObj);

                result.UnionWith(testData.OtherCollection);

                return result;
            });

            testData.AssertExceptWith(managedSet, otherSet);
            testData.AssertIntersectWith(managedSet, otherSet);
            testData.AssertIsProperSubsetOf(managedSet, otherSet);
            testData.AssertIsProperSupersetOf(managedSet, otherSet);
            testData.AssertIsSubsetOf(managedSet, otherSet);
            testData.AssertIsSupersetOf(managedSet, otherSet);
            testData.AssertOverlaps(managedSet, otherSet);
            testData.AssertSymmetricExceptWith(managedSet, otherSet);
            testData.AssertUnionWith(managedSet, otherSet);

            var otherList = _realm.Write(() =>
            {
                var otherObj = _realm.Add(new CollectionsObject());
                var result = listAccessor(otherObj);

                foreach (var item in testData.OtherCollection)
                {
                    result.Add(item);
                }

                return result;
            });

            testData.AssertExceptWith(managedSet, otherList);
            testData.AssertIntersectWith(managedSet, otherList);
            testData.AssertIsProperSubsetOf(managedSet, otherList);
            testData.AssertIsProperSupersetOf(managedSet, otherList);
            testData.AssertIsSubsetOf(managedSet, otherList);
            testData.AssertIsSupersetOf(managedSet, otherList);
            testData.AssertOverlaps(managedSet, otherList);
            testData.AssertSymmetricExceptWith(managedSet, otherList);
            testData.AssertUnionWith(managedSet, otherList);

            // Dictionary.Values is backed by RealmResults
            var otherResults = _realm.Write(() =>
            {
                var otherObj = _realm.Add(new CollectionsObject());
                var result = dictAccessor(otherObj);

                foreach (var item in testData.OtherCollection)
                {
                    result.Add(Guid.NewGuid().ToString(), item);
                }

                return result.Values;
            });

            testData.AssertExceptWith(managedSet, otherResults);
            testData.AssertIntersectWith(managedSet, otherResults);
            testData.AssertIsProperSubsetOf(managedSet, otherResults);
            testData.AssertIsProperSupersetOf(managedSet, otherResults);
            testData.AssertIsSubsetOf(managedSet, otherResults);
            testData.AssertIsSupersetOf(managedSet, otherResults);
            testData.AssertOverlaps(managedSet, otherResults);
            testData.AssertSymmetricExceptWith(managedSet, otherResults);
            testData.AssertUnionWith(managedSet, otherResults);
        }

        private void RunManagedNotificationsTests<T>(Func<CollectionsObject, ISet<T>> accessor, TestCaseData<T> testData, T newValue)
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var testObject = new CollectionsObject();
                var set = accessor(testObject);

                testData.Seed(set);

                _realm.Write(() =>
                {
                    _realm.Add(testObject);
                });

                var managedSet = accessor(testObject);
                Assert.That(set, Is.Not.SameAs(managedSet));

                await testData.AssertNotifications_Realm(managedSet, newValue);
                await testData.AssertNotifications_CollectionChanged(managedSet, newValue);
            });
        }

        private static TestCaseData<RealmInteger<T>> ToInteger<T>(TestCaseData<T> data)
            where T : struct, IComparable<T>, IFormattable, IConvertible, IEquatable<T>
        {
            return new TestCaseData<RealmInteger<T>>(data.InitialValues.ToInteger(), data.OtherCollection.ToInteger());
        }

        private static TestCaseData<RealmInteger<T>?> ToInteger<T>(TestCaseData<T?> data)
            where T : struct, IComparable<T>, IFormattable, IConvertible, IEquatable<T>
        {
            return new TestCaseData<RealmInteger<T>?>(data.InitialValues.ToInteger(), data.OtherCollection.ToInteger());
        }

        public class TestCaseData<T>
        {
            private readonly string _description;

            public T[] InitialValues { get; }

            public T[] OtherCollection { get; }

            public TestCaseData(params T[] initialValues) : this(initialValues, Array.Empty<T>())
            {
            }

            public TestCaseData(IEnumerable<T> initialValues, ICollection<T> otherCollection)
            {
                InitialValues = initialValues.ToArray();
                OtherCollection = otherCollection.ToArray();

                if (typeof(T) == typeof(byte[]))
                {
                    var initial = InitialValues.Select(TestHelpers.ByteArrayToTestDescription);
                    var other = OtherCollection.Select(TestHelpers.ByteArrayToTestDescription);
                    _description = $"{typeof(T).Name}: {{{string.Join(",", initial)}}} - {{{string.Join(",", other)}}}";
                }
                else
                {
                    _description = $"{typeof(T).Name}: {{{string.Join(",", InitialValues)}}} - {{{string.Join(",", OtherCollection)}}}";
                }
            }

            public override string ToString() => _description;

            public void AssertCount(ISet<T> target)
            {
                Seed(target);
                var reference = new HashSet<T>(InitialValues, RealmSet<T>.Comparer);

                Assert.That(target.Count, Is.EqualTo(reference.Count));
                Assert.That(target, Is.EquivalentTo(reference));
            }

            public void AssertUnionWith(ISet<T> target, ICollection<T> otherCollection = null)
            {
                Seed(target);

                otherCollection = GetOtherCollection(otherCollection);

                WriteIfNecessary(target, () =>
                {
                    target.UnionWith(otherCollection);
                });

                Assert.That(target, Is.EquivalentTo(GetExpected(set => set.UnionWith)));
            }

            public void AssertExceptWith(ISet<T> target, ICollection<T> otherCollection = null)
            {
                Seed(target);

                otherCollection = GetOtherCollection(otherCollection);

                WriteIfNecessary(target, () =>
                {
                    target.ExceptWith(otherCollection);
                });

                Assert.That(target, Is.EquivalentTo(GetExpected(set => set.ExceptWith)));
            }

            public void AssertIntersectWith(ISet<T> target, ICollection<T> otherCollection = null)
            {
                Seed(target);

                otherCollection = GetOtherCollection(otherCollection);

                WriteIfNecessary(target, () =>
                {
                    target.IntersectWith(otherCollection);
                });

                Assert.That(target, Is.EquivalentTo(GetExpected(set => set.IntersectWith)));
            }

            public void AssertIsProperSubsetOf(ISet<T> target, ICollection<T> otherCollection = null)
            {
                Seed(target);

                var result = target.IsProperSubsetOf(GetOtherCollection(otherCollection));

                Assert.That(result, Is.EqualTo(GetExpected(set => set.IsProperSubsetOf)));
            }

            public void AssertIsProperSupersetOf(ISet<T> target, ICollection<T> otherCollection = null)
            {
                Seed(target);

                var result = target.IsProperSupersetOf(GetOtherCollection(otherCollection));

                Assert.That(result, Is.EqualTo(GetExpected(set => set.IsProperSupersetOf)));
            }

            public void AssertIsSubsetOf(ISet<T> target, ICollection<T> otherCollection = null)
            {
                Seed(target);

                var result = target.IsSubsetOf(GetOtherCollection(otherCollection));

                Assert.That(result, Is.EqualTo(GetExpected(set => set.IsSubsetOf)));
            }

            public void AssertIsSupersetOf(ISet<T> target, ICollection<T> otherCollection = null)
            {
                Seed(target);

                var result = target.IsSupersetOf(GetOtherCollection(otherCollection));

                Assert.That(result, Is.EqualTo(GetExpected(set => set.IsSupersetOf)));
            }

            public void AssertOverlaps(ISet<T> target, ICollection<T> otherCollection = null)
            {
                Seed(target);

                var result = target.Overlaps(GetOtherCollection(otherCollection));

                Assert.That(result, Is.EqualTo(GetExpected(set => set.Overlaps)));
            }

            public void AssertSymmetricExceptWith(ISet<T> target, ICollection<T> otherCollection = null)
            {
                Seed(target);

                otherCollection = GetOtherCollection(otherCollection);

                WriteIfNecessary(target, () =>
                {
                    target.SymmetricExceptWith(otherCollection);
                });

                Assert.That(target, Is.EquivalentTo(GetExpected(set => set.SymmetricExceptWith)));
            }

            public async Task AssertNotifications_Realm(ISet<T> target, T newValue)
            {
                Assert.That(target, Is.TypeOf<RealmSet<T>>());

                Seed(target);
                target.AsRealmCollection().Realm.Refresh();

                var callbacks = new List<ChangeSet>();
                using var token = target.SubscribeForNotifications((collection, changes, error) =>
                {
                    Assert.That(error, Is.Null);

                    if (changes != null)
                    {
                        callbacks.Add(changes);
                    }
                });

                await AssertNotificationsCore(target, newValue, callbacks, changes =>
                {
                    Assert.That(changes.InsertedIndices.Length, Is.EqualTo(1));
                    Assert.That(changes.ModifiedIndices, Is.Empty);
                    Assert.That(changes.NewModifiedIndices, Is.Empty);
                    Assert.That(changes.DeletedIndices, Is.Empty);
                    Assert.That(changes.Moves, Is.Empty);

                    return changes.InsertedIndices[0];
                }, changes =>
                {
                    Assert.That(changes.InsertedIndices, Is.Empty);
                    Assert.That(changes.ModifiedIndices, Is.Empty);
                    Assert.That(changes.NewModifiedIndices, Is.Empty);
                    Assert.That(changes.DeletedIndices.Length, Is.EqualTo(1));
                    Assert.That(changes.Moves, Is.Empty);

                    return changes.DeletedIndices[0];
                });
            }

            public async Task AssertNotifications_CollectionChanged(ISet<T> target, T newValue)
            {
                Assert.That(target, Is.TypeOf<RealmSet<T>>());

                Seed(target);

                target.AsRealmCollection().Realm.Refresh();

                var callbacks = new List<NotifyCollectionChangedEventArgs>();
                target.AsRealmCollection().CollectionChanged += HandleCollectionChanged;

                await AssertNotificationsCore(target, newValue, callbacks, changes =>
                {
                    Assert.That(changes.Action, Is.EqualTo(NotifyCollectionChangedAction.Add));
                    Assert.That(changes.NewItems.Count, Is.EqualTo(1));

                    return changes.NewStartingIndex;
                }, changes =>
                {
                    Assert.That(changes.Action, Is.EqualTo(NotifyCollectionChangedAction.Remove));
                    Assert.That(changes.OldItems.Count, Is.EqualTo(1));

                    return changes.OldStartingIndex;
                });

                target.AsRealmCollection().CollectionChanged -= HandleCollectionChanged;

                void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
                {
                    Assert.That(sender, Is.EqualTo(target));

                    callbacks.Add(e);
                }
            }

            private static async Task AssertNotificationsCore<TArgs>(
                ISet<T> target,
                T newValue,
                List<TArgs> callbacks,
                Func<TArgs, int> assertInsertion,
                Func<TArgs, int> assertDeletion)
            {
                WriteIfNecessary(target, () =>
                {
                    target.Add(newValue);
                });

                var changes = await EnsureRefreshed(1);
                var insertedIndex = assertInsertion(changes);

                Assert.That(target.ElementAt(insertedIndex), Is.EqualTo(newValue));

                Assert.That(target.AsRealmCollection()[insertedIndex], Is.EqualTo(newValue));

                WriteIfNecessary(target, () =>
                {
                    target.Remove(newValue);
                });

                changes = await EnsureRefreshed(2);

                var deletedIndex = assertDeletion(changes);
                Assert.That(deletedIndex, Is.EqualTo(insertedIndex));

                async Task<TArgs> EnsureRefreshed(int expectedCallbackCount)
                {
                    await TestHelpers.WaitForConditionAsync(() => callbacks.Count == expectedCallbackCount);

                    Assert.That(callbacks.Count, Is.EqualTo(expectedCallbackCount));

                    return callbacks[expectedCallbackCount - 1];
                }
            }

            public void Seed(ICollection<T> target, IEnumerable<T> values = null)
            {
                WriteIfNecessary(target, () =>
                {
                    target.Clear();
                    foreach (var item in values ?? InitialValues)
                    {
                        target.Add(item);
                    }
                });
            }

            private bool GetExpected(Func<ISet<T>, Func<IEnumerable<T>, bool>> getInvoker)
            {
                var reference = new HashSet<T>(InitialValues, RealmSet<T>.Comparer);

                return getInvoker(reference).Invoke(OtherCollection);
            }

            private ISet<T> GetExpected(Func<ISet<T>, Action<ICollection<T>>> getInvoker)
            {
                var reference = new HashSet<T>(InitialValues, RealmSet<T>.Comparer);
                getInvoker(reference).Invoke(OtherCollection);
                return reference;
            }

            private ICollection<T> GetOtherCollection(ICollection<T> target) => target ?? OtherCollection;

            private static void WriteIfNecessary(IEnumerable<T> collection, Action writeAction)
            {
                Transaction transaction = null;
                if (collection is RealmSet<T> realmSet)
                {
                    transaction = realmSet.Realm.BeginWrite();
                }

                writeAction();

                transaction?.Commit();
            }
        }
    }
}
