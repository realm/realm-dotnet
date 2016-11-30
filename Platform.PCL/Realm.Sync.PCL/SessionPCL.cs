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

using System;

namespace Realms.Sync
{
    /// <summary>
    /// An object encapsulating a Realm Object Server session. Sessions represent the communication between the client (and a local Realm file on disk), and the server (and a remote Realm at a given URL stored on a Realm Object Server).
    /// Sessions are always created by the SDK and vended out through various APIs. The lifespans of sessions associated with Realms are managed automatically.
    /// </summary>
    public class Session
    {
        /// <summary>
        /// Gets the <see cref="SyncConfiguration"/> that is responsible for controlling the session.
        /// </summary>
        public SyncConfiguration Configuration
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return null;
            }
        }

        /// <summary>
        /// Gets the <see cref="Uri"/> describing the remote Realm which this session connects to and synchronizes changes with.
        /// </summary>
        public Uri ServerUri
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return null;
            }
        }

        /// <summary>
        /// Gets the session’s current state.
        /// </summary>
        /// <value>The state.</value>
        public SessionState State
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return default(SessionState);
            }
        }

        /// <summary>
        /// Gets the <see cref="User"/> defined by the <see cref="SyncConfiguration"/> that is used to connect to the Realm Object Server.
        /// </summary>
        public User User
        {
            get
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return null;
            }
        }
    }
}
