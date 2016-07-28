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
 
/// PROXY VERSION OF CLASS USED IN PCL FOR BAIT AND SWITCH PATTERN 
 
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Realms
{
    /// <summary>
    /// Iterable collection of one kind of RealmObject resulting from Realm.All or from a LINQ query expression.
    /// </summary>
    /// <typeparam name="T">Type of the RealmObject which is being returned.</typeparam>
    public class RealmResults<T> : IOrderedQueryable<T>
    {
        /// <summary>
        /// A <see cref="ChangeSet" /> describes the changes inside a <see cref="RealmResults{T}" /> since the last time the notification callback was invoked.
        /// </summary>
        public class ChangeSet
        {
            /// <summary>
            /// The indices in the new version of the <see cref="RealmResults{T}" /> which were newly inserted.
            /// </summary>
            public readonly int[] InsertedIndices;

            /// <summary>
            /// The indices in the new version of the <see cref="RealmResults{T}"/> which were modified. This means that the property of an object at that index was modified
            /// or the property of another object it's related to.
            /// </summary>
            public readonly int[] ModifiedIndices;

            /// <summary>
            /// The indices of objects in the previous version of the <see cref="RealmResults{T}"/> which have been removed from this one.
            /// </summary>
            public readonly int[] DeletedIndices;

            internal ChangeSet(int[] insertedIndices, int[] modifiedIndices, int[] deletedIndices)
            {
                InsertedIndices = insertedIndices;
                ModifiedIndices = modifiedIndices;
                DeletedIndices = deletedIndices;
            }
        }

        /// <summary>
        /// A callback that will be invoked each time the contents of a <see cref="RealmResults{T}"/> have changed.
        /// </summary>
        /// <param name="sender">The <see cref="RealmResults{T}"/> being monitored for changes.</param>
        /// <param name="changes">The <see cref="ChangeSet"/> describing the changes to a <see cref="RealmResults{T}"/>, or <c>null</c> if an error occured.</param>
        /// <param name="error">An exception that might have occured while asynchronously monitoring a <see cref="RealmResults{T}"/> for changes, or <c>null</c> if no errors occured.</param>
        public delegate void NotificationCallback(RealmResults<T> sender, ChangeSet changes, Exception error);

        public Type ElementType => typeof (T);
        public Expression Expression { get; }
        public IQueryProvider Provider => null;

        /// <summary>
        /// Standard method from interface IEnumerable allows the RealmResults to be used in a <c>foreach</c>.
        /// </summary>
        /// <returns>An IEnumerator which will iterate through found Realm persistent objects.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }


        /// <summary>
        /// Count all objects if created by <see cref="Realm.All"/> of the parameterised type, faster than a search.
        /// </summary>
        /// <remarks>
        /// Resolves to this method instead of the static extension <c>Count&lt;T&gt;(this IEnumerable&lt;T&gt;)</c>.
        /// </remarks>
        public int Count()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return 0;
        }

        /// <summary>
        /// Register a callback to be invoked each time this <see cref="RealmResults{T}"/> changes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The callback will be asynchronously invoked with the initial <see cref="RealmResults{T}" />, and then called again after each write transaction 
        /// which changes either any of the objects in the collection, or which objects are in the collection.
        /// The <c>changes</c> parameter will be <c>null</c> the first time the callback is invoked with the initial results.
        /// For each call after that, it will contain information about which rows in the results were added, removed or modified.
        /// </para>
        /// <para>
        /// If a write transaction did not modify any objects in this <see cref="RealmResults{T}" />, the callback is not invoked at all.
        /// If an error occurs the callback will be invoked with <c>null</c> for the <c>sender</c> parameter and a non-<c>null</c> <c>error</c>.
        /// Currently the only errors that can occur are when opening the <see cref="Realm" /> on the background worker thread.
        /// </para>
        /// <para>
        /// At the time when the block is called, the <see cref="RealmResults{T}" /> object will be fully evaluated and up-to-date, and as long as you do not perform a write transaction on the same thread
        /// or explicitly call <see cref="Realm.Refresh" />, accessing it will never perform blocking work.
        /// </para>
        /// <para>
        /// Notifications are delivered via the standard event loop, and so can't be delivered while the event loop is blocked by other activity.
        /// When notifications can't be delivered instantly, multiple notifications may be coalesced into a single notification.
        /// This can include the notification with the initial collection.
        /// </para>
        /// </remarks>
        /// <param name="callback">The callback to be invoked with the updated <see cref="RealmResults{T}" />.</param>
        /// <returns>
        /// A subscription token. It must be kept alive for as long as you want to receive change notifications.
        /// To stop receiving notifications, call <see cref="IDisposable.Dispose" />.
        /// </returns>
        public IDisposable SubscribeForNotifications(NotificationCallback callback)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }
    }
}