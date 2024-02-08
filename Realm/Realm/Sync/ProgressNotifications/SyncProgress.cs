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

using System;

namespace Realms.Sync
{
    /// <summary>
    /// A struct containing information about the progress state at a given instant.
    /// </summary>
    public readonly struct SyncProgress
    {
        /// <summary>
        /// Gets the number of bytes that have been transferred since subscribing for progress notifications.
        /// </summary>
        /// <value>The number of transferred bytes.</value>
        [Obsolete("Not accurate, use ProgressEstimate instead.")]
        public ulong TransferredBytes { get; }

        /// <summary>
        /// Gets the total number of bytes that have to be transferred since subscribing for progress notifications.
        /// The difference between that number and <see cref="TransferredBytes"/> gives you the number of bytes not yet
        /// transferred. If the difference is 0, then all changes at the instant the callback fires have been
        /// successfully transferred.
        /// </summary>
        /// <value>The number of transferable bytes.</value>
        [Obsolete("Not accurate, use ProgressEstimate instead.")]
        public ulong TransferableBytes { get; }

        /// <summary>
        /// Gets the percentage estimate of the current progress, expressed as a float between 0.0 and 1.0.
        /// </summary>
        /// <value>A percentage estimate of the progress.</value>
        public double ProgressEstimate { get; }

        internal SyncProgress(ulong transferred, ulong transferable, double progressEstimate)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            TransferredBytes = transferred;
            TransferableBytes = transferable;
#pragma warning restore CS0618 // Type or member is obsolete
            ProgressEstimate = progressEstimate;
        }

        internal readonly bool IsComplete => ProgressEstimate >= 1.0;
    }
}
