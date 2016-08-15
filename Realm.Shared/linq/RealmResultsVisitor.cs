////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////
 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

using LazyMethod = System.Lazy<System.Reflection.MethodInfo>;

namespace Realms
{
    internal class RealmResultsVisitor : ExpressionVisitor
    {
        private Realm _realm;
        internal QueryHandle _coreQueryHandle;  // set when recurse down to VisitConstant
        internal SortOrderHandle _optionalSortOrderHandle;  // set only when get OrderBy*
        private readonly RealmObject.Metadata _metadata;

        private static class Methods 
        {
            internal static LazyMethod Capture<T>(Expression<Action<T>> lambda)
            {
                return new LazyMethod(() => {
                    var method = (lambda.Body as MethodCallExpression).Method;
                    if (method.IsGenericMethod)
                    {
                        method = method.GetGenericMethodDefinition();
                    }

                    return method;
                });
            }

            internal static class String
            {
                internal static readonly LazyMethod Contains = Methods.Capture<string>(s => s.Contains(""));

                internal static readonly LazyMethod StartsWith = Methods.Capture<string>(s => s.StartsWith(""));

                internal static readonly LazyMethod EndsWith = Methods.Capture<string>(s => s.EndsWith(""));
            }
        }

        internal RealmResultsVisitor(Realm realm, RealmObject.Metadata metadata)
        {
            _realm = realm;
            _metadata = metadata;
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }


        /**
        Expressions will typically be in a form:
        - with embedded Lambda `Count(p => !p.IsInteresting)`
        - at the end of a Where `Where(p => !p.IsInteresting).Where()`

        The latter form is handled by recursion where evaluation of Visit will 
        take us back into VisitMethodCall to evaluate the Where call.

        */
        private void RecurseToWhereOrRunLambda(MethodCallExpression m)
        {
            this.Visit(m.Arguments[0]);  // creates the query or recurse to "Where"
            if (m.Arguments.Count > 1) {
                LambdaExpression lambda = (LambdaExpression)StripQuotes (m.Arguments[1]);
                this.Visit (lambda.Body);
            }
        }


        private void AddSort(LambdaExpression lambda, bool isStarting, bool ascending)
        {
            var body = lambda.Body as MemberExpression;
            if (body == null)
                throw new NotSupportedException($"The expression {lambda} cannot be used in an Order clause");

            if (isStarting)
            {
                if (_optionalSortOrderHandle == null)
                    _optionalSortOrderHandle = _realm.MakeSortOrderForTable(_metadata);
                else
                {
                    var badCall = ascending ? "By" : "ByDescending";
                    throw new NotSupportedException($"You can only use one OrderBy or OrderByDescending clause, subsequent sort conditions should be Then{badCall}");
                }
            }

            var sortColName = body.Member.Name;
            _optionalSortOrderHandle.AddClause(sortColName, ascending);
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
                if (m.Method.Name == "OrderBy")
                {
                    this.Visit(m.Arguments[0]);
                    AddSort((LambdaExpression)StripQuotes(m.Arguments[1]), true, true);
                    return m;
                }
                if (m.Method.Name == "OrderByDescending")
                {
                    this.Visit(m.Arguments[0]);
                    AddSort((LambdaExpression)StripQuotes(m.Arguments[1]), true, false);
                    return m;
                }
                if (m.Method.Name == "ThenBy")
                {
                    this.Visit(m.Arguments[0]);
                    AddSort((LambdaExpression)StripQuotes(m.Arguments[1]), false, true);
                    return m;
                }
                if (m.Method.Name == "ThenByDescending")
                {
                    this.Visit(m.Arguments[0]);
                    AddSort((LambdaExpression)StripQuotes(m.Arguments[1]), false, false);
                    return m;
                }
                if (m.Method.Name == "Count")
                {
                    RecurseToWhereOrRunLambda(m);
                    var foundCount = _coreQueryHandle.Count();
                    return Expression.Constant(foundCount);
                }
                if (m.Method.Name == "Any")
                {
                    RecurseToWhereOrRunLambda(m);
                    bool foundAny = _coreQueryHandle.FindDirect(IntPtr.Zero) != IntPtr.Zero;
                    return Expression.Constant(foundAny);
                }
                if (m.Method.Name == "First")
                {
                    RecurseToWhereOrRunLambda(m);  
                    IntPtr firstRowPtr = IntPtr.Zero;
                    if (_optionalSortOrderHandle == null)
                    {
                        firstRowPtr = _coreQueryHandle.FindDirect(IntPtr.Zero);
                    }
                    else 
                    {
                        using (ResultsHandle rh = _realm.MakeResultsForQuery(_metadata.Schema, _coreQueryHandle, _optionalSortOrderHandle)) 
                        {
                            firstRowPtr = rh.GetRow(0);
                        }
                    }
                    if (firstRowPtr == IntPtr.Zero)
                        throw new InvalidOperationException("Sequence contains no matching element");
                    return Expression.Constant(_realm.MakeObjectForRow(_metadata, firstRowPtr));
                }
                if (m.Method.Name == "Single")  // same as unsorted First with extra checks
                {
                    RecurseToWhereOrRunLambda(m);  
                    var firstRowPtr = _coreQueryHandle.FindDirect(IntPtr.Zero);
                    if (firstRowPtr == IntPtr.Zero)
                        throw new InvalidOperationException("Sequence contains no matching element");
                    var firstRow = Realm.CreateRowHandle(firstRowPtr, _realm.SharedRealmHandle);
                    IntPtr nextIndex = (IntPtr)(firstRow.RowIndex+1);
                    var nextRowPtr = _coreQueryHandle.FindDirect(nextIndex);
                    if (nextRowPtr != IntPtr.Zero)
                        throw new InvalidOperationException("Sequence contains more than one matching element");
                    return Expression.Constant(_realm.MakeObjectForRow(_metadata, firstRow));
                }

            }

            if (m.Method.DeclaringType == typeof(string))
            {
                QueryHandle.Operation<string> queryMethod = null;

                if (m.Method == Methods.String.Contains.Value)
                {
                    queryMethod = (q, c, v) => q.StringContains(c, v);
                }
                else if (m.Method == Methods.String.StartsWith.Value)
                {
                    queryMethod = (q, c, v) => q.StringStartsWith(c, v);
                }
                else if (m.Method == Methods.String.EndsWith.Value)
                {
                    queryMethod = (q, c, v) => q.StringEndsWith(c, v);
                }

                if (queryMethod != null)
                {
                    var member = m.Object as MemberExpression;
                    if (member == null)
                    {
                        throw new NotSupportedException($"The method '{m.Method}' has to be invoked on a RealmObject member");
                    }
                    var columnIndex = _coreQueryHandle.GetColumnIndex(member.Member.Name);

                    var argument = ExtractConstantValue (m.Arguments.SingleOrDefault());
                    if (argument == null || argument.GetType() != typeof(string))
                    {
                        throw new NotSupportedException($"The method '{m.Method}' has to be invoked with a single string constant argument or closure variable");
                    }
                    queryMethod(_coreQueryHandle, columnIndex, (string)argument);
                    return m;
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
                        _coreQueryHandle.Not();
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
            _coreQueryHandle.GroupBegin();
            Visit(b.Left);
            combineWith(_coreQueryHandle);
            Visit(b.Right);
            _coreQueryHandle.GroupEnd();
        }

        internal static object ExtractConstantValue(Expression expr)
        {
            var constant = expr as ConstantExpression;
            if (constant != null)
            {
                return constant.Value;
            }

            var memberAccess = expr as MemberExpression;
            if (memberAccess != null && memberAccess.Expression is ConstantExpression && memberAccess.Member is System.Reflection.FieldInfo)
            {
                // handle closure variables
                return ((System.Reflection.FieldInfo)memberAccess.Member).GetValue(((ConstantExpression)memberAccess.Expression).Value);
            }
                
            return null;
        }

        internal override Expression VisitBinary(BinaryExpression b)
        {
            if (b.NodeType == ExpressionType.AndAlso)  // Boolean And with short-circuit
            {
                VisitCombination(b, (qh) => { /* noop -- AND is the default combinator */} );
            }
            else if (b.NodeType == ExpressionType.OrElse)  // Boolean Or with short-circuit
            {
                VisitCombination(b, qh => qh.Or());
            }
            else
            {
                var leftMember = b.Left as MemberExpression;
                if (leftMember == null)
                    throw new NotSupportedException(
                        $"The lhs of the binary operator '{b.NodeType}' should be a member expression. \nUnable to process `{b.Left}`");
                var leftName = leftMember.Member.Name;

                var rightValue = ExtractConstantValue(b.Right);
                if (rightValue == null)
                {
                    throw new NotSupportedException($"The rhs of the binary operator '{b.NodeType}' should be a constant or closure variable expression. \nUnable to process `{b.Right}`");
                }

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
                        throw new NotSupportedException($"The binary operator '{b.NodeType}' is not supported");
                }
            }
            return b;
        }

        private static void AddQueryEqual(QueryHandle queryHandle, string columnName, object value)
            {
            var columnIndex = queryHandle.GetColumnIndex(columnName);

            if (value is string)
                queryHandle.StringEqual(columnIndex, (string)value);
            else if (value is bool)
                queryHandle.BoolEqual(columnIndex, (bool)value);
            else if (value is int)
                queryHandle.IntEqual(columnIndex, (int)value);
            else if (value is long)
                queryHandle.LongEqual(columnIndex, (long)value);
            else if (value is float)
                queryHandle.FloatEqual(columnIndex, (float)value);
            else if (value is double)
                queryHandle.DoubleEqual(columnIndex, (double)value);
            else if (value is DateTimeOffset)
                queryHandle.TimestampTicksEqual(columnIndex, (DateTimeOffset)value);
            else if (value.GetType() == typeof(byte[]))
            {
                var buffer = (byte[])value;
                if (buffer.Length == 0)
                {
                    // see RealmObject.SetByteArrayValue
                    queryHandle.BinaryEqual(columnIndex, (IntPtr)0x1, IntPtr.Zero);
                    return;
                }

                unsafe
                {
                    fixed (byte* bufferPtr = (byte[])value)
                    {
                        queryHandle.BinaryEqual(columnIndex, (IntPtr)bufferPtr, (IntPtr)buffer.LongLength);
                    }
                }
            }
            else
                throw new NotImplementedException();
        }

        private static void AddQueryNotEqual(QueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = queryHandle.GetColumnIndex(columnName);

            if (value is string)
                queryHandle.StringNotEqual(columnIndex, (string)value);
            else if (value is bool)
                queryHandle.BoolNotEqual(columnIndex, (bool)value);
            else if (value is int)
                queryHandle.IntNotEqual(columnIndex, (int)value);
            else if (value is long)
                queryHandle.LongNotEqual(columnIndex, (long)value);
            else if (value is float)
                queryHandle.FloatNotEqual(columnIndex, (float)value);
            else if (value is double)
                queryHandle.DoubleNotEqual(columnIndex, (double)value);
            else if (value is DateTimeOffset)
                queryHandle.TimestampTicksNotEqual(columnIndex, (DateTimeOffset)value);
            else if (value.GetType()== typeof(byte[]))
            {
                var buffer = (byte[])value;
                if (buffer.Length == 0)
                {
                    // see RealmObject.SetByteArrayValue
                    queryHandle.BinaryNotEqual(columnIndex, (IntPtr)0x1, IntPtr.Zero);
                    return;
                }

                unsafe
                {
                    fixed (byte* bufferPtr = (byte[])value)
                    {
                        queryHandle.BinaryNotEqual(columnIndex, (IntPtr)bufferPtr, (IntPtr)buffer.LongLength);
                    }
                }
            }
            else
                throw new NotImplementedException();
        }

        private static void AddQueryLessThan(QueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = queryHandle.GetColumnIndex(columnName);

            if (value is int)
                queryHandle.IntLess(columnIndex, (int)value);
            else if (value is long)
                queryHandle.LongLess(columnIndex, (long)value);
            else if (value is float)
                queryHandle.FloatLess(columnIndex, (float)value);
            else if (value is double)
                queryHandle.DoubleLess(columnIndex, (double)value);
            else if (value is DateTimeOffset)
                queryHandle.TimestampTicksLess(columnIndex, (DateTimeOffset)value);
            else if (value is string || value is bool)
                throw new Exception($"Unsupported type {value.GetType().Name}");
            else
                throw new NotImplementedException();
        }

        private static void AddQueryLessThanOrEqual(QueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = queryHandle.GetColumnIndex(columnName);

            if (value is int)
                queryHandle.IntLessEqual(columnIndex, (int)value);
            else if (value is long)
                queryHandle.LongLessEqual(columnIndex, (long)value);
            else if (value is float)
                queryHandle.FloatLessEqual(columnIndex, (float)value);
            else if (value is double)
                queryHandle.DoubleLessEqual(columnIndex, (double)value);
            else if (value is DateTimeOffset)
                queryHandle.TimestampTicksLessEqual(columnIndex, (DateTimeOffset)value);
            else if (value is string || value is bool)
                throw new Exception($"Unsupported type {value.GetType().Name}");
            else
                throw new NotImplementedException();
        }

        private static void AddQueryGreaterThan(QueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = queryHandle.GetColumnIndex(columnName);

            if (value is int)
                queryHandle.IntGreater(columnIndex, (int)value);
            else if (value is long)
                queryHandle.LongGreater(columnIndex, (long)value);
            else if (value is float)
                queryHandle.FloatGreater(columnIndex, (float)value);
            else if (value is double)
                queryHandle.DoubleGreater(columnIndex, (double)value);
            else if (value is DateTimeOffset)
                queryHandle.TimestampTicksGreater(columnIndex, (DateTimeOffset)value);
            else if (value is string || value is bool)
                throw new Exception($"Unsupported type {value.GetType().Name}");
            else
                throw new NotImplementedException();
        }

        private static void AddQueryGreaterThanOrEqual(QueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = queryHandle.GetColumnIndex(columnName);

            if (value is int)
                queryHandle.IntGreaterEqual(columnIndex, (int)value);
            else if (value is long)
                queryHandle.LongGreaterEqual(columnIndex, (long)value);
            else if (value is float)
                queryHandle.FloatGreaterEqual(columnIndex, (float)value);
            else if (value is double)
                queryHandle.DoubleGreaterEqual(columnIndex, (double)value);
            else if (value is DateTimeOffset)
                queryHandle.TimestampTicksGreaterEqual(columnIndex, (DateTimeOffset)value);
            else if (value is string || value is bool)
                throw new Exception($"Unsupported type {value.GetType().Name}");
            else
                throw new NotImplementedException();
        }

        // strange as it may seem, this is also called for the LHS when simply iterating All<T>()
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
                    throw new NotSupportedException($"The constant for '{c.Value}' is not supported");
                }
                else
                {
                }
            }
            return c;
        }

        private QueryHandle CreateQuery(Type elementType)
        {
            var tableHandle = _realm.Metadata[elementType.Name].Table;
            var queryHandle = tableHandle.TableWhere();

            //At this point sh is invalid due to its handle being uninitialized, but the root is set correctly
            //a finalize at this point will not leak anything and the handle will not do anything

            //now, set the TableView handle...
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try { }
            finally
            {
                queryHandle.SetHandle(NativeTable.Where(tableHandle));
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            return queryHandle;
        }

        internal override Expression VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                if (m.Type == typeof(bool)) {
                    object rhs = true;  // box value
                    var leftName = m.Member.Name;
                    AddQueryEqual(_coreQueryHandle, leftName, rhs);
                }
                return m;
            }
            throw new NotSupportedException($"The member '{m.Member.Name}' is not supported");
        }
    }
}