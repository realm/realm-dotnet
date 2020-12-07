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
using MongoDB.Bson;

namespace Realms.Helpers
{
    /// <summary>
    /// A class that exposes a set of methods that we know are defined on a generic argument
    /// but there's no way to express them via the C# generic constraint system. It is generated
    /// by T4 transforming Operator.tt.
    /// </summary>
    [Preserve]
    internal static class Operator
    {
        private static IDictionary<(Type, Type), IConverter> _valueConverters = new Dictionary<(Type, Type), IConverter>
        {
            [(typeof(char), typeof(RealmValue))] = new CharRealmValueConverter(),
            [(typeof(byte), typeof(RealmValue))] = new ByteRealmValueConverter(),
            [(typeof(short), typeof(RealmValue))] = new ShortRealmValueConverter(),
            [(typeof(int), typeof(RealmValue))] = new IntRealmValueConverter(),
            [(typeof(long), typeof(RealmValue))] = new LongRealmValueConverter(),
            [(typeof(float), typeof(RealmValue))] = new FloatRealmValueConverter(),
            [(typeof(double), typeof(RealmValue))] = new DoubleRealmValueConverter(),
            [(typeof(bool), typeof(RealmValue))] = new BoolRealmValueConverter(),
            [(typeof(DateTimeOffset), typeof(RealmValue))] = new DateTimeOffsetRealmValueConverter(),
            [(typeof(decimal), typeof(RealmValue))] = new DecimalRealmValueConverter(),
            [(typeof(Decimal128), typeof(RealmValue))] = new Decimal128RealmValueConverter(),
            [(typeof(ObjectId), typeof(RealmValue))] = new ObjectIdRealmValueConverter(),
            [(typeof(Guid), typeof(RealmValue))] = new GuidRealmValueConverter(),
            [(typeof(char?), typeof(RealmValue))] = new NullableCharRealmValueConverter(),
            [(typeof(byte?), typeof(RealmValue))] = new NullableByteRealmValueConverter(),
            [(typeof(short?), typeof(RealmValue))] = new NullableShortRealmValueConverter(),
            [(typeof(int?), typeof(RealmValue))] = new NullableIntRealmValueConverter(),
            [(typeof(long?), typeof(RealmValue))] = new NullableLongRealmValueConverter(),
            [(typeof(float?), typeof(RealmValue))] = new NullableFloatRealmValueConverter(),
            [(typeof(double?), typeof(RealmValue))] = new NullableDoubleRealmValueConverter(),
            [(typeof(bool?), typeof(RealmValue))] = new NullableBoolRealmValueConverter(),
            [(typeof(DateTimeOffset?), typeof(RealmValue))] = new NullableDateTimeOffsetRealmValueConverter(),
            [(typeof(decimal?), typeof(RealmValue))] = new NullableDecimalRealmValueConverter(),
            [(typeof(Decimal128?), typeof(RealmValue))] = new NullableDecimal128RealmValueConverter(),
            [(typeof(ObjectId?), typeof(RealmValue))] = new NullableObjectIdRealmValueConverter(),
            [(typeof(Guid?), typeof(RealmValue))] = new NullableGuidRealmValueConverter(),
            [(typeof(RealmInteger<byte>), typeof(RealmValue))] = new RealmIntegerByteRealmValueConverter(),
            [(typeof(RealmInteger<short>), typeof(RealmValue))] = new RealmIntegerShortRealmValueConverter(),
            [(typeof(RealmInteger<int>), typeof(RealmValue))] = new RealmIntegerIntRealmValueConverter(),
            [(typeof(RealmInteger<long>), typeof(RealmValue))] = new RealmIntegerLongRealmValueConverter(),
            [(typeof(RealmInteger<byte>?), typeof(RealmValue))] = new NullableRealmIntegerByteRealmValueConverter(),
            [(typeof(RealmInteger<short>?), typeof(RealmValue))] = new NullableRealmIntegerShortRealmValueConverter(),
            [(typeof(RealmInteger<int>?), typeof(RealmValue))] = new NullableRealmIntegerIntRealmValueConverter(),
            [(typeof(RealmInteger<long>?), typeof(RealmValue))] = new NullableRealmIntegerLongRealmValueConverter(),
            [(typeof(byte[]), typeof(RealmValue))] = new ByteArrayRealmValueConverter(),
            [(typeof(string), typeof(RealmValue))] = new StringRealmValueConverter(),
            [(typeof(RealmObjectBase), typeof(RealmValue))] = new RealmObjectBaseRealmValueConverter(),
            [(typeof(RealmValue), typeof(char))] = new RealmValueCharConverter(),
            [(typeof(RealmValue), typeof(byte))] = new RealmValueByteConverter(),
            [(typeof(RealmValue), typeof(short))] = new RealmValueShortConverter(),
            [(typeof(RealmValue), typeof(int))] = new RealmValueIntConverter(),
            [(typeof(RealmValue), typeof(long))] = new RealmValueLongConverter(),
            [(typeof(RealmValue), typeof(float))] = new RealmValueFloatConverter(),
            [(typeof(RealmValue), typeof(double))] = new RealmValueDoubleConverter(),
            [(typeof(RealmValue), typeof(bool))] = new RealmValueBoolConverter(),
            [(typeof(RealmValue), typeof(DateTimeOffset))] = new RealmValueDateTimeOffsetConverter(),
            [(typeof(RealmValue), typeof(decimal))] = new RealmValueDecimalConverter(),
            [(typeof(RealmValue), typeof(Decimal128))] = new RealmValueDecimal128Converter(),
            [(typeof(RealmValue), typeof(ObjectId))] = new RealmValueObjectIdConverter(),
            [(typeof(RealmValue), typeof(Guid))] = new RealmValueGuidConverter(),
            [(typeof(RealmValue), typeof(char?))] = new RealmValueNullableCharConverter(),
            [(typeof(RealmValue), typeof(byte?))] = new RealmValueNullableByteConverter(),
            [(typeof(RealmValue), typeof(short?))] = new RealmValueNullableShortConverter(),
            [(typeof(RealmValue), typeof(int?))] = new RealmValueNullableIntConverter(),
            [(typeof(RealmValue), typeof(long?))] = new RealmValueNullableLongConverter(),
            [(typeof(RealmValue), typeof(float?))] = new RealmValueNullableFloatConverter(),
            [(typeof(RealmValue), typeof(double?))] = new RealmValueNullableDoubleConverter(),
            [(typeof(RealmValue), typeof(bool?))] = new RealmValueNullableBoolConverter(),
            [(typeof(RealmValue), typeof(DateTimeOffset?))] = new RealmValueNullableDateTimeOffsetConverter(),
            [(typeof(RealmValue), typeof(decimal?))] = new RealmValueNullableDecimalConverter(),
            [(typeof(RealmValue), typeof(Decimal128?))] = new RealmValueNullableDecimal128Converter(),
            [(typeof(RealmValue), typeof(ObjectId?))] = new RealmValueNullableObjectIdConverter(),
            [(typeof(RealmValue), typeof(Guid?))] = new RealmValueNullableGuidConverter(),
            [(typeof(RealmValue), typeof(RealmInteger<byte>))] = new RealmValueRealmIntegerByteConverter(),
            [(typeof(RealmValue), typeof(RealmInteger<short>))] = new RealmValueRealmIntegerShortConverter(),
            [(typeof(RealmValue), typeof(RealmInteger<int>))] = new RealmValueRealmIntegerIntConverter(),
            [(typeof(RealmValue), typeof(RealmInteger<long>))] = new RealmValueRealmIntegerLongConverter(),
            [(typeof(RealmValue), typeof(RealmInteger<byte>?))] = new RealmValueNullableRealmIntegerByteConverter(),
            [(typeof(RealmValue), typeof(RealmInteger<short>?))] = new RealmValueNullableRealmIntegerShortConverter(),
            [(typeof(RealmValue), typeof(RealmInteger<int>?))] = new RealmValueNullableRealmIntegerIntConverter(),
            [(typeof(RealmValue), typeof(RealmInteger<long>?))] = new RealmValueNullableRealmIntegerLongConverter(),
            [(typeof(RealmValue), typeof(byte[]))] = new RealmValueByteArrayConverter(),
            [(typeof(RealmValue), typeof(string))] = new RealmValueStringConverter(),
            [(typeof(RealmValue), typeof(RealmObjectBase))] = new RealmValueRealmObjectBaseConverter(),
            [(typeof(char), typeof(char?))] = new CharNullableCharConverter(),
            [(typeof(char), typeof(byte?))] = new CharNullableByteConverter(),
            [(typeof(char), typeof(short?))] = new CharNullableShortConverter(),
            [(typeof(char), typeof(int?))] = new CharNullableIntConverter(),
            [(typeof(char), typeof(long?))] = new CharNullableLongConverter(),
            [(typeof(char), typeof(RealmInteger<byte>?))] = new CharNullableRealmIntegerByteConverter(),
            [(typeof(char), typeof(RealmInteger<short>?))] = new CharNullableRealmIntegerShortConverter(),
            [(typeof(char), typeof(RealmInteger<int>?))] = new CharNullableRealmIntegerIntConverter(),
            [(typeof(char), typeof(RealmInteger<long>?))] = new CharNullableRealmIntegerLongConverter(),
            [(typeof(char), typeof(float?))] = new CharNullableFloatConverter(),
            [(typeof(char), typeof(double?))] = new CharNullableDoubleConverter(),
            [(typeof(char), typeof(decimal?))] = new CharNullableDecimalConverter(),
            [(typeof(char), typeof(Decimal128?))] = new CharNullableDecimal128Converter(),
            [(typeof(byte), typeof(char?))] = new ByteNullableCharConverter(),
            [(typeof(byte), typeof(byte?))] = new ByteNullableByteConverter(),
            [(typeof(byte), typeof(short?))] = new ByteNullableShortConverter(),
            [(typeof(byte), typeof(int?))] = new ByteNullableIntConverter(),
            [(typeof(byte), typeof(long?))] = new ByteNullableLongConverter(),
            [(typeof(byte), typeof(RealmInteger<byte>?))] = new ByteNullableRealmIntegerByteConverter(),
            [(typeof(byte), typeof(RealmInteger<short>?))] = new ByteNullableRealmIntegerShortConverter(),
            [(typeof(byte), typeof(RealmInteger<int>?))] = new ByteNullableRealmIntegerIntConverter(),
            [(typeof(byte), typeof(RealmInteger<long>?))] = new ByteNullableRealmIntegerLongConverter(),
            [(typeof(byte), typeof(float?))] = new ByteNullableFloatConverter(),
            [(typeof(byte), typeof(double?))] = new ByteNullableDoubleConverter(),
            [(typeof(byte), typeof(decimal?))] = new ByteNullableDecimalConverter(),
            [(typeof(byte), typeof(Decimal128?))] = new ByteNullableDecimal128Converter(),
            [(typeof(short), typeof(char?))] = new ShortNullableCharConverter(),
            [(typeof(short), typeof(byte?))] = new ShortNullableByteConverter(),
            [(typeof(short), typeof(short?))] = new ShortNullableShortConverter(),
            [(typeof(short), typeof(int?))] = new ShortNullableIntConverter(),
            [(typeof(short), typeof(long?))] = new ShortNullableLongConverter(),
            [(typeof(short), typeof(RealmInteger<byte>?))] = new ShortNullableRealmIntegerByteConverter(),
            [(typeof(short), typeof(RealmInteger<short>?))] = new ShortNullableRealmIntegerShortConverter(),
            [(typeof(short), typeof(RealmInteger<int>?))] = new ShortNullableRealmIntegerIntConverter(),
            [(typeof(short), typeof(RealmInteger<long>?))] = new ShortNullableRealmIntegerLongConverter(),
            [(typeof(short), typeof(float?))] = new ShortNullableFloatConverter(),
            [(typeof(short), typeof(double?))] = new ShortNullableDoubleConverter(),
            [(typeof(short), typeof(decimal?))] = new ShortNullableDecimalConverter(),
            [(typeof(short), typeof(Decimal128?))] = new ShortNullableDecimal128Converter(),
            [(typeof(int), typeof(char?))] = new IntNullableCharConverter(),
            [(typeof(int), typeof(byte?))] = new IntNullableByteConverter(),
            [(typeof(int), typeof(short?))] = new IntNullableShortConverter(),
            [(typeof(int), typeof(int?))] = new IntNullableIntConverter(),
            [(typeof(int), typeof(long?))] = new IntNullableLongConverter(),
            [(typeof(int), typeof(RealmInteger<byte>?))] = new IntNullableRealmIntegerByteConverter(),
            [(typeof(int), typeof(RealmInteger<short>?))] = new IntNullableRealmIntegerShortConverter(),
            [(typeof(int), typeof(RealmInteger<int>?))] = new IntNullableRealmIntegerIntConverter(),
            [(typeof(int), typeof(RealmInteger<long>?))] = new IntNullableRealmIntegerLongConverter(),
            [(typeof(int), typeof(float?))] = new IntNullableFloatConverter(),
            [(typeof(int), typeof(double?))] = new IntNullableDoubleConverter(),
            [(typeof(int), typeof(decimal?))] = new IntNullableDecimalConverter(),
            [(typeof(int), typeof(Decimal128?))] = new IntNullableDecimal128Converter(),
            [(typeof(long), typeof(char?))] = new LongNullableCharConverter(),
            [(typeof(long), typeof(byte?))] = new LongNullableByteConverter(),
            [(typeof(long), typeof(short?))] = new LongNullableShortConverter(),
            [(typeof(long), typeof(int?))] = new LongNullableIntConverter(),
            [(typeof(long), typeof(long?))] = new LongNullableLongConverter(),
            [(typeof(long), typeof(RealmInteger<byte>?))] = new LongNullableRealmIntegerByteConverter(),
            [(typeof(long), typeof(RealmInteger<short>?))] = new LongNullableRealmIntegerShortConverter(),
            [(typeof(long), typeof(RealmInteger<int>?))] = new LongNullableRealmIntegerIntConverter(),
            [(typeof(long), typeof(RealmInteger<long>?))] = new LongNullableRealmIntegerLongConverter(),
            [(typeof(long), typeof(float?))] = new LongNullableFloatConverter(),
            [(typeof(long), typeof(double?))] = new LongNullableDoubleConverter(),
            [(typeof(long), typeof(decimal?))] = new LongNullableDecimalConverter(),
            [(typeof(long), typeof(Decimal128?))] = new LongNullableDecimal128Converter(),
            [(typeof(RealmInteger<byte>), typeof(char?))] = new RealmIntegerByteNullableCharConverter(),
            [(typeof(RealmInteger<byte>), typeof(byte?))] = new RealmIntegerByteNullableByteConverter(),
            [(typeof(RealmInteger<byte>), typeof(short?))] = new RealmIntegerByteNullableShortConverter(),
            [(typeof(RealmInteger<byte>), typeof(int?))] = new RealmIntegerByteNullableIntConverter(),
            [(typeof(RealmInteger<byte>), typeof(long?))] = new RealmIntegerByteNullableLongConverter(),
            [(typeof(RealmInteger<byte>), typeof(RealmInteger<byte>?))] = new RealmIntegerByteNullableRealmIntegerByteConverter(),
            [(typeof(RealmInteger<byte>), typeof(RealmInteger<short>?))] = new RealmIntegerByteNullableRealmIntegerShortConverter(),
            [(typeof(RealmInteger<byte>), typeof(RealmInteger<int>?))] = new RealmIntegerByteNullableRealmIntegerIntConverter(),
            [(typeof(RealmInteger<byte>), typeof(RealmInteger<long>?))] = new RealmIntegerByteNullableRealmIntegerLongConverter(),
            [(typeof(RealmInteger<byte>), typeof(float?))] = new RealmIntegerByteNullableFloatConverter(),
            [(typeof(RealmInteger<byte>), typeof(double?))] = new RealmIntegerByteNullableDoubleConverter(),
            [(typeof(RealmInteger<byte>), typeof(decimal?))] = new RealmIntegerByteNullableDecimalConverter(),
            [(typeof(RealmInteger<byte>), typeof(Decimal128?))] = new RealmIntegerByteNullableDecimal128Converter(),
            [(typeof(RealmInteger<short>), typeof(char?))] = new RealmIntegerShortNullableCharConverter(),
            [(typeof(RealmInteger<short>), typeof(byte?))] = new RealmIntegerShortNullableByteConverter(),
            [(typeof(RealmInteger<short>), typeof(short?))] = new RealmIntegerShortNullableShortConverter(),
            [(typeof(RealmInteger<short>), typeof(int?))] = new RealmIntegerShortNullableIntConverter(),
            [(typeof(RealmInteger<short>), typeof(long?))] = new RealmIntegerShortNullableLongConverter(),
            [(typeof(RealmInteger<short>), typeof(RealmInteger<byte>?))] = new RealmIntegerShortNullableRealmIntegerByteConverter(),
            [(typeof(RealmInteger<short>), typeof(RealmInteger<short>?))] = new RealmIntegerShortNullableRealmIntegerShortConverter(),
            [(typeof(RealmInteger<short>), typeof(RealmInteger<int>?))] = new RealmIntegerShortNullableRealmIntegerIntConverter(),
            [(typeof(RealmInteger<short>), typeof(RealmInteger<long>?))] = new RealmIntegerShortNullableRealmIntegerLongConverter(),
            [(typeof(RealmInteger<short>), typeof(float?))] = new RealmIntegerShortNullableFloatConverter(),
            [(typeof(RealmInteger<short>), typeof(double?))] = new RealmIntegerShortNullableDoubleConverter(),
            [(typeof(RealmInteger<short>), typeof(decimal?))] = new RealmIntegerShortNullableDecimalConverter(),
            [(typeof(RealmInteger<short>), typeof(Decimal128?))] = new RealmIntegerShortNullableDecimal128Converter(),
            [(typeof(RealmInteger<int>), typeof(char?))] = new RealmIntegerIntNullableCharConverter(),
            [(typeof(RealmInteger<int>), typeof(byte?))] = new RealmIntegerIntNullableByteConverter(),
            [(typeof(RealmInteger<int>), typeof(short?))] = new RealmIntegerIntNullableShortConverter(),
            [(typeof(RealmInteger<int>), typeof(int?))] = new RealmIntegerIntNullableIntConverter(),
            [(typeof(RealmInteger<int>), typeof(long?))] = new RealmIntegerIntNullableLongConverter(),
            [(typeof(RealmInteger<int>), typeof(RealmInteger<byte>?))] = new RealmIntegerIntNullableRealmIntegerByteConverter(),
            [(typeof(RealmInteger<int>), typeof(RealmInteger<short>?))] = new RealmIntegerIntNullableRealmIntegerShortConverter(),
            [(typeof(RealmInteger<int>), typeof(RealmInteger<int>?))] = new RealmIntegerIntNullableRealmIntegerIntConverter(),
            [(typeof(RealmInteger<int>), typeof(RealmInteger<long>?))] = new RealmIntegerIntNullableRealmIntegerLongConverter(),
            [(typeof(RealmInteger<int>), typeof(float?))] = new RealmIntegerIntNullableFloatConverter(),
            [(typeof(RealmInteger<int>), typeof(double?))] = new RealmIntegerIntNullableDoubleConverter(),
            [(typeof(RealmInteger<int>), typeof(decimal?))] = new RealmIntegerIntNullableDecimalConverter(),
            [(typeof(RealmInteger<int>), typeof(Decimal128?))] = new RealmIntegerIntNullableDecimal128Converter(),
            [(typeof(RealmInteger<long>), typeof(char?))] = new RealmIntegerLongNullableCharConverter(),
            [(typeof(RealmInteger<long>), typeof(byte?))] = new RealmIntegerLongNullableByteConverter(),
            [(typeof(RealmInteger<long>), typeof(short?))] = new RealmIntegerLongNullableShortConverter(),
            [(typeof(RealmInteger<long>), typeof(int?))] = new RealmIntegerLongNullableIntConverter(),
            [(typeof(RealmInteger<long>), typeof(long?))] = new RealmIntegerLongNullableLongConverter(),
            [(typeof(RealmInteger<long>), typeof(RealmInteger<byte>?))] = new RealmIntegerLongNullableRealmIntegerByteConverter(),
            [(typeof(RealmInteger<long>), typeof(RealmInteger<short>?))] = new RealmIntegerLongNullableRealmIntegerShortConverter(),
            [(typeof(RealmInteger<long>), typeof(RealmInteger<int>?))] = new RealmIntegerLongNullableRealmIntegerIntConverter(),
            [(typeof(RealmInteger<long>), typeof(RealmInteger<long>?))] = new RealmIntegerLongNullableRealmIntegerLongConverter(),
            [(typeof(RealmInteger<long>), typeof(float?))] = new RealmIntegerLongNullableFloatConverter(),
            [(typeof(RealmInteger<long>), typeof(double?))] = new RealmIntegerLongNullableDoubleConverter(),
            [(typeof(RealmInteger<long>), typeof(decimal?))] = new RealmIntegerLongNullableDecimalConverter(),
            [(typeof(RealmInteger<long>), typeof(Decimal128?))] = new RealmIntegerLongNullableDecimal128Converter(),
            [(typeof(char), typeof(byte))] = new CharByteConverter(),
            [(typeof(char), typeof(short))] = new CharShortConverter(),
            [(typeof(char), typeof(int))] = new CharIntConverter(),
            [(typeof(char), typeof(long))] = new CharLongConverter(),
            [(typeof(char), typeof(RealmInteger<byte>))] = new CharRealmIntegerByteConverter(),
            [(typeof(char), typeof(RealmInteger<short>))] = new CharRealmIntegerShortConverter(),
            [(typeof(char), typeof(RealmInteger<int>))] = new CharRealmIntegerIntConverter(),
            [(typeof(char), typeof(RealmInteger<long>))] = new CharRealmIntegerLongConverter(),
            [(typeof(char), typeof(float))] = new CharFloatConverter(),
            [(typeof(char), typeof(double))] = new CharDoubleConverter(),
            [(typeof(char), typeof(decimal))] = new CharDecimalConverter(),
            [(typeof(char), typeof(Decimal128))] = new CharDecimal128Converter(),
            [(typeof(byte), typeof(char))] = new ByteCharConverter(),
            [(typeof(byte), typeof(short))] = new ByteShortConverter(),
            [(typeof(byte), typeof(int))] = new ByteIntConverter(),
            [(typeof(byte), typeof(long))] = new ByteLongConverter(),
            [(typeof(byte), typeof(RealmInteger<byte>))] = new ByteRealmIntegerByteConverter(),
            [(typeof(byte), typeof(RealmInteger<short>))] = new ByteRealmIntegerShortConverter(),
            [(typeof(byte), typeof(RealmInteger<int>))] = new ByteRealmIntegerIntConverter(),
            [(typeof(byte), typeof(RealmInteger<long>))] = new ByteRealmIntegerLongConverter(),
            [(typeof(byte), typeof(float))] = new ByteFloatConverter(),
            [(typeof(byte), typeof(double))] = new ByteDoubleConverter(),
            [(typeof(byte), typeof(decimal))] = new ByteDecimalConverter(),
            [(typeof(byte), typeof(Decimal128))] = new ByteDecimal128Converter(),
            [(typeof(short), typeof(char))] = new ShortCharConverter(),
            [(typeof(short), typeof(byte))] = new ShortByteConverter(),
            [(typeof(short), typeof(int))] = new ShortIntConverter(),
            [(typeof(short), typeof(long))] = new ShortLongConverter(),
            [(typeof(short), typeof(RealmInteger<byte>))] = new ShortRealmIntegerByteConverter(),
            [(typeof(short), typeof(RealmInteger<short>))] = new ShortRealmIntegerShortConverter(),
            [(typeof(short), typeof(RealmInteger<int>))] = new ShortRealmIntegerIntConverter(),
            [(typeof(short), typeof(RealmInteger<long>))] = new ShortRealmIntegerLongConverter(),
            [(typeof(short), typeof(float))] = new ShortFloatConverter(),
            [(typeof(short), typeof(double))] = new ShortDoubleConverter(),
            [(typeof(short), typeof(decimal))] = new ShortDecimalConverter(),
            [(typeof(short), typeof(Decimal128))] = new ShortDecimal128Converter(),
            [(typeof(int), typeof(char))] = new IntCharConverter(),
            [(typeof(int), typeof(byte))] = new IntByteConverter(),
            [(typeof(int), typeof(short))] = new IntShortConverter(),
            [(typeof(int), typeof(long))] = new IntLongConverter(),
            [(typeof(int), typeof(RealmInteger<byte>))] = new IntRealmIntegerByteConverter(),
            [(typeof(int), typeof(RealmInteger<short>))] = new IntRealmIntegerShortConverter(),
            [(typeof(int), typeof(RealmInteger<int>))] = new IntRealmIntegerIntConverter(),
            [(typeof(int), typeof(RealmInteger<long>))] = new IntRealmIntegerLongConverter(),
            [(typeof(int), typeof(float))] = new IntFloatConverter(),
            [(typeof(int), typeof(double))] = new IntDoubleConverter(),
            [(typeof(int), typeof(decimal))] = new IntDecimalConverter(),
            [(typeof(int), typeof(Decimal128))] = new IntDecimal128Converter(),
            [(typeof(long), typeof(char))] = new LongCharConverter(),
            [(typeof(long), typeof(byte))] = new LongByteConverter(),
            [(typeof(long), typeof(short))] = new LongShortConverter(),
            [(typeof(long), typeof(int))] = new LongIntConverter(),
            [(typeof(long), typeof(RealmInteger<byte>))] = new LongRealmIntegerByteConverter(),
            [(typeof(long), typeof(RealmInteger<short>))] = new LongRealmIntegerShortConverter(),
            [(typeof(long), typeof(RealmInteger<int>))] = new LongRealmIntegerIntConverter(),
            [(typeof(long), typeof(RealmInteger<long>))] = new LongRealmIntegerLongConverter(),
            [(typeof(long), typeof(float))] = new LongFloatConverter(),
            [(typeof(long), typeof(double))] = new LongDoubleConverter(),
            [(typeof(long), typeof(decimal))] = new LongDecimalConverter(),
            [(typeof(long), typeof(Decimal128))] = new LongDecimal128Converter(),
            [(typeof(RealmInteger<byte>), typeof(char))] = new RealmIntegerByteCharConverter(),
            [(typeof(RealmInteger<byte>), typeof(byte))] = new RealmIntegerByteByteConverter(),
            [(typeof(RealmInteger<byte>), typeof(short))] = new RealmIntegerByteShortConverter(),
            [(typeof(RealmInteger<byte>), typeof(int))] = new RealmIntegerByteIntConverter(),
            [(typeof(RealmInteger<byte>), typeof(long))] = new RealmIntegerByteLongConverter(),
            [(typeof(RealmInteger<byte>), typeof(RealmInteger<short>))] = new RealmIntegerByteRealmIntegerShortConverter(),
            [(typeof(RealmInteger<byte>), typeof(RealmInteger<int>))] = new RealmIntegerByteRealmIntegerIntConverter(),
            [(typeof(RealmInteger<byte>), typeof(RealmInteger<long>))] = new RealmIntegerByteRealmIntegerLongConverter(),
            [(typeof(RealmInteger<byte>), typeof(float))] = new RealmIntegerByteFloatConverter(),
            [(typeof(RealmInteger<byte>), typeof(double))] = new RealmIntegerByteDoubleConverter(),
            [(typeof(RealmInteger<byte>), typeof(decimal))] = new RealmIntegerByteDecimalConverter(),
            [(typeof(RealmInteger<byte>), typeof(Decimal128))] = new RealmIntegerByteDecimal128Converter(),
            [(typeof(RealmInteger<short>), typeof(char))] = new RealmIntegerShortCharConverter(),
            [(typeof(RealmInteger<short>), typeof(byte))] = new RealmIntegerShortByteConverter(),
            [(typeof(RealmInteger<short>), typeof(short))] = new RealmIntegerShortShortConverter(),
            [(typeof(RealmInteger<short>), typeof(int))] = new RealmIntegerShortIntConverter(),
            [(typeof(RealmInteger<short>), typeof(long))] = new RealmIntegerShortLongConverter(),
            [(typeof(RealmInteger<short>), typeof(RealmInteger<byte>))] = new RealmIntegerShortRealmIntegerByteConverter(),
            [(typeof(RealmInteger<short>), typeof(RealmInteger<int>))] = new RealmIntegerShortRealmIntegerIntConverter(),
            [(typeof(RealmInteger<short>), typeof(RealmInteger<long>))] = new RealmIntegerShortRealmIntegerLongConverter(),
            [(typeof(RealmInteger<short>), typeof(float))] = new RealmIntegerShortFloatConverter(),
            [(typeof(RealmInteger<short>), typeof(double))] = new RealmIntegerShortDoubleConverter(),
            [(typeof(RealmInteger<short>), typeof(decimal))] = new RealmIntegerShortDecimalConverter(),
            [(typeof(RealmInteger<short>), typeof(Decimal128))] = new RealmIntegerShortDecimal128Converter(),
            [(typeof(RealmInteger<int>), typeof(char))] = new RealmIntegerIntCharConverter(),
            [(typeof(RealmInteger<int>), typeof(byte))] = new RealmIntegerIntByteConverter(),
            [(typeof(RealmInteger<int>), typeof(short))] = new RealmIntegerIntShortConverter(),
            [(typeof(RealmInteger<int>), typeof(int))] = new RealmIntegerIntIntConverter(),
            [(typeof(RealmInteger<int>), typeof(long))] = new RealmIntegerIntLongConverter(),
            [(typeof(RealmInteger<int>), typeof(RealmInteger<byte>))] = new RealmIntegerIntRealmIntegerByteConverter(),
            [(typeof(RealmInteger<int>), typeof(RealmInteger<short>))] = new RealmIntegerIntRealmIntegerShortConverter(),
            [(typeof(RealmInteger<int>), typeof(RealmInteger<long>))] = new RealmIntegerIntRealmIntegerLongConverter(),
            [(typeof(RealmInteger<int>), typeof(float))] = new RealmIntegerIntFloatConverter(),
            [(typeof(RealmInteger<int>), typeof(double))] = new RealmIntegerIntDoubleConverter(),
            [(typeof(RealmInteger<int>), typeof(decimal))] = new RealmIntegerIntDecimalConverter(),
            [(typeof(RealmInteger<int>), typeof(Decimal128))] = new RealmIntegerIntDecimal128Converter(),
            [(typeof(RealmInteger<long>), typeof(char))] = new RealmIntegerLongCharConverter(),
            [(typeof(RealmInteger<long>), typeof(byte))] = new RealmIntegerLongByteConverter(),
            [(typeof(RealmInteger<long>), typeof(short))] = new RealmIntegerLongShortConverter(),
            [(typeof(RealmInteger<long>), typeof(int))] = new RealmIntegerLongIntConverter(),
            [(typeof(RealmInteger<long>), typeof(long))] = new RealmIntegerLongLongConverter(),
            [(typeof(RealmInteger<long>), typeof(RealmInteger<byte>))] = new RealmIntegerLongRealmIntegerByteConverter(),
            [(typeof(RealmInteger<long>), typeof(RealmInteger<short>))] = new RealmIntegerLongRealmIntegerShortConverter(),
            [(typeof(RealmInteger<long>), typeof(RealmInteger<int>))] = new RealmIntegerLongRealmIntegerIntConverter(),
            [(typeof(RealmInteger<long>), typeof(float))] = new RealmIntegerLongFloatConverter(),
            [(typeof(RealmInteger<long>), typeof(double))] = new RealmIntegerLongDoubleConverter(),
            [(typeof(RealmInteger<long>), typeof(decimal))] = new RealmIntegerLongDecimalConverter(),
            [(typeof(RealmInteger<long>), typeof(Decimal128))] = new RealmIntegerLongDecimal128Converter(),
            [(typeof(float), typeof(float?))] = new FloatNullableFloatConverter(),
            [(typeof(float), typeof(double?))] = new FloatNullableDoubleConverter(),
            [(typeof(float), typeof(decimal?))] = new FloatNullableDecimalConverter(),
            [(typeof(float), typeof(Decimal128?))] = new FloatNullableDecimal128Converter(),
            [(typeof(double), typeof(float?))] = new DoubleNullableFloatConverter(),
            [(typeof(double), typeof(double?))] = new DoubleNullableDoubleConverter(),
            [(typeof(double), typeof(decimal?))] = new DoubleNullableDecimalConverter(),
            [(typeof(double), typeof(Decimal128?))] = new DoubleNullableDecimal128Converter(),
            [(typeof(decimal), typeof(float?))] = new DecimalNullableFloatConverter(),
            [(typeof(decimal), typeof(double?))] = new DecimalNullableDoubleConverter(),
            [(typeof(decimal), typeof(decimal?))] = new DecimalNullableDecimalConverter(),
            [(typeof(decimal), typeof(Decimal128?))] = new DecimalNullableDecimal128Converter(),
            [(typeof(Decimal128), typeof(float?))] = new Decimal128NullableFloatConverter(),
            [(typeof(Decimal128), typeof(double?))] = new Decimal128NullableDoubleConverter(),
            [(typeof(Decimal128), typeof(decimal?))] = new Decimal128NullableDecimalConverter(),
            [(typeof(Decimal128), typeof(Decimal128?))] = new Decimal128NullableDecimal128Converter(),
            [(typeof(float), typeof(double))] = new FloatDoubleConverter(),
            [(typeof(float), typeof(decimal))] = new FloatDecimalConverter(),
            [(typeof(float), typeof(Decimal128))] = new FloatDecimal128Converter(),
            [(typeof(double), typeof(float))] = new DoubleFloatConverter(),
            [(typeof(double), typeof(decimal))] = new DoubleDecimalConverter(),
            [(typeof(double), typeof(Decimal128))] = new DoubleDecimal128Converter(),
            [(typeof(decimal), typeof(float))] = new DecimalFloatConverter(),
            [(typeof(decimal), typeof(double))] = new DecimalDoubleConverter(),
            [(typeof(decimal), typeof(Decimal128))] = new DecimalDecimal128Converter(),
            [(typeof(Decimal128), typeof(float))] = new Decimal128FloatConverter(),
            [(typeof(Decimal128), typeof(double))] = new Decimal128DoubleConverter(),
            [(typeof(Decimal128), typeof(decimal))] = new Decimal128DecimalConverter(),
        };

        /// <summary>
        /// Efficiently convert a <typeparamref name="TFrom"/> value to <typeparamref name="TResult"/>.
        /// It is intended to be used when we want to convert to or from a generic where we don't
        /// know the exact type, but we know that a conversion exists.
        /// </summary>
        /// <remarks>
        /// In synthetic benchmarks it performs about
        /// two orders of magnitude faster than Convert.ChangeType. It is about 4 times slower than a direct cast
        /// when the types are known, but about an order of magnitude faster than a cast that involves boxing to
        /// object.
        /// <br/>
        /// It makes use of implicit and explicit conversion operators defined on types to convert between
        /// numeric types, which means that we can use it both for downcasting and upcasting numeric types.
        /// </remarks>
        /// <typeparam name="TFrom">The type from which to convert.</typeparam>
        /// <typeparam name="TResult">The type to which <paramref name="value"/> will be converted.</typeparam>
        /// <param name="value">The value to convert to <typeparamref name="TResult"/>.</param>
        /// <returns>The value of <paramref name="value"/> represented as <typeparamref name="TResult"/>.</returns>
        public static TResult Convert<TFrom, TResult>(TFrom value)
        {
            if (typeof(TResult) == typeof(RealmValue))
            {
                /* This is special cased due to a bug in the Xamarin.iOS interpreter. When value
                 * is null, we end up with a NRE with the following stacktrace:
                 *
                 * <System.NullReferenceException: Object reference not set to an instance of an object
                 * at System.Linq.Expressions.Interpreter.LightLambda.Run1[T0,TRet] (T0 arg0) [0x00038] in <ee28ffe65f2e47a98ea97b07327fb8f4>:0
                 * at (wrapper delegate-invoke) System.Func`2[System.String,Realms.RealmValue].invoke_TResult_T(string)
                 * at Realms.Helpers.Operator.Convert[TFrom,TResult] (TFrom value) [0x00005] in <675c1cc840764fcb9ab78b319ccfeee3>:0
                 * at Realms.RealmList`1[T].<.ctor>b__5_1 (T item) [0x00000] in <675c1cc840764fcb9ab78b319ccfeee3>:0
                 * at Realms.RealmList`1[T].Add (T item) [0x00000] in <675c1cc840764fcb9ab78b319ccfeee3>:0
                 *
                 * May or may not be related to https://github.com/mono/mono/issues/15852.
                 */
                if (value is null)
                {
                    return Convert<RealmValue, TResult>(RealmValue.Null());
                }

                /* This is another special case where `value` is inheritable from RealmObjectBase. There's
                 * no direct conversion from T to RealmValue, but there's conversion if we go through RealmObjectBase.
                 */
                if (value is RealmObjectBase robj)
                {
                    return Convert<RealmValue, TResult>(robj);
                }
            }

            return GenericOperator<TFrom, TResult>.Convert(value);
        }

        /// <summary>
        /// An operator that exposes a method to convert from <typeparamref name="TSource"/>
        /// to <typeparamref name="TTarget"/>. Upon constructing the closed generic type, the static
        /// constructor will instantiate a <see cref="ISpecializedConverter{TSource, TTarget}"/> and
        /// assign it to a static field for the duration of the application domain.
        /// </summary>
        /// <typeparam name="TSource">The type from which to convert.</typeparam>
        /// <typeparam name="TTarget">The type to which <typeparamref name="TSource"/> will be converted.</typeparam>
        private static class GenericOperator<TSource, TTarget>
        {
            private static readonly ISpecializedConverter<TSource, TTarget> _converter;

            public static TTarget Convert(TSource value) => _converter.Convert(value);

            static GenericOperator()
            {
                var sourceType = typeof(TSource);
                var targetType = typeof(TTarget);

                if (sourceType == targetType)
                {
                    _converter = (ISpecializedConverter<TSource, TTarget>)new UnaryConverter<TSource>();
                }
                else if (_valueConverters.TryGetValue((sourceType, targetType), out var converter))
                {
                    _converter = (ISpecializedConverter<TSource, TTarget>)converter;
                }
                else if (targetType.IsAssignableFrom(sourceType) || sourceType == typeof(object))
                {
                    _converter = new InheritanceConverter<TSource, TTarget>();
                }
                else
                {
                    _converter = new ThrowingConverter<TSource, TTarget>();
                }
            }
        }

        /// <summary>
        /// An interface representing a converter - used primarily to guarantee type safety of the
        /// generated <see cref="_valueConverters"/> dictionary.
        /// </summary>
        private interface IConverter
        {
        }

        /// <summary>
        /// Interface representing a concrete converter from <typeparamref name="TSource"/>
        /// to <typeparamref name="TTarget"/>. For most types there will be exactly one concrete
        /// implementation, but there may be cases, such as <see cref="InheritanceConverter{TSource, TTarget}"/>
        /// where a single converter type can handle multiple source/target types.
        /// </summary>
        /// <typeparam name="TSource">The type from which to convert.</typeparam>
        /// <typeparam name="TTarget">The type to which <typeparamref name="TSource"/> will be converted.</typeparam>
        private interface ISpecializedConverter<TSource, TTarget> : IConverter
        {
            TTarget Convert(TSource source);
        }

        /// <summary>
        /// A converter that will throw whenever <see cref="ISpecializedConverter{TSource, TTarget}.Convert(TSource)"/>
        /// is called. This is used to handle cases where there is no conversion from <typeparamref name="TSource"/> to
        /// <typeparamref name="TTarget"/>.
        /// </summary>
        /// <typeparam name="TSource">The type from which to convert.</typeparam>
        /// <typeparam name="TTarget">The type to which <typeparamref name="TSource"/> will be converted.</typeparam>
        private class ThrowingConverter<TSource, TTarget> : ISpecializedConverter<TSource, TTarget>
        {
            public TTarget Convert(TSource source) => throw new NotSupportedException($"No conversion exists from {typeof(TSource).FullName} to {typeof(TTarget).FullName}");
        }

        /// <summary>
        /// A converter that converts from the type to itself. There are cases where we don't know what the source or
        /// the target type is, so we need to convert, just in case.
        /// </summary>
        /// <typeparam name="T">The type of both the source and the target.</typeparam>
        private class UnaryConverter<T> : ISpecializedConverter<T, T>
        {
            public T Convert(T source) => source;
        }

        /// <summary>
        /// A converter that converts from a type to its base type. This is typically needed
        /// when we want to cast from a RealmObject inheritor to RealmObjectBase or when we
        /// get passed <see cref="object"/>.
        /// </summary>
        /// <typeparam name="TSource">The type from which to convert.</typeparam>
        /// <typeparam name="TTarget">The type to which <typeparamref name="TSource"/> will be converted.</typeparam>
        private class InheritanceConverter<TSource, TTarget> : ISpecializedConverter<TSource, TTarget>
        {
            public TTarget Convert(TSource source) => source is TTarget obj ? obj : throw new NotSupportedException($"No conversion exists from {typeof(TSource).FullName} to {typeof(TTarget).FullName}");
        }

        #region ToRealmValue Converters

        public class CharRealmValueConverter : ISpecializedConverter<char, RealmValue>
        {
            public RealmValue Convert(char value) => value;
        }

        public class ByteRealmValueConverter : ISpecializedConverter<byte, RealmValue>
        {
            public RealmValue Convert(byte value) => value;
        }

        public class ShortRealmValueConverter : ISpecializedConverter<short, RealmValue>
        {
            public RealmValue Convert(short value) => value;
        }

        public class IntRealmValueConverter : ISpecializedConverter<int, RealmValue>
        {
            public RealmValue Convert(int value) => value;
        }

        public class LongRealmValueConverter : ISpecializedConverter<long, RealmValue>
        {
            public RealmValue Convert(long value) => value;
        }

        public class FloatRealmValueConverter : ISpecializedConverter<float, RealmValue>
        {
            public RealmValue Convert(float value) => value;
        }

        public class DoubleRealmValueConverter : ISpecializedConverter<double, RealmValue>
        {
            public RealmValue Convert(double value) => value;
        }

        public class BoolRealmValueConverter : ISpecializedConverter<bool, RealmValue>
        {
            public RealmValue Convert(bool value) => value;
        }

        public class DateTimeOffsetRealmValueConverter : ISpecializedConverter<DateTimeOffset, RealmValue>
        {
            public RealmValue Convert(DateTimeOffset value) => value;
        }

        public class DecimalRealmValueConverter : ISpecializedConverter<decimal, RealmValue>
        {
            public RealmValue Convert(decimal value) => value;
        }

        public class Decimal128RealmValueConverter : ISpecializedConverter<Decimal128, RealmValue>
        {
            public RealmValue Convert(Decimal128 value) => value;
        }

        public class ObjectIdRealmValueConverter : ISpecializedConverter<ObjectId, RealmValue>
        {
            public RealmValue Convert(ObjectId value) => value;
        }

        public class GuidRealmValueConverter : ISpecializedConverter<Guid, RealmValue>
        {
            public RealmValue Convert(Guid value) => value;
        }

        public class NullableCharRealmValueConverter : ISpecializedConverter<char?, RealmValue>
        {
            public RealmValue Convert(char? value) => value;
        }

        public class NullableByteRealmValueConverter : ISpecializedConverter<byte?, RealmValue>
        {
            public RealmValue Convert(byte? value) => value;
        }

        public class NullableShortRealmValueConverter : ISpecializedConverter<short?, RealmValue>
        {
            public RealmValue Convert(short? value) => value;
        }

        public class NullableIntRealmValueConverter : ISpecializedConverter<int?, RealmValue>
        {
            public RealmValue Convert(int? value) => value;
        }

        public class NullableLongRealmValueConverter : ISpecializedConverter<long?, RealmValue>
        {
            public RealmValue Convert(long? value) => value;
        }

        public class NullableFloatRealmValueConverter : ISpecializedConverter<float?, RealmValue>
        {
            public RealmValue Convert(float? value) => value;
        }

        public class NullableDoubleRealmValueConverter : ISpecializedConverter<double?, RealmValue>
        {
            public RealmValue Convert(double? value) => value;
        }

        public class NullableBoolRealmValueConverter : ISpecializedConverter<bool?, RealmValue>
        {
            public RealmValue Convert(bool? value) => value;
        }

        public class NullableDateTimeOffsetRealmValueConverter : ISpecializedConverter<DateTimeOffset?, RealmValue>
        {
            public RealmValue Convert(DateTimeOffset? value) => value;
        }

        public class NullableDecimalRealmValueConverter : ISpecializedConverter<decimal?, RealmValue>
        {
            public RealmValue Convert(decimal? value) => value;
        }

        public class NullableDecimal128RealmValueConverter : ISpecializedConverter<Decimal128?, RealmValue>
        {
            public RealmValue Convert(Decimal128? value) => value;
        }

        public class NullableObjectIdRealmValueConverter : ISpecializedConverter<ObjectId?, RealmValue>
        {
            public RealmValue Convert(ObjectId? value) => value;
        }

        public class NullableGuidRealmValueConverter : ISpecializedConverter<Guid?, RealmValue>
        {
            public RealmValue Convert(Guid? value) => value;
        }

        public class RealmIntegerByteRealmValueConverter : ISpecializedConverter<RealmInteger<byte>, RealmValue>
        {
            public RealmValue Convert(RealmInteger<byte> value) => value;
        }

        public class RealmIntegerShortRealmValueConverter : ISpecializedConverter<RealmInteger<short>, RealmValue>
        {
            public RealmValue Convert(RealmInteger<short> value) => value;
        }

        public class RealmIntegerIntRealmValueConverter : ISpecializedConverter<RealmInteger<int>, RealmValue>
        {
            public RealmValue Convert(RealmInteger<int> value) => value;
        }

        public class RealmIntegerLongRealmValueConverter : ISpecializedConverter<RealmInteger<long>, RealmValue>
        {
            public RealmValue Convert(RealmInteger<long> value) => value;
        }

        public class NullableRealmIntegerByteRealmValueConverter : ISpecializedConverter<RealmInteger<byte>?, RealmValue>
        {
            public RealmValue Convert(RealmInteger<byte>? value) => value;
        }

        public class NullableRealmIntegerShortRealmValueConverter : ISpecializedConverter<RealmInteger<short>?, RealmValue>
        {
            public RealmValue Convert(RealmInteger<short>? value) => value;
        }

        public class NullableRealmIntegerIntRealmValueConverter : ISpecializedConverter<RealmInteger<int>?, RealmValue>
        {
            public RealmValue Convert(RealmInteger<int>? value) => value;
        }

        public class NullableRealmIntegerLongRealmValueConverter : ISpecializedConverter<RealmInteger<long>?, RealmValue>
        {
            public RealmValue Convert(RealmInteger<long>? value) => value;
        }

        public class ByteArrayRealmValueConverter : ISpecializedConverter<byte[], RealmValue>
        {
            public RealmValue Convert(byte[] value) => value;
        }

        public class StringRealmValueConverter : ISpecializedConverter<string, RealmValue>
        {
            public RealmValue Convert(string value) => value;
        }

        public class RealmObjectBaseRealmValueConverter : ISpecializedConverter<RealmObjectBase, RealmValue>
        {
            public RealmValue Convert(RealmObjectBase value) => value;
        }
        #endregion ToRealmValue Converters

        #region FromRealmValue Converters

        public class RealmValueCharConverter : ISpecializedConverter<RealmValue, char>
        {
            public char Convert(RealmValue value) => (char)value;
        }

        public class RealmValueByteConverter : ISpecializedConverter<RealmValue, byte>
        {
            public byte Convert(RealmValue value) => (byte)value;
        }

        public class RealmValueShortConverter : ISpecializedConverter<RealmValue, short>
        {
            public short Convert(RealmValue value) => (short)value;
        }

        public class RealmValueIntConverter : ISpecializedConverter<RealmValue, int>
        {
            public int Convert(RealmValue value) => (int)value;
        }

        public class RealmValueLongConverter : ISpecializedConverter<RealmValue, long>
        {
            public long Convert(RealmValue value) => (long)value;
        }

        public class RealmValueFloatConverter : ISpecializedConverter<RealmValue, float>
        {
            public float Convert(RealmValue value) => (float)value;
        }

        public class RealmValueDoubleConverter : ISpecializedConverter<RealmValue, double>
        {
            public double Convert(RealmValue value) => (double)value;
        }

        public class RealmValueBoolConverter : ISpecializedConverter<RealmValue, bool>
        {
            public bool Convert(RealmValue value) => (bool)value;
        }

        public class RealmValueDateTimeOffsetConverter : ISpecializedConverter<RealmValue, DateTimeOffset>
        {
            public DateTimeOffset Convert(RealmValue value) => (DateTimeOffset)value;
        }

        public class RealmValueDecimalConverter : ISpecializedConverter<RealmValue, decimal>
        {
            public decimal Convert(RealmValue value) => (decimal)value;
        }

        public class RealmValueDecimal128Converter : ISpecializedConverter<RealmValue, Decimal128>
        {
            public Decimal128 Convert(RealmValue value) => (Decimal128)value;
        }

        public class RealmValueObjectIdConverter : ISpecializedConverter<RealmValue, ObjectId>
        {
            public ObjectId Convert(RealmValue value) => (ObjectId)value;
        }

        public class RealmValueGuidConverter : ISpecializedConverter<RealmValue, Guid>
        {
            public Guid Convert(RealmValue value) => (Guid)value;
        }

        public class RealmValueNullableCharConverter : ISpecializedConverter<RealmValue, char?>
        {
            public char? Convert(RealmValue value) => (char?)value;
        }

        public class RealmValueNullableByteConverter : ISpecializedConverter<RealmValue, byte?>
        {
            public byte? Convert(RealmValue value) => (byte?)value;
        }

        public class RealmValueNullableShortConverter : ISpecializedConverter<RealmValue, short?>
        {
            public short? Convert(RealmValue value) => (short?)value;
        }

        public class RealmValueNullableIntConverter : ISpecializedConverter<RealmValue, int?>
        {
            public int? Convert(RealmValue value) => (int?)value;
        }

        public class RealmValueNullableLongConverter : ISpecializedConverter<RealmValue, long?>
        {
            public long? Convert(RealmValue value) => (long?)value;
        }

        public class RealmValueNullableFloatConverter : ISpecializedConverter<RealmValue, float?>
        {
            public float? Convert(RealmValue value) => (float?)value;
        }

        public class RealmValueNullableDoubleConverter : ISpecializedConverter<RealmValue, double?>
        {
            public double? Convert(RealmValue value) => (double?)value;
        }

        public class RealmValueNullableBoolConverter : ISpecializedConverter<RealmValue, bool?>
        {
            public bool? Convert(RealmValue value) => (bool?)value;
        }

        public class RealmValueNullableDateTimeOffsetConverter : ISpecializedConverter<RealmValue, DateTimeOffset?>
        {
            public DateTimeOffset? Convert(RealmValue value) => (DateTimeOffset?)value;
        }

        public class RealmValueNullableDecimalConverter : ISpecializedConverter<RealmValue, decimal?>
        {
            public decimal? Convert(RealmValue value) => (decimal?)value;
        }

        public class RealmValueNullableDecimal128Converter : ISpecializedConverter<RealmValue, Decimal128?>
        {
            public Decimal128? Convert(RealmValue value) => (Decimal128?)value;
        }

        public class RealmValueNullableObjectIdConverter : ISpecializedConverter<RealmValue, ObjectId?>
        {
            public ObjectId? Convert(RealmValue value) => (ObjectId?)value;
        }

        public class RealmValueNullableGuidConverter : ISpecializedConverter<RealmValue, Guid?>
        {
            public Guid? Convert(RealmValue value) => (Guid?)value;
        }

        public class RealmValueRealmIntegerByteConverter : ISpecializedConverter<RealmValue, RealmInteger<byte>>
        {
            public RealmInteger<byte> Convert(RealmValue value) => (RealmInteger<byte>)value;
        }

        public class RealmValueRealmIntegerShortConverter : ISpecializedConverter<RealmValue, RealmInteger<short>>
        {
            public RealmInteger<short> Convert(RealmValue value) => (RealmInteger<short>)value;
        }

        public class RealmValueRealmIntegerIntConverter : ISpecializedConverter<RealmValue, RealmInteger<int>>
        {
            public RealmInteger<int> Convert(RealmValue value) => (RealmInteger<int>)value;
        }

        public class RealmValueRealmIntegerLongConverter : ISpecializedConverter<RealmValue, RealmInteger<long>>
        {
            public RealmInteger<long> Convert(RealmValue value) => (RealmInteger<long>)value;
        }

        public class RealmValueNullableRealmIntegerByteConverter : ISpecializedConverter<RealmValue, RealmInteger<byte>?>
        {
            public RealmInteger<byte>? Convert(RealmValue value) => (RealmInteger<byte>?)value;
        }

        public class RealmValueNullableRealmIntegerShortConverter : ISpecializedConverter<RealmValue, RealmInteger<short>?>
        {
            public RealmInteger<short>? Convert(RealmValue value) => (RealmInteger<short>?)value;
        }

        public class RealmValueNullableRealmIntegerIntConverter : ISpecializedConverter<RealmValue, RealmInteger<int>?>
        {
            public RealmInteger<int>? Convert(RealmValue value) => (RealmInteger<int>?)value;
        }

        public class RealmValueNullableRealmIntegerLongConverter : ISpecializedConverter<RealmValue, RealmInteger<long>?>
        {
            public RealmInteger<long>? Convert(RealmValue value) => (RealmInteger<long>?)value;
        }

        public class RealmValueByteArrayConverter : ISpecializedConverter<RealmValue, byte[]>
        {
            public byte[] Convert(RealmValue value) => (byte[])value;
        }

        public class RealmValueStringConverter : ISpecializedConverter<RealmValue, string>
        {
            public string Convert(RealmValue value) => (string)value;
        }

        public class RealmValueRealmObjectBaseConverter : ISpecializedConverter<RealmValue, RealmObjectBase>
        {
            public RealmObjectBase Convert(RealmValue value) => (RealmObjectBase)value;
        }
        #endregion FromRealmValue Converters

        #region Integral Converters

        public class CharNullableCharConverter : ISpecializedConverter<char, char?>
        {
            public char? Convert(char value) => (char)value;
        }

        public class CharNullableByteConverter : ISpecializedConverter<char, byte?>
        {
            public byte? Convert(char value) => (byte)value;
        }

        public class CharNullableShortConverter : ISpecializedConverter<char, short?>
        {
            public short? Convert(char value) => (short)value;
        }

        public class CharNullableIntConverter : ISpecializedConverter<char, int?>
        {
            public int? Convert(char value) => value;
        }

        public class CharNullableLongConverter : ISpecializedConverter<char, long?>
        {
            public long? Convert(char value) => value;
        }

        public class CharNullableRealmIntegerByteConverter : ISpecializedConverter<char, RealmInteger<byte>?>
        {
            public RealmInteger<byte>? Convert(char value) => (byte)value;
        }

        public class CharNullableRealmIntegerShortConverter : ISpecializedConverter<char, RealmInteger<short>?>
        {
            public RealmInteger<short>? Convert(char value) => (short)value;
        }

        public class CharNullableRealmIntegerIntConverter : ISpecializedConverter<char, RealmInteger<int>?>
        {
            public RealmInteger<int>? Convert(char value) => value;
        }

        public class CharNullableRealmIntegerLongConverter : ISpecializedConverter<char, RealmInteger<long>?>
        {
            public RealmInteger<long>? Convert(char value) => value;
        }

        public class CharNullableFloatConverter : ISpecializedConverter<char, float?>
        {
            public float? Convert(char value) => value;
        }

        public class CharNullableDoubleConverter : ISpecializedConverter<char, double?>
        {
            public double? Convert(char value) => value;
        }

        public class CharNullableDecimalConverter : ISpecializedConverter<char, decimal?>
        {
            public decimal? Convert(char value) => value;
        }

        public class CharNullableDecimal128Converter : ISpecializedConverter<char, Decimal128?>
        {
            public Decimal128? Convert(char value) => value;
        }

        public class ByteNullableCharConverter : ISpecializedConverter<byte, char?>
        {
            public char? Convert(byte value) => (char)value;
        }

        public class ByteNullableByteConverter : ISpecializedConverter<byte, byte?>
        {
            public byte? Convert(byte value) => value;
        }

        public class ByteNullableShortConverter : ISpecializedConverter<byte, short?>
        {
            public short? Convert(byte value) => value;
        }

        public class ByteNullableIntConverter : ISpecializedConverter<byte, int?>
        {
            public int? Convert(byte value) => value;
        }

        public class ByteNullableLongConverter : ISpecializedConverter<byte, long?>
        {
            public long? Convert(byte value) => value;
        }

        public class ByteNullableRealmIntegerByteConverter : ISpecializedConverter<byte, RealmInteger<byte>?>
        {
            public RealmInteger<byte>? Convert(byte value) => value;
        }

        public class ByteNullableRealmIntegerShortConverter : ISpecializedConverter<byte, RealmInteger<short>?>
        {
            public RealmInteger<short>? Convert(byte value) => value;
        }

        public class ByteNullableRealmIntegerIntConverter : ISpecializedConverter<byte, RealmInteger<int>?>
        {
            public RealmInteger<int>? Convert(byte value) => value;
        }

        public class ByteNullableRealmIntegerLongConverter : ISpecializedConverter<byte, RealmInteger<long>?>
        {
            public RealmInteger<long>? Convert(byte value) => value;
        }

        public class ByteNullableFloatConverter : ISpecializedConverter<byte, float?>
        {
            public float? Convert(byte value) => value;
        }

        public class ByteNullableDoubleConverter : ISpecializedConverter<byte, double?>
        {
            public double? Convert(byte value) => value;
        }

        public class ByteNullableDecimalConverter : ISpecializedConverter<byte, decimal?>
        {
            public decimal? Convert(byte value) => value;
        }

        public class ByteNullableDecimal128Converter : ISpecializedConverter<byte, Decimal128?>
        {
            public Decimal128? Convert(byte value) => value;
        }

        public class ShortNullableCharConverter : ISpecializedConverter<short, char?>
        {
            public char? Convert(short value) => (char)value;
        }

        public class ShortNullableByteConverter : ISpecializedConverter<short, byte?>
        {
            public byte? Convert(short value) => (byte)value;
        }

        public class ShortNullableShortConverter : ISpecializedConverter<short, short?>
        {
            public short? Convert(short value) => value;
        }

        public class ShortNullableIntConverter : ISpecializedConverter<short, int?>
        {
            public int? Convert(short value) => value;
        }

        public class ShortNullableLongConverter : ISpecializedConverter<short, long?>
        {
            public long? Convert(short value) => value;
        }

        public class ShortNullableRealmIntegerByteConverter : ISpecializedConverter<short, RealmInteger<byte>?>
        {
            public RealmInteger<byte>? Convert(short value) => (byte)value;
        }

        public class ShortNullableRealmIntegerShortConverter : ISpecializedConverter<short, RealmInteger<short>?>
        {
            public RealmInteger<short>? Convert(short value) => value;
        }

        public class ShortNullableRealmIntegerIntConverter : ISpecializedConverter<short, RealmInteger<int>?>
        {
            public RealmInteger<int>? Convert(short value) => value;
        }

        public class ShortNullableRealmIntegerLongConverter : ISpecializedConverter<short, RealmInteger<long>?>
        {
            public RealmInteger<long>? Convert(short value) => value;
        }

        public class ShortNullableFloatConverter : ISpecializedConverter<short, float?>
        {
            public float? Convert(short value) => value;
        }

        public class ShortNullableDoubleConverter : ISpecializedConverter<short, double?>
        {
            public double? Convert(short value) => value;
        }

        public class ShortNullableDecimalConverter : ISpecializedConverter<short, decimal?>
        {
            public decimal? Convert(short value) => value;
        }

        public class ShortNullableDecimal128Converter : ISpecializedConverter<short, Decimal128?>
        {
            public Decimal128? Convert(short value) => value;
        }

        public class IntNullableCharConverter : ISpecializedConverter<int, char?>
        {
            public char? Convert(int value) => (char)value;
        }

        public class IntNullableByteConverter : ISpecializedConverter<int, byte?>
        {
            public byte? Convert(int value) => (byte)value;
        }

        public class IntNullableShortConverter : ISpecializedConverter<int, short?>
        {
            public short? Convert(int value) => (short)value;
        }

        public class IntNullableIntConverter : ISpecializedConverter<int, int?>
        {
            public int? Convert(int value) => value;
        }

        public class IntNullableLongConverter : ISpecializedConverter<int, long?>
        {
            public long? Convert(int value) => value;
        }

        public class IntNullableRealmIntegerByteConverter : ISpecializedConverter<int, RealmInteger<byte>?>
        {
            public RealmInteger<byte>? Convert(int value) => (byte)value;
        }

        public class IntNullableRealmIntegerShortConverter : ISpecializedConverter<int, RealmInteger<short>?>
        {
            public RealmInteger<short>? Convert(int value) => (short)value;
        }

        public class IntNullableRealmIntegerIntConverter : ISpecializedConverter<int, RealmInteger<int>?>
        {
            public RealmInteger<int>? Convert(int value) => value;
        }

        public class IntNullableRealmIntegerLongConverter : ISpecializedConverter<int, RealmInteger<long>?>
        {
            public RealmInteger<long>? Convert(int value) => value;
        }

        public class IntNullableFloatConverter : ISpecializedConverter<int, float?>
        {
            public float? Convert(int value) => value;
        }

        public class IntNullableDoubleConverter : ISpecializedConverter<int, double?>
        {
            public double? Convert(int value) => value;
        }

        public class IntNullableDecimalConverter : ISpecializedConverter<int, decimal?>
        {
            public decimal? Convert(int value) => value;
        }

        public class IntNullableDecimal128Converter : ISpecializedConverter<int, Decimal128?>
        {
            public Decimal128? Convert(int value) => value;
        }

        public class LongNullableCharConverter : ISpecializedConverter<long, char?>
        {
            public char? Convert(long value) => (char)value;
        }

        public class LongNullableByteConverter : ISpecializedConverter<long, byte?>
        {
            public byte? Convert(long value) => (byte)value;
        }

        public class LongNullableShortConverter : ISpecializedConverter<long, short?>
        {
            public short? Convert(long value) => (short)value;
        }

        public class LongNullableIntConverter : ISpecializedConverter<long, int?>
        {
            public int? Convert(long value) => (int)value;
        }

        public class LongNullableLongConverter : ISpecializedConverter<long, long?>
        {
            public long? Convert(long value) => value;
        }

        public class LongNullableRealmIntegerByteConverter : ISpecializedConverter<long, RealmInteger<byte>?>
        {
            public RealmInteger<byte>? Convert(long value) => (byte)value;
        }

        public class LongNullableRealmIntegerShortConverter : ISpecializedConverter<long, RealmInteger<short>?>
        {
            public RealmInteger<short>? Convert(long value) => (short)value;
        }

        public class LongNullableRealmIntegerIntConverter : ISpecializedConverter<long, RealmInteger<int>?>
        {
            public RealmInteger<int>? Convert(long value) => (int)value;
        }

        public class LongNullableRealmIntegerLongConverter : ISpecializedConverter<long, RealmInteger<long>?>
        {
            public RealmInteger<long>? Convert(long value) => value;
        }

        public class LongNullableFloatConverter : ISpecializedConverter<long, float?>
        {
            public float? Convert(long value) => value;
        }

        public class LongNullableDoubleConverter : ISpecializedConverter<long, double?>
        {
            public double? Convert(long value) => value;
        }

        public class LongNullableDecimalConverter : ISpecializedConverter<long, decimal?>
        {
            public decimal? Convert(long value) => value;
        }

        public class LongNullableDecimal128Converter : ISpecializedConverter<long, Decimal128?>
        {
            public Decimal128? Convert(long value) => value;
        }

        public class RealmIntegerByteNullableCharConverter : ISpecializedConverter<RealmInteger<byte>, char?>
        {
            public char? Convert(RealmInteger<byte> value) => (char)(byte)value;
        }

        public class RealmIntegerByteNullableByteConverter : ISpecializedConverter<RealmInteger<byte>, byte?>
        {
            public byte? Convert(RealmInteger<byte> value) => value;
        }

        public class RealmIntegerByteNullableShortConverter : ISpecializedConverter<RealmInteger<byte>, short?>
        {
            public short? Convert(RealmInteger<byte> value) => value;
        }

        public class RealmIntegerByteNullableIntConverter : ISpecializedConverter<RealmInteger<byte>, int?>
        {
            public int? Convert(RealmInteger<byte> value) => value;
        }

        public class RealmIntegerByteNullableLongConverter : ISpecializedConverter<RealmInteger<byte>, long?>
        {
            public long? Convert(RealmInteger<byte> value) => value;
        }

        public class RealmIntegerByteNullableRealmIntegerByteConverter : ISpecializedConverter<RealmInteger<byte>, RealmInteger<byte>?>
        {
            public RealmInteger<byte>? Convert(RealmInteger<byte> value) => value;
        }

        public class RealmIntegerByteNullableRealmIntegerShortConverter : ISpecializedConverter<RealmInteger<byte>, RealmInteger<short>?>
        {
            public RealmInteger<short>? Convert(RealmInteger<byte> value) => (short)value;
        }

        public class RealmIntegerByteNullableRealmIntegerIntConverter : ISpecializedConverter<RealmInteger<byte>, RealmInteger<int>?>
        {
            public RealmInteger<int>? Convert(RealmInteger<byte> value) => (int)value;
        }

        public class RealmIntegerByteNullableRealmIntegerLongConverter : ISpecializedConverter<RealmInteger<byte>, RealmInteger<long>?>
        {
            public RealmInteger<long>? Convert(RealmInteger<byte> value) => (long)value;
        }

        public class RealmIntegerByteNullableFloatConverter : ISpecializedConverter<RealmInteger<byte>, float?>
        {
            public float? Convert(RealmInteger<byte> value) => value;
        }

        public class RealmIntegerByteNullableDoubleConverter : ISpecializedConverter<RealmInteger<byte>, double?>
        {
            public double? Convert(RealmInteger<byte> value) => value;
        }

        public class RealmIntegerByteNullableDecimalConverter : ISpecializedConverter<RealmInteger<byte>, decimal?>
        {
            public decimal? Convert(RealmInteger<byte> value) => value;
        }

        public class RealmIntegerByteNullableDecimal128Converter : ISpecializedConverter<RealmInteger<byte>, Decimal128?>
        {
            public Decimal128? Convert(RealmInteger<byte> value) => (byte)value;
        }

        public class RealmIntegerShortNullableCharConverter : ISpecializedConverter<RealmInteger<short>, char?>
        {
            public char? Convert(RealmInteger<short> value) => (char)(short)value;
        }

        public class RealmIntegerShortNullableByteConverter : ISpecializedConverter<RealmInteger<short>, byte?>
        {
            public byte? Convert(RealmInteger<short> value) => (byte)value;
        }

        public class RealmIntegerShortNullableShortConverter : ISpecializedConverter<RealmInteger<short>, short?>
        {
            public short? Convert(RealmInteger<short> value) => value;
        }

        public class RealmIntegerShortNullableIntConverter : ISpecializedConverter<RealmInteger<short>, int?>
        {
            public int? Convert(RealmInteger<short> value) => value;
        }

        public class RealmIntegerShortNullableLongConverter : ISpecializedConverter<RealmInteger<short>, long?>
        {
            public long? Convert(RealmInteger<short> value) => value;
        }

        public class RealmIntegerShortNullableRealmIntegerByteConverter : ISpecializedConverter<RealmInteger<short>, RealmInteger<byte>?>
        {
            public RealmInteger<byte>? Convert(RealmInteger<short> value) => (byte)value;
        }

        public class RealmIntegerShortNullableRealmIntegerShortConverter : ISpecializedConverter<RealmInteger<short>, RealmInteger<short>?>
        {
            public RealmInteger<short>? Convert(RealmInteger<short> value) => value;
        }

        public class RealmIntegerShortNullableRealmIntegerIntConverter : ISpecializedConverter<RealmInteger<short>, RealmInteger<int>?>
        {
            public RealmInteger<int>? Convert(RealmInteger<short> value) => (int)value;
        }

        public class RealmIntegerShortNullableRealmIntegerLongConverter : ISpecializedConverter<RealmInteger<short>, RealmInteger<long>?>
        {
            public RealmInteger<long>? Convert(RealmInteger<short> value) => (long)value;
        }

        public class RealmIntegerShortNullableFloatConverter : ISpecializedConverter<RealmInteger<short>, float?>
        {
            public float? Convert(RealmInteger<short> value) => value;
        }

        public class RealmIntegerShortNullableDoubleConverter : ISpecializedConverter<RealmInteger<short>, double?>
        {
            public double? Convert(RealmInteger<short> value) => value;
        }

        public class RealmIntegerShortNullableDecimalConverter : ISpecializedConverter<RealmInteger<short>, decimal?>
        {
            public decimal? Convert(RealmInteger<short> value) => value;
        }

        public class RealmIntegerShortNullableDecimal128Converter : ISpecializedConverter<RealmInteger<short>, Decimal128?>
        {
            public Decimal128? Convert(RealmInteger<short> value) => (short)value;
        }

        public class RealmIntegerIntNullableCharConverter : ISpecializedConverter<RealmInteger<int>, char?>
        {
            public char? Convert(RealmInteger<int> value) => (char)value;
        }

        public class RealmIntegerIntNullableByteConverter : ISpecializedConverter<RealmInteger<int>, byte?>
        {
            public byte? Convert(RealmInteger<int> value) => (byte)value;
        }

        public class RealmIntegerIntNullableShortConverter : ISpecializedConverter<RealmInteger<int>, short?>
        {
            public short? Convert(RealmInteger<int> value) => (short)value;
        }

        public class RealmIntegerIntNullableIntConverter : ISpecializedConverter<RealmInteger<int>, int?>
        {
            public int? Convert(RealmInteger<int> value) => value;
        }

        public class RealmIntegerIntNullableLongConverter : ISpecializedConverter<RealmInteger<int>, long?>
        {
            public long? Convert(RealmInteger<int> value) => value;
        }

        public class RealmIntegerIntNullableRealmIntegerByteConverter : ISpecializedConverter<RealmInteger<int>, RealmInteger<byte>?>
        {
            public RealmInteger<byte>? Convert(RealmInteger<int> value) => (byte)value;
        }

        public class RealmIntegerIntNullableRealmIntegerShortConverter : ISpecializedConverter<RealmInteger<int>, RealmInteger<short>?>
        {
            public RealmInteger<short>? Convert(RealmInteger<int> value) => (short)value;
        }

        public class RealmIntegerIntNullableRealmIntegerIntConverter : ISpecializedConverter<RealmInteger<int>, RealmInteger<int>?>
        {
            public RealmInteger<int>? Convert(RealmInteger<int> value) => value;
        }

        public class RealmIntegerIntNullableRealmIntegerLongConverter : ISpecializedConverter<RealmInteger<int>, RealmInteger<long>?>
        {
            public RealmInteger<long>? Convert(RealmInteger<int> value) => (long)value;
        }

        public class RealmIntegerIntNullableFloatConverter : ISpecializedConverter<RealmInteger<int>, float?>
        {
            public float? Convert(RealmInteger<int> value) => value;
        }

        public class RealmIntegerIntNullableDoubleConverter : ISpecializedConverter<RealmInteger<int>, double?>
        {
            public double? Convert(RealmInteger<int> value) => value;
        }

        public class RealmIntegerIntNullableDecimalConverter : ISpecializedConverter<RealmInteger<int>, decimal?>
        {
            public decimal? Convert(RealmInteger<int> value) => value;
        }

        public class RealmIntegerIntNullableDecimal128Converter : ISpecializedConverter<RealmInteger<int>, Decimal128?>
        {
            public Decimal128? Convert(RealmInteger<int> value) => (int)value;
        }

        public class RealmIntegerLongNullableCharConverter : ISpecializedConverter<RealmInteger<long>, char?>
        {
            public char? Convert(RealmInteger<long> value) => (char)value;
        }

        public class RealmIntegerLongNullableByteConverter : ISpecializedConverter<RealmInteger<long>, byte?>
        {
            public byte? Convert(RealmInteger<long> value) => (byte)value;
        }

        public class RealmIntegerLongNullableShortConverter : ISpecializedConverter<RealmInteger<long>, short?>
        {
            public short? Convert(RealmInteger<long> value) => (short)value;
        }

        public class RealmIntegerLongNullableIntConverter : ISpecializedConverter<RealmInteger<long>, int?>
        {
            public int? Convert(RealmInteger<long> value) => (int)value;
        }

        public class RealmIntegerLongNullableLongConverter : ISpecializedConverter<RealmInteger<long>, long?>
        {
            public long? Convert(RealmInteger<long> value) => value;
        }

        public class RealmIntegerLongNullableRealmIntegerByteConverter : ISpecializedConverter<RealmInteger<long>, RealmInteger<byte>?>
        {
            public RealmInteger<byte>? Convert(RealmInteger<long> value) => (byte)value;
        }

        public class RealmIntegerLongNullableRealmIntegerShortConverter : ISpecializedConverter<RealmInteger<long>, RealmInteger<short>?>
        {
            public RealmInteger<short>? Convert(RealmInteger<long> value) => (short)value;
        }

        public class RealmIntegerLongNullableRealmIntegerIntConverter : ISpecializedConverter<RealmInteger<long>, RealmInteger<int>?>
        {
            public RealmInteger<int>? Convert(RealmInteger<long> value) => (int)value;
        }

        public class RealmIntegerLongNullableRealmIntegerLongConverter : ISpecializedConverter<RealmInteger<long>, RealmInteger<long>?>
        {
            public RealmInteger<long>? Convert(RealmInteger<long> value) => value;
        }

        public class RealmIntegerLongNullableFloatConverter : ISpecializedConverter<RealmInteger<long>, float?>
        {
            public float? Convert(RealmInteger<long> value) => value;
        }

        public class RealmIntegerLongNullableDoubleConverter : ISpecializedConverter<RealmInteger<long>, double?>
        {
            public double? Convert(RealmInteger<long> value) => value;
        }

        public class RealmIntegerLongNullableDecimalConverter : ISpecializedConverter<RealmInteger<long>, decimal?>
        {
            public decimal? Convert(RealmInteger<long> value) => value;
        }

        public class RealmIntegerLongNullableDecimal128Converter : ISpecializedConverter<RealmInteger<long>, Decimal128?>
        {
            public Decimal128? Convert(RealmInteger<long> value) => (long)value;
        }

        public class CharByteConverter : ISpecializedConverter<char, byte>
        {
            public byte Convert(char value) => (byte)value;
        }

        public class CharShortConverter : ISpecializedConverter<char, short>
        {
            public short Convert(char value) => (short)value;
        }

        public class CharIntConverter : ISpecializedConverter<char, int>
        {
            public int Convert(char value) => value;
        }

        public class CharLongConverter : ISpecializedConverter<char, long>
        {
            public long Convert(char value) => value;
        }

        public class CharRealmIntegerByteConverter : ISpecializedConverter<char, RealmInteger<byte>>
        {
            public RealmInteger<byte> Convert(char value) => (byte)value;
        }

        public class CharRealmIntegerShortConverter : ISpecializedConverter<char, RealmInteger<short>>
        {
            public RealmInteger<short> Convert(char value) => (short)value;
        }

        public class CharRealmIntegerIntConverter : ISpecializedConverter<char, RealmInteger<int>>
        {
            public RealmInteger<int> Convert(char value) => value;
        }

        public class CharRealmIntegerLongConverter : ISpecializedConverter<char, RealmInteger<long>>
        {
            public RealmInteger<long> Convert(char value) => value;
        }

        public class CharFloatConverter : ISpecializedConverter<char, float>
        {
            public float Convert(char value) => value;
        }

        public class CharDoubleConverter : ISpecializedConverter<char, double>
        {
            public double Convert(char value) => value;
        }

        public class CharDecimalConverter : ISpecializedConverter<char, decimal>
        {
            public decimal Convert(char value) => value;
        }

        public class CharDecimal128Converter : ISpecializedConverter<char, Decimal128>
        {
            public Decimal128 Convert(char value) => value;
        }

        public class ByteCharConverter : ISpecializedConverter<byte, char>
        {
            public char Convert(byte value) => (char)value;
        }

        public class ByteShortConverter : ISpecializedConverter<byte, short>
        {
            public short Convert(byte value) => value;
        }

        public class ByteIntConverter : ISpecializedConverter<byte, int>
        {
            public int Convert(byte value) => value;
        }

        public class ByteLongConverter : ISpecializedConverter<byte, long>
        {
            public long Convert(byte value) => value;
        }

        public class ByteRealmIntegerByteConverter : ISpecializedConverter<byte, RealmInteger<byte>>
        {
            public RealmInteger<byte> Convert(byte value) => value;
        }

        public class ByteRealmIntegerShortConverter : ISpecializedConverter<byte, RealmInteger<short>>
        {
            public RealmInteger<short> Convert(byte value) => value;
        }

        public class ByteRealmIntegerIntConverter : ISpecializedConverter<byte, RealmInteger<int>>
        {
            public RealmInteger<int> Convert(byte value) => value;
        }

        public class ByteRealmIntegerLongConverter : ISpecializedConverter<byte, RealmInteger<long>>
        {
            public RealmInteger<long> Convert(byte value) => value;
        }

        public class ByteFloatConverter : ISpecializedConverter<byte, float>
        {
            public float Convert(byte value) => value;
        }

        public class ByteDoubleConverter : ISpecializedConverter<byte, double>
        {
            public double Convert(byte value) => value;
        }

        public class ByteDecimalConverter : ISpecializedConverter<byte, decimal>
        {
            public decimal Convert(byte value) => value;
        }

        public class ByteDecimal128Converter : ISpecializedConverter<byte, Decimal128>
        {
            public Decimal128 Convert(byte value) => value;
        }

        public class ShortCharConverter : ISpecializedConverter<short, char>
        {
            public char Convert(short value) => (char)value;
        }

        public class ShortByteConverter : ISpecializedConverter<short, byte>
        {
            public byte Convert(short value) => (byte)value;
        }

        public class ShortIntConverter : ISpecializedConverter<short, int>
        {
            public int Convert(short value) => value;
        }

        public class ShortLongConverter : ISpecializedConverter<short, long>
        {
            public long Convert(short value) => value;
        }

        public class ShortRealmIntegerByteConverter : ISpecializedConverter<short, RealmInteger<byte>>
        {
            public RealmInteger<byte> Convert(short value) => (byte)value;
        }

        public class ShortRealmIntegerShortConverter : ISpecializedConverter<short, RealmInteger<short>>
        {
            public RealmInteger<short> Convert(short value) => value;
        }

        public class ShortRealmIntegerIntConverter : ISpecializedConverter<short, RealmInteger<int>>
        {
            public RealmInteger<int> Convert(short value) => value;
        }

        public class ShortRealmIntegerLongConverter : ISpecializedConverter<short, RealmInteger<long>>
        {
            public RealmInteger<long> Convert(short value) => value;
        }

        public class ShortFloatConverter : ISpecializedConverter<short, float>
        {
            public float Convert(short value) => value;
        }

        public class ShortDoubleConverter : ISpecializedConverter<short, double>
        {
            public double Convert(short value) => value;
        }

        public class ShortDecimalConverter : ISpecializedConverter<short, decimal>
        {
            public decimal Convert(short value) => value;
        }

        public class ShortDecimal128Converter : ISpecializedConverter<short, Decimal128>
        {
            public Decimal128 Convert(short value) => value;
        }

        public class IntCharConverter : ISpecializedConverter<int, char>
        {
            public char Convert(int value) => (char)value;
        }

        public class IntByteConverter : ISpecializedConverter<int, byte>
        {
            public byte Convert(int value) => (byte)value;
        }

        public class IntShortConverter : ISpecializedConverter<int, short>
        {
            public short Convert(int value) => (short)value;
        }

        public class IntLongConverter : ISpecializedConverter<int, long>
        {
            public long Convert(int value) => value;
        }

        public class IntRealmIntegerByteConverter : ISpecializedConverter<int, RealmInteger<byte>>
        {
            public RealmInteger<byte> Convert(int value) => (byte)value;
        }

        public class IntRealmIntegerShortConverter : ISpecializedConverter<int, RealmInteger<short>>
        {
            public RealmInteger<short> Convert(int value) => (short)value;
        }

        public class IntRealmIntegerIntConverter : ISpecializedConverter<int, RealmInteger<int>>
        {
            public RealmInteger<int> Convert(int value) => value;
        }

        public class IntRealmIntegerLongConverter : ISpecializedConverter<int, RealmInteger<long>>
        {
            public RealmInteger<long> Convert(int value) => value;
        }

        public class IntFloatConverter : ISpecializedConverter<int, float>
        {
            public float Convert(int value) => value;
        }

        public class IntDoubleConverter : ISpecializedConverter<int, double>
        {
            public double Convert(int value) => value;
        }

        public class IntDecimalConverter : ISpecializedConverter<int, decimal>
        {
            public decimal Convert(int value) => value;
        }

        public class IntDecimal128Converter : ISpecializedConverter<int, Decimal128>
        {
            public Decimal128 Convert(int value) => value;
        }

        public class LongCharConverter : ISpecializedConverter<long, char>
        {
            public char Convert(long value) => (char)value;
        }

        public class LongByteConverter : ISpecializedConverter<long, byte>
        {
            public byte Convert(long value) => (byte)value;
        }

        public class LongShortConverter : ISpecializedConverter<long, short>
        {
            public short Convert(long value) => (short)value;
        }

        public class LongIntConverter : ISpecializedConverter<long, int>
        {
            public int Convert(long value) => (int)value;
        }

        public class LongRealmIntegerByteConverter : ISpecializedConverter<long, RealmInteger<byte>>
        {
            public RealmInteger<byte> Convert(long value) => (byte)value;
        }

        public class LongRealmIntegerShortConverter : ISpecializedConverter<long, RealmInteger<short>>
        {
            public RealmInteger<short> Convert(long value) => (short)value;
        }

        public class LongRealmIntegerIntConverter : ISpecializedConverter<long, RealmInteger<int>>
        {
            public RealmInteger<int> Convert(long value) => (int)value;
        }

        public class LongRealmIntegerLongConverter : ISpecializedConverter<long, RealmInteger<long>>
        {
            public RealmInteger<long> Convert(long value) => value;
        }

        public class LongFloatConverter : ISpecializedConverter<long, float>
        {
            public float Convert(long value) => value;
        }

        public class LongDoubleConverter : ISpecializedConverter<long, double>
        {
            public double Convert(long value) => value;
        }

        public class LongDecimalConverter : ISpecializedConverter<long, decimal>
        {
            public decimal Convert(long value) => value;
        }

        public class LongDecimal128Converter : ISpecializedConverter<long, Decimal128>
        {
            public Decimal128 Convert(long value) => value;
        }

        public class RealmIntegerByteCharConverter : ISpecializedConverter<RealmInteger<byte>, char>
        {
            public char Convert(RealmInteger<byte> value) => (char)(byte)value;
        }

        public class RealmIntegerByteByteConverter : ISpecializedConverter<RealmInteger<byte>, byte>
        {
            public byte Convert(RealmInteger<byte> value) => value;
        }

        public class RealmIntegerByteShortConverter : ISpecializedConverter<RealmInteger<byte>, short>
        {
            public short Convert(RealmInteger<byte> value) => value;
        }

        public class RealmIntegerByteIntConverter : ISpecializedConverter<RealmInteger<byte>, int>
        {
            public int Convert(RealmInteger<byte> value) => value;
        }

        public class RealmIntegerByteLongConverter : ISpecializedConverter<RealmInteger<byte>, long>
        {
            public long Convert(RealmInteger<byte> value) => value;
        }

        public class RealmIntegerByteRealmIntegerShortConverter : ISpecializedConverter<RealmInteger<byte>, RealmInteger<short>>
        {
            public RealmInteger<short> Convert(RealmInteger<byte> value) => (short)value;
        }

        public class RealmIntegerByteRealmIntegerIntConverter : ISpecializedConverter<RealmInteger<byte>, RealmInteger<int>>
        {
            public RealmInteger<int> Convert(RealmInteger<byte> value) => (int)value;
        }

        public class RealmIntegerByteRealmIntegerLongConverter : ISpecializedConverter<RealmInteger<byte>, RealmInteger<long>>
        {
            public RealmInteger<long> Convert(RealmInteger<byte> value) => (long)value;
        }

        public class RealmIntegerByteFloatConverter : ISpecializedConverter<RealmInteger<byte>, float>
        {
            public float Convert(RealmInteger<byte> value) => value;
        }

        public class RealmIntegerByteDoubleConverter : ISpecializedConverter<RealmInteger<byte>, double>
        {
            public double Convert(RealmInteger<byte> value) => value;
        }

        public class RealmIntegerByteDecimalConverter : ISpecializedConverter<RealmInteger<byte>, decimal>
        {
            public decimal Convert(RealmInteger<byte> value) => value;
        }

        public class RealmIntegerByteDecimal128Converter : ISpecializedConverter<RealmInteger<byte>, Decimal128>
        {
            public Decimal128 Convert(RealmInteger<byte> value) => (byte)value;
        }

        public class RealmIntegerShortCharConverter : ISpecializedConverter<RealmInteger<short>, char>
        {
            public char Convert(RealmInteger<short> value) => (char)(short)value;
        }

        public class RealmIntegerShortByteConverter : ISpecializedConverter<RealmInteger<short>, byte>
        {
            public byte Convert(RealmInteger<short> value) => (byte)value;
        }

        public class RealmIntegerShortShortConverter : ISpecializedConverter<RealmInteger<short>, short>
        {
            public short Convert(RealmInteger<short> value) => value;
        }

        public class RealmIntegerShortIntConverter : ISpecializedConverter<RealmInteger<short>, int>
        {
            public int Convert(RealmInteger<short> value) => value;
        }

        public class RealmIntegerShortLongConverter : ISpecializedConverter<RealmInteger<short>, long>
        {
            public long Convert(RealmInteger<short> value) => value;
        }

        public class RealmIntegerShortRealmIntegerByteConverter : ISpecializedConverter<RealmInteger<short>, RealmInteger<byte>>
        {
            public RealmInteger<byte> Convert(RealmInteger<short> value) => (byte)value;
        }

        public class RealmIntegerShortRealmIntegerIntConverter : ISpecializedConverter<RealmInteger<short>, RealmInteger<int>>
        {
            public RealmInteger<int> Convert(RealmInteger<short> value) => (int)value;
        }

        public class RealmIntegerShortRealmIntegerLongConverter : ISpecializedConverter<RealmInteger<short>, RealmInteger<long>>
        {
            public RealmInteger<long> Convert(RealmInteger<short> value) => (long)value;
        }

        public class RealmIntegerShortFloatConverter : ISpecializedConverter<RealmInteger<short>, float>
        {
            public float Convert(RealmInteger<short> value) => value;
        }

        public class RealmIntegerShortDoubleConverter : ISpecializedConverter<RealmInteger<short>, double>
        {
            public double Convert(RealmInteger<short> value) => value;
        }

        public class RealmIntegerShortDecimalConverter : ISpecializedConverter<RealmInteger<short>, decimal>
        {
            public decimal Convert(RealmInteger<short> value) => value;
        }

        public class RealmIntegerShortDecimal128Converter : ISpecializedConverter<RealmInteger<short>, Decimal128>
        {
            public Decimal128 Convert(RealmInteger<short> value) => (short)value;
        }

        public class RealmIntegerIntCharConverter : ISpecializedConverter<RealmInteger<int>, char>
        {
            public char Convert(RealmInteger<int> value) => (char)value;
        }

        public class RealmIntegerIntByteConverter : ISpecializedConverter<RealmInteger<int>, byte>
        {
            public byte Convert(RealmInteger<int> value) => (byte)value;
        }

        public class RealmIntegerIntShortConverter : ISpecializedConverter<RealmInteger<int>, short>
        {
            public short Convert(RealmInteger<int> value) => (short)value;
        }

        public class RealmIntegerIntIntConverter : ISpecializedConverter<RealmInteger<int>, int>
        {
            public int Convert(RealmInteger<int> value) => value;
        }

        public class RealmIntegerIntLongConverter : ISpecializedConverter<RealmInteger<int>, long>
        {
            public long Convert(RealmInteger<int> value) => value;
        }

        public class RealmIntegerIntRealmIntegerByteConverter : ISpecializedConverter<RealmInteger<int>, RealmInteger<byte>>
        {
            public RealmInteger<byte> Convert(RealmInteger<int> value) => (byte)value;
        }

        public class RealmIntegerIntRealmIntegerShortConverter : ISpecializedConverter<RealmInteger<int>, RealmInteger<short>>
        {
            public RealmInteger<short> Convert(RealmInteger<int> value) => (short)value;
        }

        public class RealmIntegerIntRealmIntegerLongConverter : ISpecializedConverter<RealmInteger<int>, RealmInteger<long>>
        {
            public RealmInteger<long> Convert(RealmInteger<int> value) => (long)value;
        }

        public class RealmIntegerIntFloatConverter : ISpecializedConverter<RealmInteger<int>, float>
        {
            public float Convert(RealmInteger<int> value) => value;
        }

        public class RealmIntegerIntDoubleConverter : ISpecializedConverter<RealmInteger<int>, double>
        {
            public double Convert(RealmInteger<int> value) => value;
        }

        public class RealmIntegerIntDecimalConverter : ISpecializedConverter<RealmInteger<int>, decimal>
        {
            public decimal Convert(RealmInteger<int> value) => value;
        }

        public class RealmIntegerIntDecimal128Converter : ISpecializedConverter<RealmInteger<int>, Decimal128>
        {
            public Decimal128 Convert(RealmInteger<int> value) => (int)value;
        }

        public class RealmIntegerLongCharConverter : ISpecializedConverter<RealmInteger<long>, char>
        {
            public char Convert(RealmInteger<long> value) => (char)value;
        }

        public class RealmIntegerLongByteConverter : ISpecializedConverter<RealmInteger<long>, byte>
        {
            public byte Convert(RealmInteger<long> value) => (byte)value;
        }

        public class RealmIntegerLongShortConverter : ISpecializedConverter<RealmInteger<long>, short>
        {
            public short Convert(RealmInteger<long> value) => (short)value;
        }

        public class RealmIntegerLongIntConverter : ISpecializedConverter<RealmInteger<long>, int>
        {
            public int Convert(RealmInteger<long> value) => (int)value;
        }

        public class RealmIntegerLongLongConverter : ISpecializedConverter<RealmInteger<long>, long>
        {
            public long Convert(RealmInteger<long> value) => value;
        }

        public class RealmIntegerLongRealmIntegerByteConverter : ISpecializedConverter<RealmInteger<long>, RealmInteger<byte>>
        {
            public RealmInteger<byte> Convert(RealmInteger<long> value) => (byte)value;
        }

        public class RealmIntegerLongRealmIntegerShortConverter : ISpecializedConverter<RealmInteger<long>, RealmInteger<short>>
        {
            public RealmInteger<short> Convert(RealmInteger<long> value) => (short)value;
        }

        public class RealmIntegerLongRealmIntegerIntConverter : ISpecializedConverter<RealmInteger<long>, RealmInteger<int>>
        {
            public RealmInteger<int> Convert(RealmInteger<long> value) => (int)value;
        }

        public class RealmIntegerLongFloatConverter : ISpecializedConverter<RealmInteger<long>, float>
        {
            public float Convert(RealmInteger<long> value) => value;
        }

        public class RealmIntegerLongDoubleConverter : ISpecializedConverter<RealmInteger<long>, double>
        {
            public double Convert(RealmInteger<long> value) => value;
        }

        public class RealmIntegerLongDecimalConverter : ISpecializedConverter<RealmInteger<long>, decimal>
        {
            public decimal Convert(RealmInteger<long> value) => value;
        }

        public class RealmIntegerLongDecimal128Converter : ISpecializedConverter<RealmInteger<long>, Decimal128>
        {
            public Decimal128 Convert(RealmInteger<long> value) => (long)value;
        }

        #endregion Integral Converters

        #region Floating Point Converters

        public class FloatNullableFloatConverter : ISpecializedConverter<float, float?>
        {
            public float? Convert(float value) => value;
        }

        public class FloatNullableDoubleConverter : ISpecializedConverter<float, double?>
        {
            public double? Convert(float value) => value;
        }

        public class FloatNullableDecimalConverter : ISpecializedConverter<float, decimal?>
        {
            public decimal? Convert(float value) => (decimal)value;
        }

        public class FloatNullableDecimal128Converter : ISpecializedConverter<float, Decimal128?>
        {
            public Decimal128? Convert(float value) => (Decimal128)value;
        }

        public class DoubleNullableFloatConverter : ISpecializedConverter<double, float?>
        {
            public float? Convert(double value) => (float)value;
        }

        public class DoubleNullableDoubleConverter : ISpecializedConverter<double, double?>
        {
            public double? Convert(double value) => value;
        }

        public class DoubleNullableDecimalConverter : ISpecializedConverter<double, decimal?>
        {
            public decimal? Convert(double value) => (decimal)value;
        }

        public class DoubleNullableDecimal128Converter : ISpecializedConverter<double, Decimal128?>
        {
            public Decimal128? Convert(double value) => (Decimal128)value;
        }

        public class DecimalNullableFloatConverter : ISpecializedConverter<decimal, float?>
        {
            public float? Convert(decimal value) => (float)value;
        }

        public class DecimalNullableDoubleConverter : ISpecializedConverter<decimal, double?>
        {
            public double? Convert(decimal value) => (double)value;
        }

        public class DecimalNullableDecimalConverter : ISpecializedConverter<decimal, decimal?>
        {
            public decimal? Convert(decimal value) => value;
        }

        public class DecimalNullableDecimal128Converter : ISpecializedConverter<decimal, Decimal128?>
        {
            public Decimal128? Convert(decimal value) => value;
        }

        public class Decimal128NullableFloatConverter : ISpecializedConverter<Decimal128, float?>
        {
            public float? Convert(Decimal128 value) => (float)value;
        }

        public class Decimal128NullableDoubleConverter : ISpecializedConverter<Decimal128, double?>
        {
            public double? Convert(Decimal128 value) => (double)value;
        }

        public class Decimal128NullableDecimalConverter : ISpecializedConverter<Decimal128, decimal?>
        {
            public decimal? Convert(Decimal128 value) => (decimal)value;
        }

        public class Decimal128NullableDecimal128Converter : ISpecializedConverter<Decimal128, Decimal128?>
        {
            public Decimal128? Convert(Decimal128 value) => value;
        }

        public class FloatDoubleConverter : ISpecializedConverter<float, double>
        {
            public double Convert(float value) => value;
        }

        public class FloatDecimalConverter : ISpecializedConverter<float, decimal>
        {
            public decimal Convert(float value) => (decimal)value;
        }

        public class FloatDecimal128Converter : ISpecializedConverter<float, Decimal128>
        {
            public Decimal128 Convert(float value) => (Decimal128)value;
        }

        public class DoubleFloatConverter : ISpecializedConverter<double, float>
        {
            public float Convert(double value) => (float)value;
        }

        public class DoubleDecimalConverter : ISpecializedConverter<double, decimal>
        {
            public decimal Convert(double value) => (decimal)value;
        }

        public class DoubleDecimal128Converter : ISpecializedConverter<double, Decimal128>
        {
            public Decimal128 Convert(double value) => (Decimal128)value;
        }

        public class DecimalFloatConverter : ISpecializedConverter<decimal, float>
        {
            public float Convert(decimal value) => (float)value;
        }

        public class DecimalDoubleConverter : ISpecializedConverter<decimal, double>
        {
            public double Convert(decimal value) => (double)value;
        }

        public class DecimalDecimal128Converter : ISpecializedConverter<decimal, Decimal128>
        {
            public Decimal128 Convert(decimal value) => value;
        }

        public class Decimal128FloatConverter : ISpecializedConverter<Decimal128, float>
        {
            public float Convert(Decimal128 value) => (float)value;
        }

        public class Decimal128DoubleConverter : ISpecializedConverter<Decimal128, double>
        {
            public double Convert(Decimal128 value) => (double)value;
        }

        public class Decimal128DecimalConverter : ISpecializedConverter<Decimal128, decimal>
        {
            public decimal Convert(Decimal128 value) => (decimal)value;
        }

        #endregion Floating Point Converters
    }
}
