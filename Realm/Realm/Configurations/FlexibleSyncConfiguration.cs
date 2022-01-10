////////////////////////////////////////////////////////////////////////////
//
// Copyright 2021 Realm Inc.
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

using System.Diagnostics.CodeAnalysis;

namespace Realms.Sync
{
    /// <summary>
    /// A <see cref="FlexibleSyncConfiguration"/> is used to setup a <see cref="Realm"/> whose data can be synchronized
    /// between devices using MongoDB Realm. Unlike <see cref="PartitionSyncConfiguration"/>, a Realm opened with
    /// <see cref="FlexibleSyncConfiguration"/> will be initially empty until one or more subscriptions are added
    /// via <see cref="Realm.Subscriptions"/>.
    /// </summary>
    /// <seealso href="https://docs.mongodb.com/realm/sdk/dotnet/fundamentals/realm-sync/">Sync Docs</seealso>
    public class FlexibleSyncConfiguration : SyncConfigurationBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlexibleSyncConfiguration"/> class.
        /// </summary>
        /// <param name="user">
        /// A valid <see cref="User"/>.
        /// </param>
        /// <param name="optionalPath">
        /// Path to the realm, must be a valid full path for the current platform, relative subdirectory, or just filename.
        /// </param>
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Arguments are validated in the base ctor.")]
        public FlexibleSyncConfiguration(User user, string optionalPath = null)
            : base(user)
        {
            DatabasePath = GetPathToRealm(optionalPath ?? user.App.Handle.GetRealmPath(User, "default"));
        }

        internal override Native.SyncConfiguration CreateNativeSyncConfiguration()
        {
            var config = base.CreateNativeSyncConfiguration();
            config.is_flexible_sync = true;
            return config;
        }
    }
}
