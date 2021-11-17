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

namespace Realms.Sync
{
    /// <summary>
    /// An enum representing the state of a Realm's subscription set.
    /// </summary>
    public enum SubscriptionSetState : byte
    {
        /// <summary>
        /// The subscription update has been persisted locally, but the server hasn't
        /// yet returned all the data that matched the updated subscription queries.
        /// </summary>
        Pending,

        /// <summary>
        /// The server has acknowledged the subscription and sent all the data that
        /// matched the subscription queries at the time the subscription set was
        /// updated. The server is now in steady-state synchronization mode where it
        /// will stream updates as they come.
        /// </summary>
        Complete,

        /// <summary>
        /// The server has returned an error and synchronization is paused for this
        /// Realm. To view the actual error, use <see cref="SubscriptionSet.Error"/>.
        /// You can still use <see cref="SubscriptionSet.Update"/> to update the
        /// subscriptions and, if the new update doesn't trigger an error, synchronization
        /// will be restarted.
        /// </summary>
        Error,

        /// <summary>
        /// The subscription set has been superceded by an updated one. This typically means
        /// that someone has called <see cref="SubscriptionSet.Update"/> on a different instance
        /// of the <see cref="SubscriptionSet"/>. You should not use a superseded subscription set
        /// and instead obtain a new instance by calling <see cref="Realm.Subscriptions"/>.
        /// </summary>
        Superceded,
    }

}
