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

using System.Linq;

namespace Realms.Sync
{
    /// <summary>
    /// A representation of a sync user within the permissions system.
    /// </summary>
    /// <remarks>
    /// <see cref="PermissionUser"/> objects are created automatically for each sync user which connects
    /// to a Realm, and can also be created manually if you wish to grant permissions to a user
    /// which has not yet connected to this Realm.
    /// </remarks>
    [MapTo("__User")]
    [Explicit]
    public class PermissionUser : RealmObject
    {
        /// <summary>
        /// Gets the unique Realm Object Server user ID string identifying this user. This will have
        /// the same value as <see cref="User.Identity"/>.
        /// </summary>
        [MapTo("id")]
        [PrimaryKey]
        [Required]
        public string Identity { get; private set; }

        /// <summary>
        /// Gets the Roles which this user belongs to.
        /// </summary>
        [MapTo("roles")]
        [Backlink(nameof(PermissionRole.Users))]
        public IQueryable<PermissionRole> Roles { get; }

        /// <summary>
        /// Gets or creates a <see cref="PermissionUser"/> with the specified identity.
        /// </summary>
        /// <param name="realm">The Realm instance.</param>
        /// <param name="identity">The Realm Object Server user ID.</param>
        /// <returns>
        /// A <see cref="PermissionUser"/> instance that can be added to one or more <see cref="PermissionRole"/>s.
        /// </returns>
        public static PermissionUser Get(Realm realm, string identity)
        {
            var user = realm.Find<PermissionUser>(identity);
            if (user != null)
            {
                return user;
            }

            return realm.Add(new PermissionUser { Identity = identity });
        }

        private PermissionUser()
        {
        }
    }
}
