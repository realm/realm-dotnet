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
    /// A <see cref="SyncConfigurationBase"/> is used to setup a <see cref="Realm"/> that can be synchronized between devices using Atlas Device Sync.
    /// There are two synchronization modes with their respective configurations - "partition" sync with <see cref="PartitionSyncConfiguration"/> allows you
    /// to split your data in separarate partitions and synchronize an entire partition with an entire Realm; "flexible" sync with
    /// <see cref="FlexibleSyncConfiguration"/> allows you to start with an empty Realm and send the server a set of queries which it will run and
    /// populate the Realm with all documents matching them.
    /// </summary>
    /// <seealso href="https://docs.mongodb.com/realm/sync/overview/">Sync Overview Docs</seealso>
    public abstract class SyncConfigurationBase : RealmConfigurationBase
    {
        private ClientResetHandlerBase _clientResetHandler = new RecoverOrDiscardUnsyncedChangesHandler();

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
        /// Gets or sets a handler that will be invoked if a client reset error occurs for this Realm. Default is <see cref="RecoverUnsyncedChangesHandler"/>
        /// with fallback to discarding unsynced local changes.
        /// </summary>
        /// <value>The <see cref="ClientResetHandlerBase"/> that will be used to handle a client reset.</value>
        /// <remarks>
        /// Supported values are instances of <see cref="ManualRecoveryHandler"/>, <see cref="DiscardUnsyncedChangesHandler"/> and
        /// <see cref="RecoverUnsyncedChangesHandler"/>.
        /// The default <see cref="RecoverUnsyncedChangesHandler"/> will have no custom actions set for the before and after callbacks.
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
        public SessionErrorCallback? OnSessionError { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether async operations, such as <see cref="Realm.GetInstanceAsync"/>,
        /// <see cref="Session.WaitForUploadAsync"/>, or <see cref="Session.WaitForDownloadAsync"/> should throw an
        /// error whenever a non-fatal error, such as timeout occurs.
        /// </summary>
        /// <remarks>
        /// If set to <c>false</c>, non-fatal session errors will be ignored and sync will continue retrying the
        /// connection under in the background. This means that in cases where the devie is offline, these operations
        /// may take an indeterminate time to complete.
        /// </remarks>
        /// <value><c>true</c> to throw an error if a non-fatal session error occurs, <c>false</c> otherwise.</value>
        public bool CancelAsyncOperationsOnNonFatalErrors { get; set; }

        /// <summary>
        /// Gets or sets the key, used to encrypt the entire Realm. Once set, must be specified each time the file is used.
        /// </summary>
        /// <value>Full 64byte (512bit) key for AES-256 encryption.</value>
        public new byte[]? EncryptionKey
        {
            get => base.EncryptionKey;
            set => base.EncryptionKey = value;
        }

        internal SessionStopPolicy SessionStopPolicy { get; set; } = SessionStopPolicy.AfterChangesUploaded;

        private protected SyncConfigurationBase(User user, string databasePath)
            : base(databasePath)
        {
            Argument.NotNull(user, nameof(user));

            User = user;
        }

        internal override SharedRealmHandle CreateHandle(Schema.RealmSchema schema)
        {
            var syncConfiguration = CreateNativeSyncConfiguration();
            return SharedRealmHandle.OpenWithSync(CreateNativeConfiguration(), syncConfiguration, schema, EncryptionKey);
        }

        internal override async Task<SharedRealmHandle> CreateHandleAsync(Schema.RealmSchema schema, CancellationToken cancellationToken)
        {
            var syncConfiguration = CreateNativeSyncConfiguration();

            var tcs = new TaskCompletionSource<ThreadSafeReferenceHandle>();
            var tcsHandle = GCHandle.Alloc(tcs);
            using var handle = SharedRealmHandle.OpenWithSyncAsync(CreateNativeConfiguration(), syncConfiguration, schema, EncryptionKey, GCHandle.ToIntPtr(tcsHandle));
            cancellationToken.Register(() =>
            {
                if (!handle.IsClosed)
                {
                    handle.Cancel();
                    tcs.TrySetCanceled();
                }
            });

            using var progressToken = OnBeforeRealmOpen(handle);

            try
            {
                using var realmReference = await tcs.Task;
                return SharedRealmHandle.ResolveFromReference(realmReference);
            }
            finally
            {
                tcsHandle.Free();
            }
        }

        internal virtual IDisposable? OnBeforeRealmOpen(AsyncOpenTaskHandle handle) => null;

        internal virtual Native.SyncConfiguration CreateNativeSyncConfiguration()
        {
            var (proxyAddress, proxyPort) = GetSystemProxy();

            return new Native.SyncConfiguration
            {
                SyncUserHandle = User.Handle,
                session_stop_policy = SessionStopPolicy,
                schema_mode = _schema == null ? SchemaMode.AdditiveDiscovered : SchemaMode.AdditiveExplicit,
                client_resync_mode = ClientResetHandler.ClientResetMode,
                cancel_waits_on_nonfatal_error = CancelAsyncOperationsOnNonFatalErrors,
                ProxyAddress = proxyAddress,
                proxy_port = proxyPort,
            };
        }

        private static readonly Uri FakeProxyAddress = new("wss://realm.mongodb.com");

        private static (string? Address, ushort Port) GetSystemProxy()
        {
            var proxyUrl = System.Net.WebRequest.DefaultWebProxy?.GetProxy(FakeProxyAddress);
            if (proxyUrl != null && proxyUrl != FakeProxyAddress)
            {
                if (proxyUrl.Scheme != "http")
                {
                    throw new InvalidOperationException($"Unsupported proxy scheme '${proxyUrl.Scheme}', expected 'http'");
                }

                return (proxyUrl.Host, (ushort)proxyUrl.Port);
            }

            return (null, 0);
        }
    }
}
