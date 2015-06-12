using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace RealmIO
{
    public class RealmQuery<T> : IQueryable<T>
    {
        public Type ElementType => typeof (T);
        public Expression Expression { get; }
        public IQueryProvider Provider => provider;
        private QueryProvider provider;

        public RealmQuery(QueryProvider queryProvider, Expression expression)
        {
            Expression = expression;
            provider = queryProvider;
        }

        public RealmQuery(ICoreProvider coreProvider) : this(new RealmQueryProvider(coreProvider), null)
        {
            Expression = Expression.Constant(this);
        }

        public IEnumerator<T> GetEnumerator()
        {
            Debug.WriteLine("Queryable.GetEnumerator: " + Expression);
            return (Provider.Execute<IEnumerable<T>>(Expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            Debug.WriteLine("Queryable: " + Expression);
            return (Provider.Execute<System.Collections.IEnumerable>(Expression)).GetEnumerator();
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
            //return this.Execute(expression);
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
            //Debug.WriteLine("Provider: " + expression);
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
            return _coreProvider.ExecuteQuery(_coreQueryHandle, returnType);
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
                //Debug.WriteLine("SELECT * FROM (");
                //this.Visit(m.Arguments[0]);
                //Debug.WriteLine(") AS T WHERE ");
                Debug.WriteLine("M(");
                this.Visit(m.Arguments[0]);
                Debug.WriteLine(")");

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
                    Debug.WriteLine("!");
                    this.Visit(u.Operand);
                    break;
                default:
                    throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
            }
            return u;
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            Debug.WriteLine("(");
            this.Visit(b.Left);
            switch (b.NodeType)
            {
                case ExpressionType.And:
                    Debug.WriteLine(" AND ");
                    break;
                case ExpressionType.Or:
                    Debug.WriteLine(" OR");
                    break;
                case ExpressionType.Equal:
                    Debug.WriteLine(" = ");
                    _coreProvider.QueryEqual(_coreQueryHandle, ((MemberExpression)b.Left).Member.Name, ((ConstantExpression)b.Right).Value);
                    break;
                case ExpressionType.NotEqual:
                    Debug.WriteLine(" <> ");
                    break;
                case ExpressionType.LessThan:
                    Debug.WriteLine(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    Debug.WriteLine(" <= ");
                    break;
                case ExpressionType.GreaterThan:
                    Debug.WriteLine(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    Debug.WriteLine(" >= ");
                    break;
                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));
            }
            this.Visit(b.Right);
            Debug.WriteLine(")");
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
                //Debug.WriteLine("SELECT * FROM ");
                //Debug.WriteLine(q.ElementType.Name);
            }
            else if (c.Value == null)
            {
                Debug.WriteLine("NULL");
            }
            else
            {
                if (c.Value is bool)
                {
                    Debug.WriteLine(((bool) c.Value) ? 1 : 0);
                } 
                else if (c.Value is string)
                {
                    Debug.WriteLine("");
                    Debug.WriteLine(c.Value);
                    Debug.WriteLine("");
                }
                else if (c.Value.GetType() == typeof (object))
                {
                    throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));
                }
                else
                {
                    Debug.WriteLine(c.Value);
                }
            }
            return c;
        }

        protected  Expression VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                Debug.WriteLine(m.Member.Name);
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


    #region From sample..

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

    public static class Evaluator
    {
        /// <summary> 
        /// Performs evaluation & replacement of independent sub-trees 
        /// </summary> 
        /// <param name="expression">The root of the expression tree.</param>
        /// <param name="fnCanBeEvaluated">A function that decides whether a given expression node can be part of the local function.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns> 
        public static Expression PartialEval(Expression expression, Func<Expression, bool> fnCanBeEvaluated)
        {
            return new SubtreeEvaluator(new Nominator(fnCanBeEvaluated).Nominate(expression)).Eval(expression);
        }

        /// <summary> 
        /// Performs evaluation & replacement of independent sub-trees 
        /// </summary> 
        /// <param name="expression">The root of the expression tree.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns> 
        public static Expression PartialEval(Expression expression)
        {
            return PartialEval(expression, Evaluator.CanBeEvaluatedLocally);
        }

        private static bool CanBeEvaluatedLocally(Expression expression)
        {
            return expression.NodeType != ExpressionType.Parameter;
        }

        /// <summary> 
        /// Evaluates & replaces sub-trees when first candidate is reached (top-down) 
        /// </summary> 
        private class SubtreeEvaluator : ExpressionVisitor
        {
            private HashSet<Expression> candidates;

            internal SubtreeEvaluator(HashSet<Expression> candidates)
            {
                this.candidates = candidates;
            }

            internal Expression Eval(Expression exp)
            {
                return this.Visit(exp);
            }

            public override Expression Visit(Expression exp)
            {
                if (exp == null)
                {
                    return null;
                }
                if (this.candidates.Contains(exp))
                {
                    return this.Evaluate(exp);
                }
                return base.Visit(exp);
            }

            private Expression Evaluate(Expression e)
            {
                if (e.NodeType == ExpressionType.Constant)
                {
                    return e;
                }
                LambdaExpression lambda = Expression.Lambda(e);
                Delegate fn = lambda.Compile();
                return Expression.Constant(fn.DynamicInvoke(null), e.Type);
            }
        }

        /// <summary> 
        /// Performs bottom-up analysis to determine which nodes can possibly 
        /// be part of an evaluated sub-tree. 
        /// </summary> 
        private class Nominator : ExpressionVisitor
        {
            private Func<Expression, bool> fnCanBeEvaluated;
            private HashSet<Expression> candidates;
            private bool cannotBeEvaluated;

            internal Nominator(Func<Expression, bool> fnCanBeEvaluated)
            {
                this.fnCanBeEvaluated = fnCanBeEvaluated;
            }

            internal HashSet<Expression> Nominate(Expression expression)
            {
                this.candidates = new HashSet<Expression>();
                this.Visit(expression);
                return this.candidates;
            }

            public override Expression Visit(Expression expression)
            {
                if (expression != null)
                {
                    bool saveCannotBeEvaluated = this.cannotBeEvaluated;
                    this.cannotBeEvaluated = false;
                    base.Visit(expression);
                    if (!this.cannotBeEvaluated)
                    {
                        if (this.fnCanBeEvaluated(expression))
                        {
                            this.candidates.Add(expression);
                        }
                        else
                        {
                            this.cannotBeEvaluated = true;
                        }
                    }
                    this.cannotBeEvaluated |= saveCannotBeEvaluated;
                }
                return expression;
            }
        }
    }

    #endregion
}