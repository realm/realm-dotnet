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
    public class RealmResults<T> : IQueryable<T>
    {
        public Type ElementType => typeof (T);
        public Expression Expression { get; }
        public IQueryProvider Provider => _provider;
        private readonly RealmResultsProvider _provider;
        private bool _allRecords = false;

        internal RealmResults(RealmResultsProvider queryProvider, Expression expression) 
        {
            this._provider = queryProvider;
            this.Expression = expression;
        }

        internal RealmResults(Realm realm, bool createdByAll=false) : this(new RealmResultsProvider(realm), null)
        {
            _allRecords = createdByAll;
            this.Expression = Expression.Constant(this);
        }

        /// <summary>
        /// Standard method from interface IEnumerable allows the RealmResults to be used in a <c>foreach</c>.
        /// </summary>
        /// <returns>An IEnumerator which will iterate through found Realm persistent objects.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            var retType = typeof(T);
            return new RealmResultsEnumerator<T>(_provider._realm, _provider.MakeVisitor(retType), Expression);
        }

        IEnumerator IEnumerable.GetEnumerator()

        {
            return (IEnumerator)GetEnumerator();  // using our class generic type, just redirect the legacy get
        }


        /// <summary>
        /// Count all objects if created by <see cref="Realm.All"/> of the parameterised type, faster than a search.
        /// </summary>
        /// <remarks>
        /// Resolves to this method instead of the static extension <c>Count&lt;T&gt;(this IEnumerable&lt;T&gt;)</c>.
        /// </remarks>
        public int Count()
        {
            if (_allRecords)
            {
                // use the type captured at build based on generic T
                var tableHandle = _provider._realm._tableHandles[ElementType];
                return (int)NativeTable.count_all(tableHandle);
            }
            // we should be in RealmQRealmResultsr.VisitMethodCall, not here, ever, seriously!
            throw new NotImplementedException("Count should not be invoked directly on a RealmQRealmResultss created by All. LINQ will not invoke this."); 
        }    
    }
}