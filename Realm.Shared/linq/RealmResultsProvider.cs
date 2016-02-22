/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Realms
{
    internal class RealmResultsProvider : IQueryProvider
    {
        internal Realm _realm;

        internal RealmResultsProvider(Realm realm)
        {
            _realm = realm;
        }

        internal RealmResultsVisitor MakeVisitor(Type retType)
        {
            return new RealmResultsVisitor(_realm, retType);
        }

        public IQueryable<T> CreateQuery<T>(Expression expression)
        {
            return new RealmResults<T>(this, expression);
        }

        public IQueryable CreateQuery(Expression expression)
        {
            Type elementType = TypeSystem.GetElementType(expression.Type);
            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(RealmResults<>).MakeGenericType(elementType), new object[] { this, expression });
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        public T Execute<T>(Expression expression)
        {
            var retType = typeof(T);
            var v = MakeVisitor(retType);
            Expression visitResult = v.Visit(expression);
            var constExp = visitResult as ConstantExpression;
            T ret = (T)constExp?.Value;
            return ret;
        }

        public object Execute(Expression expression)
        {
            throw new Exception("Non-generic Execute() called...");
        }

    }
}