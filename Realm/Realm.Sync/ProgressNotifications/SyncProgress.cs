////////////////////////////////////////////////////////////////////////////
//
// Copyright 2017 Realm Inc.
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

namespace Realms.Sync
{
    /// <summary>
    /// A struct containing information about the progress state at a given instant.
    /// </summary>
    public struct SyncProgress
    {
        /// <summary>
        /// Gets the number of bytes that have been transferred since subscribing for progress notifications.
        /// </summary>
        /// <value>The number of transferred bytes.</value>
        public ulong TransferredBytes { get; }

        /// <summary>
        /// Gets the total number of bytes that have to be transferred since subscribing for progress notifications.
        /// The difference between that number and <see cref="TransferredBytes"/> gives you the number of bytes not yet
        /// transferred. If the difference is 0, then all changes at the instant the callback fires have been
        /// successfully transferred.
        /// </summary>
        /// <value>The number of transferable bytes.</value>
        public ulong TransferableBytes { get; }

        internal SyncProgress(ulong transferred, ulong transferable)
        {
            TransferredBytes = transferred;
            TransferableBytes = transferable;
        }
    }
}
