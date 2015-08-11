using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RealmNet
{
    public abstract class QueryProvider : IQueryProvider
    {
        public IQueryable<S> CreateQuery<S>(Expression expression)
        {
            return new RealmQuery<S>(this, expression);
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

        public S Execute<S>(Expression expression)
        {
            return (S)this.Execute(expression, typeof(S));
        }

        public object Execute(Expression expression)
        {
            throw new Exception("Non-generic Execute() called...");
        }

        public abstract object Execute(Expression expression, Type returnType);
    }
}