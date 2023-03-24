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

using Realms.Sync.Exceptions;

namespace Realms.Sync.ErrorHandling
{
    /// <summary>
    /// The base class for the different types of client reset handlers. The possible implementations are <see cref="RecoverUnsyncedChangesHandler"/>,
    /// <see cref="DiscardUnsyncedChangesHandler"/> and <see cref="ManualRecoveryHandler"/>.
    /// To use either of them, create a new instance and assign it to <see cref="SyncConfigurationBase.ClientResetHandler"/> on the configuration
    /// you use to open the synchronized <see cref="Realm"/> instance.
    /// </summary>
    /// <seealso href="https://docs.mongodb.com/realm/sdk/dotnet/advanced-guides/client-reset/">Client Resets - .NET SDK</seealso>
    public abstract class ClientResetHandlerBase
    {
        /// <summary>
        /// Callback triggered when a Client Reset error happens in a synchronized Realm.
        /// </summary>
        /// <param name="clientResetException">
        /// The specific <see cref="ClientResetException"/> that holds useful data to be used when trying to manually recover from a client reset.
        /// </param>
        public delegate void ClientResetCallback(ClientResetException clientResetException);

        /// <summary>
        /// Callback that indicates a Client Reset is about to happen.
        /// </summary>
        /// <param name="beforeFrozen">
        /// The frozen <see cref="Realm"/> before the reset.
        /// </param>
        /// <remarks>
        /// The lifetime of the Realm is tied to the callback, so don't store references to the Realm or objects
        /// obtained from it for use outside of the callback. If you need to preserve the state as it was, use
        /// <see cref="Realm.WriteCopy(RealmConfigurationBase)"/> to create a backup.
        /// </remarks>
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
        /// <remarks>
        /// The lifetime of the Realm instances supplied is tied to the callback, so don't store references to
        /// the Realm or objects obtained from it for use outside of the callback. If you need to preserve the
        /// state as it was, use <see cref="Realm.WriteCopy(RealmConfigurationBase)"/> to create a backup.
        /// </remarks>
        public delegate void AfterResetCallback(Realm beforeFrozen, Realm after);

        internal abstract ClientResyncMode ClientResetMode { get; }

        internal ClientResetCallback? ManualClientReset { get; set; }

        internal ClientResetHandlerBase()
        {
        }
    }
}
