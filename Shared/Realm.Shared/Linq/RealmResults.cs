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
using System.Linq;
using System.Linq.Expressions;

namespace Realms
{
    /// <summary>
    /// Iterable, sortable collection of one kind of RealmObject resulting from <see cref="Realm.()"/> or from a LINQ query expression.
    /// </summary>
    /// <remarks>Implements <a hlink="https://msdn.microsoft.com/en-us/library/system.linq.iorderedqueryable">IOrderedQueryable</a>.  <br />
    /// You can sort efficiently using the standard LINQ operators <c>OrderBy</c> or <c>OrderByDescending</c> followed by any number of
    /// <c>ThenBy</c> or <c>ThenByDescending</c>.</remarks>
    /// <typeparam name="T">Type of the RealmObject which is being returned.</typeparam>
    public class RealmResults<T> : RealmCollectionBase<T>, IOrderedQueryable<T>, IRealmResults
    {
        private readonly RealmResultsProvider _provider;  // null if _allRecords
        private readonly bool _allRecords;
        private readonly RealmObject.Metadata _targetMetadata;

        public Type ElementType => typeof(T);

        public Expression Expression { get; } // null if _allRecords

        /// <summary>
        /// The <see cref="Schema.ObjectSchema"/> that describes the type of item this collection can contain.
        /// </summary>
        public Schema.ObjectSchema ObjectSchema => _targetMetadata.Schema;

        internal ResultsHandle ResultsHandle => (ResultsHandle)Handle;

        public IQueryProvider Provider => _provider;

        internal RealmResults(Realm realm, RealmResultsProvider realmResultsProvider, Expression expression, RealmObject.Metadata metadata, bool createdByAll) : base(realm)
        {
            _provider = realmResultsProvider;
            Expression = expression ?? Expression.Constant(this);
            _targetMetadata = metadata;
            _allRecords = createdByAll;
        }

        protected override T ItemAtIndex(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            var objectPtr = ResultsHandle.GetObject(index);
            return (T)(object)Realm.MakeObject(_targetMetadata, objectPtr);
        }

        internal RealmResults(Realm realm, RealmObject.Metadata metadata, bool createdByAll)
            : this(realm, new RealmResultsProvider(realm, metadata), null, metadata, createdByAll)
        {
        }

        internal override CollectionHandleBase CreateHandle()
        {
            if (_allRecords)
            {
                return Realm.MakeResultsForTable(_targetMetadata);
            }

            // do all the LINQ expression evaluation to build a query
            var qv = _provider.MakeVisitor();
            qv.Visit(Expression);
            var queryHandle = qv.CoreQueryHandle; // grab out the built query definition
            var sortHandle = qv.OptionalSortDescriptorBuilder;
            return Realm.MakeResultsForQuery(queryHandle, sortHandle);
        }

        /// <summary>
        /// Standard method from interface IEnumerable allows the RealmResults to be used in a <c>foreach</c> or <c>ToList()</c>.
        /// </summary>
        /// <returns>An IEnumerator which will iterate through found Realm persistent objects.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new RealmResultsEnumerator<T>(Realm, ResultsHandle, ObjectSchema);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator(); // using our class generic type, just redirect the legacy get

        /// <summary>
        /// Fast count all objects of a given class, or in a RealmResults after casting.
        /// </summary>
        /// <remarks>
        /// Resolves to this method instead of the LINQ static extension <c>Count&lt;T&gt;(this IEnumerable&lt;T&gt;)</c>, when used directly on <c>Realm.All</c>.
        /// <br/>
        /// if someone CASTS a RealmResults&lt;T&gt; variable from a Where call to
        /// a RealmResults&lt;T&gt; they change its compile-time type from IQueryable&lt;T&gt; (which invokes LINQ)
        /// to RealmResults&lt;T&gt; and thus ends up here.
        /// </remarks>
        /// <returns>Count of all objects in a class or in the results of a search, without instantiating them.</returns>
        public int Count()
        {
            if (_allRecords)
            {
                // use the type captured at build based on generic T
                var tableHandle = Realm.Metadata[ObjectSchema.Name].Table;
                return (int)tableHandle.CountAll();
            }

            // normally we would  be in RealmQRealmResultsr.VisitMethodCall, not here
            // however, casting as described in the remarks above can cause this method to be invoked.
            // as in the unit test CountFoundWithCasting
            return (int)ResultsHandle.Count();
        }
    }
}