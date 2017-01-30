////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
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

namespace Realms.Sync.Exceptions
{
    /// <summary>
    /// An exception describing a condition where a reset of the local Realm is required.
    /// </summary>
    public class SessionErrorClientResetException : SessionErrorException
    {
        /// <summary>
        /// Gets the path where the backup copy of the realm will be placed once the client reset process is complete.
        /// </summary>
        public string BackupFilePath { get; }

        private SessionErrorClientResetException()
            : base(null, ErrorCode.DivergingHistories)
        {
        }

        /// <summary>
        /// Initiates the client reset process.
        /// </summary>
        /// <returns><c>true</c> if actions were run successfully, <c>false</c> otherwise.</returns>
        public bool InitiateClientReset()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return false;
        }
    }
}
