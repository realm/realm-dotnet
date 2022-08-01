﻿////////////////////////////////////////////////////////////////////////////
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
    /// A client reset strategy that attempts to automatically recover any unsynchronized changes.
    /// If that fails, this handler fallsback to the discard local strategy.
    /// </summary>
    /// <remarks>
    /// The automatic recovery fails when a client that is configured for recovery is flagged on the server "as not allowed to execute automatic recovery".
    /// In this situation this strategy fallsback to the discard local one.
    /// To reiterate what it does: it discards all the local changes and uses the latest realm that is available on the remote sync server.
    /// You can read more about the automatic merge rules at <see href="https://docs.mongodb.com/realm/sdk/dotnet/advanced-guides/client-reset/">Client Resets - .NET SDK</see>.
    /// The automatic recovery mechanism creates write transactions meaning that all the changes that take place
    /// are properly propagated through the standard Realm's change notifications.
    /// The <see cref="AutomaticOrDiscardRecoveryHandler"/> strategy supplies four callbacks: <see cref="OnBeforeReset"/>, <see cref="OnAfterAutomaticReset"/>,
    /// <see cref="OnAfterDiscardLocalReset"/> and <see cref="ManualResetFallback"/>.
    /// The first callback is invoked just before the client reset happens. While the second and third callbacks are respectively called after an automatic
    /// client reset succeeded, or the automatic client reset failed and instead the discard local one succeded.
    /// The last callback is invoked whenever an error occurs in either of the recovery stragegies and the system needs to fallback to a manual mode.
    /// The overall recommendation for using this strategy is that using the three available callbacks should only be considered when:
    /// 1. The user needs to be notified (in <see cref="OnBeforeReset"/>) of an incoming potential data loss
    ///    of unsynced data as a result of a merge or a complete discard of local changes
    /// 2. The user needs to be notified (in <see cref="OnAfterAutomaticReset"/> or <see cref="OnAfterDiscardLocalReset"/>)
    ///    that the reset process has completed
    /// 3. Advanced use cases for data-sensitive applications where the developer wants
    ///    to recover in the most appropriate way the unsynced data
    /// 4. Backup the whole realm before the client reset happens (in <see cref="OnBeforeReset"/>).
    ///    Such backup could, for example, be used to restore the unsynced data (see 3.)
    /// </remarks>
    /// <seealso href="https://docs.mongodb.com/realm/sdk/dotnet/advanced-guides/client-reset/">Client Resets - .NET SDK</seealso>
    public sealed class AutomaticOrDiscardRecoveryHandler : ClientResetHandlerBase
    {
        internal override ClientResyncMode ClientResetMode => ClientResyncMode.AutomaticRecoveryOrDiscardLocal;

        /// <summary>
        /// Gets or sets the callback that indicates a Client Reset is about to happen.
        /// </summary>
        /// <value>Callback invoked right before a Client Reset.</value>
        public BeforeResetCallback OnBeforeReset { get; set; }

        /// <summary>
        /// Gets or sets the callback that indicates that an automatic Client Reset just happened.
        /// </summary>
        /// <value>Callback invoked right after a Client Reset.</value>
        public AfterResetCallback OnAfterAutomaticReset { get; set; }

        /// <summary>
        /// Gets or sets the callback that indicates that the discard local fallback for a Client Reset just happened.
        /// </summary>
        /// <remarks>
        /// When a Client Reset with automatic recovery is attempted but the client is not allowed to use such strategy by the server,
        /// then a Client Reset is re-tried with the fallback discard local strategy. If this second attempt succeeds,
        /// the <see cref="OnAfterDiscardLocalReset"/> callback is called.
        /// </remarks>
        /// <value>Callback invoked right after a Client Reset that fell back to discard local.</value>
        public AfterResetCallback OnAfterDiscardLocalReset { get; set; }

        /// <summary>
        /// Gets or sets the callback triggered when an error has occurred that makes the operation unable to complete,
        /// for example in the case of a destructive schema change.
        /// </summary>
        /// <value>Callback invoked if automatic Client Reset handling fails.</value>
        public ClientResetCallback ManualResetFallback
        {
            get => ManualClientReset;
            set => ManualClientReset = value;
        }
    }
}
