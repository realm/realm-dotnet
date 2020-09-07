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

namespace Realms
{
    /// <summary>
    /// A callback that will be invoked each time the contents of a <see cref="IRealmCollection{T}"/> have changed.
    /// </summary>
    /// <param name="sender">The <see cref="IRealmCollection{T}"/> being monitored for changes.</param>
    /// <param name="changes">The <see cref="ChangeSet"/> describing the changes to a <see cref="IRealmCollection{T}"/>,
    /// or <c>null</c> if an has error occurred.</param>
    /// <param name="error">An exception that might have occurred while asynchronously monitoring a
    /// <see cref="IRealmCollection{T}"/> for changes, or <c>null</c> if no errors have occurred.</param>
    /// <typeparam name="T">Type of the <see cref="RealmObject"/> which is being returned.</typeparam>
    public delegate void NotificationCallbackDelegate<in T>(IRealmCollection<T> sender, ChangeSet changes, Exception error);

    /// <summary>
    /// Iterable, sortable collection of one kind of RealmObject resulting from <see cref="Realm.All{T}"/> or from a LINQ query expression.
    /// </summary>
    /// <typeparam name="T">Type of the <see cref="RealmObject"/> which is being returned.</typeparam>
    public interface IRealmCollection<out T> : IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first
        /// occurrence within the entire <see cref="IRealmCollection{T}"/>.
        /// </summary>
        /// <param name="item">
        /// The object to locate in the <see cref="IRealmCollection{T}"/>.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of item within the entire <see cref="IRealmCollection{T}"/>,
        /// if found; otherwise, –1.
        /// </returns>
        int IndexOf(object item);

        /// <summary>
        /// Determines whether an element is in the <see cref="IRealmCollection{T}"/>.
        /// </summary>
        /// <param name="item">
        /// The object to locate in the <see cref="IRealmCollection{T}"/>.
        /// </param>
        /// <returns>true if item is found in the <see cref="IRealmCollection{T}"/>; otherwise, false.</returns>
        bool Contains(object item);

        /// <summary>
        /// Gets a value indicating whether this collection is still valid to use, i.e. the <see cref="Realm"/> instance
        /// hasn't been closed and, if it represents a to-many relationship, it's parent object hasn't been deleted.
        /// </summary>
        /// <value><c>true</c> if the collection is valid to use; <c>false</c> otherwise.</value>
        bool IsValid { get; }

        /// <summary>
        /// Gets the <see cref="Realm"/> instance this collection belongs to.
        /// </summary>
        /// <value>The <see cref="Realm"/> instance this collection belongs to.</value>
        Realm Realm { get; }

        /// <summary>
        /// Gets a value indicating whether this collection is frozen. Frozen collections are immutable and can be accessed
        /// from any thread. The objects read from a frozen collection will also be frozen.
        /// </summary>
        bool IsFrozen { get; }

        /// <summary>
        /// Creates a frozen snapshot of this collection. The frozen copy can be read and queried from any thread.
        /// <para/>
        /// Freezing a collection also creates a frozen Realm which has its own lifecycle, but if the live Realm that spawned the
        /// original collection is fully closed (i.e. all instances across all threads are closed), the frozen Realm and
        /// collection will be closed as well.
        /// <para/>
        /// Frozen collections can be queried as normal, but trying to mutate it in any way or attempting to register a listener will
        /// throw a <see cref="Exceptions.RealmFrozenException"/>.
        /// <para/>
        /// Note: Keeping a large number of frozen objects with different versions alive can have a negative impact on the filesize
        /// of the Realm. In order to avoid such a situation it is possible to set <see cref="RealmConfigurationBase.MaxNumberOfActiveVersions"/>.
        /// </summary>
        /// <returns>A frozen copy of this collection.</returns>
        /// <seealso cref="FrozenObjectsExtensions.Freeze{T}(IList{T})"/>
        /// <seealso cref="FrozenObjectsExtensions.Freeze{T}(System.Linq.IQueryable{T})"/>
        IRealmCollection<T> Freeze();

        /// <summary>
        /// Register a callback to be invoked each time this <see cref="IRealmCollection{T}"/> changes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The callback will be asynchronously invoked with the initial <see cref="IRealmCollection{T}" />, and then
        /// called again after each write transaction which changes either any of the objects in the collection, or
        /// which objects are in the collection. The <c>changes</c> parameter will
        /// be <c>null</c> the first time the callback is invoked with the initial results. For each call after that,
        /// it will contain information about which rows in the results were added, removed or modified.
        /// </para>
        /// <para>
        /// If a write transaction did not modify any objects in this <see cref="IRealmCollection{T}" />, the callback is not invoked at all.
        /// If an error occurs the callback will be invoked with <c>null</c> for the <c>sender</c> parameter and a non-<c>null</c> <c>error</c>.
        /// Currently the only errors that can occur are when opening the <see cref="Realms.Realm" /> on the background worker thread.
        /// </para>
        /// <para>
        /// At the time when the block is called, the <see cref="IRealmCollection{T}" /> object will be fully evaluated
        /// and up-to-date, and as long as you do not perform a write transaction on the same thread
        /// or explicitly call <see cref="Realm.Refresh" />, accessing it will never perform blocking work.
        /// </para>
        /// <para>
        /// Notifications are delivered via the standard event loop, and so can't be delivered while the event loop is blocked by other activity.
        /// When notifications can't be delivered instantly, multiple notifications may be coalesced into a single notification.
        /// This can include the notification with the initial collection.
        /// </para>
        /// </remarks>
        /// <param name="callback">The callback to be invoked with the updated <see cref="IRealmCollection{T}" />.</param>
        /// <returns>
        /// A subscription token. It must be kept alive for as long as you want to receive change notifications.
        /// To stop receiving notifications, call <see cref="IDisposable.Dispose" />.
        /// </returns>
        /// <seealso cref="CollectionNotificationsExtensions.SubscribeForNotifications{T}(IList{T}, NotificationCallbackDelegate{T})"/>
        /// <seealso cref="CollectionNotificationsExtensions.SubscribeForNotifications{T}(System.Linq.IQueryable{T}, NotificationCallbackDelegate{T})"/>
        IDisposable SubscribeForNotifications(NotificationCallbackDelegate<T> callback);
    }
}