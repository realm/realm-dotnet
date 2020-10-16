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
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using Realms.Helpers;
using Realms.Schema;

namespace Realms.Sync
{
    /// <summary>
    /// A <see cref="SyncConfiguration"/> is used to setup a <see cref="Realm"/> that can be synchronized between devices using MongoDB Realm.
    /// </summary>
    /// <seealso href="https://docs.mongodb.com/realm/sync/overview/">Sync Overview Docs</seealso>
    public class SyncConfiguration : RealmConfigurationBase
    {
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
        /// Gets the partition identifying the Realm this configuration is describing.
        /// </summary>
        /// <value>The partition value for the Realm.</value>
        public object Partition { get; }

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
            : this((object)partition, user, optionalPath)
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
            : this((object)partition, user, optionalPath)
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
            : this((object)partition, user, optionalPath)
        {
        }

        private SyncConfiguration(object partition, User user, string path)
        {
            Argument.NotNull(user, nameof(user));

            User = user;
            Partition = partition;
            DatabasePath = GetPathToRealm(path ?? user.App.Handle.GetRealmPath(User, partition.ToNativeJson()));
        }

        internal override Realm CreateRealm(RealmSchema schema)
        {
            var configuration = CreateConfiguration();

            var srHandle = SharedRealmHandle.OpenWithSync(configuration, ToNative(), schema, EncryptionKey);
            if (IsDynamic && !schema.Any())
            {
                srHandle.GetSchema(nativeSchema => schema = RealmSchema.CreateFromObjectStoreSchema(nativeSchema));
            }

            return new Realm(srHandle, this, schema);
        }

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The Realm instance will own its handle")]
        internal override async Task<Realm> CreateRealmAsync(RealmSchema schema, CancellationToken cancellationToken)
        {
            var configuration = CreateConfiguration();

            var tcs = new TaskCompletionSource<ThreadSafeReferenceHandle>();
            var tcsHandle = GCHandle.Alloc(tcs);
            ProgressNotificationToken progressToken = null;
            try
            {
                using var handle = SharedRealmHandle.OpenWithSyncAsync(configuration, ToNative(), schema, EncryptionKey, tcsHandle);
                cancellationToken.Register(() =>
                {
                    if (!handle.IsClosed)
                    {
                        handle.Cancel();
                        tcs.TrySetCanceled();
                    }
                });

                if (OnProgress != null)
                {
                    progressToken = new ProgressNotificationToken(
                        observer: (progress) =>
                        {
                            OnProgress(progress);
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
                    sharedRealmHandle.GetSchema(nativeSchema => schema = RealmSchema.CreateFromObjectStoreSchema(nativeSchema));
                }

                return new Realm(sharedRealmHandle, this, schema);
            }
            finally
            {
                tcsHandle.Free();
                progressToken?.Dispose();
            }
        }

        internal Native.SyncConfiguration ToNative()
        {
            return new Native.SyncConfiguration
            {
                SyncUserHandle = User.Handle,
                Partition = Partition.ToNativeJson(),
                session_stop_policy = SessionStopPolicy,
            };
        }
    }
}
