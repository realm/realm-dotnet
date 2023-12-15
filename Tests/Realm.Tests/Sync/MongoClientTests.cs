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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Baas;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NUnit.Framework;
using Realms.Schema;
using Realms.Sync;
using Realms.Sync.Exceptions;

using static Realms.Tests.TestHelpers;

namespace Realms.Tests.Sync
{
    [TestFixture, Preserve(AllMembers = true)]
    public class MongoClientTests : SyncTestBase
    {
        private const string ServiceName = "BackingDB";
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
            var db = client.GetDatabase(SyncTestHelpers.RemoteMongoDBName());

            Assert.That(db.Name, Is.EqualTo(SyncTestHelpers.RemoteMongoDBName()));
            Assert.That(db.Client.ServiceName, Is.EqualTo("foo-bar"));
        }

        [Test]
        public void MongoCollection_Name_ReturnsOriginalName()
        {
            var user = GetFakeUser();
            var client = user.GetMongoClient("foo-bar");
            var db = client.GetDatabase(SyncTestHelpers.RemoteMongoDBName());
            var collection = db.GetCollection("foos");

            Assert.That(collection.Name, Is.EqualTo("foos"));
            Assert.That(collection.Database?.Name, Is.EqualTo(SyncTestHelpers.RemoteMongoDBName()));
            Assert.That(collection.Database?.Client.ServiceName, Is.EqualTo("foo-bar"));
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

                await TestHelpers.AssertThrows<ArgumentNullException>(() => collection.InsertOneAsync(null!));
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

                await TestHelpers.AssertThrows<ArgumentNullException>(() => collection.InsertManyAsync(null!));
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

                var ex = await TestHelpers.AssertThrows<ArgumentException>(() => collection.InsertManyAsync(foos!));
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

                var ex = await TestHelpers.AssertThrows<ArgumentNullException>(() => collection.UpdateOneAsync(filter: null, updateDocument: null!));
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

                var ex = await TestHelpers.AssertThrows<ArgumentNullException>(() => collection.UpdateManyAsync(filter: null, updateDocument: null!));
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

                await InsertSomeData(collection, 3);

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

                await InsertSomeData(collection, 3);

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

                await InsertSomeData(collection, 3);

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

                await InsertSomeData(collection, 3);

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

                var sort = new BsonDocument { { "LongValue", -1 } };
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

                var sort = new BsonDocument { { "StringValue", -1 } };

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

                var projection = new BsonDocument
                {
                    { "_id", 0 },
                    { "StringValue", 1 }
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

                var projection = new BsonDocument
                {
                    { "_id", 0 },
                    { "LongValue", 1 }
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

                var projection = new BsonDocument
                {
                    { "_id", 1 },
                    { "LongValue", 1 }
                };

                var sort = new BsonDocument { { "LongValue", -1 } };

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

                var projection = new BsonDocument
                {
                    { "_id", 0 },
                    { "LongValue", 1 },
                    { "StringValue", 1 }
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

                var sort = new BsonDocument { { "LongValue", -1 } };

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

                var sort = new BsonDocument { { "LongValue", -1 } };
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

                var sort = new BsonDocument { { "StringValue", -1 } };

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

                var projection = new BsonDocument
                {
                    { "_id", 0 },
                    { "StringValue", 1 }
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

#if UNITY
                var projection = new BsonDocument
                {
                    { "_id",  0 },
                    { "LongValue", 1 }
                };
#else
                var projection = new
                {
                    _id = 0,
                    LongValue = 1
                };
#endif

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

#if UNITY
                var sort = new BsonDocument { { "LongValue", -1 } };
                var projection = new BsonDocument
                {
                    { "_id", 1 },
                    { "LongValue", 1 }
                };
#else
                var sort = new { LongValue = -1 };
                var projection = new
                {
                    _id = 1,
                    LongValue = 1
                };
#endif

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

                var filter = new BsonDocument
                {
                    {
                        "LongValue", new BsonDocument
                        {
                            { "$gte", 1 }
                        }
                    }
                };

#if UNITY
                var sort = new BsonDocument { { "LongValue", -1 } };
                var projection = new BsonDocument
                {
                    { "_id", 0 },
                    { "LongValue", 1 },
                    { "StringValue", 1 },
                };
#else
                var sort = new { LongValue = -1 };
                var projection = new
                {
                    _id = 0,
                    LongValue = 1,
                    StringValue = 1
                };
#endif

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

                var sort = new BsonDocument { { "LongValue", -1 } };
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

                var sort = new BsonDocument { { "StringValue", -1 } };

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

                var projection = new BsonDocument
                {
                    { "_id", 0 },
                    { "StringValue", 1 }
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

                var projection = new BsonDocument
                {
                    { "_id", 0 },
                    { "LongValue", 1 }
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

                var projection = new BsonDocument
                {
                    { "_id", 1 },
                    { "LongValue", 1 }
                };

                var sort = new BsonDocument { { "LongValue", -1 } };

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

                var projection = new BsonDocument
                {
                    { "_id", 0 },
                    { "LongValue", 1 },
                    { "StringValue", 1 }
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

                var sort = new BsonDocument { { "LongValue", -1 } };

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

        [Test]
        public void MongoCollection_FindOne_Remapped()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection<RemappedTypeObject>(getSchema: true);
                var inserted = await InsertRemappedData(collection);

                var result = await collection.FindOneAsync();
                Assert.That(result.Id, Is.EqualTo(inserted[0].Id));
                Assert.That(result.StringValue, Is.EqualTo(inserted[0].StringValue));
                Assert.That(result.MappedLink!.Id, Is.EqualTo(inserted[1].Id));
                Assert.That(result.MappedList[0].Id, Is.EqualTo(inserted[2].Id));
            });
        }

        #region Static queries

        [Test]
        public void RealmObjectAPI_Collections()
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var props = SyncCollectionsObject.RealmSchema
                    .Where(p => p.Type.IsCollection(out _) && !p.Type.HasFlag(PropertyType.Object))
                    .ToArray();

                var collection = await GetCollection<SyncCollectionsObject>(AppConfigType.FlexibleSync);
                var obj1 = new SyncCollectionsObject();
                FillCollectionProps(obj1);

                var syncObj2 = new SyncCollectionsObject();
                FillCollectionProps(syncObj2);

                await collection.InsertOneAsync(obj1);

                using var realm = await GetFLXIntegrationRealmAsync();
                var syncObjects = await realm.All<SyncCollectionsObject>().Where(o => o.Id == obj1.Id || o.Id == syncObj2.Id).SubscribeAsync();
                await syncObjects.WaitForEventAsync((sender, _) => sender.Count > 0);

                var syncObj1 = syncObjects.Single();

                AssertProps(obj1, syncObj1);

                realm.Write(() => realm.Add(syncObj2));

                var filter = new { _id = syncObj2.Id };

                var obj2 = await WaitForConditionAsync(() => collection.FindOneAsync(filter), item => Task.FromResult(item != null));

                AssertProps(syncObj2, obj2);

                void FillCollectionProps(SyncCollectionsObject obj)
                {
                    foreach (var prop in props)
                    {
                        DataGenerator.FillCollection(obj.GetProperty<IEnumerable>(prop), 5);
                    }
                }

                void AssertProps(SyncCollectionsObject expected, SyncCollectionsObject actual)
                {
                    foreach (var prop in props)
                    {
                        var expectedProp = expected.GetProperty<IEnumerable>(prop);
                        var actualProp = actual.GetProperty<IEnumerable>(prop);

                        Assert.That(actualProp, Is.EquivalentTo(expectedProp).Using((object a, object e) => AreValuesEqual(a, e)), $"Expected collections to match for {prop.ManagedName}");
                    }
                }
            }, timeout: 120000);
        }

        public static readonly object[] PrimitiveTestCases = new[]
        {
            new object[] { CreateTestCase("Empty object", new SyncAllTypesObject()) },
            new object[]
            {
                CreateTestCase("All values", new SyncAllTypesObject
                {
                    BooleanProperty = true,
                    ByteArrayProperty = GetBytes(5),
                    ByteProperty = 255,
                    CharProperty = 'C',
                    DateTimeOffsetProperty = new DateTimeOffset(638380790696454240, TimeSpan.Zero),
                    Decimal128Property = 4932.539258328M,
                    DecimalProperty = 4884884883.99999999999M,
                    DoubleProperty = 34934.123456,
                    GuidProperty = Guid.NewGuid(),
                    Int16Property = 999,
                    Int32Property = 49394939,
                    Int64Property = 889898965342443,
                    ObjectIdProperty = ObjectId.GenerateNewId(),
                    RealmValueProperty = "this is a string",
                    StringProperty = "foo bar"
                })
            },
            new object[] { CreateTestCase("Bool RealmValue", new SyncAllTypesObject { RealmValueProperty = true }) },
            new object[] { CreateTestCase("Int RealmValue", new SyncAllTypesObject { RealmValueProperty = 123 }) },
            new object[] { CreateTestCase("Long RealmValue", new SyncAllTypesObject { RealmValueProperty = 9999999999 }) },
            new object[] { CreateTestCase("Null RealmValue", new SyncAllTypesObject { RealmValueProperty = RealmValue.Null }) },
            new object[] { CreateTestCase("String RealmValue", new SyncAllTypesObject { RealmValueProperty = "abc" }) },
            new object[] { CreateTestCase("Data RealmValue", new SyncAllTypesObject { RealmValueProperty = GetBytes(10) }) },
            new object[] { CreateTestCase("Float RealmValue", new SyncAllTypesObject { RealmValueProperty = 15.2f }) },
            new object[] { CreateTestCase("Double RealmValue", new SyncAllTypesObject { RealmValueProperty = -123.45678909876 }) },
            new object[] { CreateTestCase("Decimal RealmValue", new SyncAllTypesObject { RealmValueProperty = 1.1111111111111111111M }) },
            new object[] { CreateTestCase("Decimal RealmValue", new SyncAllTypesObject { RealmValueProperty = 1.1111111111111111111M }) },
            new object[] { CreateTestCase("Decimal128 RealmValue", new SyncAllTypesObject { RealmValueProperty = new Decimal128(2.1111111111111111111M) }) },
            new object[] { CreateTestCase("ObjectId RealmValue", new SyncAllTypesObject { RealmValueProperty = ObjectId.GenerateNewId() }) },
            new object[] { CreateTestCase("Guid RealmValue", new SyncAllTypesObject { RealmValueProperty = Guid.NewGuid() }) },
        };

        [TestCaseSource(nameof(PrimitiveTestCases))]
        public void RealmObjectAPI_Primitive_AtlasToRealm(TestCaseData<SyncAllTypesObject> testCase)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var props = SyncAllTypesObject.RealmSchema
                    .Where(p => !p.Type.HasFlag(PropertyType.Object))
                    .ToArray();

                var collection = await GetCollection<SyncAllTypesObject>(AppConfigType.FlexibleSync);
                var obj = testCase.Value;

                await collection.InsertOneAsync(obj);

                using var realm = await GetFLXIntegrationRealmAsync();
                var syncObjects = await realm.All<SyncAllTypesObject>().Where(o => o.Id == obj.Id).SubscribeAsync();
                await syncObjects.WaitForEventAsync((sender, _) => sender.Count > 0);

                var syncObj = syncObjects.Single();

                AssertProps(props, obj, syncObj);
            }, timeout: 120000);
        }

        [TestCaseSource(nameof(PrimitiveTestCases))]
        public void RealmObjectAPI_Primitive_RealmToAtlas(TestCaseData<SyncAllTypesObject> testCase)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var props = SyncAllTypesObject.RealmSchema
                    .Where(p => !p.Type.HasFlag(PropertyType.Object))
                    .ToArray();

                var collection = await GetCollection<SyncAllTypesObject>(AppConfigType.FlexibleSync);
                var obj = testCase.Value;

                using var realm = await GetFLXIntegrationRealmAsync();
                await realm.All<SyncAllTypesObject>().Where(o => o.Id == obj.Id).SubscribeAsync();
                realm.Write(() => realm.Add(obj));

                var filter = new { _id = obj.Id };

                var syncObj = await WaitForConditionAsync(() => collection.FindOneAsync(filter), item => Task.FromResult(item != null));

                AssertProps(props, obj, syncObj);
            }, timeout: 120000);
        }

        public static readonly object[] CounterTestCases = new[]
        {
            new object[]
            {
                CreateTestCase("All values", new CounterObject
                {
                    Id = 1,
                    ByteProperty = 255,
                    Int16Property = 999,
                    Int32Property = 49394939,
                    Int64Property = 889898965342443,
                    NullableByteProperty = 255,
                    NullableInt16Property = 999,
                    NullableInt32Property = 49394939,
                    NullableInt64Property = 889898965342443
                })
            },
            new object[]
            {
                CreateTestCase("Nullable values", new CounterObject
                {
                    Id = 2,
                    ByteProperty = 255,
                    Int16Property = 999,
                    Int32Property = 49394939,
                    Int64Property = 889898965342443,
                    NullableByteProperty = null,
                    NullableInt16Property = null,
                    NullableInt32Property = null,
                    NullableInt64Property = null,
                })
            },
        };

        [TestCaseSource(nameof(CounterTestCases))]
        public void RealmObjectAPI_Counter_AtlasToRealm(TestCaseData<CounterObject> testCase)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var props = CounterObject.RealmSchema.ToArray();

                var collection = await GetCollection<CounterObject>(AppConfigType.FlexibleSync);
                var obj = testCase.Value;

                await collection.InsertOneAsync(obj);

                using var realm = await GetFLXIntegrationRealmAsync();
                var syncObjects = await realm.All<CounterObject>().Where(o => o.Id == obj.Id).SubscribeAsync();
                await syncObjects.WaitForEventAsync((sender, _) => sender.Count > 0);

                var syncObj = syncObjects.Single();

                AssertProps(props, obj, syncObj);
            }, timeout: 120000);
        }

        [TestCaseSource(nameof(CounterTestCases))]
        public void RealmObjectAPI_Counter_RealmToAtlas(TestCaseData<CounterObject> testCase)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var props = CounterObject.RealmSchema.ToArray();

                var collection = await GetCollection<CounterObject>(AppConfigType.FlexibleSync, getSchema: true);
                var obj = testCase.Value;

                using var realm = await GetFLXIntegrationRealmAsync();
                await realm.All<CounterObject>().Where(o => o.Id == obj.Id).SubscribeAsync();
                realm.Write(() => realm.Add(obj));

                var filter = new { _id = obj.Id };

                var syncObj = await WaitForConditionAsync(() => collection.FindOneAsync(filter), item => Task.FromResult(item != null));

                AssertProps(props, obj, syncObj);
            }, timeout: 120000);
        }

        public static readonly object[] AsymmetricTestCases = new[]
        {
            new object[]
            {
                CreateTestCase("Base", new BasicAsymmetricObject { PartitionLike = "testString" })
            },
        };

        [TestCaseSource(nameof(AsymmetricTestCases))]
        public void RealmObjectAPI_Asymmetric_RealmToAtlas(TestCaseData<BasicAsymmetricObject> testCase)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection<BasicAsymmetricObject>(AppConfigType.FlexibleSync, getSchema: true);
                var obj = testCase.Value;
                var stringProperty = obj.PartitionLike;

                var filter = new { _id = obj.Id };

                using var realm = await GetFLXIntegrationRealmAsync();
                realm.Write(() => realm.Add(obj));

                var syncObj = await WaitForConditionAsync(() => collection.FindOneAsync(filter), item => Task.FromResult(item != null));

                Assert.That(stringProperty, Is.EqualTo(syncObj.PartitionLike));
            }, timeout: 120000);
        }

        public static readonly object[] ObjectTestCases = new[]
        {
            new object[]
            {
                CreateTestCase("All values", new SyncAllTypesObject
                {
                    ObjectProperty = new IntPropertyObject { Int = 23 },
                })
            },
        };

        // TODO We could remove this and the following test, as they are covered by the link tests
        // The only difference is that here we insert objects as they are needed.
        [TestCaseSource(nameof(ObjectTestCases))]
        public void RealmObjectAPI_Object_AtlasToRealm(TestCaseData<SyncAllTypesObject> testCase)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var syncAllTypesCollection = await GetCollection<SyncAllTypesObject>(AppConfigType.FlexibleSync);
                var intPropertyCollection = await GetCollection<IntPropertyObject>(AppConfigType.FlexibleSync);

                var obj = testCase.Value;

                await syncAllTypesCollection.InsertOneAsync(obj);

                using var realm = await GetFLXIntegrationRealmAsync();
                var syncAllTypesObjects = await realm.All<SyncAllTypesObject>().Where(o => o.Id == obj.Id).SubscribeAsync();
                var intPropertyObjects = await realm.All<IntPropertyObject>().Where(o => o.Id == obj.ObjectProperty!.Id).SubscribeAsync();

                await syncAllTypesObjects.WaitForEventAsync((sender, _) => sender.Count > 0);

                var syncObj = syncAllTypesObjects.Single();

                // The object property is null, because we didn't add the object yet to Atlas
                Assert.That(syncObj.ObjectProperty, Is.Null);

                await intPropertyCollection.InsertOneAsync(obj.ObjectProperty!);
                await intPropertyObjects.WaitForEventAsync((sender, _) => sender.Count > 0);

                Assert.That(syncObj.ObjectProperty!.Id, Is.EqualTo(obj.ObjectProperty!.Id));
                Assert.That(syncObj.ObjectProperty!.Int, Is.EqualTo(obj.ObjectProperty!.Int));
            }, timeout: 120000);
        }

        [TestCaseSource(nameof(ObjectTestCases))]
        public void RealmObjectAPI_Object_RealmToAtlas(TestCaseData<SyncAllTypesObject> testCase)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var syncAllTypesCollection = await GetCollection<SyncAllTypesObject>(AppConfigType.FlexibleSync, getSchema: true);
                var intPropertyCollection = await GetCollection<IntPropertyObject>(AppConfigType.FlexibleSync);

                var obj = testCase.Value;

                using var realm = await GetFLXIntegrationRealmAsync();
                await realm.All<SyncAllTypesObject>().Where(o => o.Id == obj.Id).SubscribeAsync();
                await realm.All<IntPropertyObject>().Where(o => o.Id == obj.ObjectProperty!.Id).SubscribeAsync();
                realm.Write(() => realm.Add(obj));

                var syncAllTypeObj = await WaitForConditionAsync(() => syncAllTypesCollection.FindOneAsync(new { _id = obj.Id }), item => Task.FromResult(item != null));
                var intPropertyObj = await WaitForConditionAsync(() => intPropertyCollection.FindOneAsync(new { _id = obj.ObjectProperty!.Id }), item => Task.FromResult(item != null));

                Assert.That(syncAllTypeObj.ObjectProperty!.Id, Is.EqualTo(obj.ObjectProperty!.Id));
                Assert.That(syncAllTypeObj.ObjectProperty!.Int, Is.Not.EqualTo(obj.ObjectProperty!.Int));

                Assert.That(intPropertyObj.Id, Is.EqualTo(obj.ObjectProperty.Id));
                Assert.That(intPropertyObj.Int, Is.EqualTo(obj.ObjectProperty.Int));
            }, timeout: 120000);
        }

        //TODO Maybe we can have only one test here, we don't need all these cases
        public static readonly object[] LinksTestCases = new[]
        {
            new object[]
            {
                CreateTestCase("Single link", new LinksObject("first")
                {
                    Link = new("second") { Value = 2 },
                    Value = 1,
                }),
            },
            new object[]
            {
                CreateTestCase("List", new LinksObject("first")
                {
                    List =
                    {
                        new("list.1") { Value = 100 },
                        new("list.2") { Value = 200 },
                    },
                    Value = 987
                }),
            },
            new object[]
            {
                CreateTestCase("Dictionary", new LinksObject("first")
                {
                    Dictionary =
                    {
                        ["key_1"] = new("dict.1") { Value = 100 },
                        ["key_null"] = null,
                        ["key_2"] = new("dict.2") { Value = 200 },
                    },
                    Value = 999
                })
            },
            new object[]
            {
                CreateTestCase("Set", new LinksObject("first")
                {
                    Set =
                    {
                        new("list.1") { Value = 100 },
                        new("list.2") { Value = 200 },
                    },
                    Value = 123
                }),
            },
            new object[]
            {
                CreateTestCase("All types", new LinksObject("parent")
                {
                    Value = 1,
                    Link = new("link") { Value = 2 },
                    List =
                    {
                        new("list.1") { Value = 3 },
                        new("list.2") { Value = 4 },
                    },
                    Set =
                    {
                        new("set.1") { Value = 5 },
                        new("set.2") { Value = 6 },
                    },
                    Dictionary =
                    {
                        ["dict_1"] = new("dict.1") { Value = 7 },
                        ["dict_2"] = new("dict.2") { Value = 8 },
                        ["dict_null"] = null
                    }
                }),
            }
        };

        [TestCaseSource(nameof(LinksTestCases))]
        public void RealmObjectAPI_Links_AtlasToRealm(TestCaseData<LinksObject> testCase)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection<LinksObject>(AppConfigType.FlexibleSync);

                var obj = testCase.Value;

                var elementsToInsert = obj.List.Concat(obj.Set).Concat(obj.Dictionary.Values.Where(d => d is not null)).Concat(new[] { obj });

                if (obj.Link is not null)
                {
                    elementsToInsert = elementsToInsert.Concat(new[] { obj.Link });
                }

                await collection.InsertManyAsync(elementsToInsert!);

                // How many objects we expect
                var totalCount = obj.List.Count + obj.Set.Count + obj.Dictionary.Count + 1;

                using var realm = await GetFLXIntegrationRealmAsync();
                var linkObjs = await realm.All<LinksObject>().SubscribeAsync();

                await linkObjs.WaitForEventAsync((sender, _) => sender.Count >= totalCount);

                var linkObj = realm.Find<LinksObject>(obj.Id);

                AssertEqual(linkObj!.Link, obj.Link);

                Assert.That(linkObj.List.Count, Is.EqualTo(obj.List.Count));

                for (int i = 0; i < linkObj.List.Count; i++)
                {
                    AssertEqual(linkObj.List[i], obj.List[i]);
                }

                Assert.That(linkObj.Dictionary.Count, Is.EqualTo(obj.Dictionary.Count));

                foreach (var key in obj.Dictionary.Keys)
                {
                    Assert.That(linkObj.Dictionary.ContainsKey(key));
                    AssertEqual(linkObj.Dictionary[key], obj.Dictionary[key]);
                }

                Assert.That(linkObj.Set.Count, Is.EqualTo(obj.Set.Count));

                var orderedOriginalSet = obj.Set.OrderBy(a => a.Id).ToList();
                var orderedRetrievedSet = linkObj.Set.OrderBy(a => a.Id).ToList();

                for (int i = 0; i < orderedOriginalSet.Count; i++)
                {
                    AssertEqual(orderedRetrievedSet[i], orderedOriginalSet[i]);
                }

                static void AssertEqual(LinksObject? retrieved, LinksObject? original)
                {
                    if (original is null)
                    {
                        Assert.That(retrieved, Is.Null);
                    }
                    else
                    {
                        Assert.That(retrieved, Is.Not.Null);
                        Assert.That(retrieved!.Id, Is.EqualTo(original!.Id));
                        Assert.That(retrieved!.Value, Is.EqualTo(original!.Value));
                    }
                }
            }, timeout: 120000);
        }

        [TestCaseSource(nameof(LinksTestCases))]
        public void RealmObjectAPI_Links_RealmToAtlas(TestCaseData<LinksObject> testCase)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var collection = await GetCollection<LinksObject>(AppConfigType.FlexibleSync);

                var obj = testCase.Value;

                using var realm = await GetFLXIntegrationRealmAsync();
                await realm.All<LinksObject>().SubscribeAsync();

                realm.Write(() => realm.Add(obj));
                await WaitForUploadAsync(realm);

                var linkObj = await WaitForConditionAsync(() => collection.FindOneAsync(new { _id = obj.Id }), item => Task.FromResult(item != null));

                await AssertEqual(collection, linkObj.Link, obj.Link);

                for (int i = 0; i < linkObj.List.Count; i++)
                {
                    await AssertEqual(collection, linkObj.List[i], obj.List[i]);
                }

                Assert.That(linkObj.Dictionary.Count, Is.EqualTo(obj.Dictionary.Count));

                foreach (var key in obj.Dictionary.Keys)
                {
                    Assert.That(linkObj.Dictionary.ContainsKey(key));
                    await AssertEqual(collection, linkObj.Dictionary[key], obj.Dictionary[key]);
                }

                Assert.That(linkObj.Set.Count, Is.EqualTo(obj.Set.Count));

                var orderedOriginalSet = obj.Set.OrderBy(a => a.Id).ToList();
                var orderedRetrievedSet = linkObj.Set.OrderBy(a => a.Id).ToList();

                for (int i = 0; i < orderedOriginalSet.Count; i++)
                {
                    await AssertEqual(collection, orderedRetrievedSet[i], orderedOriginalSet[i]);
                }

                static async Task AssertEqual(MongoClient.Collection<LinksObject> collection, LinksObject? partiallyRetrieved, LinksObject? original)
                {
                    if (original is null)
                    {
                        Assert.That(partiallyRetrieved, Is.Null);
                        return;
                    }

                    // The partiallyRetrieved object should contain only the id, and not other fields
                    Assert.That(partiallyRetrieved, Is.Not.Null);
                    Assert.That(partiallyRetrieved!.Id, Is.EqualTo(original.Id));
                    Assert.That(partiallyRetrieved.Value, Is.Not.EqualTo(original.Value));

                    var fullyRetrieved = await WaitForConditionAsync(() => collection.FindOneAsync(new { _id = original.Id }), item => Task.FromResult(item != null));

                    Assert.That(fullyRetrieved.Id, Is.EqualTo(original.Id));
                    Assert.That(fullyRetrieved.Value, Is.EqualTo(original.Value));
                }

            }, timeout: 120000);
        }


        private void AssertProps(IEnumerable<Property> props, IRealmObjectBase expected, IRealmObjectBase actual)
        {
            foreach (var prop in props)
            {
                var expectedProp = expected.GetProperty<object>(prop);
                var actualProp = actual.GetProperty<object>(prop);

                AssertAreEqual(actualProp, expectedProp, $"property: {prop.Name}");
            }
        }

        #endregion

        private static async Task<RemappedTypeObject[]> InsertRemappedData(MongoClient.Collection<RemappedTypeObject> collection)
        {
            const int documentCount = 3;

            var docs = Enumerable.Range(0, documentCount)
                .Select(i => new RemappedTypeObject
                {
                    Id = ObjectId.GenerateNewId().GetHashCode(),
                    StringValue = $"Doc #{i}",
                })
                .ToArray();

            docs[0].MappedLink = docs[1];
            docs[0].MappedList.Add(docs[2]);

            await collection.InsertManyAsync(docs);

            var remoteCount = await collection.CountAsync();
            Assert.That(remoteCount, Is.EqualTo(documentCount));

            return docs;
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
            var db = client.GetDatabase(SyncTestHelpers.RemoteMongoDBName());
            var collection = db.GetCollection<Foo>(FoosCollectionName);

            await collection.DeleteManyAsync();

            return collection;
        }

        // Retrieves the MongoClient.Collection for a specific object type and removes everything that's eventually there already
        private async Task<MongoClient.Collection<T>> GetCollection<T>(string appConfigType = AppConfigType.Default, bool getSchema = false)
            where T : class, IRealmObjectBase
        {
            var app = App.Create(SyncTestHelpers.GetAppConfig(appConfigType));
            var user = await GetUserAsync(app);

            // Use sync to create the schema/rules
            SyncConfigurationBase config = appConfigType == AppConfigType.FlexibleSync ? GetFLXIntegrationConfig(user) : GetIntegrationConfig(user);

            using var realm = await GetRealmAsync(config);
            var client = user.GetMongoClient(ServiceName);
            var collection = client.GetCollection<T>();
            await collection.DeleteManyAsync(new object());

            return collection;
        }

        private async Task<MongoClient.Collection<Sale>> GetSalesCollection()
        {
            var user = await GetUserAsync();
            var client = user.GetMongoClient(ServiceName);
            var db = client.GetDatabase(SyncTestHelpers.RemoteMongoDBName());
            var collection = db.GetCollection<Sale>(SalesCollectionName);

            await collection.DeleteManyAsync();

            return collection;
        }

        private async Task<MongoClient.Collection<BsonDocument>> GetBsonCollection()
        {
            var user = await GetUserAsync();
            var client = user.GetMongoClient(ServiceName);
            var db = client.GetDatabase(SyncTestHelpers.RemoteMongoDBName());
            var collection = db.GetCollection(FoosCollectionName);

            await collection.DeleteManyAsync();

            return collection;
        }

        private class Foo
        {
            [BsonElement("_id")]
            [BsonIgnoreIfDefault]
            [Preserve]
            public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

            public string? StringValue { get; set; }

            public long LongValue { get; set; }

            public Foo(string? stringValue, long longValue)
            {
                StringValue = stringValue;
                LongValue = longValue;
            }

            public static Foo WithoutId(string? stringValue, long longValue) => new(stringValue, longValue)
            {
                Id = default
            };

            public override bool Equals(object? obj) =>
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
            [Preserve]
            [BsonElement("_id")]
            public int Id { get; set; }

            [Preserve]
            public string Item { get; set; }

            [Preserve]
            public decimal Price { get; set; }

            [Preserve]
            public decimal Quantity { get; set; }

            [Preserve]
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
            [Preserve]
            [BsonElement("_id")]
            public IdResult Id { get; set; } = null!;

            [Preserve]
            public decimal TotalAmount { get; set; }

            [Preserve]
            public int Count { get; set; }

            public class IdResult
            {
                [Preserve]
                public int Day { get; set; }

                [Preserve]
                public int Year { get; set; }
            }
        }
    }
}
