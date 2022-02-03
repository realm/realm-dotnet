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
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Realms.Helpers;
using Realms.Sync.ErrorHandling;
using Realms.Sync.Exceptions;

namespace Realms.Sync
{
    /// <summary>
    /// A <see cref="SyncConfigurationBase"/> is used to setup a <see cref="Realm"/> that can be synchronized between devices using MongoDB Realm.
    /// There are two synchronization modes with their respective configurations - "partition" sync with <see cref="PartitionSyncConfiguration"/> allows you
    /// to split your data in separarate partitions and synchronize an entire partition with an entire Realm; "flexible" sync with
    /// <see cref="FlexibleSyncConfiguration"/> allows you to start with an empty Realm and send the server a set of queries which it will run and
    /// populate the Realm with all documents matching them.
    /// </summary>
    /// <seealso href="https://docs.mongodb.com/realm/sync/overview/">Sync Overview Docs</seealso>
    public abstract class SyncConfigurationBase : RealmConfigurationBase
    {
        /// <summary>
        /// Gets the <see cref="User"/> used to create this <see cref="SyncConfigurationBase"/>.
        /// </summary>
        /// <value>The <see cref="User"/> whose <see cref="Realm"/>s will be synced.</value>
        public User User { get; }

        /// <summary>
        /// Gets or sets a handler that will be invoked if a client reset error occurs for this Realm.
        /// Supported values are instances of <see cref="ManualRecoveryHandler"/> or <see cref="DiscardLocalResetHandler"/>.
        /// The default is <see cref="DiscardLocalResetHandler"/> with no custom actions set for the before and after callbacks.
        /// </summary>
        public ClientResetHandlerBase ClientResetHandler { get; set; }

        /// <summary>
        /// Gets or sets the user callback to handle all the <see cref="SessionException"/>s that could happen on a synchronized Realm.
        /// To be noted that a client reset is not a sync error and in order to handle that subclasses of <see cref="ClientResetHandlerBase"/> are available.
        /// </summary>
        /// <seealso href="https://docs.mongodb.com/realm/sync/overview/">Sync Overview Docs</seealso>
        public SessionErrorCallback OnSessionError { get; set; }

        internal SessionStopPolicy SessionStopPolicy { get; set; } = SessionStopPolicy.AfterChangesUploaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncConfigurationBase"/> class.
        /// </summary>
        /// <param name="user">
        /// A valid <see cref="User"/>.
        /// </param>
        protected SyncConfigurationBase(User user)
        {
            Argument.NotNull(user, nameof(user));

            User = user;
        }

        internal override Realm CreateRealm()
        {
            var schema = GetSchema();
            var configuration = CreateNativeConfiguration();
            var syncConfiguration = CreateNativeSyncConfiguration();

            var srHandle = SharedRealmHandle.OpenWithSync(configuration, syncConfiguration, schema, EncryptionKey);
            return GetRealm(srHandle, schema);
        }

        internal override async Task<Realm> CreateRealmAsync(CancellationToken cancellationToken)
        {
            var schema = GetSchema();
            var configuration = CreateNativeConfiguration();
            var syncConfiguration = CreateNativeSyncConfiguration();

            var tcs = new TaskCompletionSource<ThreadSafeReferenceHandle>();
            var tcsHandle = GCHandle.Alloc(tcs);
            IDisposable progressToken = null;
            try
            {
                using var handle = SharedRealmHandle.OpenWithSyncAsync(configuration, syncConfiguration, schema, EncryptionKey, tcsHandle);
                cancellationToken.Register(() =>
                {
                    if (!handle.IsClosed)
                    {
                        handle.Cancel();
                        tcs.TrySetCanceled();
                    }
                });

                progressToken = OnBeforeRealmOpen(handle);

                using var realmReference = await tcs.Task;
                var sharedRealmHandle = SharedRealmHandle.ResolveFromReference(realmReference);
                return GetRealm(sharedRealmHandle, schema);
            }
            finally
            {
                tcsHandle.Free();
                progressToken?.Dispose();
            }
        }

        internal virtual IDisposable OnBeforeRealmOpen(AsyncOpenTaskHandle handle) => null;

        internal virtual Native.SyncConfiguration CreateNativeSyncConfiguration()
        {
            var syncConfHandle = GCHandle.Alloc(this);
            return new Native.SyncConfiguration
            {
                SyncUserHandle = User.Handle,
                session_stop_policy = SessionStopPolicy,
                schema_mode = Schema == null ? SchemaMode.AdditiveDiscovered : SchemaMode.AdditiveExplicit,
                client_resync_mode = ClientResetHandler is DiscardLocalResetHandler ? ClientResyncMode.DiscardLocal : ClientResyncMode.Manual,
                managed_sync_configuration_handle = GCHandle.ToIntPtr(syncConfHandle),
            };
        }
    }
}
