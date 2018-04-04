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
using System.Reflection;

namespace Realms.Sync
{
    /// <summary>
    /// An object which describes class-wide permissions.
    /// </summary>
    /// <remarks>
    /// An instance of this object is automatically created in the Realm for class in your schema,
    /// and should not be created manually. Call <see cref="Get{T}(Realm)"/> or
    /// <see cref="Get(Realm, string)"/> to obtain the existing instance, or query
    /// <see cref="ClassPermission"/> as normal.
    /// </remarks>
    [MapTo("__Class")]
    public class ClassPermission : RealmObject
    {
        /// <summary>
        /// Gets the name of the class which these permissions apply to.
        /// </summary>
        [MapTo("name")]
        [PrimaryKey]
        [Required]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the permissions for this class.
        /// </summary>
        [MapTo("permissions")]
        public IList<Permission> Permissions { get; }

        /// <summary>
        /// Retrieves the <see cref="ClassPermission"/> for the given
        /// <see cref="RealmObject"/> subclass. This will return <c>null</c> for non-partial Realms.
        /// </summary>
        /// <typeparam name="T">
        /// The <see cref="RealmObject"/> subclass whose corresponding <see cref="ClassPermission"/>
        /// will be obtained.
        /// </typeparam>
        /// <param name="realm">The Realm instance.</param>
        /// <returns>
        /// A <c>ClassPermission</c> instance that allows you to manipulate the permissions
        /// for this class.
        /// </returns>
        public static ClassPermission Get<T>(Realm realm) where T : RealmObject
        {
            return Get(realm, typeof(T).GetTypeInfo().GetMappedOrOriginalName());
        }

        /// <summary>
        /// Retrieves the <see cref="ClassPermission"/> for the given class name.
        /// This will return <c>null</c> for non-partial Realms.
        /// </summary>
        /// <param name="realm">The Realm instance.</param>
        /// <param name="className">
        /// The name of a <see cref="RealmObject"/> subclass whose corresponding <see cref="ClassPermission"/>
        /// will be obtained.
        /// </param>
        /// <returns>
        /// A <c>ClassPermission</c> instance that allows you to manipulate the permissions
        /// for this class.
        /// </returns>
        public static ClassPermission Get(Realm realm, string className)
        {
            return realm.Find<ClassPermission>(className);
        }

        private ClassPermission()
        {
        }
    }
}
