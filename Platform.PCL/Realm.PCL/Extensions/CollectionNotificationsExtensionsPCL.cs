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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Realms
{
    /// <summary>
    /// Realm results collection changed.
    /// </summary>
    public static class CollectionNotificationsExtensions
    {
        /// <summary>
        /// A convenience method that casts <c>IQueryable{T}</c> to <see cref="IRealmCollection{T}"/> which implements INotifyCollectionChanged.
        /// </summary>
        /// <param name="results">The <see cref="IQueryable{T}" /> to observe for changes.</param>
        /// <typeparam name="T">Type of the RealmObject in the results.</typeparam>
        /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications(NotificationCallbackDelegate{T})"/>
        /// <returns>The collection, implementing <see cref="INotifyCollectionChanged"/>.</returns>
        public static IRealmCollection<T> AsRealmCollection<T>(this IQueryable<T> results) where T : RealmObject
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// A convenience method that casts <c>IQueryable{T}</c> to <see cref="IRealmCollection{T}"/> and subscribes for change notifications.
        /// </summary>
        /// <param name="results">The <see cref="IQueryable{T}" /> to observe for changes.</param>
        /// <typeparam name="T">Type of the RealmObject in the results.</typeparam>
        /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications(NotificationCallbackDelegate{T})"/>
        /// <param name="callback">The callback to be invoked with the updated <see cref="IRealmCollection{T}" />.</param>
        /// <returns>
        /// A subscription token. It must be kept alive for as long as you want to receive change notifications.
        /// To stop receiving notifications, call <see cref="IDisposable.Dispose" />.
        /// </returns>
        public static IDisposable SubscribeForNotifications<T>(this IQueryable<T> results, NotificationCallbackDelegate<T> callback) where T : RealmObject
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// A convenience method that casts <c>IList{T}</c> to <see cref="IRealmCollection{T}"/> which implements INotifyCollectionChanged.
        /// </summary>
        /// <param name="list">The <see cref="IQueryable{T}" /> to observe for changes.</param>
        /// <typeparam name="T">Type of the RealmObject in the list.</typeparam>
        /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications(NotificationCallbackDelegate{T})"/>
        /// <returns>The collection, implementing <see cref="INotifyCollectionChanged"/>.</returns>
        public static IRealmCollection<T> AsRealmCollection<T>(this IList<T> list) where T : RealmObject
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// A convenience method that casts <c>IList{T}</c> to <see cref="IRealmCollection{T}"/> and subscribes for change notifications.
        /// </summary>
        /// <param name="results">The <see cref="IList{T}" /> to observe for changes.</param>
        /// <typeparam name="T">Type of the RealmObject in the results.</typeparam>
        /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications(NotificationCallbackDelegate{T})"/>
        /// <param name="callback">The callback to be invoked with the updated <see cref="IRealmCollection{T}" />.</param>
        /// <returns>
        /// A subscription token. It must be kept alive for as long as you want to receive change notifications.
        /// To stop receiving notifications, call <see cref="IDisposable.Dispose" />.
        /// </returns>
        public static IDisposable SubscribeForNotifications<T>(this IList<T> results, NotificationCallbackDelegate<T> callback) where T : RealmObject
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// A convenience method that casts <c>IQueryable{T}</c> to <see cref="IRealmCollection{T}"/> which implements INotifyCollectionChanged.
        /// </summary>
        /// <param name="results">The <see cref="IQueryable{T}" /> to observe for changes.</param>
        /// <param name="errorCallback">The parameter is not used.</param>
        /// <typeparam name="T">Type of the RealmObject in the results.</typeparam>
        /// <returns>The collection, implementing <see cref="INotifyCollectionChanged"/>.</returns>
        /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications(NotificationCallbackDelegate{T})"/>
        [Obsolete("Use .AsRealmCollection to get a collection that implements INotifyCollectionChanged. For error callback, use Realm.Error.")]
        public static INotifyCollectionChanged ToNotifyCollectionChanged<T>(this IOrderedQueryable<T> results, Action<Exception> errorCallback) where T : RealmObject
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// A convenience method that casts <c>IQueryable{T}</c> to <see cref="IRealmCollection{T}"/> which implements INotifyCollectionChanged.
        /// </summary>
        /// <param name="results">The <see cref="IQueryable{T}" /> to observe for changes.</param>
        /// <param name="errorCallback">The parameter is not used.</param>
        /// <param name="coalesceMultipleChangesIntoReset">The parameter is not used.</param>
        /// <typeparam name="T">Type of the RealmObject in the results.</typeparam>
        /// <returns>The collection, implementing <see cref="INotifyCollectionChanged"/>.</returns>
        /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications(NotificationCallbackDelegate{T})"/>
        [Obsolete("Use .AsRealmCollection to get a collection that implements INotifyCollectionChanged. For error callback, use Realm.Error.")]
        public static INotifyCollectionChanged ToNotifyCollectionChanged<T>(this IOrderedQueryable<T> results, Action<Exception> errorCallback, bool coalesceMultipleChangesIntoReset) where T : RealmObject
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }
    }
}
