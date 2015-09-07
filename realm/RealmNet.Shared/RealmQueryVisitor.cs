using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using RealmNet.Interop;
using System.Diagnostics;  // for Debug.Writeline - as a PCL don't have Console

namespace RealmNet
{
    public class RealmQueryVisitor : RealmNet.ExpressionVisitor
    {
        private Realm _realm;
        private ICoreProvider _coreProvider;
        private IQueryHandle _coreQueryHandle;

        public IEnumerable Process(Realm realm, ICoreProvider coreProvider, Expression expression, Type returnType)
        {
            _realm = realm;
            _coreProvider = coreProvider;

            Visit(expression);

            var innerType = returnType.GetGenericArguments()[0];
            var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(innerType));
            var add = list.GetType().GetMethod("Add");

            var handles = _coreProvider.ExecuteQuery(_coreQueryHandle, innerType);
            foreach (var rowHandle in handles)
            {
                var o = Activator.CreateInstance(innerType);
                ((RealmObject)o)._Manage(_realm, _coreProvider, rowHandle);
                add.Invoke(list, new[] { o });
            }
            return (IEnumerable)list;
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }

        internal override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Where")
            {
                this.Visit(m.Arguments[0]);

                LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                this.Visit(lambda.Body);
                return m;
            }
            throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");
        }

        internal override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    this.Visit(u.Operand);
                    break;
                default:
                    throw new NotSupportedException($"The unary operator '{u.NodeType}' is not supported");
            }
            return u;
        }

        protected void VisitCombination(BinaryExpression b,  Action<IQueryHandle> combineWith )
        {
            _coreProvider.AddQueryGroupBegin(_coreQueryHandle);
            Visit(b.Left);
            combineWith(_coreQueryHandle);
            Visit(b.Right);
            _coreProvider.AddQueryGroupEnd(_coreQueryHandle);
        }


        internal override Expression VisitBinary(BinaryExpression b)
        {
            if (b.NodeType == ExpressionType.AndAlso)  // Boolean And with short-circuit
            {
                VisitCombination(b, (qh) => _coreProvider.AddQueryAnd(qh) );
            }
            else if (b.NodeType == ExpressionType.OrElse)  // Boolean Or with short-circuit
            {
                VisitCombination(b, qh => _coreProvider.AddQueryOr(qh) );
            }
            else
            {
                var leftMember = b.Left as MemberExpression;
                if (leftMember == null)
                    throw new NotSupportedException(string.Format("The lhs of the binary operator '{0}' should be a member expression", b.NodeType));
                var leftName = leftMember.Member.Name;
                var rightConst = b.Right as ConstantExpression;
                if (rightConst == null)
                    throw new NotSupportedException(string.Format("The rhs of the binary operator '{0}' should be a constant expression", b.NodeType));

                var rightValue = rightConst.Value;
                switch (b.NodeType)
                {
                    case ExpressionType.Equal:
                        _coreProvider.AddQueryEqual(_coreQueryHandle, leftName, rightValue);
                        break;

                    case ExpressionType.NotEqual:
                        _coreProvider.AddQueryNotEqual(_coreQueryHandle, leftName, rightValue);
                        break;

                    case ExpressionType.LessThan:
                        _coreProvider.AddQueryLessThan(_coreQueryHandle, leftName, rightValue);
                        break;

                    case ExpressionType.LessThanOrEqual:
                        _coreProvider.AddQueryLessThanOrEqual(_coreQueryHandle, leftName, rightValue);
                        break;

                    case ExpressionType.GreaterThan:
                        _coreProvider.AddQueryGreaterThan(_coreQueryHandle, leftName, rightValue);
                        break;

                    case ExpressionType.GreaterThanOrEqual:
                        _coreProvider.AddQueryGreaterThanOrEqual(_coreQueryHandle, leftName, rightValue);
                        break;

                    default:
                        throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));
                }
            }
            return b;
        }

        internal override Expression VisitConstant(ConstantExpression c)
        {
            IQueryable q = c.Value as IQueryable;
            if (q != null)
            {
                // assume constant nodes w/ IQueryables are table references
                if (_coreQueryHandle != null)
                    throw new Exception("We already have a table...");

                var tableName = q.ElementType.Name;
                _coreQueryHandle = _coreProvider.CreateQuery(_realm.TransactionGroupHandle, tableName);
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

        internal override Expression VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                return m;
            }
            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }
    }
}