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
    /// A description of the actual privileges which apply to a Class within a Realm.
    /// </summary>
    /// <remarks>
    /// This is a combination of all of the privileges granted to all of the Roles which the
    /// current User is a member of, obtained by calling <see cref="PermissionExtensions.GetPrivileges(Realm, string)"/>
    /// or <see cref="PermissionExtensions.GetPrivileges{T}(Realm)"/>.
    /// </remarks>
    [Flags]
    public enum ClassPrivileges : byte
    {
        /// <summary>
        /// If this flag is not present, the current User is not permitted to see objects of this type,
        /// and attempting to query this class will always return empty results.
        /// </summary>
        /// <remarks>
        /// Note that Read permissions are transitive, and so it may be possible to read an
        /// object which the user does not directly have Read permissions for by following a
        /// link to it from an object they do have Read permissions for. This does not apply
        /// to any of the other permission types.
        /// </remarks>
        Read = 1 << 0,

        /// <summary>
        /// If this flag is not present, no modifications to objects of this type are permitted. Write
        /// transactions modifying the objects can be performed locally, but any changes made
        /// will be reverted by the server.
        /// </summary>
        /// <remarks>
        /// Deleting an object is considered a modification, and is governed by this privilege.
        /// </remarks>
        Update = 1 << 1,

        /// <summary>
        /// If this flag is not present, no modifications to the permissions property of the <see cref="ClassPermission"/>
        /// object for this type are permitted. Write transactions can be performed locally,
        /// but any changes made will be reverted by the server.
        /// </summary>
        /// <remarks>
        /// Note that if invalid privilege changes are made, <see cref="PermissionExtensions.GetPrivileges{T}(Realm)"/>
        /// will return results reflecting those invalid changes until synchronization occurs.
        /// <para/>
        /// Even if this flag is present, note that the user will be unable to grant more
        /// privileges to a Role than what they have themselves, e.g. they won't be able to grant
        /// <see cref="Update"/> if they haven't been granted <see cref="Update"/> first.
        /// </remarks>
        SetPermissions = 1 << 3,

        /// <summary>
        /// If this flag is not present, the User is not permitted to create new subscriptions for this class.
        /// Local queries against the objects within the Realm will work, but new
        /// subscriptions will never add objects to the Realm.
        /// </summary>
        Subscribe = 1 << 4,

        /// <summary>
        /// If this flag is not present, creating new objects of this type is not permitted. Write transactions
        /// creating objects can be performed locally, but the objects will be deleted by the
        /// server when synchronization occurs.
        /// </summary>
        /// <remarks>
        /// For objects with Primary Keys, it may not be locally determinable if <see cref="Create"/> or
        /// <see cref="Update"/> privileges are applicable. It may appear that you are creating a new object,
        /// but an object with that Primary Key may already exist and simply not be visible to
        /// you, in which case it is actually an Update operation.
        /// </remarks>
        Create = 1 << 5
    }
}
