////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
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
using Realms.Helpers;

namespace Realms.Sync
{
    /// <summary>
    /// A set of extension methods that provide Sync-related functionality on top of Realm classes.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class RealmSyncExtensions
    {
        /// <summary>
        /// Gets the <see cref="Session"/> for the realm file behind this <see cref="Realm"/>.
        /// </summary>
        /// <param name="realm">An instance of the <see cref="Realm"/> class created with a <see cref="SyncConfigurationBase"/> object.</param>
        /// <returns>The <see cref="Session"/> that is responsible for synchronizing with the MongoDB Realm server.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="realm"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="realm"/> was not created with a <see cref="SyncConfigurationBase"/> object.</exception>
        [Obsolete("Use Realm.SyncSession instead.")]
        public static Session GetSession(this Realm realm)
        {
            Argument.NotNull(realm, nameof(realm));
            Argument.EnsureType<SyncConfigurationBase>(realm.Config, "Cannot get a Session for a Realm without a SyncConfiguration", nameof(realm));

            return realm.SyncSession;
        }

        /// <summary>
        /// Gets the <see cref="SubscriptionSet"/> representing the active subscriptions for this <see cref="Realm"/>.
        /// </summary>
        /// <param name="realm">An instance of the <see cref="Realm"/> class created with a <see cref="FlexibleSyncConfiguration"/> object.</param>
        /// <returns>
        /// The <see cref="SubscriptionSet"/> containing the query subscriptions that the server is using to decide which objects to
        /// synchronize with the local <see cref="Realm"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="realm"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="realm"/> was not created with a <see cref="FlexibleSyncConfiguration"/> object.</exception>
        public static SubscriptionSet GetSubscriptions(this Realm realm)
        {
            Argument.NotNull(realm, nameof(realm));
            var flxConfig = Argument.EnsureType<FlexibleSyncConfiguration>(realm.Config, "Cannot get subscriptions for a Realm without a FlexibleSyncConfiguration", nameof(realm));

            throw new NotImplementedException();
        }
    }
}
