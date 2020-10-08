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

using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;

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
        }

        public static string ToNativeJson<T>(this T value, bool tryDynamic = true)
        {
            if (tryDynamic && !(value is null))
            {
                if (typeof(T) == typeof(object))
                {
                    return ToNativeJson((dynamic)value, tryDynamic: false);
                }

                if (typeof(T) == typeof(object[]))
                {
                    var elements = (value as object[]).Select(o => o is null ? ToNativeJson(o, tryDynamic: false) : ToNativeJson((dynamic)o, tryDynamic: false));
                    return $"[{string.Join(",", elements)}]";
                }
            }

            if (tryDynamic && (typeof(T) == typeof(object)) && !(value is null))
            {
                return ToNativeJson((dynamic)value, tryDynamic: false);
            }

            return value.ToJson(_jsonSettings);
        }
    }
}
