////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Realms.Helpers;

namespace Realms.Sync
{
    /// <summary>
    /// A set of extension methods that provide Sync-related functionality on top of Realm classes.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class RealmSyncExtensions
    {
        /// <summary>
        /// Gets the <see cref="Session"/> for the realm file behind this <see cref="Realm"/>.
        /// </summary>
        /// <returns>The <see cref="Session"/> that is responsible for synchronizing with a Realm Object Server instance.</returns>
        /// <param name="realm">An instance of the <see cref="Realm"/> class created with a <see cref="SyncConfiguration"/> object.</param>
        /// <exception cref="ArgumentNullException">Thrown if <c>realm</c> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if the <c>realm</c> was not created with a <see cref="SyncConfiguration"/> object.</exception>
        public static Session GetSession(this Realm realm)
        {
            Argument.NotNull(realm, nameof(realm));
            Argument.Ensure(realm.Config is SyncConfiguration, "Cannot get a Session for a Realm without a SyncConfiguration", nameof(realm));

            return new Session(realm.Config.DatabasePath);
        }

        /// <summary>
        /// If the Realm is a partially synchronized Realm, fetch and synchronize the objects
        /// of a given object type that match the given query (in string format).
        /// </summary>
        /// <typeparam name="T">The type of the objects making up the query.</typeparam>
        /// <param name="realm">An instance of the <see cref="Realm"/> class created with a <see cref="SyncConfiguration"/> object.</param>
        /// <param name="query">A string-based query using the NSPredicate syntax to specify which objects should be returned.</param>
        /// <returns>An awaitable task that, upon completion, contains all objects matching the query.</returns>
        /// <remarks>Partial synchronization is in beta. Its APIs are subject to change.</remarks>
        /// <seealso href="https://academy.realm.io/posts/nspredicate-cheatsheet/">NSPredicate Cheatsheet</seealso>
        [Obsolete("Use the IQueryable.SubscribeToObjects extension method")]
        public static async Task<IQueryable<T>> SubscribeToObjectsAsync<T>(this Realm realm, string query)
        {
            Argument.NotNull(realm, nameof(realm));
            Argument.Ensure(realm.Config is SyncConfiguration, "Cannot get a Session for a Realm without a SyncConfiguration", nameof(realm));

            var type = typeof(T);
            if (!realm.Metadata.TryGetValue(type.GetTypeInfo().GetMappedOrOriginalName(), out var metadata) || metadata.Schema.Type.AsType() != type)
            {
                throw new ArgumentException($"The class {type.Name} is not in the limited set of classes for this realm");
            }

            var tcs = new TaskCompletionSource<ResultsHandle>();

            SharedRealmHandleExtensions.SubscribeForObjects(realm.SharedRealmHandle, type, query, tcs);

            var resultsHandle = await tcs.Task;
            return new RealmResults<T>(realm, metadata, resultsHandle);
        }

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
            return realm.SharedRealmHandle.GetPrivileges();
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
            return realm.GetPrivileges(typeof(T).GetTypeInfo().GetMappedOrOriginalName());
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
        /// <param name="className">The name of a <see cref="RealmObject"/> inheritor to get the privileges for.</param>
        /// <param name="realm">The Realm whose privileges are inspected.</param>
        /// <returns>The privileges which the current user has for the given class.</returns>
        public static ClassPrivileges GetPrivileges(this Realm realm, string className)
        {
            return realm.SharedRealmHandle.GetPrivileges(className);
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
        /// <param name="obj">A managed object to get the privileges for.</param>
        /// <param name="realm">The Realm whose privileges are inspected.</param>
        /// <returns>The privileges which the current user has for the given object.</returns>
        public static ObjectPrivileges GetPrivileges(this Realm realm, RealmObject obj)
        {
            return realm.SharedRealmHandle.GetPrivileges(obj.ObjectHandle);
        }
    }
}
