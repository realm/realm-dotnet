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
using System.Collections.Specialized;
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

        internal ResultsHandle ResultsHandle => _resultsHandle ?? (_resultsHandle = CreateResultsHandle()); 
        private ResultsHandle _resultsHandle = null;

        public IQueryProvider Provider => _provider;

        public class ChangeSet
        {
            public readonly int[] InsertedIndices;
            public readonly int[] ModifiedIndices;
            public readonly int[] DeletedIndices;

            public struct Move
            {
                public int From;
                public int To;
            }
            public readonly Move[] MovedIndices;

            internal ChangeSet(int[] insertedIndices, int[] modifiedIndices, int[] deletedIndices, Move[] movedIndices)
            {
                InsertedIndices = insertedIndices;
                ModifiedIndices = modifiedIndices;
                DeletedIndices = deletedIndices;
                MovedIndices = movedIndices;
            }
        }

        public delegate void NotificationCallback(RealmResults<T> sender, ChangeSet changes, Exception error);

        internal RealmResults(Realm realm, RealmResultsProvider realmResultsProvider, Expression expression,
            bool createdByAll)
        {
            _realm = realm;
            _provider = realmResultsProvider;
            Expression = expression ?? Expression.Constant(this);
            _allRecords = createdByAll;

        }

        internal RealmResults(Realm realm, bool createdByAll)
            : this(realm, new RealmResultsProvider(realm), null, createdByAll)
        {
        }

        private ResultsHandle CreateResultsHandle()
        {
            var retType = typeof (T);
            if (_allRecords)
            {
                return _realm.MakeResultsForTable(retType);
            }
            else
            {
                // do all the LINQ expression evaluation to build a query
                var qv = _provider.MakeVisitor(retType);
                qv.Visit(Expression);
                var queryHandle = qv._coreQueryHandle; // grab out the built query definition
                var sortHandle = qv._optionalSortOrderHandle;
                return _realm.MakeResultsForQuery(retType, queryHandle, sortHandle);
            }
        }

        /// <summary>
        /// Standard method from interface IEnumerable allows the RealmResults to be used in a <c>foreach</c> or <c>ToList()</c>.
        /// </summary>
        /// <returns>An IEnumerator which will iterate through found Realm persistent objects.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new RealmResultsEnumerator<T>(_realm, ResultsHandle);
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
                var tableHandle = _realm.Metadata[ElementType].Table;
                return (int)NativeTable.count_all(tableHandle);
            }

            // normally we would  be in RealmQRealmResultsr.VisitMethodCall, not here
            // however, if someone CASTS a RealmResults<blah> variable from a Where call to 
            // a RealmResults<blah> they change its compile-time type from IQueryable<blah> (which invokes LINQ)
            // to RealmResults<blah> and thus ends up here.
            // as in the unit test CountFoundWithCasting
            return (int)NativeResults.count(ResultsHandle);
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
                _results = null;
            }
        }

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
            var tokenHandle = NativeResults.add_notification_callback(ResultsHandle, GCHandle.ToIntPtr(managedResultsHandle), RealmResultsNativeHelper.NotificationCallback);;

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
                    
        void RealmResultsNativeHelper.Interface.NotifyCallbacks(NativeResults.CollectionChangeSet? changes, NativeException? exception)
        {
            var managedException = exception?.Convert();
            ChangeSet changeset = null;
            if (changes != null)
            {
                NativeResults.CollectionChangeSet actualChanges = changes.Value;
                changeset = new ChangeSet(
                    insertedIndices: actualChanges.Insertions.AsEnumerable().Select(i => (int)i).ToArray(),
                    modifiedIndices: actualChanges.Modifications.AsEnumerable().Select(i => (int)i).ToArray(),
                    deletedIndices: actualChanges.Deletions.AsEnumerable().Select(i => (int)i).ToArray(),
                    movedIndices: actualChanges.Moves.AsEnumerable().Select(m => new ChangeSet.Move { From = (int)m.From, To = (int)m.To }).ToArray()
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
            void NotifyCallbacks(NativeResults.CollectionChangeSet? changes, NativeException? exception);
        }

        #if __IOS__
        [ObjCRuntime.MonoPInvokeCallback(typeof(NativeResults.NotificationCallback))]
        #endif
        internal static void NotificationCallback(IntPtr managedResultsHandle, PtrTo<NativeResults.CollectionChangeSet> changes, PtrTo<NativeException> exception)
        {
            var results = (Interface)GCHandle.FromIntPtr(managedResultsHandle).Target;
            results.NotifyCallbacks(changes.Value, exception.Value);
        }
    }
}