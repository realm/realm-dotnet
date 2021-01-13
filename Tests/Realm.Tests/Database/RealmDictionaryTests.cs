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
    public class RealmDictionaryTests : RealmInstanceTest
    {
        #region Boolean

        public static IEnumerable<TestCaseData<bool>> BoolTestValues()
        {
            yield return new TestCaseData<bool>();
            yield return new TestCaseData<bool>(("a", true));
            yield return new TestCaseData<bool>(("b", false));
            yield return new TestCaseData<bool>(("a", false), ("b", true));
            yield return new TestCaseData<bool>(("a", true), ("b", false), ("c", true));
        }

        public static IEnumerable<TestCaseData<bool?>> NullableBoolTestValues()
        {
            yield return new TestCaseData<bool?>();
            yield return new TestCaseData<bool?>(("a", true));
            yield return new TestCaseData<bool?>(("b", false));
            yield return new TestCaseData<bool?>(("c", null));
            yield return new TestCaseData<bool?>(("a", false), ("b", true));
            yield return new TestCaseData<bool?>(("a", true), ("b", false), ("c", null));
        }

        [TestCaseSource(nameof(BoolTestValues))]
        public void RealmDictionary_WhenUnmanaged_Bool(TestCaseData<bool> testData)
        {
            RunUnmanagedTests(o => o.BooleanDictionary, testData);
        }

        [TestCaseSource(nameof(NullableBoolTestValues))]
        public void RealmDictionary_WhenUnmanaged_NullableBool(TestCaseData<bool?> testData)
        {
            //RunUnmanagedTests(o => o.NullableBooleanDictionary, testData);
        }

        [TestCaseSource(nameof(BoolTestValues))]
        public void RealmDictionary_WhenManaged_Bool(TestCaseData<bool> testData)
        {
            RunManagedTests(o => o.BooleanDictionary, testData);
        }

        [TestCaseSource(nameof(NullableBoolTestValues))]
        public void RealmDictionary_WhenManaged_NullableBool(TestCaseData<bool?> testData)
        {
            //RunManagedTests(o => o.NullableBooleanDictionary, testData);
        }

        #endregion

        #region Byte

        public static IEnumerable<TestCaseData<byte>> ByteTestValues()
        {
            yield return new TestCaseData<byte>();
            yield return new TestCaseData<byte>(("123", 123));
            yield return new TestCaseData<byte>(("a", 1), ("b", 1), ("c", 1));
            yield return new TestCaseData<byte>(("a", 1), ("b", 2), ("c", 3));
            yield return new TestCaseData<byte>(("a", byte.MinValue), ("z", byte.MaxValue));
            yield return new TestCaseData<byte>(("a", byte.MinValue), ("zero", 0), ("one", 1), ("z", byte.MaxValue));
        }

        public static IEnumerable<TestCaseData<byte?>> NullableByteTestValues()
        {
            yield return new TestCaseData<byte?>();
            yield return new TestCaseData<byte?>(("123", 123));
            yield return new TestCaseData<byte?>(("null", null));
            yield return new TestCaseData<byte?>(("null1", null), ("null2", null));
            yield return new TestCaseData<byte?>(("a", 1), ("b", null), ("c", 3));
            yield return new TestCaseData<byte?>(("a", byte.MinValue), ("m", null), ("z", byte.MaxValue));
            yield return new TestCaseData<byte?>(("a", byte.MinValue), ("zero", 0), ("null", null), ("one", 1), ("z", byte.MaxValue));
        }

        [TestCaseSource(nameof(ByteTestValues))]
        public void RealmDictionary_WhenUnmanaged_Byte(TestCaseData<byte> testData)
        {
            RunUnmanagedTests(o => o.ByteDictionary, testData);
        }

        [TestCaseSource(nameof(ByteTestValues))]
        public void RealmDictionary_WhenUnmanaged_ByteCounter(TestCaseData<byte> testData)
        {
            RunUnmanagedTests(o => o.ByteCounterDictionary, ToInteger(testData));
        }

        [TestCaseSource(nameof(NullableByteTestValues))]
        public void RealmDictionary_WhenUnmanaged_NullableByte(TestCaseData<byte?> testData)
        {
            //RunUnmanagedTests(o => o.NullableByteDictionary, testData);
        }

        [TestCaseSource(nameof(NullableByteTestValues))]
        public void RealmDictionary_WhenUnmanaged_NullableByteCounter(TestCaseData<byte?> testData)
        {
            //RunUnmanagedTests(o => o.NullableByteCounterDictionary, ToInteger(testData));
        }

        [TestCaseSource(nameof(ByteTestValues))]
        public void RealmDictionary_WhenManaged_Byte(TestCaseData<byte> testData)
        {
            RunManagedTests(o => o.ByteDictionary, testData);
        }

        [TestCaseSource(nameof(ByteTestValues))]
        public void RealmDictionary_WhenManaged_ByteCounter(TestCaseData<byte> testData)
        {
            RunManagedTests(o => o.ByteCounterDictionary, ToInteger(testData));
        }

        [TestCaseSource(nameof(NullableByteTestValues))]
        public void RealmDictionary_WhenManaged_NullableByte(TestCaseData<byte?> testData)
        {
            //RunManagedTests(o => o.NullableByteDictionary, testData);
        }

        [TestCaseSource(nameof(NullableByteTestValues))]
        public void RealmDictionary_WhenManaged_NullableByteCounter(TestCaseData<byte?> testData)
        {
            //RunManagedTests(o => o.NullableByteCounterDictionary, ToInteger(testData));
        }

        #endregion

        #region Int16

        public static IEnumerable<TestCaseData<short>> Int16TestValues()
        {
            yield return new TestCaseData<short>();
            yield return new TestCaseData<short>(("123", 123));
            yield return new TestCaseData<short>(("123", -123));
            yield return new TestCaseData<short>(("a", 1), ("b", 1), ("c", 1));
            yield return new TestCaseData<short>(("a", 1), ("b", 2), ("c", 3));
            yield return new TestCaseData<short>(("a", short.MinValue), ("z", short.MaxValue));
            yield return new TestCaseData<short>(("a", short.MinValue), ("zero", 0), ("one", 1), ("z", short.MaxValue));
        }

        public static IEnumerable<TestCaseData<short?>> NullableInt16TestValues()
        {
            yield return new TestCaseData<short?>();
            yield return new TestCaseData<short?>(("123", 123));
            yield return new TestCaseData<short?>(("123", -123));
            yield return new TestCaseData<short?>(("null", null));
            yield return new TestCaseData<short?>(("null1", null), ("null2", null));
            yield return new TestCaseData<short?>(("a", 1), ("b", null), ("c", 3));
            yield return new TestCaseData<short?>(("a", short.MinValue), ("m", null), ("z", short.MaxValue));
            yield return new TestCaseData<short?>(("a", short.MinValue), ("zero", 0), ("null", null), ("one", 1), ("z", short.MaxValue));
        }

        [TestCaseSource(nameof(Int16TestValues))]
        public void RealmDictionary_WhenUnmanaged_Int16(TestCaseData<short> testData)
        {
            RunUnmanagedTests(o => o.Int16Dictionary, testData);
        }

        [TestCaseSource(nameof(Int16TestValues))]
        public void RealmDictionary_WhenUnmanaged_Int16Counter(TestCaseData<short> testData)
        {
            RunUnmanagedTests(o => o.Int16CounterDictionary, ToInteger(testData));
        }

        [TestCaseSource(nameof(NullableInt16TestValues))]
        public void RealmDictionary_WhenUnmanaged_NullableInt16(TestCaseData<short?> testData)
        {
            //RunUnmanagedTests(o => o.NullableInt16Dictionary, testData);
        }

        [TestCaseSource(nameof(NullableInt16TestValues))]
        public void RealmDictionary_WhenUnmanaged_NullableInt16Counter(TestCaseData<short?> testData)
        {
            //RunUnmanagedTests(o => o.NullableInt16CounterDictionary, ToInteger(testData));
        }

        [TestCaseSource(nameof(Int16TestValues))]
        public void RealmDictionary_WhenManaged_Int16(TestCaseData<short> testData)
        {
            RunManagedTests(o => o.Int16Dictionary, testData);
        }

        [TestCaseSource(nameof(Int16TestValues))]
        public void RealmDictionary_WhenManaged_Int16Counter(TestCaseData<short> testData)
        {
            RunManagedTests(o => o.Int16CounterDictionary, ToInteger(testData));
        }

        [TestCaseSource(nameof(NullableInt16TestValues))]
        public void RealmDictionary_WhenManaged_NullableInt16(TestCaseData<short?> testData)
        {
            //RunManagedTests(o => o.NullableInt16Dictionary, testData);
        }

        [TestCaseSource(nameof(NullableInt16TestValues))]
        public void RealmDictionary_WhenManaged_NullableInt16Counter(TestCaseData<short?> testData)
        {
            //RunManagedTests(o => o.NullableInt16CounterDictionary, ToInteger(testData));
        }

        #endregion

        #region Int32

        public static IEnumerable<TestCaseData<int>> Int32TestValues()
        {
            yield return new TestCaseData<int>();
            yield return new TestCaseData<int>(("123", 123));
            yield return new TestCaseData<int>(("123", -123));
            yield return new TestCaseData<int>(("a", 1), ("b", 1), ("c", 1));
            yield return new TestCaseData<int>(("a", 1), ("b", 2), ("c", 3));
            yield return new TestCaseData<int>(("a", int.MinValue), ("z", int.MaxValue));
            yield return new TestCaseData<int>(("a", int.MinValue), ("zero", 0), ("one", 1), ("z", int.MaxValue));
        }

        public static IEnumerable<TestCaseData<int?>> NullableInt32TestValues()
        {
            yield return new TestCaseData<int?>();
            yield return new TestCaseData<int?>(("123", 123));
            yield return new TestCaseData<int?>(("123", -123));
            yield return new TestCaseData<int?>(("null", null));
            yield return new TestCaseData<int?>(("null1", null), ("null2", null));
            yield return new TestCaseData<int?>(("a", 1), ("b", null), ("c", 3));
            yield return new TestCaseData<int?>(("a", int.MinValue), ("m", null), ("z", int.MaxValue));
            yield return new TestCaseData<int?>(("a", int.MinValue), ("zero", 0), ("null", null), ("one", 1), ("z", int.MaxValue));
        }

        [TestCaseSource(nameof(Int32TestValues))]
        public void RealmDictionary_WhenUnmanaged_Int32(TestCaseData<int> testData)
        {
            RunUnmanagedTests(o => o.Int32Dictionary, testData);
        }

        [TestCaseSource(nameof(Int32TestValues))]
        public void RealmDictionary_WhenUnmanaged_Int32Counter(TestCaseData<int> testData)
        {
            RunUnmanagedTests(o => o.Int32CounterDictionary, ToInteger(testData));
        }

        [TestCaseSource(nameof(NullableInt32TestValues))]
        public void RealmDictionary_WhenUnmanaged_NullableInt32(TestCaseData<int?> testData)
        {
            //RunUnmanagedTests(o => o.NullableInt32Dictionary, testData);
        }

        [TestCaseSource(nameof(NullableInt32TestValues))]
        public void RealmDictionary_WhenUnmanaged_NullableInt32Counter(TestCaseData<int?> testData)
        {
            //RunUnmanagedTests(o => o.NullableInt32CounterDictionary, ToInteger(testData));
        }

        [TestCaseSource(nameof(Int32TestValues))]
        public void RealmDictionary_WhenManaged_Int32(TestCaseData<int> testData)
        {
            RunManagedTests(o => o.Int32Dictionary, testData);
        }

        [TestCaseSource(nameof(Int32TestValues))]
        public void RealmDictionary_WhenManaged_Int32Counter(TestCaseData<int> testData)
        {
            RunManagedTests(o => o.Int32CounterDictionary, ToInteger(testData));
        }

        [TestCaseSource(nameof(NullableInt32TestValues))]
        public void RealmDictionary_WhenManaged_NullableInt32(TestCaseData<int?> testData)
        {
            //RunManagedTests(o => o.NullableInt32Dictionary, testData);
        }

        [TestCaseSource(nameof(NullableInt32TestValues))]
        public void RealmDictionary_WhenManaged_NullableInt32Counter(TestCaseData<int?> testData)
        {
            //RunManagedTests(o => o.NullableInt32CounterDictionary, ToInteger(testData));
        }

        #endregion

        #region Int64

        public static IEnumerable<TestCaseData<long>> Int64TestValues()
        {
            yield return new TestCaseData<long>();
            yield return new TestCaseData<long>(("123", 123));
            yield return new TestCaseData<long>(("123", -123));
            yield return new TestCaseData<long>(("a", 1), ("b", 1), ("c", 1));
            yield return new TestCaseData<long>(("a", 1), ("b", 2), ("c", 3));
            yield return new TestCaseData<long>(("a", long.MinValue), ("z", long.MaxValue));
            yield return new TestCaseData<long>(("a", long.MinValue), ("zero", 0), ("one", 1), ("z", long.MaxValue));
        }

        public static IEnumerable<TestCaseData<long?>> NullableInt64TestValues()
        {
            yield return new TestCaseData<long?>();
            yield return new TestCaseData<long?>(("123", 123));
            yield return new TestCaseData<long?>(("123", -123));
            yield return new TestCaseData<long?>(("null", null));
            yield return new TestCaseData<long?>(("null1", null), ("null2", null));
            yield return new TestCaseData<long?>(("a", 1), ("b", null), ("c", 3));
            yield return new TestCaseData<long?>(("a", long.MinValue), ("m", null), ("z", long.MaxValue));
            yield return new TestCaseData<long?>(("a", long.MinValue), ("zero", 0), ("null", null), ("one", 1), ("z", long.MaxValue));
        }

        [TestCaseSource(nameof(Int64TestValues))]
        public void RealmDictionary_WhenUnmanaged_Int64(TestCaseData<long> testData)
        {
            RunUnmanagedTests(o => o.Int64Dictionary, testData);
        }

        [TestCaseSource(nameof(Int64TestValues))]
        public void RealmDictionary_WhenUnmanaged_Int64Counter(TestCaseData<long> testData)
        {
            RunUnmanagedTests(o => o.Int64CounterDictionary, ToInteger(testData));
        }

        [TestCaseSource(nameof(NullableInt64TestValues))]
        public void RealmDictionary_WhenUnmanaged_NullableInt64(TestCaseData<long?> testData)
        {
            //RunUnmanagedTests(o => o.NullableInt64Dictionary, testData);
        }

        [TestCaseSource(nameof(NullableInt64TestValues))]
        public void RealmDictionary_WhenUnmanaged_NullableInt64Counter(TestCaseData<long?> testData)
        {
            //RunUnmanagedTests(o => o.NullableInt64CounterDictionary, ToInteger(testData));
        }

        [TestCaseSource(nameof(Int64TestValues))]
        public void RealmDictionary_WhenManaged_Int64(TestCaseData<long> testData)
        {
            RunManagedTests(o => o.Int64Dictionary, testData);
        }

        [TestCaseSource(nameof(Int64TestValues))]
        public void RealmDictionary_WhenManaged_Int64Counter(TestCaseData<long> testData)
        {
            RunManagedTests(o => o.Int64CounterDictionary, ToInteger(testData));
        }

        [TestCaseSource(nameof(NullableInt64TestValues))]
        public void RealmDictionary_WhenManaged_NullableInt64(TestCaseData<long?> testData)
        {
            //RunManagedTests(o => o.NullableInt64Dictionary, testData);
        }

        [TestCaseSource(nameof(NullableInt64TestValues))]
        public void RealmDictionary_WhenManaged_NullableInt64Counter(TestCaseData<long?> testData)
        {
            //RunManagedTests(o => o.NullableInt64CounterDictionary, ToInteger(testData));
        }

        #endregion

        #region Float

        public static IEnumerable<TestCaseData<float>> FloatTestValues()
        {
            yield return new TestCaseData<float>();
            yield return new TestCaseData<float>(("123", 123.123f));
            yield return new TestCaseData<float>(("123", -123.456f));
            yield return new TestCaseData<float>(("a", 1.1f), ("b", 1.1f), ("c", 1.1f));
            yield return new TestCaseData<float>(("a", 1), ("b", 2.2f), ("c", 3.3f));
            yield return new TestCaseData<float>(("a", float.MinValue), ("z", float.MaxValue));
            yield return new TestCaseData<float>(("a", float.MinValue), ("zero", 0.0f), ("one", 1.1f), ("z", float.MaxValue));
        }

        public static IEnumerable<TestCaseData<float?>> NullableFloatTestValues()
        {
            yield return new TestCaseData<float?>();
            yield return new TestCaseData<float?>(("123", 123.123f));
            yield return new TestCaseData<float?>(("123", -123.456f));
            yield return new TestCaseData<float?>(("null", null));
            yield return new TestCaseData<float?>(("null1", null), ("null2", null));
            yield return new TestCaseData<float?>(("a", 1), ("b", null), ("c", 3.3f));
            yield return new TestCaseData<float?>(("a", float.MinValue), ("m", null), ("z", float.MaxValue));
            yield return new TestCaseData<float?>(("a", float.MinValue), ("zero", 0), ("null", null), ("one", 1.1f), ("z", float.MaxValue));
        }

        [TestCaseSource(nameof(FloatTestValues))]
        public void RealmDictionary_WhenUnmanaged_Float(TestCaseData<float> testData)
        {
            RunUnmanagedTests(o => o.SingleDictionary, testData);
        }

        [TestCaseSource(nameof(NullableFloatTestValues))]
        public void RealmDictionary_WhenUnmanaged_NullableFloat(TestCaseData<float?> testData)
        {
            //RunUnmanagedTests(o => o.NullableSingleDictionary, testData);
        }

        [TestCaseSource(nameof(FloatTestValues))]
        public void RealmDictionary_WhenManaged_Float(TestCaseData<float> testData)
        {
            RunManagedTests(o => o.SingleDictionary, testData);
        }

        [TestCaseSource(nameof(NullableFloatTestValues))]
        public void RealmDictionary_WhenManaged_NullableFloat(TestCaseData<float?> testData)
        {
            //RunManagedTests(o => o.NullableSingleDictionary, testData);
        }

        #endregion

        #region Double

        public static IEnumerable<TestCaseData<double>> DoubleTestValues()
        {
            yield return new TestCaseData<double>();
            yield return new TestCaseData<double>(("123", 123.123));
            yield return new TestCaseData<double>(("123", -123.456));
            yield return new TestCaseData<double>(("a", 1.1), ("b", 1.1), ("c", 1.1));
            yield return new TestCaseData<double>(("a", 1), ("b", 2.2), ("c", 3.3));
            yield return new TestCaseData<double>(("a", 1), ("b", 2.2), ("c", 3.3), ("d", 4385948963486946854968945789458794538793438693486934869.238593285932859238952398));
            yield return new TestCaseData<double>(("a", double.MinValue), ("z", double.MaxValue));
            yield return new TestCaseData<double>(("a", double.MinValue), ("zero", 0.0), ("one", 1.1), ("z", double.MaxValue));
        }

        public static IEnumerable<TestCaseData<double?>> NullableDoubleTestValues()
        {
            yield return new TestCaseData<double?>();
            yield return new TestCaseData<double?>(("123", 123.123));
            yield return new TestCaseData<double?>(("123", -123.456));
            yield return new TestCaseData<double?>(("null", null));
            yield return new TestCaseData<double?>(("null1", null), ("null2", null));
            yield return new TestCaseData<double?>(("a", 1), ("b", null), ("c", 3.3));
            yield return new TestCaseData<double?>(("a", 1), ("b", null), ("c", 3.3), ("d", 4385948963486946854968945789458794538793438693486934869.238593285932859238952398));
            yield return new TestCaseData<double?>(("a", double.MinValue), ("m", null), ("z", double.MaxValue));
            yield return new TestCaseData<double?>(("a", double.MinValue), ("zero", 0), ("null", null), ("one", 1.1), ("z", double.MaxValue));
        }

        [TestCaseSource(nameof(DoubleTestValues))]
        public void RealmDictionary_WhenUnmanaged_Double(TestCaseData<double> testData)
        {
            RunUnmanagedTests(o => o.DoubleDictionary, testData);
        }

        [TestCaseSource(nameof(NullableDoubleTestValues))]
        public void RealmDictionary_WhenUnmanaged_NullableDouble(TestCaseData<double?> testData)
        {
            //RunUnmanagedTests(o => o.NullableDoubleDictionary, testData);
        }

        [TestCaseSource(nameof(DoubleTestValues))]
        public void RealmDictionary_WhenManaged_Double(TestCaseData<double> testData)
        {
            RunManagedTests(o => o.DoubleDictionary, testData);
        }

        [TestCaseSource(nameof(NullableDoubleTestValues))]
        public void RealmDictionary_WhenManaged_NullableDouble(TestCaseData<double?> testData)
        {
            //RunManagedTests(o => o.NullableDoubleDictionary, testData);
        }

        #endregion

        #region Decimal

        public static IEnumerable<TestCaseData<decimal>> DecimalTestValues()
        {
            yield return new TestCaseData<decimal>();
            yield return new TestCaseData<decimal>(("123", 123.123m));
            yield return new TestCaseData<decimal>(("123", -123.456m));
            yield return new TestCaseData<decimal>(("a", 1.1m), ("b", 1.1m), ("c", 1.1m));
            yield return new TestCaseData<decimal>(("a", 1), ("b", 2.2m), ("c", 3.3m));
            yield return new TestCaseData<decimal>(("a", 1), ("b", 2.2m), ("c", 3.3m), ("d", 43859489538793438693486934869.238436346943634634634634634634634634634593285932859238952398m));
            yield return new TestCaseData<decimal>(("a", decimal.MinValue), ("z", decimal.MaxValue));
            yield return new TestCaseData<decimal>(("a", decimal.MinValue), ("zero", 0.0m), ("one", 1.1m), ("z", decimal.MaxValue));
        }

        public static IEnumerable<TestCaseData<decimal?>> NullableDecimalTestValues()
        {
            yield return new TestCaseData<decimal?>();
            yield return new TestCaseData<decimal?>(("123", 123.123m));
            yield return new TestCaseData<decimal?>(("123", -123.456m));
            yield return new TestCaseData<decimal?>(("null", null));
            yield return new TestCaseData<decimal?>(("null1", null), ("null2", null));
            yield return new TestCaseData<decimal?>(("a", 1), ("b", null), ("c", 3.3m));
            yield return new TestCaseData<decimal?>(("a", 1), ("b", null), ("c", 3.3m), ("d", 43859489538793438693486934869.238436346943634634634634634634634634634593285932859238952398m));
            yield return new TestCaseData<decimal?>(("a", decimal.MinValue), ("m", null), ("z", decimal.MaxValue));
            yield return new TestCaseData<decimal?>(("a", decimal.MinValue), ("zero", 0), ("null", null), ("one", 1.1m), ("z", decimal.MaxValue));
        }

        [TestCaseSource(nameof(DecimalTestValues))]
        public void RealmDictionary_WhenUnmanaged_Decimal(TestCaseData<decimal> testData)
        {
            RunUnmanagedTests(o => o.DecimalDictionary, testData);
        }

        [TestCaseSource(nameof(NullableDecimalTestValues))]
        public void RealmDictionary_WhenUnmanaged_NullableDecimal(TestCaseData<decimal?> testData)
        {
            //RunUnmanagedTests(o => o.NullableDecimalDictionary, testData);
        }

        [TestCaseSource(nameof(DecimalTestValues))]
        public void RealmDictionary_WhenManaged_Decimal(TestCaseData<decimal> testData)
        {
            RunManagedTests(o => o.DecimalDictionary, testData);
        }

        [TestCaseSource(nameof(NullableDecimalTestValues))]
        public void RealmDictionary_WhenManaged_NullableDecimal(TestCaseData<decimal?> testData)
        {
            //RunManagedTests(o => o.NullableDecimalDictionary, testData);
        }

        #endregion

        #region Decimal128

        public static IEnumerable<TestCaseData<Decimal128>> Decimal128TestValues()
        {
            yield return new TestCaseData<Decimal128>();
            yield return new TestCaseData<Decimal128>(("123", 123.123m));
            yield return new TestCaseData<Decimal128>(("123", -123.456m));
            yield return new TestCaseData<Decimal128>(("a", 1.1m), ("b", 1.1m), ("c", 1.1m));
            yield return new TestCaseData<Decimal128>(("a", 1), ("b", 2.2m), ("c", 3.3m));
            yield return new TestCaseData<Decimal128>(("a", 1), ("b", 2.2m), ("c", 3.3m), ("d", 43859489538793438693486934869.238436346943634634634634634634634634634593285932859238952398m));
            yield return new TestCaseData<Decimal128>(("a", decimal.MinValue), ("a1", Decimal128.MinValue), ("z", decimal.MaxValue), ("z1", Decimal128.MaxValue));
            yield return new TestCaseData<Decimal128>(("a", Decimal128.MinValue), ("zero", 0.0m), ("one", 1.1m), ("z", Decimal128.MaxValue));
        }

        public static IEnumerable<TestCaseData<Decimal128?>> NullableDecimal128TestValues()
        {
            yield return new TestCaseData<Decimal128?>();
            yield return new TestCaseData<Decimal128?>(("123", 123.123m));
            yield return new TestCaseData<Decimal128?>(("123", -123.456m));
            yield return new TestCaseData<Decimal128?>(("null", null));
            yield return new TestCaseData<Decimal128?>(("null1", null), ("null2", null));
            yield return new TestCaseData<Decimal128?>(("a", 1), ("b", null), ("c", 3.3m));
            yield return new TestCaseData<Decimal128?>(("a", 1), ("b", null), ("c", 3.3m), ("d", 43859489538793438693486934869.238436346943634634634634634634634634634593285932859238952398m));
            yield return new TestCaseData<Decimal128?>(("a", decimal.MinValue), ("a1", Decimal128.MinValue), ("m", null), ("z", decimal.MaxValue), ("z1", Decimal128.MaxValue));
            yield return new TestCaseData<Decimal128?>(("a", Decimal128.MinValue), ("zero", 0), ("null", null), ("one", 1.1m), ("z", Decimal128.MaxValue));
        }

        [TestCaseSource(nameof(Decimal128TestValues))]
        public void RealmDictionary_WhenUnmanaged_Decimal128(TestCaseData<Decimal128> testData)
        {
            RunUnmanagedTests(o => o.Decimal128Dictionary, testData);
        }

        [TestCaseSource(nameof(NullableDecimal128TestValues))]
        public void RealmDictionary_WhenUnmanaged_NullableDecimal128(TestCaseData<Decimal128?> testData)
        {
            //RunUnmanagedTests(o => o.NullableDecimal128Dictionary, testData);
        }

        [TestCaseSource(nameof(Decimal128TestValues))]
        public void RealmDictionary_WhenManaged_Decimal128(TestCaseData<Decimal128> testData)
        {
            RunManagedTests(o => o.Decimal128Dictionary, testData);
        }

        [TestCaseSource(nameof(NullableDecimal128TestValues))]
        public void RealmDictionary_WhenManaged_NullableDecimal128(TestCaseData<Decimal128?> testData)
        {
            //RunManagedTests(o => o.NullableDecimal128Dictionary, testData);
        }

        #endregion

        #region ObjectId

        private static readonly ObjectId ObjectId0 = TestHelpers.GenerateRepetitiveObjectId(0);
        private static readonly ObjectId ObjectId1 = TestHelpers.GenerateRepetitiveObjectId(1);
        private static readonly ObjectId ObjectId2 = TestHelpers.GenerateRepetitiveObjectId(2);
        private static readonly ObjectId ObjectIdMax = TestHelpers.GenerateRepetitiveObjectId(byte.MaxValue);

        public static IEnumerable<TestCaseData<ObjectId>> ObjectIdTestValues()
        {
            yield return new TestCaseData<ObjectId>();
            yield return new TestCaseData<ObjectId>(("123", ObjectId1));
            yield return new TestCaseData<ObjectId>(("123", ObjectId2));
            yield return new TestCaseData<ObjectId>(("a", ObjectId1), ("b", ObjectId1), ("c", ObjectId1));
            yield return new TestCaseData<ObjectId>(("a", ObjectId0), ("b", ObjectId1), ("c", ObjectId2));
            yield return new TestCaseData<ObjectId>(("a", ObjectId0), ("z", ObjectIdMax));
            yield return new TestCaseData<ObjectId>(("a", ObjectId0), ("zero", ObjectId1), ("one", ObjectId2), ("z", ObjectIdMax));
        }

        public static IEnumerable<TestCaseData<ObjectId?>> NullableObjectIdTestValues()
        {
            yield return new TestCaseData<ObjectId?>();
            yield return new TestCaseData<ObjectId?>(("123", ObjectId1));
            yield return new TestCaseData<ObjectId?>(("123", ObjectId2));
            yield return new TestCaseData<ObjectId?>(("null", null));
            yield return new TestCaseData<ObjectId?>(("null1", null), ("null2", null));
            yield return new TestCaseData<ObjectId?>(("a", ObjectId0), ("b", null), ("c", ObjectId2));
            yield return new TestCaseData<ObjectId?>(("a", ObjectId2), ("b", null), ("c", ObjectId1), ("d", ObjectId0));
            yield return new TestCaseData<ObjectId?>(("a", ObjectId0), ("m", null), ("z", ObjectIdMax));
            yield return new TestCaseData<ObjectId?>(("a", ObjectId0), ("zero", ObjectId1), ("null", null), ("one", ObjectId2), ("z", ObjectIdMax));
        }

        [TestCaseSource(nameof(ObjectIdTestValues))]
        public void RealmDictionary_WhenUnmanaged_ObjectId(TestCaseData<ObjectId> testData)
        {
            RunUnmanagedTests(o => o.ObjectIdDictionary, testData);
        }

        [TestCaseSource(nameof(NullableObjectIdTestValues))]
        public void RealmDictionary_WhenUnmanaged_NullableObjectId(TestCaseData<ObjectId?> testData)
        {
            //RunUnmanagedTests(o => o.NullableObjectIdDictionary, testData);
        }

        [TestCaseSource(nameof(ObjectIdTestValues))]
        public void RealmDictionary_WhenManaged_ObjectId(TestCaseData<ObjectId> testData)
        {
            RunManagedTests(o => o.ObjectIdDictionary, testData);
        }

        [TestCaseSource(nameof(NullableObjectIdTestValues))]
        public void RealmDictionary_WhenManaged_NullableObjectId(TestCaseData<ObjectId?> testData)
        {
            //RunManagedTests(o => o.NullableObjectIdDictionary, testData);
        }

        #endregion

        #region DateTimeOffset

        private static readonly DateTimeOffset Date0 = new DateTimeOffset(0, TimeSpan.Zero);
        private static readonly DateTimeOffset Date1 = new DateTimeOffset(1999, 3, 4, 5, 30, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset Date2 = new DateTimeOffset(2030, 1, 3, 9, 25, 34, TimeSpan.FromHours(3));

        public static IEnumerable<TestCaseData<DateTimeOffset>> DateTimeOffsetTestValues()
        {
            yield return new TestCaseData<DateTimeOffset>();
            yield return new TestCaseData<DateTimeOffset>(("123", Date1));
            yield return new TestCaseData<DateTimeOffset>(("123", Date2));
            yield return new TestCaseData<DateTimeOffset>(("a", Date1), ("b", Date1), ("c", Date1));
            yield return new TestCaseData<DateTimeOffset>(("a", Date0), ("b", Date1), ("c", Date2));
            yield return new TestCaseData<DateTimeOffset>(("a", DateTimeOffset.MinValue), ("z", DateTimeOffset.MaxValue));
            yield return new TestCaseData<DateTimeOffset>(("a", DateTimeOffset.MinValue), ("zero", Date1), ("one", Date2), ("z", DateTimeOffset.MaxValue));
        }

        public static IEnumerable<TestCaseData<DateTimeOffset?>> NullableDateTimeOffsetTestValues()
        {
            yield return new TestCaseData<DateTimeOffset?>();
            yield return new TestCaseData<DateTimeOffset?>(("123", Date1));
            yield return new TestCaseData<DateTimeOffset?>(("123", Date2));
            yield return new TestCaseData<DateTimeOffset?>(("null", null));
            yield return new TestCaseData<DateTimeOffset?>(("null1", null), ("null2", null));
            yield return new TestCaseData<DateTimeOffset?>(("a", Date0), ("b", null), ("c", Date2));
            yield return new TestCaseData<DateTimeOffset?>(("a", Date2), ("b", null), ("c", Date1), ("d", Date0));
            yield return new TestCaseData<DateTimeOffset?>(("a", DateTimeOffset.MinValue), ("m", null), ("z", DateTimeOffset.MaxValue));
            yield return new TestCaseData<DateTimeOffset?>(("a", DateTimeOffset.MinValue), ("zero", Date1), ("null", null), ("one", Date2), ("z", DateTimeOffset.MaxValue));
        }

        [TestCaseSource(nameof(DateTimeOffsetTestValues))]
        public void RealmDictionary_WhenUnmanaged_DateTimeOffset(TestCaseData<DateTimeOffset> testData)
        {
            RunUnmanagedTests(o => o.DateTimeOffsetDictionary, testData);
        }

        [TestCaseSource(nameof(NullableDateTimeOffsetTestValues))]
        public void RealmDictionary_WhenUnmanaged_NullableDateTimeOffset(TestCaseData<DateTimeOffset?> testData)
        {
            //RunUnmanagedTests(o => o.NullableDateTimeOffsetDictionary, testData);
        }

        [TestCaseSource(nameof(DateTimeOffsetTestValues))]
        public void RealmDictionary_WhenManaged_DateTimeOffset(TestCaseData<DateTimeOffset> testData)
        {
            RunManagedTests(o => o.DateTimeOffsetDictionary, testData);
        }

        [TestCaseSource(nameof(NullableDateTimeOffsetTestValues))]
        public void RealmDictionary_WhenManaged_NullableDateTimeOffset(TestCaseData<DateTimeOffset?> testData)
        {
            //RunManagedTests(o => o.NullableDateTimeOffsetDictionary, testData);
        }

        #endregion

        #region String

        public static IEnumerable<TestCaseData<string>> StringTestValues()
        {
            yield return new TestCaseData<string>();
            yield return new TestCaseData<string>(("123", "abc"));
            yield return new TestCaseData<string>(("123", "ced"));
            yield return new TestCaseData<string>(("a", "AbCdEfG"), ("b", "HiJklMn"), ("c", "OpQrStU"));
            yield return new TestCaseData<string>(("a", "vwxyz"), ("b", string.Empty), ("c", " "));
            yield return new TestCaseData<string>(("a", string.Empty), ("z", "aa bb cc dd ee ff gg hh ii jj kk ll mm nn oo pp qq rr ss tt uu vv ww xx yy zz"));
            yield return new TestCaseData<string>(("a", string.Empty), ("zero", "lorem ipsum"), ("one", "-1234567890"), ("z", "lololo"));
        }

        public static IEnumerable<TestCaseData<string>> NullableStringTestValues()
        {
            yield return new TestCaseData<string>();
            yield return new TestCaseData<string>(("123", "abc"));
            yield return new TestCaseData<string>(("123", "ced"));
            yield return new TestCaseData<string>(("null", null));
            yield return new TestCaseData<string>(("null1", null), ("null2", null));
            yield return new TestCaseData<string>(("a", "AbCdEfG"), ("b", null), ("c", "OpQrStU"));
            yield return new TestCaseData<string>(("a", "vwxyz"), ("b", null), ("c", string.Empty), ("d", " "));
            yield return new TestCaseData<string>(("a", string.Empty), ("m", null), ("z", "aa bb cc dd ee ff gg hh ii jj kk ll mm nn oo pp qq rr ss tt uu vv ww xx yy zz"));
            yield return new TestCaseData<string>(("a", string.Empty), ("zero", "lorem ipsum"), ("null", null), ("one", "-1234567890"), ("z", "lololo"));
        }

        [TestCaseSource(nameof(StringTestValues))]
        public void RealmDictionary_WhenUnmanaged_String(TestCaseData<string> testData)
        {
            RunUnmanagedTests(o => o.StringDictionary, testData);
        }

        [TestCaseSource(nameof(NullableStringTestValues))]
        public void RealmDictionary_WhenUnmanaged_NullableString(TestCaseData<string> testData)
        {
            //RunUnmanagedTests(o => o.NullableStringDictionary, testData);
        }

        [TestCaseSource(nameof(StringTestValues))]
        public void RealmDictionary_WhenManaged_String(TestCaseData<string> testData)
        {
            RunManagedTests(o => o.StringDictionary, testData);
        }

        [TestCaseSource(nameof(NullableStringTestValues))]
        public void RealmDictionary_WhenManaged_NullableString(TestCaseData<string> testData)
        {
            //RunManagedTests(o => o.NullableStringDictionary, testData);
        }

        #endregion

        private static void RunUnmanagedTests<T>(Func<DictionariesObject, IDictionary<string, T>> accessor, TestCaseData<T> testData)
        {
            var testObject = new DictionariesObject();
            var dictionary = accessor(testObject);

            // We can't freeze unmanaged dictionaries.
            Assert.Throws<RealmException>(() => dictionary.Freeze());

            testData.AssertCount(dictionary);
        }

        private void RunManagedTests<T>(Func<DictionariesObject, IDictionary<string, T>> accessor, TestCaseData<T> testData)
        {
            var testObject = new DictionariesObject();
            var dictionary = accessor(testObject);

            testData.Seed(dictionary);

            _realm.Write(() =>
            {
                _realm.Add(testObject);
            });

            var managedDictionary = accessor(testObject);
            Assert.That(dictionary, Is.Not.SameAs(managedDictionary));

            Assert.That(managedDictionary, Is.EquivalentTo(testData.GetReferenceDictionary()));

            testData.AssertCount(managedDictionary);
        }

        private static TestCaseData<RealmInteger<T>> ToInteger<T>(TestCaseData<T> data)
            where T : struct, IComparable<T>, IFormattable, IConvertible, IEquatable<T>
        {
            return new TestCaseData<RealmInteger<T>>(data.InitialValues.ToIntegerTuple());
        }

        private static TestCaseData<RealmInteger<T>?> ToInteger<T>(TestCaseData<T?> data)
            where T : struct, IComparable<T>, IFormattable, IConvertible, IEquatable<T>
        {
            return new TestCaseData<RealmInteger<T>?>(data.InitialValues.ToIntegerTuple());
        }

        public class TestCaseData<T>
        {
            public (string Key, T Value)[] InitialValues { get; }

            public TestCaseData(params (string Key, T Value)[] initialValues)
            {
                InitialValues = initialValues.ToArray();
            }

            public override string ToString()
            {
                return $"{typeof(T).Name}: {{{string.Join(",", InitialValues.Select(kvp => $"({kvp.Key}, {kvp.Value})"))}}}";
            }

            public void AssertCount(IDictionary<string, T> target)
            {
                Seed(target);
                var reference = GetReferenceDictionary();

                Assert.That(target.Count, Is.EqualTo(reference.Count));
                Assert.That(target, Is.EquivalentTo(reference));
            }

            public void Seed(IDictionary<string, T> target, IEnumerable<(string Key, T Value)> values = null)
            {
                WriteIfNecessary(target, () =>
                {
                    target.Clear();
                    foreach (var (key, value) in values ?? InitialValues)
                    {
                        target.Add(key, value);
                    }
                });
            }

            private static void WriteIfNecessary(IDictionary<string, T> collection, Action writeAction)
            {
                Transaction transaction = null;
                if (collection is RealmDictionary<T> realmDictionary)
                {
                    transaction = realmDictionary.Realm.BeginWrite();
                }

                writeAction();

                transaction?.Commit();
            }

            public IDictionary<string, T> GetReferenceDictionary() => InitialValues.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
