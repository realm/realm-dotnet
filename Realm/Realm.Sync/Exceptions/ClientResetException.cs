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

using System.Collections.Generic;

namespace Realms.Sync.Exceptions
{
    /// <summary>
    /// An exception describing a condition where a reset of the local Realm is required.
    /// </summary>
    public class ClientResetException : SessionException
    {
        private const string OriginalFilePathKey = "ORIGINAL_FILE_PATH";
        private const string BackupFilePathKey = "RECOVERY_FILE_PATH";

        private readonly string _originalFilePath;

        /// <summary>
        /// Gets the path where the backup copy of the realm will be placed once the client reset process is complete.
        /// </summary>
        /// <value>The path to the backup realm.</value>
        public string BackupFilePath { get; }

        internal ClientResetException(string message, IDictionary<string, string> userInfo)
            : base(message, ErrorCode.DivergingHistories)
        {
            _originalFilePath = userInfo[OriginalFilePathKey];
            BackupFilePath = userInfo[BackupFilePathKey];
        }

        /// <summary>
        /// Initiates the client reset process.
        /// </summary>
        /// <returns><c>true</c> if actions were run successfully, <c>false</c> otherwise.</returns>
        public bool InitiateClientReset()
        {
            return SharedRealmHandleExtensions.ImmediatelyRunFileActions(_originalFilePath);
        }
    }
}
