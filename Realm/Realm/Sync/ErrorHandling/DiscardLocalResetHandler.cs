﻿////////////////////////////////////////////////////////////////////////////
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

namespace Realms.Sync.ErrorHandling
{
    public class DiscardLocalResetHandler : ClientResetHandlerBase
    {
        /// <summary>
        /// Callback that indicates a Client Reset is about to happen.
        /// </summary>
        /// <param name="beforeFrozen">
        /// The frozen <see cref="Realm"/> before the reset.
        /// </param>
        public delegate void BeforeResetCallback(Realm beforeFrozen);

        /// <summary>
        /// Callback that indicates a Client Reset is about to happen.
        /// </summary>
        /// <param name="beforeFrozen">
        /// The frozen <see cref="Realm"/> as it was before the reset.
        /// </param>
        /// <param name="after">
        /// The <see cref="Realm"/> after the client reset.
        /// </param>
        public delegate void AfterResetCallback(Realm beforeFrozen, Realm after);

        /// <summary>
        /// Gets or sets the callback that indicates a Client Reset is about to happen.
        /// Among other things, you can use this call to temporarily store the before Realm as a backup and in the <see cref="OnAfterReset"/> callback merge the changes, if necessary.
        /// </summary>
        public BeforeResetCallback OnBeforeReset { get; set; }

        /// <summary>
        /// Gets or sets the callback that indicates a Client Reset just happened. Special custom actions can be taken at this point like merging local changes if the "before" realm was stored during <see cref="BeforeResetCallback"/>.
        /// </summary>
        public AfterResetCallback OnAfterReset { get; set; }

        /// <summary>
        /// Gets or sets the callback triggered when an error has occurred that makes the operation unable to complete, for example in the case of a destructive schema change.
        /// </summary>
        public ClientResetCallback ManualResetFallback { get; set; }
    }
}
