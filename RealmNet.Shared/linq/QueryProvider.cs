/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RealmNet
{
    public abstract class QueryProvider : IQueryProvider
    {
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
            return (T)this.Execute(expression, typeof(T));
        }

        public object Execute(Expression expression)
        {
            throw new Exception("Non-generic Execute() called...");
        }

        public abstract object Execute(Expression expression, Type returnType);
    }
}