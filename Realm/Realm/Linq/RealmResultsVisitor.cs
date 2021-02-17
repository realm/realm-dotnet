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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson;
using Realms.Schema;
using LazyMethod = System.Lazy<System.Reflection.MethodInfo>;

namespace Realms
{
    internal class RealmResultsVisitor : ExpressionVisitor
    {
        private readonly Realm _realm;
        private readonly RealmObjectBase.Metadata _metadata;

        private QueryHandle _coreQueryHandle;  // set when recurse down to VisitConstant
        private SortDescriptorHandle _sortDescriptor;

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

                [SuppressMessage("Globalization", "CA1310:Specify StringComparison for correctness", Justification = "We want to capture StartsWith(string).")]
                internal static readonly LazyMethod StartsWith = Capture<string>(s => s.StartsWith(string.Empty));

                internal static readonly LazyMethod StartsWithStringComparison = Capture<string>(s => s.StartsWith(string.Empty, StringComparison.Ordinal));

                [SuppressMessage("Globalization", "CA1310:Specify StringComparison for correctness", Justification = "We want to capture EndsWith(string).")]
                internal static readonly LazyMethod EndsWith = Capture<string>(s => s.EndsWith(string.Empty));

                internal static readonly LazyMethod EndsWithStringComparison = Capture<string>(s => s.EndsWith(string.Empty, StringComparison.Ordinal));

                internal static readonly LazyMethod IsNullOrEmpty = Capture<string>(s => string.IsNullOrEmpty(s));

                internal static readonly LazyMethod EqualsMethod = Capture<string>(s => s.Equals(string.Empty));

                internal static readonly LazyMethod EqualsStringComparison = Capture<string>(s => s.Equals(string.Empty, StringComparison.Ordinal));
            }
        }

        internal RealmResultsVisitor(Realm realm, RealmObjectBase.Metadata metadata)
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

        private void AddSort(LambdaExpression lambda, bool ascending)
        {
            if (!(lambda.Body is MemberExpression body))
            {
                throw new NotSupportedException($"The expression {lambda} cannot be used in an Order clause");
            }

            var propertyChain = TraverseSort(body);
            _sortDescriptor.AddClause(_realm.SharedRealmHandle, _metadata.TableKey, propertyChain, ascending);
        }

        private IntPtr[] TraverseSort(MemberExpression expression)
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

            return chain.ToArray();
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

                if (node.Method.Name == nameof(Queryable.OrderBy) || node.Method.Name == nameof(Queryable.ThenBy))
                {
                    Visit(node.Arguments[0]);
                    AddSort((LambdaExpression)StripQuotes(node.Arguments[1]), true);
                    return node;
                }

                if (node.Method.Name == nameof(Queryable.OrderByDescending) || node.Method.Name == nameof(Queryable.ThenByDescending))
                {
                    Visit(node.Arguments[0]);
                    AddSort((LambdaExpression)StripQuotes(node.Arguments[1]), false);
                    return node;
                }

                if (node.Method.Name == nameof(Queryable.Count))
                {
                    RecurseToWhereOrRunLambda(node);
                    var foundCount = _coreQueryHandle.Count();
                    return Expression.Constant(foundCount);
                }

                if (node.Method.Name == nameof(Queryable.Any))
                {
                    RecurseToWhereOrRunLambda(node);
                    return Expression.Constant(_coreQueryHandle.Count() > 0);
                }

                if (node.Method.Name.StartsWith(nameof(Queryable.First), StringComparison.OrdinalIgnoreCase))
                {
                    RecurseToWhereOrRunLambda(node);
                    using var rh = MakeResultsForQuery();
                    return GetObjectAtIndex(0, rh, node.Method.Name);
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

                // same as unsorted First with extra checks
                if (node.Method.Name.StartsWith(nameof(Queryable.Single), StringComparison.OrdinalIgnoreCase))
                {
                    RecurseToWhereOrRunLambda(node);
                    using var rh = MakeResultsForQuery();
                    var count = rh.Count();
                    if (count == 0)
                    {
                        if (node.Method.Name == nameof(Queryable.Single))
                        {
                            throw new InvalidOperationException("Sequence contains no matching element");
                        }

                        Debug.Assert(node.Method.Name == nameof(Queryable.SingleOrDefault), $"The method {node.Method.Name}  is not supported. We expected {nameof(Queryable.SingleOrDefault)}.");
                        return Expression.Constant(null);
                    }

                    if (count > 1)
                    {
                        throw new InvalidOperationException("Sequence contains more than one matching element");
                    }

                    return GetObjectAtIndex(0, rh, node.Method.Name);
                }

                if (node.Method.Name.StartsWith(nameof(Queryable.Last), StringComparison.OrdinalIgnoreCase))
                {
                    RecurseToWhereOrRunLambda(node);

                    using var rh = MakeResultsForQuery();

                    var count = rh.Count();
                    if (count == 0)
                    {
                        if (node.Method.Name == nameof(Queryable.Last))
                        {
                            throw new InvalidOperationException("Sequence contains no matching element");
                        }

                        Debug.Assert(node.Method.Name == nameof(Queryable.LastOrDefault), $"The method {node.Method.Name}  is not supported. We expected {nameof(Queryable.LastOrDefault)}.");
                        return Expression.Constant(null);
                    }

                    return GetObjectAtIndex(count - 1, rh, node.Method.Name);
                }

                if (node.Method.Name.StartsWith(nameof(Queryable.ElementAt), StringComparison.OrdinalIgnoreCase))
                {
                    Visit(node.Arguments.First());
                    if (!TryExtractConstantValue(node.Arguments.Last(), out object argument) || argument.GetType() != typeof(int))
                    {
                        throw new NotSupportedException($"The method '{node.Method}' has to be invoked with a single integer constant argument or closure variable");
                    }

                    var index = (int)argument;
                    using var rh = MakeResultsForQuery();

                    return GetObjectAtIndex(index, rh, node.Method.Name);
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
                    queryMethod = (q, r, p, v) => q.StringContains(r, p, v, caseSensitive: true);
                }
                else if (IsStringContainsWithComparison(node.Method, out var index))
                {
                    member = node.Arguments[0] as MemberExpression;
                    stringArgumentIndex = index;
                    queryMethod = (q, r, p, v) => q.StringContains(r, p, v, GetComparisonCaseSensitive(node));
                }
                else if (AreMethodsSame(node.Method, Methods.String.StartsWith.Value))
                {
                    queryMethod = (q, r, p, v) => q.StringStartsWith(r, p, v, caseSensitive: true);
                }
                else if (AreMethodsSame(node.Method, Methods.String.StartsWithStringComparison.Value))
                {
                    queryMethod = (q, r, p, v) => q.StringStartsWith(r, p, v, GetComparisonCaseSensitive(node));
                }
                else if (AreMethodsSame(node.Method, Methods.String.EndsWith.Value))
                {
                    queryMethod = (q, r, p, v) => q.StringEndsWith(r, p, v, caseSensitive: true);
                }
                else if (AreMethodsSame(node.Method, Methods.String.EndsWithStringComparison.Value))
                {
                    queryMethod = (q, r, p, v) => q.StringEndsWith(r, p, v, GetComparisonCaseSensitive(node));
                }
                else if (AreMethodsSame(node.Method, Methods.String.IsNullOrEmpty.Value))
                {
                    member = node.Arguments.SingleOrDefault() as MemberExpression;
                    if (member == null)
                    {
                        throw new NotSupportedException($"The method '{node.Method}' has to be invoked with a RealmObjectBase member");
                    }

                    var columnName = GetColumnName(member, node.NodeType);
                    var propertyIndex = _metadata.PropertyIndices[columnName];

                    _coreQueryHandle.GroupBegin();
                    _coreQueryHandle.NullEqual(_realm.SharedRealmHandle, propertyIndex);
                    _coreQueryHandle.Or();
                    _coreQueryHandle.StringEqual(_realm.SharedRealmHandle, propertyIndex, string.Empty, caseSensitive: true);
                    _coreQueryHandle.GroupEnd();
                    return node;
                }
                else if (AreMethodsSame(node.Method, Methods.String.EqualsMethod.Value))
                {
                    queryMethod = (q, r, p, v) => q.StringEqual(r, p, v, caseSensitive: true);
                }
                else if (AreMethodsSame(node.Method, Methods.String.EqualsStringComparison.Value))
                {
                    queryMethod = (q, r, p, v) => q.StringEqual(r, p, v, GetComparisonCaseSensitive(node));
                }
                else if (AreMethodsSame(node.Method, Methods.String.Like.Value))
                {
                    member = node.Arguments[0] as MemberExpression;
                    stringArgumentIndex = 1;
                    if (!TryExtractConstantValue(node.Arguments.Last(), out object caseSensitive) || !(caseSensitive is bool))
                    {
                        throw new NotSupportedException($"The method '{node.Method}' has to be invoked with a string and boolean constant arguments.");
                    }

                    queryMethod = (q, r, p, v) => q.StringLike(r, p, v, (bool)caseSensitive);
                }

                if (queryMethod != null)
                {
                    member ??= node.Object as MemberExpression;

                    if (member == null)
                    {
                        throw new NotSupportedException($"The method '{node.Method}' has to be invoked on a RealmObjectBase member");
                    }

                    var columnName = GetColumnName(member, node.NodeType);
                    var propertyIndex = _metadata.PropertyIndices[columnName];

                    if (!TryExtractConstantValue(node.Arguments[stringArgumentIndex], out object argument) ||
                        (argument != null && argument.GetType() != typeof(string)))
                    {
                        throw new NotSupportedException($"The method '{node.Method}' has to be invoked with a single string constant argument or closure variable");
                    }

                    queryMethod(_coreQueryHandle, _realm.SharedRealmHandle, propertyIndex, (string)argument);
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

        private static bool IsStringContainsWithComparison(MethodInfo method, out int stringArgumentIndex)
        {
            if (AreMethodsSame(method, Methods.String.ContainsStringComparison.Value))
            {
                // This is an extension method, so the string to compare against is at position 1.
                stringArgumentIndex = 1;
                return true;
            }

            // On .NET Core 2.1+ and Xamarin platforms, there's a built-in
            // string.Contains overload that accepts comparison.
            stringArgumentIndex = 0;
            var parameters = method.GetParameters();
            return method.DeclaringType == typeof(string) &&
                method.Name == nameof(string.Contains) &&
                parameters.Length == 2 &&
                parameters[0].ParameterType == typeof(string) &&
                parameters[1].ParameterType == typeof(StringComparison);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    _coreQueryHandle.Not();
                    Visit(node.Operand);  // recurse into richer expression, expect to VisitCombination
                    break;
                default:
                    throw new NotSupportedException($"The unary operator '{node.NodeType}' is not supported");
            }

            return node;
        }

        protected void VisitCombination(BinaryExpression b, Action<QueryHandle> combineWith)
        {
            _coreQueryHandle.GroupBegin();
            Visit(b.Left);
            combineWith(_coreQueryHandle);
            Visit(b.Right);
            _coreQueryHandle.GroupEnd();
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
            if (node.NodeType == ExpressionType.AndAlso)
            {
                // Boolean And with short-circuit
                VisitCombination(node, (qh) => { /* noop -- AND is the default combinator */ });
            }
            else if (node.NodeType == ExpressionType.OrElse)
            {
                // Boolean Or with short-circuit
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
                    throw new NotSupportedException($"The rhs of the binary operator '{rightExpression.NodeType}' should be a constant or closure variable expression. \nUnable to process '{node.Right}'.");
                }

                if (rightValue is RealmObjectBase obj && (!obj.IsManaged || !obj.IsValid))
                {
                    throw new NotSupportedException($"The rhs of the binary operator '{rightExpression.NodeType}' should be a managed RealmObjectBase. \nUnable to process '{node.Right}'.");
                }

                switch (node.NodeType)
                {
                    case ExpressionType.Equal:
                        AddQueryEqual(_coreQueryHandle, leftName, rightValue, memberExpression.Type);
                        break;
                    case ExpressionType.NotEqual:
                        AddQueryNotEqual(_coreQueryHandle, leftName, rightValue, memberExpression.Type);
                        break;
                    case ExpressionType.LessThan:
                        AddQueryLessThan(_coreQueryHandle, leftName, rightValue, memberExpression.Type);
                        break;
                    case ExpressionType.LessThanOrEqual:
                        AddQueryLessThanOrEqual(_coreQueryHandle, leftName, rightValue, memberExpression.Type);
                        break;
                    case ExpressionType.GreaterThan:
                        AddQueryGreaterThan(_coreQueryHandle, leftName, rightValue, memberExpression.Type);
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        AddQueryGreaterThanOrEqual(_coreQueryHandle, leftName, rightValue, memberExpression.Type);
                        break;
                    default:
                        throw new NotSupportedException($"The binary operator '{node.NodeType}' is not supported");
                }
            }

            return node;
        }

        private enum OutOfRangeBehavior
        {
            Throw,
            Ignore,
            Convert
        }

        private Expression GetObjectAtIndex(int index, ResultsHandle rh, string methodName)
        {
            try
            {
                var val = rh.GetValueAtIndex(index, _metadata, _realm);
                return Expression.Constant(val.AsRealmObject());
            }
            catch (ArgumentOutOfRangeException ex)
            {
                if (methodName.EndsWith("OrDefault", StringComparison.OrdinalIgnoreCase))
                {
                    // For First/Last/Single/ElemetAtOrDefault - ignore
                    return Expression.Constant(null);
                }

                if (methodName == nameof(Queryable.ElementAt))
                {
                    // ElementAt should rethrow the ArgumentOutOfRangeException.
                    throw;
                }

                // All the rest should throw an InvalidOperationException.
                throw new InvalidOperationException("Sequence contains no matching element", ex);
            }
        }

        private void AddQueryEqual(QueryHandle queryHandle, string columnName, object value, Type columnType)
        {
            var propertyIndex = _metadata.PropertyIndices[columnName];

            switch (value)
            {
                case null:
                    queryHandle.NullEqual(_realm.SharedRealmHandle, propertyIndex);
                    break;
                case string stringValue:
                    queryHandle.StringEqual(_realm.SharedRealmHandle, propertyIndex, stringValue, caseSensitive: true);
                    break;
                default:
                    // The other types aren't handled by the switch because of potential compiler applied conversions
                    AddQueryForConvertibleTypes(_realm.SharedRealmHandle, propertyIndex, value, columnType, queryHandle.ValueEqual);
                    break;
            }
        }

        private void AddQueryNotEqual(QueryHandle queryHandle, string columnName, object value, Type columnType)
        {
            var propertyIndex = _metadata.PropertyIndices[columnName];
            switch (value)
            {
                case null:
                    queryHandle.NullNotEqual(_realm.SharedRealmHandle, propertyIndex);
                    break;
                case string stringValue:
                    queryHandle.StringNotEqual(_realm.SharedRealmHandle, propertyIndex, stringValue, caseSensitive: true);
                    break;
                default:
                    // The other types aren't handled by the switch because of potential compiler applied conversions
                    AddQueryForConvertibleTypes(_realm.SharedRealmHandle, propertyIndex, value, columnType, queryHandle.ValueNotEqual);
                    break;
            }
        }

        private void AddQueryLessThan(QueryHandle queryHandle, string columnName, object value, Type columnType)
        {
            var propertyIndex = _metadata.PropertyIndices[columnName];
            switch (value)
            {
                case string _:
                case bool _:
                    throw new Exception($"Unsupported type {value.GetType().Name}");
                default:
                    // The other types aren't handled by the switch because of potential compiler applied conversions
                    AddQueryForConvertibleTypes(_realm.SharedRealmHandle, propertyIndex, value, columnType, queryHandle.ValueLess);
                    break;
            }
        }

        private void AddQueryLessThanOrEqual(QueryHandle queryHandle, string columnName, object value, Type columnType)
        {
            var propertyIndex = _metadata.PropertyIndices[columnName];
            switch (value)
            {
                case string _:
                case bool _:
                    throw new Exception($"Unsupported type {value.GetType().Name}");
                default:
                    // The other types aren't handled by the switch because of potential compiler applied conversions
                    AddQueryForConvertibleTypes(_realm.SharedRealmHandle, propertyIndex, value, columnType, queryHandle.ValueLessEqual);
                    break;
            }
        }

        private void AddQueryGreaterThan(QueryHandle queryHandle, string columnName, object value, Type columnType)
        {
            var propertyIndex = _metadata.PropertyIndices[columnName];
            switch (value)
            {
                case string _:
                case bool _:
                    throw new Exception($"Unsupported type {value.GetType().Name}");
                default:
                    // The other types aren't handled by the switch because of potential compiler applied conversions
                    AddQueryForConvertibleTypes(_realm.SharedRealmHandle, propertyIndex, value, columnType, queryHandle.ValueGreater);
                    break;
            }
        }

        private void AddQueryGreaterThanOrEqual(QueryHandle queryHandle, string columnName, object value, Type columnType)
        {
            var propertyIndex = _metadata.PropertyIndices[columnName];
            switch (value)
            {
                case string _:
                case bool _:
                    throw new Exception($"Unsupported type {value.GetType().Name}");
                default:
                    // The other types aren't handled by the switch because of potential compiler applied conversions
                    AddQueryForConvertibleTypes(_realm.SharedRealmHandle, propertyIndex, value, columnType, queryHandle.ValueGreaterEqual);
                    break;
            }
        }

        private delegate void QueryAction(SharedRealmHandle realm, IntPtr propertyIndex, in RealmValue value);

        private static void AddQueryForConvertibleTypes(SharedRealmHandle realm, IntPtr propertyIndex, object value, Type columnType, QueryAction action)
        {
            if (columnType.IsConstructedGenericType && columnType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                columnType = columnType.GetGenericArguments().Single();
            }

            if (columnType == typeof(byte) ||
                columnType == typeof(short) ||
                columnType == typeof(char) ||
                columnType == typeof(int) ||
                columnType == typeof(long) ||
                columnType == typeof(RealmInteger<byte>) ||
                columnType == typeof(RealmInteger<short>) ||
                columnType == typeof(RealmInteger<int>) ||
                columnType == typeof(RealmInteger<long>))
            {
                action(realm, propertyIndex, (long)Convert.ChangeType(value, typeof(long)));
            }
            else if (columnType == typeof(float))
            {
                action(realm, propertyIndex, (float)Convert.ChangeType(value, typeof(float)));
            }
            else if (columnType == typeof(double))
            {
                action(realm, propertyIndex, (double)Convert.ChangeType(value, typeof(double)));
            }
            else if (columnType == typeof(Decimal128))
            {
                // This is needed, because Convert.ChangeType will throw if value is Decimal128
                if (!(value is Decimal128 decimalValue))
                {
                    decimalValue = (Decimal128)Convert.ChangeType(value, typeof(Decimal128));
                }

                action(realm, propertyIndex, decimalValue);
            }
            else if (columnType == typeof(decimal))
            {
                action(realm, propertyIndex, (decimal)Convert.ChangeType(value, typeof(decimal)));
            }
            else if (columnType == typeof(ObjectId))
            {
                action(realm, propertyIndex, (ObjectId)Convert.ChangeType(value, typeof(ObjectId)));
            }
            else if (columnType == typeof(Guid))
            {
                action(realm, propertyIndex, (Guid)Convert.ChangeType(value, typeof(Guid)));
            }
            else if (columnType == typeof(byte[]))
            {
                action(realm, propertyIndex, (byte[])value);
            }
            else if (columnType == typeof(bool))
            {
                action(realm, propertyIndex, (bool)value);
            }
            else if (columnType == typeof(DateTimeOffset))
            {
                action(realm, propertyIndex, (DateTimeOffset)value);
            }
            else if (typeof(RealmObjectBase).IsAssignableFrom(columnType))
            {
                action(realm, propertyIndex, (RealmObjectBase)value);
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
            return comparison switch
            {
                StringComparison.Ordinal => true,
                StringComparison.OrdinalIgnoreCase => false,
                _ => throw new NotSupportedException($"The comparison {comparison} is not yet supported. Use {StringComparison.Ordinal} or {StringComparison.OrdinalIgnoreCase}."),
            };
        }

        private string GetColumnName(MemberExpression memberExpression, ExpressionType? parentType = null)
        {
            var name = memberExpression?.Member.GetMappedOrOriginalName();

            if (parentType.HasValue)
            {
                if (name == null ||
                    memberExpression.Expression.NodeType != ExpressionType.Parameter ||
                    !(memberExpression.Member is PropertyInfo) ||
                    !_metadata.Schema.TryFindProperty(name, out var property) ||
                    property.Type.HasFlag(PropertyType.Array) ||
                    property.Type.HasFlag(PropertyType.Set))
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
                if (_coreQueryHandle != null)
                {
                    throw new Exception("We already have a table...");
                }

                _coreQueryHandle = results.GetQuery();
                _sortDescriptor = results.GetSortDescriptor();
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
                    AddQueryEqual(_coreQueryHandle, leftName, rhs, node.Type);
                }

                return node;
            }

            throw new NotSupportedException($"The member '{node.Member.Name}' is not supported");
        }

        public ResultsHandle MakeResultsForQuery()
        {
            return _coreQueryHandle.CreateResults(_realm.SharedRealmHandle, _sortDescriptor);
        }
    }
}