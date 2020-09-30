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

using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Realms.Native
{
    internal unsafe struct BsonPayload
    {
        public enum BsonType
        {
            Null,
            Int32,
            Int64,
            Bool,
            Double,
            String,
            Binary,
            Timestamp,
            Datetime,
            ObjectId,
            Decimal128,
            RegularExpression,
            MaxKey,
            MinKey,
            Document,
            Array
        }

        public BsonType type;

        private byte* serialized;
        private int serialized_len;

        private string Serialized => Encoding.UTF8.GetString(serialized, serialized_len);

        public BsonValue GetValue()
        {
            if (type == BsonType.Null || Serialized == null)
            {
                return null;
            }

            if (type == BsonType.Document)
            {
                return BsonDocument.Parse(Serialized);
            }

            var fakeDoc = $"{{ \"value\": {Serialized} }}";
            return BsonDocument.Parse(fakeDoc)["value"];
        }

        public T GetValue<T>()
        {
            return BsonSerializer.Deserialize<T>(Serialized);
        }
    }
}
