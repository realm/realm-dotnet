/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Realms
{
    internal class RealmQueryProvider : IQueryProvider
    {
        internal Realm _realm;

        internal RealmQueryProvider(Realm realm)
        {
            _realm = realm;
        }

        internal RealmQueryVisitor<T> MakeVisitor<T>()
        {
            return new RealmQueryVisitor<T>(_realm);
        }

        public IQueryable<T> CreateQuery<T>(Expression expression)
        {
            return new RealmQuery<T>(this, expression);
        }

        public IQueryable CreateQuery(Expression expression)
        {
            Type elementType = TypeSystem.GetElementType(expression.Type);
            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(RealmQuery<>).MakeGenericType(elementType), new object[] { this, expression });
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        public TRes Execute<TRes>(Expression expression)
        {
            var v = MakeVisitor<T>();
            Expression visitResult = v.Visit(expression);
            var constExp = visitResult as ConstantExpression;
            TRes ret = (TRes)constExp?.Value;
            return ret;
        }

        public object Execute(Expression expression)
        {
            throw new Exception("Non-generic Execute() called...");
        }

    }
}