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
    /// Access levels which can be granted to Realm Platform users for specific synchronized Realms, using the permissions APIs.
    /// <br/>
    /// Note that each access level guarantees all allowed actions provided by less permissive access levels.
    /// Specifically, users with write access to a Realm can always read from that Realm, and users with administrative
    /// access can always read or write from the Realm.
    /// </summary>
    public enum AccessLevel
    {
        /// <summary>
        /// No access whatsoever.
        /// </summary>
        None,

        /// <summary>
        /// User can only read the contents of the Realm.
        /// </summary>
        /// <remarks>
        /// Users who have read-only access to a Realm should open the Realm using
        /// <see cref="Realm.GetInstanceAsync(RealmConfigurationBase)"/> Attempting to directly open the Realm is
        /// an error; in this case the Realm must be deleted and re-opened.
        /// </remarks>
        Read,

        /// <summary>
        /// User can read and write the contents of the Realm.
        /// </summary>
        Write,

        /// <summary>
        /// User can read, write, and administer the Realm, including granting permissions to other users.
        /// </summary>
        Admin
    }
}
