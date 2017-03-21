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
using System.ComponentModel;
using System.Linq;

namespace Realms
{
    /// <summary>
    /// A set of extensions methods exposing notification-related functionality over collections.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class CollectionNotificationsExtensions
    {
        /// <summary>
        /// A convenience method that casts <see cref="IQueryable{T}"/> to <see cref="IRealmCollection{T}"/> which
        /// implements <see cref="INotifyCollectionChanged"/>.
        /// </summary>
        /// <param name="results">The <see cref="IQueryable{T}" /> to observe for changes.</param>
        /// <typeparam name="T">Type of the <see cref="RealmObject"/> in the results.</typeparam>
        /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications"/>
        /// <returns>The collection, implementing <see cref="INotifyCollectionChanged"/>.</returns>
        public static IRealmCollection<T> AsRealmCollection<T>(this IQueryable<T> results) where T : RealmObject
        {
            var collection = results as IRealmCollection<T>;
            if (collection == null)
            {
                throw new ArgumentException($"{nameof(results)} must be an instance of IRealmCollection<{typeof(T).Name}>.", nameof(results));
            }

            return collection;
        }

        /// <summary>
        /// A convenience method that casts <see cref="IQueryable{T}"/> to <see cref="IRealmCollection{T}"/> and subscribes for change notifications.
        /// </summary>
        /// <param name="results">The <see cref="IQueryable{T}" /> to observe for changes.</param>
        /// <typeparam name="T">Type of the <see cref="RealmObject"/> in the results.</typeparam>
        /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications"/>
        /// <param name="callback">The callback to be invoked with the updated <see cref="IRealmCollection{T}" />.</param>
        /// <returns>
        /// A subscription token. It must be kept alive for as long as you want to receive change notifications.
        /// To stop receiving notifications, call <see cref="IDisposable.Dispose" />.
        /// </returns>
        public static IDisposable SubscribeForNotifications<T>(this IQueryable<T> results, NotificationCallbackDelegate<T> callback) where T : RealmObject
        {
            return results.AsRealmCollection().SubscribeForNotifications(callback);
        }

        /// <summary>
        /// A convenience method that casts <see cref="IList{T}"/> to <see cref="IRealmCollection{T}"/> which implements
        /// <see cref="INotifyCollectionChanged"/>.
        /// </summary>
        /// <param name="list">The <see cref="IList{T}" /> to observe for changes.</param>
        /// <typeparam name="T">Type of the <see cref="RealmObject"/> in the list.</typeparam>
        /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications"/>
        /// <returns>The collection, implementing <see cref="INotifyCollectionChanged"/>.</returns>
        public static IRealmCollection<T> AsRealmCollection<T>(this IList<T> list) where T : RealmObject
        {
            var collection = list as IRealmCollection<T>;
            if (collection == null)
            {
                throw new ArgumentException($"{nameof(list)} must be an instance of IRealmCollection<{typeof(T).Name}>.", nameof(list));
            }

            return collection;
        }

        /// <summary>
        /// A convenience method that casts <see cref="IList{T}" /> to <see cref="IRealmCollection{T}"/> and subscribes for change notifications.
        /// </summary>
        /// <param name="results">The <see cref="IList{T}" /> to observe for changes.</param>
        /// <typeparam name="T">Type of the <see cref="RealmObject"/> in the results.</typeparam>
        /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications"/>
        /// <param name="callback">The callback to be invoked with the updated <see cref="IRealmCollection{T}" />.</param>
        /// <returns>
        /// A subscription token. It must be kept alive for as long as you want to receive change notifications.
        /// To stop receiving notifications, call <see cref="IDisposable.Dispose" />.
        /// </returns>
        public static IDisposable SubscribeForNotifications<T>(this IList<T> results, NotificationCallbackDelegate<T> callback) where T : RealmObject
        {
            return results.AsRealmCollection().SubscribeForNotifications(callback);
        }

        /// <summary>
        /// Move the specified item to a new position within the list.
        /// </summary>
        /// <param name="list">The list where the move should occur.</param>
        /// <param name="item">The item that will be moved.</param>
        /// <param name="index">The new position to which the item will be moved.</param>
        /// <typeparam name="T">Type of the <see cref="RealmObject"/> in the list.</typeparam>
        /// <remarks>
        /// This extension method will work for standalone lists as well by calling <see cref="ICollection{T}.Remove"/>
        /// and then <see cref="IList{T}.Insert"/>.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is less than 0 or greater than <see cref="ICollection{T}.Count"/> - 1.</exception>
        public static void Move<T>(this IList<T> list, T item, int index) where T : RealmObject
        {
            var realmList = list as RealmList<T>;

            if (realmList != null)
            {
                realmList.Move(item, index);
            }
            else
            {
                list.Remove(item);
                list.Insert(index, item);
            }
        }

        /// <summary>
        /// <b>Deprecated</b> A convenience method that casts <see cref="IQueryable{T}"/> to <see cref="IRealmCollection{T}"/> which implements INotifyCollectionChanged.
        /// </summary>
        /// <param name="results">The <see cref="IQueryable{T}" /> to observe for changes.</param>
        /// <param name="errorCallback">The parameter is not used.</param>
        /// <typeparam name="T">Type of the <see cref="RealmObject"/> in the results.</typeparam>
        /// <returns>The collection, implementing <see cref="INotifyCollectionChanged"/>.</returns>
        /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications"/>
        [Obsolete("Use .AsRealmCollection to get a collection that implements INotifyCollectionChanged. For error callback, use Realm.Error.")]
        public static INotifyCollectionChanged ToNotifyCollectionChanged<T>(this IOrderedQueryable<T> results, Action<Exception> errorCallback) where T : RealmObject
        {
            return ToNotifyCollectionChanged(results, errorCallback, coalesceMultipleChangesIntoReset: false);
        }

        /// <summary>
        /// <b>Deprecated</b> A convenience method that casts <see cref="IQueryable{T}"/> to <see cref="IRealmCollection{T}"/> which implements INotifyCollectionChanged.
        /// </summary>
        /// <param name="results">The <see cref="IQueryable{T}" /> to observe for changes.</param>
        /// <param name="errorCallback">The parameter is not used.</param>
        /// <param name="coalesceMultipleChangesIntoReset">The parameter is not used.</param>
        /// <typeparam name="T">Type of the <see cref="RealmObject"/> in the results.</typeparam>
        /// <returns>The collection, implementing <see cref="INotifyCollectionChanged"/>.</returns>
        /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications"/>
        [Obsolete("Use .AsRealmCollection to get a collection that implements INotifyCollectionChanged. For error callback, use Realm.Error.")]
        public static INotifyCollectionChanged ToNotifyCollectionChanged<T>(this IOrderedQueryable<T> results, Action<Exception> errorCallback, bool coalesceMultipleChangesIntoReset) where T : RealmObject
        {
            if (errorCallback == null)
            {
                throw new ArgumentNullException(nameof(errorCallback));
            }

            return results.AsRealmCollection();
        }
    }
}
