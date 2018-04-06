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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Realms.Schema;

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
    [Explicit]
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
        /// Gets or creates a <see cref="Permission"/> instance for the named role
        /// on the Realm.
        /// </summary>
        /// <remarks>
        /// This function should be used in preference to manually querying for the
        /// applicable Permission as it ensures that there is exactly one Permission for
        /// the given Role on the Realm, merging duplicates or creating and adding new ones
        /// as needed.
        /// </remarks>
        /// <param name="role">The <see cref="PermissionRole"/> associated with that Permission.</param>
        /// <param name="realm">The Realm whose permissions this <see cref="Permission"/> instance manipulates.</param>
        /// <returns>
        /// A <see cref="Permission"/> instance that can be used to inspect or modify the Realm
        /// permissions of that <see cref="PermissionRole"/>.
        /// </returns>
        public static Permission Get(PermissionRole role, Realm realm)
        {
            return GetPermissionForRole(role, RealmPermission.Get(realm).Permissions);
        }

        /// <summary>
        /// Gets or creates a <see cref="Permission"/> instance for the named role
        /// on the class.
        /// </summary>
        /// <remarks>
        /// This function should be used in preference to manually querying for the
        /// applicable Permission as it ensures that there is exactly one Permission for
        /// the given Role on the class, merging duplicates or creating and adding new ones
        /// as needed.
        /// </remarks>
        /// <typeparam name="T">
        /// The <see cref="RealmObject"/> subclass whose corresponding class permissions this
        /// <see cref="Permission"/> instance manipulates.
        /// </typeparam>
        /// <param name="role">The <see cref="PermissionRole"/> associated with that Permission.</param>
        /// <param name="realm">The Realm whose class permissions this <see cref="Permission"/> instance manipulates.</param>
        /// <returns>
        /// A <see cref="Permission"/> instance that can be used to inspect or modify the class
        /// permissions of that <see cref="PermissionRole"/>.
        /// </returns>
        public static Permission Get<T>(PermissionRole role, Realm realm) where T : RealmObject
        {
            return GetPermissionForRole(role, ClassPermission.Get<T>(realm).Permissions);
        }

        /// <summary>
        /// Gets or creates a <see cref="Permission"/> instance for the named role
        /// on the class.
        /// </summary>
        /// <remarks>
        /// This function should be used in preference to manually querying for the
        /// applicable Permission as it ensures that there is exactly one Permission for
        /// the given Role on the class, merging duplicates or creating and adding new ones
        /// as needed.
        /// </remarks>
        /// <param name="role">The <see cref="PermissionRole"/> associated with that Permission.</param>
        /// <param name="className">
        /// The  name of the <see cref="RealmObject"/> subclass whose corresponding class permissions this
        /// <see cref="Permission"/> instance manipulates.
        /// </param>
        /// <param name="realm">The Realm whose class permissions this <see cref="Permission"/> instance manipulates.</param>
        /// <returns>
        /// A <see cref="Permission"/> instance that can be used to inspect or modify the class
        /// permissions of that <see cref="PermissionRole"/>.
        /// </returns>
        public static Permission Get(PermissionRole role, string className, Realm realm)
        {
            return GetPermissionForRole(role, ClassPermission.Get(realm, className).Permissions);
        }

        /// <summary>
        /// Gets or creates a <see cref="Permission"/> instance for the named role
        /// on the object.
        /// </summary>
        /// <remarks>
        /// This function should be used in preference to manually querying for the
        /// applicable Permission as it ensures that there is exactly one Permission for
        /// the given Role on the object, merging duplicates or creating and adding new ones
        /// as needed.
        /// <para/>
        /// The given object must have a <c>IList&lt;Permission&gt;</c> property defined on it.
        /// If more than one such property exists, the first one will be used.
        /// </remarks>
        /// <param name="role">The <see cref="PermissionRole"/> associated with that Permission.</param>
        /// <param name="obj">
        /// The <see cref="RealmObject"/> inheritor whose permissions this
        /// <see cref="Permission"/> instance manipulates.
        /// </param>
        /// <returns>
        /// A <see cref="Permission"/> instance that can be used to inspect or modify the object
        /// permissions of that <see cref="PermissionRole"/>.
        /// </returns>
        public static Permission Get(PermissionRole role, RealmObject obj)
        {
            var permissionType = typeof(Permission).GetTypeInfo().GetMappedOrOriginalName();
            var prop = obj.ObjectSchema.FirstOrDefault(o => o.Type == PropertyType.Array && o.ObjectType == permissionType);
            if (prop.Name == null)
            {
                throw new ArgumentException("The given object doesn't have an IList<Permission> property.", nameof(obj));
            }

            var permissions = obj.GetListValue<Permission>(prop.Name);
            return GetPermissionForRole(role, permissions);
        }

        /// <summary>
        /// Gets or creates a <see cref="Permission"/> instance for the named role
        /// on the Realm.
        /// </summary>
        /// <remarks>
        /// This function should be used in preference to manually querying for the
        /// applicable Permission as it ensures that there is exactly one Permission for
        /// the given Role on the Realm, merging duplicates or creating and adding new ones
        /// as needed.
        /// </remarks>
        /// <param name="roleName">
        /// The name of the <see cref="PermissionRole"/> associated with that Permission. If no such
        /// Role exists, it will be created automatically.
        /// </param>
        /// <param name="realm">The Realm whose permissions this <see cref="Permission"/> instance manipulates.</param>
        /// <returns>
        /// A <see cref="Permission"/> instance that can be used to inspect or modify the Realm
        /// permissions of that <see cref="PermissionRole"/>.
        /// </returns>
        public static Permission Get(string roleName, Realm realm)
        {
            return Get(PermissionRole.Get(realm, roleName), realm);
        }

        /// <summary>
        /// Gets or creates a <see cref="Permission"/> instance for the named role
        /// on the class.
        /// </summary>
        /// <remarks>
        /// This function should be used in preference to manually querying for the
        /// applicable Permission as it ensures that there is exactly one Permission for
        /// the given Role on the class, merging duplicates or creating and adding new ones
        /// as needed.
        /// </remarks>
        /// <typeparam name="T">
        /// The <see cref="RealmObject"/> subclass whose corresponding class permissions this
        /// <see cref="Permission"/> instance manipulates.
        /// </typeparam>
        /// <param name="roleName">
        /// The name of the <see cref="PermissionRole"/> associated with that Permission. If no such
        /// Role exists, it will be created automatically.
        /// </param>
        /// <param name="realm">The Realm whose class permissions this <see cref="Permission"/> instance manipulates.</param>
        /// <returns>
        /// A <see cref="Permission"/> instance that can be used to inspect or modify the class
        /// permissions of that <see cref="PermissionRole"/>.
        /// </returns>
        public static Permission Get<T>(string roleName, Realm realm) where T : RealmObject
        {
            return GetPermissionForRole(PermissionRole.Get(realm, roleName), ClassPermission.Get<T>(realm).Permissions);
        }

        /// <summary>
        /// Gets or creates a <see cref="Permission"/> instance for the named role
        /// on the class.
        /// </summary>
        /// <remarks>
        /// This function should be used in preference to manually querying for the
        /// applicable Permission as it ensures that there is exactly one Permission for
        /// the given Role on the class, merging duplicates or creating and adding new ones
        /// as needed.
        /// </remarks>
        /// <param name="roleName">
        /// The name of the <see cref="PermissionRole"/> associated with that Permission. If no such
        /// Role exists, it will be created automatically.
        /// </param>
        /// <param name="className">
        /// The  name of the <see cref="RealmObject"/> subclass whose corresponding class permissions this
        /// <see cref="Permission"/> instance manipulates.
        /// </param>
        /// <param name="realm">The Realm whose class permissions this <see cref="Permission"/> instance manipulates.</param>
        /// <returns>
        /// A <see cref="Permission"/> instance that can be used to inspect or modify the class
        /// permissions of that <see cref="PermissionRole"/>.
        /// </returns>
        public static Permission Get(string roleName, string className, Realm realm)
        {
            return GetPermissionForRole(PermissionRole.Get(realm, roleName), ClassPermission.Get(realm, className).Permissions);
        }

        /// <summary>
        /// Gets or creates a <see cref="Permission"/> instance for the named role
        /// on the object.
        /// </summary>
        /// <remarks>
        /// This function should be used in preference to manually querying for the
        /// applicable Permission as it ensures that there is exactly one Permission for
        /// the given Role on the object, merging duplicates or creating and adding new ones
        /// as needed.
        /// <para/>
        /// The given object must have a <c>IList&lt;Permission&gt;</c> property defined on it.
        /// If more than one such property exists, the first one will be used.
        /// </remarks>
        /// <param name="roleName">
        /// The name of the <see cref="PermissionRole"/> associated with that Permission. If no such
        /// Role exists, it will be created automatically.
        /// </param>
        /// <param name="obj">
        /// The <see cref="RealmObject"/> inheritor whose permissions this
        /// <see cref="Permission"/> instance manipulates.
        /// </param>
        /// <returns>
        /// A <see cref="Permission"/> instance that can be used to inspect or modify the object
        /// permissions of that <see cref="PermissionRole"/>.
        /// </returns>
        public static Permission Get(string roleName, RealmObject obj)
        {
            var permissionType = typeof(Permission).GetTypeInfo().GetMappedOrOriginalName();
            var prop = obj.ObjectSchema.SingleOrDefault(o => o.Type == PropertyType.Array && o.ObjectType == permissionType);
            var permissions = obj.GetListValue<Permission>(prop.Name);
            return GetPermissionForRole(PermissionRole.Get(obj.Realm, roleName), permissions);
        }

        private Permission()
        {
        }

        private static string[] _propertiesToMerge = new[]
        {
            nameof(CanRead),
            nameof(CanUpdate),
            nameof(CanDelete),
            nameof(CanSetPermissions),
            nameof(CanQuery),
            nameof(CanCreate),
            nameof(CanModifySchema),
        };

        internal static Permission GetPermissionForRole(PermissionRole role, IList<Permission> permissions)
        {
#if PCL
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
#else
            if (!(permissions is RealmList<Permission> realmList))
            {
                throw new ArgumentException("Permission.Get may only be called on managed objects.");
            }

            if (!realmList.Realm.IsInTransaction)
            {
                throw new ArgumentException("Permissions may only be obtained or created in a write transaction.");
            }

            Permission result = null;
            var filtered = realmList.Where(p => p.Role.Equals(role)).ToArray();
            foreach (var permission in filtered)
            {
                if (result == null)
                {
                    result = permission;
                }
                else
                {
                    foreach (var propertyToMerge in _propertiesToMerge)
                    {
                        MergePermission(result, permission, propertyToMerge);
                    }

                    realmList.Remove(permission);
                }
            }

            if (result == null)
            {
                result = realmList.Realm.Add(new Permission
                {
                    Role = role
                });
                realmList.Add(result);
            }

            return result;
#endif
        }

        private static void MergePermission(Permission target, Permission source, string propertyName)
        {
            if (!target.GetBooleanValue(propertyName) && source.GetBooleanValue(propertyName))
            {
                target.SetBooleanValue(propertyName, true);
            }
        }
    }
}
