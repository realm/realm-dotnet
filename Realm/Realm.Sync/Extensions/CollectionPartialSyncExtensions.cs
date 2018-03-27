////////////////////////////////////////////////////////////////////////////
//
// Copyright 2018 Realm Inc.
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
using System.ComponentModel;
using System.Linq;
using Realms.Helpers;

namespace Realms.Sync
{
    /// <summary>
    /// A set of extensions methods exposing partial-sync related functionality over collections.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class CollectionPartialSyncExtensions
    {
        /// <summary>
        /// For partially synchronized Realms, fetches and synchronizes the objects that match the query. 
        /// </summary>
        /// <typeparam name="T">The type of the objects making up the query.</typeparam>
        /// <param name="query">
        /// A query, obtained by calling <see cref="Realm.All{T}"/> with or without additional filtering applied.
        /// </param>
        /// <param name="name">The name of this query that can be used to unsubscribe from.</param>
        /// <returns>
        /// A <see cref="Subscription{T}"/> instance that contains information and methods for monitoring
        /// the state of the subscription.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <c>query</c> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if the <c>query</c> was not obtained from a partially synchronized Realm.</exception>
        public static Subscription<T> SubscribeToObjects<T>(this IQueryable<T> query, string name = null)
        {
            Argument.NotNull(query, nameof(query));

            var results = query as RealmResults<T>;
            Argument.Ensure(results != null, $"{nameof(query)} must be an instance of IRealmCollection<{typeof(T).Name}>.", nameof(query));

            var syncConfig = results.Realm.Config as SyncConfiguration;
            Argument.Ensure(syncConfig != null, $"{nameof(query)} must be obtained from a synchronized Realm.", nameof(query));
            Argument.Ensure(syncConfig.IsPartial, $"{nameof(query)} must be obtained from a partial Realm.", nameof(query));

            var handle = SubscriptionHandle.Create(results.ResultsHandle, name);
            return new Subscription<T>(handle, results);
        }
    }
}
