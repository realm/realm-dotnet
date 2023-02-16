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

namespace Realms.Sync.ErrorHandling
{
    /// <summary>
    /// A client reset strategy where the user needs to fully take care of a client reset.
    /// </summary>
    /// <seealso href="https://docs.mongodb.com/realm/sdk/dotnet/advanced-guides/client-reset/">Client Resets - .NET SDK</seealso>
    public sealed class ManualRecoveryHandler : ClientResetHandlerBase
    {
        /// <summary>
        /// Gets the callback to manually handle a Client Reset.
        /// A Client Reset should be handled as quickly as possible as any further changes to the Realm will not be synchronized with the server and
        /// must be moved manually from the backup Realm to the new one.
        /// </summary>
        /// <value>Callback invoked on Client Reset.</value>
        public ClientResetCallback OnClientReset => ManualClientReset;

        internal override ClientResyncMode ClientResetMode => ClientResyncMode.Manual;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManualRecoveryHandler"/> class with the supplied client reset handler.
        /// </summary>
        /// <param name="onClientReset">
        /// Callback triggered when a manual client reset happens.
        /// </param>
        public ManualRecoveryHandler(ClientResetCallback onClientReset)
        {
            ManualClientReset = onClientReset;
        }
    }
}
