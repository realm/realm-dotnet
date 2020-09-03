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
using Realms.Helpers;

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
        /// <param name="query">The <see cref="IQueryable{T}" /> to observe for changes.</param>
        /// <typeparam name="T">Type of the <see cref="RealmObject"/> in the results.</typeparam>
        /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications"/>
        /// <returns>The collection, implementing <see cref="INotifyCollectionChanged"/>.</returns>
        public static IRealmCollection<T> AsRealmCollection<T>(this IQueryable<T> query)
            where T : RealmObject
        {
            Argument.NotNull(query, nameof(query));

            if (query is IRealmCollection<T> collection)
            {
                return collection;
            }

            throw new ArgumentException($"{nameof(query)} must be an instance of IRealmCollection<{typeof(T).Name}>.", nameof(query));
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
        public static IDisposable SubscribeForNotifications<T>(this IQueryable<T> results, NotificationCallbackDelegate<T> callback)
            where T : RealmObject
        {
            return results.AsRealmCollection().SubscribeForNotifications(callback);
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
            Argument.NotNull(list, nameof(list));

            if (list is IRealmCollection<T> collection)
            {
                return collection;
            }

            throw new ArgumentException($"{nameof(list)} must be an instance of IRealmCollection<{typeof(T).Name}>.", nameof(list));
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
        public static IDisposable SubscribeForNotifications<T>(this IList<T> list, NotificationCallbackDelegate<T> callback) => list.AsRealmCollection().SubscribeForNotifications(callback);

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
            Argument.NotNull(list, nameof(list));

            var from = list.IndexOf(item);
            list.Move(from, index);
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
            Argument.NotNull(list, nameof(list));

            if (list is RealmList<T> realmList)
            {
                realmList.Move(from, to);
            }
            else
            {
                var item = list[from];
                list.RemoveAt(from);
                list.Insert(to, item);
            }
        }

        /// <summary>
        /// Apply an NSPredicate-based filter over a collection. It can be used to create
        /// more complex queries, that are currently unsupported by the LINQ provider and
        /// supports SORT and DISTINCT clauses in addition to filtering.
        /// </summary>
        /// <typeparam name="T">The type of the objects that will be filtered.</typeparam>
        /// <param name="query">
        /// A Queryable collection, obtained by calling <see cref="Realm.All{T}"/>.
        /// </param>
        /// <param name="predicate">The predicate that will be applied.</param>
        /// <returns>A queryable observable collection of objects that match the predicate.</returns>
        /// <remarks>
        /// This method can be used in combination with LINQ filtering, but it is strongly recommended
        /// to avoid combining it if a <c>SORT</c> clause appears in the predicate.
        /// <para/>
        /// If you're not going to apply additional filters, it's recommended to use <see cref="AsRealmCollection{T}(IQueryable{T})"/>
        /// after applying the predicate.
        /// </remarks>
        /// <example>
        /// <code>
        /// var results1 = realm.All&lt;Foo&gt;("Bar.IntValue > 0");
        /// var results2 = realm.All&lt;Foo&gt;("Bar.IntValue > 0 SORT(Bar.IntValue ASC Bar.StringValue DESC)");
        /// var results3 = realm.All&lt;Foo&gt;("Bar.IntValue > 0 SORT(Bar.IntValue ASC Bar.StringValue DESC) DISTINCT(Bar.IntValue)");
        /// </code>
        /// </example>
        /// <seealso href="https://github.com/realm/realm-js/blob/master/docs/tutorials/query-language.md">
        /// Examples of the NSPredicate syntax
        /// </seealso>
        /// <seealso href="https://academy.realm.io/posts/nspredicate-cheatsheet/">NSPredicate Cheatsheet</seealso>
        public static IQueryable<T> Filter<T>(this IQueryable<T> query, string predicate)
        {
            var realmResults = Argument.EnsureType<RealmResults<T>>(query, $"{nameof(query)} must be a query obtained by calling Realm.All.", nameof(query));
            return realmResults.GetFilteredResults(predicate);
        }
    }
}
