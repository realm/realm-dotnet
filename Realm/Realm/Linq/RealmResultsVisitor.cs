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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Realms.Native;
using Realms.Schema;
using LazyMethod = System.Lazy<System.Reflection.MethodInfo>;

namespace Realms
{
    internal class RealmResultsVisitor : ExpressionVisitor
    {
        private readonly Realm _realm;
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

                internal static readonly LazyMethod ContainsStringComparison = Capture<string>(s => s.Contains(string.Empty, StringComparison.Ordinal));

                internal static readonly LazyMethod Like = Capture<string>(s => s.Like(string.Empty, true));

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
                    OptionalSortDescriptorBuilder = new SortDescriptorBuilder(_metadata.Table);
                }
                else
                {
                    var badCall = ascending ? "ThenBy" : "ThenByDescending";
                    throw new NotSupportedException($"You can only use one OrderBy or OrderByDescending clause, subsequent sort conditions should be {badCall}");
                }
            }

            var propertyChain = TraverseSort(body);

            OptionalSortDescriptorBuilder.AddClause(propertyChain, ascending);
        }

        private IEnumerable<IntPtr> TraverseSort(MemberExpression expression)
        {
            var chain = new List<IntPtr>();

            while (expression != null)
            {
                var type = expression.Member.DeclaringType;
                var typeName = type.GetTypeInfo().GetMappedOrOriginalName();
                if (!_realm.Metadata.TryGetValue(typeName, out var metadata))
                {
                    throw new NotSupportedException($"The class {type.Name} is not in the limited set of classes for this Realm, so sorting by its properties is not allowed.");
                }

                var columnName = GetColumnName(expression);
                if (!metadata.PropertyIndices.TryGetValue(columnName, out var index))
                {
                    throw new NotSupportedException($"The property {columnName} is not a persisted property on {type.Name} so sorting by it is not allowed.");
                }

                chain.Add(index);
                expression = expression.Expression as MemberExpression;
            }

            chain.Reverse();

            return chain;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable))
            {
                if (node.Method.Name == nameof(Queryable.Where))
                {
                    Visit(node.Arguments[0]);
                    var lambda = (LambdaExpression)StripQuotes(node.Arguments[1]);
                    Visit(lambda.Body);
                    return node;
                }

                if (node.Method.Name == nameof(Queryable.OrderBy))
                {
                    Visit(node.Arguments[0]);
                    AddSort((LambdaExpression)StripQuotes(node.Arguments[1]), true, true);
                    return node;
                }

                if (node.Method.Name == nameof(Queryable.OrderByDescending))
                {
                    Visit(node.Arguments[0]);
                    AddSort((LambdaExpression)StripQuotes(node.Arguments[1]), true, false);
                    return node;
                }

                if (node.Method.Name == nameof(Queryable.ThenBy))
                {
                    Visit(node.Arguments[0]);
                    AddSort((LambdaExpression)StripQuotes(node.Arguments[1]), false, true);
                    return node;
                }

                if (node.Method.Name == nameof(Queryable.ThenByDescending))
                {
                    Visit(node.Arguments[0]);
                    AddSort((LambdaExpression)StripQuotes(node.Arguments[1]), false, false);
                    return node;
                }

                if (node.Method.Name == nameof(Queryable.Count))
                {
                    RecurseToWhereOrRunLambda(node);
                    var foundCount = CoreQueryHandle.Count();
                    return Expression.Constant(foundCount);
                }

                if (node.Method.Name == nameof(Queryable.Any))
                {
                    RecurseToWhereOrRunLambda(node);
                    var foundAny = CoreQueryHandle.TryFindDirect(_realm.SharedRealmHandle, out _);
                    return Expression.Constant(foundAny);
                }

                if (node.Method.Name.StartsWith(nameof(Queryable.First)))
                {
                    RecurseToWhereOrRunLambda(node);
                    ObjectHandle firstObject;
                    if (OptionalSortDescriptorBuilder == null)
                    {
                        CoreQueryHandle.TryFindDirect(_realm.SharedRealmHandle, out firstObject);
                    }
                    else
                    {
                        using (ResultsHandle rh = _realm.MakeResultsForQuery(CoreQueryHandle, OptionalSortDescriptorBuilder))
                        {
                            rh.TryGetObjectAtIndex(0, out firstObject);
                        }
                    }

                    if (firstObject != null)
                    {
                        return Expression.Constant(_realm.MakeObject(_metadata, firstObject));
                    }

                    if (node.Method.Name == nameof(Queryable.First))
                    {
                        throw new InvalidOperationException("Sequence contains no matching element");
                    }

                    Debug.Assert(node.Method.Name == nameof(Queryable.FirstOrDefault), $"The method {node.Method.Name}  is not supported. We expected {nameof(Queryable.FirstOrDefault)}.");
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
                if (node.Method.Name.StartsWith(nameof(Queryable.Single)))  // same as unsorted First with extra checks
                {
                    RecurseToWhereOrRunLambda(node);
                    if (!CoreQueryHandle.TryFindDirect(_realm.SharedRealmHandle, out var firstObject))
                    {
                        if (node.Method.Name == nameof(Queryable.Single))
                        {
                            throw new InvalidOperationException("Sequence contains no matching element");
                        }

                        Debug.Assert(node.Method.Name == nameof(Queryable.SingleOrDefault), $"The method {node.Method.Name}  is not supported. We expected {nameof(Queryable.SingleOrDefault)}.");
                        return Expression.Constant(null);
                    }

                    if (CoreQueryHandle.TryFindNext(firstObject, _realm.SharedRealmHandle, out _))
                    {
                        throw new InvalidOperationException("Sequence contains more than one matching element");
                    }

                    return Expression.Constant(_realm.MakeObject(_metadata, firstObject));
                }

                if (node.Method.Name.StartsWith(nameof(Queryable.Last)))
                {
                    RecurseToWhereOrRunLambda(node);

                    ObjectHandle lastObject = null;
                    using (ResultsHandle rh = _realm.MakeResultsForQuery(CoreQueryHandle, OptionalSortDescriptorBuilder))
                    {
                        var lastIndex = rh.Count() - 1;
                        if (lastIndex >= 0)
                        {
                            rh.TryGetObjectAtIndex(lastIndex, out lastObject);
                        }
                    }

                    if (lastObject != null)
                    {
                        return Expression.Constant(_realm.MakeObject(_metadata, lastObject));
                    }

                    if (node.Method.Name == nameof(Queryable.Last))
                    {
                        throw new InvalidOperationException("Sequence contains no matching element");
                    }

                    Debug.Assert(node.Method.Name == nameof(Queryable.LastOrDefault), $"The method {node.Method.Name}  is not supported. We expected {nameof(Queryable.LastOrDefault)}.");
                    return Expression.Constant(null);
                }

                if (node.Method.Name.StartsWith(nameof(Queryable.ElementAt)))
                {
                    Visit(node.Arguments.First());
                    if (!TryExtractConstantValue(node.Arguments.Last(), out object argument) || argument.GetType() != typeof(int))
                    {
                        throw new NotSupportedException($"The method '{node.Method}' has to be invoked with a single integer constant argument or closure variable");
                    }

                    ObjectHandle objectHandle;
                    var index = (int)argument;
                    if (OptionalSortDescriptorBuilder == null)
                    {
                        CoreQueryHandle.TryFindDirect(_realm.SharedRealmHandle, out objectHandle, (IntPtr)index);
                    }
                    else
                    {
                        using (var rh = _realm.MakeResultsForQuery(CoreQueryHandle, OptionalSortDescriptorBuilder))
                        {
                            rh.TryGetObjectAtIndex(index, out objectHandle);
                        }
                    }

                    if (objectHandle != null)
                    {
                        return Expression.Constant(_realm.MakeObject(_metadata, objectHandle));
                    }

                    if (node.Method.Name == nameof(Queryable.ElementAt))
                    {
                        throw new ArgumentOutOfRangeException();
                    }

                    Debug.Assert(node.Method.Name == nameof(Queryable.ElementAtOrDefault), $"The method {node.Method.Name}  is not supported. We expected {nameof(Queryable.ElementAtOrDefault)}.");
                    return Expression.Constant(null);
                }
            }

            if (node.Method.DeclaringType == typeof(string) ||
                node.Method.DeclaringType == typeof(StringExtensions))
            {
                QueryHandle.Operation<string> queryMethod = null;

                // For extension methods, member should be m.Arguments[0] as MemberExpression;
                MemberExpression member = null;

                // For extension methods, that should be 1
                var stringArgumentIndex = 0;

                if (AreMethodsSame(node.Method, Methods.String.Contains.Value))
                {
                    queryMethod = (q, c, v) => q.StringContains(c, v, caseSensitive: true);
                }
                else if (AreMethodsSame(node.Method, Methods.String.ContainsStringComparison.Value))
                {
                    member = node.Arguments[0] as MemberExpression;
                    stringArgumentIndex = 1;
                    queryMethod = (q, c, v) => q.StringContains(c, v, GetComparisonCaseSensitive(node));
                }
                else if (AreMethodsSame(node.Method, Methods.String.StartsWith.Value))
                {
                    queryMethod = (q, c, v) => q.StringStartsWith(c, v, caseSensitive: true);
                }
                else if (AreMethodsSame(node.Method, Methods.String.StartsWithStringComparison.Value))
                {
                    queryMethod = (q, c, v) => q.StringStartsWith(c, v, GetComparisonCaseSensitive(node));
                }
                else if (AreMethodsSame(node.Method, Methods.String.EndsWith.Value))
                {
                    queryMethod = (q, c, v) => q.StringEndsWith(c, v, caseSensitive: true);
                }
                else if (AreMethodsSame(node.Method, Methods.String.EndsWithStringComparison.Value))
                {
                    queryMethod = (q, c, v) => q.StringEndsWith(c, v, GetComparisonCaseSensitive(node));
                }
                else if (AreMethodsSame(node.Method, Methods.String.IsNullOrEmpty.Value))
                {
                    member = node.Arguments.SingleOrDefault() as MemberExpression;
                    if (member == null)
                    {
                        throw new NotSupportedException($"The method '{node.Method}' has to be invoked with a RealmObject member");
                    }

                    var columnName = GetColumnName(member, node.NodeType);
                    var columnIndex = CoreQueryHandle.GetColumnIndex(columnName);

                    CoreQueryHandle.GroupBegin();
                    CoreQueryHandle.NullEqual(columnIndex);
                    CoreQueryHandle.Or();
                    CoreQueryHandle.StringEqual(columnIndex, string.Empty, caseSensitive: true);
                    CoreQueryHandle.GroupEnd();
                    return node;
                }
                else if (AreMethodsSame(node.Method, Methods.String.EqualsMethod.Value))
                {
                    queryMethod = (q, c, v) => q.StringEqual(c, v, caseSensitive: true);
                }
                else if (AreMethodsSame(node.Method, Methods.String.EqualsStringComparison.Value))
                {
                    queryMethod = (q, c, v) => q.StringEqual(c, v, GetComparisonCaseSensitive(node));
                }
                else if (AreMethodsSame(node.Method, Methods.String.Like.Value))
                {
                    member = node.Arguments[0] as MemberExpression;
                    stringArgumentIndex = 1;
                    if (!TryExtractConstantValue(node.Arguments.Last(), out object caseSensitive) || !(caseSensitive is bool))
                    {
                        throw new NotSupportedException($"The method '{node.Method}' has to be invoked with a string and boolean constant arguments.");
                    }

                    queryMethod = (q, c, v) => q.StringLike(c, v, (bool)caseSensitive);
                }

                if (queryMethod != null)
                {
                    member = member ?? node.Object as MemberExpression;

                    if (member == null)
                    {
                        throw new NotSupportedException($"The method '{node.Method}' has to be invoked on a RealmObject member");
                    }

                    var columnName = GetColumnName(member, node.NodeType);
                    var columnIndex = CoreQueryHandle.GetColumnIndex(columnName);

                    if (!TryExtractConstantValue(node.Arguments[stringArgumentIndex], out object argument) ||
                        (argument != null && argument.GetType() != typeof(string)))
                    {
                        throw new NotSupportedException($"The method '{node.Method}' has to be invoked with a single string constant argument or closure variable");
                    }

                    queryMethod(CoreQueryHandle, columnIndex, (string)argument);
                    return node;
                }
            }

            throw new NotSupportedException($"The method '{node.Method.Name}' is not supported");
        }

        // Compares two methods for equality. .NET Native's == doesn't return expected results.
        private static bool AreMethodsSame(MethodInfo first, MethodInfo second)
        {
            if (first == second)
            {
                return true;
            }

            if (first.Name != second.Name ||
                first.DeclaringType != second.DeclaringType)
            {
                return false;
            }

            var firstParameters = first.GetParameters();
            var secondParameters = second.GetParameters();

            if (firstParameters.Length != secondParameters.Length)
            {
                return false;
            }

            for (var i = 0; i < firstParameters.Length; i++)
            {
                if (firstParameters[i].ParameterType != secondParameters[i].ParameterType)
                {
                    return false;
                }
            }

            return true;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    CoreQueryHandle.Not();
                    Visit(node.Operand);  // recurse into richer expression, expect to VisitCombination
                    break;
                default:
                    throw new NotSupportedException($"The unary operator '{node.NodeType}' is not supported");
            }

            return node;
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
            if (expr.NodeType == ExpressionType.Convert)
            {
                var operand = ((UnaryExpression)expr).Operand;
                if (TryExtractConstantValue(operand, out var innerValue))
                {
                    var parameter = Expression.Parameter(operand.Type, "op");
                    value = Expression.Lambda(expr, parameter).Compile().DynamicInvoke(innerValue);
                    return true;
                }

                value = null;
                return false;
            }

            if (expr is ConstantExpression constant)
            {
                value = constant.Value;
                return true;
            }

            var memberAccess = expr as MemberExpression;
            if (memberAccess?.Member is FieldInfo fieldInfo)
            {
                if (fieldInfo.Attributes.HasFlag(FieldAttributes.Static))
                {
                    // Handle static fields (e.g. string.Empty)
                    value = fieldInfo.GetValue(null);
                    return true;
                }

                if (TryExtractConstantValue(memberAccess.Expression, out object targetObject))
                {
                    value = fieldInfo.GetValue(targetObject);
                    return true;
                }
            }

            if (memberAccess?.Member is PropertyInfo propertyInfo)
            {
                if (propertyInfo.GetMethod != null && propertyInfo.GetMethod.Attributes.HasFlag(MethodAttributes.Static))
                {
                    value = propertyInfo.GetValue(null);
                    return true;
                }

                if (TryExtractConstantValue(memberAccess.Expression, out object targetObject))
                {
                    value = propertyInfo.GetValue(targetObject);
                    return true;
                }
            }

            value = null;
            return false;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.AndAlso)  // Boolean And with short-circuit
            {
                VisitCombination(node, (qh) => { /* noop -- AND is the default combinator */ });
            }
            else if (node.NodeType == ExpressionType.OrElse)  // Boolean Or with short-circuit
            {
                VisitCombination(node, qh => qh.Or());
            }
            else
            {
                var leftExpression = node.Left;
                var memberExpression = leftExpression as MemberExpression;
                var rightExpression = node.Right;

                // bit of a hack to cope with the way LINQ changes the RHS of a char literal to an Int32
                // so an incoming lambda looks like {p => (Convert(p.CharProperty) == 65)}
                // from Where(p => p.CharProperty == 'A')
                while (memberExpression == null && leftExpression.NodeType == ExpressionType.Convert)
                {
                    leftExpression = ((UnaryExpression)leftExpression).Operand;
                    memberExpression = leftExpression as MemberExpression;
                }

                var leftName = GetColumnName(memberExpression, node.NodeType);

                if (!TryExtractConstantValue(node.Right, out object rightValue))
                {
                    throw new NotSupportedException($"The rhs of the binary operator '{rightExpression.NodeType}' should be a constant or closure variable expression. \nUnable to process `{node.Right}`");
                }

                switch (node.NodeType)
                {
                    case ExpressionType.Equal:
                        AddQueryEqual(CoreQueryHandle, leftName, rightValue, memberExpression.Type);
                        break;
                    case ExpressionType.NotEqual:
                        AddQueryNotEqual(CoreQueryHandle, leftName, rightValue, memberExpression.Type);
                        break;
                    case ExpressionType.LessThan:
                        AddQueryLessThan(CoreQueryHandle, leftName, rightValue, memberExpression.Type);
                        break;
                    case ExpressionType.LessThanOrEqual:
                        AddQueryLessThanOrEqual(CoreQueryHandle, leftName, rightValue, memberExpression.Type);
                        break;
                    case ExpressionType.GreaterThan:
                        AddQueryGreaterThan(CoreQueryHandle, leftName, rightValue, memberExpression.Type);
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        AddQueryGreaterThanOrEqual(CoreQueryHandle, leftName, rightValue, memberExpression.Type);
                        break;
                    default:
                        throw new NotSupportedException($"The binary operator '{node.NodeType}' is not supported");
                }
            }

            return node;
        }

        private static void AddQueryEqual(QueryHandle queryHandle, string columnName, object value, Type columnType)
        {
            var columnIndex = queryHandle.GetColumnIndex(columnName);

            switch (value)
            {
                case null:
                    queryHandle.NullEqual(columnIndex);
                    break;
                case string stringValue:
                    queryHandle.StringEqual(columnIndex, stringValue, caseSensitive: true);
                    break;
                case bool boolValue:
                    queryHandle.BoolEqual(columnIndex, boolValue);
                    break;
                case DateTimeOffset dateValue:
                    queryHandle.TimestampTicksEqual(columnIndex, dateValue);
                    break;
                case byte[] buffer:
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
                            queryHandle.BinaryEqual(columnIndex, (IntPtr)bufferPtr, (IntPtr)buffer.LongCount());
                        }
                    }
                    break;
                case RealmObject obj:
                    queryHandle.ObjectEqual(columnIndex, obj.ObjectHandle);
                    break;
                default:
                    // The other types aren't handled by the switch because of potential compiler applied conversions
                    AddQueryForConvertibleTypes(columnIndex, value, columnType, queryHandle.NumericEqualMethods);
                    break;
            }
        }

        private static void AddQueryNotEqual(QueryHandle queryHandle, string columnName, object value, Type columnType)
        {
            var columnIndex = queryHandle.GetColumnIndex(columnName);
            switch (value)
            {
                case null:
                    queryHandle.NullNotEqual(columnIndex);
                    break;
                case string stringValue:
                    queryHandle.StringNotEqual(columnIndex, stringValue, caseSensitive: true);
                    break;
                case bool boolValue:
                    queryHandle.BoolNotEqual(columnIndex, boolValue);
                    break;
                case DateTimeOffset date:
                    queryHandle.TimestampTicksNotEqual(columnIndex, date);
                    break;
                case byte[] buffer:
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
                            queryHandle.BinaryNotEqual(columnIndex, (IntPtr)bufferPtr, (IntPtr)buffer.LongCount());
                        }
                    }
                    break;
                case RealmObject obj:
                    queryHandle.Not();
                    queryHandle.ObjectEqual(columnIndex, obj.ObjectHandle);
                    break;
                default:
                    // The other types aren't handled by the switch because of potential compiler applied conversions
                    AddQueryForConvertibleTypes(columnIndex, value, columnType, queryHandle.NumericNotEqualMethods);
                    break;
            }
        }

        private static void AddQueryLessThan(QueryHandle queryHandle, string columnName, object value, Type columnType)
        {
            var columnIndex = queryHandle.GetColumnIndex(columnName);
            switch (value)
            {
                case DateTimeOffset date:
                    queryHandle.TimestampTicksLess(columnIndex, date);
                    break;
                case string _:
                case bool _:
                    throw new Exception($"Unsupported type {value.GetType().Name}");
                default:
                    // The other types aren't handled by the switch because of potential compiler applied conversions
                    AddQueryForConvertibleTypes(columnIndex, value, columnType, queryHandle.NumericLessMethods);
                    break;
            }
        }

        private static void AddQueryLessThanOrEqual(QueryHandle queryHandle, string columnName, object value, Type columnType)
        {
            var columnIndex = queryHandle.GetColumnIndex(columnName);
            switch (value)
            {
                case DateTimeOffset date:
                    queryHandle.TimestampTicksLessEqual(columnIndex, date);
                    break;
                case string _:
                case bool _:
                    throw new Exception($"Unsupported type {value.GetType().Name}");
                default:
                    // The other types aren't handled by the switch because of potential compiler applied conversions
                    AddQueryForConvertibleTypes(columnIndex, value, columnType, queryHandle.NumericLessEqualMethods);
                    break;
            }
        }

        private static void AddQueryGreaterThan(QueryHandle queryHandle, string columnName, object value, Type columnType)
        {
            var columnIndex = queryHandle.GetColumnIndex(columnName);
            switch (value)
            {
                case DateTimeOffset date:
                    queryHandle.TimestampTicksGreater(columnIndex, date);
                    break;
                case string _:
                case bool _:
                    throw new Exception($"Unsupported type {value.GetType().Name}");
                default:
                    // The other types aren't handled by the switch because of potential compiler applied conversions
                    AddQueryForConvertibleTypes(columnIndex, value, columnType, queryHandle.NumericGreaterMethods);
                    break;
            }
        }

        private static void AddQueryGreaterThanOrEqual(QueryHandle queryHandle, string columnName, object value, Type columnType)
        {
            var columnIndex = queryHandle.GetColumnIndex(columnName);
            switch (value)
            {
                case DateTimeOffset date:
                    queryHandle.TimestampTicksGreaterEqual(columnIndex, date);
                    break;
                case string _:
                case bool _:
                    throw new Exception($"Unsupported type {value.GetType().Name}");
                default:
                    // The other types aren't handled by the switch because of potential compiler applied conversions
                    AddQueryForConvertibleTypes(columnIndex, value, columnType, queryHandle.NumericGreaterEqualMethods);
                    break;
            }
        }

        private static void AddQueryForConvertibleTypes(IntPtr columnIndex, object value, Type columnType, QueryHandle.NumericQueryMethods queryMethods)
        {
            if (columnType.IsConstructedGenericType && columnType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                columnType = columnType.GetGenericArguments().Single();
            }

            if (columnType == typeof(byte) ||
                columnType == typeof(short) ||
                columnType == typeof(char) ||
                columnType == typeof(int) ||
                columnType == typeof(RealmInteger<byte>) ||
                columnType == typeof(RealmInteger<short>) ||
                columnType == typeof(RealmInteger<int>))
            {
                queryMethods.Int(columnIndex, (int)Convert.ChangeType(value, typeof(int)));
            }
            else if (columnType == typeof(long) ||
                     columnType == typeof(RealmInteger<long>))
            {
                queryMethods.Long(columnIndex, (long)Convert.ChangeType(value, typeof(long)));
            }
            else if (columnType == typeof(float))
            {
                queryMethods.Float(columnIndex, (float)Convert.ChangeType(value, typeof(float)));
            }
            else if (columnType == typeof(double))
            {
                queryMethods.Double(columnIndex, (double)Convert.ChangeType(value, typeof(double)));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static bool GetComparisonCaseSensitive(MethodCallExpression m)
        {
            if (!TryExtractConstantValue(m.Arguments.Last(), out object argument) || !(argument is StringComparison))
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

        private string GetColumnName(MemberExpression memberExpression, ExpressionType? parentType = null)
        {
            var name = memberExpression?.Member.GetMappedOrOriginalName();

            if (parentType.HasValue)
            {
                if (name == null ||
                    memberExpression.Expression.NodeType != ExpressionType.Parameter ||
                    !(memberExpression.Member is PropertyInfo pi) ||
                    !_metadata.Schema.TryFindProperty(name, out var property) ||
                    property.Type.HasFlag(PropertyType.Array))
                {
                    throw new NotSupportedException($"The left-hand side of the {parentType} operator must be a direct access to a persisted property in Realm.\nUnable to process '{memberExpression}'.");
                }
            }

            return name;
        }

        // strange as it may seem, this is also called for the LHS when simply iterating All<T>()
        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value is IQueryableCollection results)
            {
                // assume constant nodes w/ IQueryables are table references
                if (CoreQueryHandle != null)
                {
                    throw new Exception("We already have a table...");
                }

                CoreQueryHandle = results.CreateQuery();
            }
            else if (node.Value?.GetType() == typeof(object))
            {
                throw new NotSupportedException($"The constant for '{node.Value}' is not supported");
            }

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
            {
                if (node.Type == typeof(bool))
                {
                    object rhs = true;  // box value
                    var leftName = GetColumnName(node, node.NodeType);
                    AddQueryEqual(CoreQueryHandle, leftName, rhs, node.Type);
                }

                return node;
            }

            throw new NotSupportedException($"The member '{node.Member.Name}' is not supported");
        }
    }
}