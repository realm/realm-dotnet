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
using Realms.Schema;

namespace Realms.Sync
{
    /// <summary>
    /// A <see cref="QueryBasedSyncConfiguration"/> is used to setup a <see cref="Realm"/> that can be synchronized
    /// in "query-based" mode between devices using the Realm Object Server. Only objects that match the subscribed
    /// queries will be synchronized to the client.
    /// </summary>
    /// <seealso href="https://docs.realm.io/platform/using-synced-realms/syncing-data#using-query-based-synchronization">
    /// Query-based Synchronization docs.
    /// </seealso>
    /// <seealso cref="FullSyncConfiguration"/>
    /// <see cref="Subscription.Subscribe"/>
    public class QueryBasedSyncConfiguration : SyncConfigurationBase
    {
        internal override bool IsFullSync => false;

        internal static readonly Type[] _queryBasedPermissionTypes = new[]
        {
            typeof(ClassPermission),
            typeof(Permission),
            typeof(PermissionRole),
            typeof(PermissionUser),
            typeof(RealmPermission)
        };


        /// <summary>
        /// Initializes a new instance of the <see cref="QueryBasedSyncConfiguration"/> class.
        /// </summary>
        /// <param name="user">
        /// A valid <see cref="User"/>. If not provided, the currently logged-in user will be used.
        /// </param>
        /// <param name="serverUri">
        /// A unique <see cref="Uri"/> that identifies the Realm. In URIs, <c>~</c> can be used as a placeholder for a user Id.
        /// If a relative Uri is provided, it will be resolved using the user's <see cref="User.ServerUri"/> as baseUri.
        /// If <c>null</c> is passed, a Uri will be constructed from the user's <see cref="User.ServerUri"/>, combined with
        /// <c>/default</c>.
        /// </param>
        /// <param name="optionalPath">
        /// Path to the realm, must be a valid full path for the current platform, relative subdirectory, or just filename.
        /// </param>
        public QueryBasedSyncConfiguration(Uri serverUri = null, User user = null, string optionalPath = null)
            : base(serverUri ?? new Uri("/default", UriKind.Relative), user, optionalPath)
        {
        }

        internal override Realm CreateRealm(RealmSchema schema)
        {
            schema = RealmSchema.CreateSchemaForClasses(_queryBasedPermissionTypes, schema);
            return base.CreateRealm(schema);
        }
    }
}
