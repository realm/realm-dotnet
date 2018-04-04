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

namespace Realms.Sync
{
    /// <summary>
    /// A permission which can be applied to a Realm, Class, or specific Object.
    /// </summary>
    /// <remarks>
    /// Permissions are applied by adding the permission to the <see cref="RealmPermission"/> singleton
    /// object, the <see cref="ClassPermission"/> object for the desired class, or to a user-defined
    /// <c>IList&lt;Permission&gt;</c> property on a specific Object instance. The meaning of each of
    /// the properties of <see cref="Permission"/> depend on what the permission is applied to, and so are
    /// left undocumented here. See <see cref="RealmPrivileges"/>, <see cref="ClassPrivileges"/>, and
    /// <see cref="ObjectPrivileges"/> for details about what each of the properties mean when applied to
    /// that type.
    /// </remarks>
    [MapTo("__Permission")]
    public class Permission : RealmObject
    {
        /// <summary>
        /// Gets the Role which this Permission applies to. All users within the Role are
        /// granted the permissions specified by the fields below any
        /// objects/classes/realms which use this <see cref="Permission"/>.
        /// </summary>
        [MapTo("role")]
        public PermissionRole Role { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user can read the object to which this
        /// <see cref="Permission"/> is attached.
        /// </summary>
        [MapTo("canRead")]
        public bool CanRead { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user can modify the object to which this <see cref="Permission"/> is attached.
        /// </summary>
        [MapTo("canUpdate")]
        public bool CanUpdate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user can delete the object to which this <see cref="Permission"/> is attached.
        /// </summary>
        /// <remarks>
        /// This field is only applicable to Permissions attached to Objects, and not to Realms or Classes.
        /// </remarks>
        [MapTo("canDelete")]
        public bool CanDelete { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user can add or modify Permissions for the object which this
        /// <see cref="Permission"/> is attached to.
        /// </summary>
        [MapTo("canSetPermissions")]
        public bool CanSetPermissions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user can subscribe to queries for this object type.
        /// </summary>
        /// <remarks>
        /// This field is only applicable to Permissions attached to Classes, and not to Realms or Objects.
        /// </remarks>
        [MapTo("canQuery")]
        public bool CanQuery { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user can create new objects of the type this <see cref="Permission"/> is attached to.
        /// </summary>
        /// <remarks>
        /// This field is only applicable to Permissions attached to Classes, and not to Realms or Objects.
        /// </remarks>
        [MapTo("canCreate")]
        public bool CanCreate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user can modify the schema of the Realm which this
        /// <see cref="Permission"/> is attached to.
        /// </summary>
        /// <remarks>
        /// This field is only applicable to Permissions attached to Realms, and not to Realms or Objects.
        /// </remarks>
        [MapTo("canModifySchema")]
        public bool CanModifySchema { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Permission"/> class.
        /// </summary>
        /// <param name="role">
        /// The <see cref="PermissionRole"/> this <see cref="Permission"/> will apply to.
        /// </param>
        public Permission(PermissionRole role)
        {
            Role = role;
        }

        private Permission()
        {
        }
    }
}
