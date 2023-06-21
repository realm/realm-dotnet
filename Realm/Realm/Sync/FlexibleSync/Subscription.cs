////////////////////////////////////////////////////////////////////////////
//
// Copyright 2021 Realm Inc.
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

using System;
using System.Runtime.CompilerServices;
using MongoDB.Bson;

namespace Realms.Sync
{
    /// <summary>
    /// A class representing a single query subscription. The server will continuously
    /// evaluate the <see cref="Query"/> that the app subscribed to and will send data
    /// that matches it as well as remove data that no longer does.
    /// </summary>
    public class Subscription
    {
        internal ObjectId Id { get; }

        /// <summary>
        /// Gets the name of the subscription if one was provided at creation time.
        /// If no name was provided, then this will return <c>null</c>.
        /// </summary>
        /// <value>The subscription's name.</value>
        public string? Name { get; }

        /// <summary>
        /// Gets the type of objects this subscription refers to.
        /// </summary>
        /// <remarks>
        /// If your types are remapped using <see cref="MapToAttribute"/>, the value
        /// returned will be the mapped-to value - i.e. the one that Realm uses internally
        /// rather than the name of the C# class.
        /// </remarks>
        /// <value>The object type for the subscription.</value>
        public string ObjectType { get; }

        /// <summary>
        /// Gets the query that describes the subscription. Objects matched by the query
        /// will be sent to the device by the server.
        /// </summary>
        /// <value>The subscription query.</value>
        public string Query { get; }

        /// <summary>
        /// Gets a value indicating when this subscription was created.
        /// </summary>
        /// <value>The creation date/time of the subscription.</value>
        public DateTimeOffset CreatedAt { get; }

        /// <summary>
        /// Gets a value indicating when this subscription was last updated.
        /// </summary>
        /// <value>The date/time of the last update to the subscription.</value>
        public DateTimeOffset UpdatedAt { get; }

        internal Subscription(ObjectId id, string? name, string objectType, string query, DateTimeOffset createdAt, DateTimeOffset updatedAt)
        {
            Id = id;
            Name = name;
            ObjectType = objectType;
            Query = query;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }
    }
}
