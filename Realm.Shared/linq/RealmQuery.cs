/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Realms
{
    /// <summary>
    /// Objects resulting from Realm.All or from a LINQ query expression.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RealmQuery<T> : IQueryable<T>
    {
        public Type ElementType => typeof (T);
        public Expression Expression { get; }
        public IQueryProvider Provider => _provider;
        private readonly QueryProvider _provider;

        internal RealmQuery(QueryProvider queryProvider, Expression expression) 
        {
            this._provider = queryProvider;
            this.Expression = expression;
        }

        internal RealmQuery(Realm realm) : this(new RealmQueryProvider(realm), null)
        {
            this.Expression = Expression.Constant(this);
        }

        /// <summary>
        /// Standard method from interface IEnumerable allows the RealmQuery to be used in a <c>foreach</c>
        /// </summary>
        /// <returns>An IEnumerator which will iterate through found Realm persistent objects</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return (Provider.Execute<IEnumerable<T>>(Expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (Provider.Execute<IEnumerable>(Expression)).GetEnumerator();
        }
    }
}