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
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Realms.Native;
using LazyMethod = System.Lazy<System.Reflection.MethodInfo>;

namespace Realms
{
    internal class RealmResultsVisitor : ExpressionVisitor
    {
        private Realm _realm;
        private readonly RealmObject.Metadata _metadata;

        internal QueryHandle CoreQueryHandle;  // set when recurse down to VisitConstant
        internal SortDescriptorBuilder OptionalSortDescriptorBuilder;  // set only when get OrderBy*

        private static class Methods
        {
            internal static LazyMethod Capture<T>(Expression<Action<T>> lambda)
            {
                return new LazyMethod(() =>
                {
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
                internal static readonly LazyMethod Contains = Capture<string>(s => s.Contains(string.Empty));

                internal static readonly LazyMethod StartsWith = Capture<string>(s => s.StartsWith(string.Empty));

                internal static readonly LazyMethod StartsWithStringComparison = Capture<string>(s => s.StartsWith(string.Empty, StringComparison.Ordinal));

                internal static readonly LazyMethod EndsWith = Capture<string>(s => s.EndsWith(string.Empty));

                internal static readonly LazyMethod EndsWithStringComparison = Capture<string>(s => s.EndsWith(string.Empty, StringComparison.Ordinal));
                
                internal static readonly LazyMethod IsNullOrEmpty = Capture<string>(s => string.IsNullOrEmpty(s));

                internal static readonly LazyMethod EqualsMethod = Capture<string>(s => s.Equals(string.Empty));

                internal static readonly LazyMethod EqualsStringComparison = Capture<string>(s => s.Equals(string.Empty, StringComparison.Ordinal));
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

        /*
            Expressions will typically be in a form:
            - with embedded Lambda `Count(p => !p.IsInteresting)`
            - at the end of a Where `Where(p => !p.IsInteresting).Where()`

            The latter form is handled by recursion where evaluation of Visit will 
            take us back into VisitMethodCall to evaluate the Where call.
        */
        private void RecurseToWhereOrRunLambda(MethodCallExpression m)
        {
            Visit(m.Arguments[0]); // creates the query or recurse to "Where"
            if (m.Arguments.Count > 1)
            {
                var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                Visit(lambda.Body);
            }
        }

        private void AddSort(LambdaExpression lambda, bool isStarting, bool ascending)
        {
            var body = lambda.Body as MemberExpression;
            if (body == null)
            {
                throw new NotSupportedException($"The expression {lambda} cannot be used in an Order clause");
            }

            if (isStarting)
            {
                if (OptionalSortDescriptorBuilder == null)
                {
                    OptionalSortDescriptorBuilder = _realm.CreateSortDescriptorForTable(_metadata);
                }
                else
                {
                    var badCall = ascending ? "ThenBy" : "ThenByDescending";
                    throw new NotSupportedException($"You can only use one OrderBy or OrderByDescending clause, subsequent sort conditions should be {badCall}");
                }
            }

            var sortColName = body.Member.Name;
            OptionalSortDescriptorBuilder.AddClause(sortColName, ascending);
        }

        private ObjectHandle VisitElementAt(MethodCallExpression m)
        {
            Visit(m.Arguments.First());
            object argument;
            if (!TryExtractConstantValue(m.Arguments.Last(), out argument) || argument.GetType() != typeof(int))
            {
                throw new NotSupportedException($"The method '{m.Method}' has to be invoked with a single integer constant argument or closure variable");
            }

            var index = (int)argument;

            ObjectHandle obj;
            if (OptionalSortDescriptorBuilder == null)
            {
                var objectPtr = CoreQueryHandle.FindDirect(_realm.SharedRealmHandle, (IntPtr)index);
                obj = Realm.CreateObjectHandle(objectPtr, _realm.SharedRealmHandle);
            }
            else
            {
                using (var rh = _realm.MakeResultsForQuery(CoreQueryHandle, OptionalSortDescriptorBuilder))
                {
                    var objectPtr = rh.GetObjectAtIndex(index);
                    obj = Realm.CreateObjectHandle(objectPtr, _realm.SharedRealmHandle);
                }
            }

            return obj;
        }

        internal override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable))
            {
                if (m.Method.Name == nameof(Queryable.Where))
                {
                    this.Visit(m.Arguments[0]);
                    var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                    this.Visit(lambda.Body);
                    return m;
                }

                if (m.Method.Name == nameof(Queryable.OrderBy))
                {
                    this.Visit(m.Arguments[0]);
                    AddSort((LambdaExpression)StripQuotes(m.Arguments[1]), true, true);
                    return m;
                }

                if (m.Method.Name == nameof(Queryable.OrderByDescending))
                {
                    this.Visit(m.Arguments[0]);
                    AddSort((LambdaExpression)StripQuotes(m.Arguments[1]), true, false);
                    return m;
                }

                if (m.Method.Name == nameof(Queryable.ThenBy))
                {
                    this.Visit(m.Arguments[0]);
                    AddSort((LambdaExpression)StripQuotes(m.Arguments[1]), false, true);
                    return m;
                }

                if (m.Method.Name == nameof(Queryable.ThenByDescending))
                {
                    this.Visit(m.Arguments[0]);
                    AddSort((LambdaExpression)StripQuotes(m.Arguments[1]), false, false);
                    return m;
                }

                if (m.Method.Name == nameof(Queryable.Count))
                {
                    RecurseToWhereOrRunLambda(m);
                    var foundCount = CoreQueryHandle.Count();
                    return Expression.Constant(foundCount);
                }

                if (m.Method.Name == nameof(Queryable.Any))
                {
                    RecurseToWhereOrRunLambda(m);
                    var foundAny = CoreQueryHandle.FindDirect(_realm.SharedRealmHandle) != IntPtr.Zero;
                    return Expression.Constant(foundAny);
                }

                if (m.Method.Name.StartsWith(nameof(Queryable.First)))
                {
                    RecurseToWhereOrRunLambda(m);
                    var firstObjectPtr = IntPtr.Zero;
                    if (OptionalSortDescriptorBuilder == null)
                    {
                        firstObjectPtr = CoreQueryHandle.FindDirect(_realm.SharedRealmHandle);
                    }
                    else
                    {
                        using (ResultsHandle rh = _realm.MakeResultsForQuery(CoreQueryHandle, OptionalSortDescriptorBuilder))
                        {
                            firstObjectPtr = rh.GetObjectAtIndex(0);
                        }
                    }

                    if (firstObjectPtr != IntPtr.Zero)
                    {
                        return Expression.Constant(_realm.MakeObject(_metadata, firstObjectPtr));
                    }

                    if (m.Method.Name == nameof(Queryable.First))
                    {
                        throw new InvalidOperationException("Sequence contains no matching element");
                    }

                    Debug.Assert(m.Method.Name == nameof(Queryable.FirstOrDefault), $"The method {m.Method.Name}  is not supported. We expected {nameof(Queryable.FirstOrDefault)}.");
                    return Expression.Constant(null);
                }

                /*
                // FIXME: See discussion in the test DefaultIfEmptyReturnsDefault
                // kept because it shows part of what might be a viable implementation if can work out architectural issues

                                if (m.Method.Name == nameof(Queryable.DefaultIfEmpty))
                                {
                                    RecurseToWhereOrRunLambda(m);
                                    IntPtr firstObjectPtr = _coreQueryHandle.FindDirect(IntPtr.Zero);
                                    if (firstObjectPtr != IntPtr.Zero)
                                        return m;  // as if just a "Where"
                                    var innerType = m.Type.GetGenericArguments()[0];
                                    var listType = typeof(List<>).MakeGenericType(innerType);
                                    var singleNullItemList = Activator.CreateInstance(listType);
                                    ((IList)singleNullItemList).Add(null);
                                    return Expression.Constant(singleNullItemList);
                                }
                */
                if (m.Method.Name.StartsWith(nameof(Queryable.Single)))  // same as unsorted First with extra checks
                {
                    RecurseToWhereOrRunLambda(m);
                    var firstObjectPtr = CoreQueryHandle.FindDirect(_realm.SharedRealmHandle);
                    if (firstObjectPtr == IntPtr.Zero)
                    {
                        if (m.Method.Name == nameof(Queryable.Single))
                        {
                            throw new InvalidOperationException("Sequence contains no matching element");
                        }

                        Debug.Assert(m.Method.Name == nameof(Queryable.SingleOrDefault), $"The method {m.Method.Name}  is not supported. We expected {nameof(Queryable.SingleOrDefault)}.");
                        return Expression.Constant(null);
                    }

                    var firstObject = Realm.CreateObjectHandle(firstObjectPtr, _realm.SharedRealmHandle);
                    var nextObjectPtr = CoreQueryHandle.FindNext(firstObject);
                    if (nextObjectPtr != IntPtr.Zero)
                    {
                        throw new InvalidOperationException("Sequence contains more than one matching element");
                    }

                    return Expression.Constant(_realm.MakeObject(_metadata, firstObject));
                }

                if (m.Method.Name.StartsWith(nameof(Queryable.Last)))
                {
                    RecurseToWhereOrRunLambda(m);

                    var lastObjectPtr = IntPtr.Zero;
                    using (ResultsHandle rh = _realm.MakeResultsForQuery(CoreQueryHandle, OptionalSortDescriptorBuilder))
                    {
                        var lastIndex = rh.Count() - 1;
                        if (lastIndex >= 0)
                        {
                            lastObjectPtr = rh.GetObjectAtIndex(lastIndex);
                        }
                    }

                    if (lastObjectPtr != IntPtr.Zero)
                    {
                        return Expression.Constant(_realm.MakeObject(_metadata, lastObjectPtr));
                    }

                    if (m.Method.Name == nameof(Queryable.Last))
                    {
                        throw new InvalidOperationException("Sequence contains no matching element");
                    }

                    Debug.Assert(m.Method.Name == nameof(Queryable.LastOrDefault), $"The method {m.Method.Name}  is not supported. We expected {nameof(Queryable.LastOrDefault)}.");
                    return Expression.Constant(null);
                }

                if (m.Method.Name.StartsWith(nameof(Queryable.ElementAt)))
                {
                    var objectHandle = VisitElementAt(m);
                    if (objectHandle == null || objectHandle.IsInvalid)
                    {
                        if (m.Method.Name == nameof(Queryable.ElementAt))
                        {
                            throw new ArgumentOutOfRangeException();
                        }

                        Debug.Assert(m.Method.Name == nameof(Queryable.ElementAtOrDefault), $"The method {m.Method.Name}  is not supported. We expected {nameof(Queryable.ElementAtOrDefault)}.");
                        return Expression.Constant(null);
                    }

                    return Expression.Constant(_realm.MakeObject(_metadata, objectHandle));
                }
            }

            if (m.Method.DeclaringType == typeof(string))
            {
                QueryHandle.Operation<string> queryMethod = null;

                if (m.Method == Methods.String.Contains.Value)
                {
                    queryMethod = (q, c, v) => q.StringContains(c, v, caseSensitive: true);
                }
                else if (m.Method == Methods.String.StartsWith.Value)
                {
                    queryMethod = (q, c, v) => q.StringStartsWith(c, v, caseSensitive: true);
                }
                else if (m.Method == Methods.String.StartsWithStringComparison.Value)
                {
                    queryMethod = (q, c, v) => q.StringStartsWith(c, v, GetComparisonCaseSensitive(m));
                }
                else if (m.Method == Methods.String.EndsWith.Value)
                {
                    queryMethod = (q, c, v) => q.StringEndsWith(c, v, caseSensitive: true);
                }
                else if (m.Method == Methods.String.EndsWithStringComparison.Value)
                {
                    queryMethod = (q, c, v) => q.StringEndsWith(c, v, GetComparisonCaseSensitive(m));
                }
                else if (m.Method == Methods.String.IsNullOrEmpty.Value)
                {
                    var member = m.Arguments.SingleOrDefault() as MemberExpression;
                    if (member == null)
                    {
                        throw new NotSupportedException($"The method '{m.Method}' has to be invoked with a RealmObject member");
                    }

                    var columnIndex = CoreQueryHandle.GetColumnIndex(member.Member.Name);

                    CoreQueryHandle.GroupBegin();
                    CoreQueryHandle.NullEqual(columnIndex);
                    CoreQueryHandle.Or();
                    CoreQueryHandle.StringEqual(columnIndex, string.Empty, caseSensitive: true);
                    CoreQueryHandle.GroupEnd();
                    return m;
                }
                else if (m.Method == Methods.String.EqualsMethod.Value)
                {
                    queryMethod = (q, c, v) => q.StringEqual(c, v, caseSensitive: true);
                }
                else if (m.Method == Methods.String.EqualsStringComparison.Value)
                {
                    queryMethod = (q, c, v) => q.StringEqual(c, v, GetComparisonCaseSensitive(m));
                }

                if (queryMethod != null)
                {
                    var member = m.Object as MemberExpression;
                    if (member == null)
                    {
                        throw new NotSupportedException($"The method '{m.Method}' has to be invoked on a RealmObject member");
                    }

                    var columnIndex = CoreQueryHandle.GetColumnIndex(member.Member.Name);

                    object argument;
                    if (!TryExtractConstantValue(m.Arguments[0], out argument) || argument.GetType() != typeof(string))
                    {
                        throw new NotSupportedException($"The method '{m.Method}' has to be invoked with a single string constant argument or closure variable");
                    }

                    queryMethod(CoreQueryHandle, columnIndex, (string)argument);
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
                    CoreQueryHandle.Not();
                    this.Visit(u.Operand);  // recurse into richer expression, expect to VisitCombination
                    break;
                default:
                    throw new NotSupportedException($"The unary operator '{u.NodeType}' is not supported");
            }

            return u;
        }

        protected void VisitCombination(BinaryExpression b, Action<QueryHandle> combineWith)
        {
            CoreQueryHandle.GroupBegin();
            Visit(b.Left);
            combineWith(CoreQueryHandle);
            Visit(b.Right);
            CoreQueryHandle.GroupEnd();
        }

        internal static bool TryExtractConstantValue(Expression expr, out object value)
        {
            var constant = expr as ConstantExpression;
            if (constant != null)
            {
                value = constant.Value;
                return true;
            }

            var memberAccess = expr as MemberExpression;
            var fieldInfo = memberAccess?.Member as FieldInfo;
            if (fieldInfo != null)
            {
                if (fieldInfo.Attributes.HasFlag(FieldAttributes.Static))
                {
                    // Handle static fields (e.g. string.Empty)
                    value = fieldInfo.GetValue(null);
                    return true;
                }

                object targetObject;
                if (TryExtractConstantValue(memberAccess.Expression, out targetObject))
                {
                    value = fieldInfo.GetValue(targetObject);
                    return true;
                }
            }

            var propertyInfo = memberAccess?.Member as PropertyInfo;
            if (propertyInfo != null)
            {
                if (propertyInfo.GetMethod != null && propertyInfo.GetMethod.Attributes.HasFlag(MethodAttributes.Static))
                {
                    value = propertyInfo.GetValue(null);
                    return true;
                }

                object targetObject;
                if (TryExtractConstantValue(memberAccess.Expression, out targetObject))
                {
                    value = propertyInfo.GetValue(targetObject);
                    return true;
                }
            }

            value = null;
            return false;
        }

        internal override Expression VisitBinary(BinaryExpression b)
        {
            if (b.NodeType == ExpressionType.AndAlso)  // Boolean And with short-circuit
            {
                VisitCombination(b, (qh) => { /* noop -- AND is the default combinator */ });
            }
            else if (b.NodeType == ExpressionType.OrElse)  // Boolean Or with short-circuit
            {
                VisitCombination(b, qh => qh.Or());
            }
            else
            {
                var memberExpression = b.Left as MemberExpression;

                // bit of a hack to cope with the way LINQ changes the RHS of a char literal to an Int32
                // so an incoming lambda looks like {p => (Convert(p.CharProperty) == 65)}
                // from Where(p => p.CharProperty == 'A')
                if (memberExpression == null && b.Left.NodeType == ExpressionType.Convert)
                {
                    memberExpression = ((UnaryExpression)b.Left).Operand as MemberExpression;
                }

                if (memberExpression == null ||
                    memberExpression.Member.MemberType != MemberTypes.Property ||
                    !_metadata.Schema.PropertyNames.Contains(memberExpression.Member.Name))
                {
                    throw new NotSupportedException($"The left-hand side of the {b.NodeType} operator must be a direct access to a persisted property in Realm.\nUnable to process '{b.Left}'.");
                }

                var leftName = memberExpression.Member.Name;

                object rightValue;
                if (!TryExtractConstantValue(b.Right, out rightValue))
                {
                    throw new NotSupportedException($"The rhs of the binary operator '{b.NodeType}' should be a constant or closure variable expression. \nUnable to process `{b.Right}`");
                }

                switch (b.NodeType)
                {
                    case ExpressionType.Equal:
                        AddQueryEqual(CoreQueryHandle, leftName, rightValue);
                        break;

                    case ExpressionType.NotEqual:
                        AddQueryNotEqual(CoreQueryHandle, leftName, rightValue);
                        break;

                    case ExpressionType.LessThan:
                        AddQueryLessThan(CoreQueryHandle, leftName, rightValue);
                        break;

                    case ExpressionType.LessThanOrEqual:
                        AddQueryLessThanOrEqual(CoreQueryHandle, leftName, rightValue);
                        break;

                    case ExpressionType.GreaterThan:
                        AddQueryGreaterThan(CoreQueryHandle, leftName, rightValue);
                        break;

                    case ExpressionType.GreaterThanOrEqual:
                        AddQueryGreaterThanOrEqual(CoreQueryHandle, leftName, rightValue);
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

            if (value == null)
            {
                queryHandle.NullEqual(columnIndex);
            }
            else if (value is string)
            {
                queryHandle.StringEqual(columnIndex, (string)value, caseSensitive: true);
            }
            else if (value is bool)
            {
                queryHandle.BoolEqual(columnIndex, (bool)value);
            }
            else if (value is char)
            {
                queryHandle.IntEqual(columnIndex, (int)value);
            }
            else if (value is int)
            {
                queryHandle.IntEqual(columnIndex, (int)value);
            }
            else if (value is long)
            {
                queryHandle.LongEqual(columnIndex, (long)value);
            }
            else if (value is float)
            {
                queryHandle.FloatEqual(columnIndex, (float)value);
            }
            else if (value is double)
            {
                queryHandle.DoubleEqual(columnIndex, (double)value);
            }
            else if (value is DateTimeOffset)
            {
                queryHandle.TimestampTicksEqual(columnIndex, (DateTimeOffset)value);
            }
            else if (value is byte[])
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
            else if (value is RealmObject)
            {
                queryHandle.ObjectEqual(columnIndex, ((RealmObject)value).ObjectHandle);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static void AddQueryNotEqual(QueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = queryHandle.GetColumnIndex(columnName);

            if (value == null)
            {
                queryHandle.NullNotEqual(columnIndex);
            }
            else if (value is string)
            {
                queryHandle.StringNotEqual(columnIndex, (string)value, caseSensitive: true);
            }
            else if (value is bool)
            {
                queryHandle.BoolNotEqual(columnIndex, (bool)value);
            }
            else if (value is char)
            {
                queryHandle.IntNotEqual(columnIndex, (int)value);
            }
            else if (value is int)
            {
                queryHandle.IntNotEqual(columnIndex, (int)value);
            }
            else if (value is long)
            {
                queryHandle.LongNotEqual(columnIndex, (long)value);
            }
            else if (value is float)
            {
                queryHandle.FloatNotEqual(columnIndex, (float)value);
            }
            else if (value is double)
            {
                queryHandle.DoubleNotEqual(columnIndex, (double)value);
            }
            else if (value is DateTimeOffset)
            {
                queryHandle.TimestampTicksNotEqual(columnIndex, (DateTimeOffset)value);
            }
            else if (value is byte[])
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
            else if (value is RealmObject)
            {
                queryHandle.Not();
                queryHandle.ObjectEqual(columnIndex, ((RealmObject)value).ObjectHandle);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static void AddQueryLessThan(QueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = queryHandle.GetColumnIndex(columnName);

            if (value is char)
            {
                queryHandle.IntLess(columnIndex, (int)value);
            }
            else if (value is int)
            {
                queryHandle.IntLess(columnIndex, (int)value);
            }
            else if (value is long)
            {
                queryHandle.LongLess(columnIndex, (long)value);
            }
            else if (value is float)
            {
                queryHandle.FloatLess(columnIndex, (float)value);
            }
            else if (value is double)
            {
                queryHandle.DoubleLess(columnIndex, (double)value);
            }
            else if (value is DateTimeOffset)
            {
                queryHandle.TimestampTicksLess(columnIndex, (DateTimeOffset)value);
            }
            else if (value is string || value is bool)
            {
                throw new Exception($"Unsupported type {value.GetType().Name}");
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static void AddQueryLessThanOrEqual(QueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = queryHandle.GetColumnIndex(columnName);

            if (value is char)
            {
                queryHandle.IntLessEqual(columnIndex, (int)value);
            }
            else if (value is int)
            {
                queryHandle.IntLessEqual(columnIndex, (int)value);
            }
            else if (value is long)
            {
                queryHandle.LongLessEqual(columnIndex, (long)value);
            }
            else if (value is float)
            {
                queryHandle.FloatLessEqual(columnIndex, (float)value);
            }
            else if (value is double)
            {
                queryHandle.DoubleLessEqual(columnIndex, (double)value);
            }
            else if (value is DateTimeOffset)
            {
                queryHandle.TimestampTicksLessEqual(columnIndex, (DateTimeOffset)value);
            }
            else if (value is string || value is bool)
            {
                throw new Exception($"Unsupported type {value.GetType().Name}");
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static void AddQueryGreaterThan(QueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = queryHandle.GetColumnIndex(columnName);

            if (value is char)
            {
                queryHandle.IntGreater(columnIndex, (int)value);
            }
            else if (value is int)
            {
                queryHandle.IntGreater(columnIndex, (int)value);
            }
            else if (value is long)
            {
                queryHandle.LongGreater(columnIndex, (long)value);
            }
            else if (value is float)
            {
                queryHandle.FloatGreater(columnIndex, (float)value);
            }
            else if (value is double)
            {
                queryHandle.DoubleGreater(columnIndex, (double)value);
            }
            else if (value is DateTimeOffset)
            {
                queryHandle.TimestampTicksGreater(columnIndex, (DateTimeOffset)value);
            }
            else if (value is string || value is bool)
            {
                throw new Exception($"Unsupported type {value.GetType().Name}");
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static void AddQueryGreaterThanOrEqual(QueryHandle queryHandle, string columnName, object value)
        {
            var columnIndex = queryHandle.GetColumnIndex(columnName);

            if (value is char)
            {
                queryHandle.IntGreaterEqual(columnIndex, (int)value);
            }
            else if (value is int)
            {
                queryHandle.IntGreaterEqual(columnIndex, (int)value);
            }
            else if (value is long)
            {
                queryHandle.LongGreaterEqual(columnIndex, (long)value);
            }
            else if (value is float)
            {
                queryHandle.FloatGreaterEqual(columnIndex, (float)value);
            }
            else if (value is double)
            {
                queryHandle.DoubleGreaterEqual(columnIndex, (double)value);
            }
            else if (value is DateTimeOffset)
            {
                queryHandle.TimestampTicksGreaterEqual(columnIndex, (DateTimeOffset)value);
            }
            else if (value is string || value is bool)
            {
                throw new Exception($"Unsupported type {value.GetType().Name}");
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static bool GetComparisonCaseSensitive(MethodCallExpression m)
        {
            object argument;
            if (!TryExtractConstantValue(m.Arguments[1], out argument) || !(argument is StringComparison))
            {
                throw new NotSupportedException($"The method '{m.Method}' has to be invoked with a string and StringComparison constant arguments.");
            }

            var comparison = (StringComparison)argument;
            bool caseSensitive;
            switch (comparison)
            {
                case StringComparison.Ordinal:
                    caseSensitive = true;
                    break;
                case StringComparison.OrdinalIgnoreCase:
                    caseSensitive = false;
                    break;
                default:
                    throw new NotSupportedException($"The comparison {comparison} is not yet supported. Use {StringComparison.Ordinal} or {StringComparison.OrdinalIgnoreCase}.");
            }

            return caseSensitive;
        }

        // strange as it may seem, this is also called for the LHS when simply iterating All<T>()
        internal override Expression VisitConstant(ConstantExpression c)
        {
            var results = c.Value as IRealmResults;
            if (results != null)
            {
                // assume constant nodes w/ IQueryables are table references
                if (CoreQueryHandle != null)
                {
                    throw new Exception("We already have a table...");
                }

                CoreQueryHandle = CreateQuery(results.ObjectSchema);
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
                else if (c.Value.GetType() == typeof(object))
                {
                    throw new NotSupportedException($"The constant for '{c.Value}' is not supported");
                }
                else
                {
                }
            }

            return c;
        }

        private QueryHandle CreateQuery(Schema.ObjectSchema elementType)
        {
            var tableHandle = _realm.Metadata[elementType.Name].Table;
            var queryHandle = tableHandle.TableWhere();

            // At this point sh is invalid due to its handle being uninitialized, but the root is set correctly
            // a finalize at this point will not leak anything and the handle will not do anything

            // now, set the TableView handle...
            RuntimeHelpers.PrepareConstrainedRegions(); // the following finally will run with no out-of-band exceptions
            try
            {
            }
            finally
            {
                queryHandle.SetHandle(tableHandle.Where());
            } // at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly

            return queryHandle;
        }

        internal override Expression VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                if (m.Type == typeof(bool))
                {
                    object rhs = true;  // box value
                    var leftName = m.Member.Name;
                    AddQueryEqual(CoreQueryHandle, leftName, rhs);
                }

                return m;
            }

            throw new NotSupportedException($"The member '{m.Member.Name}' is not supported");
        }
    }
}