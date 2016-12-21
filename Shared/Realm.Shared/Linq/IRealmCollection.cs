﻿////////////////////////////////////////////////////////////////////////////
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
    /// A <see cref="ChangeSet" /> describes the changes inside a <see cref="IRealmCollection{T}" /> since the last time the notification callback was invoked.
    /// </summary>
    public class ChangeSet
    {
        /// <summary>
        /// Gets the indices in the new version of the <see cref="IRealmCollection{T}" /> which were newly inserted.
        /// </summary>
        public int[] InsertedIndices { get; }

        /// <summary>
        /// Gets the indices in the new version of the <see cref="IRealmCollection{T}"/> which were modified. This means that the property of an object at that index was modified
        /// or the property of another object it's related to.
        /// </summary>
        public int[] ModifiedIndices { get; }

        /// <summary>
        /// Gets the indices of objects in the previous version of the <see cref="IRealmCollection{T}"/> which have been removed from this one.
        /// </summary>
        public int[] DeletedIndices { get; }

        /// <summary>
        /// Gets the rows in the collection which moved.
        /// </summary>
        /// <remarks>
        /// Every <c>From</c> index will be present in <c>DeletedIndices</c> and every <c>To</c> index will be present in <c>InsertedIndices</c>.
        /// </remarks>
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
            public int From { get; }

            /// <summary>
            /// Gets the index in the new version of the <see cref="IRealmCollection{T}" /> to which the object has moved.
            /// </summary>
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
    /// <param name="changes">The <see cref="ChangeSet"/> describing the changes to a <see cref="IRealmCollection{T}"/>, or <c>null</c> if an error occurred.</param>
    /// <param name="error">An exception that might have occurred while asynchronously monitoring a <see cref="IRealmCollection{T}"/> for changes, or <c>null</c> if no errors occurred.</param>
    /// <typeparam name="T">Type of the RealmObject which is being returned.</typeparam>
    public delegate void NotificationCallbackDelegate<in T>(IRealmCollection<T> sender, ChangeSet changes, Exception error);

    /// <summary>
    /// Iterable, sortable collection of one kind of RealmObject resulting from <see cref="Realm.All"/> or from a LINQ query expression.
    /// </summary>
    /// <typeparam name="T">Type of the RealmObject which is being returned.</typeparam>
    public interface IRealmCollection<out T> : IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the object schema of the contained objects.
        /// </summary>
        Schema.ObjectSchema ObjectSchema { get; }

        /// <summary>
        /// Register a callback to be invoked each time this <see cref="IRealmCollection{T}"/> changes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The callback will be asynchronously invoked with the initial <see cref="IRealmCollection{T}" />, and then called again after each write transaction
        /// which changes either any of the objects in the collection, or which objects are in the collection.
        /// The <c>changes</c> parameter will be <c>null</c> the first time the callback is invoked with the initial results.
        /// For each call after that, it will contain information about which rows in the results were added, removed or modified.
        /// </para>
        /// <para>
        /// If a write transaction did not modify any objects in this <see cref="IRealmCollection{T}" />, the callback is not invoked at all.
        /// If an error occurs the callback will be invoked with <c>null</c> for the <c>sender</c> parameter and a non-<c>null</c> <c>error</c>.
        /// Currently the only errors that can occur are when opening the <see cref="Realms.Realm" /> on the background worker thread.
        /// </para>
        /// <para>
        /// At the time when the block is called, the <see cref="IRealmCollection{T}" /> object will be fully evaluated and up-to-date, and as long as you do not perform a write transaction on the same thread
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