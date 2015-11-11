/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace RealmNet
{
    public class RealmQuery<T> : IQueryable<T>
    {
        public Type ElementType => typeof (T);
        public Expression Expression { get; }
        public IQueryProvider Provider => _provider;
        private readonly QueryProvider _provider;

        public RealmQuery(QueryProvider queryProvider, Expression expression) 
        {
            this._provider = queryProvider;
            this.Expression = expression;
        }

        public RealmQuery(Realm realm) : this(new RealmQueryProvider(realm), null)
        {
            this.Expression = Expression.Constant(this);
        }

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