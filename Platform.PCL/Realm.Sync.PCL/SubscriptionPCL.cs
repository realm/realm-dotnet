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
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Realms.Exceptions;

namespace Realms.Sync
{
    /// <summary>
    /// A set of helper methods exposing partial-sync related functionality over collections.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public static class Subscription
    {        
        /// <summary>
        /// For partially synchronized Realms, fetches and synchronizes the objects that match the query. 
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
        /// <exception cref="ArgumentException">Thrown if the <c>query</c> was not obtained from a partially synchronized Realm.</exception>
        public static Subscription<T> Subscribe<T>(this IQueryable<T> query, string name = null)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// Cancel a named subscription that was created by calling <see cref="Subscribe"/>.
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// Cancel a subscription that was created by calling <see cref="Subscribe"/>.
        /// <para />
        /// Removing a subscription will delete all objects from the local Realm that were matched
        /// only by that subscription and not any remaining subscriptions. The deletion is performed
        /// by the server, and so has no immediate impact on the contents of the local Realm. If the
        /// device is currently offline, the removal will not be processed until the device returns online.
        /// </summary>
        /// <typeparam name="T">The type of the objects that make up the subscription query.</typeparam>
        /// <param name="subscription">The subscription to cancel.</param>
        /// <returns>An awaitable task, that indicates that the subscription has been removed locally.</returns>
        public static Task UnsubscribeAsync<T>(this Subscription<T> subscription)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }
    }

    /// <summary>
    /// A class that represents a subscription to a set of objects in a synced Realm.
    /// <para/>
    /// When partial sync is enabled for a synced Realm, the only objects that the server synchronizes to the
    /// client are those that match a sync subscription registered by that client. A subscription consists of
    /// of a query (represented by an <c>IQueryable{T}</c>) and an optional name.
    /// <para/>
    /// The state of the subscription can be observed by subscribing to the <see cref="PropertyChanged"/> event handler.
    /// <para/>
    /// Subscriptions are created by calling <see cref="Subscription.Subscribe"/>.
    /// </summary>
    /// <typeparam name="T">The type of the objects that make up the subscription query.</typeparam>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class Subscription<T> : INotifyPropertyChanged
    {
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

        private Subscription()
        {
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }
    }
}
