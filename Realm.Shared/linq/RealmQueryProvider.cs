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

        internal RealmQueryVisitor MakeVisitor()
        {
            return new RealmQueryVisitor(_realm);
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

        public T Execute<T>(Expression expression)
        {
            return default(T);  // TODO get a visitor result
        }

        public object Execute(Expression expression)
        {
            throw new Exception("Non-generic Execute() called...");
        }

    }
}