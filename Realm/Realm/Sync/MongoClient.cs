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

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Realms.Helpers;

namespace Realms.Sync
{
    /// <summary>
    /// The remote MongoClient used for working with data in MongoDB remotely via Realm.
    /// </summary>
    public class MongoClient
    {
        internal User User { get; }

        /// <summary>
        /// Gets the service name for this client.
        /// </summary>
        /// <value>The name of the remote MongoDB service.</value>
        public string ServiceName { get; }

        internal MongoClient(User user, string serviceName)
        {
            User = user;
            ServiceName = serviceName;
        }

        /// <summary>
        /// Gets a <see cref="Database"/> instance for the given database name.
        /// </summary>
        /// <param name="name">The name of the database to retrieve.</param>
        /// <returns>A <see cref="Database"/> instance that exposes an API for querying its collections.</returns>
        public Database GetDatabase(string name)
        {
            Argument.Ensure(IsNameValid(name), "Database names must be non-empty and not contain '.' or the null character.", nameof(name));

            return new Database(this, name);
        }

        internal static bool IsNameValid(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            var index = name.IndexOfAny(new[] { '\0', '.' });
            return index == -1;
        }

        /// <summary>
        /// An object representing a remote MongoDB database.
        /// </summary>
        public class Database
        {
            /// <summary>
            /// Gets the <see cref="MongoClient"/> that manages this database.
            /// </summary>
            /// <value>The database's <see cref="MongoClient"/>.</value>
            public MongoClient Client { get; }

            /// <summary>
            /// Gets the name of the database.
            /// </summary>
            /// <value>The database name.</value>
            public string Name { get; }

            internal Database(MongoClient client, string name)
            {
                Client = client;
                Name = name;
            }

            /// <summary>
            /// Gets a collection from the database.
            /// </summary>
            /// <param name="name">The name of the collection.</param>
            /// <returns>A <see cref="Collection{BsonDocument}"/> instance that exposes an API for CRUD operations on its contents.</returns>
            public Collection<BsonDocument> GetCollection(string name) => GetCollection<BsonDocument>(name);

            /// <summary>
            /// Gets a collection from the database.
            /// </summary>
            /// <remarks>
            /// The <see href="https://mongodb.github.io/mongo-csharp-driver/2.11/">MongoDB Bson</see> library is used
            /// to decode the response. It will automatically handle most cases, but if you want to control the behavior
            /// of the deserializer, you can use the attributes in the
            /// <see href="https://mongodb.github.io/mongo-csharp-driver/2.11/apidocs/html/N_MongoDB_Bson_Serialization_Attributes.htm">MongoDB.Bson.Serialization.Attributes</see>
            /// namespace.
            /// <br/>
            /// If you want to modify the global conventions used when deserializing the response, such as convert
            /// camelCase properties to PascalCase, you can register a
            /// <see href="https://mongodb.github.io/mongo-csharp-driver/2.11/reference/bson/mapping/conventions/">ConventionPack</see>.
            /// </remarks>
            /// <typeparam name="TDocument">The managed type that matches the shape of the documents in the collection.</typeparam>
            /// <param name="name">The name of the collection.</param>
            /// <returns>A <see cref="Collection{TDocument}"/> instance that exposes an API for CRUD operations on its contents.</returns>
            public Collection<TDocument> GetCollection<TDocument>(string name)
                where TDocument : class
            {
                Argument.Ensure(IsNameValid(name), "Collection names must be non-empty and not contain '.' or the null character.", nameof(name));

                return new Collection<TDocument>(this, name);
            }
        }

        /// <summary>
        /// An object representing a remote MongoDB collection.
        /// </summary>
        /// <typeparam name="TDocument">The managed type that matches the shape of the documents in the collection.</typeparam>
        public class Collection<TDocument>
            where TDocument : class
        {
            /// <summary>
            /// Gets the <see cref="Database"/> this collection belongs to.
            /// </summary>
            /// <value>The collection's <see cref="Database"/>.</value>
            public Database Database { get; }

            /// <summary>
            /// Gets the name of the collection.
            /// </summary>
            /// <value>The collection name.</value>
            public string Name { get; }

            internal Collection(Database database, string name)
            {
                Database = database;
                Name = name;
            }

            /// <summary>
            /// Inserts the provided document in the collection.
            /// </summary>
            /// <param name="doc">The document to insert.</param>
            /// <returns>
            /// An awaitable <see cref="Task{T}"/> representing the remote insert operation. The result of the task
            /// contains the <c>_id</c> of the inserted document.
            /// </returns>
            /// <seealso href="https://docs.mongodb.com/manual/reference/method/db.collection.insertOne/"/>
            public async Task<InsertResult> InsertOneAsync(TDocument doc)
            {
                Argument.NotNull(doc, nameof(doc));

                return await InvokeOperationAsync<InsertResult>("insertOne", "document", doc);
            }

            /// <summary>
            /// Inserts one or more documents in the collection.
            /// </summary>
            /// <param name="docs">The documents to insert.</param>
            /// <returns>
            /// An awaitable <see cref="Task{T}"/> representing the remote insert many operation. The result of the task
            /// contains the <c>_id</c>s of the inserted documents.
            /// </returns>
            /// <seealso href="https://docs.mongodb.com/manual/reference/method/db.collection.insertMany/"/>
            public async Task<InsertManyResult> InsertManyAsync(IEnumerable<TDocument> docs)
            {
                Argument.NotNull(docs, nameof(docs));
                Argument.Ensure(docs.All(d => d != null), "Collection must not contain null elements.", nameof(docs));

                return await InvokeOperationAsync<InsertManyResult>("insertMany", "documents", docs);
            }

            /// <summary>
            /// Updates a single document in the collection according to the specified arguments.
            /// </summary>
            /// <param name="filter">
            /// A document describing the selection criteria of the update. If not specified, the first document in the
            /// collection will be updated. Can only contain
            /// <see href="https://docs.mongodb.com/manual/reference/operator/query/#query-selectors">query selector expressions</see>.
            /// </param>
            /// <param name="updateDocument">
            /// A document describing the update. Can only contain
            /// <see href="https://docs.mongodb.com/manual/reference/operator/update/#id1">update operator expressions</see>.
            /// </param>
            /// <param name="upsert">
            /// A boolean controlling whether the update should insert a document if no documents match the <paramref name="filter"/>.
            /// Defaults to <c>false</c>.
            /// </param>
            /// <returns>
            /// An awaitable <see cref="Task{T}"/> representing the remote update one operation. The result of the task
            /// contains information about the number of matched and updated documents, as well as the <c>_id</c> of the
            /// upserted document if <paramref name="upsert"/> was set to <c>true</c> and the operation resulted in an
            /// upsert.
            /// </returns>
            /// <seealso href="https://docs.mongodb.com/manual/reference/method/db.collection.updateOne/"/>
            public async Task<UpdateResult> UpdateOneAsync(object filter, object updateDocument, bool upsert = false)
            {
                Argument.NotNull(updateDocument, nameof(updateDocument));

                return await InvokeOperationAsync<UpdateResult>("updateOne", "query", filter, "update", updateDocument, "upsert", upsert);
            }

            /// <summary>
            /// Updates one or more documents in the collection according to the specified arguments.
            /// </summary>
            /// <param name="filter">
            /// A document describing the selection criteria of the update. If not specified, all documents in the
            /// collection will be updated. Can only contain
            /// <see href="https://docs.mongodb.com/manual/reference/operator/query/#query-selectors">query selector expressions</see>.
            /// </param>
            /// <param name="updateDocument">
            /// A document describing the update. Can only contain
            /// <see href="https://docs.mongodb.com/manual/reference/operator/update/#id1">update operator expressions</see>.
            /// </param>
            /// <param name="upsert">
            /// A boolean controlling whether the update should insert a document if no documents match the <paramref name="filter"/>.
            /// Defaults to <c>false</c>.
            /// </param>
            /// <returns>
            /// An awaitable <see cref="Task{T}"/> representing the remote update many operation. The result of the task
            /// contains information about the number of matched and updated documents, as well as the <c>_id</c> of the
            /// upserted document if <paramref name="upsert"/> was set to <c>true</c> and the operation resulted in an
            /// upsert.
            /// </returns>
            /// <seealso href="https://docs.mongodb.com/manual/reference/method/db.collection.updateMany/"/>
            public async Task<UpdateResult> UpdateManyAsync(object filter, object updateDocument, bool upsert = false)
            {
                Argument.NotNull(updateDocument, nameof(updateDocument));

                return await InvokeOperationAsync<UpdateResult>("updateMany", "query", filter, "update", updateDocument, "upsert", upsert);
            }

            /// <summary>
            /// Removes a single document from a collection. If no documents match the <paramref name="filter"/>, the collection is not modified.
            /// </summary>
            /// <param name="filter">
            /// A document describing the deletion criteria using <see href="https://docs.mongodb.com/manual/reference/operator/query/">query operators</see>.
            /// If not specified, the first document in the collection will be deleted.
            /// </param>
            /// <returns>
            /// An awaitable <see cref="Task{T}"/> representing the remote delete one operation. The result of the task contains the number
            /// of deleted documents.
            /// </returns>
            /// <seealso href="https://docs.mongodb.com/manual/reference/method/db.collection.deleteOne/"/>
            public Task<DeleteResult> DeleteOneAsync(object filter = null) => InvokeOperationAsync<DeleteResult>("deleteOne", "query", filter);

            /// <summary>
            /// Removes one or more documents from a collection. If no documents match the <paramref name="filter"/>, the collection is not modified.
            /// </summary>
            /// <param name="filter">
            /// A document describing the deletion criteria using <see href="https://docs.mongodb.com/manual/reference/operator/query/">query operators</see>.
            /// If not specified, all documents in the collection will be deleted.
            /// </param>
            /// <returns>
            /// An awaitable <see cref="Task{T}"/> representing the remote delete many operation. The result of the task contains the number
            /// of deleted documents.
            /// </returns>
            /// <seealso href="https://docs.mongodb.com/manual/reference/method/db.collection.deleteMany/"/>
            public Task<DeleteResult> DeleteManyAsync(object filter = null) => InvokeOperationAsync<DeleteResult>("deleteMany", "query", filter);

            /// <summary>
            /// Finds the all documents in the collection up to <paramref name="limit"/>.
            /// </summary>
            /// <param name="filter">
            /// A document describing the find criteria using <see href="https://docs.mongodb.com/manual/reference/operator/query/">query operators</see>.
            /// If not specified, all documents in the collection will be returned.
            /// </param>
            /// <param name="sort">A document describing the sort criteria. If not specified, the order of the returned documents is not guaranteed.</param>
            /// <param name="projection">
            /// A document describing the fields to return for all matching documents. If not specified, all fields are returned.
            /// </param>
            /// <param name="limit">The maximum number of documents to return. If not specified, all documents in the collection are returned.</param>
            /// <returns>
            /// An awaitable <see cref="Task"/> representing the remote find operation. The result of the task is an array containing the documents that match the find criteria.
            /// </returns>
            /// <seealso href="https://docs.mongodb.com/manual/reference/method/db.collection.find/"/>
            public Task<TDocument[]> FindAsync(object filter = null, object sort = null, object projection = null, long? limit = null)
                => InvokeOperationAsync<TDocument[]>("find", "query", filter, "project", projection, "sort", sort, "limit", limit);

            /// <summary>
            /// Finds the first document in the collection that satisfies the query criteria.
            /// </summary>
            /// <param name="filter">
            /// A document describing the find criteria using <see href="https://docs.mongodb.com/manual/reference/operator/query/">query operators</see>.
            /// If not specified, all documents in the collection will match the request.
            /// </param>
            /// <param name="sort">A document describing the sort criteria. If not specified, the order of the returned documents is not guaranteed.</param>
            /// <param name="projection">
            /// A document describing the fields to return for all matching documents. If not specified, all fields are returned.
            /// </param>
            /// <returns>
            /// An awaitable <see cref="Task{T}"/> representing the remote find one operation. The result of the task is the first document that matches the find criteria.
            /// </returns>
            /// <seealso href="https://docs.mongodb.com/manual/reference/method/db.collection.findOne/"/>
            public Task<TDocument> FindOneAsync(object filter = null, object sort = null, object projection = null)
                => InvokeOperationAsync<TDocument>("findOne", "query", filter, "project", projection, "sort", sort);

            /// <summary>
            /// Finds the first document in the collection that satisfies the query criteria.
            /// </summary>
            /// <param name="filter">
            /// A document describing the find criteria using <see href="https://docs.mongodb.com/manual/reference/operator/query/">query operators</see>.
            /// If not specified, all documents in the collection will match the request.
            /// </param>
            /// <param name="updateDocument">
            /// A document describing the update. Can only contain
            /// <see href="https://docs.mongodb.com/manual/reference/operator/update/#id1">update operator expressions</see>.
            /// </param>
            /// <param name="sort">A document describing the sort criteria. If not specified, the order of the returned documents is not guaranteed.</param>
            /// <param name="projection">
            /// A document describing the fields to return for all matching documents. If not specified, all fields are returned.
            /// </param>
            /// <param name="upsert">
            /// A boolean controlling whether the update should insert a document if no documents match the <paramref name="filter"/>.
            /// Defaults to <c>false</c>.
            /// </param>
            /// <param name="returnNewDocument">
            /// A boolean controlling whether to return the new updated document. If set to <c>false</c> the original document
            /// before the update is returned. Defaults to <c>false</c>.
            /// </param>
            /// <returns>
            /// An awaitable <see cref="Task{T}"/> representing the remote find one operation. The result of the task is the first document that matches the find criteria.
            /// </returns>
            /// <seealso href="https://docs.mongodb.com/manual/reference/method/db.collection.findOneAndUpdate/"/>
            public async Task<TDocument> FindOneAndUpdateAsync(object filter, object updateDocument, object sort = null, object projection = null, bool upsert = false, bool returnNewDocument = false)
            {
                Argument.NotNull(updateDocument, nameof(updateDocument));

                return await InvokeOperationAsync<TDocument>("findOneAndUpdate", "filter", filter, "update", updateDocument, "projection", projection, "sort", sort, "upsert", upsert, "returnNewDocument", returnNewDocument);
            }

            /// <summary>
            /// Finds the first document in the collection that satisfies the query criteria.
            /// </summary>
            /// <param name="filter">
            /// A document describing the find criteria using <see href="https://docs.mongodb.com/manual/reference/operator/query/">query operators</see>.
            /// If not specified, all documents in the collection will match the request.
            /// </param>
            /// <param name="replacementDoc">
            /// The replacement document. Cannot contain update operator expressions.
            /// </param>
            /// <param name="sort">
            /// A document describing the sort criteria. If not specified, the order of the returned documents is not guaranteed.
            /// </param>
            /// <param name="projection">
            /// A document describing the fields to return for all matching documents. If not specified, all fields are returned.
            /// </param>
            /// <param name="upsert">
            /// A boolean controlling whether the replace should insert a document if no documents match the <paramref name="filter"/>.
            /// Defaults to <c>false</c>.
            /// <br/>
            /// MongoDB will add the <c>_id</c> field to the replacement document if it is not specified in either the filter or
            /// replacement documents. If <c>_id</c> is present in both, the values must be equal.
            /// </param>
            /// <param name="returnNewDocument">
            /// A boolean controlling whether to return the replacement document. If set to <c>false</c> the original document
            /// before the update is returned. Defaults to <c>false</c>.
            /// </param>
            /// <returns>
            /// An awaitable <see cref="Task{T}"/> representing the remote find one operation. The result of the task is the first document that matches the find criteria.
            /// </returns>
            /// <seealso href="https://docs.mongodb.com/manual/reference/method/db.collection.findOneAndReplace/"/>
            public async Task<TDocument> FindOneAndReplaceAsync(object filter, TDocument replacementDoc, object sort = null, object projection = null, bool upsert = false, bool returnNewDocument = false)
            {
                Argument.NotNull(replacementDoc, nameof(replacementDoc));

                return await InvokeOperationAsync<TDocument>("findOneAndReplace", "filter", filter, "update", replacementDoc, "projection", projection, "sort", sort, "upsert", upsert, "returnNewDocument", returnNewDocument);
            }

            /// <summary>
            /// Finds the first document in the collection that satisfies the query criteria.
            /// </summary>
            /// <param name="filter">
            /// A document describing the find criteria using <see href="https://docs.mongodb.com/manual/reference/operator/query/">query operators</see>.
            /// If not specified, all documents in the collection will match the request.
            /// </param>
            /// <param name="sort">A document describing the sort criteria. If not specified, the order of the returned documents is not guaranteed.</param>
            /// <param name="projection">
            /// A document describing the fields to return for all matching documents. If not specified, all fields are returned.
            /// </param>
            /// <returns>
            /// An awaitable <see cref="Task{T}"/> representing the remote find one operation. The result of the task is the first document that matches the find criteria.
            /// </returns>
            /// <seealso href="https://docs.mongodb.com/manual/reference/method/db.collection.findOneAndDelete/"/>
            public Task<TDocument> FindOneAndDeleteAsync(object filter = null, object sort = null, object projection = null)
                => InvokeOperationAsync<TDocument>("findOneAndDelete", "filter", filter, "projection", projection, "sort", sort);

            /// <summary>
            /// Executes an aggregation pipeline on the collection and returns the results as a <typeparamref name="TProjection"/> array.
            /// </summary>
            /// <typeparam name="TProjection">The managed type that matches the shape of the result of the pipeline.</typeparam>
            /// <param name="pipeline">
            /// Documents describing the different pipeline stages using <see href="https://docs.mongodb.com/manual/core/aggregation-pipeline/#pipeline-expressions">pipeline expressions</see>.
            /// </param>
            /// <returns>
            /// An awaitable <see cref="Task"/> representing the remote aggregate operation. The result of the task is an array containing the documents returned
            /// by executing the aggregation <paramref name="pipeline"/>.
            /// </returns>
            /// <seealso href="https://docs.mongodb.com/manual/aggregation/"/>
            public Task<TProjection[]> AggregateAsync<TProjection>(params object[] pipeline) => InvokeOperationAsync<TProjection[]>("aggregate", "pipeline", pipeline);

            /// <summary>
            /// Executes an aggregation pipeline on the collection and returns the results as a <see cref="BsonDocument"/> array.
            /// </summary>
            /// <param name="pipeline">
            /// Documents describing the different pipeline stages using <see href="https://docs.mongodb.com/manual/core/aggregation-pipeline/#pipeline-expressions">pipeline expressions</see>.
            /// </param>
            /// <returns>
            /// An awaitable <see cref="Task"/> representing the remote aggregate operation. The result of the task is an array containing the documents returned
            /// by executing the aggregation <paramref name="pipeline"/>.
            /// </returns>
            /// <seealso href="https://docs.mongodb.com/manual/aggregation/"/>
            public Task<BsonDocument[]> AggregateAsync(params object[] pipeline) => AggregateAsync<BsonDocument>(pipeline);

            /// <summary>
            /// Counts the number of documents in the collection that match the provided <paramref name="filter"/>.
            /// </summary>
            /// <param name="filter">
            /// A document describing the find criteria using <see href="https://docs.mongodb.com/manual/reference/operator/query/">query operators</see>.
            /// If not specified, all documents in the collection will be counted.
            /// </param>
            /// <param name="limit">The maximum number of documents to count. If not specified, all documents in the collection are counted.</param>
            /// <returns>
            /// An awaitable <see cref="Task"/> representing the remote count operation. The result of the task is the number of documents that match the
            /// <paramref name="filter"/> and <paramref name="limit"/> criteria.
            /// </returns>
            public Task<long> CountAsync(object filter = null, long? limit = null) => InvokeOperationAsync<long>("count", "query", filter, "limit", limit);

            private async Task<T> InvokeOperationAsync<T>(string functionName, params object[] args)
            {
                var jsonBuilder = new StringBuilder();
                jsonBuilder.Append($"[{{\"database\":\"{Database.Name}\",\"collection\":\"{Name}\"");

                Debug.Assert(args.Length % 2 == 0, "args should be provided as key-value pairs");

                for (var i = 0; i < args.Length; i += 2)
                {
                    if (args[i + 1] != null)
                    {
                        jsonBuilder.Append($",\"{args[i]}\":{args[i + 1].ToNativeJson()}");
                    }
                }

                jsonBuilder.Append("}]");

                return await Database.Client.User.Functions.CallSerializedAsync<T>(functionName, Database.Client.ServiceName, jsonBuilder.ToString());
            }
        }

        /// <summary>
        /// The result of <see cref="Collection{TDocument}.UpdateOneAsync"/> or <see cref="Collection{TDocument}.UpdateManyAsync"/> operation.
        /// </summary>
        public class UpdateResult
        {
            /// <summary>
            /// Gets the number of documents matched by the filter.
            /// </summary>
            /// <value>The number of matched documents.</value>
            [BsonElement("matchedCount")]
            [Preserve]
            public int MatchedCount { get; private set; }

            /// <summary>
            /// Gets the number of documents modified by the operation.
            /// </summary>
            /// <value>The number of modified documents.</value>
            [BsonElement("modifiedCount")]
            [Preserve]
            public int ModifiedCount { get; private set; }

            /// <summary>
            /// Gets the <c>_id</c> of the inserted document if the operation resulted in an insertion.
            /// </summary>
            /// <value>The <c>_id</c> of the inserted document or <c>null</c> if the operation didn't result in an insertion.</value>
            [BsonElement("upsertedId")]
            [Preserve]
            public object UpsertedId { get; private set; }
        }

        /// <summary>
        /// The result of <see cref="Collection{TDocument}.InsertOneAsync"/> operation.
        /// </summary>
        public class InsertResult
        {
            /// <summary>
            /// Gets the <c>_id</c> of the inserted document.
            /// </summary>
            /// <value>The <c>_id</c> of the inserted document.</value>
            [BsonElement("insertedId")]
            [Preserve]
            public object InsertedId { get; private set; }
        }

        /// <summary>
        /// The result of <see cref="Collection{TDocument}.InsertManyAsync"/> operation.
        /// </summary>
        public class InsertManyResult
        {
            /// <summary>
            /// Gets an array containing the <c>_id</c>s of the inserted documents.
            /// </summary>
            /// <value>The <c>_id</c>s of the inserted documents.</value>
            [BsonElement("insertedIds")]
            [Preserve]
            public object[] InsertedIds { get; private set; }
        }

        /// <summary>
        /// The result of <see cref="Collection{TDocument}.DeleteOneAsync"/> or <see cref="Collection{TDocument}.DeleteManyAsync(object)"/> operation.
        /// </summary>
        public class DeleteResult
        {
            /// <summary>
            /// Gets the number of deleted documents.
            /// </summary>
            /// <value>The number of deleted documents.</value>
            [BsonElement("deletedCount")]
            [Preserve]
            public int DeletedCount { get; private set; }
        }
    }
}
