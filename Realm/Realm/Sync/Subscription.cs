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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Realms.Exceptions;
using Realms.Helpers;
using Realms.Native;

namespace Realms.Sync
{
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    internal interface ISubscription
    {
        void ReloadState();
    }

    /// <summary>
    /// A set of extension methods exposing query-based sync related functionality over collections.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public static class Subscription
    {
        internal static readonly SubscriptionHandle.SubscriptionCallbackDelegate SubscriptionCallback;

        static Subscription()
        {
            SubscriptionCallback = SubscriptionCallbackImpl;

            // prevent the delegate from ever being garbage collected
            GCHandle.Alloc(SubscriptionCallback);
        }

        /// <summary>
        /// For Realms using query-based synchronization, fetches and synchronizes the objects that match the query. 
        /// </summary>
        /// <typeparam name="T">The type of the objects making up the query.</typeparam>
        /// <param name="query">
        /// A query, obtained by calling <see cref="Realm.All{T}"/> with or without additional filtering applied.
        /// </param>
        /// <param name="name">The name of this query that can be used to unsubscribe from.</param>
        /// <returns>
        /// A <see cref="Subscription{T}"/> instance that contains information and methods for monitoring
        /// the state of the subscription.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <c>query</c> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the <c>query</c> was not obtained from a query-based synchronized Realm.
        /// </exception>
        [Obsolete("Use Subscribe(query, options) instead.")]
        public static Subscription<T> Subscribe<T>(this IQueryable<T> query, string name)
        {
            return query.Subscribe(new SubscriptionOptions
            {
                Name = name
            });
        }

        /// <summary>
        /// For Realms using query-based synchronization, fetches and synchronizes the objects that match the query. 
        /// </summary>
        /// <typeparam name="T">The type of the objects making up the query.</typeparam>
        /// <param name="query">
        /// A query, obtained by calling <see cref="Realm.All{T}"/> with or without additional filtering applied.
        /// </param>
        /// <param name="options">
        /// Options that configure some metadata of the subscription, such as its name or time to live.
        /// </param>
        /// <param name="includedBacklinks">
        /// An array of property expressions which specifies which linkingObjects relationships should be included in
        /// the subscription. Subscriptions already include link and list properties (in the forward direction)
        /// automatically by default.
        /// </param>
        /// <returns>
        /// A <see cref="Subscription{T}"/> instance that contains information and methods for monitoring
        /// the state of the subscription.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <c>query</c> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the <c>query</c> was not obtained from a query-based synchronized Realm.
        /// </exception>
        public static Subscription<T> Subscribe<T>(
            this IQueryable<T> query,
            SubscriptionOptions options = null,
            params Expression<Func<T, IQueryable>>[] includedBacklinks)
        {
            Argument.NotNull(query, nameof(query));

            var results = query as RealmResults<T>;
            Argument.Ensure(results != null, $"{nameof(query)} must be an instance of IRealmCollection<{typeof(T).Name}>.", nameof(query));

            options = options ?? new SubscriptionOptions();

            var syncConfig = results.Realm.Config as SyncConfigurationBase;
            Argument.Ensure(syncConfig?.IsFullSync == false, $"{nameof(query)} must be obtained from a synchronized Realm using query-based synchronization.", nameof(query));

            var handle = SubscriptionHandle.Create(
                results.ResultsHandle,
                options.Name,
                (long?)options.TimeToLive?.TotalMilliseconds,
                options.ShouldUpdate,
                includedBacklinks.ToStringPaths());
            return new Subscription<T>(handle, results);
        }

        /// <summary>
        /// Returns all subscriptions registered for that Realm.
        /// </summary>
        /// <returns>A queryable collection of all registered subscriptions.</returns>
        /// <param name="realm">A Realm opened with a <see cref="QueryBasedSyncConfiguration"/>.</param>
        public static IRealmCollection<NamedSubscription> GetAllSubscriptions(this Realm realm)
        {
            Argument.NotNull(realm, nameof(realm));
            var syncConfig = realm.Config as SyncConfigurationBase;
            Argument.Ensure(syncConfig?.IsFullSync == false, $"{nameof(realm)} must be a synchronized Realm using query-based synchronization.", nameof(realm));

            return realm.All<NamedSubscription>().AsRealmCollection();
        }

        /// <summary>
        /// Cancel a named subscription that was created by calling <see cref="Subscribe{T}(IQueryable{T}, SubscriptionOptions, Expression{Func{T, IQueryable}}[])"/>.
        /// <para />
        /// Removing a subscription will delete all objects from the local Realm that were matched
        /// only by that subscription and not any remaining subscriptions. The deletion is performed
        /// by the server, and so has no immediate impact on the contents of the local Realm. If the
        /// device is currently offline, the removal will not be processed until the device returns online.
        /// </summary>
        /// <param name="realm">The Realm where this subscription was added.</param>
        /// <param name="subscriptionName">The name of the subscription to remove.</param>
        /// <returns>An awaitable task, that indicates that the subscription has been removed locally.</returns>
        public static Task UnsubscribeAsync(this Realm realm, string subscriptionName)
        {
            Argument.NotNull(realm, nameof(realm));
            var syncConfig = realm.Config as SyncConfigurationBase;
            Argument.Ensure(syncConfig?.IsFullSync == false, $"{nameof(realm)} must be a synchronized Realm using query-based synchronization.", nameof(realm));

            var config = realm.Config.Clone();
            config.ObjectClasses = new[] { typeof(NamedSubscription) };
            config.EnableCache = false;

            return Task.Run(() =>
            {
                using (var backgroundRealm = Realm.GetInstance(config))
                {
                    var resultSets = backgroundRealm.All<NamedSubscription>().Where(r => r.Name == subscriptionName);
                    if (!resultSets.Any())
                    {
                        throw new RealmException($"A subscription with the name {subscriptionName} doesn't exist.");
                    }

                    backgroundRealm.Write(() =>
                    {
                        backgroundRealm.RemoveRange(resultSets);
                    });
                }
            });
        }

        /// <summary>
        /// Cancel a subscription that was created by calling <see cref="Subscribe{T}(IQueryable{T}, SubscriptionOptions, Expression{Func{T, IQueryable}}[])"/>.
        /// <para />
        /// Removing a subscription will delete all objects from the local Realm that were matched
        /// only by that subscription and not any remaining subscriptions. The deletion is performed
        /// by the server, and so has no immediate impact on the contents of the local Realm. If the
        /// device is currently offline, the removal will not be processed until the device returns online.
        /// </summary>
        /// <typeparam name="T">The type of the objects that make up the subscription query.</typeparam>
        /// <param name="subscription">The subscription to cancel.</param>
        /// <returns>An awaitable task, that indicates that the subscription has been removed locally.</returns>
        public static async Task UnsubscribeAsync<T>(this Subscription<T> subscription)
        {
            Argument.NotNull(subscription, nameof(subscription));
            if (subscription.State == SubscriptionState.Invalidated)
            {
                return;
            }

            AsyncHelper.EnsureValidContext();

            subscription.Handle.Unsubscribe();

            var tcs = new TaskCompletionSource<object>();
            PropertyChangedEventHandler handler = null;
            handler = new PropertyChangedEventHandler((s, e) =>
            {
                switch (subscription.State)
                {
                    case SubscriptionState.Invalidated:
                        tcs.TrySetResult(null);
                        break;
                    case SubscriptionState.Error:
                        tcs.TrySetException(subscription.Error);
                        break;
                }
            });

            subscription.PropertyChanged += handler;
            try
            {
                await tcs.Task;
            }
            finally
            {
                subscription.PropertyChanged -= handler;
                subscription.SubscriptionToken.Dispose();
            }
        }

        [NativeCallback(typeof(SubscriptionHandle.SubscriptionCallbackDelegate))]
        private static void SubscriptionCallbackImpl(IntPtr managedHandle)
        {
            if (GCHandle.FromIntPtr(managedHandle).Target is ISubscription subscription)
            {
                subscription.ReloadState();
            }
        }
    }

    /// <summary>
    /// A class that represents a subscription to a set of objects in a synced Realm.
    /// <para/>
    /// When query-based sync is enabled for a synced Realm, the only objects that the server synchronizes to the
    /// client are those that match a sync subscription registered by that client. A subscription consists of
    /// of a query (represented by an <c>IQueryable{T}</c>) and an optional name.
    /// <para/>
    /// The state of the subscription can be observed by subscribing to the <see cref="PropertyChanged"/> event handler.
    /// <para/>
    /// Subscriptions are created by calling <see cref="Subscription.Subscribe{T}(IQueryable{T}, SubscriptionOptions, Expression{Func{T, IQueryable}}[])"/>.
    /// </summary>
    /// <typeparam name="T">The type of the objects that make up the subscription query.</typeparam>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class Subscription<T> : INotifyPropertyChanged, ISubscription
    {
        internal readonly SubscriptionHandle Handle;
        internal readonly SubscriptionTokenHandle SubscriptionToken;

        private readonly TaskCompletionSource<object> _syncTcs = new TaskCompletionSource<object>();

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets a value indicating the state of this subscription.
        /// </summary>
        /// <value>The state of the subscription.</value>
        public SubscriptionState State { get; private set; }

        /// <summary>
        /// Gets a value indicating what error (if any) has occurred while processing the subscription.
        /// If the <see cref="State"/> is not <see cref="SubscriptionState.Error"/>, this will be <c>null</c>.
        /// </summary>
        /// <value>An instance of <see cref="Exception"/> if an error has occurred; <c>null</c> otherwise.</value>
        public Exception Error { get; private set; }

        /// <summary>
        /// Gets the query that this subscription is associated with. Regardless of the state of the subscription,
        /// this value will reflect the results in the local Realm. This allows you to data-bind to this property
        /// immediately and show the last synchronized data. If the <see cref="State"/> is <see cref="SubscriptionState.Invalidated"/>,
        /// the values returned will not be an adequate representation of the state of the remote Realm.
        /// </summary>
        /// <value>
        /// A queryable collection that can be further filtered, ordered, or observed for changes.
        /// </value>
        public IQueryable<T> Results { get; }

        internal Subscription(SubscriptionHandle handle, RealmResults<T> query)
        {
            Results = query;

            Handle = handle;
            ((ISubscription)this).ReloadState();

            var managedSubscriptionHandle = GCHandle.Alloc(this, GCHandleType.Weak);
            SubscriptionToken = Handle.AddNotificationCallback(GCHandle.ToIntPtr(managedSubscriptionHandle), Subscription.SubscriptionCallback);
        }

        /// <summary>
        /// Waits for the subscription to complete synchronizing (equivalent to transitioning to the
        /// <see cref="SubscriptionState.Complete"/> state.
        /// </summary>
        /// <returns>
        /// An awaitable task, that, upon completion, indicates that the objects matching the specified query
        /// have been synchronized to the local Realm.
        /// </returns>
        public Task WaitForSynchronizationAsync()
        {
            AsyncHelper.EnsureValidContext();

            return _syncTcs.Task;
        }

        void ISubscription.ReloadState()
        {
            if (Handle.IsClosed)
            {
                return;
            }

            var newState = Handle.GetState();
            if (newState != State)
            {
                try
                {
                    // If we encounter an unexpected value, assume it's an error.
                    if (!Enum.IsDefined(typeof(SubscriptionState), newState))
                    {
                        newState = SubscriptionState.Error;
                    }

                    State = newState;
                    switch (State)
                    {
                        case SubscriptionState.Error:
                            Error = Handle.GetError() ?? new RealmException($"An unknown error has occurred. State: {Handle.GetState()}");
                            _syncTcs.TrySetException(Error);
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Error)));
                            break;
                        case SubscriptionState.Complete:
                            _syncTcs.TrySetResult(null);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _syncTcs.TrySetException(ex);
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
            }
        }
    }
}
