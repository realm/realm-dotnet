////////////////////////////////////////////////////////////////////////////
//
// Copyright 2018 Realm Inc.
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

using System.Collections.Generic;

namespace Realms.Sync
{
    /// <summary>
    /// A singleton object which describes Realm-wide permissions.
    /// </summary>
    /// <remarks>
    /// An object of this type is automatically created in the Realm for you, and more objects
    /// cannot be created manually. Call <see cref="Get(Realm)"/> to obtain the
    /// instance for a specific Realm.
    /// </remarks>
    [MapTo("__Realm")]
    [Explicit]
    public class RealmPermission : RealmObject
    {
        [MapTo("id")]
        [PrimaryKey]
        private int Id { get; set; }

        /// <summary>
        /// Gets the permissions for the Realm.
        /// </summary>
        [MapTo("permissions")]
        public IList<Permission> Permissions { get; }

        /// <summary>
        /// Retrieve the singleton object for the given Realm. This will return
        /// <c>null</c> for non-partial Realms.
        /// </summary>
        /// <param name="realm">The Realm instance.</param>
        /// <returns>
        /// A <c>RealmPermission</c> instance that allows you to manipulate the permissions
        /// for this Realm.
        /// </returns>
        public static RealmPermission Get(Realm realm)
        {
            return realm.Find<RealmPermission>(0);
        }

        private RealmPermission()
        {
        }
    }
}
