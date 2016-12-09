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
    /// Enumeration that specifies how and if logged-in <see cref="User"/> objects are persisted
    /// across application launches.
    /// </summary>
    public enum UserPersistenceMode
    {
        /// <summary>
        /// Persist <see cref="User"/> objects, but do not encrypt them.
        /// </summary>
        NotEncrypted = 0,

        /// <summary>
        /// Persist <see cref="User"/> objects in an encrypted store.
        /// </summary>
        Encrypted,

        /// <summary>
        /// Do not persist <see cref="User"/> objects.
        /// </summary>
        Disabled
    }
}
