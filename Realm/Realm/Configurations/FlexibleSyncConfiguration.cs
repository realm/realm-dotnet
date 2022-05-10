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
        /// <br/>
        /// The <see cref="SubscriptionSet"/> returned by <see cref="Realm.Subscriptions"/> is already
        /// called in an <see cref="SubscriptionSet.Update(System.Action)"/> block, so there's no need
        /// to start one inside the callback.
        /// </remarks>
        /// <example>
        /// <code>
        /// var config = new FlexibleSyncConfiguration(user)
        /// {
        ///     PopulateInitialSubscriptions = (realm) =>
        ///     {
        ///         var myNotes = realm.All&lt;Note&gt;().Where(n =&gt; n.AuthorId == myUserId);
        ///         realm.Subscriptions.Add(myNotes);
        ///     }
        /// };
        ///
        /// // The task will complete when all the user notes have been downloaded.
        /// var realm = await Realm.GetInstanceAsync(config);
        /// </code>
        /// </example>
        /// <value>
        /// The <see cref="InitialSubscriptionsDelegate"/> that will be invoked the first time
        /// a Realm is opened.
        /// </value>
        public InitialSubscriptionsDelegate PopulateInitialSubscriptions { get; set; }

        internal override async Task<Realm> CreateRealmAsync(CancellationToken cancellationToken)
        {
            var shouldPopulate = false;
            var populateSubs = PopulateInitialSubscriptions;
            if (populateSubs != null)
            {
                var oldDataCallback = InitialDataCallback;
                InitialDataCallback = (realm) =>
                {
                    // We can't run the PopulateInitialSubscriptions callback here because
                    // the Realm is already in a write transaction, so Subscriptions.Update
                    // will hang. We flag that `shouldPopulate` 
                    oldDataCallback?.Invoke(realm);
                    shouldPopulate = true;
                };
            }

            var result = await base.CreateRealmAsync(cancellationToken);
            if (shouldPopulate)
            {
                result.Subscriptions.Update(() =>
                {
                    // We're not guarded by a write lock between invoking the `InitialDataCallback`
                    // and getting here, so it's possible someone managed to insert subscriptions
                    // before us. If that's the case, then don't insert anything and just wait for
                    // sync.
                    if (result.Subscriptions.Count == 0)
                    {
                        populateSubs(result);
                    }
                });
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
