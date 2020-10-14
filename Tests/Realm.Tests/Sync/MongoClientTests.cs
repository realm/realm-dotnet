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
using System.Net;
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
        private const string FoosCollectionName = "foos";
        private const string SalesCollectionName = "sales";

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
                Assert.That(ex.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
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

        [Test]
        public void MongoCollection_InsertMany_WithNullCollection()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                await TestHelpers.AssertThrows<ArgumentNullException>(() => collection.InsertManyAsync(null));
            });
        }

        [Test]
        public void MongoCollection_InsertMany_WithNullDocuments()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var foos = new[]
                {
                    new Foo("first", 123),
                    null
                };

                var ex = await TestHelpers.AssertThrows<ArgumentException>(() => collection.InsertManyAsync(foos));
                Assert.That(ex.ParamName, Is.EqualTo("docs"));
                Assert.That(ex.Message, Does.Contain("null elements"));
            });
        }

        [Test]
        public void MongoCollection_InsertMany_WithDocWithInvalidSchema()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetBsonCollection();

                var doc = new BsonDocument
                {
                    { "_id", ObjectId.GenerateNewId() },
                    { "longValue", "this is a string!" }
                };

                var ex = await TestHelpers.AssertThrows<AppException>(() => collection.InsertManyAsync(new[] { doc }));
                Assert.That(ex.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                Assert.That(ex.Message, Does.Contain("insert not permitted"));
                Assert.That(ex.HelpLink, Does.Contain("/logs?co_id="));
            });
        }

        [Test]
        public void MongoCollection_InsertMany_WithBsonDoc()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetBsonCollection();

                var docs = new[]
                {
                    new BsonDocument
                    {
                        { "_id", ObjectId.GenerateNewId() },
                        { "longValue", 5L },
                        { "stringValue", "first" },
                    },
                    new BsonDocument
                    {
                        { "_id", ObjectId.GenerateNewId() },
                        { "longValue", 999L },
                        { "stringValue", "second" },
                    },
                };

                var result = await collection.InsertManyAsync(docs);

                Assert.That(result.InsertedIds, Is.EquivalentTo(docs.Select(d => d["_id"].AsObjectId)));

                var foos = await collection.FindAsync();
                Assert.That(foos.Length, Is.EqualTo(2));
                Assert.That(foos[0]["stringValue"].AsString, Is.EqualTo("first"));
                Assert.That(foos[0]["longValue"].AsInt64, Is.EqualTo(5));
                Assert.That(foos[1]["stringValue"].AsString, Is.EqualTo("second"));
                Assert.That(foos[1]["longValue"].AsInt64, Is.EqualTo(999));
            });
        }

        [Test]
        public void MongoCollection_UpdateOne_WithoutFilter()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var update = BsonDocument.Parse(@"{
                    $set: {
                        StringValue: ""this is update!"",
                        LongValue: { $numberLong: ""999""}
                    }
                }");

                var result = await collection.UpdateOneAsync(filter: null, update);

                Assert.That(result.UpsertedId, Is.Null);
                Assert.That(result.MatchedCount, Is.EqualTo(1));
                Assert.That(result.ModifiedCount, Is.EqualTo(1));

                // Update inserted with expected values after the update
                inserted[0].StringValue = "this is update!";
                inserted[0].LongValue = 999;

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_UpdateOne_WithFilter()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var update = BsonDocument.Parse(@"{
                    $set: {
                        StringValue: ""this is update!"",
                        LongValue: { $numberLong: ""999"" }
                    }
                }");

                var filter = BsonDocument.Parse(@"{
                    LongValue: { $gte: 1 }
                }");

                var result = await collection.UpdateOneAsync(filter, update);

                Assert.That(result.UpsertedId, Is.Null);
                Assert.That(result.MatchedCount, Is.EqualTo(1));
                Assert.That(result.ModifiedCount, Is.EqualTo(1));

                // Update inserted with expected values after the update - should have matched
                // the second element
                inserted[1].StringValue = "this is update!";
                inserted[1].LongValue = 999;

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_UpdateOne_NoMatches_Upsert()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var upsertId = ObjectId.GenerateNewId();

                var test = new BsonDocument
                {
                    { "foo", upsertId }
                }.ToJson();

                var update = BsonDocument.Parse(@"{
                    $set: {
                        StringValue: ""this is update!"",
                        LongValue: { $numberLong: ""999"" }
                    },
                    $setOnInsert: {
                        _id: ObjectId(""" + upsertId + @""")
                    }
                }");

                var filter = BsonDocument.Parse(@"{
                    LongValue: { $gte: 5 }
                }");

                var result = await collection.UpdateOneAsync(filter, update, upsert: true);

                Assert.That(result.UpsertedId, Is.EqualTo(upsertId));
                Assert.That(result.MatchedCount, Is.EqualTo(0));
                Assert.That(result.ModifiedCount, Is.EqualTo(0));

                // Update inserted with expected values after the update - should have matched
                // no docs, so we expect an upsert
                inserted = inserted.Concat(new[]
                {
                    new Foo("this is update!", 999)
                    {
                        Id = upsertId
                    }
                }).ToArray();

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_UpdateOne_NoMatches_Noupsert()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var upsertId = ObjectId.GenerateNewId();
                var update = BsonDocument.Parse(@"{
                    $set: {
                        StringValue: ""this is update!"",
                        LongValue: { $numberLong: ""999"" }
                    },
                    $setOnInsert: {
                        _id: ObjectId(""" + upsertId + @""")
                    }
                }");

                var filter = BsonDocument.Parse(@"{
                    LongValue: { $gte: 5 }
                }");

                var result = await collection.UpdateOneAsync(filter, update, upsert: false);

                Assert.That(result.UpsertedId, Is.Null);
                Assert.That(result.MatchedCount, Is.EqualTo(0));
                Assert.That(result.ModifiedCount, Is.EqualTo(0));

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_UpdateOne_NullUpdate_Throws()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var ex = await TestHelpers.AssertThrows<ArgumentNullException>(() => collection.UpdateOneAsync(filter: null, updateDocument: null));
                Assert.That(ex.ParamName, Is.EqualTo("updateDocument"));
            });
        }

        [Test]
        public void MongoCollection_UpdateMany_WithoutFilter()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var update = BsonDocument.Parse(@"{ $set: { 
                    StringValue: ""this is update!"",
                    LongValue: { $numberLong: ""999"" }
                } }");

                var result = await collection.UpdateManyAsync(filter: null, update);

                Assert.That(result.UpsertedId, Is.Null);
                Assert.That(result.MatchedCount, Is.EqualTo(3));
                Assert.That(result.ModifiedCount, Is.EqualTo(3));

                // Update inserted with expected values after the update
                foreach (var foo in inserted)
                {
                    foo.StringValue = "this is update!";
                    foo.LongValue = 999;
                }

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_UpdateMany_WithFilter()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var update = BsonDocument.Parse(@"{ $set: { 
                    StringValue: ""this is update!"",
                    LongValue: { $numberLong: ""999"" }
                } }");

                var filter = BsonDocument.Parse("{ LongValue: { $gte: 1} }");

                var result = await collection.UpdateManyAsync(filter, update);

                Assert.That(result.UpsertedId, Is.Null);
                Assert.That(result.MatchedCount, Is.EqualTo(2));
                Assert.That(result.ModifiedCount, Is.EqualTo(2));

                // Update inserted with expected values after the update - should have matched
                // the second element
                for (var i = 1; i < 3; i++)
                {
                    inserted[i].StringValue = "this is update!";
                    inserted[i].LongValue = 999;
                }

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_UpdateMany_NoMatches_Upsert()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var upsertId = ObjectId.GenerateNewId();

                var update = BsonDocument.Parse(@"{ 
                    $set: { 
                        StringValue: ""this is update!"",
                        LongValue: { $numberLong: ""999"" }
                    },
                    $setOnInsert: {
                        _id: ObjectId(""" + upsertId + @""")
                    }
                }");

                var filter = BsonDocument.Parse("{ LongValue: { $gte: 5 } }");

                var result = await collection.UpdateManyAsync(filter, update, upsert: true);

                Assert.That(result.UpsertedId, Is.EqualTo(upsertId));
                Assert.That(result.MatchedCount, Is.EqualTo(0));
                Assert.That(result.ModifiedCount, Is.EqualTo(0));

                // Update inserted with expected values after the update - should have matched
                // no docs, so we expect an upsert
                inserted = inserted.Concat(new[]
                {
                    new Foo("this is update!", 999)
                    {
                        Id = upsertId
                    }
                }).ToArray();

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_UpdateMany_NoMatches_Noupsert()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var upsertId = ObjectId.GenerateNewId();

                var update = BsonDocument.Parse(@"{ 
                    $set: { 
                        StringValue: ""this is update!"",
                        LongValue: { $numberLong: ""999"" }
                    },
                    $setOnInsert: {
                        _id: ObjectId(""" + upsertId + @""")
                    }
                }");

                var filter = BsonDocument.Parse("{ LongValue: { $gte: 5 } }");

                var result = await collection.UpdateManyAsync(filter, update, upsert: false);

                Assert.That(result.UpsertedId, Is.Null);
                Assert.That(result.MatchedCount, Is.EqualTo(0));
                Assert.That(result.ModifiedCount, Is.EqualTo(0));

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_UpdateMany_NullUpdate_Throws()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var ex = await TestHelpers.AssertThrows<ArgumentNullException>(() => collection.UpdateManyAsync(filter: null, updateDocument: null));
                Assert.That(ex.ParamName, Is.EqualTo("updateDocument"));
            });
        }

        [Test]
        public void MongoCollection_DeleteOne_WithoutFilter()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var result = await collection.DeleteOneAsync();

                Assert.That(result.DeletedCount, Is.EqualTo(1));

                // The first element is removed
                inserted = inserted.Where((_, i) => i > 0).ToArray();

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_DeleteOne_WithFilter()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var filter = new BsonDocument
                {
                    {
                        "LongValue", new BsonDocument
                        {
                            { "$gte", 1 }
                        }
                    }
                };

                var result = await collection.DeleteOneAsync(filter);

                Assert.That(result.DeletedCount, Is.EqualTo(1));

                // The second element is removed
                inserted = inserted.Where((_, i) => i != 1).ToArray();

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_DeleteOne_WithFilter_NoMatches()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var filter = new BsonDocument
                {
                    {
                        "LongValue", new BsonDocument
                        {
                            { "$gte", 5 }
                        }
                    }
                };

                var result = await collection.DeleteOneAsync(filter);

                Assert.That(result.DeletedCount, Is.EqualTo(0));

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_DeleteMany_WithoutFilter()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var result = await collection.DeleteManyAsync();

                Assert.That(result.DeletedCount, Is.EqualTo(3));

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.Empty);
            });
        }

        [Test]
        public void MongoCollection_DeleteMany_WithFilter()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var filter = new BsonDocument
                {
                    {
                        "LongValue", new BsonDocument
                        {
                            { "$gte", 1 }
                        }
                    }
                };

                var result = await collection.DeleteManyAsync(filter);

                Assert.That(result.DeletedCount, Is.EqualTo(2));

                // The second and third elements are removed
                inserted = inserted.Where((_, i) => i == 0).ToArray();

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_DeleteMany_WithFilter_NoMatches()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var filter = new BsonDocument
                {
                    {
                        "LongValue", new BsonDocument
                        {
                            { "$gte", 5 }
                        }
                    }
                };

                var result = await collection.DeleteManyAsync(filter);

                Assert.That(result.DeletedCount, Is.EqualTo(0));

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_Count_WithoutFilter()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var result = await collection.CountAsync();
                Assert.That(result, Is.EqualTo(3));
            });
        }

        [Test]
        public void MongoCollection_Count_WithFilter()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var filter = new BsonDocument
                {
                    {
                        "LongValue", new BsonDocument
                        {
                            { "$gte", 1 }
                        }
                    }
                };

                var result = await collection.CountAsync(filter);

                Assert.That(result, Is.EqualTo(2));
            });
        }

        [Test]
        public void MongoCollection_Count_WithFilter_NoMatches()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var filter = new BsonDocument
                {
                    {
                        "LongValue", new BsonDocument
                        {
                            { "$gte", 5 }
                        }
                    }
                };

                var result = await collection.CountAsync(filter);

                Assert.That(result, Is.EqualTo(0));
            });
        }

        [Test]
        public void MongoCollection_FindOne()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var result = await collection.FindOneAsync();
                Assert.That(result, Is.EqualTo(inserted[0]));
            });
        }

        [Test]
        public void MongoCollection_FindOne_Sort()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var sort = new { LongValue = -1 };
                var result = await collection.FindOneAsync(sort: sort);
                Assert.That(result, Is.EqualTo(inserted[2]));
            });
        }

        [Test]
        public void MongoCollection_FindOne_Filter()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var filter = new BsonDocument
                {
                    {
                        "LongValue", new BsonDocument
                        {
                            { "$gte", 1 }
                        }
                    }
                };

                var result = await collection.FindOneAsync(filter);
                Assert.That(result, Is.EqualTo(inserted[1]));
            });
        }

        [Test]
        public void MongoCollection_FindOne_FilterSort()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var filter = new BsonDocument
                {
                    {
                        "LongValue", new BsonDocument
                        {
                            { "$gte", 1 }
                        }
                    }
                };

                var sort = new { StringValue = -1 };

                var result = await collection.FindOneAsync(filter, sort: sort);
                Assert.That(result, Is.EqualTo(inserted[2]));
            });
        }

        [Test]
        public void MongoCollection_FindOne_Projection()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var projection = new
                {
                    _id = 0,
                    StringValue = 1
                };

                foreach (var foo in inserted)
                {
                    foo.Id = default;
                    foo.LongValue = default;
                }

                var result = await collection.FindOneAsync(projection: projection);
                Assert.That(result, Is.EqualTo(inserted[0]));
            });
        }

        [Test]
        public void MongoCollection_FindOne_FilterProjection()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var filter = new BsonDocument
                {
                    {
                        "LongValue", new BsonDocument
                        {
                            { "$gte", 1 }
                        }
                    }
                };

                var projection = new
                {
                    _id = 0,
                    LongValue = 1
                };

                foreach (var foo in inserted)
                {
                    foo.Id = default;
                    foo.StringValue = default;
                }

                var result = await collection.FindOneAsync(filter, projection: projection);
                Assert.That(result, Is.EqualTo(inserted[1]));
            });
        }

        [Test]
        public void MongoCollection_FindOne_ProjectionSort()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var projection = new
                {
                    _id = 1,
                    LongValue = 1
                };

                var sort = new { LongValue = -1 };

                foreach (var foo in inserted)
                {
                    foo.StringValue = default;
                }

                var result = await collection.FindOneAsync(sort: sort, projection: projection);
                Assert.That(result, Is.EqualTo(inserted[2]));
            });
        }

        [Test]
        public void MongoCollection_FindOne_FilterProjectionSort()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var projection = new
                {
                    _id = 0,
                    LongValue = 1,
                    StringValue = 1
                };

                var filter = new BsonDocument
                {
                    {
                        "LongValue", new BsonDocument
                        {
                            { "$gte", 1 }
                        }
                    }
                };

                var sort = new { LongValue = -1 };

                foreach (var foo in inserted)
                {
                    foo.Id = default;
                }

                var result = await collection.FindOneAsync(filter, sort, projection);
                Assert.That(result, Is.EqualTo(inserted[2]));
            });
        }

        [Test]
        public void MongoCollection_Find()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var result = await collection.FindAsync();
                Assert.That(result, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_Find_Sort()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var sort = new { LongValue = -1 };
                var result = await collection.FindAsync(sort: sort);
                Assert.That(result, Is.EquivalentTo(inserted.Reverse()));
            });
        }

        [Test]
        public void MongoCollection_Find_Filter()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var filter = new BsonDocument
                {
                    {
                        "LongValue", new BsonDocument
                        {
                            { "$gte", 1 }
                        }
                    }
                };

                var result = await collection.FindAsync(filter);
                Assert.That(result, Is.EquivalentTo(inserted.Where((_, i) => i >= 1)));
            });
        }

        [Test]
        public void MongoCollection_Find_FilterSort()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var filter = new BsonDocument
                {
                    {
                        "LongValue", new BsonDocument
                        {
                            { "$gte", 1 }
                        }
                    }
                };

                var sort = new { StringValue = -1 };

                var result = await collection.FindAsync(filter, sort: sort);
                Assert.That(result, Is.EquivalentTo(inserted.Where((_, i) => i >= 1).Reverse()));
            });
        }

        [Test]
        public void MongoCollection_Find_Projection()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var projection = new
                {
                    _id = 0,
                    StringValue = 1
                };

                foreach (var foo in inserted)
                {
                    foo.Id = default;
                    foo.LongValue = default;
                }

                var result = await collection.FindAsync(projection: projection);
                Assert.That(result, Is.EquivalentTo(inserted.Reverse()));
            });
        }

        [Test]
        public void MongoCollection_Find_FilterProjection()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var filter = new BsonDocument
                {
                    {
                        "LongValue", new BsonDocument
                        {
                            { "$gte", 1 }
                        }
                    }
                };

                var projection = new
                {
                    _id = 0,
                    LongValue = 1
                };

                foreach (var foo in inserted)
                {
                    foo.Id = default;
                    foo.StringValue = default;
                }

                var result = await collection.FindAsync(filter, projection: projection);
                Assert.That(result, Is.EquivalentTo(inserted.Where((_, i) => i >= 1)));
            });
        }

        [Test]
        public void MongoCollection_Find_ProjectionSort()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var projection = new
                {
                    _id = 1,
                    LongValue = 1
                };

                var sort = new { LongValue = -1 };

                foreach (var foo in inserted)
                {
                    foo.StringValue = default;
                }

                var result = await collection.FindAsync(sort: sort, projection: projection);
                Assert.That(result, Is.EquivalentTo(inserted.Reverse()));
            });
        }

        [Test]
        public void MongoCollection_Find_FilterProjectionSort()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var projection = new
                {
                    _id = 0,
                    LongValue = 1,
                    StringValue = 1
                };

                var filter = new BsonDocument
                {
                    {
                        "LongValue", new BsonDocument
                        {
                            { "$gte", 1 }
                        }
                    }
                };

                var sort = new { LongValue = -1 };

                foreach (var foo in inserted)
                {
                    foo.Id = default;
                }

                var result = await collection.FindAsync(filter, sort, projection);
                Assert.That(result, Is.EquivalentTo(inserted.Where((_, i) => i >= 1).Reverse()));
            });
        }

        [Test]
        public void MongoCollection_Find_Limit()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var result = await collection.FindAsync(limit: 2);
                Assert.That(result, Is.EquivalentTo(inserted.Take(2)));
            });
        }

        [Test]
        public void MongoCollection_Find_SortLimit()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var sort = new { LongValue = -1 };
                var result = await collection.FindAsync(sort: sort, limit: 2);
                Assert.That(result, Is.EquivalentTo(inserted.Reverse().Take(2)));
            });
        }

        [Test]
        public void MongoCollection_Find_Filter_Limit()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var filter = new BsonDocument
                {
                    {
                        "LongValue", new BsonDocument
                        {
                            { "$gte", 1 }
                        }
                    }
                };

                var result = await collection.FindAsync(filter, limit: 1);
                Assert.That(result, Is.EquivalentTo(inserted.Where((_, i) => i >= 1).Take(1)));
            });
        }

        [Test]
        public void MongoCollection_Find_FilterSortLimit()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var filter = new BsonDocument
                {
                    {
                        "LongValue", new BsonDocument
                        {
                            { "$gte", 1 }
                        }
                    }
                };

                var sort = new { StringValue = -1 };

                var result = await collection.FindAsync(filter, sort: sort, limit: 1);
                Assert.That(result, Is.EquivalentTo(inserted.Where((_, i) => i >= 1).Reverse().Take(1)));
            });
        }

        [Test]
        public void MongoCollection_Find_ProjectionLimit()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var projection = new
                {
                    _id = 0,
                    StringValue = 1
                };

                foreach (var foo in inserted)
                {
                    foo.Id = default;
                    foo.LongValue = default;
                }

                var result = await collection.FindAsync(projection: projection, limit: 100);
                Assert.That(result, Is.EquivalentTo(inserted.Reverse().Take(100)));
            });
        }

        [Test]
        public void MongoCollection_Find_FilterProjectionLimit()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var filter = new BsonDocument
                {
                    {
                        "LongValue", new BsonDocument
                        {
                            { "$gte", 1 }
                        }
                    }
                };

                var projection = new
                {
                    _id = 0,
                    LongValue = 1
                };

                foreach (var foo in inserted)
                {
                    foo.Id = default;
                    foo.StringValue = default;
                }

                var result = await collection.FindAsync(filter, projection: projection, limit: 2);
                Assert.That(result, Is.EquivalentTo(inserted.Where((_, i) => i >= 1).Take(2)));
            });
        }

        [Test]
        public void MongoCollection_Find_ProjectionSortLimit()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var projection = new
                {
                    _id = 1,
                    LongValue = 1
                };

                var sort = new { LongValue = -1 };

                foreach (var foo in inserted)
                {
                    foo.StringValue = default;
                }

                var result = await collection.FindAsync(sort: sort, projection: projection, limit: 2);
                Assert.That(result, Is.EquivalentTo(inserted.Reverse().Take(2)));
            });
        }

        [Test]
        public void MongoCollection_Find_FilterProjectionSortLimit()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();

                var inserted = await InsertSomeData(collection, 3);

                var projection = new
                {
                    _id = 0,
                    LongValue = 1,
                    StringValue = 1
                };

                var filter = new BsonDocument
                {
                    {
                        "LongValue", new BsonDocument
                        {
                            { "$gte", 1 }
                        }
                    }
                };

                var sort = new { LongValue = -1 };

                foreach (var foo in inserted)
                {
                    foo.Id = default;
                }

                var result = await collection.FindAsync(filter, sort, projection, limit: 1);
                Assert.That(result, Is.EquivalentTo(inserted.Where((_, i) => i >= 1).Reverse().Take(1)));
            });
        }

        [Test]
        public void MongoCollection_Aggregate()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetSalesCollection();

                await InsertSalesData(collection);

                var aggregation = GetSalesAggregation();

                var result = await collection.AggregateAsync(aggregation);

                // Expected results:
                // { "_id" : { "day" : 46, "year" : 2014 }, "totalAmount" : 150, "count" : 2 }
                // { "_id" : { "day" : 34, "year" : 2014 }, "totalAmount" : 45, "count" : 2 }
                // { "_id" : { "day" : 1, "year" : 2014 }, "totalAmount" : 20, "count" : 1 }
                Assert.That(result.Length, Is.EqualTo(3));

                // Fix the ordering to make it easier to assert the expected values
                result = result.OrderByDescending(r => r["_id"]["Day"].AsInt32).ToArray();

                Assert.That(result[0]["_id"]["Day"].AsInt32, Is.EqualTo(46));
                Assert.That(result[0]["_id"]["Year"].AsInt32, Is.EqualTo(2014));
                Assert.That(result[0]["TotalAmount"].AsDecimal, Is.EqualTo(150));
                Assert.That(result[0]["Count"].AsInt32, Is.EqualTo(2));

                Assert.That(result[1]["_id"]["Day"].AsInt32, Is.EqualTo(34));
                Assert.That(result[1]["_id"]["Year"].AsInt32, Is.EqualTo(2014));
                Assert.That(result[1]["TotalAmount"].AsDecimal, Is.EqualTo(45));
                Assert.That(result[1]["Count"].AsInt32, Is.EqualTo(2));

                Assert.That(result[2]["_id"]["Day"].AsInt32, Is.EqualTo(1));
                Assert.That(result[2]["_id"]["Year"].AsInt32, Is.EqualTo(2014));
                Assert.That(result[2]["TotalAmount"].AsDecimal, Is.EqualTo(20));
                Assert.That(result[2]["Count"].AsInt32, Is.EqualTo(1));
            });
        }

        [Test]
        public void MongoCollection_Aggregate_GenericResult()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetSalesCollection();

                await InsertSalesData(collection);

                var aggregation = GetSalesAggregation();

                var result = await collection.AggregateAsync<AggregationResult>(aggregation);

                // Expected results:
                // { "_id" : { "day" : 46, "year" : 2014 }, "totalAmount" : 150, "count" : 2 }
                // { "_id" : { "day" : 34, "year" : 2014 }, "totalAmount" : 45, "count" : 2 }
                // { "_id" : { "day" : 1, "year" : 2014 }, "totalAmount" : 20, "count" : 1 }
                Assert.That(result.Length, Is.EqualTo(3));

                // Fix the ordering to make it easier to assert the expected values
                result = result.OrderByDescending(r => r.Id.Day).ToArray();

                Assert.That(result[0].Id.Day, Is.EqualTo(46));
                Assert.That(result[0].Id.Year, Is.EqualTo(2014));
                Assert.That(result[0].TotalAmount, Is.EqualTo(150));
                Assert.That(result[0].Count, Is.EqualTo(2));

                Assert.That(result[1].Id.Day, Is.EqualTo(34));
                Assert.That(result[1].Id.Year, Is.EqualTo(2014));
                Assert.That(result[1].TotalAmount, Is.EqualTo(45));
                Assert.That(result[1].Count, Is.EqualTo(2));

                Assert.That(result[2].Id.Day, Is.EqualTo(1));
                Assert.That(result[2].Id.Year, Is.EqualTo(2014));
                Assert.That(result[2].TotalAmount, Is.EqualTo(20));
                Assert.That(result[2].Count, Is.EqualTo(1));
            });
        }

        [Test]
        public void MongoCollection_FindOneAndUpdate()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();
                var inserted = await InsertSomeData(collection, 3);

                var update = BsonDocument.Parse(@"{ $set: { 
                    StringValue: ""this is update!"",
                    LongValue: { $numberLong: ""999"" }
                } }");

                var result = await collection.FindOneAndUpdateAsync(filter: null, update);

                Assert.That(result, Is.EqualTo(inserted[0]));

                // Update inserted with expected values after the update
                inserted[0].StringValue = "this is update!";
                inserted[0].LongValue = 999;

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_FindOneAndUpdate_ReturnNewDocument()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();
                var inserted = await InsertSomeData(collection, 3);

                var update = BsonDocument.Parse(@"{ $set: { 
                    StringValue: ""this is update!"",
                    LongValue: { $numberLong: ""999"" }
                } }");

                var result = await collection.FindOneAndUpdateAsync(filter: null, update, returnNewDocument: true);

                Assert.That(result.StringValue, Is.EqualTo("this is update!"));
                Assert.That(result.LongValue, Is.EqualTo(999));

                // Update inserted with expected values after the update
                inserted[0] = result;

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_FindOneAndUpdate_Filter()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();
                var inserted = await InsertSomeData(collection, 3);

                var update = BsonDocument.Parse(@"{ $set: { 
                    StringValue: ""this is update!"",
                    LongValue: { $numberLong: ""999"" }
                } }");

                var filter = BsonDocument.Parse("{ LongValue: { $gte: 1} }");

                var result = await collection.FindOneAndUpdateAsync(filter, update);

                Assert.That(result, Is.EqualTo(inserted[1]));

                // Update inserted with expected values after the update
                inserted[1].StringValue = "this is update!";
                inserted[1].LongValue = 999;

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_FindOneAndUpdate_Sort()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();
                var inserted = await InsertSomeData(collection, 3);

                var update = BsonDocument.Parse(@"{ $set: { 
                    StringValue: ""this is update!"",
                    LongValue: { $numberLong: ""999"" }
                } }");

                var sort = BsonDocument.Parse("{ LongValue: -1 }");

                var result = await collection.FindOneAndUpdateAsync(filter: null, update, sort: sort);

                Assert.That(result, Is.EqualTo(inserted[2]));

                // Update inserted with expected values after the update
                inserted[2].StringValue = "this is update!";
                inserted[2].LongValue = 999;

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_FindOneAndUpdate_Projection()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();
                var inserted = await InsertSomeData(collection, 3);

                var update = BsonDocument.Parse(@"{ $set: { 
                    StringValue: ""this is update!"",
                    LongValue: { $numberLong: ""999"" }
                } }");

                var projection = BsonDocument.Parse("{ LongValue: 1, _id: 0 }");

                var result = await collection.FindOneAndUpdateAsync(filter: null, update, projection: projection);

                Assert.That(result.StringValue, Is.Null);
                Assert.That(result.LongValue, Is.EqualTo(inserted[0].LongValue));
                Assert.That(result.Id, Is.EqualTo(default(ObjectId)));

                // Update inserted with expected values after the update
                inserted[0].StringValue = "this is update!";
                inserted[0].LongValue = 999;

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_FindOneAndUpdate_FilterUpsert_Matches()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();
                var inserted = await InsertSomeData(collection, 3);

                var update = BsonDocument.Parse(@"{ $set: { 
                    StringValue: ""this is update!"",
                    LongValue: { $numberLong: ""999"" }
                } }");

                var filter = BsonDocument.Parse("{ LongValue: { $gte: 1} }");

                var result = await collection.FindOneAndUpdateAsync(filter, update, upsert: true);

                Assert.That(result, Is.EqualTo(inserted[1]));

                // Update inserted with expected values after the update
                inserted[1].StringValue = "this is update!";
                inserted[1].LongValue = 999;

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_FindOneAndUpdate_FilterUpsert_NoMatches()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();
                var inserted = await InsertSomeData(collection, 3);

                var update = BsonDocument.Parse(@"{ $set: { 
                    StringValue: ""this is update!"",
                    LongValue: { $numberLong: ""999"" }
                } }");

                var filter = BsonDocument.Parse("{ LongValue: { $gte: 5} }");

                var result = await collection.FindOneAndUpdateAsync(filter, update, upsert: true);
                Assert.That(result, Is.Null);

                // Update inserted with expected values after the update
                inserted = inserted.Concat(new[] { new Foo("this is update!", 999) }).ToArray();

                var docs = await collection.FindAsync();
                inserted[3].Id = docs[3].Id;
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_FindOneAndUpdate_FilterUpsertReturnNewDocument_NoMatches()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();
                var inserted = await InsertSomeData(collection, 3);

                var update = BsonDocument.Parse(@"{ $set: { 
                    StringValue: ""this is update!"",
                    LongValue: { $numberLong: ""999"" }
                } }");

                var filter = BsonDocument.Parse("{ LongValue: { $gte: 5} }");

                var result = await collection.FindOneAndUpdateAsync(filter, update, upsert: true, returnNewDocument: true);
                Assert.That(result.StringValue, Is.EqualTo("this is update!"));
                Assert.That(result.LongValue, Is.EqualTo(999));

                // Update inserted with expected values after the update
                inserted = inserted.Concat(new[] { result }).ToArray();

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_FindOneAndUpdate_FilterSortProjectionUpsertReturnNewDocument()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();
                var inserted = await InsertSomeData(collection, 3);

                var update = BsonDocument.Parse(@"{ $set: { 
                    StringValue: ""this is update!"",
                    LongValue: { $numberLong: ""999"" }
                } }");

                var sort = BsonDocument.Parse("{ LongValue: -1 }");
                var projection = BsonDocument.Parse("{ StringValue: 1, _id: 0 }");
                var filter = BsonDocument.Parse("{ LongValue: { $gte: 1} }");

                var result = await collection.FindOneAndUpdateAsync(filter, update, sort, projection, upsert: true, returnNewDocument: true);
                Assert.That(result.StringValue, Is.EqualTo("this is update!"));
                Assert.That(result.LongValue, Is.EqualTo(default(long)));
                Assert.That(result.Id, Is.EqualTo(default(ObjectId)));

                inserted[2].StringValue = "this is update!";
                inserted[2].LongValue = 999;

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_FindOneAndReplace()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();
                var inserted = await InsertSomeData(collection, 3);

                var replacement = Foo.WithoutId("this is update!", 999);

                var result = await collection.FindOneAndReplaceAsync(filter: null, replacement);

                Assert.That(result, Is.EqualTo(inserted[0]));

                // Update inserted with expected values after the update
                inserted[0].StringValue = replacement.StringValue;
                inserted[0].LongValue = replacement.LongValue;

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_FindOneAndReplace_ReturnNewDocument()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();
                var inserted = await InsertSomeData(collection, 3);

                var replacement = Foo.WithoutId("this is update!", 999);

                var result = await collection.FindOneAndReplaceAsync(filter: null, replacement, returnNewDocument: true);

                replacement.Id = inserted[0].Id;
                Assert.That(result, Is.EqualTo(replacement));

                // Update inserted with expected values after the update
                inserted[0].StringValue = replacement.StringValue;
                inserted[0].LongValue = replacement.LongValue;

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_FindOneAndReplace_UpsertNoMatches_GeneratesId()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();
                var inserted = await InsertSomeData(collection, 3);

                var replacement = Foo.WithoutId("this is update!", 999);
                var filter = BsonDocument.Parse("{ LongValue: { $gte: 5} }");

                var result = await collection.FindOneAndReplaceAsync(filter, replacement, upsert: true, returnNewDocument: true);

                Assert.That(result.Id, Is.Not.EqualTo(default(ObjectId)));
                replacement.Id = result.Id;
                Assert.That(result, Is.EqualTo(replacement));

                // Update inserted with expected values after the update
                inserted = inserted.Concat(new[] { result }).ToArray();

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_FindOneAndReplace_Filter()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();
                var inserted = await InsertSomeData(collection, 3);

                var replacement = Foo.WithoutId("this is update!", 999);

                var filter = BsonDocument.Parse("{ LongValue: { $gte: 1} }");

                var result = await collection.FindOneAndReplaceAsync(filter, replacement);

                Assert.That(result, Is.EqualTo(inserted[1]));

                // Update inserted with expected values after the update
                inserted[1].StringValue = replacement.StringValue;
                inserted[1].LongValue = replacement.LongValue;

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_FindOneAndReplace_Sort()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();
                var inserted = await InsertSomeData(collection, 3);

                var replacement = Foo.WithoutId("this is update!", 999);
                var sort = BsonDocument.Parse("{ LongValue: -1 }");

                var result = await collection.FindOneAndReplaceAsync(filter: null, replacement, sort: sort);

                Assert.That(result, Is.EqualTo(inserted[2]));

                // Update inserted with expected values after the update
                inserted[2].StringValue = replacement.StringValue;
                inserted[2].LongValue = replacement.LongValue;

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_FindOneAndReplace_Projection()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();
                var inserted = await InsertSomeData(collection, 3);

                var replacement = Foo.WithoutId("this is update!", 999);
                var projection = BsonDocument.Parse("{ LongValue: 1, _id: 0 }");

                var result = await collection.FindOneAndReplaceAsync(filter: null, replacement, projection: projection);

                Assert.That(result.StringValue, Is.Null);
                Assert.That(result.LongValue, Is.EqualTo(inserted[0].LongValue));
                Assert.That(result.Id, Is.EqualTo(default(ObjectId)));

                // Update inserted with expected values after the update
                inserted[0].StringValue = replacement.StringValue;
                inserted[0].LongValue = replacement.LongValue;

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_FindOneAndReplace_FilterUpsert_Matches()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();
                var inserted = await InsertSomeData(collection, 3);

                var replacement = Foo.WithoutId("this is update!", 999);
                var filter = BsonDocument.Parse("{ LongValue: { $gte: 1} }");

                var result = await collection.FindOneAndReplaceAsync(filter, replacement, upsert: true);

                Assert.That(result, Is.EqualTo(inserted[1]));

                // Update inserted with expected values after the update
                inserted[1].StringValue = replacement.StringValue;
                inserted[1].LongValue = replacement.LongValue;

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_FindOneAndReplace_FilterUpsert_NoMatches()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();
                var inserted = await InsertSomeData(collection, 3);

                var replacement = new Foo("this is update!", 999);
                var filter = BsonDocument.Parse("{ LongValue: { $gte: 5} }");

                var result = await collection.FindOneAndReplaceAsync(filter, replacement, upsert: true);
                Assert.That(result, Is.Null);

                // Update inserted with expected values after the update
                inserted = inserted.Concat(new[] { replacement }).ToArray();

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_FindOneAndReplace_FilterUpsertReturnNewDocument_NoMatches()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();
                var inserted = await InsertSomeData(collection, 3);

                var replacement = new Foo("this is update!", 999);

                var filter = BsonDocument.Parse("{ LongValue: { $gte: 5} }");

                var result = await collection.FindOneAndReplaceAsync(filter, replacement, upsert: true, returnNewDocument: true);
                Assert.That(result, Is.EqualTo(replacement));

                // Update inserted with expected values after the update
                inserted = inserted.Concat(new[] { replacement }).ToArray();

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_FindOneAndReplace_FilterSortProjectionUpsertReturnNewDocument()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();
                var inserted = await InsertSomeData(collection, 3);

                var replacement = Foo.WithoutId("this is update!", 999);
                var sort = BsonDocument.Parse("{ LongValue: -1 }");
                var projection = BsonDocument.Parse("{ StringValue: 1, _id: 0 }");
                var filter = BsonDocument.Parse("{ LongValue: { $gte: 1} }");

                var result = await collection.FindOneAndReplaceAsync(filter, replacement, sort, projection, upsert: true, returnNewDocument: true);
                Assert.That(result.StringValue, Is.EqualTo(replacement.StringValue));
                Assert.That(result.LongValue, Is.EqualTo(default(long)));
                Assert.That(result.Id, Is.EqualTo(default(ObjectId)));

                inserted[2].StringValue = replacement.StringValue;
                inserted[2].LongValue = replacement.LongValue;

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_FindOneAndDelete()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();
                var inserted = await InsertSomeData(collection, 3);

                var result = await collection.FindOneAndDeleteAsync();
                Assert.That(result, Is.EqualTo(inserted[0]));

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted.Where((_, i) => i != 0)));
            });
        }

        [Test]
        public void MongoCollection_FindOneAndDelete_Filter()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();
                var inserted = await InsertSomeData(collection, 3);

                var filter = BsonDocument.Parse("{ LongValue: { $gte: 1} }");

                var result = await collection.FindOneAndDeleteAsync(filter);
                Assert.That(result, Is.EqualTo(inserted[1]));

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted.Where((_, i) => i != 1)));
            });
        }

        [Test]
        public void MongoCollection_FindOneAndDelete_Filter_NoMatches()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();
                var inserted = await InsertSomeData(collection, 3);

                var filter = BsonDocument.Parse("{ LongValue: { $gte: 5} }");

                var result = await collection.FindOneAndDeleteAsync(filter);
                Assert.That(result, Is.Null);

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted));
            });
        }

        [Test]
        public void MongoCollection_FindOneAndDelete_Sort()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();
                var inserted = await InsertSomeData(collection, 3);

                var sort = BsonDocument.Parse("{ LongValue: -1 }");

                var result = await collection.FindOneAndDeleteAsync(sort: sort);
                Assert.That(result, Is.EqualTo(inserted[2]));

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted.Where((_, i) => i != 2)));
            });
        }

        [Test]
        public void MongoCollection_FindOneAndDelete_FilterSort()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();
                var inserted = await InsertSomeData(collection, 3);

                var sort = BsonDocument.Parse("{ LongValue: -1 }");
                var filter = BsonDocument.Parse("{ LongValue: { $lt: 2 } }");

                var result = await collection.FindOneAndDeleteAsync(filter, sort: sort);
                Assert.That(result, Is.EqualTo(inserted[1]));

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted.Where((_, i) => i != 1)));
            });
        }

        [Test]
        public void MongoCollection_FindOneAndDelete_FilterSortProjection()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection();
                var inserted = await InsertSomeData(collection, 3);

                var sort = BsonDocument.Parse("{ LongValue: -1 }");
                var filter = BsonDocument.Parse("{ LongValue: { $lt: 2 } }");
                var projection = BsonDocument.Parse("{ LongValue: 1 }");

                var result = await collection.FindOneAndDeleteAsync(filter, sort, projection);
                Assert.That(result.Id, Is.EqualTo(inserted[1].Id));
                Assert.That(result.StringValue, Is.Null);
                Assert.That(result.LongValue, Is.EqualTo(inserted[1].LongValue));

                var docs = await collection.FindAsync();
                Assert.That(docs, Is.EquivalentTo(inserted.Where((_, i) => i != 1)));
            });
        }

        private static async Task<Foo[]> InsertSomeData(MongoClient.Collection<Foo> collection, int documentCount)
        {
            var docs = Enumerable.Range(0, documentCount)
                                 .Select(i => new Foo("Document #" + i, i))
                                 .ToArray();

            await collection.InsertManyAsync(docs);

            var remoteCount = await collection.CountAsync();
            Assert.That(remoteCount, Is.EqualTo(documentCount));

            return docs;
        }

        private static async Task InsertSalesData(MongoClient.Collection<Sale> collection)
        {
            // Example from https://docs.mongodb.com/manual/reference/operator/aggregation/sum/
            var sales = new[]
            {
                new Sale(1, "abc", 10, 2,  new DateTime(2014, 1, 1,  8, 0, 0, DateTimeKind.Utc)),
                new Sale(2, "jkl", 20, 1,  new DateTime(2014, 2, 3,  9, 0, 0, DateTimeKind.Utc)),
                new Sale(3, "xyz", 5,  5,  new DateTime(2014, 2, 3,  9, 5, 0, DateTimeKind.Utc)),
                new Sale(4, "abc", 10, 10, new DateTime(2014, 2, 15, 8, 0, 0, DateTimeKind.Utc)),
                new Sale(5, "xyz", 5,  10, new DateTime(2014, 2, 15, 9, 5, 0, DateTimeKind.Utc)),
            };

            await collection.InsertManyAsync(sales);
        }

        private static BsonDocument GetSalesAggregation()
        {
            return BsonDocument.Parse(@"{ $group: {
                _id: { Day: { $dayOfYear: ""$Date""}, Year: { $year: ""$Date"" } },
                TotalAmount: { $sum: { $multiply:[ ""$Price"", ""$Quantity"" ] } },
                Count: { $sum: 1 }
            }}");
        }

        private async Task<MongoClient.Collection<Foo>> GetCollection()
        {
            var user = await GetUserAsync();
            var client = user.GetMongoClient(ServiceName);
            var db = client.GetDatabase(DbName);
            var collection = db.GetCollection<Foo>(FoosCollectionName);

            await collection.DeleteManyAsync();

            return collection;
        }

        private async Task<MongoClient.Collection<Sale>> GetSalesCollection()
        {
            var user = await GetUserAsync();
            var client = user.GetMongoClient(ServiceName);
            var db = client.GetDatabase(DbName);
            var collection = db.GetCollection<Sale>(SalesCollectionName);

            await collection.DeleteManyAsync();

            return collection;
        }

        private async Task<MongoClient.Collection<BsonDocument>> GetBsonCollection()
        {
            var user = await GetUserAsync();
            var client = user.GetMongoClient(ServiceName);
            var db = client.GetDatabase(DbName);
            var collection = db.GetCollection(FoosCollectionName);

            await collection.DeleteManyAsync();

            return collection;
        }

        private class Foo
        {
            [BsonElement("_id")]
            [BsonIgnoreIfDefault]
            public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

            public string StringValue { get; set; }

            public long LongValue { get; set; }

            public Foo(string stringValue, long longValue)
            {
                StringValue = stringValue;
                LongValue = longValue;
            }

            public static Foo WithoutId(string stringValue, long longValue)
            {
                var result = new Foo(stringValue, longValue);
                result.Id = default;
                return result;
            }

            public override bool Equals(object obj) =>
                (obj is Foo foo) &&
                foo.Id == Id &&
                foo.StringValue == StringValue &&
                foo.LongValue == LongValue;

            public override string ToString()
            {
                return $"Id: {Id}, StringValue: {StringValue}, LongValue: {LongValue}";
            }
        }

        private class Sale
        {
            [BsonElement("_id")]
            public int Id { get; set; }

            public string Item { get; set; }

            public decimal Price { get; set; }

            public decimal Quantity { get; set; }

            public DateTime Date { get; set; }

            public Sale(int id, string item, decimal price, decimal quantity, DateTime date)
            {
                Id = id;
                Item = item;
                Price = price;
                Quantity = quantity;
                Date = date;
            }
        }

        private class AggregationResult
        {
            [BsonElement("_id")]
            public IdResult Id { get; set; }

            public decimal TotalAmount { get; set; }

            public int Count { get; set; }

            public class IdResult
            {
                public int Day { get; set; }

                public int Year { get; set; }
            }
        }
    }
}
