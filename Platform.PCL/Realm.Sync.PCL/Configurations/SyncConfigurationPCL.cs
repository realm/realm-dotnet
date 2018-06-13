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

namespace Realms.Sync
{
    /// <summary>
    /// A <see cref="SyncConfiguration"/> is used to setup a <see cref="Realm"/> that can be synchronized between devices using the
    /// Realm Object Server. A valid <see cref="User"/> is required to create a <see cref="SyncConfiguration"/>.
    /// </summary>
    /// <seealso cref="User.LoginAsync"/>
    /// <seealso cref="Credentials"/>
    [Obsolete("Use FullSyncConfiguration or QueryBasedSyncConfiguration instead.")]
    public class SyncConfiguration : SyncConfigurationBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether this Realm should be opened in 'query-based synchronization' mode.
        /// Query-based synchronization mode means that no objects are synchronized from the remote Realm
        /// except those matching queries that the user explicitly specifies.
        /// </summary>
        /// <see cref="QueryBasedSyncConfiguration"/>
        [Obsolete("Create QueryBasedSyncConfiguration instead.")]
        public bool IsPartial { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncConfiguration"/> class.
        /// </summary>
        /// <param name="user">
        /// A valid <see cref="User"/>. If not provided, the currently logged-in user will be used.
        /// </param>
        /// <param name="serverUri">
        /// A unique <see cref="Uri"/> that identifies the Realm. In URIs, <c>~</c> can be used as a placeholder for a user Id.
        /// If a relative Uri is provided, it will be resolved using the user's <see cref="User.ServerUri"/> as baseUri.
        /// If <c>null</c> is passed, a Uri will be constructed from the user's <see cref="User.ServerUri"/>, combined with
        /// <c>/default</c> and <see cref="IsPartial"/> will be set to <c>true</c>.
        /// </param>
        /// <param name="optionalPath">
        /// Path to the realm, must be a valid full path for the current platform, relative subdirectory, or just filename.
        /// </param>
        public SyncConfiguration(User user = null, Uri serverUri = null, string optionalPath = null)
            : base(serverUri, user, optionalPath)
        {
        }

        /// <summary>
        /// Sets the feature token, associated with your edition. You only need to call it if you're using a professional
        /// or higher edition and only on platforms where features are disabled for lower editions.
        /// </summary>
        /// <param name="token">The feature token provided to you by the Realm team.</param>
        /// <seealso href="https://realm.io/docs/realm-object-server/pe-ee/#enabling-professional-and-enterprise-apis">
        /// See more details on Enabling Professional and Enterprise APIs in the documentation.
        /// </seealso>
        [Obsolete("Feature tokens are no longer necessary to access Professional or Enterprise API.")]
        public static void SetFeatureToken(string token)
        {
        }
    }
}