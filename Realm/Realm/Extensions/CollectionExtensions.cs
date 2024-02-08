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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Realms.Helpers;
using Realms.Sync;

namespace Realms;

/// <summary>
/// A set of extensions methods exposing notification-related functionality over collections.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class CollectionExtensions
{
    /// <summary>
    /// A convenience method that casts <see cref="IQueryable{T}"/> to <see cref="IRealmCollection{T}"/> which
    /// implements <see cref="INotifyCollectionChanged"/>.
    /// </summary>
    /// <param name="query">The <see cref="IQueryable{T}"/> to observe for changes.</param>
    /// <typeparam name="T">Type of the <see cref="RealmObject"/> or <see cref="EmbeddedObject"/> in the results.</typeparam>
    /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications"/>
    /// <returns>The collection, implementing <see cref="INotifyCollectionChanged"/>.</returns>
    public static IRealmCollection<T> AsRealmCollection<T>(this IQueryable<T> query)
        where T : IRealmObjectBase?
    {
        Argument.NotNull(query, nameof(query));

        return Argument.EnsureType<IRealmCollection<T>>(query, $"{nameof(query)} must be an instance of IRealmCollection<{typeof(T).Name}>.", nameof(query));
    }

    /// <summary>
    /// A convenience method that casts <see cref="IQueryable{T}"/> to <see cref="IRealmCollection{T}"/> and subscribes for change notifications.
    /// </summary>
    /// <param name="results">The <see cref="IQueryable{T}"/> to observe for changes.</param>
    /// <typeparam name="T">Type of the <see cref="RealmObject"/> or <see cref="EmbeddedObject"/> in the results.</typeparam>
    /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications"/>
    /// <param name="callback">The callback to be invoked with the updated <see cref="IRealmCollection{T}"/>.</param>
    /// <returns>
    /// A subscription token. It must be kept alive for as long as you want to receive change notifications.
    /// To stop receiving notifications, call <see cref="IDisposable.Dispose"/>.
    /// </returns>
    public static IDisposable SubscribeForNotifications<T>(this IQueryable<T> results, NotificationCallbackDelegate<T> callback)
        where T : IRealmObjectBase?
    {
        return results.AsRealmCollection().SubscribeForNotifications(callback);
    }

    /// <summary>
    /// A convenience method that casts <see cref="ISet{T}"/> to <see cref="IRealmCollection{T}"/> which implements
    /// <see cref="INotifyCollectionChanged"/>.
    /// </summary>
    /// <param name="set">The <see cref="ISet{T}"/> to observe for changes.</param>
    /// <typeparam name="T">Type of the elements in the set.</typeparam>
    /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications"/>
    /// <returns>The collection, implementing <see cref="INotifyCollectionChanged"/>.</returns>
    public static IRealmCollection<T> AsRealmCollection<T>(this ISet<T> set)
    {
        Argument.NotNull(set, nameof(set));

        return Argument.EnsureType<IRealmCollection<T>>(set, $"{nameof(set)} must be an instance of IRealmCollection<{typeof(T).Name}>.", nameof(set));
    }

    /// <summary>
    /// A convenience method that casts <see cref="ISet{T}"/> to <see cref="IRealmCollection{T}"/> and subscribes for change notifications.
    /// </summary>
    /// <param name="set">The <see cref="ISet{T}"/> to observe for changes.</param>
    /// <typeparam name="T">Type of the elements in the set.</typeparam>
    /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications"/>
    /// <param name="callback">The callback to be invoked with the updated <see cref="IRealmCollection{T}"/>.</param>
    /// <returns>
    /// A subscription token. It must be kept alive for as long as you want to receive change notifications.
    /// To stop receiving notifications, call <see cref="IDisposable.Dispose"/>.
    /// </returns>
    public static IDisposable SubscribeForNotifications<T>(this ISet<T> set, NotificationCallbackDelegate<T> callback) => set.AsRealmCollection().SubscribeForNotifications(callback);

    /// <summary>
    /// A convenience method that casts <see cref="IList{T}"/> to <see cref="IRealmCollection{T}"/> which implements
    /// <see cref="INotifyCollectionChanged"/>.
    /// </summary>
    /// <param name="list">The <see cref="IList{T}"/> to observe for changes.</param>
    /// <typeparam name="T">Type of the elements in the list.</typeparam>
    /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications"/>
    /// <returns>The collection, implementing <see cref="INotifyCollectionChanged"/>.</returns>
    public static IRealmCollection<T> AsRealmCollection<T>(this IList<T> list)
    {
        Argument.NotNull(list, nameof(list));

        return Argument.EnsureType<IRealmCollection<T>>(list, $"{nameof(list)} must be an instance of IRealmCollection<{typeof(T).Name}>.", nameof(list));
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
        where T : IRealmObjectBase
    {
        Argument.NotNull(list, nameof(list));

        var realmList = Argument.EnsureType<RealmList<T>>(list, $"{nameof(list)} must be a Realm List property.", nameof(list));
        return realmList.ToResults();
    }

    /// <summary>
    /// Converts a Realm-backed <see cref="ISet{T}"/> to a Realm-backed <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the objects contained in the set.</typeparam>
    /// <param name="set">The set of objects as obtained from a to-many relationship property.</param>
    /// <returns>A queryable collection that represents the objects contained in the set.</returns>
    /// <remarks>
    /// This method works differently from <see cref="Queryable.AsQueryable"/> in that it actually creates
    /// an underlying Realm query to represent the set. This means that all LINQ methods will be executed
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
    public static IQueryable<T> AsRealmQueryable<T>(this ISet<T> set)
        where T : IRealmObjectBase
    {
        Argument.NotNull(set, nameof(set));

        var realmSet = Argument.EnsureType<RealmSet<T>>(set, $"{nameof(set)} must be a Realm Set property.", nameof(set));
        return realmSet.ToResults();
    }

    /// <summary>
    /// A convenience method that casts <see cref="IList{T}"/> to <see cref="IRealmCollection{T}"/> and subscribes for change notifications.
    /// </summary>
    /// <param name="list">The <see cref="IList{T}"/> to observe for changes.</param>
    /// <typeparam name="T">Type of the elements in the list.</typeparam>
    /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications"/>
    /// <param name="callback">The callback to be invoked with the updated <see cref="IRealmCollection{T}"/>.</param>
    /// <returns>
    /// A subscription token. It must be kept alive for as long as you want to receive change notifications.
    /// To stop receiving notifications, call <see cref="IDisposable.Dispose"/>.
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
            if (from < 0 || from >= list.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(from));
            }

            if (to < 0 || to >= list.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(to));
            }

            var item = list[from];
            list.RemoveAt(from);
            list.Insert(to, item);
        }
    }

    /// <summary>
    /// A convenience method that casts <see cref="IDictionary{String, T}"/> to <see cref="IRealmCollection{KeyValuePair}"/> which implements
    /// <see cref="INotifyCollectionChanged"/>.
    /// </summary>
    /// <param name="dictionary">The <see cref="IDictionary{String, T}"/> to observe for changes.</param>
    /// <typeparam name="T">Type of the elements in the dictionary.</typeparam>
    /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications"/>
    /// <returns>The collection, implementing <see cref="INotifyCollectionChanged"/>.</returns>
    public static IRealmCollection<KeyValuePair<string, T>> AsRealmCollection<T>(this IDictionary<string, T> dictionary)
    {
        Argument.NotNull(dictionary, nameof(dictionary));

        var realmDictionary = Argument.EnsureType<RealmDictionary<T>>(dictionary, $"{nameof(dictionary)} must be an instance of IRealmCollection<KeyValuePair<string, {typeof(T).Name}>>.", nameof(dictionary));
        return realmDictionary;
    }

    /// <summary>
    /// Converts a Realm-backed <see cref="IDictionary{String, T}"/> to a Realm-backed <see cref="IQueryable{T}"/> of dictionary's values.
    /// </summary>
    /// <typeparam name="T">The type of the values contained in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary of objects as obtained from a to-many relationship property.</param>
    /// <returns>A queryable collection that represents the values contained in the dictionary.</returns>
    /// <remarks>
    /// This method works differently from <see cref="Queryable.AsQueryable"/> in that it only returns a collection of values,
    /// not a collection of <see cref="KeyValuePair{String, T}"/> and it actually creates an underlying Realm query that represents the dictionary's values.
    /// This means that all LINQ methods will be executed by the database and also that you can subscribe for
    /// notifications even after applying LINQ filters or ordering.
    /// </remarks>
    /// <example>
    /// <code>
    /// var query = owner.DictOfDogs.AsRealmQueryable()
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
    /// <exception cref="ArgumentException">Thrown if the dictionary is not managed by Realm.</exception>
    public static IQueryable<T> AsRealmQueryable<T>(this IDictionary<string, T?> dictionary)
        where T : IRealmObjectBase
    {
        Argument.NotNull(dictionary, nameof(dictionary));

        var realmDictionary = Argument.EnsureType<RealmDictionary<T>>(dictionary, $"{nameof(dictionary)} must be an instance of RealmDictionary<{typeof(T).Name}>.", nameof(dictionary));
        return realmDictionary.ToResults();
    }

    /// <summary>
    /// A convenience method that casts <see cref="IQueryable{T}"/> to <see cref="IRealmCollection{T}"/> and subscribes for change notifications.
    /// </summary>
    /// <param name="dictionary">The <see cref="IDictionary{String, T}"/> to observe for changes.</param>
    /// <typeparam name="T">Type of the elements in the dictionary.</typeparam>
    /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications"/>
    /// <param name="callback">The callback to be invoked with the updated <see cref="IRealmCollection{T}"/>.</param>
    /// <returns>
    /// A subscription token. It must be kept alive for as long as you want to receive change notifications.
    /// To stop receiving notifications, call <see cref="IDisposable.Dispose"/>.
    /// </returns>
    public static IDisposable SubscribeForNotifications<T>(this IDictionary<string, T> dictionary, NotificationCallbackDelegate<KeyValuePair<string, T>> callback)
    {
        return dictionary.AsRealmCollection().SubscribeForNotifications(callback);
    }

    /// <summary>
    /// A convenience method that casts <see cref="IQueryable{T}"/> to <see cref="IRealmCollection{T}"/> and subscribes for change notifications.
    /// </summary>
    /// <param name="dictionary">The <see cref="IDictionary{String, T}"/> to observe for changes.</param>
    /// <typeparam name="T">Type of the elements in the dictionary.</typeparam>
    /// <seealso cref="IRealmCollection{T}.SubscribeForNotifications"/>
    /// <param name="callback">The callback to be invoked with the updated <see cref="IRealmCollection{T}"/>.</param>
    /// <returns>
    /// A subscription token. It must be kept alive for as long as you want to receive change notifications.
    /// To stop receiving notifications, call <see cref="IDisposable.Dispose"/>.
    /// </returns>
    public static IDisposable SubscribeForKeyNotifications<T>(this IDictionary<string, T> dictionary, DictionaryNotificationCallbackDelegate<T> callback)
    {
        Argument.NotNull(dictionary, nameof(dictionary));

        var realmDictionary = Argument.EnsureType<RealmDictionary<T>>(dictionary, $"{nameof(dictionary)} must be an instance of IRealmCollection<KeyValuePair<string, {typeof(T).Name}>>.", nameof(dictionary));
        return realmDictionary.SubscribeForKeyNotifications(callback);
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
    /// <param name="arguments">
    /// Values used for substitution in the predicate.
    /// Note that all primitive types are accepted as they are implicitly converted to RealmValue.
    /// </param>
    /// <returns>A queryable observable collection of objects that match the predicate.</returns>
    /// <remarks>
    /// If you're not going to apply additional filters, it's recommended to use <see cref="AsRealmCollection{T}(IQueryable{T})"/>
    /// after applying the predicate.
    /// </remarks>
    /// <example>
    /// <code>
    /// var results1 = realm.All&lt;Foo&gt;("Bar.IntValue > 0");
    /// var results2 = realm.All&lt;Foo&gt;("Bar.IntValue > 0 SORT(Bar.IntValue ASC Bar.StringValue DESC)");
    /// var results3 = realm.All&lt;Foo&gt;("Bar.IntValue > 0 SORT(Bar.IntValue ASC Bar.StringValue DESC) DISTINCT(Bar.IntValue)");
    /// var results4 = realm.All&lt;Foo&gt;("Bar.IntValue > $0 || (Bar.String == $1 &amp;&amp; Bar.Bool == $2)", 5, "small", true);
    /// </code>
    /// </example>
    /// <seealso href="https://docs.mongodb.com/realm/reference/realm-query-language/">
    /// Examples of the NSPredicate syntax
    /// </seealso>
    /// <seealso href="https://academy.realm.io/posts/nspredicate-cheatsheet/">NSPredicate Cheatsheet</seealso>
    public static IQueryable<T> Filter<T>(this IQueryable<T> query, string predicate, params QueryArgument[] arguments)
    {
        Argument.NotNull(predicate, nameof(predicate));
        Argument.NotNull(arguments, nameof(arguments));

        var realmResults = Argument.EnsureType<RealmResults<T>>(query, $"{nameof(query)} must be a query obtained by calling Realm.All.", nameof(query));
        return realmResults.GetFilteredResults(predicate, arguments);
    }

    /// <summary>
    /// Apply an NSPredicate-based filter over a collection. It can be used to create
    /// more complex queries, that are currently unsupported by the LINQ provider and
    /// supports SORT and DISTINCT clauses in addition to filtering.
    /// </summary>
    /// <typeparam name="T">The type of the objects that will be filtered.</typeparam>
    /// <param name="list">A Realm List.</param>
    /// <param name="predicate">The predicate that will be applied.</param>
    /// <param name="arguments">
    /// Values used for substitution in the predicate.
    /// Note that all primitive types are accepted as they are implicitly converted to RealmValue.
    /// </param>
    /// <returns>A queryable observable collection of objects that match the predicate.</returns>
    /// <remarks>
    /// If you're not going to apply additional filters, it's recommended to use <see cref="AsRealmCollection{T}(IQueryable{T})"/>
    /// after applying the predicate.
    /// </remarks>
    /// <example>
    /// <code>
    /// var joe = realm.All&lt;Person&gt;().Single(p =&gt; p.Name == "Joe");
    /// joe.dogs.Filter("Name BEGINSWITH $0", "R");
    /// </code>
    /// </example>
    /// <seealso href="https://docs.mongodb.com/realm/reference/realm-query-language/"/>
    public static IQueryable<T> Filter<T>(this IList<T> list, string predicate, params QueryArgument[] arguments)
        where T : IRealmObjectBase
    {
        Argument.NotNull(predicate, nameof(predicate));
        Argument.NotNull(arguments, nameof(arguments));

        var realmList = Argument.EnsureType<RealmList<T>>(list, $"{nameof(list)} must be a Realm List property.", nameof(list));
        return realmList.GetFilteredResults(predicate, arguments);
    }

    /// <summary>
    /// Apply an NSPredicate-based filter over a collection. It can be used to create
    /// more complex queries, that are currently unsupported by the LINQ provider and
    /// supports SORT and DISTINCT clauses in addition to filtering.
    /// </summary>
    /// <typeparam name="T">The type of the objects that will be filtered.</typeparam>
    /// <param name="set">A Realm Set.</param>
    /// <param name="predicate">The predicate that will be applied.</param>
    /// <param name="arguments">
    /// Values used for substitution in the predicate.
    /// Note that all primitive types are accepted as they are implicitly converted to RealmValue.
    /// </param>
    /// <returns>A queryable observable collection of objects that match the predicate.</returns>
    /// <remarks>
    /// If you're not going to apply additional filters, it's recommended to use <see cref="AsRealmCollection{T}(IQueryable{T})"/>
    /// after applying the predicate.
    /// </remarks>
    /// <example>
    /// <code>
    /// var joe = realm.All&lt;Person&gt;().Single(p =&gt; p.Name == "Joe");
    /// joe.dogs.Filter("Name BEGINSWITH $0", "R");
    /// </code>
    /// </example>
    /// <seealso href="https://docs.mongodb.com/realm/reference/realm-query-language/">
    /// Examples of the NSPredicate syntax
    /// </seealso>
    /// <seealso href="https://academy.realm.io/posts/nspredicate-cheatsheet/">NSPredicate Cheatsheet</seealso>
    public static IQueryable<T> Filter<T>(this ISet<T> set, string predicate, params QueryArgument[] arguments)
        where T : IRealmObjectBase
    {
        Argument.NotNull(predicate, nameof(predicate));
        Argument.NotNull(arguments, nameof(arguments));

        var realmSet = Argument.EnsureType<RealmSet<T>>(set, $"{nameof(set)} must be a Realm Set property.", nameof(set));
        return realmSet.GetFilteredResults(predicate, arguments);
    }

    /// <summary>
    /// Apply an NSPredicate-based filter over dictionary's values. It can be used to create
    /// more complex queries, that are currently unsupported by the LINQ provider and
    /// supports SORT and DISTINCT clauses in addition to filtering.
    /// </summary>
    /// <typeparam name="T">The type of the dictionary's values that will be filtered.</typeparam>
    /// <param name="dictionary">A Realm Dictionary.</param>
    /// <param name="predicate">The predicate that will be applied.</param>
    /// <param name="arguments">
    /// Values used for substitution in the predicate.
    /// Note that all primitive types are accepted as they are implicitly converted to RealmValue.
    /// </param>
    /// <returns>A queryable observable collection of dictionary values that match the predicate.</returns>
    /// <remarks>
    /// If you're not going to apply additional filters, it's recommended to use <see cref="AsRealmCollection{T}(IQueryable{T})"/>
    /// after applying the predicate.
    /// </remarks>
    /// <example>
    /// <code>
    /// joe.DictOfDogs.Filter("Name BEGINSWITH $0", "R");
    /// </code>
    /// </example>
    /// <seealso href="https://docs.mongodb.com/realm/reference/realm-query-language/">
    /// Examples of the NSPredicate syntax
    /// </seealso>
    /// <seealso href="https://academy.realm.io/posts/nspredicate-cheatsheet/">NSPredicate Cheatsheet</seealso>
    public static IQueryable<T> Filter<T>(this IDictionary<string, T?> dictionary, string predicate, params QueryArgument[] arguments)
        where T : IRealmObjectBase
    {
        Argument.NotNull(predicate, nameof(predicate));
        Argument.NotNull(arguments, nameof(arguments));

        var realmDictionary = Argument.EnsureType<RealmDictionary<T>>(dictionary, $"{nameof(dictionary)} must be an instance of RealmDictionary<{typeof(T).Name}>.", nameof(dictionary));
        return realmDictionary.GetFilteredValueResults(predicate, arguments);
    }

    /// <summary>
    /// Adds a query to the set of active flexible sync subscriptions. The query will be joined via an OR statement
    /// with any existing queries for the same type.
    /// </summary>
    /// <param name="query">The query that will be matched on the server.</param>
    /// <param name="options">
    /// The subscription options controlling the name and/or the type of insert that will be performed.
    /// </param>
    /// <param name="waitForSync">
    /// A parameter controlling when this method should asynchronously wait for the server to send the objects
    /// matching the subscription.
    /// </param>
    /// <param name="cancellationToken">
    /// An optional cancellation token to cancel waiting for synchronization with the server. Note that cancelling the
    /// operation only cancels the wait itself and not the actual subscription, so the subscription will be added even
    /// if the task is cancelled. To remove the subscription, you can use <see cref="SubscriptionSet.Remove{T}"/>.
    /// </param>
    /// <typeparam name="T">The type of objects in the query results.</typeparam>
    /// <remarks>
    /// Adding a query that already exists is a no-op.
    /// <br/>
    /// This method is roughly equivalent to calling <see cref="SubscriptionSet.Add{T}"/> and then
    /// <see cref="SubscriptionSet.WaitForSynchronizationAsync"/>.
    /// </remarks>
    /// <returns>The original query after it has been added to the subscription set.</returns>
    /// <seealso cref="SubscriptionSet"/>
    public static async Task<IQueryable<T>> SubscribeAsync<T>(this IQueryable<T> query,
        SubscriptionOptions? options = null,
        WaitForSyncMode waitForSync = WaitForSyncMode.FirstTime,
        CancellationToken? cancellationToken = null)
        where T : IRealmObject
    {
        Argument.NotNull(query, nameof(query));

        var realmResults = Argument.EnsureType<RealmResults<T>>(query, $"{nameof(query)} must be a query obtained by calling Realm.All.", nameof(query));
        if (realmResults.Realm.Config is not FlexibleSyncConfiguration)
        {
            throw new NotSupportedException(
                "SubscribeAsync can only be called on queries created in a flexible sync Realm (i.e. one open with a FlexibleSyncConfiguration)");
        }

        var subscriptions = realmResults.Realm.Subscriptions;
        var existingSub = options?.Name == null ? subscriptions.Find(query) : subscriptions.Find(options.Name);

        Subscription newSub = null!;

        subscriptions.Update(() =>
        {
            newSub = subscriptions.Add(realmResults, options);
        });

        if (ShouldWaitForSync(waitForSync, existingSub, newSub))
        {
            await subscriptions.WaitForSynchronizationAsync(cancellationToken);
            await realmResults.Realm.SyncSession.WaitForDownloadAsync(cancellationToken);
        }

        return query;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "This is only used by the weaver and should not be exposed to users.")]
    public static void PopulateCollection<T>(ICollection<T> source, ICollection<T> target, bool update, bool skipDefaults)
        => PopulateCollectionCore(source, target, update, skipDefaults, value => value);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "This is only used by the weaver and should not be exposed to users.")]
    public static void PopulateCollection<T>(IDictionary<string, T> source, IDictionary<string, T> target, bool update, bool skipDefaults)
        => PopulateCollectionCore(source, target, update, skipDefaults, kvp => kvp.Value);

    private static bool ShouldWaitForSync(WaitForSyncMode mode, Subscription? oldSub, Subscription newSub)
    {
        switch (mode)
        {
            case WaitForSyncMode.Never:
                return false;
            case WaitForSyncMode.FirstTime:
                // For FirstTimeSync mode we want to wait for sync only if we're adding a brand new sub
                // or if the sub changed object type/query.
                return oldSub == null ||
                       oldSub.ObjectType != newSub.ObjectType ||
                       oldSub.Query != newSub.Query;
            case WaitForSyncMode.Always:
                return true;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    private static void PopulateCollectionCore<T>(ICollection<T>? source, ICollection<T> target, bool update, bool skipDefaults, Func<T, object?> valueGetter)
    {
        Argument.NotNull(target, nameof(target));

        if (!skipDefaults || source != null)
        {
            target.Clear();
        }

        var realm = ((IRealmCollection<T>)target).Realm;

        if (source != null)
        {
            foreach (var item in source)
            {
                var value = valueGetter(item);
                if (value is IRealmObject obj)
                {
                    realm.Add(obj, update);
                }
                else if (value is RealmValue { Type: RealmValueType.Object } val)
                {
                    var wrappedObj = val.AsIRealmObject();
                    if (wrappedObj is IRealmObject robj)
                    {
                        realm.Add(robj, update);
                    }
                }

                target.Add(item);
            }
        }
    }
}
