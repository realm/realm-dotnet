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

namespace Realms.Sync
{
    /// <summary>
    /// An enumeration, representing the possible state of a sync subscription.
    /// </summary>
    public enum SubscriptionState : sbyte
    {
        /// <summary>
        /// An error occurred while creating the subscription or while the server was processing it.
        /// </summary>
        Error = -1,

        /// <summary>
        /// The subscription has been created, and is waiting to be processed by the server.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// The subscription has been processed by the server, and objects matching the subscription
        /// are now being synchronized to this client.
        /// </summary>
        Complete = 1,

        /// <summary>
        /// The subscription is being created, but has not yet been written to the synced Realm.
        /// </summary>
        Creating = 2,

        /// <summary>
        /// This subscription has been removed.
        /// </summary>
        Invalidated = 3
    }
}
