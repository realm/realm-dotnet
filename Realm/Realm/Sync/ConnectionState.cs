﻿////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
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
    /// The current connection state of a sync session object.
    /// </summary>
    public enum ConnectionState : byte
    {
        /// <summary>
        /// The session is disconnected from Atlas Device Sync.
        /// </summary>
        Disconnected = 0,

        /// <summary>
        /// The session is connecting to the Atlas Device Sync.
        /// </summary>
        Connecting = 1,

        /// <summary>
        /// The session is connected to the Atlas Device Sync.
        /// </summary>
        Connected = 2
    }
}
