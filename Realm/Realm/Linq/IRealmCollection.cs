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
using Realms.Schema;

namespace Realms
{
    /// <summary>
    /// A <see cref="ChangeSet" /> describes the changes inside a <see cref="IRealmCollection{T}" /> since the last time the notification callback was invoked.
    /// </summary>
    public class ChangeSet
    {
        /// <summary>
        /// Gets the indices in the new version of the <see cref="IRealmCollection{T}" /> which were newly inserted.
        /// </summary>
        /// <value>An array, containing the indices of the inserted objects.</value>
        public int[] InsertedIndices { get; }

        /// <summary>
        /// Gets the indices in the new version of the <see cref="IRealmCollection{T}"/> which were modified.
        /// This means that either the property of an object at that index was modified or the property of
        /// of an object it's related to has changed.
        /// </summary>
        /// <value>An array, containing the indices of the modified objects.</value>
        public int[] ModifiedIndices { get; }

        /// <summary>
        /// Gets the indices of objects in the previous version of the <see cref="IRealmCollection{T}"/> which have been removed from this one.
        /// </summary>
        /// <value>An array, containing the indices of the deleted objects.</value>
        public int[] DeletedIndices { get; }

        /// <summary>
        /// Gets the rows in the collection which moved.
        /// </summary>
        /// <remarks>
        /// Every <see cref="Move.From"/> index will be present in <see cref="DeletedIndices"/> and every <see cref="Move.To"/>
        /// index will be present in <see cref="InsertedIndices"/>.
        /// </remarks>
        /// <value>An array of <see cref="Move"/> structs, indicating the source and the destination index of the moved row.</value>
        public Move[] Moves { get; }

        internal ChangeSet(int[] insertedIndices, int[] modifiedIndices, int[] deletedIndices, Move[] moves)
        {
            InsertedIndices = insertedIndices;
            ModifiedIndices = modifiedIndices;
            DeletedIndices = deletedIndices;
            Moves = moves;
        }

        /// <summary>
        /// A <see cref="Move" /> contains information about objects that moved within the same <see cref="IRealmCollection{T}"/>.
        /// </summary>
        public struct Move
        {
            /// <summary>
            /// Gets the index in the old version of the <see cref="IRealmCollection{T}" /> from which the object has moved.
            /// </summary>
            /// <value>The source index of the object.</value>
            public int From { get; }

            /// <summary>
            /// Gets the index in the new version of the <see cref="IRealmCollection{T}" /> to which the object has moved.
            /// </summary>
            /// <value>The destination index of the object.</value>
            public int To { get; }

            internal Move(int from, int to)
            {
                From = from;
                To = to;
            }
        }
    }

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
        /// Gets the <see cref="ObjectSchema"/> of the contained objects.
        /// </summary>
        /// <value>The ObjectSchema of the contained objects.</value>
        /// <seealso cref="ISchemaSource.ObjectSchema"/>
        [Obsolete("Use ISchemaObject.ObjectSchema instead.")]
        ObjectSchema ObjectSchema { get; }

        /// <summary>
        /// Gets a value indicating whether this collection is still valid to use, i.e. the <see cref="Realm"/> instance
        /// hasn't been closed and, if it represents a to-many relationship, it's parent object hasn't been deleted.
        /// </summary>
        /// <value><c>true</c> if the collection is valid to use; <c>false</c> otherwise.</value>
        bool IsValid { get; }

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
        IDisposable SubscribeForNotifications(NotificationCallbackDelegate<T> callback);
    }
}