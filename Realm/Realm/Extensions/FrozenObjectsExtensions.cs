////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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
using System.Linq;
using Realms.Exceptions;
using Realms.Helpers;

namespace Realms
{
    /// <summary>
    /// A set of extension methods on top of RealmObjectBase.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class FrozenObjectsExtensions
    {
        /// <summary>
        /// Returns a frozen snapshot of this object. The frozen copy can be read and queried from any thread without throwing an exception.
        /// <para/>
        /// Freezing a RealmObjectBase also creates a frozen Realm which has its own lifecycle, but if the live Realm that spawned the
        /// original object is fully closed (i.e. all instances across all threads are closed), the frozen Realm and
        /// object will be closed as well.
        /// <para/>
        /// Frozen objects can be queried as normal, but trying to mutate it in any way or attempting to register a listener will
        /// throw a <see cref="Exceptions.RealmFrozenException"/>.
        /// <para/>
        /// Note: Keeping a large number of frozen objects with different versions alive can have a negative impact on the filesize
        /// of the Realm. In order to avoid such a situation it is possible to set <see cref="RealmConfigurationBase.MaxNumberOfActiveVersions"/>.
        /// </summary>
        /// <param name="realmObj">The <see cref="RealmObject"/> or <see cref="EmbeddedObject"/> instance that you want to create a frozen version of.</param>
        /// <typeparam name="T">The type of the <see cref="RealmObject"/>/<see cref="EmbeddedObject"/>.</typeparam>
        /// <returns>A new frozen instance of the passed in object or the object itself if it was already frozen.</returns>
        /// <seealso cref="RealmObjectBase.FreezeInPlace"/>
        public static T Freeze<T>(this T realmObj)
            where T : RealmObjectBase
        {
            Argument.NotNull(realmObj, nameof(realmObj));

            if (realmObj.IsFrozen)
            {
                return realmObj;
            }

            var (realm, objectHandle) = realmObj.FreezeImpl();
            return (T)realm.MakeObject(realmObj.ObjectMetadata, objectHandle);
        }

        /// <summary>
        /// Creates a frozen snapshot of this list. The frozen copy can be read and iterated over from any thread. If the list is
        /// not managed, a <see cref="RealmException"/> will be thrown.
        /// <para/>
        /// Freezing a list also creates a frozen Realm which has its own lifecycle, but if the live Realm that spawned the
        /// original list is fully closed (i.e. all instances across all threads are closed), the frozen Realm and
        /// list will be closed as well.
        /// <para/>
        /// Frozen lists can be read and iterated as normal, but trying to mutate it in any way or attempting to register a listener will
        /// throw a <see cref="RealmFrozenException"/>.
        /// <para/>
        /// Note: Keeping a large number of frozen objects with different versions alive can have a negative impact on the filesize
        /// of the Realm. In order to avoid such a situation it is possible to set <see cref="RealmConfigurationBase.MaxNumberOfActiveVersions"/>.
        /// </summary>
        /// <param name="list">The list you want to create a frozen copy of.</param>
        /// <typeparam name="T">Type of the objects in the list.</typeparam>
        /// <returns>A frozen copy of this list.</returns>
        public static IList<T> Freeze<T>(this IList<T> list)
        {
            Argument.NotNull(list, nameof(list));

            if (list is RealmList<T> realmList)
            {
                return (RealmList<T>)realmList.Freeze();
            }

            throw new RealmException("Unmanaged lists cannot be frozen.");
        }

        /// <summary>
        /// Creates a frozen snapshot of this query. The frozen copy can be read and queried from any thread. If the query is
        /// not managed (i.e. not a result of <see cref="Realm.All{T}"/> invocation), a <see cref="RealmException"/> will be thrown.
        /// <para/>
        /// Freezing a query also creates a frozen Realm which has its own lifecycle, but if the live Realm that spawned the
        /// original query is fully closed (i.e. all instances across all threads are closed), the frozen Realm and
        /// query will be closed as well.
        /// <para/>
        /// Frozen queries can be read and iterated as normal, but trying to mutate it in any way or attempting to register a listener will
        /// throw a <see cref="RealmFrozenException"/>.
        /// <para/>
        /// Note: Keeping a large number of frozen objects with different versions alive can have a negative impact on the filesize
        /// of the Realm. In order to avoid such a situation it is possible to set <see cref="RealmConfigurationBase.MaxNumberOfActiveVersions"/>.
        /// </summary>
        /// <param name="query">The query you want to create a frozen copy of.</param>
        /// <typeparam name="T">The type of the <see cref="RealmObject"/>/<see cref="EmbeddedObject"/> in the query.</typeparam>
        /// <returns>A frozen copy of this query.</returns>
        public static IQueryable<T> Freeze<T>(this IQueryable<T> query)
            where T : RealmObjectBase
        {
            Argument.NotNull(query, nameof(query));

            if (query is RealmResults<T> realmResults)
            {
                return (RealmResults<T>)realmResults.Freeze();
            }

            throw new RealmException("Unmanaged queries cannot be frozen.");
        }
    }
}
