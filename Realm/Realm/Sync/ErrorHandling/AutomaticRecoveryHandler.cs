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
    /// A client reset strategy where it is attempted to automatically recover the locally committed data according to some set merge rules.
    /// You can read more about these rules at <see href="https://docs.mongodb.com/realm/sdk/dotnet/advanced-guides/client-reset/">Client Resets - .NET SDK</see>.
    /// </summary>
    /// <remarks>
    /// The merge internally simulates write transactions, meaning that all the changes that take place
    /// are properly propagated through the standard Realm's change notifications.
    /// This strategy supplies three callbacks: <see cref="OnBeforeReset"/>, <see cref="OnAfterReset"/>, and <see cref="ManualResetFallback"/>.
    /// The first two are invoked just before and after the client reset has happened,
    /// while the last one will be invoked in case an error occurs during the automated process and the system needs to fallback to a manual mode.
    /// The overall recommendation for using this strategy is that using the three available callbacks should only be considered when:
    /// 1. the user needs to be notified (in <see cref="OnBeforeReset"/>) that incoming potential data loss of unsynced data
    ///    may happen as a result of the merge that is about to happen
    /// 2. the user needs to be notified (in <see cref="OnAfterReset"/>) that the reset process has completed
    /// 3. advanced use cases for data-sensitive applications where the Realm's chosen merge rules aren't appropriate for the specific use case
    /// 4. backup the whole realm before the client reset happens (in <see cref="OnBeforeReset"/>). Such backup could, for example, be used to restore the unsynced data (see 3.)
    /// </remarks>
    /// <seealso href="https://docs.mongodb.com/realm/sdk/dotnet/advanced-guides/client-reset/">Client Resets - .NET SDK</seealso>
    public sealed class AutomaticRecoveryHandler : ClientResetHandlerBase
    {
        /// <summary>
        /// Gets or sets the callback that indicates a Client Reset is about to happen.
        /// </summary>
        /// <value>Callback invoked right before a Client Reset.</value>
        public BeforeResetCallback OnBeforeReset { get; set; }

        /// <summary>
        /// Gets or sets the callback that indicates a Client Reset just happened.
        /// </summary>
        /// <value>Callback invoked right after a Client Reset.</value>
        public AfterResetCallback OnAfterReset { get; set; }

        /// <summary>
        /// Gets or sets the callback triggered when an error has occurred that makes the operation unable to complete, for example in the case of a destructive schema change.
        /// </summary>
        /// <value>Callback invoked if automatic Client Reset handling fails.</value>
        public ClientResetCallback ManualResetFallback
        {
            get => ManualClientReset;
            set => ManualClientReset = value;
        }
    }
}
