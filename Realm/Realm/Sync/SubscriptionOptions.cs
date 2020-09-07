////////////////////////////////////////////////////////////////////////////
//
// Copyright 2019 Realm Inc.
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

namespace Realms.Sync
{
    /// <summary>
    /// A set of options provided to <see cref="Subscription.Subscribe{T}(IQueryable{T}, SubscriptionOptions, System.Linq.Expressions.Expression{Func{T, IQueryable}}[])"/>
    /// to control the behavior of the subscription.
    /// </summary>
    public class SubscriptionOptions
    {
        /// <summary>
        /// Gets or sets the name of the subscription.
        /// </summary>
        /// <value>The subscription name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the time to live of the subscription. If not set or set
        /// to <c>null</c>, the subscription is kept indefinitely. The subscription
        /// will be automatically removed after the time to live passes.
        /// </summary>
        /// <value>The time to live.</value>
        public TimeSpan? TimeToLive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the subscription should be updated
        /// if one with the same name already exists. If set to <c>false</c> and a subscription
        /// with the same name exists, an exception will be thrown.
        /// </summary>
        /// <value><c>true</c> if subscription should be updated; otherwise, <c>false</c>.</value>
        public bool ShouldUpdate { get; set; }
    }
}
