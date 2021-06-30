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
using System.Diagnostics.CodeAnalysis;
using MongoDB.Bson;
using Realms.Helpers;

namespace Realms.Sync
{
    /// <summary>
    /// A <see cref="PartitionSyncConfiguration"/> is used to setup a <see cref="Realm"/> that can be synchronized between devices using MongoDB Realm.
    /// </summary>
    /// <seealso href="https://docs.mongodb.com/realm/sync/overview/">Sync Overview Docs</seealso>
    public class PartitionSyncConfiguration : SyncConfigurationBase
    {
        /// <summary>
        /// Gets the partition identifying the Realm this configuration is describing.
        /// </summary>
        /// <value>The partition value for the Realm.</value>
        public object Partition { get; }

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
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Arguments are validated in the private ctor.")]
        public PartitionSyncConfiguration(string partition, User user, string optionalPath = null)
            : this((object)partition, user, optionalPath)
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
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Arguments are validated in the private ctor.")]
        public PartitionSyncConfiguration(long? partition, User user, string optionalPath = null)
            : this((object)partition, user, optionalPath)
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
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Arguments are validated in the private ctor.")]
        public PartitionSyncConfiguration(ObjectId? partition, User user, string optionalPath = null)
            : this((object)partition, user, optionalPath)
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
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Arguments are validated in the private ctor.")]
        public PartitionSyncConfiguration(Guid? partition, User user, string optionalPath = null)
            : this((object)partition, user, optionalPath)
        {
        }

        private PartitionSyncConfiguration(object partition, User user, string path)
            : base(user, path, partition.ToNativeJson())
        {
            Partition = partition;
        }

        internal override Native.SyncConfiguration CreateNativeSyncConfiguration()
        {
            var config = base.CreateNativeSyncConfiguration();
            config.Partition = Partition.ToNativeJson();
            return config;
        }
    }
}
