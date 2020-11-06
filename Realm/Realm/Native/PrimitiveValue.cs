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
using System.Runtime.InteropServices;
using MongoDB.Bson;
using Realms.Helpers;

namespace Realms.Native
{
    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct PrimitiveValue
    {
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.U1)]
        private bool bool_value;

        [FieldOffset(0)]
        private long int_value;

        [FieldOffset(0)]
        private float float_value;

        [FieldOffset(0)]
        private double double_value;

        [FieldOffset(0)]
        private fixed ulong decimal_bits[2];

        [FieldOffset(0)]
        private fixed byte object_id_bytes[12];

        // Without this padding, .NET fails to marshal the decimal_bits array correctly and the second element is always 0.
        [FieldOffset(8)]
        [Obsolete("Don't use, please!")]
        private long dontuse;

        [FieldOffset(16)]
        [MarshalAs(UnmanagedType.U1)]
        public RealmValueType Type;

        public static PrimitiveValue Null() => new PrimitiveValue
        {
            Type = RealmValueType.Null,
        };

        public static PrimitiveValue Bool(bool value) => new PrimitiveValue
        {
            Type = RealmValueType.Bool,
            bool_value = value
        };

        public static PrimitiveValue NullableBool(bool? value) => value.HasValue ? Bool(value.Value) : Null();

        public static PrimitiveValue Int(long value) => new PrimitiveValue
        {
            Type = RealmValueType.Int,
            int_value = value
        };

        public static PrimitiveValue NullableInt(long? value) => value.HasValue ? Int(value.Value) : Null();

        public static PrimitiveValue Float(float value) => new PrimitiveValue
        {
            Type = RealmValueType.Float,
            float_value = value
        };

        public static PrimitiveValue NullableFloat(float? value) => value.HasValue ? Float(value.Value) : Null();

        public static PrimitiveValue Double(double value) => new PrimitiveValue
        {
            Type = RealmValueType.Double,
            double_value = value
        };

        public static PrimitiveValue NullableDouble(double? value) => value.HasValue ? Double(value.Value) : Null();

        public static PrimitiveValue Date(DateTimeOffset value) => new PrimitiveValue
        {
            Type = RealmValueType.Date,
            int_value = value.ToUniversalTime().Ticks
        };

        public static PrimitiveValue NullableDate(DateTimeOffset? value) => value.HasValue ? Date(value.Value) : Null();

        public static PrimitiveValue Decimal(Decimal128 value)
        {
            var result = new PrimitiveValue
            {
                Type = RealmValueType.Decimal128,
            };

            result.decimal_bits[0] = value.GetIEEELowBits();
            result.decimal_bits[1] = value.GetIEEEHighBits();

            return result;
        }

        public static PrimitiveValue NullableDecimal(Decimal128? value) => value.HasValue ? Decimal(value.Value) : Null();

        public static PrimitiveValue ObjectId(ObjectId value)
        {
            var result = new PrimitiveValue
            {
                Type = RealmValueType.ObjectId,
            };

            var objectIdBytes = value.ToByteArray();
            for (var i = 0; i < 12; i++)
            {
                result.object_id_bytes[i] = objectIdBytes[i];
            }

            return result;
        }

        public static PrimitiveValue NullableObjectId(ObjectId? value) => value.HasValue ? ObjectId(value.Value) : Null();

        public static PrimitiveValue Create<T>(T value, RealmValueType type)
        {
            if (value is null)
            {
                return Null();
            }

            return type switch
            {
                RealmValueType.Bool => Bool(Operator.Convert<T, bool>(value)),
                RealmValueType.Int => Int(Operator.Convert<T, long>(value)),
                RealmValueType.Float => Float(Operator.Convert<T, float>(value)),
                RealmValueType.Double => Double(Operator.Convert<T, double>(value)),
                RealmValueType.Date => Date(Operator.Convert<T, DateTimeOffset>(value)),
                RealmValueType.Decimal128 => Decimal(Operator.Convert<T, Decimal128>(value)),
                RealmValueType.ObjectId => ObjectId(Operator.Convert<T, ObjectId>(value)),
                _ => throw new NotSupportedException($"PrimitiveType {type} is not supported."),
            };
        }

        public bool ToBool() => bool_value;

        public bool? ToNullableBool() => Type != RealmValueType.Null ? bool_value : (bool?)null;

        public long ToInt() => int_value;

        public long? ToNullableInt() => Type != RealmValueType.Null ? int_value : (long?)null;

        public T ToIntegral<T>() => Operator.Convert<long, T>(int_value);

        public T ToNullableIntegral<T>() => Operator.Convert<long?, T>(Type != RealmValueType.Null ? int_value : (long?)null);

        public float ToFloat() => float_value;

        public float? ToNullableFloat() => Type != RealmValueType.Null ? float_value : (float?)null;

        public double ToDouble() => double_value;

        public double? ToNullableDouble() => Type != RealmValueType.Null ? double_value : (double?)null;

        public DateTimeOffset ToDate() => new DateTimeOffset(int_value, TimeSpan.Zero);

        public DateTimeOffset? ToNullableDate() => Type != RealmValueType.Null ? new DateTimeOffset(int_value, TimeSpan.Zero) : (DateTimeOffset?)null;

        public Decimal128 ToDecimal() => Decimal128.FromIEEEBits(decimal_bits[1], decimal_bits[0]);

        public Decimal128? ToNullableDecimal() => Type != RealmValueType.Null ? Decimal128.FromIEEEBits(decimal_bits[1], decimal_bits[0]) : (Decimal128?)null;

        public ObjectId ToObjectId()
        {
            var bytes = new byte[12];
            for (var i = 0; i < 12; i++)
            {
                bytes[i] = object_id_bytes[i];
            }

            return new ObjectId(bytes);
        }

        public ObjectId? ToNullableObjectId() => Type != RealmValueType.Null ? ToObjectId() : (ObjectId?)null;

        public T Get<T>()
        {
            return Type switch
            {
                RealmValueType.Null => (T)(object)null,
                RealmValueType.Bool => Operator.Convert<bool, T>(ToBool()),
                RealmValueType.Int => ToIntegral<T>(),
                RealmValueType.Float => Operator.Convert<float, T>(ToFloat()),
                RealmValueType.Double => Operator.Convert<double, T>(ToDouble()),
                RealmValueType.Date => Operator.Convert<DateTimeOffset, T>(ToDate()),
                RealmValueType.Decimal128 => Operator.Convert<Decimal128, T>(ToDecimal()),
                RealmValueType.ObjectId => Operator.Convert<ObjectId, T>(ToObjectId()),
                _ => throw new NotSupportedException($"PrimitiveType {Type} is not supported."),
            };
        }
    }
}
