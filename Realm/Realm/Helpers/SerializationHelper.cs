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
using System.Linq;
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
        private static readonly JsonWriterSettings _jsonSettings = new JsonWriterSettings
        {
            OutputMode = JsonOutputMode.CanonicalExtendedJson,
        };

        static SerializationHelper()
        {
            var decimalSerializer = new DecimalSerializer(BsonType.Decimal128, new RepresentationConverter(allowOverflow: false, allowTruncation: false));
            BsonSerializer.RegisterSerializer(decimalSerializer);
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

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
        }

        public static string ToNativeJson(this object value)
        {
            if (value is object[] arr)
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
    }
}
