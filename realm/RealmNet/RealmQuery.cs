using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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

        public RealmQuery(ICoreProvider coreProvider) : this(new RealmQueryProvider(coreProvider), null)
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

    public abstract class QueryProvider : IQueryProvider
    {
        IQueryable<S> IQueryProvider.CreateQuery<S>(Expression expression)
        {
            return new RealmQuery<S>(this, expression);
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression)
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

        S IQueryProvider.Execute<S>(Expression expression)
        {
            return (S)this.Execute(expression, typeof(S));
        }

        object IQueryProvider.Execute(Expression expression)
        {
            throw new Exception("Non-generic Execute() called...");
        }

        public abstract object Execute(Expression expression, Type returnType);
    }

    public class RealmQueryProvider : QueryProvider
    {
        private ICoreProvider _coreProvider;

        public RealmQueryProvider(ICoreProvider coreProvider)
        {
            _coreProvider = coreProvider;
        }

        public override object Execute(Expression expression, Type returnType)
        {
            return new RealmQueryVisitor().Process(_coreProvider, expression, returnType);
        }
    }

    public class RealmQueryVisitor : ExpressionVisitor
    {
        private ICoreProvider _coreProvider;
        private ICoreQueryHandle _coreQueryHandle;

        public object Process(ICoreProvider coreProvider, Expression expression, Type returnType)
        {
            _coreProvider = coreProvider;
            Visit(expression);
            return _coreProvider.ExecuteQuery(_coreQueryHandle, returnType.GenericTypeArguments[0]);
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Where")
            {
                this.Visit(m.Arguments[0]);

                LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                this.Visit(lambda.Body);
                return m;
            }
            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    this.Visit(u.Operand);
                    break;
                default:
                    throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
            }
            return u;
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            this.Visit(b.Left);
            switch (b.NodeType)
            {
                case ExpressionType.And:
                    break;
                case ExpressionType.Or:
                    break;
                case ExpressionType.Equal:
                    _coreProvider.QueryEqual(_coreQueryHandle, ((MemberExpression)b.Left).Member.Name, ((ConstantExpression)b.Right).Value);
                    break;
                case ExpressionType.NotEqual:
                    break;
                case ExpressionType.LessThan:
                    break;
                case ExpressionType.LessThanOrEqual:
                    break;
                case ExpressionType.GreaterThan:
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    break;
                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));
            }
            this.Visit(b.Right);
            return b;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            IQueryable q = c.Value as IQueryable;
            if (q != null)
            {
                // assume constant nodes w/ IQueryables are table references
                if (_coreQueryHandle != null)
                    throw new Exception("We already have a table...");

                var tableName = q.ElementType.Name;
                _coreQueryHandle = _coreProvider.CreateQuery(tableName);
            }
            else if (c.Value == null)
            {
            }
            else
            {
                if (c.Value is bool)
                {
                } 
                else if (c.Value is string)
                {
                }
                else if (c.Value.GetType() == typeof (object))
                {
                    throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));
                }
                else
                {
                }
            }
            return c;
        }

        protected  Expression VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                return m;
            }
            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }
    }

    internal static class TypeSystem
    {
        internal static Type GetElementType(Type seqType)
        {
            Type ienum = FindIEnumerable(seqType);
            if (ienum == null) return seqType;
            return ienum.GetTypeInfo().GenericTypeArguments[0];
        }

        private static Type FindIEnumerable(Type seqType)
        {
            if (seqType == null || seqType == typeof(string))
                return null;
            if (seqType.IsArray)
                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
            if (seqType.GetTypeInfo().IsGenericType)
            {
                foreach (Type arg in seqType.GetTypeInfo().GenericTypeArguments)
                {
                    Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (ienum.GetTypeInfo().IsAssignableFrom(seqType.GetTypeInfo()))
                    {
                        return ienum;
                    }
                }
            }
            Type[] ifaces = seqType.GetTypeInfo().ImplementedInterfaces.ToArray();
            if (ifaces != null && ifaces.Length > 0)
            {
                foreach (Type iface in ifaces)
                {
                    Type ienum = FindIEnumerable(iface);
                    if (ienum != null) return ienum;
                }
            }
            if (seqType.GetTypeInfo().BaseType != null && seqType.GetTypeInfo().BaseType != typeof(object))
            {
                return FindIEnumerable(seqType.GetTypeInfo().BaseType);
            }
            return null;
        }
    }
}