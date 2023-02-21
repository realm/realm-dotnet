////////////////////////////////////////////////////////////////////////////
//
// Copyright 2021 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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

using System.Linq;

namespace Realms.Sync
{
    /// <summary>
    /// A class providing various options to <see cref="SubscriptionSet.Add{T}(IQueryable{T}, SubscriptionOptions)"/>.
    /// All the properties in this class are optional.
    /// </summary>
    public class SubscriptionOptions
    {
        /// <summary>
        /// Gets or sets name of the subscription that is being added. This will
        /// be reflected in <see cref="Subscription.Name"/>. If not specified,
        /// an automatic name will be generated from the query.
        /// </summary>
        /// <value>The subscription's name.</value>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the operation should update
        /// an existing subscription with the same name. The default is <c>true</c>.
        /// </summary>
        /// <value>
        /// <c>true</c> if <see cref="SubscriptionSet.Add{T}(IQueryable{T}, SubscriptionOptions)"/>
        /// should have UPSERT semantics, <c>false</c> if you need it to be strictly an INSERT.
        /// </value>
        /// <remarks>
        /// Adding a subscription with the same name and query string is a no-op, regardless
        /// of the value of <see cref="UpdateExisting"/>. This means that if <see cref="Name"/>
        /// is not specified, <see cref="SubscriptionSet.Add{T}(IQueryable{T}, SubscriptionOptions)"/>
        /// will always succeed since the name is derived from the query string. If <see cref="Name"/>
        /// is set to a non-null value and <see cref="UpdateExisting"/> is set to <c>false</c>,
        /// <see cref="SubscriptionSet.Add{T}(IQueryable{T}, SubscriptionOptions)"/> may throw an exception
        /// if the subscription set contains a subscription with the same name, but a different query string.
        /// </remarks>
        public bool UpdateExisting { get; set; } = true;
    }
}
