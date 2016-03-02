/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Realms
{
    internal class RealmResultsVisitor : ExpressionVisitor
    {
        private Realm _realm;
        private QueryHandle _coreQueryHandle;  // set when recurse down to VisitConstant
        private Type _retType;


        internal RealmResultsVisitor(Realm realm, Type retType)
        {
            _realm = realm;
            _retType = retType;
        }


        internal RealmObject FindNextObject(ref long nextRowIndex)
        {
            var rowHandle = NativeQuery.find(_coreQueryHandle, (IntPtr)nextRowIndex);
            if (rowHandle.IsInvalid)
                return null;
            nextRowIndex = rowHandle.RowIndex + 1;  // bump caller index
            return MakeObject(rowHandle);
        }

        private RealmObject MakeObject(RowHandle rowHandle)
        {
            var o = Activator.CreateInstance(_retType);
            ((RealmObject)o)._Manage(_realm, rowHandle);
            return (RealmObject)o;
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
            if (m.Method.DeclaringType == typeof(Queryable)) { 
                if (m.Method.Name == "Where")
                {
                    this.Visit(m.Arguments[0]);

                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                    this.Visit(lambda.Body);
                    return m;
                }
                if (m.Method.Name == "Count")
                {
                    this.Visit(m.Arguments[0]);  // typically recurse down to a "Where"
                    int foundCount = (int)NativeQuery.count(_coreQueryHandle);
                    return Expression.Constant(foundCount);
                }
                if (m.Method.Name == "Any")
                {
                    this.Visit(m.Arguments[0]);   // typically recurse down to a "Where"
                    RowHandle firstRow = NativeQuery.find(_coreQueryHandle, IntPtr.Zero);
                    bool foundAny = !firstRow.IsInvalid;
                    return Expression.Constant(foundAny);
                }
                if (m.Method.Name == "First")
                {
                    // unlike Any, has embedded lambda
                    this.Visit(m.Arguments[0]);  // creates the query
                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                    this.Visit(lambda.Body);
                    RowHandle firstRow = NativeQuery.find(_coreQueryHandle, IntPtr.Zero);
                    if (firstRow.IsInvalid)
                        throw new InvalidOperationException("Sequence contains no matching element");
                    return Expression.Constant(MakeObject(firstRow));
                }
                if (m.Method.Name == "Single")
                {
                    // unlike Any, has embedded lambda, so treat it more like a Where
                    // eg: m    {value(Realms.RealmResults`1[IntegrationTests.Person]).Single(p => (p.Latitude > 100))}
                    this.Visit(m.Arguments[0]);  // creates the query
                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                    this.Visit(lambda.Body);
                    RowHandle firstRow = NativeQuery.find(_coreQueryHandle, IntPtr.Zero);
                    if (firstRow.IsInvalid)
                        throw new InvalidOperationException("Sequence contains no matching element");
                    IntPtr nextIndex = (IntPtr)(firstRow.RowIndex+1);
                    RowHandle nextRow = NativeQuery.find(_coreQueryHandle, nextIndex);
                    if (!nextRow.IsInvalid)
                        throw new InvalidOperationException("Sequence contains more than one matching element");
                    return Expression.Constant(MakeObject(firstRow));
                }

            }
            throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");
        }

        internal override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
            case ExpressionType.Not:
                {
                    NativeQuery.not(_coreQueryHandle);                        
                    this.Visit (u.Operand);  // recurse into richer expression, expect to VisitCombination
                }
                    break;
                default:
                    throw new NotSupportedException($"The unary operator '{u.NodeType}' is not supported");
            }
            return u;
        }

        protected void VisitCombination(BinaryExpression b,  Action<QueryHandle> combineWith )
        {
            NativeQuery.group_begin(_coreQueryHandle);
            Visit(b.Left);
            combineWith(_coreQueryHandle);
            Visit(b.Right);
            NativeQuery.group_end(_coreQueryHandle);
        }


        internal override Expression VisitBinary(BinaryExpression b)
        {
            if (b.NodeType == ExpressionType.AndAlso)  // Boolean And with short-circuit
            {
                VisitCombination(b, (qh) => { /* noop -- AND is the default combinator */} );
            }
            else if (b.NodeType == ExpressionType.OrElse)  // Boolean Or with short-circuit
            {
                VisitCombination(b, qh => NativeQuery.or(qh) );
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

#pragma warning disable 0642    // Disable warning about empty statements (See issue #68)

        private void AddQueryEqual(QueryHandle queryHandle, string columnName, object value)
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
                NativeQuery.float_equal((QueryHandle)queryHandle, columnIndex, (float)value);
            else if (valueType == typeof(double))
                NativeQuery.double_equal((QueryHandle)queryHandle, columnIndex, (double)value);
            else
                throw new NotImplementedException();
        }

        private void AddQueryNotEqual(QueryHandle queryHandle, string columnName, object value)
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
                NativeQuery.float_not_equal((QueryHandle)queryHandle, columnIndex, (float)value);
            else if (valueType == typeof(double))
                NativeQuery.double_not_equal((QueryHandle)queryHandle, columnIndex, (double)value);
            else
                throw new NotImplementedException();
        }

        private void AddQueryLessThan(QueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = NativeQuery.get_column_index((QueryHandle)queryHandle, columnName, (IntPtr)columnName.Length);

            var valueType = value.GetType();
            if (valueType == typeof(int))
                NativeQuery.int_less((QueryHandle)queryHandle, columnIndex, (IntPtr)((int)value));
            else if (valueType == typeof(float))
                NativeQuery.float_less((QueryHandle)queryHandle, columnIndex, (float)value);
            else if (valueType == typeof(double))
                NativeQuery.double_less((QueryHandle)queryHandle, columnIndex, (double)value);
            else if (valueType == typeof(string) || valueType == typeof(bool))
                throw new Exception("Unsupported type " + valueType.Name);
            else
                throw new NotImplementedException();
        }

        private void AddQueryLessThanOrEqual(QueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = NativeQuery.get_column_index((QueryHandle)queryHandle, columnName, (IntPtr)columnName.Length);

            var valueType = value.GetType();
            if (valueType == typeof(int))
                NativeQuery.int_less_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((int)value));
            else if (valueType == typeof(float))
                NativeQuery.float_less_equal((QueryHandle)queryHandle, columnIndex, (float)value);
            else if (valueType == typeof(double))
                NativeQuery.double_less_equal((QueryHandle)queryHandle, columnIndex, (double)value);
            else if (valueType == typeof(string) || valueType == typeof(bool))
                throw new Exception("Unsupported type " + valueType.Name);
            else
                throw new NotImplementedException();
        }

        private void AddQueryGreaterThan(QueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = NativeQuery.get_column_index((QueryHandle)queryHandle, columnName, (IntPtr)columnName.Length);

            var valueType = value.GetType();
            if (valueType == typeof(int))
                NativeQuery.int_greater((QueryHandle)queryHandle, columnIndex, (IntPtr)((int)value));
            else if (valueType == typeof(float))
                NativeQuery.float_greater((QueryHandle)queryHandle, columnIndex, (float)value);
            else if (valueType == typeof(double))
                NativeQuery.double_greater((QueryHandle)queryHandle, columnIndex, (double)value);
            else if (valueType == typeof(string) || valueType == typeof(bool))
                throw new Exception("Unsupported type " + valueType.Name);
            else
                throw new NotImplementedException();
        }

        private void AddQueryGreaterThanOrEqual(QueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = NativeQuery.get_column_index((QueryHandle)queryHandle, columnName, (IntPtr)columnName.Length);

            var valueType = value.GetType();
            if (valueType == typeof(int))
                NativeQuery.int_greater_equal((QueryHandle)queryHandle, columnIndex, (IntPtr)((int)value));
            else if (valueType == typeof(float))
                NativeQuery.float_greater_equal((QueryHandle)queryHandle, columnIndex, (float)value);
            else if (valueType == typeof(double))
                NativeQuery.double_greater_equal((QueryHandle)queryHandle, columnIndex, (double)value);
            else if (valueType == typeof(string) || valueType == typeof(bool))
                throw new Exception("Unsupported type " + valueType.Name);
            else
                throw new NotImplementedException();
        }

#pragma warning restore 0642

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

        private QueryHandle CreateQuery(Type elementType)
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
                if (m.Type == typeof(Boolean)) {
                    object rhs = true;  // box value
                    var leftName = m.Member.Name;
                    AddQueryEqual(_coreQueryHandle, leftName, rhs);
                }
                return m;
            }
            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }
    }
}