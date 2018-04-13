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

using System;

namespace Realms.Sync
{
    /// <summary>
    /// A description of the actual privileges which apply to a Realm.
    /// </summary>
    /// <remarks>
    /// This is a combination of all of the privileges granted to all of the Roles which the
    /// current User is a member of, obtained by calling <see cref="PermissionExtensions.GetPrivileges(Realm)"/> on
    /// the Realm.
    /// </remarks>
    [Flags]
    public enum RealmPrivileges : byte
    {
        /// <summary>
        /// If this flag is not present, the current User is not permitted to see the Realm at all. This can
        /// happen only if the Realm was created locally and has not yet been synchronized.
        /// </summary>
        Read = 1 << 0,

        /// <summary>
        /// If this flag is not present, no modifications to the Realm are permitted. Write transactions can
        /// be performed locally, but any changes made will be reverted by the server. <see cref="SetPermissions"/>
        /// and <see cref="ModifySchema"/> will always be denied when this is denied.
        /// </summary>
        Update = 1 << 1,

        /// <summary>
        /// If this flag is not present, no modifications to the permissions property of the <see cref="RealmPermission"/>
        /// object for are permitted. Write transactions can be performed locally, but any
        /// changes made will be reverted by the server.
        /// </summary>
        /// <remarks>
        /// Note that if invalid privilege changes are made, <see cref="PermissionExtensions.GetPrivileges(Realm)"/>
        /// will return results reflecting those invalid changes until synchronization occurs.
        /// <para/>
        /// Even if this flag is present, note that the user will be unable to grant more
        /// privileges to a Role than what they have themselves, e.g. they won't be able to grant
        /// <see cref="Update"/> if they haven't been granted <see cref="Update"/> first.
        /// <para/>
        /// Adding or removing Users from a Role is controlled by <see cref="Update"/> privileges on that
        /// Role, and not by this value.
        /// </remarks>
        SetPermissions = 1 << 3,

        /// <summary>
        /// If this flag is not present, the user is not permitted to add new object types to the Realm or add
        /// new properties to existing object types.
        /// </summary>
        /// <remarks>
        /// Defining new <see cref="RealmObject"/> subclasses (and not
        /// excluding them from the schema with <see cref="RealmConfigurationBase.ObjectClasses"/> will result
        /// in the application crashing if the object types are not first added on the server by a more privileged
        /// user.
        /// </remarks>
        ModifySchema = 1 << 6
    }
}
