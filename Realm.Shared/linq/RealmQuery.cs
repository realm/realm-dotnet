/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
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
    public class RealmQuery<T> : IQueryable<T>
    {
        public Type ElementType => typeof (T);
        public Expression Expression { get; }
        public IQueryProvider Provider => _provider;
        private readonly QueryProvider _provider;
        private bool _allRecords = false;

        internal RealmQuery(QueryProvider queryProvider, Expression expression) 
        {
            this._provider = queryProvider;
            this.Expression = expression;
        }

        internal RealmQuery(Realm realm, bool createdByAll=false) : this(new RealmQueryProvider(realm), null)
        {
            _allRecords = createdByAll;
            this.Expression = Expression.Constant(this);
        }

        /// <summary>
        /// Standard method from interface IEnumerable allows the RealmQuery to be used in a <c>foreach</c>.
        /// </summary>
        /// <returns>An IEnumerator which will iterate through found Realm persistent objects.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return (Provider.Execute<IEnumerable<T>>(Expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (Provider.Execute<IEnumerable>(Expression)).GetEnumerator();
        }


        /// <summary>
        /// Count all objects of query or if created by <see cref="Realm.All"/> of the parameterised type, faster than a search.
        /// </summary>
        /// <remarks>
        /// Resolves to this method instead of the static extension <c>Count&lt;T&gt;(this IEnumerable&lt;T&gt;)</c>.
        /// </remarks>
        public int Count()
        {
            if (_allRecords)
            {
                var prov = _provider as RealmQueryProvider;
                Debug.Assert(prov != null, "RealmQuery cannot have _allRecords state and not have  RealmQueryProvider");
                // use the type captured at build based on generic T
                var tableHandle = prov._realm._tableHandles[ElementType];
                return (int)NativeTable.count_all(tableHandle);
            }
            return 0;  // TODO Count for query, result of TableView
        }    
    }
}