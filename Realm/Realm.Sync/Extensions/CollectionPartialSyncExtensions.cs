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

using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Realms.Helpers;

namespace Realms.Sync
{
    /// <summary>
    /// A set of extensions methods exposing notification-related functionality over collections.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class CollectionPartialSyncExtensions
    {
        public static Task<Subscription<T>> SubscribeToObjectsAsync<T>(this IQueryable<T> query, string name = null)
        {
            Argument.NotNull(query, nameof(query));

            var results = query as RealmResults<T>;
            Argument.Ensure(results != null, $"{nameof(query)} must be an instance of IRealmCollection<{typeof(T).Name}>.", nameof(query));

            return results.Realm.SubscribeToObjectsAsync(query, name);
        }

        public static Task UnsubscribeFromObjectsAsync<T>(this IQueryable<T> query)
        {
            Argument.NotNull(query, nameof(query));

            var results = query as RealmResults<T>;
            Argument.Ensure(results != null, $"{nameof(query)} must be an instance of IRealmCollection<{typeof(T).Name}>.", nameof(query));

            return results.Realm.UnsubscribeFromObjectsAsync(query);
        }
    }
}
