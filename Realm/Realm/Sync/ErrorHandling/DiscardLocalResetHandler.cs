////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
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

namespace Realms.Sync.ErrorHandling
{
    /// <summary>
    /// A client reset strategy where all the not yet synchronized data is automatically discarded and a fresh copy of the synchronized Realm is obtained.
    /// </summary>
    /// <remarks>
    /// The freshly downloaded copy of the synchronized Realm triggers all change notifications as a write transaction is internally simulated.
    /// This strategy supplies three callbacks: <see cref="OnBeforeReset"/>, <see cref="OnAfterReset"/>, and <see cref="ManualResetFallback"/>.
    /// The first two are invoked just before and after the client reset has happened,
    /// while the last one will be invoked in case an error occurs during the automated process and the system needs to fallback to a manual mode.
    /// The overall recommendation for using this strategy is that using the three available callbacks should only be considered when:
    /// 1. the user needs to be notified (in <see cref="OnBeforeReset"/>) of an incoming data loss of unsynced data
    /// 2. the user needs to be notified (in <see cref="OnAfterReset"/>) that the reset process has completed
    /// 3. advanced use cases for data-sensitive applications where the developer wants to recover in the most appropriate way the unsynced data
    /// 4. backup the whole realm before the client reset happens (in <see cref="OnBeforeReset"/>). Such backup could, for example, be used to restore the unsynced data (see 3.)
    /// </remarks>
    /// <seealso href="https://docs.mongodb.com/realm/sync/overview/">Sync Overview Docs</seealso>
    public sealed class DiscardLocalResetHandler : ClientResetHandlerBase
    {
        /// <summary>
        /// Callback that indicates a Client Reset is about to happen.
        /// </summary>
        /// <param name="beforeFrozen">
        /// The frozen <see cref="Realm"/> before the reset.
        /// </param>
        public delegate void BeforeResetCallback(Realm beforeFrozen);

        /// <summary>
        /// Callback that indicates a Client Reset has just happened.
        /// </summary>
        /// <param name="beforeFrozen">
        /// The frozen <see cref="Realm"/> as it was before the reset.
        /// </param>
        /// <param name="after">
        /// The <see cref="Realm"/> after the client reset. In order to modify this realm a write transaction needs to be started.
        /// </param>
        public delegate void AfterResetCallback(Realm beforeFrozen, Realm after);

        /// <summary>
        /// Gets or sets the callback that indicates a Client Reset is about to happen.
        /// </summary>
        /// <value>Callback invoked right before a Client Reset.</value>
        public BeforeResetCallback OnBeforeReset { get; set; }

        /// <summary>
        /// Gets or sets the callback that indicates a Client Reset just happened.
        /// Special custom actions can be taken at this point like merging local changes from <value>beforeFrozen</value>.
        /// </summary>
        /// <value>Callback invoked right after a Client Reset.</value>
        public AfterResetCallback OnAfterReset { get; set; }

        /// <summary>
        /// Gets or sets the callback triggered when an error has occurred that makes the operation unable to complete, for example in the case of a destructive schema change.
        /// </summary>
        /// <value>Callback invoked if a Client Reset fails.</value>
        public ClientResetCallback ManualResetFallback
        {
            get
            {
                return ManualClientReset;
            }

            set
            {
                ManualClientReset = value;
            }
        }
    }
}
