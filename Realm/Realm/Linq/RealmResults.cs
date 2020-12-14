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
using Realms.Helpers;

namespace Realms
{
    internal class RealmResults<T> : RealmCollectionBase<T>, IOrderedQueryable<T>, IQueryableCollection
    {
        private readonly ResultsHandle _handle;

        internal ResultsHandle ResultsHandle => (ResultsHandle)Handle.Value;

        public Type ElementType => typeof(T);

        public Expression Expression { get; }

        public IQueryProvider Provider { get; }

        internal RealmResults(Realm realm, RealmObjectBase.Metadata metadata, RealmResultsProvider realmResultsProvider, Expression expression) : base(realm, metadata)
        {
            Provider = realmResultsProvider;
            Expression = expression ?? Expression.Constant(this);
        }

        internal RealmResults(Realm realm, ResultsHandle handle, RealmObjectBase.Metadata metadata)
            : this(realm, metadata, new RealmResultsProvider(realm, metadata), null)
        {
            _handle = handle;
        }

        internal RealmResults(Realm realm, RealmObjectBase.Metadata metadata)
            : this(realm, metadata.Table.CreateResults(realm.SharedRealmHandle), metadata)
        {
        }

        public QueryHandle GetQuery() => ResultsHandle.GetQuery();

        public SortDescriptorHandle GetSortDescriptor() => ResultsHandle.GetSortDescriptor();

        internal override RealmCollectionBase<T> CreateCollection(Realm realm, CollectionHandleBase handle) => new RealmResults<T>(realm, (ResultsHandle)handle, Metadata);

        internal override CollectionHandleBase GetOrCreateHandle()
        {
            if (_handle != null)
            {
                return _handle;
            }

            // do all the LINQ expression evaluation to build a query
            var qv = ((RealmResultsProvider)Provider).MakeVisitor();
            qv.Visit(Expression);
            return qv.MakeResultsForQuery();
        }

        public override int IndexOf(T value)
        {
            Argument.NotNull(value, nameof(value));

            if (_argumentType != RealmValueType.Object)
            {
                throw new NotSupportedException("IndexOf on non-object results is not supported.");
            }

            var obj = value as RealmObjectBase;
            if (!obj.IsManaged)
            {
                throw new ArgumentException("Value does not belong to a realm", nameof(value));
            }

            return ResultsHandle.Find(obj.ObjectHandle);
        }
    }

    /// <summary>
    /// IQueryableCollection exposes a method to create QueryHandle without forcing the caller to infer the type of the objects contained in the results.
    /// </summary>
    internal interface IQueryableCollection
    {
        /// <summary>
        /// Creates a query handle for the results.
        /// </summary>
        /// <returns>The query handle.</returns>
        QueryHandle GetQuery();

        /// <summary>
        /// Creates a sort descriptor handle for the results.
        /// </summary>
        /// <returns>The sort descriptor handle.</returns>
        SortDescriptorHandle GetSortDescriptor();
    }
}