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
        private static readonly IDictionary<(Type, Type), IConverter> _valueConverters = new Dictionary<(Type, Type), IConverter>
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
            [(typeof(RealmValue), typeof(IRealmObjectBase))] = new RealmValueIRealmObjectBaseConverter(),
            [(typeof(IRealmObjectBase), typeof(RealmValue))] = new IRealmObjectBaseRealmValueConverter(),
        };

        /// <summary>
        /// Efficiently convert a <typeparamref name="TFrom"/> value to <typeparamref name="TResult"/>.
        /// It is intended to be used when we want to convert to or from a generic where we don't
        /// know the exact type, but we know that a conversion exists.
        /// </summary>
        /// <remarks>
        /// In synthetic benchmarks it performs about two orders of magnitude faster than Convert.ChangeType.
        /// It is about 4 times slower than a direct cast when the types are known, but about an order of
        /// magnitude faster than a cast that involves boxing to object.
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
            if (value is TResult result)
            {
                return result;
            }

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
                    return Convert<RealmValue, TResult>(RealmValue.Null);
                }

                /* This is another special case where `value` is inheritable from IRealmObjectBase. There's
                 * no direct conversion from T to RealmValue, but there's conversion if we go through IRealmObjectBase.
                 */
                if (value is IRealmObjectBase irobj)
                {
                    return Convert<RealmValue, TResult>(RealmValue.Object(irobj));
                }
            }

            return GenericOperator<TFrom, TResult>.Convert(value);
        }

        /// <summary>
        /// Converts an object to <typeparamref name="TResult"/>. It is intended to be used instead of Convert.ChangeType
        /// for database types. It is less efficient than <see cref="Convert{TFrom, TResult}(TFrom)"/> so if both the source
        /// and the target types are known, use the concrete conversion.
        /// </summary>
        /// <typeparam name="TResult">The type to which <paramref name="value"/> will be converted.</typeparam>
        /// <param name="value">The value to convert to <typeparamref name="TResult"/>.</param>
        /// <returns>The value of <paramref name="value"/> represented as <typeparamref name="TResult"/>.</returns>
        public static TResult Convert<TResult>(object value)
        {
            if (value is TResult result)
            {
                return result;
            }

            var targetType = typeof(TResult);
            if (targetType == typeof(RealmValue))
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
                    return Convert<RealmValue, TResult>(RealmValue.Null);
                }

                /* This is another special case where `value` is inheritable from RealmObjectBase. There's
                 * no direct conversion from T to RealmValue, but there's conversion if we go through RealmObjectBase.
                 */
                if (value is RealmObjectBase robj)
                {
                    return Convert<RealmValue, TResult>(robj);
                }
            }

            if (value is null)
            {
                return default(TResult) == null ? default(TResult) : throw new InvalidCastException($"Can't convert from null to {targetType.FullName} because the target type is not nullable.");
            }

            var sourceType = value.GetType();

            if (_valueConverters.TryGetValue((sourceType, targetType), out var converter))
            {
                return ((IGenericConverter<TResult>)converter).Convert(value);
            }

            if (value is IConvertible)
            {
                return (TResult)System.Convert.ChangeType(value, targetType);
            }

            throw new InvalidCastException($"No conversion exists from {sourceType.FullName} to {targetType.FullName}");
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
        /// An interface representing converter that can convert from <see cref="SourceType"/> to
        /// <typeparamref name="TTarget"/>.
        /// </summary>
        /// <typeparam name="TTarget">The type to which <see cref="SourceType"/> will be converted.</typeparam>
        private interface IGenericConverter<TTarget> : IConverter
        {
            Type SourceType { get; }

            TTarget Convert(object obj);
        }

        /// <summary>
        /// Interface representing a concrete converter from <typeparamref name="TSource"/>
        /// to <typeparamref name="TTarget"/>. For most types there will be exactly one concrete
        /// implementation, but there may be cases, such as <see cref="InheritanceConverter{TSource, TTarget}"/>
        /// where a single converter type can handle multiple source/target types.
        /// </summary>
        /// <typeparam name="TSource">The type from which to convert.</typeparam>
        /// <typeparam name="TTarget">The type to which <typeparamref name="TSource"/> will be converted.</typeparam>
        private interface ISpecializedConverter<TSource, TTarget> : IGenericConverter<TTarget>
        {
            TTarget Convert(TSource source);
        }

        private abstract class SpecializedConverterBase<TSource, TTarget> : ISpecializedConverter<TSource, TTarget>
        {
            public Type SourceType { get; } = typeof(TSource);

            public abstract TTarget Convert(TSource source);

            public virtual TTarget Convert(object obj) => Convert((TSource)obj);
        }

        /// <summary>
        /// A converter that will throw whenever <see cref="ISpecializedConverter{TSource, TTarget}.Convert(TSource)"/>
        /// is called. This is used to handle cases where there is no conversion from <typeparamref name="TSource"/> to
        /// <typeparamref name="TTarget"/>.
        /// </summary>
        /// <typeparam name="TSource">The type from which to convert.</typeparam>
        /// <typeparam name="TTarget">The type to which <typeparamref name="TSource"/> will be converted.</typeparam>
        private class ThrowingConverter<TSource, TTarget> : SpecializedConverterBase<TSource, TTarget>
        {
            public override TTarget Convert(TSource source) => throw new InvalidCastException($"No conversion exists from {typeof(TSource).FullName} to {typeof(TTarget).FullName}");
        }

        /// <summary>
        /// A converter that converts from the type to itself. There are cases where we don't know what the source or
        /// the target type is, so we need to convert, just in case.
        /// </summary>
        /// <typeparam name="T">The type of both the source and the target.</typeparam>
        private class UnaryConverter<T> : SpecializedConverterBase<T, T>
        {
            public override T Convert(T source) => source;
        }

        /// <summary>
        /// A converter that converts from a type to its base type. This is typically needed
        /// when we want to cast from a RealmObject inheritor to RealmObjectBase or when we
        /// get passed <see cref="object"/>.
        /// </summary>
        /// <typeparam name="TSource">The type from which to convert.</typeparam>
        /// <typeparam name="TTarget">The type to which <typeparamref name="TSource"/> will be converted.</typeparam>
        private class InheritanceConverter<TSource, TTarget> : SpecializedConverterBase<TSource, TTarget>
        {
            public override TTarget Convert(TSource source) => source is TTarget obj ? obj : throw new InvalidCastException($"No conversion exists from {typeof(TSource).FullName} to {typeof(TTarget).FullName}");

            public override TTarget Convert(object source) => source is TTarget obj ? obj : throw new InvalidCastException($"No conversion exists from {source?.GetType().FullName} to {typeof(TTarget).FullName}");
        }

        #region ToRealmValue Converters

        private class CharRealmValueConverter : SpecializedConverterBase<char, RealmValue>
        {
            public override RealmValue Convert(char value) => value;
        }

        private class ByteRealmValueConverter : SpecializedConverterBase<byte, RealmValue>
        {
            public override RealmValue Convert(byte value) => value;
        }

        private class ShortRealmValueConverter : SpecializedConverterBase<short, RealmValue>
        {
            public override RealmValue Convert(short value) => value;
        }

        private class IntRealmValueConverter : SpecializedConverterBase<int, RealmValue>
        {
            public override RealmValue Convert(int value) => value;
        }

        private class LongRealmValueConverter : SpecializedConverterBase<long, RealmValue>
        {
            public override RealmValue Convert(long value) => value;
        }

        private class FloatRealmValueConverter : SpecializedConverterBase<float, RealmValue>
        {
            public override RealmValue Convert(float value) => value;
        }

        private class DoubleRealmValueConverter : SpecializedConverterBase<double, RealmValue>
        {
            public override RealmValue Convert(double value) => value;
        }

        private class BoolRealmValueConverter : SpecializedConverterBase<bool, RealmValue>
        {
            public override RealmValue Convert(bool value) => value;
        }

        private class DateTimeOffsetRealmValueConverter : SpecializedConverterBase<DateTimeOffset, RealmValue>
        {
            public override RealmValue Convert(DateTimeOffset value) => value;
        }

        private class DecimalRealmValueConverter : SpecializedConverterBase<decimal, RealmValue>
        {
            public override RealmValue Convert(decimal value) => value;
        }

        private class Decimal128RealmValueConverter : SpecializedConverterBase<Decimal128, RealmValue>
        {
            public override RealmValue Convert(Decimal128 value) => value;
        }

        private class ObjectIdRealmValueConverter : SpecializedConverterBase<ObjectId, RealmValue>
        {
            public override RealmValue Convert(ObjectId value) => value;
        }

        private class GuidRealmValueConverter : SpecializedConverterBase<Guid, RealmValue>
        {
            public override RealmValue Convert(Guid value) => value;
        }

        private class NullableCharRealmValueConverter : SpecializedConverterBase<char?, RealmValue>
        {
            public override RealmValue Convert(char? value) => value;
        }

        private class NullableByteRealmValueConverter : SpecializedConverterBase<byte?, RealmValue>
        {
            public override RealmValue Convert(byte? value) => value;
        }

        private class NullableShortRealmValueConverter : SpecializedConverterBase<short?, RealmValue>
        {
            public override RealmValue Convert(short? value) => value;
        }

        private class NullableIntRealmValueConverter : SpecializedConverterBase<int?, RealmValue>
        {
            public override RealmValue Convert(int? value) => value;
        }

        private class NullableLongRealmValueConverter : SpecializedConverterBase<long?, RealmValue>
        {
            public override RealmValue Convert(long? value) => value;
        }

        private class NullableFloatRealmValueConverter : SpecializedConverterBase<float?, RealmValue>
        {
            public override RealmValue Convert(float? value) => value;
        }

        private class NullableDoubleRealmValueConverter : SpecializedConverterBase<double?, RealmValue>
        {
            public override RealmValue Convert(double? value) => value;
        }

        private class NullableBoolRealmValueConverter : SpecializedConverterBase<bool?, RealmValue>
        {
            public override RealmValue Convert(bool? value) => value;
        }

        private class NullableDateTimeOffsetRealmValueConverter : SpecializedConverterBase<DateTimeOffset?, RealmValue>
        {
            public override RealmValue Convert(DateTimeOffset? value) => value;
        }

        private class NullableDecimalRealmValueConverter : SpecializedConverterBase<decimal?, RealmValue>
        {
            public override RealmValue Convert(decimal? value) => value;
        }

        private class NullableDecimal128RealmValueConverter : SpecializedConverterBase<Decimal128?, RealmValue>
        {
            public override RealmValue Convert(Decimal128? value) => value;
        }

        private class NullableObjectIdRealmValueConverter : SpecializedConverterBase<ObjectId?, RealmValue>
        {
            public override RealmValue Convert(ObjectId? value) => value;
        }

        private class NullableGuidRealmValueConverter : SpecializedConverterBase<Guid?, RealmValue>
        {
            public override RealmValue Convert(Guid? value) => value;
        }

        private class RealmIntegerByteRealmValueConverter : SpecializedConverterBase<RealmInteger<byte>, RealmValue>
        {
            public override RealmValue Convert(RealmInteger<byte> value) => value;
        }

        private class RealmIntegerShortRealmValueConverter : SpecializedConverterBase<RealmInteger<short>, RealmValue>
        {
            public override RealmValue Convert(RealmInteger<short> value) => value;
        }

        private class RealmIntegerIntRealmValueConverter : SpecializedConverterBase<RealmInteger<int>, RealmValue>
        {
            public override RealmValue Convert(RealmInteger<int> value) => value;
        }

        private class RealmIntegerLongRealmValueConverter : SpecializedConverterBase<RealmInteger<long>, RealmValue>
        {
            public override RealmValue Convert(RealmInteger<long> value) => value;
        }

        private class NullableRealmIntegerByteRealmValueConverter : SpecializedConverterBase<RealmInteger<byte>?, RealmValue>
        {
            public override RealmValue Convert(RealmInteger<byte>? value) => value;
        }

        private class NullableRealmIntegerShortRealmValueConverter : SpecializedConverterBase<RealmInteger<short>?, RealmValue>
        {
            public override RealmValue Convert(RealmInteger<short>? value) => value;
        }

        private class NullableRealmIntegerIntRealmValueConverter : SpecializedConverterBase<RealmInteger<int>?, RealmValue>
        {
            public override RealmValue Convert(RealmInteger<int>? value) => value;
        }

        private class NullableRealmIntegerLongRealmValueConverter : SpecializedConverterBase<RealmInteger<long>?, RealmValue>
        {
            public override RealmValue Convert(RealmInteger<long>? value) => value;
        }

        private class ByteArrayRealmValueConverter : SpecializedConverterBase<byte[], RealmValue>
        {
            public override RealmValue Convert(byte[] value) => value;
        }

        private class StringRealmValueConverter : SpecializedConverterBase<string, RealmValue>
        {
            public override RealmValue Convert(string value) => value;
        }

        private class RealmObjectBaseRealmValueConverter : SpecializedConverterBase<RealmObjectBase, RealmValue>
        {
            public override RealmValue Convert(RealmObjectBase value) => value;
        }

        private class IRealmObjectBaseRealmValueConverter : SpecializedConverterBase<IRealmObjectBase, RealmValue>
        {
            public override RealmValue Convert(IRealmObjectBase value) => RealmValue.Object(value);
        }
        #endregion ToRealmValue Converters

        #region FromRealmValue Converters

        private class RealmValueCharConverter : SpecializedConverterBase<RealmValue, char>
        {
            public override char Convert(RealmValue value) => (char)value;
        }

        private class RealmValueByteConverter : SpecializedConverterBase<RealmValue, byte>
        {
            public override byte Convert(RealmValue value) => (byte)value;
        }

        private class RealmValueShortConverter : SpecializedConverterBase<RealmValue, short>
        {
            public override short Convert(RealmValue value) => (short)value;
        }

        private class RealmValueIntConverter : SpecializedConverterBase<RealmValue, int>
        {
            public override int Convert(RealmValue value) => (int)value;
        }

        private class RealmValueLongConverter : SpecializedConverterBase<RealmValue, long>
        {
            public override long Convert(RealmValue value) => (long)value;
        }

        private class RealmValueFloatConverter : SpecializedConverterBase<RealmValue, float>
        {
            public override float Convert(RealmValue value) => (float)value;
        }

        private class RealmValueDoubleConverter : SpecializedConverterBase<RealmValue, double>
        {
            public override double Convert(RealmValue value) => (double)value;
        }

        private class RealmValueBoolConverter : SpecializedConverterBase<RealmValue, bool>
        {
            public override bool Convert(RealmValue value) => (bool)value;
        }

        private class RealmValueDateTimeOffsetConverter : SpecializedConverterBase<RealmValue, DateTimeOffset>
        {
            public override DateTimeOffset Convert(RealmValue value) => (DateTimeOffset)value;
        }

        private class RealmValueDecimalConverter : SpecializedConverterBase<RealmValue, decimal>
        {
            public override decimal Convert(RealmValue value) => (decimal)value;
        }

        private class RealmValueDecimal128Converter : SpecializedConverterBase<RealmValue, Decimal128>
        {
            public override Decimal128 Convert(RealmValue value) => (Decimal128)value;
        }

        private class RealmValueObjectIdConverter : SpecializedConverterBase<RealmValue, ObjectId>
        {
            public override ObjectId Convert(RealmValue value) => (ObjectId)value;
        }

        private class RealmValueGuidConverter : SpecializedConverterBase<RealmValue, Guid>
        {
            public override Guid Convert(RealmValue value) => (Guid)value;
        }

        private class RealmValueNullableCharConverter : SpecializedConverterBase<RealmValue, char?>
        {
            public override char? Convert(RealmValue value) => (char?)value;
        }

        private class RealmValueNullableByteConverter : SpecializedConverterBase<RealmValue, byte?>
        {
            public override byte? Convert(RealmValue value) => (byte?)value;
        }

        private class RealmValueNullableShortConverter : SpecializedConverterBase<RealmValue, short?>
        {
            public override short? Convert(RealmValue value) => (short?)value;
        }

        private class RealmValueNullableIntConverter : SpecializedConverterBase<RealmValue, int?>
        {
            public override int? Convert(RealmValue value) => (int?)value;
        }

        private class RealmValueNullableLongConverter : SpecializedConverterBase<RealmValue, long?>
        {
            public override long? Convert(RealmValue value) => (long?)value;
        }

        private class RealmValueNullableFloatConverter : SpecializedConverterBase<RealmValue, float?>
        {
            public override float? Convert(RealmValue value) => (float?)value;
        }

        private class RealmValueNullableDoubleConverter : SpecializedConverterBase<RealmValue, double?>
        {
            public override double? Convert(RealmValue value) => (double?)value;
        }

        private class RealmValueNullableBoolConverter : SpecializedConverterBase<RealmValue, bool?>
        {
            public override bool? Convert(RealmValue value) => (bool?)value;
        }

        private class RealmValueNullableDateTimeOffsetConverter : SpecializedConverterBase<RealmValue, DateTimeOffset?>
        {
            public override DateTimeOffset? Convert(RealmValue value) => (DateTimeOffset?)value;
        }

        private class RealmValueNullableDecimalConverter : SpecializedConverterBase<RealmValue, decimal?>
        {
            public override decimal? Convert(RealmValue value) => (decimal?)value;
        }

        private class RealmValueNullableDecimal128Converter : SpecializedConverterBase<RealmValue, Decimal128?>
        {
            public override Decimal128? Convert(RealmValue value) => (Decimal128?)value;
        }

        private class RealmValueNullableObjectIdConverter : SpecializedConverterBase<RealmValue, ObjectId?>
        {
            public override ObjectId? Convert(RealmValue value) => (ObjectId?)value;
        }

        private class RealmValueNullableGuidConverter : SpecializedConverterBase<RealmValue, Guid?>
        {
            public override Guid? Convert(RealmValue value) => (Guid?)value;
        }

        private class RealmValueRealmIntegerByteConverter : SpecializedConverterBase<RealmValue, RealmInteger<byte>>
        {
            public override RealmInteger<byte> Convert(RealmValue value) => (RealmInteger<byte>)value;
        }

        private class RealmValueRealmIntegerShortConverter : SpecializedConverterBase<RealmValue, RealmInteger<short>>
        {
            public override RealmInteger<short> Convert(RealmValue value) => (RealmInteger<short>)value;
        }

        private class RealmValueRealmIntegerIntConverter : SpecializedConverterBase<RealmValue, RealmInteger<int>>
        {
            public override RealmInteger<int> Convert(RealmValue value) => (RealmInteger<int>)value;
        }

        private class RealmValueRealmIntegerLongConverter : SpecializedConverterBase<RealmValue, RealmInteger<long>>
        {
            public override RealmInteger<long> Convert(RealmValue value) => (RealmInteger<long>)value;
        }

        private class RealmValueNullableRealmIntegerByteConverter : SpecializedConverterBase<RealmValue, RealmInteger<byte>?>
        {
            public override RealmInteger<byte>? Convert(RealmValue value) => (RealmInteger<byte>?)value;
        }

        private class RealmValueNullableRealmIntegerShortConverter : SpecializedConverterBase<RealmValue, RealmInteger<short>?>
        {
            public override RealmInteger<short>? Convert(RealmValue value) => (RealmInteger<short>?)value;
        }

        private class RealmValueNullableRealmIntegerIntConverter : SpecializedConverterBase<RealmValue, RealmInteger<int>?>
        {
            public override RealmInteger<int>? Convert(RealmValue value) => (RealmInteger<int>?)value;
        }

        private class RealmValueNullableRealmIntegerLongConverter : SpecializedConverterBase<RealmValue, RealmInteger<long>?>
        {
            public override RealmInteger<long>? Convert(RealmValue value) => (RealmInteger<long>?)value;
        }

        private class RealmValueByteArrayConverter : SpecializedConverterBase<RealmValue, byte[]>
        {
            public override byte[] Convert(RealmValue value) => (byte[])value;
        }

        private class RealmValueStringConverter : SpecializedConverterBase<RealmValue, string>
        {
            public override string Convert(RealmValue value) => (string)value;
        }

        private class RealmValueRealmObjectBaseConverter : SpecializedConverterBase<RealmValue, RealmObjectBase>
        {
            public override RealmObjectBase Convert(RealmValue value) => (RealmObjectBase)value;
        }

        private class RealmValueIRealmObjectBaseConverter : SpecializedConverterBase<RealmValue, IRealmObjectBase>
        {
            public override IRealmObjectBase Convert(RealmValue value) => value.AsIRealmObject();
        }
        #endregion FromRealmValue Converters

        #region Integral Converters

        private class CharNullableCharConverter : SpecializedConverterBase<char, char?>
        {
            public override char? Convert(char value) => (char)value;
        }

        private class CharNullableByteConverter : SpecializedConverterBase<char, byte?>
        {
            public override byte? Convert(char value) => (byte)value;
        }

        private class CharNullableShortConverter : SpecializedConverterBase<char, short?>
        {
            public override short? Convert(char value) => (short)value;
        }

        private class CharNullableIntConverter : SpecializedConverterBase<char, int?>
        {
            public override int? Convert(char value) => value;
        }

        private class CharNullableLongConverter : SpecializedConverterBase<char, long?>
        {
            public override long? Convert(char value) => value;
        }

        private class CharNullableRealmIntegerByteConverter : SpecializedConverterBase<char, RealmInteger<byte>?>
        {
            public override RealmInteger<byte>? Convert(char value) => (byte)value;
        }

        private class CharNullableRealmIntegerShortConverter : SpecializedConverterBase<char, RealmInteger<short>?>
        {
            public override RealmInteger<short>? Convert(char value) => (short)value;
        }

        private class CharNullableRealmIntegerIntConverter : SpecializedConverterBase<char, RealmInteger<int>?>
        {
            public override RealmInteger<int>? Convert(char value) => value;
        }

        private class CharNullableRealmIntegerLongConverter : SpecializedConverterBase<char, RealmInteger<long>?>
        {
            public override RealmInteger<long>? Convert(char value) => value;
        }

        private class CharNullableFloatConverter : SpecializedConverterBase<char, float?>
        {
            public override float? Convert(char value) => value;
        }

        private class CharNullableDoubleConverter : SpecializedConverterBase<char, double?>
        {
            public override double? Convert(char value) => value;
        }

        private class CharNullableDecimalConverter : SpecializedConverterBase<char, decimal?>
        {
            public override decimal? Convert(char value) => value;
        }

        private class CharNullableDecimal128Converter : SpecializedConverterBase<char, Decimal128?>
        {
            public override Decimal128? Convert(char value) => value;
        }

        private class ByteNullableCharConverter : SpecializedConverterBase<byte, char?>
        {
            public override char? Convert(byte value) => (char)value;
        }

        private class ByteNullableByteConverter : SpecializedConverterBase<byte, byte?>
        {
            public override byte? Convert(byte value) => value;
        }

        private class ByteNullableShortConverter : SpecializedConverterBase<byte, short?>
        {
            public override short? Convert(byte value) => value;
        }

        private class ByteNullableIntConverter : SpecializedConverterBase<byte, int?>
        {
            public override int? Convert(byte value) => value;
        }

        private class ByteNullableLongConverter : SpecializedConverterBase<byte, long?>
        {
            public override long? Convert(byte value) => value;
        }

        private class ByteNullableRealmIntegerByteConverter : SpecializedConverterBase<byte, RealmInteger<byte>?>
        {
            public override RealmInteger<byte>? Convert(byte value) => value;
        }

        private class ByteNullableRealmIntegerShortConverter : SpecializedConverterBase<byte, RealmInteger<short>?>
        {
            public override RealmInteger<short>? Convert(byte value) => value;
        }

        private class ByteNullableRealmIntegerIntConverter : SpecializedConverterBase<byte, RealmInteger<int>?>
        {
            public override RealmInteger<int>? Convert(byte value) => value;
        }

        private class ByteNullableRealmIntegerLongConverter : SpecializedConverterBase<byte, RealmInteger<long>?>
        {
            public override RealmInteger<long>? Convert(byte value) => value;
        }

        private class ByteNullableFloatConverter : SpecializedConverterBase<byte, float?>
        {
            public override float? Convert(byte value) => value;
        }

        private class ByteNullableDoubleConverter : SpecializedConverterBase<byte, double?>
        {
            public override double? Convert(byte value) => value;
        }

        private class ByteNullableDecimalConverter : SpecializedConverterBase<byte, decimal?>
        {
            public override decimal? Convert(byte value) => value;
        }

        private class ByteNullableDecimal128Converter : SpecializedConverterBase<byte, Decimal128?>
        {
            public override Decimal128? Convert(byte value) => value;
        }

        private class ShortNullableCharConverter : SpecializedConverterBase<short, char?>
        {
            public override char? Convert(short value) => (char)value;
        }

        private class ShortNullableByteConverter : SpecializedConverterBase<short, byte?>
        {
            public override byte? Convert(short value) => (byte)value;
        }

        private class ShortNullableShortConverter : SpecializedConverterBase<short, short?>
        {
            public override short? Convert(short value) => value;
        }

        private class ShortNullableIntConverter : SpecializedConverterBase<short, int?>
        {
            public override int? Convert(short value) => value;
        }

        private class ShortNullableLongConverter : SpecializedConverterBase<short, long?>
        {
            public override long? Convert(short value) => value;
        }

        private class ShortNullableRealmIntegerByteConverter : SpecializedConverterBase<short, RealmInteger<byte>?>
        {
            public override RealmInteger<byte>? Convert(short value) => (byte)value;
        }

        private class ShortNullableRealmIntegerShortConverter : SpecializedConverterBase<short, RealmInteger<short>?>
        {
            public override RealmInteger<short>? Convert(short value) => value;
        }

        private class ShortNullableRealmIntegerIntConverter : SpecializedConverterBase<short, RealmInteger<int>?>
        {
            public override RealmInteger<int>? Convert(short value) => value;
        }

        private class ShortNullableRealmIntegerLongConverter : SpecializedConverterBase<short, RealmInteger<long>?>
        {
            public override RealmInteger<long>? Convert(short value) => value;
        }

        private class ShortNullableFloatConverter : SpecializedConverterBase<short, float?>
        {
            public override float? Convert(short value) => value;
        }

        private class ShortNullableDoubleConverter : SpecializedConverterBase<short, double?>
        {
            public override double? Convert(short value) => value;
        }

        private class ShortNullableDecimalConverter : SpecializedConverterBase<short, decimal?>
        {
            public override decimal? Convert(short value) => value;
        }

        private class ShortNullableDecimal128Converter : SpecializedConverterBase<short, Decimal128?>
        {
            public override Decimal128? Convert(short value) => value;
        }

        private class IntNullableCharConverter : SpecializedConverterBase<int, char?>
        {
            public override char? Convert(int value) => (char)value;
        }

        private class IntNullableByteConverter : SpecializedConverterBase<int, byte?>
        {
            public override byte? Convert(int value) => (byte)value;
        }

        private class IntNullableShortConverter : SpecializedConverterBase<int, short?>
        {
            public override short? Convert(int value) => (short)value;
        }

        private class IntNullableIntConverter : SpecializedConverterBase<int, int?>
        {
            public override int? Convert(int value) => value;
        }

        private class IntNullableLongConverter : SpecializedConverterBase<int, long?>
        {
            public override long? Convert(int value) => value;
        }

        private class IntNullableRealmIntegerByteConverter : SpecializedConverterBase<int, RealmInteger<byte>?>
        {
            public override RealmInteger<byte>? Convert(int value) => (byte)value;
        }

        private class IntNullableRealmIntegerShortConverter : SpecializedConverterBase<int, RealmInteger<short>?>
        {
            public override RealmInteger<short>? Convert(int value) => (short)value;
        }

        private class IntNullableRealmIntegerIntConverter : SpecializedConverterBase<int, RealmInteger<int>?>
        {
            public override RealmInteger<int>? Convert(int value) => value;
        }

        private class IntNullableRealmIntegerLongConverter : SpecializedConverterBase<int, RealmInteger<long>?>
        {
            public override RealmInteger<long>? Convert(int value) => value;
        }

        private class IntNullableFloatConverter : SpecializedConverterBase<int, float?>
        {
            public override float? Convert(int value) => value;
        }

        private class IntNullableDoubleConverter : SpecializedConverterBase<int, double?>
        {
            public override double? Convert(int value) => value;
        }

        private class IntNullableDecimalConverter : SpecializedConverterBase<int, decimal?>
        {
            public override decimal? Convert(int value) => value;
        }

        private class IntNullableDecimal128Converter : SpecializedConverterBase<int, Decimal128?>
        {
            public override Decimal128? Convert(int value) => value;
        }

        private class LongNullableCharConverter : SpecializedConverterBase<long, char?>
        {
            public override char? Convert(long value) => (char)value;
        }

        private class LongNullableByteConverter : SpecializedConverterBase<long, byte?>
        {
            public override byte? Convert(long value) => (byte)value;
        }

        private class LongNullableShortConverter : SpecializedConverterBase<long, short?>
        {
            public override short? Convert(long value) => (short)value;
        }

        private class LongNullableIntConverter : SpecializedConverterBase<long, int?>
        {
            public override int? Convert(long value) => (int)value;
        }

        private class LongNullableLongConverter : SpecializedConverterBase<long, long?>
        {
            public override long? Convert(long value) => value;
        }

        private class LongNullableRealmIntegerByteConverter : SpecializedConverterBase<long, RealmInteger<byte>?>
        {
            public override RealmInteger<byte>? Convert(long value) => (byte)value;
        }

        private class LongNullableRealmIntegerShortConverter : SpecializedConverterBase<long, RealmInteger<short>?>
        {
            public override RealmInteger<short>? Convert(long value) => (short)value;
        }

        private class LongNullableRealmIntegerIntConverter : SpecializedConverterBase<long, RealmInteger<int>?>
        {
            public override RealmInteger<int>? Convert(long value) => (int)value;
        }

        private class LongNullableRealmIntegerLongConverter : SpecializedConverterBase<long, RealmInteger<long>?>
        {
            public override RealmInteger<long>? Convert(long value) => value;
        }

        private class LongNullableFloatConverter : SpecializedConverterBase<long, float?>
        {
            public override float? Convert(long value) => value;
        }

        private class LongNullableDoubleConverter : SpecializedConverterBase<long, double?>
        {
            public override double? Convert(long value) => value;
        }

        private class LongNullableDecimalConverter : SpecializedConverterBase<long, decimal?>
        {
            public override decimal? Convert(long value) => value;
        }

        private class LongNullableDecimal128Converter : SpecializedConverterBase<long, Decimal128?>
        {
            public override Decimal128? Convert(long value) => value;
        }

        private class RealmIntegerByteNullableCharConverter : SpecializedConverterBase<RealmInteger<byte>, char?>
        {
            public override char? Convert(RealmInteger<byte> value) => (char)(byte)value;
        }

        private class RealmIntegerByteNullableByteConverter : SpecializedConverterBase<RealmInteger<byte>, byte?>
        {
            public override byte? Convert(RealmInteger<byte> value) => value;
        }

        private class RealmIntegerByteNullableShortConverter : SpecializedConverterBase<RealmInteger<byte>, short?>
        {
            public override short? Convert(RealmInteger<byte> value) => value;
        }

        private class RealmIntegerByteNullableIntConverter : SpecializedConverterBase<RealmInteger<byte>, int?>
        {
            public override int? Convert(RealmInteger<byte> value) => value;
        }

        private class RealmIntegerByteNullableLongConverter : SpecializedConverterBase<RealmInteger<byte>, long?>
        {
            public override long? Convert(RealmInteger<byte> value) => value;
        }

        private class RealmIntegerByteNullableRealmIntegerByteConverter : SpecializedConverterBase<RealmInteger<byte>, RealmInteger<byte>?>
        {
            public override RealmInteger<byte>? Convert(RealmInteger<byte> value) => value;
        }

        private class RealmIntegerByteNullableRealmIntegerShortConverter : SpecializedConverterBase<RealmInteger<byte>, RealmInteger<short>?>
        {
            public override RealmInteger<short>? Convert(RealmInteger<byte> value) => (short)value;
        }

        private class RealmIntegerByteNullableRealmIntegerIntConverter : SpecializedConverterBase<RealmInteger<byte>, RealmInteger<int>?>
        {
            public override RealmInteger<int>? Convert(RealmInteger<byte> value) => (int)value;
        }

        private class RealmIntegerByteNullableRealmIntegerLongConverter : SpecializedConverterBase<RealmInteger<byte>, RealmInteger<long>?>
        {
            public override RealmInteger<long>? Convert(RealmInteger<byte> value) => (long)value;
        }

        private class RealmIntegerByteNullableFloatConverter : SpecializedConverterBase<RealmInteger<byte>, float?>
        {
            public override float? Convert(RealmInteger<byte> value) => value;
        }

        private class RealmIntegerByteNullableDoubleConverter : SpecializedConverterBase<RealmInteger<byte>, double?>
        {
            public override double? Convert(RealmInteger<byte> value) => value;
        }

        private class RealmIntegerByteNullableDecimalConverter : SpecializedConverterBase<RealmInteger<byte>, decimal?>
        {
            public override decimal? Convert(RealmInteger<byte> value) => value;
        }

        private class RealmIntegerByteNullableDecimal128Converter : SpecializedConverterBase<RealmInteger<byte>, Decimal128?>
        {
            public override Decimal128? Convert(RealmInteger<byte> value) => (byte)value;
        }

        private class RealmIntegerShortNullableCharConverter : SpecializedConverterBase<RealmInteger<short>, char?>
        {
            public override char? Convert(RealmInteger<short> value) => (char)(short)value;
        }

        private class RealmIntegerShortNullableByteConverter : SpecializedConverterBase<RealmInteger<short>, byte?>
        {
            public override byte? Convert(RealmInteger<short> value) => (byte)value;
        }

        private class RealmIntegerShortNullableShortConverter : SpecializedConverterBase<RealmInteger<short>, short?>
        {
            public override short? Convert(RealmInteger<short> value) => value;
        }

        private class RealmIntegerShortNullableIntConverter : SpecializedConverterBase<RealmInteger<short>, int?>
        {
            public override int? Convert(RealmInteger<short> value) => value;
        }

        private class RealmIntegerShortNullableLongConverter : SpecializedConverterBase<RealmInteger<short>, long?>
        {
            public override long? Convert(RealmInteger<short> value) => value;
        }

        private class RealmIntegerShortNullableRealmIntegerByteConverter : SpecializedConverterBase<RealmInteger<short>, RealmInteger<byte>?>
        {
            public override RealmInteger<byte>? Convert(RealmInteger<short> value) => (byte)value;
        }

        private class RealmIntegerShortNullableRealmIntegerShortConverter : SpecializedConverterBase<RealmInteger<short>, RealmInteger<short>?>
        {
            public override RealmInteger<short>? Convert(RealmInteger<short> value) => value;
        }

        private class RealmIntegerShortNullableRealmIntegerIntConverter : SpecializedConverterBase<RealmInteger<short>, RealmInteger<int>?>
        {
            public override RealmInteger<int>? Convert(RealmInteger<short> value) => (int)value;
        }

        private class RealmIntegerShortNullableRealmIntegerLongConverter : SpecializedConverterBase<RealmInteger<short>, RealmInteger<long>?>
        {
            public override RealmInteger<long>? Convert(RealmInteger<short> value) => (long)value;
        }

        private class RealmIntegerShortNullableFloatConverter : SpecializedConverterBase<RealmInteger<short>, float?>
        {
            public override float? Convert(RealmInteger<short> value) => value;
        }

        private class RealmIntegerShortNullableDoubleConverter : SpecializedConverterBase<RealmInteger<short>, double?>
        {
            public override double? Convert(RealmInteger<short> value) => value;
        }

        private class RealmIntegerShortNullableDecimalConverter : SpecializedConverterBase<RealmInteger<short>, decimal?>
        {
            public override decimal? Convert(RealmInteger<short> value) => value;
        }

        private class RealmIntegerShortNullableDecimal128Converter : SpecializedConverterBase<RealmInteger<short>, Decimal128?>
        {
            public override Decimal128? Convert(RealmInteger<short> value) => (short)value;
        }

        private class RealmIntegerIntNullableCharConverter : SpecializedConverterBase<RealmInteger<int>, char?>
        {
            public override char? Convert(RealmInteger<int> value) => (char)value;
        }

        private class RealmIntegerIntNullableByteConverter : SpecializedConverterBase<RealmInteger<int>, byte?>
        {
            public override byte? Convert(RealmInteger<int> value) => (byte)value;
        }

        private class RealmIntegerIntNullableShortConverter : SpecializedConverterBase<RealmInteger<int>, short?>
        {
            public override short? Convert(RealmInteger<int> value) => (short)value;
        }

        private class RealmIntegerIntNullableIntConverter : SpecializedConverterBase<RealmInteger<int>, int?>
        {
            public override int? Convert(RealmInteger<int> value) => value;
        }

        private class RealmIntegerIntNullableLongConverter : SpecializedConverterBase<RealmInteger<int>, long?>
        {
            public override long? Convert(RealmInteger<int> value) => value;
        }

        private class RealmIntegerIntNullableRealmIntegerByteConverter : SpecializedConverterBase<RealmInteger<int>, RealmInteger<byte>?>
        {
            public override RealmInteger<byte>? Convert(RealmInteger<int> value) => (byte)value;
        }

        private class RealmIntegerIntNullableRealmIntegerShortConverter : SpecializedConverterBase<RealmInteger<int>, RealmInteger<short>?>
        {
            public override RealmInteger<short>? Convert(RealmInteger<int> value) => (short)value;
        }

        private class RealmIntegerIntNullableRealmIntegerIntConverter : SpecializedConverterBase<RealmInteger<int>, RealmInteger<int>?>
        {
            public override RealmInteger<int>? Convert(RealmInteger<int> value) => value;
        }

        private class RealmIntegerIntNullableRealmIntegerLongConverter : SpecializedConverterBase<RealmInteger<int>, RealmInteger<long>?>
        {
            public override RealmInteger<long>? Convert(RealmInteger<int> value) => (long)value;
        }

        private class RealmIntegerIntNullableFloatConverter : SpecializedConverterBase<RealmInteger<int>, float?>
        {
            public override float? Convert(RealmInteger<int> value) => value;
        }

        private class RealmIntegerIntNullableDoubleConverter : SpecializedConverterBase<RealmInteger<int>, double?>
        {
            public override double? Convert(RealmInteger<int> value) => value;
        }

        private class RealmIntegerIntNullableDecimalConverter : SpecializedConverterBase<RealmInteger<int>, decimal?>
        {
            public override decimal? Convert(RealmInteger<int> value) => value;
        }

        private class RealmIntegerIntNullableDecimal128Converter : SpecializedConverterBase<RealmInteger<int>, Decimal128?>
        {
            public override Decimal128? Convert(RealmInteger<int> value) => (int)value;
        }

        private class RealmIntegerLongNullableCharConverter : SpecializedConverterBase<RealmInteger<long>, char?>
        {
            public override char? Convert(RealmInteger<long> value) => (char)value;
        }

        private class RealmIntegerLongNullableByteConverter : SpecializedConverterBase<RealmInteger<long>, byte?>
        {
            public override byte? Convert(RealmInteger<long> value) => (byte)value;
        }

        private class RealmIntegerLongNullableShortConverter : SpecializedConverterBase<RealmInteger<long>, short?>
        {
            public override short? Convert(RealmInteger<long> value) => (short)value;
        }

        private class RealmIntegerLongNullableIntConverter : SpecializedConverterBase<RealmInteger<long>, int?>
        {
            public override int? Convert(RealmInteger<long> value) => (int)value;
        }

        private class RealmIntegerLongNullableLongConverter : SpecializedConverterBase<RealmInteger<long>, long?>
        {
            public override long? Convert(RealmInteger<long> value) => value;
        }

        private class RealmIntegerLongNullableRealmIntegerByteConverter : SpecializedConverterBase<RealmInteger<long>, RealmInteger<byte>?>
        {
            public override RealmInteger<byte>? Convert(RealmInteger<long> value) => (byte)value;
        }

        private class RealmIntegerLongNullableRealmIntegerShortConverter : SpecializedConverterBase<RealmInteger<long>, RealmInteger<short>?>
        {
            public override RealmInteger<short>? Convert(RealmInteger<long> value) => (short)value;
        }

        private class RealmIntegerLongNullableRealmIntegerIntConverter : SpecializedConverterBase<RealmInteger<long>, RealmInteger<int>?>
        {
            public override RealmInteger<int>? Convert(RealmInteger<long> value) => (int)value;
        }

        private class RealmIntegerLongNullableRealmIntegerLongConverter : SpecializedConverterBase<RealmInteger<long>, RealmInteger<long>?>
        {
            public override RealmInteger<long>? Convert(RealmInteger<long> value) => value;
        }

        private class RealmIntegerLongNullableFloatConverter : SpecializedConverterBase<RealmInteger<long>, float?>
        {
            public override float? Convert(RealmInteger<long> value) => value;
        }

        private class RealmIntegerLongNullableDoubleConverter : SpecializedConverterBase<RealmInteger<long>, double?>
        {
            public override double? Convert(RealmInteger<long> value) => value;
        }

        private class RealmIntegerLongNullableDecimalConverter : SpecializedConverterBase<RealmInteger<long>, decimal?>
        {
            public override decimal? Convert(RealmInteger<long> value) => value;
        }

        private class RealmIntegerLongNullableDecimal128Converter : SpecializedConverterBase<RealmInteger<long>, Decimal128?>
        {
            public override Decimal128? Convert(RealmInteger<long> value) => (long)value;
        }

        private class CharByteConverter : SpecializedConverterBase<char, byte>
        {
            public override byte Convert(char value) => (byte)value;
        }

        private class CharShortConverter : SpecializedConverterBase<char, short>
        {
            public override short Convert(char value) => (short)value;
        }

        private class CharIntConverter : SpecializedConverterBase<char, int>
        {
            public override int Convert(char value) => value;
        }

        private class CharLongConverter : SpecializedConverterBase<char, long>
        {
            public override long Convert(char value) => value;
        }

        private class CharRealmIntegerByteConverter : SpecializedConverterBase<char, RealmInteger<byte>>
        {
            public override RealmInteger<byte> Convert(char value) => (byte)value;
        }

        private class CharRealmIntegerShortConverter : SpecializedConverterBase<char, RealmInteger<short>>
        {
            public override RealmInteger<short> Convert(char value) => (short)value;
        }

        private class CharRealmIntegerIntConverter : SpecializedConverterBase<char, RealmInteger<int>>
        {
            public override RealmInteger<int> Convert(char value) => value;
        }

        private class CharRealmIntegerLongConverter : SpecializedConverterBase<char, RealmInteger<long>>
        {
            public override RealmInteger<long> Convert(char value) => value;
        }

        private class CharFloatConverter : SpecializedConverterBase<char, float>
        {
            public override float Convert(char value) => value;
        }

        private class CharDoubleConverter : SpecializedConverterBase<char, double>
        {
            public override double Convert(char value) => value;
        }

        private class CharDecimalConverter : SpecializedConverterBase<char, decimal>
        {
            public override decimal Convert(char value) => value;
        }

        private class CharDecimal128Converter : SpecializedConverterBase<char, Decimal128>
        {
            public override Decimal128 Convert(char value) => value;
        }

        private class ByteCharConverter : SpecializedConverterBase<byte, char>
        {
            public override char Convert(byte value) => (char)value;
        }

        private class ByteShortConverter : SpecializedConverterBase<byte, short>
        {
            public override short Convert(byte value) => value;
        }

        private class ByteIntConverter : SpecializedConverterBase<byte, int>
        {
            public override int Convert(byte value) => value;
        }

        private class ByteLongConverter : SpecializedConverterBase<byte, long>
        {
            public override long Convert(byte value) => value;
        }

        private class ByteRealmIntegerByteConverter : SpecializedConverterBase<byte, RealmInteger<byte>>
        {
            public override RealmInteger<byte> Convert(byte value) => value;
        }

        private class ByteRealmIntegerShortConverter : SpecializedConverterBase<byte, RealmInteger<short>>
        {
            public override RealmInteger<short> Convert(byte value) => value;
        }

        private class ByteRealmIntegerIntConverter : SpecializedConverterBase<byte, RealmInteger<int>>
        {
            public override RealmInteger<int> Convert(byte value) => value;
        }

        private class ByteRealmIntegerLongConverter : SpecializedConverterBase<byte, RealmInteger<long>>
        {
            public override RealmInteger<long> Convert(byte value) => value;
        }

        private class ByteFloatConverter : SpecializedConverterBase<byte, float>
        {
            public override float Convert(byte value) => value;
        }

        private class ByteDoubleConverter : SpecializedConverterBase<byte, double>
        {
            public override double Convert(byte value) => value;
        }

        private class ByteDecimalConverter : SpecializedConverterBase<byte, decimal>
        {
            public override decimal Convert(byte value) => value;
        }

        private class ByteDecimal128Converter : SpecializedConverterBase<byte, Decimal128>
        {
            public override Decimal128 Convert(byte value) => value;
        }

        private class ShortCharConverter : SpecializedConverterBase<short, char>
        {
            public override char Convert(short value) => (char)value;
        }

        private class ShortByteConverter : SpecializedConverterBase<short, byte>
        {
            public override byte Convert(short value) => (byte)value;
        }

        private class ShortIntConverter : SpecializedConverterBase<short, int>
        {
            public override int Convert(short value) => value;
        }

        private class ShortLongConverter : SpecializedConverterBase<short, long>
        {
            public override long Convert(short value) => value;
        }

        private class ShortRealmIntegerByteConverter : SpecializedConverterBase<short, RealmInteger<byte>>
        {
            public override RealmInteger<byte> Convert(short value) => (byte)value;
        }

        private class ShortRealmIntegerShortConverter : SpecializedConverterBase<short, RealmInteger<short>>
        {
            public override RealmInteger<short> Convert(short value) => value;
        }

        private class ShortRealmIntegerIntConverter : SpecializedConverterBase<short, RealmInteger<int>>
        {
            public override RealmInteger<int> Convert(short value) => value;
        }

        private class ShortRealmIntegerLongConverter : SpecializedConverterBase<short, RealmInteger<long>>
        {
            public override RealmInteger<long> Convert(short value) => value;
        }

        private class ShortFloatConverter : SpecializedConverterBase<short, float>
        {
            public override float Convert(short value) => value;
        }

        private class ShortDoubleConverter : SpecializedConverterBase<short, double>
        {
            public override double Convert(short value) => value;
        }

        private class ShortDecimalConverter : SpecializedConverterBase<short, decimal>
        {
            public override decimal Convert(short value) => value;
        }

        private class ShortDecimal128Converter : SpecializedConverterBase<short, Decimal128>
        {
            public override Decimal128 Convert(short value) => value;
        }

        private class IntCharConverter : SpecializedConverterBase<int, char>
        {
            public override char Convert(int value) => (char)value;
        }

        private class IntByteConverter : SpecializedConverterBase<int, byte>
        {
            public override byte Convert(int value) => (byte)value;
        }

        private class IntShortConverter : SpecializedConverterBase<int, short>
        {
            public override short Convert(int value) => (short)value;
        }

        private class IntLongConverter : SpecializedConverterBase<int, long>
        {
            public override long Convert(int value) => value;
        }

        private class IntRealmIntegerByteConverter : SpecializedConverterBase<int, RealmInteger<byte>>
        {
            public override RealmInteger<byte> Convert(int value) => (byte)value;
        }

        private class IntRealmIntegerShortConverter : SpecializedConverterBase<int, RealmInteger<short>>
        {
            public override RealmInteger<short> Convert(int value) => (short)value;
        }

        private class IntRealmIntegerIntConverter : SpecializedConverterBase<int, RealmInteger<int>>
        {
            public override RealmInteger<int> Convert(int value) => value;
        }

        private class IntRealmIntegerLongConverter : SpecializedConverterBase<int, RealmInteger<long>>
        {
            public override RealmInteger<long> Convert(int value) => value;
        }

        private class IntFloatConverter : SpecializedConverterBase<int, float>
        {
            public override float Convert(int value) => value;
        }

        private class IntDoubleConverter : SpecializedConverterBase<int, double>
        {
            public override double Convert(int value) => value;
        }

        private class IntDecimalConverter : SpecializedConverterBase<int, decimal>
        {
            public override decimal Convert(int value) => value;
        }

        private class IntDecimal128Converter : SpecializedConverterBase<int, Decimal128>
        {
            public override Decimal128 Convert(int value) => value;
        }

        private class LongCharConverter : SpecializedConverterBase<long, char>
        {
            public override char Convert(long value) => (char)value;
        }

        private class LongByteConverter : SpecializedConverterBase<long, byte>
        {
            public override byte Convert(long value) => (byte)value;
        }

        private class LongShortConverter : SpecializedConverterBase<long, short>
        {
            public override short Convert(long value) => (short)value;
        }

        private class LongIntConverter : SpecializedConverterBase<long, int>
        {
            public override int Convert(long value) => (int)value;
        }

        private class LongRealmIntegerByteConverter : SpecializedConverterBase<long, RealmInteger<byte>>
        {
            public override RealmInteger<byte> Convert(long value) => (byte)value;
        }

        private class LongRealmIntegerShortConverter : SpecializedConverterBase<long, RealmInteger<short>>
        {
            public override RealmInteger<short> Convert(long value) => (short)value;
        }

        private class LongRealmIntegerIntConverter : SpecializedConverterBase<long, RealmInteger<int>>
        {
            public override RealmInteger<int> Convert(long value) => (int)value;
        }

        private class LongRealmIntegerLongConverter : SpecializedConverterBase<long, RealmInteger<long>>
        {
            public override RealmInteger<long> Convert(long value) => value;
        }

        private class LongFloatConverter : SpecializedConverterBase<long, float>
        {
            public override float Convert(long value) => value;
        }

        private class LongDoubleConverter : SpecializedConverterBase<long, double>
        {
            public override double Convert(long value) => value;
        }

        private class LongDecimalConverter : SpecializedConverterBase<long, decimal>
        {
            public override decimal Convert(long value) => value;
        }

        private class LongDecimal128Converter : SpecializedConverterBase<long, Decimal128>
        {
            public override Decimal128 Convert(long value) => value;
        }

        private class RealmIntegerByteCharConverter : SpecializedConverterBase<RealmInteger<byte>, char>
        {
            public override char Convert(RealmInteger<byte> value) => (char)(byte)value;
        }

        private class RealmIntegerByteByteConverter : SpecializedConverterBase<RealmInteger<byte>, byte>
        {
            public override byte Convert(RealmInteger<byte> value) => value;
        }

        private class RealmIntegerByteShortConverter : SpecializedConverterBase<RealmInteger<byte>, short>
        {
            public override short Convert(RealmInteger<byte> value) => value;
        }

        private class RealmIntegerByteIntConverter : SpecializedConverterBase<RealmInteger<byte>, int>
        {
            public override int Convert(RealmInteger<byte> value) => value;
        }

        private class RealmIntegerByteLongConverter : SpecializedConverterBase<RealmInteger<byte>, long>
        {
            public override long Convert(RealmInteger<byte> value) => value;
        }

        private class RealmIntegerByteRealmIntegerShortConverter : SpecializedConverterBase<RealmInteger<byte>, RealmInteger<short>>
        {
            public override RealmInteger<short> Convert(RealmInteger<byte> value) => (short)value;
        }

        private class RealmIntegerByteRealmIntegerIntConverter : SpecializedConverterBase<RealmInteger<byte>, RealmInteger<int>>
        {
            public override RealmInteger<int> Convert(RealmInteger<byte> value) => (int)value;
        }

        private class RealmIntegerByteRealmIntegerLongConverter : SpecializedConverterBase<RealmInteger<byte>, RealmInteger<long>>
        {
            public override RealmInteger<long> Convert(RealmInteger<byte> value) => (long)value;
        }

        private class RealmIntegerByteFloatConverter : SpecializedConverterBase<RealmInteger<byte>, float>
        {
            public override float Convert(RealmInteger<byte> value) => value;
        }

        private class RealmIntegerByteDoubleConverter : SpecializedConverterBase<RealmInteger<byte>, double>
        {
            public override double Convert(RealmInteger<byte> value) => value;
        }

        private class RealmIntegerByteDecimalConverter : SpecializedConverterBase<RealmInteger<byte>, decimal>
        {
            public override decimal Convert(RealmInteger<byte> value) => value;
        }

        private class RealmIntegerByteDecimal128Converter : SpecializedConverterBase<RealmInteger<byte>, Decimal128>
        {
            public override Decimal128 Convert(RealmInteger<byte> value) => (byte)value;
        }

        private class RealmIntegerShortCharConverter : SpecializedConverterBase<RealmInteger<short>, char>
        {
            public override char Convert(RealmInteger<short> value) => (char)(short)value;
        }

        private class RealmIntegerShortByteConverter : SpecializedConverterBase<RealmInteger<short>, byte>
        {
            public override byte Convert(RealmInteger<short> value) => (byte)value;
        }

        private class RealmIntegerShortShortConverter : SpecializedConverterBase<RealmInteger<short>, short>
        {
            public override short Convert(RealmInteger<short> value) => value;
        }

        private class RealmIntegerShortIntConverter : SpecializedConverterBase<RealmInteger<short>, int>
        {
            public override int Convert(RealmInteger<short> value) => value;
        }

        private class RealmIntegerShortLongConverter : SpecializedConverterBase<RealmInteger<short>, long>
        {
            public override long Convert(RealmInteger<short> value) => value;
        }

        private class RealmIntegerShortRealmIntegerByteConverter : SpecializedConverterBase<RealmInteger<short>, RealmInteger<byte>>
        {
            public override RealmInteger<byte> Convert(RealmInteger<short> value) => (byte)value;
        }

        private class RealmIntegerShortRealmIntegerIntConverter : SpecializedConverterBase<RealmInteger<short>, RealmInteger<int>>
        {
            public override RealmInteger<int> Convert(RealmInteger<short> value) => (int)value;
        }

        private class RealmIntegerShortRealmIntegerLongConverter : SpecializedConverterBase<RealmInteger<short>, RealmInteger<long>>
        {
            public override RealmInteger<long> Convert(RealmInteger<short> value) => (long)value;
        }

        private class RealmIntegerShortFloatConverter : SpecializedConverterBase<RealmInteger<short>, float>
        {
            public override float Convert(RealmInteger<short> value) => value;
        }

        private class RealmIntegerShortDoubleConverter : SpecializedConverterBase<RealmInteger<short>, double>
        {
            public override double Convert(RealmInteger<short> value) => value;
        }

        private class RealmIntegerShortDecimalConverter : SpecializedConverterBase<RealmInteger<short>, decimal>
        {
            public override decimal Convert(RealmInteger<short> value) => value;
        }

        private class RealmIntegerShortDecimal128Converter : SpecializedConverterBase<RealmInteger<short>, Decimal128>
        {
            public override Decimal128 Convert(RealmInteger<short> value) => (short)value;
        }

        private class RealmIntegerIntCharConverter : SpecializedConverterBase<RealmInteger<int>, char>
        {
            public override char Convert(RealmInteger<int> value) => (char)value;
        }

        private class RealmIntegerIntByteConverter : SpecializedConverterBase<RealmInteger<int>, byte>
        {
            public override byte Convert(RealmInteger<int> value) => (byte)value;
        }

        private class RealmIntegerIntShortConverter : SpecializedConverterBase<RealmInteger<int>, short>
        {
            public override short Convert(RealmInteger<int> value) => (short)value;
        }

        private class RealmIntegerIntIntConverter : SpecializedConverterBase<RealmInteger<int>, int>
        {
            public override int Convert(RealmInteger<int> value) => value;
        }

        private class RealmIntegerIntLongConverter : SpecializedConverterBase<RealmInteger<int>, long>
        {
            public override long Convert(RealmInteger<int> value) => value;
        }

        private class RealmIntegerIntRealmIntegerByteConverter : SpecializedConverterBase<RealmInteger<int>, RealmInteger<byte>>
        {
            public override RealmInteger<byte> Convert(RealmInteger<int> value) => (byte)value;
        }

        private class RealmIntegerIntRealmIntegerShortConverter : SpecializedConverterBase<RealmInteger<int>, RealmInteger<short>>
        {
            public override RealmInteger<short> Convert(RealmInteger<int> value) => (short)value;
        }

        private class RealmIntegerIntRealmIntegerLongConverter : SpecializedConverterBase<RealmInteger<int>, RealmInteger<long>>
        {
            public override RealmInteger<long> Convert(RealmInteger<int> value) => (long)value;
        }

        private class RealmIntegerIntFloatConverter : SpecializedConverterBase<RealmInteger<int>, float>
        {
            public override float Convert(RealmInteger<int> value) => value;
        }

        private class RealmIntegerIntDoubleConverter : SpecializedConverterBase<RealmInteger<int>, double>
        {
            public override double Convert(RealmInteger<int> value) => value;
        }

        private class RealmIntegerIntDecimalConverter : SpecializedConverterBase<RealmInteger<int>, decimal>
        {
            public override decimal Convert(RealmInteger<int> value) => value;
        }

        private class RealmIntegerIntDecimal128Converter : SpecializedConverterBase<RealmInteger<int>, Decimal128>
        {
            public override Decimal128 Convert(RealmInteger<int> value) => (int)value;
        }

        private class RealmIntegerLongCharConverter : SpecializedConverterBase<RealmInteger<long>, char>
        {
            public override char Convert(RealmInteger<long> value) => (char)value;
        }

        private class RealmIntegerLongByteConverter : SpecializedConverterBase<RealmInteger<long>, byte>
        {
            public override byte Convert(RealmInteger<long> value) => (byte)value;
        }

        private class RealmIntegerLongShortConverter : SpecializedConverterBase<RealmInteger<long>, short>
        {
            public override short Convert(RealmInteger<long> value) => (short)value;
        }

        private class RealmIntegerLongIntConverter : SpecializedConverterBase<RealmInteger<long>, int>
        {
            public override int Convert(RealmInteger<long> value) => (int)value;
        }

        private class RealmIntegerLongLongConverter : SpecializedConverterBase<RealmInteger<long>, long>
        {
            public override long Convert(RealmInteger<long> value) => value;
        }

        private class RealmIntegerLongRealmIntegerByteConverter : SpecializedConverterBase<RealmInteger<long>, RealmInteger<byte>>
        {
            public override RealmInteger<byte> Convert(RealmInteger<long> value) => (byte)value;
        }

        private class RealmIntegerLongRealmIntegerShortConverter : SpecializedConverterBase<RealmInteger<long>, RealmInteger<short>>
        {
            public override RealmInteger<short> Convert(RealmInteger<long> value) => (short)value;
        }

        private class RealmIntegerLongRealmIntegerIntConverter : SpecializedConverterBase<RealmInteger<long>, RealmInteger<int>>
        {
            public override RealmInteger<int> Convert(RealmInteger<long> value) => (int)value;
        }

        private class RealmIntegerLongFloatConverter : SpecializedConverterBase<RealmInteger<long>, float>
        {
            public override float Convert(RealmInteger<long> value) => value;
        }

        private class RealmIntegerLongDoubleConverter : SpecializedConverterBase<RealmInteger<long>, double>
        {
            public override double Convert(RealmInteger<long> value) => value;
        }

        private class RealmIntegerLongDecimalConverter : SpecializedConverterBase<RealmInteger<long>, decimal>
        {
            public override decimal Convert(RealmInteger<long> value) => value;
        }

        private class RealmIntegerLongDecimal128Converter : SpecializedConverterBase<RealmInteger<long>, Decimal128>
        {
            public override Decimal128 Convert(RealmInteger<long> value) => (long)value;
        }

        #endregion Integral Converters

        #region Floating Point Converters

        private class FloatNullableFloatConverter : SpecializedConverterBase<float, float?>
        {
            public override float? Convert(float value) => value;
        }

        private class FloatNullableDoubleConverter : SpecializedConverterBase<float, double?>
        {
            public override double? Convert(float value) => value;
        }

        private class FloatNullableDecimalConverter : SpecializedConverterBase<float, decimal?>
        {
            public override decimal? Convert(float value) => (decimal)value;
        }

        private class FloatNullableDecimal128Converter : SpecializedConverterBase<float, Decimal128?>
        {
            public override Decimal128? Convert(float value) => (Decimal128)value;
        }

        private class DoubleNullableFloatConverter : SpecializedConverterBase<double, float?>
        {
            public override float? Convert(double value) => (float)value;
        }

        private class DoubleNullableDoubleConverter : SpecializedConverterBase<double, double?>
        {
            public override double? Convert(double value) => value;
        }

        private class DoubleNullableDecimalConverter : SpecializedConverterBase<double, decimal?>
        {
            public override decimal? Convert(double value) => (decimal)value;
        }

        private class DoubleNullableDecimal128Converter : SpecializedConverterBase<double, Decimal128?>
        {
            public override Decimal128? Convert(double value) => (Decimal128)value;
        }

        private class DecimalNullableFloatConverter : SpecializedConverterBase<decimal, float?>
        {
            public override float? Convert(decimal value) => (float)value;
        }

        private class DecimalNullableDoubleConverter : SpecializedConverterBase<decimal, double?>
        {
            public override double? Convert(decimal value) => (double)value;
        }

        private class DecimalNullableDecimalConverter : SpecializedConverterBase<decimal, decimal?>
        {
            public override decimal? Convert(decimal value) => value;
        }

        private class DecimalNullableDecimal128Converter : SpecializedConverterBase<decimal, Decimal128?>
        {
            public override Decimal128? Convert(decimal value) => value;
        }

        private class Decimal128NullableFloatConverter : SpecializedConverterBase<Decimal128, float?>
        {
            public override float? Convert(Decimal128 value) => (float)value;
        }

        private class Decimal128NullableDoubleConverter : SpecializedConverterBase<Decimal128, double?>
        {
            public override double? Convert(Decimal128 value) => (double)value;
        }

        private class Decimal128NullableDecimalConverter : SpecializedConverterBase<Decimal128, decimal?>
        {
            public override decimal? Convert(Decimal128 value) => (decimal)value;
        }

        private class Decimal128NullableDecimal128Converter : SpecializedConverterBase<Decimal128, Decimal128?>
        {
            public override Decimal128? Convert(Decimal128 value) => value;
        }

        private class FloatDoubleConverter : SpecializedConverterBase<float, double>
        {
            public override double Convert(float value) => value;
        }

        private class FloatDecimalConverter : SpecializedConverterBase<float, decimal>
        {
            public override decimal Convert(float value) => (decimal)value;
        }

        private class FloatDecimal128Converter : SpecializedConverterBase<float, Decimal128>
        {
            public override Decimal128 Convert(float value) => (Decimal128)value;
        }

        private class DoubleFloatConverter : SpecializedConverterBase<double, float>
        {
            public override float Convert(double value) => (float)value;
        }

        private class DoubleDecimalConverter : SpecializedConverterBase<double, decimal>
        {
            public override decimal Convert(double value) => (decimal)value;
        }

        private class DoubleDecimal128Converter : SpecializedConverterBase<double, Decimal128>
        {
            public override Decimal128 Convert(double value) => (Decimal128)value;
        }

        private class DecimalFloatConverter : SpecializedConverterBase<decimal, float>
        {
            public override float Convert(decimal value) => (float)value;
        }

        private class DecimalDoubleConverter : SpecializedConverterBase<decimal, double>
        {
            public override double Convert(decimal value) => (double)value;
        }

        private class DecimalDecimal128Converter : SpecializedConverterBase<decimal, Decimal128>
        {
            public override Decimal128 Convert(decimal value) => value;
        }

        private class Decimal128FloatConverter : SpecializedConverterBase<Decimal128, float>
        {
            public override float Convert(Decimal128 value) => (float)value;
        }

        private class Decimal128DoubleConverter : SpecializedConverterBase<Decimal128, double>
        {
            public override double Convert(Decimal128 value) => (double)value;
        }

        private class Decimal128DecimalConverter : SpecializedConverterBase<Decimal128, decimal>
        {
            public override decimal Convert(Decimal128 value) => (decimal)value;
        }

        #endregion Floating Point Converters
    }
}
