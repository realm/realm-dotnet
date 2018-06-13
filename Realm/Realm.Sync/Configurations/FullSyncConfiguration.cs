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

namespace Realms.Sync
{
    /// <summary>
    /// A <see cref="FullSyncConfiguration"/> is used to setup a <see cref="Realm"/> that can be synchronized
    /// in "full" mode between devices using the Realm Object Server. The entirety of the Realm will be kept
    /// in sync between the server and the client.
    /// </summary>
    /// <seealso href="https://docs.realm.io/platform/using-synced-realms/syncing-data#full-synchronization">
    /// Full Synchronization docs.
    /// </seealso>
    /// <seealso cref="QueryBasedSyncConfiguration"/>
    public class FullSyncConfiguration : SyncConfigurationBase
    {
        internal override bool IsFullSync => true;

        /// <summary>
        /// Initializes a new instance of the <see cref="FullSyncConfiguration"/> class.
        /// </summary>
        /// <param name="user">
        /// A valid <see cref="User"/>. If not provided, the currently logged-in user will be used.
        /// </param>
        /// <param name="serverUri">
        /// A unique <see cref="Uri"/> that identifies the Realm. In URIs, <c>~</c> can be used as a placeholder for a user Id.
        /// If a relative Uri is provided, it will be resolved using the user's <see cref="User.ServerUri"/> as baseUri.
        /// </param>
        /// <param name="optionalPath">
        /// Path to the realm, must be a valid full path for the current platform, relative subdirectory, or just filename.
        /// </param>
        public FullSyncConfiguration(Uri serverUri, User user = null, string optionalPath = null)
            : base(serverUri, user, optionalPath)
        {
        }
    }
}
