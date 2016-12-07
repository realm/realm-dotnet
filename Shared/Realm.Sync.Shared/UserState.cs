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

namespace Realms.Sync
{
    /// <summary>
    /// The state of the user object.
    /// </summary>
    public enum UserState
    {
        /// <summary>
        /// The user is logged out. Call <see cref="User.LoginAsync"/> with valid credentials to log the user back in.
        /// </summary>
        LoggedOut,

        /// <summary>
        /// The user is logged in, and any Realms associated with it are synchronizing with the Realm Object Server.
        /// </summary>
        Active,

        /// <summary>
        /// The user has encountered a fatal error state, and cannot be used.
        /// </summary>
        Error
    }
}