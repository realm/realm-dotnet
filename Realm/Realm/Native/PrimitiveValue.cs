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
using Realms.Schema;

namespace Realms.Native
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct PrimitiveValue
    {
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.U1)]
        public PropertyType Type;

        [FieldOffset(1)]
        [MarshalAs(UnmanagedType.I1)]
        private bool has_value;

        [FieldOffset(8)]
        [MarshalAs(UnmanagedType.I1)]
        private bool bool_value;

        [FieldOffset(8)]
        private long int_value;

        [FieldOffset(8)]
        private float float_value;

        [FieldOffset(8)]
        private double double_value;

        [FieldOffset(8)]
        private ulong low_bits;

        [FieldOffset(16)]
        private ulong high_bits;

        [FieldOffset(16)]
        private uint object_id_remainder;

        public static PrimitiveValue Bool(bool value)
        {
            return new PrimitiveValue
            {
                Type = PropertyType.Bool,
                has_value = true,
                bool_value = value
            };
        }

        public static PrimitiveValue NullableBool(bool? value) => new PrimitiveValue
        {
            Type = PropertyType.NullableBool,
            has_value = value.HasValue,
            bool_value = value.GetValueOrDefault()
        };

        public static PrimitiveValue Int(long value) => new PrimitiveValue
        {
            Type = PropertyType.Int,
            has_value = true,
            int_value = value
        };

        public static PrimitiveValue NullableInt(long? value) => new PrimitiveValue
        {
            Type = PropertyType.NullableInt,
            has_value = value.HasValue,
            int_value = value.GetValueOrDefault()
        };

        public static PrimitiveValue Float(float value) => new PrimitiveValue
        {
            Type = PropertyType.Float,
            has_value = true,
            float_value = value
        };

        public static PrimitiveValue NullableFloat(float? value) => new PrimitiveValue
        {
            Type = PropertyType.NullableFloat,
            has_value = value.HasValue,
            float_value = value.GetValueOrDefault()
        };

        public static PrimitiveValue Double(double value) => new PrimitiveValue
        {
            Type = PropertyType.Double,
            has_value = true,
            double_value = value
        };

        public static PrimitiveValue NullableDouble(double? value) => new PrimitiveValue
        {
            Type = PropertyType.NullableDouble,
            has_value = value.HasValue,
            double_value = value.GetValueOrDefault()
        };

        public static PrimitiveValue Date(DateTimeOffset value) => new PrimitiveValue
        {
            Type = PropertyType.Date,
            has_value = true,
            int_value = value.ToUniversalTime().Ticks
        };

        public static PrimitiveValue NullableDate(DateTimeOffset? value) => new PrimitiveValue
        {
            Type = PropertyType.NullableDate,
            has_value = value.HasValue,
            int_value = value.GetValueOrDefault().ToUniversalTime().Ticks
        };

        public static PrimitiveValue Decimal(Decimal128 value) => new PrimitiveValue
        {
            Type = PropertyType.Decimal,
            has_value = true,
            high_bits = value.GetIEEEHighBits(),
            low_bits = value.GetIEEELowBits()
        };

        public static PrimitiveValue NullableDecimal(Decimal128? value) => new PrimitiveValue
        {
            Type = PropertyType.NullableDecimal,
            has_value = value.HasValue,
            high_bits = value.GetValueOrDefault().GetIEEEHighBits(),
            low_bits = value.GetValueOrDefault().GetIEEELowBits()
        };

        public static PrimitiveValue ObjectId(ObjectId value) => throw new NotImplementedException();

        public static PrimitiveValue NullableObjectId(ObjectId? value) => throw new NotImplementedException();

        public static PrimitiveValue Create<T>(T value, PropertyType type)
        {
            return type switch
            {
                PropertyType.Bool => Bool(Operator.Convert<T, bool>(value)),
                PropertyType.NullableBool => NullableBool(Operator.Convert<T, bool?>(value)),
                PropertyType.Int => Int(Operator.Convert<T, long>(value)),
                PropertyType.NullableInt => NullableInt(Operator.Convert<T, long?>(value)),
                PropertyType.Float => Float(Operator.Convert<T, float>(value)),
                PropertyType.NullableFloat => NullableFloat(Operator.Convert<T, float?>(value)),
                PropertyType.Double => Double(Operator.Convert<T, double>(value)),
                PropertyType.NullableDouble => NullableDouble(Operator.Convert<T, double?>(value)),
                PropertyType.Date => Date(Operator.Convert<T, DateTimeOffset>(value)),
                PropertyType.NullableDate => NullableDate(Operator.Convert<T, DateTimeOffset?>(value)),
                PropertyType.Decimal => Decimal(Operator.Convert<T, Decimal128>(value)),
                PropertyType.NullableDecimal => NullableDecimal(Operator.Convert<T, Decimal128?>(value)),
                PropertyType.ObjectId => ObjectId(Operator.Convert<T, ObjectId>(value)),
                PropertyType.NullableObjectId => NullableObjectId(Operator.Convert<T, ObjectId?>(value)),
                _ => throw new NotSupportedException($"PrimitiveType {type} is not supported."),
            };
        }

        public bool ToBool() => bool_value;

        public bool? ToNullableBool() => has_value ? bool_value : (bool?)null;

        public long ToInt() => int_value;

        public long? ToNullableInt() => has_value ? int_value : (long?)null;

        public T ToIntegral<T>() => Operator.Convert<long, T>(int_value);

        public T ToNullableIntegral<T>() => Operator.Convert<long?, T>(has_value ? int_value : (long?)null);

        public float ToFloat() => float_value;

        public float? ToNullableFloat() => has_value ? float_value : (float?)null;

        public double ToDouble() => double_value;

        public double? ToNullableDouble() => has_value ? double_value : (double?)null;

        public DateTimeOffset ToDate() => new DateTimeOffset(int_value, TimeSpan.Zero);

        public DateTimeOffset? ToNullableDate() => has_value ? new DateTimeOffset(int_value, TimeSpan.Zero) : (DateTimeOffset?)null;

        public Decimal128 ToDecimal() => Decimal128.FromIEEEBits(high_bits, low_bits);

        public Decimal128? ToNullableDecimal() => has_value ? Decimal128.FromIEEEBits(high_bits, low_bits) : (Decimal128?)null;

        public ObjectId ToObjectId() => throw new NotImplementedException();

        public ObjectId? ToNullableObjectId() => has_value ? throw new NotImplementedException() : (ObjectId?)null;

        public T Get<T>()
        {
            return Type switch
            {
                PropertyType.Bool => Operator.Convert<bool, T>(ToBool()),
                PropertyType.NullableBool => Operator.Convert<bool?, T>(ToNullableBool()),
                PropertyType.Int => ToIntegral<T>(),
                PropertyType.NullableInt => ToNullableIntegral<T>(),
                PropertyType.Float => Operator.Convert<float, T>(ToFloat()),
                PropertyType.NullableFloat => Operator.Convert<float?, T>(ToNullableFloat()),
                PropertyType.Double => Operator.Convert<double, T>(ToDouble()),
                PropertyType.NullableDouble => Operator.Convert<double?, T>(ToNullableDouble()),
                PropertyType.Date => Operator.Convert<DateTimeOffset, T>(ToDate()),
                PropertyType.NullableDate => Operator.Convert<DateTimeOffset?, T>(ToNullableDate()),
                PropertyType.Decimal => Operator.Convert<Decimal128, T>(ToDecimal()),
                PropertyType.NullableDecimal => Operator.Convert<Decimal128?, T>(ToNullableDecimal()),
                PropertyType.ObjectId => Operator.Convert<ObjectId, T>(ToObjectId()),
                PropertyType.NullableObjectId => Operator.Convert<ObjectId?, T>(ToNullableObjectId()),
                _ => throw new NotSupportedException($"PrimitiveType {Type} is not supported."),
            };
        }
    }
}
