﻿////////////////////////////////////////////////////////////////////////////
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
    public readonly struct SyncProgress
    {
        /// <summary>
        /// Gets the percentage estimate of the current progress, expressed as a double between 0.0 and 1.0.
        /// </summary>
        /// <value>A percentage estimate of the progress.</value>
        public double ProgressEstimate { get; }

        internal SyncProgress(double progressEstimate)
        {
            ProgressEstimate = progressEstimate;
        }

        internal bool IsComplete => ProgressEstimate >= 1.0;
    }
}
