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
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using Realms.Helpers;
using Realms.Sync.ErrorHandling;

namespace Realms.Sync
{
    /// <summary>
    /// A <see cref="SyncConfiguration"/> is used to setup a <see cref="Realm"/> that can be synchronized between devices using MongoDB Realm.
    /// </summary>
    /// <seealso href="https://docs.mongodb.com/realm/sync/overview/">Sync Overview Docs</seealso>
    public class SyncConfiguration : RealmConfigurationBase
    {
        // TODO: https://github.com/realm/realm-dotnet/issues/2571 - we should update the public property type to be RealmValue and
        // get rid of this field.
        internal readonly RealmValue _partition;

        /// <summary>
        /// Gets the <see cref="User"/> used to create this <see cref="SyncConfiguration"/>.
        /// </summary>
        /// <value>The <see cref="User"/> whose <see cref="Realm"/>s will be synced.</value>
        public User User { get; }

        /// <summary>
        /// Gets or sets a callback that is invoked when download progress is made when using <see cref="Realm.GetInstanceAsync"/>.
        /// This will only be invoked for the initial download of the Realm and will not be invoked as futher download
        /// progress is made during the lifetime of the Realm. It is ignored when using
        /// <see cref="Realm.GetInstance(RealmConfigurationBase)"/>.
        /// </summary>
        /// <value>A callback that will be periodically invoked as the Realm is downloaded.</value>
        public Action<SyncProgress> OnProgress { get; set; }

        /// <summary>
        /// Gets or sets a subclass of <see cref="ClientResetHandlerBase"> to specify actions to be taken for the selected Client Reset strategy: <see cref="ManualRecoveryHandler"> or <see cref="DiscardLocalResetHandler"/>.
        /// If nothing is set, the strategy defaults to <see cref="DiscardLocalResetHandler"/> with no custom actions set for the before and after synchronization.
        /// </summary>
        public ClientResetHandlerBase ClientResetHandler { get; set; }

        /// <summary>
        /// Gets or sets a callback to handle errors that happen on a session.
        /// </summary>
        public SyncErrorHandler SyncErrorHandler { get; set; }

        /// <summary>
        /// Gets the partition identifying the Realm this configuration is describing.
        /// </summary>
        /// <value>The partition value for the Realm.</value>
        public object Partition => _partition.AsAny();

        internal SessionStopPolicy SessionStopPolicy { get; set; } = SessionStopPolicy.AfterChangesUploaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncConfiguration"/> class.
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
        public SyncConfiguration(string partition, User user, string optionalPath = null)
            : this(user, partition, optionalPath)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncConfiguration"/> class.
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
        public SyncConfiguration(long? partition, User user, string optionalPath = null)
            : this(user, partition, optionalPath)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncConfiguration"/> class.
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
        public SyncConfiguration(ObjectId? partition, User user, string optionalPath = null)
            : this(user, partition, optionalPath)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncConfiguration"/> class.
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
        public SyncConfiguration(Guid? partition, User user, string optionalPath = null)
            : this(user, partition, optionalPath)
        {
        }

        private SyncConfiguration(User user, RealmValue partition, string path)
        {
            Argument.NotNull(user, nameof(user));

            User = user;
            _partition = partition;
            DatabasePath = GetPathToRealm(path ?? user.App.Handle.GetRealmPath(User, Partition.ToNativeJson()));
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
            ProgressNotificationToken progressToken = null;
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

                // Capture OnProgress to avoid having it overwritten while the download is in flight and throwing NRE
                var onProgress = OnProgress;
                if (onProgress != null)
                {
                    progressToken = new ProgressNotificationToken(
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

        internal Native.SyncConfiguration CreateNativeSyncConfiguration()
        {
            GCHandle? syncConfHandle = GCHandle.Alloc(this);
            return new Native.SyncConfiguration
            {
                SyncUserHandle = User.Handle,
                Partition = Partition.ToNativeJson(),
                session_stop_policy = SessionStopPolicy,
                schema_mode = Schema == null ? SchemaMode.AdditiveDiscovered : SchemaMode.AdditiveExplicit,
                managed_sync_configuration_handle = GCHandle.ToIntPtr(syncConfHandle.Value),
            };
        }
    }
}
