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

using System;
using MongoDB.Bson;
using Realms.Helpers;

namespace Realms.Sync
{
    /// <summary>
    /// A <see cref="PartitionSyncConfiguration"/> is used to setup a <see cref="Realm"/> that can be synchronized between devices using Atlas Device Sync.
    /// </summary>
    /// <seealso href="https://www.mongodb.com/docs/realm/sdk/dotnet/sync/">Device Sync Docs</seealso>
    public class PartitionSyncConfiguration : SyncConfigurationBase
    {
        /// <summary>
        /// Gets or sets a callback that is invoked when download progress is made when using <see cref="Realm.GetInstanceAsync"/>.
        /// This will only be invoked for the initial download of the Realm and will not be invoked as further download
        /// progress is made during the lifetime of the Realm. It is ignored when using
        /// <see cref="Realm.GetInstance(RealmConfigurationBase)"/>.
        /// </summary>
        /// <value>A callback that will be periodically invoked as the Realm is downloaded.</value>
        public Action<SyncProgress>? OnProgress { get; set; }

        /// <summary>
        /// Gets the partition identifying the Realm this configuration is describing.
        /// </summary>
        /// <value>The partition value for the Realm.</value>
        public RealmValue Partition { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartitionSyncConfiguration"/> class.
        /// </summary>
        /// <param name="partition">
        /// The partition identifying the remote Realm that will be synchronized.
        /// </param>
        /// <param name="user">
        /// A valid <see cref="User"/>.
        /// </param>
        /// <param name="optionalPath">
        /// Path to the realm, must be a valid full path for the current platform, relative subdirectory, or just filename.
        /// </param>
        public PartitionSyncConfiguration(string? partition, User user, string? optionalPath = null)
            : this(user, partition, optionalPath)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartitionSyncConfiguration"/> class.
        /// </summary>
        /// <param name="partition">
        /// The partition identifying the remote Realm that will be synchronized.
        /// </param>
        /// <param name="user">
        /// A valid <see cref="User"/>.
        /// </param>
        /// <param name="optionalPath">
        /// Path to the realm, must be a valid full path for the current platform, relative subdirectory, or just filename.
        /// </param>
        public PartitionSyncConfiguration(long? partition, User user, string? optionalPath = null)
            : this(user, partition, optionalPath)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartitionSyncConfiguration"/> class.
        /// </summary>
        /// <param name="partition">
        /// The partition identifying the remote Realm that will be synchronized.
        /// </param>
        /// <param name="user">
        /// A valid <see cref="User"/>.
        /// </param>
        /// <param name="optionalPath">
        /// Path to the realm, must be a valid full path for the current platform, relative subdirectory, or just filename.
        /// </param>
        public PartitionSyncConfiguration(ObjectId? partition, User user, string? optionalPath = null)
            : this(user, partition, optionalPath)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartitionSyncConfiguration"/> class.
        /// </summary>
        /// <param name="partition">
        /// The partition identifying the remote Realm that will be synchronized.
        /// </param>
        /// <param name="user">
        /// A valid <see cref="User"/>.
        /// </param>
        /// <param name="optionalPath">
        /// Path to the realm, must be a valid full path for the current platform, relative subdirectory, or just filename.
        /// </param>
        public PartitionSyncConfiguration(Guid? partition, User user, string? optionalPath = null)
            : this(user, partition, optionalPath)
        {
        }

        private PartitionSyncConfiguration(User user, RealmValue partition, string? path)
            : base(user, GetPathToRealm(path ?? user?.Handle.GetRealmPath(partition.ToNativeJson())))
        {
            Partition = partition;
        }

        internal override IDisposable? OnBeforeRealmOpen(AsyncOpenTaskHandle handle)
        {
            var onProgress = OnProgress;
            if (onProgress is null)
            {
                return null;
            }

            return new ProgressNotificationToken(
                observer: (progress) =>
                {
                    onProgress(progress);
                },
                register: handle.RegisterProgressNotifier,
                unregister: (token) =>
                {
                    if (!handle.IsClosed)
                    {
                        handle.UnregisterProgressNotifier(token);
                    }
                });
        }

        internal override Native.SyncConfiguration CreateNativeSyncConfiguration()
        {
            var config = base.CreateNativeSyncConfiguration();
            config.Partition = Partition.ToNativeJson();
            return config;
        }
    }
}
