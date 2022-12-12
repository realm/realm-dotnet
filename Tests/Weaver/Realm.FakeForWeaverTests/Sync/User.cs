////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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
using System.Threading.Tasks;

namespace Realms.Sync
{
    public class User
    {
        public FunctionsClient Functions => new FunctionsClient();

        public MongoClient GetMongoClient(string serviceName) => default;
    }

    public class FunctionsClient
    {
        public Task<BsonValue> CallAsync(string name, params object[] args) => default;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<T> CallAsync<T>(string name, params object[] args) => default;
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }

    public class MongoClient
    { }
}
