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
using System.Text.RegularExpressions;
using Realms.Exceptions;

namespace Realms.Sync
{
    /// <summary>
    /// A managed Realm object representing a subscription. Subscriptions are used by Query-based Realms to define which
    /// data should be available on the device. It is the persisted version of a <see cref="Subscription{T}"/> created by
    /// calling <see cref="Subscription.Subscribe{T}(IQueryable{T}, SubscriptionOptions, System.Linq.Expressions.Expression{Func{T, IQueryable}}[])"/>.
    /// </summary>
    [MapTo("__ResultSets")]
    [Explicit]
    public class NamedSubscription : RealmObject
    {
        private static readonly Regex _matchesRegex = new Regex("^(class_)?(?<objectType>.*?)(_matches)?$", RegexOptions.Compiled);

        /// <summary>
        /// Gets the name of the subscription. If no name was provided in <see cref="SubscriptionOptions.Name"/>, then
        /// an automatic name will have been generated based on the query.
        /// </summary>
        /// <value>The subscription name.</value>
        [MapTo("name")]
        [Indexed]
        [Required]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the string representation of the query used to create the subscription.
        /// </summary>
        /// <value>The subscription query.</value>
        [MapTo("query")]
        [Required]
        public string Query { get; private set; }

        /// <summary>
        /// Gets the point in time when the subscription was created.
        /// </summary>
        /// <value>The creation date of the subscription.</value>
        [MapTo("created_at")]
        public DateTimeOffset CreatedAt { get; private set; }

        /// <summary>
        /// Gets the point in time when the subscription was updated.
        /// </summary>
        /// <remarks>
        /// In this context,
        /// "updated" means that the subscription was resubscribed to or some property
        /// was updated by calling <see cref="Subscription.Subscribe{T}(IQueryable{T}, SubscriptionOptions, System.Linq.Expressions.Expression{Func{T, IQueryable}}[])"/>.
        /// The field is NOT updated whenever the results of the query changes.
        /// </remarks>
        /// <value>The last updated date of the subscription.</value>
        [MapTo("updated_at")]
        public DateTimeOffset UpdatedAt { get; private set; }

        /// <summary>
        /// Gets the point in time when the subscription will expire and become eligible for removal.
        /// </summary>
        /// <remarks>
        /// Realm will automatically remove expired subscriptions at opportunistic times. There
        /// are no guarantees as to when the subscription will be removed.
        /// </remarks>
        /// <value>The expiration date of the subscription.</value>
        [MapTo("expires_at")]
        public DateTimeOffset? ExpiresAt { get; private set; }

        [MapTo("time_to_live")]
        private long? TimeToLiveMs { get; set; }

        /// <summary>
        /// Gets the time to live of the subscription.
        /// </summary>
        /// <value>The subscription's time to live.</value>
        public TimeSpan? TimeToLive => TimeToLiveMs.HasValue ? TimeSpan.FromMilliseconds(TimeToLiveMs.Value) : (TimeSpan?)null;

        [MapTo("status")]
        private int StateInt { get; set; }

        /// <summary>
        /// Gets a value indicating the state of this subscription.
        /// </summary>
        /// <value>The state of the subscription.</value>
        public SubscriptionState State => (SubscriptionState)StateInt;

        [MapTo("error_message")]
        [Required]
        private string ErrorMessage { get; set; }

        /// <summary>
        /// Gets a value indicating what error (if any) has occurred while processing the subscription.
        /// If the <see cref="State"/> is not <see cref="SubscriptionState.Error"/>, this will be <c>null</c>.
        /// </summary>
        /// <value>An instance of <see cref="Exception"/> if an error has occurred; <c>null</c> otherwise.</value>
        public Exception Error => string.IsNullOrEmpty(ErrorMessage) ? null : new RealmException(ErrorMessage);

        [MapTo("matches_property")]
        [Required]
        private string Matches { get; set; }

        /// <summary>
        /// Gets the type of the object that this subscription is applied to.
        /// </summary>
        /// <value>The type of the object that the subscription matches.</value>
        public string ObjectType => _matchesRegex.Match(Matches).Groups["objectType"].Value;
    }
}
