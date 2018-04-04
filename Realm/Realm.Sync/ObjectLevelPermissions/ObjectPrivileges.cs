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
    /// A description of the actual privileges which apply to a specific Object.
    /// </summary>
    /// <remarks>
    /// This is a combination of all of the privileges granted to all of the Roles which the
    /// current User is a member of, obtained by calling <see cref="RealmSyncExtensions.GetPrivileges(Realm, RealmObject)"/>
    /// </remarks>
    [Flags]
    public enum ObjectPrivileges : byte
    {
        /// <summary>
        /// If this flag is not present, the current User is not permitted to read this object directly.
        /// </summary>
        /// <remarks>
        /// Note that Read permissions are transitive, and so it may be possible to read an
        /// object which the user does not directly have Read permissions for by following a
        /// link to it from an object they do have Read permissions for. This does not apply
        /// to any of the other permission types.
        /// </remarks>
        Read = 1 << 0,

        /// <summary>
        /// If this flag is not present, modifying the fields of this type is not permitted. Write
        /// transactions modifying the objects can be performed locally, but any changes made
        /// will be reverted by the server.
        /// </summary>
        /// <remarks>
        /// Note that even if the user has <see cref="Update"/> permission, they may not be able to
        /// modify the <c>IList&lt;Permission&gt;</c> property of the object (if it exists), as that is
        /// governed by <see cref="SetPermissions"/>.
        /// </remarks>
        Update = 1 << 1,

        /// <summary>
        /// If this flag is not present, deleting this object is not permitted. Write transactions which
        /// delete the object can be performed locally, but the server will restore it.
        /// </summary>
        /// <remarks>
        /// It is possible to have <see cref="Update"/> but not <see cref="Delete"/> privileges, or vice
        /// versa. For objects with primary keys, <see cref="Delete"/> but not <see cref="Update"/> is ill-advised
        /// as an object can be updated by deleting and recreating it.
        /// </remarks>
        Delete = 1 << 2,

        /// <summary>
        /// If this flag is not present, modifying the privileges of this specific object is not permitted.
        /// </summary>
        /// <remarks>
        /// Object-specific permissions are set by declaring an <c>IList&lt;Permission&gt;</c>
        /// property on the <see cref="RealmObject"/> subclass. Modifications to this property are
        /// controlled by <see cref="SetPermissions"/> rather than <see cref="Update"/>.
        /// <para/>
        /// Even if this flag is present, note that the user will be unable to grant more
        /// privileges to a Role than what they have themselves, e.g. they won't be able to grant
        /// <see cref="Update"/> if they haven't been granted <see cref="Update"/> first.
        /// </remarks>
        SetPermissions = 1 << 3
    }
}
