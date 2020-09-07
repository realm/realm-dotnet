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
using MongoDB.Bson;
using NUnit.Framework;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class ConvertTests : RealmInstanceTest
    {
        private const byte ByteOne = 1;
        private const short ShortOne = 1;
        private const long LongOne = 1;
        private const float FloatOne = 1;
        private const double DoubleOne = 1;
        private const decimal DecimalOne = 1;
        private static readonly Decimal128 Decimal128One = 1;

        private static readonly int? NullableOne = 1;
        private static readonly int? NullableZero = 0;

        private static readonly DateTimeOffset TwoThousandSeventeen = new DateTimeOffset(2017, 1, 1, 0, 0, 0, TimeSpan.Zero);

        protected override void CustomSetUp()
        {
            base.CustomSetUp();
            _realm.Write(() =>
            {
                _realm.Add(new AllTypesObject
                {
                    RequiredStringProperty = string.Empty
                });
                _realm.Add(new AllTypesObject
                {
                    BooleanProperty = true,
                    ByteProperty = 1,
                    CharProperty = 'a',
                    DateTimeOffsetProperty = TwoThousandSeventeen,
                    DoubleProperty = 1,
                    Int16Property = 1,
                    Int32Property = 1,
                    Int64Property = 1,
                    NullableBooleanProperty = true,
                    NullableByteProperty = 1,
                    NullableCharProperty = 'a',
                    NullableDateTimeOffsetProperty = TwoThousandSeventeen,
                    NullableDoubleProperty = 1,
                    NullableInt16Property = 1,
                    NullableInt32Property = 1,
                    NullableInt64Property = 1,
                    NullableSingleProperty = 1,
                    SingleProperty = 1,
                    RequiredStringProperty = string.Empty,
                    DecimalProperty = 1,
                    Decimal128Property = 1,
                    NullableDecimalProperty = 1,
                    NullableDecimal128Property = 1,
                });

                _realm.Add(new CounterObject());
                _realm.Add(new CounterObject
                {
                    Id = 1,
                    ByteProperty = 1,
                    Int16Property = 1,
                    Int32Property = 1,
                    Int64Property = 1,
                    NullableByteProperty = 1,
                    NullableInt16Property = 1,
                    NullableInt32Property = 1,
                    NullableInt64Property = 1
                });
            });
        }

        [Test]
        public void Equal_WhenUsingBroaderType_Byte()
        {
            var byteQuery = _realm.All<AllTypesObject>().Where(o => o.ByteProperty == LongOne).ToArray();
            Assert.That(byteQuery.Length, Is.EqualTo(1));
            Assert.That(byteQuery[0].Int64Property, Is.EqualTo(LongOne));
        }

        [Test]
        public void Equal_WhenUsingBroaderType_Int16()
        {
            var int16Query = _realm.All<AllTypesObject>().Where(o => o.Int16Property == LongOne).ToArray();
            Assert.That(int16Query.Length, Is.EqualTo(1));
            Assert.That(int16Query[0].Int64Property, Is.EqualTo(LongOne));
        }

        [Test]
        public void Equal_WhenUsingBroaderType_Int32()
        {
            var int32Query = _realm.All<AllTypesObject>().Where(o => o.Int32Property == LongOne).ToArray();
            Assert.That(int32Query.Length, Is.EqualTo(1));
            Assert.That(int32Query[0].Int64Property, Is.EqualTo(LongOne));
        }

        [Test]
        public void Equal_WhenUsingBroaderType_Single()
        {
            var singleQuery = _realm.All<AllTypesObject>().Where(o => o.SingleProperty == DoubleOne).ToArray();
            Assert.That(singleQuery.Length, Is.EqualTo(1));
            Assert.That(singleQuery[0].DoubleProperty, Is.EqualTo(DoubleOne));
        }

        [Test]
        public void Equal_WhenUsingBroaderType_Decimal()
        {
            var singleQuery = _realm.All<AllTypesObject>().Where(o => o.DecimalProperty == Decimal128One).ToArray();
            Assert.That(singleQuery.Length, Is.EqualTo(1));
            Assert.That(singleQuery[0].DoubleProperty, Is.EqualTo(DoubleOne));
        }

        [Test]
        public void Equal_WhenUsingBroaderType_ByteInteger()
        {
            var byteIntegerQuery = _realm.All<CounterObject>().Where(o => o.ByteProperty == LongOne).ToArray();
            Assert.That(byteIntegerQuery.Length, Is.EqualTo(1));
            Assert.That(byteIntegerQuery[0].Id, Is.EqualTo(1));
        }

        [Test]
        public void Equal_WhenUsingBroaderType_Int16Integer()
        {
            var int16IntegerQuery = _realm.All<CounterObject>().Where(o => o.Int16Property == LongOne).ToArray();
            Assert.That(int16IntegerQuery.Length, Is.EqualTo(1));
            Assert.That(int16IntegerQuery[0].Id, Is.EqualTo(1));
        }

        [Test]
        public void Equal_WhenUsingBroaderType_Int32Integer()
        {
            var int32IntegerQuery = _realm.All<CounterObject>().Where(o => o.Int32Property == LongOne).ToArray();
            Assert.That(int32IntegerQuery.Length, Is.EqualTo(1));
            Assert.That(int32IntegerQuery[0].Id, Is.EqualTo(1));
        }

        [Test]
        public void Equal_WhenUsingNarrowerType_Int16()
        {
            var int16Query = _realm.All<AllTypesObject>().Where(o => o.Int16Property == ByteOne).ToArray();
            Assert.That(int16Query.Length, Is.EqualTo(1));
            Assert.That(int16Query[0].ByteProperty, Is.EqualTo(ByteOne));
        }

        [Test]
        public void Equal_WhenUsingNarrowerType_Int32()
        {
            var int32Query = _realm.All<AllTypesObject>().Where(o => o.Int32Property == ByteOne).ToArray();
            Assert.That(int32Query.Length, Is.EqualTo(1));
            Assert.That(int32Query[0].ByteProperty, Is.EqualTo(ByteOne));
        }

        [Test]
        public void Equal_WhenUsingNarrowerType_Int64()
        {
            var longQuery = _realm.All<AllTypesObject>().Where(o => o.Int64Property == ByteOne).ToArray();
            Assert.That(longQuery.Length, Is.EqualTo(1));
            Assert.That(longQuery[0].ByteProperty, Is.EqualTo(ByteOne));
        }

        [Test]
        public void Equal_WhenUsingNarrowerType_Double()
        {
            var doubleQuery = _realm.All<AllTypesObject>().Where(o => o.DoubleProperty == FloatOne).ToArray();
            Assert.That(doubleQuery.Length, Is.EqualTo(1));
            Assert.That(doubleQuery[0].DoubleProperty, Is.EqualTo(1.0));
        }

        [Test]
        public void Equal_WhenUsingNarrowerType_Decimal()
        {
            var decimalQuery = _realm.All<AllTypesObject>().Where(o => o.DecimalProperty == ByteOne).ToArray();
            Assert.That(decimalQuery.Length, Is.EqualTo(1));
            Assert.That(decimalQuery[0].DecimalProperty, Is.EqualTo(DecimalOne));
        }

        [Test]
        public void Equal_WhenUsingNarrowerType_Decimal128()
        {
            var decimalByDecimalQuery = _realm.All<AllTypesObject>().Where(o => o.Decimal128Property == DecimalOne).ToArray();
            Assert.That(decimalByDecimalQuery.Length, Is.EqualTo(1));
            Assert.That(decimalByDecimalQuery[0].Decimal128Property, Is.EqualTo(Decimal128One));

            var decimalByLongQuery = _realm.All<AllTypesObject>().Where(o => o.Decimal128Property == LongOne).ToArray();
            Assert.That(decimalByLongQuery.Length, Is.EqualTo(1));
            Assert.That(decimalByLongQuery[0].Decimal128Property, Is.EqualTo(Decimal128One));

            var decimalByByteQuery = _realm.All<AllTypesObject>().Where(o => o.Decimal128Property == ByteOne).ToArray();
            Assert.That(decimalByByteQuery.Length, Is.EqualTo(1));
            Assert.That(decimalByByteQuery[0].Decimal128Property, Is.EqualTo(Decimal128One));
        }

        [Test]
        public void Equal_WhenUsingNarrowerType_Int16Integer()
        {
            var int16IntegerQuery = _realm.All<CounterObject>().Where(o => o.Int16Property == ByteOne).ToArray();
            Assert.That(int16IntegerQuery.Length, Is.EqualTo(1));
            Assert.That(int16IntegerQuery[0].Id, Is.EqualTo(1));
        }

        [Test]
        public void Equal_WhenUsingNarrowerType_Int32Integer()
        {
            var int32IntegerQuery = _realm.All<CounterObject>().Where(o => o.Int32Property == ByteOne).ToArray();
            Assert.That(int32IntegerQuery.Length, Is.EqualTo(1));
            Assert.That(int32IntegerQuery[0].Id, Is.EqualTo(1));
        }

        [Test]
        public void Equal_WhenUsingNarrowerType_Int64Integer()
        {
            var int64IntegerQuery = _realm.All<CounterObject>().Where(o => o.Int64Property == ByteOne).ToArray();
            Assert.That(int64IntegerQuery.Length, Is.EqualTo(1));
            Assert.That(int64IntegerQuery[0].Id, Is.EqualTo(1));
        }

        [Test]
        public void Equal_WhenPropertyIsNullable_Bool()
        {
            var boolQuery = _realm.All<AllTypesObject>().Where(o => o.NullableBooleanProperty == true).ToArray();
            Assert.That(boolQuery.Length, Is.EqualTo(1));
            Assert.That(boolQuery[0].NullableBooleanProperty, Is.EqualTo(true));

            boolQuery = _realm.All<AllTypesObject>().Where(o => o.NullableBooleanProperty != null).ToArray();
            Assert.That(boolQuery.Length, Is.EqualTo(1));
            Assert.That(boolQuery[0].NullableBooleanProperty, Is.EqualTo(true));
        }

        [Test]
        public void Equal_WhenPropertyIsNullable_Byte()
        {
            var byteQuery = _realm.All<AllTypesObject>().Where(o => o.NullableByteProperty == ByteOne).ToArray();
            Assert.That(byteQuery.Length, Is.EqualTo(1));
            Assert.That(byteQuery[0].NullableByteProperty, Is.EqualTo(ByteOne));

            byteQuery = _realm.All<AllTypesObject>().Where(o => o.NullableByteProperty != null).ToArray();
            Assert.That(byteQuery.Length, Is.EqualTo(1));
            Assert.That(byteQuery[0].NullableByteProperty, Is.EqualTo(ByteOne));
        }

        [Test]
        public void Equal_WhenPropertyIsNullable_Char()
        {
            var charQuery = _realm.All<AllTypesObject>().Where(o => o.NullableCharProperty == 'a').ToArray();
            Assert.That(charQuery.Length, Is.EqualTo(1));
            Assert.That(charQuery[0].NullableCharProperty, Is.EqualTo('a'));

            charQuery = _realm.All<AllTypesObject>().Where(o => o.NullableCharProperty != null).ToArray();
            Assert.That(charQuery.Length, Is.EqualTo(1));
            Assert.That(charQuery[0].NullableCharProperty, Is.EqualTo('a'));
        }

        [Test]
        public void Equal_WhenPropertyIsNullable_Date()
        {
            var dateQuery = _realm.All<AllTypesObject>().Where(o => o.NullableDateTimeOffsetProperty == TwoThousandSeventeen).ToArray();
            Assert.That(dateQuery.Length, Is.EqualTo(1));
            Assert.That(dateQuery[0].NullableDateTimeOffsetProperty, Is.EqualTo(TwoThousandSeventeen));

            dateQuery = _realm.All<AllTypesObject>().Where(o => o.NullableDateTimeOffsetProperty != null).ToArray();
            Assert.That(dateQuery.Length, Is.EqualTo(1));
            Assert.That(dateQuery[0].NullableDateTimeOffsetProperty, Is.EqualTo(TwoThousandSeventeen));
        }

        [Test]
        public void Equal_WhenPropertyIsNullable_Double()
        {
            var doubleQuery = _realm.All<AllTypesObject>().Where(o => o.NullableDoubleProperty == 1.0).ToArray();
            Assert.That(doubleQuery.Length, Is.EqualTo(1));
            Assert.That(doubleQuery[0].NullableDoubleProperty, Is.EqualTo(1.0));

            doubleQuery = _realm.All<AllTypesObject>().Where(o => o.NullableDoubleProperty != null).ToArray();
            Assert.That(doubleQuery.Length, Is.EqualTo(1));
            Assert.That(doubleQuery[0].NullableDoubleProperty, Is.EqualTo(1.0));
        }

        [Test]
        public void Equal_WhenPropertyIsNullable_Decimal()
        {
            var decimalQuery = _realm.All<AllTypesObject>().Where(o => o.NullableDecimalProperty == DecimalOne).ToArray();
            Assert.That(decimalQuery.Length, Is.EqualTo(1));
            Assert.That(decimalQuery[0].NullableDecimalProperty, Is.EqualTo(1));

            decimalQuery = _realm.All<AllTypesObject>().Where(o => o.NullableDecimalProperty == LongOne).ToArray();
            Assert.That(decimalQuery.Length, Is.EqualTo(1));
            Assert.That(decimalQuery[0].NullableDecimalProperty, Is.EqualTo(1));

            decimalQuery = _realm.All<AllTypesObject>().Where(o => o.NullableDecimalProperty == Decimal128One).ToArray();
            Assert.That(decimalQuery.Length, Is.EqualTo(1));
            Assert.That(decimalQuery[0].NullableDecimalProperty, Is.EqualTo(1));

            decimalQuery = _realm.All<AllTypesObject>().Where(o => o.NullableDecimalProperty != null).ToArray();
            Assert.That(decimalQuery.Length, Is.EqualTo(1));
            Assert.That(decimalQuery[0].NullableDecimalProperty, Is.EqualTo(1));
        }

        [Test]
        public void Equal_WhenPropertyIsNullable_Decimal128()
        {
            var decimalQuery = _realm.All<AllTypesObject>().Where(o => o.NullableDecimal128Property == Decimal128One).ToArray();
            Assert.That(decimalQuery.Length, Is.EqualTo(1));
            Assert.That(decimalQuery[0].NullableDecimal128Property, Is.EqualTo(Decimal128One));

            decimalQuery = _realm.All<AllTypesObject>().Where(o => o.NullableDecimal128Property == DecimalOne).ToArray();
            Assert.That(decimalQuery.Length, Is.EqualTo(1));
            Assert.That(decimalQuery[0].NullableDecimal128Property, Is.EqualTo(Decimal128One));

            decimalQuery = _realm.All<AllTypesObject>().Where(o => o.NullableDecimal128Property == LongOne).ToArray();
            Assert.That(decimalQuery.Length, Is.EqualTo(1));
            Assert.That(decimalQuery[0].NullableDecimal128Property, Is.EqualTo(Decimal128One));

            decimalQuery = _realm.All<AllTypesObject>().Where(o => o.NullableDecimal128Property != null).ToArray();
            Assert.That(decimalQuery.Length, Is.EqualTo(1));
            Assert.That(decimalQuery[0].NullableDecimal128Property, Is.EqualTo(Decimal128One));
        }

        [Test]
        public void Equal_WhenPropertyIsNullable_Int16()
        {
            var shortQuery = _realm.All<AllTypesObject>().Where(o => o.NullableInt16Property == ShortOne).ToArray();
            Assert.That(shortQuery.Length, Is.EqualTo(1));
            Assert.That(shortQuery[0].NullableInt16Property, Is.EqualTo(ShortOne));

            shortQuery = _realm.All<AllTypesObject>().Where(o => o.NullableInt16Property != null).ToArray();
            Assert.That(shortQuery.Length, Is.EqualTo(1));
            Assert.That(shortQuery[0].NullableInt16Property, Is.EqualTo(ShortOne));
        }

        [Test]
        public void Equal_WhenPropertyIsNullable_Int32()
        {
            var intQuery = _realm.All<AllTypesObject>().Where(o => o.NullableInt32Property == 1).ToArray();
            Assert.That(intQuery.Length, Is.EqualTo(1));
            Assert.That(intQuery[0].NullableInt32Property, Is.EqualTo(1));

            intQuery = _realm.All<AllTypesObject>().Where(o => o.NullableInt32Property != null).ToArray();
            Assert.That(intQuery.Length, Is.EqualTo(1));
            Assert.That(intQuery[0].NullableInt32Property, Is.EqualTo(1));
        }

        [Test]
        public void Equal_WhenPropertyIsNullable_Int64()
        {
            var longQuery = _realm.All<AllTypesObject>().Where(o => o.NullableInt64Property == 1L).ToArray();
            Assert.That(longQuery.Length, Is.EqualTo(1));
            Assert.That(longQuery[0].NullableInt64Property, Is.EqualTo(1L));

            longQuery = _realm.All<AllTypesObject>().Where(o => o.NullableInt64Property != null).ToArray();
            Assert.That(longQuery.Length, Is.EqualTo(1));
            Assert.That(longQuery[0].NullableInt64Property, Is.EqualTo(1L));
        }

        [Test]
        public void Equal_WhenPropertyIsNullable_Float()
        {
            var floatQuery = _realm.All<AllTypesObject>().Where(o => o.NullableSingleProperty == 1f).ToArray();
            Assert.That(floatQuery.Length, Is.EqualTo(1));
            Assert.That(floatQuery[0].NullableSingleProperty, Is.EqualTo(1f));

            floatQuery = _realm.All<AllTypesObject>().Where(o => o.NullableSingleProperty != null).ToArray();
            Assert.That(floatQuery.Length, Is.EqualTo(1));
            Assert.That(floatQuery[0].NullableSingleProperty, Is.EqualTo(1f));
        }

        [Test]
        public void Equal_WhenPropertyIsNullable_ByteInteger()
        {
            var byteIntegerQuery = _realm.All<CounterObject>().Where(o => o.NullableByteProperty != null).ToArray();
            Assert.That(byteIntegerQuery.Length, Is.EqualTo(1));
            Assert.That(byteIntegerQuery[0].Id, Is.EqualTo(1));

            byteIntegerQuery = _realm.All<CounterObject>().Where(o => o.NullableByteProperty == ByteOne).ToArray();
            Assert.That(byteIntegerQuery.Length, Is.EqualTo(1));
            Assert.That(byteIntegerQuery[0].Id, Is.EqualTo(1));
        }

        [Test]
        public void Equal_WhenPropertyIsNullable_Int16Integer()
        {
            var int16IntegerQuery = _realm.All<CounterObject>().Where(o => o.NullableInt16Property != null).ToArray();
            Assert.That(int16IntegerQuery.Length, Is.EqualTo(1));
            Assert.That(int16IntegerQuery[0].Id, Is.EqualTo(1));

            int16IntegerQuery = _realm.All<CounterObject>().Where(o => o.NullableInt16Property == ShortOne).ToArray();
            Assert.That(int16IntegerQuery.Length, Is.EqualTo(1));
            Assert.That(int16IntegerQuery[0].Id, Is.EqualTo(1));
        }

        [Test]
        public void Equal_WhenPropertyIsNullable_Int32Integer()
        {
            var int32IntegerQuery = _realm.All<CounterObject>().Where(o => o.NullableInt32Property != null).ToArray();
            Assert.That(int32IntegerQuery.Length, Is.EqualTo(1));
            Assert.That(int32IntegerQuery[0].Id, Is.EqualTo(1));

            int32IntegerQuery = _realm.All<CounterObject>().Where(o => o.NullableInt32Property == 1).ToArray();
            Assert.That(int32IntegerQuery.Length, Is.EqualTo(1));
            Assert.That(int32IntegerQuery[0].Id, Is.EqualTo(1));
        }

        [Test]
        public void Equal_WhenPropertyIsNullable_Int64Integer()
        {
            var int64IntegerQuery = _realm.All<CounterObject>().Where(o => o.NullableInt64Property != null).ToArray();
            Assert.That(int64IntegerQuery.Length, Is.EqualTo(1));
            Assert.That(int64IntegerQuery[0].Id, Is.EqualTo(1));

            int64IntegerQuery = _realm.All<CounterObject>().Where(o => o.NullableInt64Property == 1L).ToArray();
            Assert.That(int64IntegerQuery.Length, Is.EqualTo(1));
            Assert.That(int64IntegerQuery[0].Id, Is.EqualTo(1));
        }

        [Test]
        public void Equal_WhenVariableIsNullable_Bool()
        {
            bool? boolValue = true;
            var boolQuery = _realm.All<AllTypesObject>().Where(o => o.BooleanProperty == boolValue).ToArray();
            Assert.That(boolQuery.Length, Is.EqualTo(1));
            Assert.That(boolQuery[0].BooleanProperty, Is.EqualTo(true));
        }

        [Test]
        public void Equal_WhenVariableIsNullable_Byte()
        {
            byte? byteValue = 1;
            var byteQuery = _realm.All<AllTypesObject>().Where(o => o.ByteProperty == byteValue).ToArray();
            Assert.That(byteQuery.Length, Is.EqualTo(1));
            Assert.That(byteQuery[0].ByteProperty, Is.EqualTo(byteValue));
        }

        [Test]
        public void Equal_WhenVariableIsNullable_Char()
        {
            char? charValue = 'a';
            var charQuery = _realm.All<AllTypesObject>().Where(o => o.CharProperty == charValue).ToArray();
            Assert.That(charQuery.Length, Is.EqualTo(1));
            Assert.That(charQuery[0].CharProperty, Is.EqualTo('a'));
        }

        [Test]
        public void Equal_WhenVariableIsNullable_Date()
        {
            DateTimeOffset? dateValue = TwoThousandSeventeen;
            var dateQuery = _realm.All<AllTypesObject>().Where(o => o.DateTimeOffsetProperty == dateValue).ToArray();
            Assert.That(dateQuery.Length, Is.EqualTo(1));
            Assert.That(dateQuery[0].DateTimeOffsetProperty, Is.EqualTo(dateValue));
        }

        [Test]
        public void Equal_WhenVariableIsNullable_Double()
        {
            double? doubleValue = 1.0;
            var doubleQuery = _realm.All<AllTypesObject>().Where(o => o.DoubleProperty == doubleValue).ToArray();
            Assert.That(doubleQuery.Length, Is.EqualTo(1));
            Assert.That(doubleQuery[0].DoubleProperty, Is.EqualTo(doubleValue));
        }

        [Test]
        public void Equal_WhenVariableIsNullable_Decimal()
        {
            decimal? decimalValue = 1M;
            var doubleQuery = _realm.All<AllTypesObject>().Where(o => o.DecimalProperty == decimalValue).ToArray();
            Assert.That(doubleQuery.Length, Is.EqualTo(1));
            Assert.That(doubleQuery[0].DecimalProperty, Is.EqualTo(decimalValue));
        }

        [Test]
        public void Equal_WhenVariableIsNullable_Decimal128()
        {
            Decimal128? decimalValue = 1;
            var doubleQuery = _realm.All<AllTypesObject>().Where(o => o.Decimal128Property == decimalValue).ToArray();
            Assert.That(doubleQuery.Length, Is.EqualTo(1));
            Assert.That(doubleQuery[0].Decimal128Property, Is.EqualTo(decimalValue));
        }


        [Test]
        public void Equal_WhenVariableIsNullable_Int16()
        {
            short? shortValue = 1;
            var shortQuery = _realm.All<AllTypesObject>().Where(o => o.Int16Property == shortValue).ToArray();
            Assert.That(shortQuery.Length, Is.EqualTo(1));
            Assert.That(shortQuery[0].Int16Property, Is.EqualTo(shortValue));
        }

        [Test]
        public void Equal_WhenVariableIsNullable_Int32()
        {
            int? intValue = 1;
            var intQuery = _realm.All<AllTypesObject>().Where(o => o.Int32Property == intValue).ToArray();
            Assert.That(intQuery.Length, Is.EqualTo(1));
            Assert.That(intQuery[0].Int32Property, Is.EqualTo(1));
        }

        [Test]
        public void Equal_WhenVariableIsNullable_Int64()
        {
            long? longValue = 1;
            var longQuery = _realm.All<AllTypesObject>().Where(o => o.Int64Property == longValue).ToArray();
            Assert.That(longQuery.Length, Is.EqualTo(1));
            Assert.That(longQuery[0].Int64Property, Is.EqualTo(longValue));
        }

        [Test]
        public void Equal_WhenVariableIsNullable_Float()
        {
            float? floatValue = 1;
            var floatQuery = _realm.All<AllTypesObject>().Where(o => o.SingleProperty == floatValue).ToArray();
            Assert.That(floatQuery.Length, Is.EqualTo(1));
            Assert.That(floatQuery[0].SingleProperty, Is.EqualTo(floatValue));
        }

        [Test]
        public void GreaterThan_WhenPropertyIsNullable()
        {
            var intQuery = _realm.All<AllTypesObject>().Where(o => o.NullableInt32Property > 0).ToArray();
            Assert.That(intQuery.Length, Is.EqualTo(1));
            Assert.That(intQuery[0].NullableInt32Property, Is.EqualTo(1));
        }

        [Test]
        public void GreaterThanOrEqual_WhenPropertyIsNullable()
        {
            var intQuery = _realm.All<AllTypesObject>().Where(o => o.NullableInt32Property >= 1).ToArray();
            Assert.That(intQuery.Length, Is.EqualTo(1));
            Assert.That(intQuery[0].NullableInt32Property, Is.EqualTo(1));
        }

        [Test]
        public void LessThan_WhenPropertyIsNullable()
        {
            var intQuery = _realm.All<AllTypesObject>().Where(o => o.NullableInt32Property < 2).ToArray();
            Assert.That(intQuery.Length, Is.EqualTo(1));
            Assert.That(intQuery[0].NullableInt32Property, Is.EqualTo(1));
        }

        [Test]
        public void LessThanOrEqual_WhenPropertyIsNullable()
        {
            var intQuery = _realm.All<AllTypesObject>().Where(o => o.NullableInt32Property <= 1).ToArray();
            Assert.That(intQuery.Length, Is.EqualTo(1));
            Assert.That(intQuery[0].NullableInt32Property, Is.EqualTo(1));
        }

        [Test]
        public void NotEqual_WhenPropertyIsNullable()
        {
            var intQuery = _realm.All<AllTypesObject>().Where(o => o.NullableInt32Property != 1).ToArray();
            Assert.That(intQuery.Length, Is.EqualTo(1));
            Assert.That(intQuery[0].NullableInt32Property, Is.EqualTo(null));
        }

        [Test]
        public void GreaterThan_WhenVariableIsNullable()
        {
            var intQuery = _realm.All<AllTypesObject>().Where(o => o.Int32Property > NullableZero).ToArray();
            Assert.That(intQuery.Length, Is.EqualTo(1));
            Assert.That(intQuery[0].Int32Property, Is.EqualTo(1));
        }

        [Test]
        public void GreaterThanOrEqual_WhenVariableIsNullable()
        {
            var intQuery = _realm.All<AllTypesObject>().Where(o => o.Int32Property >= NullableOne).ToArray();
            Assert.That(intQuery.Length, Is.EqualTo(1));
            Assert.That(intQuery[0].Int32Property, Is.EqualTo(1));
        }

        [Test]
        public void LessThan_WhenVariableIsNullable()
        {
            var intQuery = _realm.All<AllTypesObject>().Where(o => o.Int32Property < NullableOne).ToArray();
            Assert.That(intQuery.Length, Is.EqualTo(1));
            Assert.That(intQuery[0].Int32Property, Is.EqualTo(0));
        }

        [Test]
        public void LessThanOrEqual_WhenVariableIsNullable()
        {
            var intQuery = _realm.All<AllTypesObject>().Where(o => o.Int32Property <= NullableZero).ToArray();
            Assert.That(intQuery.Length, Is.EqualTo(1));
            Assert.That(intQuery[0].Int32Property, Is.EqualTo(0));
        }

        [Test]
        public void NotEqual_WhenVariableIsNullable()
        {
            var intQuery = _realm.All<AllTypesObject>().Where(o => o.Int32Property != NullableZero).ToArray();
            Assert.That(intQuery.Length, Is.EqualTo(1));
            Assert.That(intQuery[0].Int32Property, Is.EqualTo(1));
        }
    }
}
