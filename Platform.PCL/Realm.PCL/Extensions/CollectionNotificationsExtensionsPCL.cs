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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
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
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// A convenience method that casts <see cref="IList{T}"/> to <see cref="IRealmCollection{T}"/> which implements
        /// <see cref="INotifyCollectionChanged"/>.
        /// </summary>
        /// <param name="list">The <see cref="IList{T}" /> to observe for changes.</param>
        /// <typeparam name="T">Type of the objects in the list.</typeparam>
        /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications"/>
        /// <returns>The collection, implementing <see cref="INotifyCollectionChanged"/>.</returns>
        public static IRealmCollection<T> AsRealmCollection<T>(this IList<T> list)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// Converts a Realm-backed <see cref="IList{T}"/> to a Realm-backed <see cref="IQueryable{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the objects contained in the list.</typeparam>
        /// <param name="list">The list of objects as obtained from a to-many relationship property.</param>
        /// <returns>A queryable collection that represents the objects contained in the list.</returns>
        /// <remarks>
        /// This method works differently from <see cref="Queryable.AsQueryable"/> in that it actually creates
        /// an underlying Realm query to represent the list. This means that all LINQ methods will be executed
        /// by the database and also that you can subscribe for notifications even after applying LINQ filters
        /// or ordering.
        /// </remarks>
        /// <example>
        /// <code>
        /// var dogs = owner.Dogs;
        /// var query = dogs.AsRealmQueryable()
        ///                 .Where(d => d.Age > 3)
        ///                 .OrderBy(d => d.Name);
        ///
        /// var token = query.SubscribeForNotifications((sender, changes, error) =>
        /// {
        ///     // You'll be notified only when dogs older than 3 have been added/removed/updated
        ///     // and the sender collection will be ordered by Name
        /// });
        /// </code>
        /// </example>
        /// <exception cref="ArgumentException">Thrown if the list is not managed by Realm.</exception>
        public static IQueryable<T> AsRealmQueryable<T>(this IList<T> list)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// A convenience method that casts <see cref="IList{T}" /> to <see cref="IRealmCollection{T}"/> and subscribes for change notifications.
        /// </summary>
        /// <param name="list">The <see cref="IList{T}" /> to observe for changes.</param>
        /// <typeparam name="T">Type of the objects in the list.</typeparam>
        /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications"/>
        /// <param name="callback">The callback to be invoked with the updated <see cref="IRealmCollection{T}" />.</param>
        /// <returns>
        /// A subscription token. It must be kept alive for as long as you want to receive change notifications.
        /// To stop receiving notifications, call <see cref="IDisposable.Dispose" />.
        /// </returns>
        public static IDisposable SubscribeForNotifications<T>(this IList<T> list, NotificationCallbackDelegate<T> callback)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// Move the specified item to a new position within the list.
        /// </summary>
        /// <param name="list">The list where the move should occur.</param>
        /// <param name="item">The item that will be moved.</param>
        /// <param name="index">The new position to which the item will be moved.</param>
        /// <typeparam name="T">Type of the objects in the list.</typeparam>
        /// <remarks>
        /// This extension method will work for standalone lists as well by calling <see cref="ICollection{T}.Remove"/>
        /// and then <see cref="IList{T}.Insert"/>.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is less than 0 or greater than <see cref="ICollection{T}.Count"/> - 1.</exception>
        public static void Move<T>(this IList<T> list, T item, int index)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        /// <summary>
        /// Move the specified item to a new position within the list.
        /// </summary>
        /// <param name="list">The list where the move should occur.</param>
        /// <param name="from">The index of the item that will be moved.</param>
        /// <param name="to">The new position to which the item will be moved.</param>
        /// <typeparam name="T">Type of the objects  in the list.</typeparam>
        /// <remarks>
        /// This extension method will work for standalone lists as well by calling <see cref="IList{T}.RemoveAt"/>
        /// and then <see cref="IList{T}.Insert"/>.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is less than 0 or greater than <see cref="ICollection{T}.Count"/> - 1.</exception>
        public static void Move<T>(this IList<T> list, int from, int to)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }
    }
}
