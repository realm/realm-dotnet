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
using System.Buffers;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using MongoDB.Bson;

namespace Realms.Native
{
    // This type is marshalled through C++ wrappers' realm_value_t
    [StructLayout(LayoutKind.Explicit)]
    [DebuggerDisplay("PrimitiveValue({Type})")]
    internal unsafe struct PrimitiveValue
    {
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

        [FieldOffset(0)]
        private fixed byte guid_bytes[16];

        // Without this padding, .NET fails to marshal the decimal_bits array correctly and the second element is always 0.
        [FieldOffset(8)]
        [Obsolete("Don't use, please!")]
        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1214:Readonly fields should appear before non-readonly fields", Justification = "The order of the struct matters.")]
        private readonly long dontuse;

        [FieldOffset(0)]
        private StringValue string_value;

        [FieldOffset(0)]
        private BinaryValue data_value;

        [FieldOffset(0)]
        private TimestampValue timestamp_value;

        [FieldOffset(0)]
        private LinkValue link_value;

        [FieldOffset(0)]
        private IntPtr collection_ptr;

        [FieldOffset(16)]
        [MarshalAs(UnmanagedType.U1)]
        public RealmValueType Type;

        public static PrimitiveValue Null() => new()
        {
            Type = RealmValueType.Null,
        };

        public static PrimitiveValue Bool(bool value) => new()
        {
            Type = RealmValueType.Bool,
            int_value = value ? 1 : 0,
        };

        public static PrimitiveValue NullableBool(bool? value) => value.HasValue ? Bool(value.Value) : Null();

        public static PrimitiveValue Int(long value) => new()
        {
            Type = RealmValueType.Int,
            int_value = value
        };

        public static PrimitiveValue NullableInt(long? value) => value.HasValue ? Int(value.Value) : Null();

        public static PrimitiveValue Float(float value) => new()
        {
            Type = RealmValueType.Float,
            float_value = value
        };

        public static PrimitiveValue NullableFloat(float? value) => value.HasValue ? Float(value.Value) : Null();

        public static PrimitiveValue Double(double value) => new()
        {
            Type = RealmValueType.Double,
            double_value = value
        };

        public static PrimitiveValue NullableDouble(double? value) => value.HasValue ? Double(value.Value) : Null();

        public static PrimitiveValue Date(DateTimeOffset value) => new()
        {
            Type = RealmValueType.Date,
            timestamp_value = new TimestampValue(value.ToUniversalTime().Ticks)
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

        public static PrimitiveValue Guid(Guid value)
        {
            var result = new PrimitiveValue
            {
                Type = RealmValueType.Guid,
            };

#pragma warning disable CS0618 // Type or member is obsolete
            var guidBytes = Realm.UseLegacyGuidRepresentation ? value.ToByteArray() : GuidConverter.ToBytes(value, GuidRepresentation.Standard);
#pragma warning restore CS0618 // Type or member is obsolete
            for (var i = 0; i < 16; i++)
            {
                result.guid_bytes[i] = guidBytes[i];
            }

            return result;
        }

        public static PrimitiveValue NullableGuid(Guid? value) => value.HasValue ? Guid(value.Value) : Null();

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
                Type = RealmValueType.Object,
                link_value = new LinkValue
                {
                    object_ptr = handle.DangerousGetHandle()
                }
            };
        }

        public readonly bool AsBool() => int_value == 1;

        public readonly long AsInt() => int_value;

        public readonly float AsFloat() => float_value;

        public readonly double AsDouble() => double_value;

        public readonly DateTimeOffset AsDate() => new(timestamp_value.ToTicks(), TimeSpan.Zero);

        public readonly Decimal128 AsDecimal() => Decimal128.FromIEEEBits(decimal_bits[1], decimal_bits[0]);

        public readonly ObjectId AsObjectId()
        {
            var bytes = new byte[12];
            for (var i = 0; i < 12; i++)
            {
                bytes[i] = object_id_bytes[i];
            }

            return new ObjectId(bytes);
        }

        public readonly Guid AsGuid()
        {
            var bytes = new byte[16];
            for (var i = 0; i < 16; i++)
            {
                bytes[i] = guid_bytes[i];
            }

#pragma warning disable CS0618 // Type or member is obsolete
            return Realm.UseLegacyGuidRepresentation ? new Guid(bytes) : GuidConverter.FromBytes(bytes, GuidRepresentation.Standard);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public readonly string AsString() => string_value!;

        public readonly byte[] AsBinary() => data_value.AsBytes();

        public readonly IRealmObjectBase AsObject(Realm realm)
        {
            var handle = new ObjectHandle(realm.SharedRealmHandle, link_value.object_ptr);

            // If Metadata doesn't contain the schema for this object, it's likely because
            // the value is Mixed and the object type was added by a newer version of the
            // app via Sync. In this case, we need to look up the object schema from disk
            if (!realm.Metadata.TryGetValue(link_value.table_key, out var objectMetadata))
            {
                var onDiskSchema = handle.GetSchema();
                objectMetadata = realm.MergeSchema(onDiskSchema)[link_value.table_key];
            }

            return realm.MakeObject(objectMetadata, handle);
        }

        public readonly RealmList<RealmValue> AsList(Realm realm)
        {
            var handle = new ListHandle(realm.SharedRealmHandle, collection_ptr);
            return new RealmList<RealmValue>(realm, handle, null);
        }

        public readonly RealmDictionary<RealmValue> AsDictionary(Realm realm)
        {
            var handle = new DictionaryHandle(realm.SharedRealmHandle, collection_ptr);
            return new RealmDictionary<RealmValue>(realm, handle, null);
        }

        public readonly bool TryGetObjectHandle(Realm realm, [NotNullWhen(true)] out ObjectHandle? handle)
        {
            if (Type == RealmValueType.Object)
            {
                handle = new ObjectHandle(realm.SharedRealmHandle, link_value.object_ptr);
                return true;
            }

            handle = null;
            return false;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LinkValue
        {
            public IntPtr object_ptr;

            [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1214:Readonly fields should appear before non-readonly fields", Justification = "The order of the struct matters.")]
            public readonly TableKey table_key;
        }

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct TimestampValue
        {
            private const long UnixEpochTicks = 621355968000000000;
            private const long TicksPerSecond = 10000000;
            private const long NanosecondsPerTick = 100;

            private readonly long seconds;
            private readonly int nanoseconds;

            public TimestampValue(long ticks)
            {
                var unix_ticks = ticks - UnixEpochTicks;
                seconds = unix_ticks / TicksPerSecond;
                nanoseconds = (int)((unix_ticks % TicksPerSecond) * NanosecondsPerTick);
            }

            public long ToTicks() => (seconds * TicksPerSecond) + (nanoseconds / NanosecondsPerTick) + UnixEpochTicks;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct StringValue
    {
        public byte* data;
        public nint size;

        public static readonly StringValue Null = new() { data = null };

        public static StringValue AllocateFrom(string? value, Arena arena)
        {
            if (value is null)
            {
                return new StringValue { data = null, size = 0 };
            }

            var byteCount = Encoding.UTF8.GetMaxByteCount(value.Length);
            var buffer = arena.Allocate<byte>(byteCount + 1);
#if NETCOREAPP2_1_OR_GREATER
            byteCount = Encoding.UTF8.GetBytes(value, new Span<byte>(buffer.Data, buffer.Length));
#else
            fixed (char* stringBytes = value)
            {
                byteCount = Encoding.UTF8.GetBytes(stringBytes, value.Length, buffer.Data, buffer.Length);
            }
#endif

            buffer.Data[byteCount] = 0;
            return new StringValue { data = buffer.Data, size = byteCount };
        }

        public static implicit operator bool(in StringValue value) => value.data != null;

        public static implicit operator string?(in StringValue value) => !value ? null : Encoding.UTF8.GetString(value.data, (int)value.size);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct BinaryValue
    {
        public byte* data;
        public IntPtr size;

        public readonly byte[] AsBytes(bool usePooledArray = false)
        {
            var bytes = usePooledArray ? ArrayPool<byte>.Shared.Rent((int)size) : new byte[(int)size];
            Marshal.Copy((IntPtr)data, bytes, 0, (int)size);
            return bytes;
        }
    }
}
