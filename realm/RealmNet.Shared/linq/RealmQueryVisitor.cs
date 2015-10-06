using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace RealmNet
{
    public class RealmQueryVisitor : RealmNet.ExpressionVisitor
    {
        private Realm _realm;
        private IQueryHandle _coreQueryHandle;

        public IEnumerable Process(Realm realm, Expression expression, Type returnType)
        {
            _realm = realm;

            Visit(expression);

            var innerType = returnType.GetGenericArguments()[0];
            var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(innerType));
            var add = list.GetType().GetMethod("Add");

            var handles = ExecuteQuery(_coreQueryHandle, innerType);
            foreach (var rowHandle in handles)
            {
                var o = Activator.CreateInstance(innerType);
                ((RealmObject)o)._Manage(_realm, rowHandle);
                add.Invoke(list, new[] { o });
            }
            return (IEnumerable)list;
        }

        private IEnumerable<IRowHandle> ExecuteQuery(IQueryHandle queryHandle, Type objectType)
        {
            long nextRowIndex = 0;
            while (nextRowIndex != -1)
            {
                var rowHandle = NativeQuery.find((QueryHandle)queryHandle, (IntPtr)nextRowIndex);
                if (!rowHandle.IsInvalid)
                {
                    nextRowIndex = rowHandle.RowIndex + 1;
                    yield return rowHandle;
                }
                else
                {
                    yield break;
                }
            }
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
            NativeQuery.group_begin((QueryHandle)_coreQueryHandle);
            Visit(b.Left);
            combineWith(_coreQueryHandle);
            Visit(b.Right);
            NativeQuery.group_end((QueryHandle)_coreQueryHandle);
        }


        internal override Expression VisitBinary(BinaryExpression b)
        {
            if (b.NodeType == ExpressionType.AndAlso)  // Boolean And with short-circuit
            {
                VisitCombination(b, (qh) => { /* noop -- AND is the default combinator */} );
            }
            else if (b.NodeType == ExpressionType.OrElse)  // Boolean Or with short-circuit
            {
                VisitCombination(b, qh => NativeQuery.or((QueryHandle)qh) );
            }
            else
            {
                var leftMember = b.Left as MemberExpression;
                if (leftMember == null)
                    throw new NotSupportedException(
                        $"The lhs of the binary operator '{b.NodeType}' should be a member expression");
                var leftName = leftMember.Member.Name;
                var rightConst = b.Right as ConstantExpression;
                if (rightConst == null)
                    throw new NotSupportedException(
                        $"The rhs of the binary operator '{b.NodeType}' should be a constant expression");

                var rightValue = rightConst.Value;
                switch (b.NodeType)
                {
                    case ExpressionType.Equal:
                        AddQueryEqual(_coreQueryHandle, leftName, rightValue);
                        break;

                    case ExpressionType.NotEqual:
                        AddQueryNotEqual(_coreQueryHandle, leftName, rightValue);
                        break;

                    case ExpressionType.LessThan:
                        AddQueryLessThan(_coreQueryHandle, leftName, rightValue);
                        break;

                    case ExpressionType.LessThanOrEqual:
                        AddQueryLessThanOrEqual(_coreQueryHandle, leftName, rightValue);
                        break;

                    case ExpressionType.GreaterThan:
                        AddQueryGreaterThan(_coreQueryHandle, leftName, rightValue);
                        break;

                    case ExpressionType.GreaterThanOrEqual:
                        AddQueryGreaterThanOrEqual(_coreQueryHandle, leftName, rightValue);
                        break;

                    default:
                        throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));
                }
            }
            return b;
        }

        private void AddQueryEqual(IQueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = NativeQuery.get_column_index((QueryHandle)queryHandle, columnName, (IntPtr)columnName.Length);

            var valueType = value.GetType();
            if (value is string)
            {
                string valueStr = (string)value;
                NativeQuery.string_equal((QueryHandle)queryHandle, columnIndex, valueStr, (IntPtr)valueStr.Length);
            }
            else if (valueType == typeof(bool))
                NativeQuery.bool_equal((QueryHandle)queryHandle, columnIndex, MarshalHelpers.BoolToIntPtr((bool)value));
            else if (valueType == typeof(int))
                NativeQuery.int_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((int)value));
            else if (valueType == typeof(float))
                ;// see issue 68 NativeQuery.float_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((float)value));
            else if (valueType == typeof(double))
                ;// see issue 68 NativeQuery.double_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((double)value));
            else
                throw new NotImplementedException();
        }

        private void AddQueryNotEqual(IQueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = NativeQuery.get_column_index((QueryHandle)queryHandle, columnName, (IntPtr)columnName.Length);

            var valueType = value.GetType();
            if (value.GetType() == typeof(string))
            {
                string valueStr = (string)value;
                NativeQuery.string_not_equal((QueryHandle)queryHandle, columnIndex, valueStr, (IntPtr)valueStr.Length);
            }
            else if (valueType == typeof(bool))
                NativeQuery.bool_not_equal((QueryHandle)queryHandle, columnIndex, MarshalHelpers.BoolToIntPtr((bool)value));
            else if (valueType == typeof(int))
                NativeQuery.int_not_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((int)value));
            else if (valueType == typeof(float))
                ;// see issue 68 NativeQuery.float_not_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((float)value));
            else if (valueType == typeof(double))
                ;// see issue 68 NativeQuery.double_not_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((double)value));
            else
                throw new NotImplementedException();
        }

        private void AddQueryLessThan(IQueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = NativeQuery.get_column_index((QueryHandle)queryHandle, columnName, (IntPtr)columnName.Length);

            var valueType = value.GetType();
            if (valueType == typeof(int))
                NativeQuery.int_less((QueryHandle)queryHandle, columnIndex, (IntPtr)((int)value));
            else if (valueType == typeof(float))
                ;// see issue 68 NativeQuery.float_less((QueryHandle)queryHandle, columnIndex, (IntPtr)((float)value));
            else if (valueType == typeof(double))
                ;// see issue 68 NativeQuery.double_less((QueryHandle)queryHandle, columnIndex, (IntPtr)((double)value));
            else if (valueType == typeof(string) || valueType == typeof(bool))
                throw new Exception("Unsupported type " + valueType.Name);
            else
                throw new NotImplementedException();
        }

        private void AddQueryLessThanOrEqual(IQueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = NativeQuery.get_column_index((QueryHandle)queryHandle, columnName, (IntPtr)columnName.Length);

            var valueType = value.GetType();
            if (valueType == typeof(int))
                NativeQuery.int_less_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((int)value));
            else if (valueType == typeof(float))
                ;// see issue 68 NativeQuery.float_less_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((float)value));
            else if (valueType == typeof(double))
                ;// see issue 68 NativeQuery.double_less_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((double)value));
            else if (valueType == typeof(string) || valueType == typeof(bool))
                throw new Exception("Unsupported type " + valueType.Name);
            else
                throw new NotImplementedException();
        }

        private void AddQueryGreaterThan(IQueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = NativeQuery.get_column_index((QueryHandle)queryHandle, columnName, (IntPtr)columnName.Length);

            var valueType = value.GetType();
            if (valueType == typeof(int))
                NativeQuery.int_greater((QueryHandle)queryHandle, columnIndex, (IntPtr)((int)value));
            else if (valueType == typeof(float))
                ;// see issue 68 NativeQuery.float_greater((QueryHandle)queryHandle, columnIndex, (IntPtr)((float)value));
            else if (valueType == typeof(double))
                ;// see issue 68 NativeQuery.double_greater((QueryHandle)queryHandle, columnIndex, (IntPtr)((double)value));
            else if (valueType == typeof(string) || valueType == typeof(bool))
                throw new Exception("Unsupported type " + valueType.Name);
            else
                throw new NotImplementedException();
        }

        private void AddQueryGreaterThanOrEqual(IQueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = NativeQuery.get_column_index((QueryHandle)queryHandle, columnName, (IntPtr)columnName.Length);

            var valueType = value.GetType();
            if (valueType == typeof(int))
                NativeQuery.int_greater_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((int)value));
            else if (valueType == typeof(float))
                ;// see issue 68 NativeQuery.float_greater_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((float)value));
            else if (valueType == typeof(double))
                ;// see issue 68 NativeQuery.double_greater_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((double)value));
            else if (valueType == typeof(string) || valueType == typeof(bool))
                throw new Exception("Unsupported type " + valueType.Name);
            else
                throw new NotImplementedException();
        }

        internal override Expression VisitConstant(ConstantExpression c)
        {
            IQueryable q = c.Value as IQueryable;
            if (q != null)
            {
                // assume constant nodes w/ IQueryables are table references
                if (_coreQueryHandle != null)
                    throw new Exception("We already have a table...");

                _coreQueryHandle = CreateQuery(q.ElementType);
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

        private IQueryHandle CreateQuery(Type elementType)
        {
            var tableHandle = _realm._tableHandles[elementType];
            var queryHandle = tableHandle.TableWhere();

            //At this point sh is invalid due to its handle being uninitialized, but the root is set correctly
            //a finalize at this point will not leak anything and the handle will not do anything

            //now, set the TableView handle...
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try { }
            finally
            {
                queryHandle.SetHandle(NativeTable.where(tableHandle));
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            return queryHandle;
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