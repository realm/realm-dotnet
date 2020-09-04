////////////////////////////////////////////////////////////////////////////
//
// Copyright 2017 Realm Inc.
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
using System.Runtime.InteropServices;
using MongoDB.Bson;
using Realms.Helpers;
using Realms.Schema;

namespace Realms.Native
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct PrimitiveValue
    {
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.U1)]
        internal PropertyType type;

        [FieldOffset(1)]
        [MarshalAs(UnmanagedType.I1)]
        internal bool has_value;

        [FieldOffset(8)]
        [MarshalAs(UnmanagedType.I1)]
        internal bool bool_value;

        [FieldOffset(8)]
        internal long int_value;

        [FieldOffset(8)]
        internal float float_value;

        [FieldOffset(8)]
        internal double double_value;

        [FieldOffset(8)]
        internal ulong low_bits;

        [FieldOffset(16)]
        internal ulong high_bits;

        [FieldOffset(16)]
        internal uint object_id_remainder;

        public static PrimitiveValue Bool(bool value)
        {
            return new PrimitiveValue
            {
                type = PropertyType.Bool,
                has_value = true,
                bool_value = value
            };
        }

        public static PrimitiveValue NullableBool(bool? value) => new PrimitiveValue
        {
            type = PropertyType.NullableBool,
            has_value = value.HasValue,
            bool_value = value.GetValueOrDefault()
        };

        public static PrimitiveValue Int(long value) => new PrimitiveValue
        {
            type = PropertyType.Int,
            has_value = true,
            int_value = value
        };

        public static PrimitiveValue NullableInt(long? value) => new PrimitiveValue
        {
            type = PropertyType.NullableInt,
            has_value = value.HasValue,
            int_value = value.GetValueOrDefault()
        };

        public static PrimitiveValue Float(float value) => new PrimitiveValue
        {
            type = PropertyType.Float,
            has_value = true,
            float_value = value
        };

        public static PrimitiveValue NullableFloat(float? value) => new PrimitiveValue
        {
            type = PropertyType.NullableFloat,
            has_value = value.HasValue,
            float_value = value.GetValueOrDefault()
        };

        public static PrimitiveValue Double(double value) => new PrimitiveValue
        {
            type = PropertyType.Double,
            has_value = true,
            double_value = value
        };

        public static PrimitiveValue NullableDouble(double? value) => new PrimitiveValue
        {
            type = PropertyType.NullableDouble,
            has_value = value.HasValue,
            double_value = value.GetValueOrDefault()
        };

        public static PrimitiveValue Date(DateTimeOffset value) => new PrimitiveValue
        {
            type = PropertyType.Date,
            has_value = true,
            int_value = value.ToUniversalTime().Ticks
        };

        public static PrimitiveValue NullableDate(DateTimeOffset? value) => new PrimitiveValue
        {
            type = PropertyType.NullableDate,
            has_value = value.HasValue,
            int_value = value.GetValueOrDefault().ToUniversalTime().Ticks
        };

        public static PrimitiveValue Create<T>(T value, PropertyType type)
        {
            var result = new PrimitiveValue
            {
                type = type,
                has_value = true
            };

            switch (type)
            {
                case PropertyType.Bool:
                    result.bool_value = Operator.Convert<T, bool>(value);
                    break;
                case PropertyType.NullableBool:
                    var boolValue = Operator.Convert<T, bool?>(value);
                    result.has_value = boolValue.HasValue;
                    result.bool_value = boolValue.GetValueOrDefault();
                    break;
                case PropertyType.Int:
                    result.int_value = Operator.Convert<T, long>(value);
                    break;
                case PropertyType.NullableInt:
                    var longValue = Operator.Convert<T, long?>(value);
                    result.has_value = longValue.HasValue;
                    result.int_value = longValue.GetValueOrDefault();
                    break;
                case PropertyType.Float:
                    result.float_value = Operator.Convert<T, float>(value);
                    break;
                case PropertyType.NullableFloat:
                    var floatValue = Operator.Convert<T, float?>(value);
                    result.has_value = floatValue.HasValue;
                    result.float_value = floatValue.GetValueOrDefault();
                    break;
                case PropertyType.Double:
                    result.double_value = Operator.Convert<T, double>(value);
                    break;
                case PropertyType.NullableDouble:
                    var doubleValue = Operator.Convert<T, double?>(value);
                    result.has_value = doubleValue.HasValue;
                    result.double_value = doubleValue.GetValueOrDefault();
                    break;
                case PropertyType.Date:
                    result.int_value = Operator.Convert<T, DateTimeOffset>(value).ToUniversalTime().Ticks;
                    break;
                case PropertyType.NullableDate:
                    var dateValue = Operator.Convert<T, DateTimeOffset?>(value);
                    result.has_value = dateValue.HasValue;
                    result.int_value = dateValue.GetValueOrDefault().ToUniversalTime().Ticks;
                    break;
                case PropertyType.Decimal:
                    var decimalValue = Operator.Convert<T, Decimal128>(value);
                    result.high_bits = decimalValue.GetIEEEHighBits();
                    result.low_bits = decimalValue.GetIEEELowBits();
                    break;
                case PropertyType.NullableDecimal:
                    var nullableDecimalValue = Operator.Convert<T, Decimal128?>(value);
                    result.has_value = nullableDecimalValue.HasValue;

                    var actualValue = nullableDecimalValue.GetValueOrDefault();
                    result.high_bits = actualValue.GetIEEEHighBits();
                    result.low_bits = actualValue.GetIEEELowBits();
                    break;
                case PropertyType.ObjectId:
                    var objectIdBytes = Operator.Convert<T, ObjectId>(value).ToByteArray();
                    result.low_bits = BitConverter.ToUInt64(objectIdBytes, 0);
                    result.object_id_remainder = BitConverter.ToUInt32(objectIdBytes, 8);
                    break;
                case PropertyType.NullableObjectId:
                    var objectId = Operator.Convert<T, ObjectId?>(value);
                    result.has_value = objectId.HasValue;

                    var actualObjectIdBytes = objectId.GetValueOrDefault().ToByteArray();
                    result.low_bits = BitConverter.ToUInt64(actualObjectIdBytes, 0);
                    result.object_id_remainder = BitConverter.ToUInt32(actualObjectIdBytes, 8);
                    break;
                default:
                    throw new NotSupportedException($"PrimitiveType {type} is not supported.");
            }

            return result;
        }

        public T Get<T>()
        {
            switch (type)
            {
                case PropertyType.Bool:
                    return Operator.Convert<bool, T>(bool_value);
                case PropertyType.NullableBool:
                    return Operator.Convert<bool?, T>(has_value ? bool_value : (bool?)null);
                case PropertyType.Int:
                    return Operator.Convert<long, T>(int_value);
                case PropertyType.NullableInt:
                    return Operator.Convert<long?, T>(has_value ? int_value : (long?)null);
                case PropertyType.Float:
                    return Operator.Convert<float, T>(float_value);
                case PropertyType.NullableFloat:
                    return Operator.Convert<float?, T>(has_value ? float_value : (float?)null);
                case PropertyType.Double:
                    return Operator.Convert<double, T>(double_value);
                case PropertyType.NullableDouble:
                    return Operator.Convert<double?, T>(has_value ? double_value : (double?)null);
                case PropertyType.Date:
                    return Operator.Convert<DateTimeOffset, T>(new DateTimeOffset(int_value, TimeSpan.Zero));
                case PropertyType.NullableDate:
                    return Operator.Convert<DateTimeOffset?, T>(has_value ? new DateTimeOffset(int_value, TimeSpan.Zero) : (DateTimeOffset?)null);
                case PropertyType.Decimal:
                    return Operator.Convert<Decimal128, T>(Decimal128.FromIEEEBits(high_bits, low_bits));
                case PropertyType.NullableDecimal:
                    return Operator.Convert<Decimal128?, T>(has_value ? Decimal128.FromIEEEBits(high_bits, low_bits) : (Decimal128?)null);
                case PropertyType.ObjectId:
                    var bytes = BitConverter.GetBytes(low_bits).Concat(BitConverter.GetBytes(object_id_remainder));
                    return Operator.Convert<ObjectId, T>(new ObjectId(bytes.ToArray()));
                case PropertyType.NullableObjectId:
                    if (has_value)
                    {
                        return Operator.Convert<ObjectId?, T>(null);
                    }

                    var nullableBytes = BitConverter.GetBytes(low_bits).Concat(BitConverter.GetBytes(object_id_remainder));
                    return Operator.Convert<ObjectId, T>(new ObjectId(nullableBytes.ToArray()));
                default:
                    throw new NotSupportedException($"PrimitiveType {type} is not supported.");
            }
        }
    }
}
