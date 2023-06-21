////////////////////////////////////////////////////////////////////////////
//
// Copyright 2023 Realm Inc.
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
    /// Options for configuring timeouts and intervals used by the sync client.
    /// </summary>
    public class SyncTimeoutOptions
    {
        /// <summary>
        /// Gets or sets the maximum amount of time to allow for a connection to
        /// become fully established.
        /// </summary>
        /// <remarks>
        /// This includes the time to resolve the
        /// network address, the TCP connect operation, the SSL handshake, and
        /// the WebSocket handshake.
        /// <br/>
        /// Defaults to 2 minutes.
        /// </remarks>
        /// <value>The connection timeout.</value>
        public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromMinutes(2);

        /// <summary>
        /// Gets or sets the amount of time to keep a connection open after all
        /// sessions have been abandoned.
        /// </summary>
        /// <remarks>
        /// After all synchronized Realms have been closed for a given server, the
        /// connection is kept open until the linger time has expired to avoid the
        /// overhead of reestablishing the connection when Realms are being closed and
        /// reopened.
        /// <br/>
        /// Defaults to 30 seconds.
        /// </remarks>
        /// <value>The time to keep the connection open.</value>
        public TimeSpan ConnectionLingerTime { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Gets or sets how long to wait between each heartbeat ping message.
        /// </summary>
        /// <remarks>
        /// The client periodically sends ping messages to the server to check if the
        /// connection is still alive. Shorter periods make connection state change
        /// notifications more responsive at the cost of battery life (as the antenna
        /// will have to wake up more often).
        /// <br/>
        /// Defaults to 1 minute.
        /// </remarks>
        /// <value>The ping interval.</value>
        public TimeSpan PingKeepAlivePeriod { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Gets or sets how long to wait for a response to a heartbeat ping before
        /// concluding that the connection has dropped.
        /// </summary>
        /// <remarks>
        /// Shorter values will make connection state change notifications more
        /// responsive as it will only change to `disconnected` after this much time has
        /// elapsed, but overly short values may result in spurious disconnection
        /// notifications when the server is simply taking a long time to respond.
        /// <br/>
        /// Defaults to 2 minutes.
        /// </remarks>
        /// <value>The pong timeout.</value>
        public TimeSpan PongKeepAliveTimeout { get; set; } = TimeSpan.FromMinutes(2);

        /// <summary>
        /// Gets or sets the maximum amount of time since the loss of a
        /// prior connection, for a new connection to be considered a "fast
        /// reconnect".
        /// </summary>
        /// <remarks>
        /// When a client first connects to the server, it defers uploading any local
        /// changes until it has downloaded all changesets from the server. This
        /// typically reduces the total amount of merging that has to be done, and is
        /// particularly beneficial the first time that a specific client ever connects
        /// to the server.
        /// <br/>
        /// When an existing client disconnects and then reconnects within the "fact
        /// reconnect" time this is skipped and any local changes are uploaded
        /// immediately without waiting for downloads, just as if the client was online
        /// the whole time.
        /// <br/>
        /// Defaults to 1 minute.
        /// </remarks>
        /// <value>The window in which a drop in connectivity is considered transient.</value>
        public TimeSpan FastReconnectLimit { get; set; } = TimeSpan.FromMinutes(1);
    }
}
