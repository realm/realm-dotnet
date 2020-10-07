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
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NUnit.Framework;
using Realms.Sync;
using Realms.Sync.Exceptions;

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class MongoClientTests : SyncTestBase
    {
        private const string ServiceName = "BackingDB";
        private const string DbName = "my-db";
        private const string CollectionName = "foos";

        [Test]
        public void MongoClient_ServiceName_ReturnsOriginalName()
        {
            var user = GetFakeUser();
            var client = user.GetMongoClient("foo-bar");

            Assert.That(client.ServiceName, Is.EqualTo("foo-bar"));
        }

        [Test]
        public void MongoDatabase_Name_ReturnsOriginalName()
        {
            var user = GetFakeUser();
            var client = user.GetMongoClient("foo-bar");
            var db = client.GetDatabase("my-db");

            Assert.That(db.Name, Is.EqualTo("my-db"));
            Assert.That(db.Client.ServiceName, Is.EqualTo("foo-bar"));
        }

        [Test]
        public void MongoCollection_Name_ReturnsOriginalName()
        {
            var user = GetFakeUser();
            var client = user.GetMongoClient("foo-bar");
            var db = client.GetDatabase("my-db");
            var collection = db.GetCollection("foos");

            Assert.That(collection.Name, Is.EqualTo("foos"));
            Assert.That(collection.Database.Name, Is.EqualTo("my-db"));
            Assert.That(collection.Database.Client.ServiceName, Is.EqualTo("foo-bar"));
        }

        [Test]
        public void MongoCollection_InsertOne()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var foo = new Foo("a", 5);
                var result = await collection.InsertOneAsync(foo);

                Assert.That(result.InsertedId, Is.EqualTo(foo.Id));

                var foos = await collection.FindAsync();
                Assert.That(foos.Length, Is.EqualTo(1));
                Assert.That(foos[0], Is.EqualTo(foo));
            });
        }

        [Test]
        public void MongoCollection_InsertOne_WithNullDoc()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                await TestHelpers.AssertThrows<ArgumentNullException>(() => collection.InsertOneAsync(null));
            });
        }

        [Test]
        public void MongoCollection_InsertOne_WithDocWithInvalidSchema()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetBsonCollection();

                var doc = new BsonDocument
                {
                    { "_id", ObjectId.GenerateNewId() },
                    { "longValue", "this is a string!" }
                };

                var ex = await TestHelpers.AssertThrows<AppException>(() => collection.InsertOneAsync(doc));
                Assert.That(ex.Message, Does.Contain("insert not permitted"));
                Assert.That(ex.HelpLink, Does.Contain("/logs?co_id="));
            });
        }

        [Test]
        public void MongoCollection_InsertOne_WithBsonDoc()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetBsonCollection();

                var doc = new BsonDocument
                {
                    { "_id", ObjectId.GenerateNewId() },
                    { "longValue", 5L },
                    { "stringValue", "bla bla" },
                };
                var result = await collection.InsertOneAsync(doc);

                Assert.That(result.InsertedId, Is.EqualTo(doc["_id"].AsObjectId));

                var foos = await collection.FindAsync();
                Assert.That(foos.Length, Is.EqualTo(1));
                Assert.That(foos[0]["stringValue"].AsString, Is.EqualTo("bla bla"));
                Assert.That(foos[0]["longValue"].AsInt64, Is.EqualTo(5));
            });
        }

        [Test]
        public void MongoCollection_InsertMany()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var foos = new[]
                {
                    new Foo("first", 123),
                    new Foo("second", 456)
                };

                var result = await collection.InsertManyAsync(foos);

                Assert.That(result.InsertedIds.Length, Is.EqualTo(2));
                Assert.That(result.InsertedIds, Is.EquivalentTo(foos.Select(f => f.Id)));

                var foundFoos = await collection.FindAsync();
                Assert.That(foundFoos.Length, Is.EqualTo(2));
                Assert.That(foundFoos, Is.EquivalentTo(foos));
            });
        }

        private async Task<MongoClient.Collection<Foo>> GetCollection(bool deleteAll = true)
        {
            var user = await GetUserAsync();
            var client = user.GetMongoClient(ServiceName);
            var db = client.GetDatabase(DbName);
            var collection = db.GetCollection<Foo>(CollectionName);

            if (deleteAll)
            {
                await collection.DeleteManyAsync();
            }

            return collection;
        }

        private async Task<MongoClient.Collection<BsonDocument>> GetBsonCollection(bool deleteAll = true)
        {
            var user = await GetUserAsync();
            var client = user.GetMongoClient(ServiceName);
            var db = client.GetDatabase(DbName);
            var collection = db.GetCollection(CollectionName);

            if (deleteAll)
            {
                await collection.DeleteManyAsync();
            }

            return collection;
        }

        private class Foo
        {
            [BsonElement("_id")]
            public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

            public string StringValue { get; set; }

            public long LongValue { get; set; }

            public Foo(string stringValue, long longValue)
            {
                StringValue = stringValue;
                LongValue = longValue;
            }

            public override bool Equals(object obj) =>
                (obj is Foo foo) &&
                foo.Id == Id &&
                foo.StringValue == StringValue &&
                foo.LongValue == LongValue;
        }
    }
}
