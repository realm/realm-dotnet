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

using System;
using System.Collections.Generic;
using System.Text;

namespace Realms.Sync.ErrorHandling
{
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
        /// When a Client Reset with automatic recovery is attempted but the client is not allowed to use such strategy by the server, then
        /// a Client Reset is re-tried with the fallback discard local strategy. If this second attempt succeeds,
        /// the <see cref="OnAfterDiscardLocalReset"/> callback is called.
        /// </remarks>
        /// <value>Callback invoked right after a Client Reset that fell back to discard local.</value>
        public AfterResetCallback OnAfterDiscardLocalReset { get; set; }

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
