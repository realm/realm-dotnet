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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using MongoDB.Bson;

namespace Realms.Native
{
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerDisplay("PrimitiveValue({Type})")]
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

        [FieldOffset(0)]
        private StringValue string_value;

        [FieldOffset(0)]
        private BinaryValue data_value;

        [FieldOffset(0)]
        private LinkValue link_value;

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

        public static PrimitiveValue Data(IntPtr dataPtr, int size)
        {
            return new PrimitiveValue
            {
                Type = RealmValueType.Data,
                data_value = new BinaryValue
                {
                    data = (byte*)dataPtr,
                    size = (IntPtr)size
                }
            };
        }

        public static PrimitiveValue String(IntPtr dataPtr, int size)
        {
            return new PrimitiveValue
            {
                Type = RealmValueType.String,
                string_value = new StringValue
                {
                    data = (byte*)dataPtr,
                    size = (IntPtr)size
                }
            };
        }

        public static PrimitiveValue Object(ObjectHandle handle)
        {
            return new PrimitiveValue
            {
                Type = handle == null ? RealmValueType.Null : RealmValueType.Object,
                link_value = new LinkValue
                {
                    object_ptr = handle?.DangerousGetHandle() ?? IntPtr.Zero
                }
            };
        }

        public bool AsBool() => bool_value;

        public long AsInt() => int_value;

        public float AsFloat() => float_value;

        public double AsDouble() => double_value;

        public DateTimeOffset AsDate() => new DateTimeOffset(int_value, TimeSpan.Zero);

        public Decimal128 AsDecimal() => Decimal128.FromIEEEBits(decimal_bits[1], decimal_bits[0]);

        public ObjectId AsObjectId()
        {
            var bytes = new byte[12];
            for (var i = 0; i < 12; i++)
            {
                bytes[i] = object_id_bytes[i];
            }

            return new ObjectId(bytes);
        }

        public string AsString() => Encoding.UTF8.GetString(string_value.data, (int)string_value.size);

        public byte[] AsBinary()
        {
            if (Type == RealmValueType.Null)
            {
                return null;
            }

            var bytes = new byte[(int)data_value.size];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = data_value.data[i];
            }

            return bytes;
        }

        public ObjectHandle AsObject(RealmHandle root) => new ObjectHandle(root, link_value.object_ptr);

        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct StringValue
        {
            public byte* data;
            public IntPtr size;
        }

        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct BinaryValue
        {
            public byte* data;
            public IntPtr size;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LinkValue
        {
            public IntPtr object_ptr;
        }
    }
}
