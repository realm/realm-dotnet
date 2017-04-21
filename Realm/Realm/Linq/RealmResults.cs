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
using System.Linq;
using System.Linq.Expressions;

namespace Realms
{
    internal interface IQueryableCollection
    {
        QueryHandle CreateQuery();
    }

    internal class RealmResults<T> : RealmCollectionBase<T>, IOrderedQueryable<T>, IQueryableCollection
    {
        private readonly RealmResultsProvider _provider;
        private readonly bool _allRecords;
        private readonly ResultsHandle _handle;

        internal ResultsHandle ResultsHandle => (ResultsHandle)Handle.Value;

        public Type ElementType => typeof(T);

        public Expression Expression { get; } // null if _allRecords

        public IQueryProvider Provider => _provider;

        internal RealmResults(Realm realm, RealmResultsProvider realmResultsProvider, Expression expression, RealmObject.Metadata metadata, bool createdByAll) : base(realm, metadata)
        {
            _provider = realmResultsProvider;
            Expression = expression ?? Expression.Constant(this);
            _allRecords = createdByAll;
        }

        internal RealmResults(Realm realm, RealmObject.Metadata metadata, bool createdByAll)
            : this(realm, new RealmResultsProvider(realm, metadata), null, metadata, createdByAll)
        {
        }

        internal RealmResults(Realm realm, ResultsHandle handle, RealmObject.Metadata metadata)
            : this(realm, new RealmResultsProvider(realm, metadata), null, metadata, false)
        {
            _handle = handle;
        }

        public QueryHandle CreateQuery()
        {
            return ResultsHandle.CreateQuery();
        }

        internal override CollectionHandleBase CreateHandle()
        {
            if (_handle != null)
            {
                return _handle;
            }

            if (_allRecords)
            {
                return Realm.MakeResultsForTable(Metadata);
            }

            // do all the LINQ expression evaluation to build a query
            var qv = _provider.MakeVisitor();
            qv.Visit(Expression);
            var queryHandle = qv.CoreQueryHandle; // grab out the built query definition
            var sortHandle = qv.OptionalSortDescriptorBuilder;
            return Realm.MakeResultsForQuery(queryHandle, sortHandle);
        }
    }
}