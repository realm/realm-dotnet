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
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Realms.Sync.ErrorHandling;

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
        /// A delegate invoked when a flexible sync Realm is first opened.
        /// </summary>
        /// <param name="realm">The realm that has just been opened.</param>
        public delegate void InitialSubscriptionsDelegate(Realm realm);

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
            DatabasePath = GetPathToRealm(optionalPath ?? user.App.Handle.GetRealmPath(User));
        }

        /// <summary>
        /// Gets or sets a callback that will be invoked the first time a Realm is opened.
        /// </summary>
        /// <remarks>
        /// This callback allows you to populate an initial set of subscriptions, which will
        /// then be awaited when <see cref="Realm.GetInstance(RealmConfigurationBase)"/> is invoked.
        /// </remarks>
        /// <value>
        /// The <see cref="InitialSubscriptionsDelegate"/> that will be invoked the first time
        /// a Realm is opened.
        /// </value>
        public InitialSubscriptionsDelegate PopulateInitialSubscriptions { get; set; }

        internal override async Task<Realm> CreateRealmAsync(CancellationToken cancellationToken)
        {
            var didPopulate = false;
            if (PopulateInitialSubscriptions != null)
            {
                var oldDataCallback = InitialDataCallback;
                InitialDataCallback = (realm) =>
                {
                    oldDataCallback?.Invoke(realm);
                    PopulateInitialSubscriptions(realm);
                    didPopulate = true;
                };
            }

            var result = await base.CreateRealmAsync(cancellationToken);
            if (didPopulate)
            {
                await result.Subscriptions.WaitForSynchronizationAsync();
            }

            return result;
        }

        internal override Native.SyncConfiguration CreateNativeSyncConfiguration()
        {
            var config = base.CreateNativeSyncConfiguration();
            config.is_flexible_sync = true;
            config.client_resync_mode = ClientResyncMode.Manual;
            return config;
        }

        /// <summary>
        /// Gets or sets a handler that will be invoked if a client reset error occurs for this Realm.
        /// </summary>
        /// <value>The <see cref="ClientResetHandlerBase"/> that will be used to handle a client reset.</value>
        /// <remarks>
        /// Currently, Flexible sync only supports the <see cref="ManualRecoveryHandler"/>. Support for <see cref="DiscardLocalResetHandler"/> will come in the future.
        /// </remarks>
        /// <exception cref="NotSupportedException">
        /// Flexible sync is still in beta, so at the moment <see cref="DiscardLocalResetHandler"/> is not supported.
        /// </exception>
        /// <seealso href="https://docs.mongodb.com/realm/sdk/dotnet/advanced-guides/client-reset/">Client reset docs</seealso>
        public override ClientResetHandlerBase ClientResetHandler
        {
            get => base.ClientResetHandler;
            set
            {
                if (value is DiscardLocalResetHandler)
                {
                    throw new NotSupportedException($"Flexible sync does not yet support {nameof(DiscardLocalResetHandler)}");
                }

                base.ClientResetHandler = value;
            }
        }
    }
}
