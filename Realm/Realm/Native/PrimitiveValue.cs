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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Realms.Helpers;
using Realms.Schema;

namespace Realms.Native
{
    [StructLayout(LayoutKind.Explicit)]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter")]
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
                case PropertyType.Bool | PropertyType.Nullable:
                    var boolValue = Operator.Convert<T, bool?>(value);
                    result.has_value = boolValue.HasValue;
                    result.bool_value = boolValue.GetValueOrDefault();
                    break;
                case PropertyType.Int:
                    result.int_value = Operator.Convert<T, long>(value);
                    break;
                case PropertyType.Int | PropertyType.Nullable:
                    var longValue = Operator.Convert<T, long?>(value);
                    result.has_value = longValue.HasValue;
                    result.int_value = longValue.GetValueOrDefault();
                    break;
                case PropertyType.Float:
                    result.float_value = Operator.Convert<T, float>(value);
                    break;
                case PropertyType.Float | PropertyType.Nullable:
                    var floatValue = Operator.Convert<T, float?>(value);
                    result.has_value = floatValue.HasValue;
                    result.float_value = floatValue.GetValueOrDefault();
                    break;
                case PropertyType.Double:
                    result.double_value = Operator.Convert<T, double>(value);
                    break;
                case PropertyType.Double | PropertyType.Nullable:
                    var doubleValue = Operator.Convert<T, double?>(value);
                    result.has_value = doubleValue.HasValue;
                    result.double_value = doubleValue.GetValueOrDefault();
                    break;
                case PropertyType.Date:
                    result.int_value = Operator.Convert<T, DateTimeOffset>(value).ToUniversalTime().Ticks;
                    break;
                case PropertyType.Date | PropertyType.Nullable:
                    var dateValue = Operator.Convert<T, DateTimeOffset?>(value);
                    result.has_value = dateValue.HasValue;
                    result.int_value = dateValue.GetValueOrDefault().ToUniversalTime().Ticks;
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
                case PropertyType.Bool | PropertyType.Nullable:
                    return Operator.Convert<bool?, T>(has_value ? bool_value : (bool?)null);
                case PropertyType.Int:
                    return Operator.Convert<long, T>(int_value);
                case PropertyType.Int | PropertyType.Nullable:
                    return Operator.Convert<long?, T>(has_value ? int_value : (long?)null);
                case PropertyType.Float:
                    return Operator.Convert<float, T>(float_value);
                case PropertyType.Float | PropertyType.Nullable:
                    return Operator.Convert<float?, T>(has_value ? float_value : (float?)null);
                case PropertyType.Double:
                    return Operator.Convert<double, T>(double_value);
                case PropertyType.Double | PropertyType.Nullable:
                    return Operator.Convert<double?, T>(has_value ? double_value : (double?)null);
                case PropertyType.Date:
                    return Operator.Convert<DateTimeOffset, T>(new DateTimeOffset(int_value, TimeSpan.Zero));
                case PropertyType.Date | PropertyType.Nullable:
                    return Operator.Convert<DateTimeOffset?, T>(has_value ? new DateTimeOffset(int_value, TimeSpan.Zero) : (DateTimeOffset?)null);
                default:
                    throw new NotSupportedException($"PrimitiveType {type} is not supported.");
            }
        }
    }
}
