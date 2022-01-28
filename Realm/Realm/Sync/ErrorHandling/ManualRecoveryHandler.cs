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

namespace Realms.Sync.ErrorHandling
{
    public class ManualRecoveryHandler : ClientResetHandlerBase
    {
        /// <summary>
        /// Callback that indicates a Client Reset has happened.
        /// This should be handled as quickly as possible as any further changes to the Realm will not be synchronized with the server and must be moved manually from the backup Realm to the new one.
        /// </summary>
        public ClientResetCallback OnClientReset { get; set; }
    }
}
