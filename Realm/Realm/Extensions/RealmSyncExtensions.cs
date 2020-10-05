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
        /// <returns>The <see cref="Session"/> that is responsible for synchronizing with the MongoDB Realm server.</returns>
        /// <param name="realm">An instance of the <see cref="Realm"/> class created with a <see cref="SyncConfiguration"/> object.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="realm"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="realm"/> was not created with a <see cref="SyncConfiguration"/> object.</exception>
        public static Session GetSession(this Realm realm)
        {
            Argument.NotNull(realm, nameof(realm));
            var syncConfig = Argument.EnsureType<SyncConfiguration>(realm.Config, "Cannot get a Session for a Realm without a SyncConfiguration", nameof(realm));

            var session = syncConfig.User.App.Handle.GetSessionForPath(realm.SharedRealmHandle);
            return new Session(session);
        }
    }
}
