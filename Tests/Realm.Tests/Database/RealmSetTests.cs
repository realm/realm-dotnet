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
using System.Linq;
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
            yield return new TestCaseData<Decimal128>(new Decimal128[] { 1, 1, 1, 1, 1, 1, 1.0m }, new Decimal128[] { 1, 1, 1 });
            yield return new TestCaseData<Decimal128>(new Decimal128[] { 1, 1, 1, 1, 1, 1, 1 }, new Decimal128[] { 1.0m, 2, 1.0m });
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
            yield return new TestCaseData<Decimal128?>(new Decimal128?[] { 1, 1, 1, 1, 1, 1, 1.0m }, new Decimal128?[] { 1, 1, 1 });
            yield return new TestCaseData<Decimal128?>(new Decimal128?[] { 1, 1, 1, 1, 1, 1, 1 }, new Decimal128?[] { 1.0m, 2, 1.0m });
            yield return new TestCaseData<Decimal128?>(new Decimal128?[] { 1, 2, 2, 1, 1, 1, 1 }, new Decimal128?[] { 1, 1, 1 });
            yield return new TestCaseData<Decimal128?>(new Decimal128?[] { 1, 2, 2, 1, null, null, null }, new Decimal128?[] { 1.0m, 1.0m, 1.0m, null });
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

        #endregion

        private static void RunUnmanagedTests<T>(Func<SetsObject, ISet<T>> accessor, TestCaseData<T> testData)
        {
            var testObject = new SetsObject();
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
            public T[] InitialValues { get; }

            public T[] OtherCollection { get; }

            public TestCaseData(IEnumerable<T> initialValues, IEnumerable<T> otherCollection)
            {
                InitialValues = initialValues.ToArray();
                OtherCollection = otherCollection.ToArray();
            }

            public override string ToString()
            {
                return $"{typeof(T).Name}: {{{string.Join(",", InitialValues)}}} - {{{string.Join(",", OtherCollection)}}}";
            }

            public void AssertCount(ISet<T> target)
            {
                Seed(target);
                var reference = GetReferenceSet();

                Assert.That(target.Count, Is.EqualTo(reference.Count));
                Assert.That(target, Is.EquivalentTo(reference));
            }

            public void AssertUnionWith(ISet<T> target)
            {
                Seed(target);
                var reference = GetReferenceSet();

                target.UnionWith(OtherCollection);
                reference.UnionWith(OtherCollection);

                Assert.That(target, Is.EquivalentTo(reference));
            }

            public void AssertExceptWith(ISet<T> target)
            {
                Seed(target);
                var reference = GetReferenceSet();

                target.ExceptWith(OtherCollection);
                reference.ExceptWith(OtherCollection);

                Assert.That(target, Is.EquivalentTo(reference));
            }

            public void AssertIntersectWith(ISet<T> target)
            {
                Seed(target);
                var reference = GetReferenceSet();

                target.IntersectWith(OtherCollection);
                reference.IntersectWith(OtherCollection);

                Assert.That(target, Is.EquivalentTo(reference));
            }

            public void AssertIsProperSubsetOf(ISet<T> target)
            {
                Seed(target);
                var reference = GetReferenceSet();

                var targetResult = target.IsProperSubsetOf(OtherCollection);
                var referenceResult = reference.IsProperSubsetOf(OtherCollection);

                Assert.That(targetResult, Is.EqualTo(referenceResult));
            }

            public void AssertIsProperSupersetOf(ISet<T> target)
            {
                Seed(target);
                var reference = GetReferenceSet();

                var targetResult = target.IsProperSupersetOf(OtherCollection);
                var referenceResult = reference.IsProperSupersetOf(OtherCollection);

                Assert.That(targetResult, Is.EqualTo(referenceResult));
            }

            public void AssertIsSubsetOf(ISet<T> target)
            {
                Seed(target);
                var reference = GetReferenceSet();

                var targetResult = target.IsSubsetOf(OtherCollection);
                var referenceResult = reference.IsSubsetOf(OtherCollection);

                Assert.That(targetResult, Is.EqualTo(referenceResult));
            }

            public void AssertIsSupersetOf(ISet<T> target)
            {
                Seed(target);
                var reference = GetReferenceSet();

                var targetResult = target.IsSupersetOf(OtherCollection);
                var referenceResult = reference.IsSupersetOf(OtherCollection);

                Assert.That(targetResult, Is.EqualTo(referenceResult));
            }

            public void AssertOverlaps(ISet<T> target)
            {
                Seed(target);
                var reference = GetReferenceSet();

                var targetResult = target.Overlaps(OtherCollection);
                var referenceResult = reference.Overlaps(OtherCollection);

                Assert.That(targetResult, Is.EqualTo(referenceResult));
            }

            public void AssertSymmetricExceptWith(ISet<T> target)
            {
                Seed(target);
                var reference = GetReferenceSet();

                target.SymmetricExceptWith(OtherCollection);
                reference.SymmetricExceptWith(OtherCollection);

                Assert.That(target, Is.EquivalentTo(reference));
            }

            private void Seed(ISet<T> target)
            {
                target.Clear();
                foreach (var item in InitialValues)
                {
                    target.Add(item);
                }
            }

            private ISet<T> GetReferenceSet() => new HashSet<T>(InitialValues);
        }
    }
}
