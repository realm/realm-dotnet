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
using System.Linq;
using System.Threading;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using Realms.Sync;

namespace Realms.Helpers
{
    internal static class SerializationHelper
    {
        private static readonly JsonWriterSettings _jsonSettings = new()
        {
            OutputMode = JsonOutputMode.CanonicalExtendedJson,
        };

        private static int _isInitialized;

        static SerializationHelper()
        {
            Initialize();
        }

        public static void Initialize()
        {
            if (Interlocked.CompareExchange(ref _isInitialized, 1, 0) == 0)
            {
                BsonSerializer.RegisterSerializationProvider(new RealmSerializationProvider());
            }
        }

        [Preserve]
        internal static void PreserveSerializers()
        {
            _ = new BooleanSerializer();
            _ = new ByteSerializer();
            _ = new CharSerializer();
            _ = new Int16Serializer();
            _ = new Int32Serializer();
            _ = new Int64Serializer();
            _ = new SingleSerializer();
            _ = new DoubleSerializer();
            _ = new DecimalSerializer();
            _ = new Decimal128Serializer();
            _ = new ObjectIdSerializer();
            _ = new GuidSerializer();
            _ = new DateTimeSerializer();
            _ = new DateTimeOffsetSerializer();
            _ = new StringSerializer();
            _ = new ByteArraySerializer();

            _ = new EnumSerializer<Credentials.AuthProvider>();

            _ = new ArraySerializer<bool>();
            _ = new ArraySerializer<byte>();
            _ = new ArraySerializer<char>();
            _ = new ArraySerializer<short>();
            _ = new ArraySerializer<int>();
            _ = new ArraySerializer<long>();
            _ = new ArraySerializer<float>();
            _ = new ArraySerializer<double>();
            _ = new ArraySerializer<decimal>();
            _ = new ArraySerializer<Decimal128>();
            _ = new ArraySerializer<ObjectId>();
            _ = new ArraySerializer<Guid>();
            _ = new ArraySerializer<DateTime>();
            _ = new ArraySerializer<DateTimeOffset>();
            _ = new ArraySerializer<string>();
            _ = new ArraySerializer<byte[]>();

            _ = new NullableSerializer<bool>();
            _ = new NullableSerializer<byte>();
            _ = new NullableSerializer<char>();
            _ = new NullableSerializer<short>();
            _ = new NullableSerializer<int>();
            _ = new NullableSerializer<long>();
            _ = new NullableSerializer<float>();
            _ = new NullableSerializer<double>();
            _ = new NullableSerializer<decimal>();
            _ = new NullableSerializer<Decimal128>();
            _ = new NullableSerializer<ObjectId>();
            _ = new NullableSerializer<Guid>();
            _ = new NullableSerializer<DateTime>();
            _ = new NullableSerializer<DateTimeOffset>();

            _ = new BsonDocumentSerializer();
            _ = new BsonArraySerializer();

            _ = new ObjectSerializer();

            _ = new EnumerableInterfaceImplementerSerializer<IEnumerable<object>, object>();
            _ = new ExpandoObjectSerializer();
        }

        public static string ToNativeJson(this object? value)
        {
            if (value is RealmValue rv)
            {
                return rv.AsAny().ToNativeJson();
            }

            if (value is object?[] arr)
            {
                var elements = arr.Select(ToNativeJson);
                return $"[{string.Join(",", elements)}]";
            }

            if (value is null)
            {
                return value.ToJson(_jsonSettings);
            }

            return value.ToJson(value.GetType(), _jsonSettings);
        }

        private class RealmSerializationProvider : IBsonSerializationProvider
        {
            public IBsonSerializer? GetSerializer(Type type) => type switch
            {
                _ when type == typeof(decimal) => new DecimalSerializer(BsonType.Decimal128, new RepresentationConverter(allowOverflow: false, allowTruncation: false)),
                _ when type == typeof(Guid) => new GuidSerializer(GuidRepresentation.Standard),
                _ when type == typeof(DateTimeOffset) => new DateTimeOffsetSerializer(BsonType.String),
                _ => null
            };
        }
    }
}
