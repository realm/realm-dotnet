﻿////////////////////////////////////////////////////////////////////////////
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
        private ClientResetHandlerBase _clientResetHandler = new DiscardLocalResetHandler();

        /// <summary>
        /// Callback triggered when an error occurs in a session.
        /// </summary>
        /// <param name="session">
        /// The <see cref="Session"/> where the error happened on.
        /// </param>
        /// <param name="error">
        /// The specific <see cref="SessionException"/> occurred on this <see cref="Session"/>.
        /// </param>
        public delegate void SessionErrorCallback(Session session, SessionException error);

        /// <summary>
        /// Gets the <see cref="User"/> used to create this <see cref="SyncConfigurationBase"/>.
        /// </summary>
        /// <value>The <see cref="User"/> whose <see cref="Realm"/>s will be synced.</value>
        public User User { get; }

        /// <summary>
        /// Gets or sets a handler that will be invoked if a client reset error occurs for this Realm. Default is <see cref="DiscardLocalResetHandler"/>.
        /// </summary>
        /// <value>The <see cref="ClientResetHandlerBase"/> that will be used to handle a client reset.</value>
        /// <remarks>
        /// Supported values are instances of <see cref="ManualRecoveryHandler"/> or <see cref="DiscardLocalResetHandler"/>.
        /// The default <see cref="DiscardLocalResetHandler"/> will have no custom actions set for the before and after callbacks.
        /// </remarks>
        /// <seealso href="https://docs.mongodb.com/realm/sdk/dotnet/advanced-guides/client-reset/">Client reset docs</seealso>
        public virtual ClientResetHandlerBase ClientResetHandler
        {
            get => _clientResetHandler;
            set => _clientResetHandler = Argument.ValidateNotNull(value, nameof(value));
        }

        /// <summary>
        /// Gets or sets a callback that will be invoked whenever a <see cref="SessionException"/> occurs for the synchronized Realm.
        /// </summary>
        /// <value>The <see cref="SessionErrorCallback"/> that will be used to report transient session errors.</value>
        /// <remarks>
        /// Client reset errors will not be reported through this callback as they are handled by the set <see cref="ClientResetHandler"/>.
        /// </remarks>
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
            var clientResyncMode = ClientResyncMode.DiscardLocal;

            if (ClientResetHandler != null && ClientResetHandler is ManualRecoveryHandler)
            {
                clientResyncMode = ClientResyncMode.Manual;
            }

            return new Native.SyncConfiguration
            {
                SyncUserHandle = User.Handle,
                session_stop_policy = SessionStopPolicy,
                schema_mode = Schema == null ? SchemaMode.AdditiveDiscovered : SchemaMode.AdditiveExplicit,
                client_resync_mode = clientResyncMode,
                managed_sync_configuration_handle = GCHandle.ToIntPtr(syncConfHandle),
            };
        }
    }
}
