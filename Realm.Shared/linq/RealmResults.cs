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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Realms
{
    /// <summary>
    /// Iterable, sortable collection of one kind of RealmObject resulting from <see cref="Realm.All()"/> or from a LINQ query expression.
    /// </summary>
    /// <remarks>Implements <a hlink="https://msdn.microsoft.com/en-us/library/system.linq.iorderedqueryable">IOrderedQueryable</a>.  <br />
    /// You can sort efficiently using the standard LINQ operators <c>OrderBy</c> or <c>OrderByDescending</c> followed by any number of
    /// <c>ThenBy</c> or <c>ThenByDescending</c>.</remarks>
    /// <typeparam name="T">Type of the RealmObject which is being returned.</typeparam>
    public class RealmResults<T> : IOrderedQueryable<T>, RealmResultsNativeHelper.Interface
    {
        public Type ElementType => typeof (T);
        public Expression Expression { get; } = null; // null if _allRecords
        private readonly RealmResultsProvider _provider = null;  // null if _allRecords
        private readonly bool _allRecords = false;
        private readonly Realm _realm;
        private readonly List<NotificationCallback> _callbacks = new List<NotificationCallback>();
        private NotificationTokenHandle _notificationToken;

        /// <summary>
        /// The <see cref="Schema.ObjectSchema"/> that describes the type of item this collection can contain.
        /// </summary>
        public Schema.ObjectSchema ObjectSchema { get; }

        internal ResultsHandle ResultsHandle => _resultsHandle ?? (_resultsHandle = CreateResultsHandle()); 
        private ResultsHandle _resultsHandle = null;

        public IQueryProvider Provider => _provider;

        internal T this[int index]
        {
            get
            {
                var rowPtr = ResultsHandle.GetRow(index);
                var rowHandle = Realm.CreateRowHandle(rowPtr, _realm.SharedRealmHandle);
                return (T)(object)_realm.MakeObjectForRow(ObjectSchema.Name, rowHandle);
            }
        }

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

        internal RealmResults(Realm realm, RealmResultsProvider realmResultsProvider, Expression expression, Schema.ObjectSchema schema, bool createdByAll)
        {
            _realm = realm;
            _provider = realmResultsProvider;
            Expression = expression ?? Expression.Constant(this);
            ObjectSchema = schema;
            _allRecords = createdByAll;

        }

        internal RealmResults(Realm realm, Schema.ObjectSchema schema, bool createdByAll)
            : this(realm, new RealmResultsProvider(realm, schema), null, schema, createdByAll)
        {
        }

        private ResultsHandle CreateResultsHandle()
        {
            if (_allRecords)
            {
                return _realm.MakeResultsForTable(ObjectSchema.Name);
            }
            else
            {
                // do all the LINQ expression evaluation to build a query
                var qv = _provider.MakeVisitor();
                qv.Visit(Expression);
                var queryHandle = qv._coreQueryHandle; // grab out the built query definition
                var sortHandle = qv._optionalSortOrderHandle;
                return _realm.MakeResultsForQuery(ObjectSchema, queryHandle, sortHandle);
            }
        }

        /// <summary>
        /// Standard method from interface IEnumerable allows the RealmResults to be used in a <c>foreach</c> or <c>ToList()</c>.
        /// </summary>
        /// <returns>An IEnumerator which will iterate through found Realm persistent objects.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new RealmResultsEnumerator<T>(_realm, ResultsHandle, ObjectSchema);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();  // using our class generic type, just redirect the legacy get
        }


        /// <summary>
        /// Fast count all objects of a given class.
        /// </summary>
        /// <remarks>
        /// Resolves to this method instead of the LINQ static extension <c>Count&lt;T&gt;(this IEnumerable&lt;T&gt;)</c>, when used directly on Realm.All.
        /// </remarks>
        public int Count()
        {
            if (_allRecords)
            {
                // use the type captured at build based on generic T
                var tableHandle = _realm.Metadata[ObjectSchema.Name].Table;
                return (int)NativeTable.CountAll(tableHandle);
            }

            // normally we would  be in RealmQRealmResultsr.VisitMethodCall, not here
            // however, if someone CASTS a RealmResults<blah> variable from a Where call to 
            // a RealmResults<blah> they change its compile-time type from IQueryable<blah> (which invokes LINQ)
            // to RealmResults<blah> and thus ends up here.
            // as in the unit test CountFoundWithCasting
            //return (int)NativeResults.count(ResultsHandle);
            return (int) ResultsHandle.Count();
        }

        class NotificationToken : IDisposable
        {
            RealmResults<T> _results;
            NotificationCallback _callback;

            internal NotificationToken(RealmResults<T> results, NotificationCallback callback)
            {
                _results = results;
                _callback = callback;
            }

            public void Dispose()
            {
                _results.RemoveCallback(_callback);
                _callback = null;
                _results = null;
            }
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
        /// Currently the only errors that can occur are when opening the <see cref="Realms.Realm" /> on the background worker thread.
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
            if (_callbacks.Count == 0)
            {
                SubscribeForNotifications();
            }

            _callbacks.Add(callback);

            return new NotificationToken(this, callback);
        }

        internal void RemoveCallback(NotificationCallback callback)
        {
            _callbacks.Remove(callback);

            if (_callbacks.Count == 0)
            {
                UnsubscribeFromNotifications();
            }
        }

        private void SubscribeForNotifications()
        {
            Debug.Assert(_notificationToken == null);

            var managedResultsHandle = GCHandle.Alloc(this);
            var token = new NotificationTokenHandle(ResultsHandle);
            var tokenHandle = ResultsHandle.AddNotificationCallback(GCHandle.ToIntPtr(managedResultsHandle), RealmResultsNativeHelper.NotificationCallback);

            RuntimeHelpers.PrepareConstrainedRegions();
            try
            { }
            finally
            {
                token.SetHandle(tokenHandle);
            }

            _notificationToken = token;
        }

        private void UnsubscribeFromNotifications()
        {
            Debug.Assert(_notificationToken != null);

            _notificationToken.Dispose();
            _notificationToken = null;
        }
                    
        void RealmResultsNativeHelper.Interface.NotifyCallbacks(ResultsHandle.CollectionChangeSet? changes, NativeException? exception)
        {
            var managedException = exception?.Convert();
            ChangeSet changeset = null;
            if (changes != null)
            {
                var actualChanges = changes.Value;
                changeset = new ChangeSet(
                    insertedIndices: actualChanges.Insertions.AsEnumerable().Select(i => (int)i).ToArray(),
                    modifiedIndices: actualChanges.Modifications.AsEnumerable().Select(i => (int)i).ToArray(),
                    deletedIndices: actualChanges.Deletions.AsEnumerable().Select(i => (int)i).ToArray()
                );
            }

            foreach (var callback in _callbacks)
            {
                callback(this, changeset, managedException);
            }
        }

    }  // RealmResults

    internal static class RealmResultsNativeHelper
    {
        internal interface Interface
        {
            void NotifyCallbacks(ResultsHandle.CollectionChangeSet? changes, NativeException? exception);
        }

        #if __IOS__
        [ObjCRuntime.MonoPInvokeCallback(typeof(ResultsHandle.NotificationCallback))]
        #endif
        internal static void NotificationCallback(IntPtr managedResultsHandle, IntPtr changes, IntPtr exception)
        {
            var results = (Interface)GCHandle.FromIntPtr(managedResultsHandle).Target;
            results.NotifyCallbacks(new PtrTo<ResultsHandle.CollectionChangeSet>(changes).Value, new PtrTo<NativeException>(exception).Value);
        }
    }
}
