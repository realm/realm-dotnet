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
using System.Linq;
using System.Linq.Expressions;
using Realms.Helpers;

namespace Realms
{
    // Note: we need ICollection<T> implementation to allow Results to be used for RealmDictionary.Keys, RealmDictionary.Values.
    // The regular array implements ICollection<T> so we're not doing anything particularly unusual here.
    internal class RealmResults<T> : RealmCollectionBase<T>, IOrderedQueryable<T>, IQueryableCollection, ICollection<T>
    {
        private readonly ResultsHandle? _handle;

        internal ResultsHandle ResultsHandle => (ResultsHandle)Handle.Value;

        public Type ElementType => typeof(T);

        public Expression Expression { get; }

        public IQueryProvider Provider { get; }

        public override bool IsReadOnly => true;

        internal RealmResults(Realm realm, Metadata? metadata, RealmResultsProvider realmResultsProvider, Expression? expression) : base(realm, metadata)
        {
            Provider = realmResultsProvider;
            Expression = expression ?? Expression.Constant(this);
        }

        internal RealmResults(Realm realm, ResultsHandle handle, Metadata? metadata)
            : this(realm, metadata, new RealmResultsProvider(realm, metadata), expression: null)
        {
            _handle = handle;
        }

        internal RealmResults(Realm realm, Metadata metadata)
            : this(realm, realm.SharedRealmHandle.CreateResults(metadata.TableKey), metadata)
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

        protected override T GetValueAtIndex(int index) => ResultsHandle.GetValueAtIndex(index, Realm).As<T>();

        public override int IndexOf(T? value)
        {
            if (value == null)
            {
                return -1;
            }

            var realmValue = Operator.Convert<T, RealmValue>(value!);

            if (realmValue.Type == RealmValueType.Object && !realmValue.AsIRealmObject().IsManaged)
            {
                return -1;
            }

            return ResultsHandle.Find(realmValue);
        }

        /// <inheritdoc/>
        public override string ToString() => $"RealmResults: {ResultsHandle.Description}";

        void ICollection<T>.Add(T item) => throw new NotSupportedException("Adding elements to the Results collection is not supported.");

        bool ICollection<T>.Remove(T item) => throw new NotSupportedException("Removing elements from the Results collection is not supported.");
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
