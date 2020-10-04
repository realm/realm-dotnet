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

using MongoDB.Bson;
using MongoDB.Bson.IO;

namespace Realms.Helpers
{
    internal static class SerializationHelper
    {
        private static readonly JsonWriterSettings _jsonSettings = new JsonWriterSettings
        {
            OutputMode = JsonOutputMode.CanonicalExtendedJson,
        };

        [Preserve]
        static SerializationHelper()
        {
            // V10TODO: remove when MongoDB.Bson releases preserved version.
            _ = new MongoDB.Bson.Serialization.Serializers.ObjectIdSerializer();
            _ = new MongoDB.Bson.Serialization.Serializers.StringSerializer();
            _ = new MongoDB.Bson.Serialization.Serializers.NullableSerializer<long>();
            _ = new MongoDB.Bson.Serialization.Serializers.NullableSerializer<ObjectId>();
            _ = new MongoDB.Bson.Serialization.Serializers.Int64Serializer();
            _ = new MongoDB.Bson.Serialization.Serializers.ObjectIdSerializer();
        }

        public static string ToJson<T>(T value) => value.ToJson(_jsonSettings);
    }
}
