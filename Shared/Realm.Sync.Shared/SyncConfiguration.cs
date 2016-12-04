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
    /// An SyncConfiguration is used to setup a Realm that can be synchronized between devices using the Realm Object Server.
    /// A valid <see cref="User"/> is required to create a <see cref="SyncConfiguration"/>.
    /// </summary>
    /// <seealso cref="User.LoginAsync"/>
    /// <seealso cref="Credentials"/>
    public class SyncConfiguration : RealmConfigurationBase, IEquatable<SyncConfiguration>
    {
        /// <summary>
        /// Gets the <see cref="Uri"/> used to create this SyncConfiguration. 
        /// </summary>
        public Uri ServerUri { get; }

        /// <summary>
        /// Gets the user used to create this SyncConfiguration.
        /// </summary>
        public User User { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncConfiguration"/> class.
        /// </summary>
        /// <param name="user">A valid <see cref="User"/>.</param>
        /// <param name="serverUri">A unique <see cref="Uri"/> that identifies the Realm. In URIs, <c>~</c> can be used as a placeholder for a user Id.</param>
        /// <param name="optionalPath">Path to the realm, must be a valid full path for the current platform, relative subdirectory, or just filename.</param>
        public SyncConfiguration(User user, Uri serverUri, string optionalPath = null) : base(optionalPath)
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

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as SyncConfiguration);
        }

        /// <summary>
        /// Determines whether the specified RealmConfiguration is equal to the current RealmConfiguration.
        /// </summary>
        /// <param name="other">The <see cref="SyncConfiguration"/> to compare with the current configuration.</param>
        /// <returns><c>true</c> if the specified <see cref="SyncConfiguration"/> is equal to the current
        /// <see cref="SyncConfiguration"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(SyncConfiguration other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other) &&
                       ServerUri == other.ServerUri &&
                       User.Equals(other.User);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = base.GetHashCode();
                hash = (23 * hash) + ServerUri.GetHashCode();
                hash = (23 * hash) + User.GetHashCode();
                return hash;
            }
        }
    }
}
