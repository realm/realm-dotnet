////////////////////////////////////////////////////////////////////////////
//
// Copyright 2024 Realm Inc.
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

namespace Realms.Sync
{
        /// <summary>
    /// Options for configuring the reconnection delay used by the sync client.
    /// </summary>
    /// <remarks>
    /// The sync client employs an exponential backoff delay strategy when reconnecting to the server.
    /// In order to not spam the network interface the sync client performs an increasing wait before reconnecting.
    /// The wait starts from <see cref="ReconnectDelayInterval"/> and multiplies by <see cref="ReconnectDelayBackoffMultiplier"/>
    /// until it reaches <see cref="MaxReconnectDelayInterval"/>.
    /// </remarks>
    public class ReconnectBackoffOptions
    {
        /// <summary>
        /// Gets or sets the maximum amount of time to wait before a reconnection attempt.
        /// </summary>
        /// <remarks>
        /// Defaults to 5 minutes.
        /// </remarks>
        /// <value>
        /// The maximum amount of time to wait before a reconnection attempt.
        /// </value>
        public TimeSpan MaxReconnectDelayInterval { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Gets or sets the initial amount of time to wait before a reconnection attempt.
        /// </summary>
        /// <remarks>
        /// Defaults to 1 second.
        /// </remarks>
        /// <value>
        /// The initial amount of time to wait before a reconnection attempt.
        /// </value>
        public TimeSpan ReconnectDelayInterval { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Gets or sets the multiplier to apply to the accumulated reconnection delay before a new reconection attempt.
        /// </summary>
        /// <remarks>
        /// Defaults to 2.
        /// </remarks>
        /// <value>
        /// The delay multiplier.
        /// </value>
        public int ReconnectDelayBackoffMultiplier { get; set; } = 2;

        /// <summary>
        /// Gets or sets the jitter randomization factor to apply to the delay.
        /// </summary>
        /// <remarks>
        /// The reconnection delay is subtracted by a value derived from this divisor so that if a lot of clients lose connection and reconnect at the same time the server won't be overwhelmed.
        /// <br />
        /// Defaults to 4.
        /// </remarks>
        /// <value>
        /// The jitter randomization factor to apply to the delay.
        /// </value>
        public int DelayJitterDivisor { get; set; } = 4;
    }
}