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
using System.ComponentModel;

namespace Realms.Sync
{
    /// <summary>
    /// A set of extension methods that simplify working with object level permissions.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class PermissionExtensions
    {
        /// <summary>
        /// Returns the computed privileges which the current user has for this Realm.
        /// <para/>
        /// This combines all privileges granted on the Realm by all Roles which the
        /// current User is a member of into the final privileges which will be
        /// enforced by the server.
        /// <para/>
        /// The privilege calculation is done locally using cached data, and inherently
        /// may be stale.It is possible that this method may indicate that an
        /// operation is permitted but the server will still reject it if permission is
        /// revoked before the changes have been integrated on the server.
        /// <para/>
        /// Non-synchronized Realms always have permission to perform all operations.
        /// </summary>
        /// <param name="realm">The Realm whose privileges are inspected.</param>
        /// <returns>The privileges which the current user has for the current Realm.</returns>
        public static RealmPrivileges GetPrivileges(this Realm realm)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return default(RealmPrivileges);
        }

        /// <summary>
        /// Returns the computed privileges which the current user has for the given class.
        /// <para/>
        /// This combines all privileges granted on the class by all Roles which the
        /// current User is a member of into the final privileges which will be
        /// enforced by the server.
        /// <para/>
        /// The privilege calculation is done locally using cached data, and inherently
        /// may be stale. It is possible that this method may indicate that an
        /// operation is permitted but the server will still reject it if permission is
        /// revoked before the changes have been integrated on the server.
        /// <para/>
        /// Non-synchronized Realms always have permission to perform all operations.
        /// </summary>
        /// <typeparam name="T">The <see cref="RealmObject"/> inheritor to get the privileges for.</typeparam>
        /// <param name="realm">The Realm whose privileges are inspected.</param>
        /// <returns>The privileges which the current user has for the given class.</returns>
        public static ClassPrivileges GetPrivileges<T>(this Realm realm) where T : RealmObject
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return default(ClassPrivileges);
        }

        /// <summary>
        /// Returns the computed privileges which the current user has for the given class.
        /// <para/>
        /// This combines all privileges granted on the class by all Roles which the
        /// current User is a member of into the final privileges which will be
        /// enforced by the server.
        /// <para/>
        /// The privilege calculation is done locally using cached data, and inherently
        /// may be stale. It is possible that this method may indicate that an
        /// operation is permitted but the server will still reject it if permission is
        /// revoked before the changes have been integrated on the server.
        /// <para/>
        /// Non-synchronized Realms always have permission to perform all operations.
        /// </summary>
        /// <param name="realm">The Realm whose privileges are inspected.</param>
        /// <param name="className">The name of a <see cref="RealmObject"/> inheritor to get the privileges for.</param>
        /// <returns>The privileges which the current user has for the given class.</returns>
        public static ClassPrivileges GetPrivileges(this Realm realm, string className)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return default(ClassPrivileges);
        }

        /// <summary>
        /// Returns the computed privileges which the current user has for the given object.
        /// </summary>
        /// <remarks>
        /// This combines all privileges granted on the object by all Roles which the
        /// current User is a member of into the final privileges which will be
        /// enforced by the server.
        /// <para/>
        /// The privilege calculation is done locally using cached data, and inherently
        /// may be stale. It is possible that this method may indicate that an
        /// operation is permitted but the server will still reject it if permission is
        /// revoked before the changes have been integrated on the server.
        /// <para/>
        /// Non-synchronized Realms always have permission to perform all operations.
        /// <para/>
        /// The object must be a valid object managed by this Realm. Passing in an
        /// invalidated object, an unmanaged object, or an object managed by a
        /// different Realm will throw an exception.
        /// </remarks>
        /// <param name="realm">The Realm whose privileges are inspected.</param>
        /// <param name="obj">A managed object to get the privileges for.</param>
        /// <returns>The privileges which the current user has for the given object.</returns>
        public static ObjectPrivileges GetPrivileges(this Realm realm, RealmObject obj)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return default(ObjectPrivileges);
        }

        /// <summary>
        /// A convenience method that converts a <see cref="User"/> to
        /// <see cref="PermissionUser"/> and adds it to the list of users
        /// if necessary. If a <see cref="PermissionUser"/> with that identity
        /// already belongs to the collection, this method will be a no-op.
        /// </summary>
        /// <param name="users">
        /// The collection of users to which <c>user</c> will be added.
        /// </param>
        /// <param name="user">The user to add.</param>
        public static void Add(this IList<PermissionUser> users, User user)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }
    }
}
