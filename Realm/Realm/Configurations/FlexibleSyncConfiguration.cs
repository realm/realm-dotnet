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
using Realms.Helpers;
using Realms.Sync.ErrorHandling;

namespace Realms.Sync
{
    /// <summary>
    /// A <see cref="FlexibleSyncConfiguration"/> is used to setup a <see cref="Realm"/> whose data can be synchronized
    /// between devices using Atlas Device Sync. Unlike <see cref="PartitionSyncConfiguration"/>, a Realm opened with
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
            var tracker = InjectInitialSubscriptions();

            var result = await base.CreateRealmAsync(cancellationToken);

            InvokeInitialSubscriptions(tracker, result);

            if (tracker.PopulateInitialDataInvoked)
            {
                await result.Subscriptions.WaitForSynchronizationAsync();

                // TODO: remove the wait once https://github.com/realm/realm-core/issues/5705 is resolved
                await result.SyncSession.WaitForDownloadAsync();
            }

            return result;
        }

        internal override Realm CreateRealm()
        {
            var tracker = InjectInitialSubscriptions();

            var result = base.CreateRealm();

            InvokeInitialSubscriptions(tracker, result);

            return result;
        }

        private InitialDataTracker InjectInitialSubscriptions()
        {
            var tracker = new InitialDataTracker
            {
                Callback = PopulateInitialSubscriptions
            };

            if (tracker.Callback != null)
            {
                var oldDataCallback = PopulateInitialData;
                PopulateInitialData = (realm) =>
                {
                    // We can't run the PopulateInitialSubscriptions callback here because
                    // the Realm is already in a write transaction, so Subscriptions.Update
                    // will hang. We flag that with `shouldPopulate` and update the subscriptions
                    // after we open the realm.
                    oldDataCallback?.Invoke(realm);
                    tracker.PopulateInitialDataInvoked = true;
                };
            }

            return tracker;
        }

        private static void InvokeInitialSubscriptions(InitialDataTracker tracker, Realm realm)
        {
            if (!tracker.PopulateInitialDataInvoked)
            {
                return;
            }

            try
            {
                realm.Subscriptions.Update(() =>
                {
                    // We're not guarded by a write lock between invoking the `InitialDataCallback`
                    // and getting here, so it's possible someone managed to insert subscriptions
                    // before us. If that's the case, then don't insert anything and just wait for
                    // sync.
                    if (realm.Subscriptions.Count == 0)
                    {
                        tracker.Callback(realm);
                    }
                });
            }
            catch (Exception ex)
            {
                // This needs to duplicate the logic in RealmConfigurationBase.CreateRealmAsync
                throw new AggregateException("Exception occurred in a Realm.PopulateInitialSubscriptions callback. See inner exception for more details.", ex);
            }
        }

        internal override Native.SyncConfiguration CreateNativeSyncConfiguration()
        {
            var config = base.CreateNativeSyncConfiguration();
            config.is_flexible_sync = true;
            return config;
        }

        // This is a holder class to workaround the fact that we can't use ref booleans
        // inside lambda functions. Its purpose is to server as a marker that PopulateInitialData
        // was invoked, which means we need to then update subscriptions.
        private class InitialDataTracker
        {
            public bool PopulateInitialDataInvoked;

            public InitialSubscriptionsDelegate Callback;
        }
    }
}
