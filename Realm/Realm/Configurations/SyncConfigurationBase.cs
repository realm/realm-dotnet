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
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Realms.Helpers;
using Realms.Schema;

namespace Realms.Sync
{
    /// <summary>
    /// A <see cref="SyncConfigurationBase"/> is used to setup a <see cref="Realm"/> that can be synchronized between devices using MongoDB Realm.
    /// </summary>
    /// <seealso href="https://docs.mongodb.com/realm/sync/overview/">Sync Overview Docs</seealso>
    public abstract class SyncConfigurationBase : RealmConfigurationBase
    {
        /// <summary>
        /// Gets the <see cref="User"/> used to create this <see cref="PartitionSyncConfiguration"/>.
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

        internal SessionStopPolicy SessionStopPolicy { get; set; } = SessionStopPolicy.AfterChangesUploaded;

        internal SyncConfigurationBase(User user, string path, string realmIdentifier)
        {
            Argument.NotNull(user, nameof(user));

            User = user;
            DatabasePath = GetPathToRealm(path ?? user.App.Handle.GetRealmPath(User, realmIdentifier));
        }

        internal override Realm CreateRealm(RealmSchema schema)
        {
            var configuration = CreateNativeConfiguration();
            var syncConfiguration = CreateNativeSyncConfiguration();

            var srHandle = SharedRealmHandle.OpenWithSync(configuration, syncConfiguration, schema, EncryptionKey);
            if (IsDynamic && !schema.Any())
            {
                schema = srHandle.GetSchema();
            }

            return new Realm(srHandle, this, schema);
        }

        internal override async Task<Realm> CreateRealmAsync(RealmSchema schema, CancellationToken cancellationToken)
        {
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
                var realmPtr = SharedRealmHandle.ResolveFromReference(realmReference);
                var sharedRealmHandle = new SharedRealmHandle(realmPtr);
                if (IsDynamic && !schema.Any())
                {
                    schema = sharedRealmHandle.GetSchema();
                }

                return new Realm(sharedRealmHandle, this, schema);
            }
            finally
            {
                tcsHandle.Free();
                progressToken?.Dispose();
            }
        }

        internal virtual Native.SyncConfiguration CreateNativeSyncConfiguration()
        {
            return new Native.SyncConfiguration
            {
                SyncUserHandle = User.Handle,
                session_stop_policy = SessionStopPolicy,
                schema_mode = ObjectClasses == null ? SchemaMode.AdditiveDiscovered : SchemaMode.AdditiveExplicit,
            };
        }
    }
}
