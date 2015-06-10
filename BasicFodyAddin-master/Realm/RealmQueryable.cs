using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace RealmIO
{
    public class RealmQueryable<T> : IQueryable<T>, IQueryProvider
    {
        private Realm realm;
        private Expression expression;

        public RealmQueryable(Realm realm)
        {
            this.realm = realm;
        }

        #region IQueryable/UQueryProvider plumbing

        public Type ElementType { get { return typeof(T);  } }
        public IQueryProvider Provider { get { return this; } }
        public Expression Expression { get { return Expression.Constant(this); } }

        public IEnumerator<T> GetEnumerator()
        {
            return (Provider.Execute<IEnumerable<T>>(Expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (Provider.Execute<System.Collections.IEnumerable>(Expression)).GetEnumerator();
        }

        public IQueryable CreateQuery(Expression expression)
        {
            this.expression = expression;
            return (IQueryable)this;
        }

        public IQueryable<S> CreateQuery<S>(Expression expression)
        {
            this.expression = expression;
            return (IQueryable<S>)this;
        }

        #endregion
        
        public object Execute(Expression @expression)
        {
            throw new NotImplementedException();
        }

        public S Execute<S>(Expression @expression)
        {
            Debug.WriteLine("@Expression: " + @expression);
            Debug.WriteLine("Expression: " + expression);

            var whereFinder = new InnermostWhereFinder();
            var whereExpression = whereFinder.GetInnermostWhere(expression);

            return default(S);
        }
    }

    internal class InnermostWhereFinder : ExpressionVisitor
    {
        private MethodCallExpression innermostWhereExpression;

        public MethodCallExpression GetInnermostWhere(Expression expression)
        {
            Visit(expression);
            return innermostWhereExpression;
        }

        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            if (expression.Method.Name == "Where")
                innermostWhereExpression = expression;

            Visit(expression.Arguments[0]);

            return expression;
        }
    }
}