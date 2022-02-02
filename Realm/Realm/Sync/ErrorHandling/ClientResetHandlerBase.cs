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

using Realms.Sync.Exceptions;

namespace Realms.Sync.ErrorHandling
{
    /// <summary>
    /// In order to specify what strategy to use when a client reset happens, instanciate a subclass of <see cref="ClientResetHandlerBase"/>
    /// and assign it to the chosen subclass of <see cref="SyncConfigurationBase"/> to its <see cref="SyncConfigurationBase.ClientResetHandler"/> field.
    /// </summary>
    /// <seealso href="https://docs.mongodb.com/realm/sdk/dotnet/advanced-guides/client-reset/">Client Resets - .NET SDK</seealso>
    public abstract class ClientResetHandlerBase
    {
        /// <summary>
        /// Callback triggered when a Client Reset error happens in a synchronized Realm.
        /// </summary>
        /// <param name="session">
        /// The <see cref="Session"/> where the error happened on.
        /// </param>
        /// <param name="clientResetException">
        /// The specific <see cref="ClientResetException"/> that holds useful data to be used when trying to manually recover from a client reset.
        /// </param>
        public delegate void ClientResetCallback(Session session, ClientResetException clientResetException);

        internal ClientResetHandlerBase()
        {
        }
    }
}
