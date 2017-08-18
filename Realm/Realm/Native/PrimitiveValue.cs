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

        [FieldOffset(2)]
        [MarshalAs(UnmanagedType.I1)]
        internal bool bool_value;

        [FieldOffset(2)]
        internal long int_value;

        [FieldOffset(2)]
        internal float float_value;

        [FieldOffset(2)]
        internal double double_value;

        public static PrimitiveValue Create(bool value)
        {
            return new PrimitiveValue
            {
                bool_value = value,
                has_value = true,
                type = PropertyType.Bool
            };
        }

        public static PrimitiveValue Create(bool? value)
        {
            return new PrimitiveValue
            {
                bool_value = value.GetValueOrDefault(),
                has_value = value.HasValue,
                type = PropertyType.Bool | PropertyType.Nullable
            };
        }

        public static PrimitiveValue Create(long value)
        {
            return new PrimitiveValue
            {
                int_value = value,
                has_value = true,
                type = PropertyType.Int
            };
        }


        public static PrimitiveValue Create(long? value)
        {
            return new PrimitiveValue
            {
                int_value = value.GetValueOrDefault(),
                has_value = value.HasValue,
                type = PropertyType.Int | PropertyType.Nullable
            };
        }

        public static PrimitiveValue Create(float value)
        {
            return new PrimitiveValue
            {
                float_value = value,
                has_value = true,
                type = PropertyType.Float
            };
        }


        public static PrimitiveValue Create(float? value)
        {
            return new PrimitiveValue
            {
                float_value = value.GetValueOrDefault(),
                has_value = value.HasValue,
                type = PropertyType.Float | PropertyType.Nullable
            };
        }

        public static PrimitiveValue Create(double value)
        {
            return new PrimitiveValue
            {
                double_value = value,
                has_value = true,
                type = PropertyType.Double
            };
        }


        public static PrimitiveValue Create(double? value)
        {
            return new PrimitiveValue
            {
                double_value = value.GetValueOrDefault(),
                has_value = value.HasValue,
                type = PropertyType.Double | PropertyType.Nullable
            };
        }

        public static PrimitiveValue Create(DateTimeOffset value)
        {
            return new PrimitiveValue
            {
                int_value = value.ToUniversalTime().Ticks,
                has_value = true,
                type = PropertyType.Date
            };
        }


        public static PrimitiveValue Create(DateTimeOffset? value)
        {
            return new PrimitiveValue
            {
                int_value = value.GetValueOrDefault().ToUniversalTime().Ticks,
                has_value = value.HasValue,
                type = PropertyType.Date | PropertyType.Nullable
            };
        }
    }
}
