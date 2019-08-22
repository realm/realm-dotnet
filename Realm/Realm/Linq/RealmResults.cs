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

using Realms.Helpers;
using Realms.Schema;
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
        private readonly ResultsHandle _handle;

        internal ResultsHandle ResultsHandle => (ResultsHandle)Handle.Value;

        public Type ElementType => typeof(T);

        public Expression Expression { get; }

        public IQueryProvider Provider { get; }

        internal RealmResults(Realm realm, RealmObject.Metadata metadata, RealmResultsProvider realmResultsProvider, Expression expression) : base(realm, metadata)
        {
            Provider = realmResultsProvider;
            Expression = expression ?? Expression.Constant(this);
        }

        internal RealmResults(Realm realm, RealmObject.Metadata metadata, ResultsHandle handle = null)
            : this(realm, metadata, new RealmResultsProvider(realm, metadata), null)
        {
            _handle = handle ?? metadata.Table.CreateResults(realm.SharedRealmHandle);
        }

        public QueryHandle CreateQuery() => ResultsHandle.CreateQuery();

        internal override CollectionHandleBase CreateHandle()
        {
            if (_handle != null)
            {
                return _handle;
            }

            // do all the LINQ expression evaluation to build a query
            var qv = ((RealmResultsProvider)Provider).MakeVisitor();
            qv.Visit(Expression);
            var queryHandle = qv.CoreQueryHandle; // grab out the built query definition
            var sortHandle = qv.OptionalSortDescriptorBuilder;
            return Realm.MakeResultsForQuery(queryHandle, sortHandle);
        }

        #region IList members

        public override int IndexOf(T value)
        {
            Argument.NotNull(value, nameof(value));

            if (_argumentType != (PropertyType.Object | PropertyType.Nullable))
            {
                throw new NotSupportedException("IndexOf on non-object results is not supported.");
            }

            var obj = Operator.Convert<T, RealmObject>(value);
            if (!obj.IsManaged || obj.Realm.IsSameInstance(Realm))
            {
                throw new ArgumentException("Value does not belong to a realm", nameof(value));
            }

            return ResultsHandle.Find(obj.ObjectHandle);
        }

        #endregion
    }
}