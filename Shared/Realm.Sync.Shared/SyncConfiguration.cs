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
using Realms.Schema;

namespace Realms.Sync
{
    /// <summary>
    /// A <see cref="SyncConfiguration"/> is used to setup a <see cref="Realm"/> that can be synchronized between devices using the
    /// Realm Object Server. A valid <see cref="User"/> is required to create a <see cref="SyncConfiguration"/>.
    /// </summary>
    /// <seealso cref="User.LoginAsync"/>
    /// <seealso cref="Credentials"/>
    public class SyncConfiguration : RealmConfigurationBase
    {
        /// <summary>
        /// Gets the <see cref="Uri"/> used to create this <see cref="SyncConfiguration"/>.
        /// </summary>
        /// <value>The <see cref="Uri"/> where the Realm Object Server is hosted.</value>
        public Uri ServerUri { get; }

        /// <summary>
        /// Gets the <see cref="User"/> used to create this <see cref="SyncConfiguration"/>.
        /// </summary>
        /// <value>The <see cref="User"/> whose <see cref="Realm"/>s will be synced.</value>
        public User User { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncConfiguration"/> class.
        /// </summary>
        /// <param name="user">A valid <see cref="User"/>.</param>
        /// <param name="serverUri">A unique <see cref="Uri"/> that identifies the Realm. In URIs, <c>~</c> can be used as a placeholder for a user Id.</param>
        /// <param name="optionalPath">Path to the realm, must be a valid full path for the current platform, relative subdirectory, or just filename.</param>
        public SyncConfiguration(User user, Uri serverUri, string optionalPath = null) : base(optionalPath ?? SharedRealmHandleExtensions.GetRealmPath(user, serverUri))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (serverUri == null)
            {
                throw new ArgumentNullException(nameof(serverUri));
            }

            User = user;
            ServerUri = serverUri;
        }

        internal override Realm CreateRealm(RealmSchema schema)
        {
            var configuration = new Realms.Native.Configuration
            {
                Path = DatabasePath,
                schema_version = SchemaVersion
            };

            var syncConfiguration = new Native.SyncConfiguration
            {
                SyncUserHandle = User.Handle,
                Url = ServerUri.ToString()
            };

            var srHandle = SharedRealmHandleExtensions.OpenWithSync(configuration, syncConfiguration, schema, EncryptionKey);
            return new Realm(srHandle, this, schema);
        }
    }
}
