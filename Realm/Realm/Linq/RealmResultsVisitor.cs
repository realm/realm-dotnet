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
using Realms.Helpers;
using Realms.Schema;
using LazyMethod = System.Lazy<System.Reflection.MethodInfo>;

namespace Realms
{
    internal class RealmResultsVisitor : ExpressionVisitor
    {
        private readonly Realm _realm;
        private readonly Metadata _metadata;

        // These are set when we recurse down to VisitConstant
        private QueryHandle _coreQueryHandle = null!;
        private SortDescriptorHandle _sortDescriptor = null!;

        private static class Methods
        {
            internal static LazyMethod Capture<T>(Expression<Action<T>> lambda)
            {
                return new LazyMethod(() =>
                {
                    var method = ((MethodCallExpression)lambda.Body).Method;
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

#pragma warning disable CS0618 // Type or member is obsolete
                internal static readonly LazyMethod InstanceContainsStringComparison = Capture<string>(s => s.Contains(string.Empty, StringComparison.Ordinal));
#pragma warning restore CS0618 // Type or member is obsolete

                internal static readonly LazyMethod ContainsStringComparison = Capture<string>(s => QueryMethods.Contains(s, string.Empty, StringComparison.Ordinal));

#pragma warning disable CS0618 // Type or member is obsolete
                internal static readonly LazyMethod LegacyLike = Capture<string>(s => s.Like(string.Empty, true));
#pragma warning restore CS0618 // Type or member is obsolete

                internal static readonly LazyMethod Like = Capture<string>(s => QueryMethods.Like(s, string.Empty, true));

                internal static readonly LazyMethod FullTextSearch = Capture<string>(s => QueryMethods.FullTextSearch(s, string.Empty));

                internal static readonly LazyMethod StartsWith = Capture<string>(s => s.StartsWith(string.Empty));

                internal static readonly LazyMethod StartsWithStringComparison = Capture<string>(s => s.StartsWith(string.Empty, StringComparison.Ordinal));

                internal static readonly LazyMethod EndsWith = Capture<string>(s => s.EndsWith(string.Empty));

                internal static readonly LazyMethod EndsWithStringComparison = Capture<string>(s => s.EndsWith(string.Empty, StringComparison.Ordinal));

                internal static readonly LazyMethod IsNullOrEmpty = Capture<string>(s => string.IsNullOrEmpty(s));

                internal static readonly LazyMethod EqualsMethod = Capture<string>(s => s.Equals(string.Empty));

                internal static readonly LazyMethod EqualsStringComparison = Capture<string>(s => s.Equals(string.Empty, StringComparison.Ordinal));
            }

            internal static class EmbeddedObject
            {
                internal static readonly LazyMethod GeoWithin = Capture<IEmbeddedObject>(o => QueryMethods.GeoWithin(o, null!));
            }
        }

        internal RealmResultsVisitor(Realm realm, Metadata metadata)
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

        private void AddSort(LambdaExpression lambda, bool isAscending, bool isReplacing)
        {
            if (lambda.Body is not MemberExpression body)
            {
                throw new NotSupportedException($"The expression {lambda} cannot be used in an Order clause");
            }

            var propertyChain = TraverseSort(body);
            _sortDescriptor.AddClause(_metadata.TableKey, propertyChain, isAscending, isReplacing);
        }

        private static bool IsSortClause(string methodName, out bool isAscending, out bool isReplacing)
        {
            switch (methodName)
            {
                case nameof(Queryable.OrderBy):
                    isAscending = true;
                    isReplacing = true;
                    return true;
                case nameof(Queryable.ThenBy):
                    isAscending = true;
                    isReplacing = false;
                    return true;
                case nameof(Queryable.OrderByDescending):
                    isAscending = false;
                    isReplacing = true;
                    return true;
                case nameof(Queryable.ThenByDescending):
                    isAscending = false;
                    isReplacing = false;
                    return true;
                default:
                    isAscending = false;
                    isReplacing = false;
                    return false;
            }
        }

        private IntPtr[] TraverseSort(MemberExpression? expression)
        {
            var chain = new List<IntPtr>();

            while (expression != null)
            {
                var type = expression.Expression!.Type;
                var typeName = type.GetMappedOrOriginalName();
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

                if (IsSortClause(node.Method.Name, out var isAscending, out var isReplacing))
                {
                    Visit(node.Arguments[0]);
                    AddSort((LambdaExpression)StripQuotes(node.Arguments[1]), isAscending, isReplacing);
                    return node;
                }

                if (node.Method.Name == nameof(Queryable.Count))
                {
                    RecurseToWhereOrRunLambda(node);
                    var foundCount = _coreQueryHandle.Count(_sortDescriptor);
                    return Expression.Constant(foundCount);
                }

                if (node.Method.Name == nameof(Queryable.Any))
                {
                    RecurseToWhereOrRunLambda(node);
                    return Expression.Constant(_coreQueryHandle.Count(_sortDescriptor) > 0);
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
                    if (!TryExtractConstantValue(node.Arguments.Last(), out var argument) || argument?.GetType() != typeof(int))
                    {
                        throw new NotSupportedException($"The method '{node.Method}' has to be invoked with a single integer constant argument or closure variable");
                    }

                    var index = (int)argument;
                    using var rh = MakeResultsForQuery();

                    return GetObjectAtIndex(index, rh, node.Method.Name);
                }
            }

            if (node.Method.DeclaringType == typeof(QueryMethods))
            {
                if (AreMethodsSame(node.Method, Methods.EmbeddedObject.GeoWithin.Value))
                {
                    var member = (MemberExpression)node.Arguments[0];
                    var columnName = GetColumnName(member, node.NodeType);
                    var propertyIndex = _metadata.PropertyIndices[columnName];
                    if (!TryExtractConstantValue(node.Arguments[1], out var argument) ||
                        argument is not GeoShapeBase geoShape)
                    {
                        throw new NotSupportedException($"The method '{node.Method}' has to be invoked with a single GeoShapeBase argument or closure variable");
                    }

                    _coreQueryHandle.GeoWithin(_realm.SharedRealmHandle, propertyIndex, geoShape);

                    return node;
                }
            }

            if (node.Method.DeclaringType == typeof(string) ||
                node.Method.DeclaringType == typeof(StringExtensions) ||
                node.Method.DeclaringType == typeof(QueryMethods))
            {
                QueryHandle.Operation<string?>? queryMethod = null;

                // For extension methods, member should be m.Arguments[0] as MemberExpression;
                MemberExpression? member = null;

                // For extension methods, that should be 1
                var stringArgumentIndex = 0;

                if (AreMethodsSame(node.Method, Methods.String.Contains.Value))
                {
                    queryMethod = (q, r, p, v) => q.StringContains(r, p, v, caseSensitive: true);
                }
                else if (IsStringContainsWithComparison(node.Method, out var index))
                {
                    if (index > 0)
                    {
                        // If index is > 0, we're an extension method where the 0th argument is the property name
                        member = node.Arguments[0] as MemberExpression;
                    }

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
                else if (AreMethodsSame(node.Method, Methods.String.Like.Value) || AreMethodsSame(node.Method, Methods.String.LegacyLike.Value))
                {
                    member = node.Arguments[0] as MemberExpression;
                    stringArgumentIndex = 1;
                    if (!TryExtractConstantValue(node.Arguments.Last(), out var caseSensitive) || caseSensitive is not bool)
                    {
                        throw new NotSupportedException($"The method '{node.Method}' has to be invoked with a string and boolean constant arguments.");
                    }

                    queryMethod = (q, r, p, v) => q.StringLike(r, p, v, (bool)caseSensitive);
                }
                else if (AreMethodsSame(node.Method, Methods.String.FullTextSearch.Value))
                {
                    member = node.Arguments[0] as MemberExpression;
                    stringArgumentIndex = 1;

                    queryMethod = (q, r, p, v) =>
                    {
                        if (v == null)
                        {
                            throw new ArgumentNullException("terms", "Cannot perform a Full-Text search against null string");
                        }

                        q.StringFTS(r, p, v);
                    };
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

                    if (!TryExtractConstantValue(node.Arguments[stringArgumentIndex], out var argument) ||
                        (argument != null && argument.GetType() != typeof(string)))
                    {
                        throw new NotSupportedException($"The method '{node.Method}' has to be invoked with a single string constant argument or closure variable");
                    }

                    queryMethod(_coreQueryHandle, _realm.SharedRealmHandle, propertyIndex, (string?)argument);
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
#if !NETCOREAPP2_1_OR_GREATER
            if (AreMethodsSame(method, Methods.String.InstanceContainsStringComparison.Value))
            {
                // This is an extension method, so the string to compare against is at position 1.
                stringArgumentIndex = 1;
                return true;
            }
#endif

            if (AreMethodsSame(method, Methods.String.ContainsStringComparison.Value))
            {
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

        internal static bool TryExtractConstantValue(Expression expr, out object? value)
        {
            if (expr.NodeType == ExpressionType.Convert)
            {
                var operand = ((UnaryExpression)expr).Operand;
                return TryExtractConstantValue(operand, out value);
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

                if (TryExtractConstantValue(memberAccess.Expression!, out var targetObject))
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

                if (TryExtractConstantValue(memberAccess.Expression!, out var targetObject))
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

                if (memberExpression == null)
                {
                    throw new NotSupportedException($"The lhs of the binary operator '{leftExpression.NodeType}' should be a member expression accessing a property on a managed RealmObjectBase.\nUnable to process '{node.Left}'.");
                }

                if (!TryExtractConstantValue(node.Right, out var rightValue))
                {
                    throw new NotSupportedException($"The rhs of the binary operator '{rightExpression.NodeType}' should be a constant or closure variable expression.\nUnable to process '{node.Right}'.");
                }

                if (rightValue is IRealmObjectBase obj && (!obj.IsManaged || !obj.IsValid))
                {
                    throw new NotSupportedException($"The rhs of the binary operator '{rightExpression.NodeType}' should be a managed RealmObjectBase.\nUnable to process '{node.Right}'.");
                }

                string? leftName = null;

                if (IsRealmValueTypeExpression(memberExpression, out leftName))
                {
                    if (node.NodeType != ExpressionType.Equal && node.NodeType != ExpressionType.NotEqual)
                    {
                        throw new NotSupportedException($"Only expressions of type Equal and NotEqual can be used with RealmValueType.");
                    }

                    if (rightValue is int intValue)
                    {
                        rightValue = (RealmValueType)intValue;
                    }
                }
                else
                {
                    leftName = GetColumnName(memberExpression, node.NodeType);
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
                var val = rh.GetValueAtIndex(index, _realm);
                return Expression.Constant(val.AsIRealmObject());
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

        private void AddQueryEqual(QueryHandle queryHandle, string columnName, object? value, Type columnType)
        {
            var propertyIndex = _metadata.PropertyIndices[columnName];

            switch (value)
            {
                case null:
                case RealmValue rv when rv.Type == RealmValueType.Null:
                    queryHandle.NullEqual(_realm.SharedRealmHandle, propertyIndex);
                    break;
                case string stringValue:
                    queryHandle.StringEqual(_realm.SharedRealmHandle, propertyIndex, stringValue, caseSensitive: true);
                    break;
                case RealmValueType realmValueType:
                    queryHandle.RealmValueTypeEqual(_realm.SharedRealmHandle, propertyIndex, realmValueType);
                    break;
                default:
                    // The other types aren't handled by the switch because of potential compiler applied conversions
                    AddQueryForConvertibleTypes(_realm.SharedRealmHandle, propertyIndex, value, columnType, queryHandle.ValueEqual);
                    break;
            }
        }

        private void AddQueryNotEqual(QueryHandle queryHandle, string columnName, object? value, Type columnType)
        {
            var propertyIndex = _metadata.PropertyIndices[columnName];
            switch (value)
            {
                case null:
                case RealmValue rv when rv.Type == RealmValueType.Null:
                    queryHandle.NullNotEqual(_realm.SharedRealmHandle, propertyIndex);
                    break;
                case string stringValue:
                    queryHandle.StringNotEqual(_realm.SharedRealmHandle, propertyIndex, stringValue, caseSensitive: true);
                    break;
                case RealmValueType realmValueType:
                    queryHandle.RealmValueTypeNotEqual(_realm.SharedRealmHandle, propertyIndex, realmValueType);
                    break;
                default:
                    // The other types aren't handled by the switch because of potential compiler applied conversions
                    AddQueryForConvertibleTypes(_realm.SharedRealmHandle, propertyIndex, value, columnType, queryHandle.ValueNotEqual);
                    break;
            }
        }

        private void AddQueryLessThan(QueryHandle queryHandle, string columnName, object? value, Type columnType)
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

        private void AddQueryLessThanOrEqual(QueryHandle queryHandle, string columnName, object? value, Type columnType)
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

        private void AddQueryGreaterThan(QueryHandle queryHandle, string columnName, object? value, Type columnType)
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

        private void AddQueryGreaterThanOrEqual(QueryHandle queryHandle, string columnName, object? value, Type columnType)
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

        private static void AddQueryForConvertibleTypes(SharedRealmHandle realm, IntPtr propertyIndex, object? value, Type columnType, QueryAction action)
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
                action(realm, propertyIndex, Operator.Convert<long>(value));
            }
            else if (columnType == typeof(float))
            {
                action(realm, propertyIndex, Operator.Convert<float>(value));
            }
            else if (columnType == typeof(double))
            {
                action(realm, propertyIndex, Operator.Convert<double>(value));
            }
            else if (columnType == typeof(Decimal128))
            {
                action(realm, propertyIndex, Operator.Convert<Decimal128>(value));
            }
            else if (columnType == typeof(decimal))
            {
                action(realm, propertyIndex, Operator.Convert<decimal>(value));
            }
            else if (columnType == typeof(ObjectId))
            {
                action(realm, propertyIndex, Operator.Convert<ObjectId>(value));
            }
            else if (columnType == typeof(Guid))
            {
                action(realm, propertyIndex, Operator.Convert<Guid>(value));
            }
            else if (columnType == typeof(byte[]))
            {
                action(realm, propertyIndex, Operator.Convert<byte[]>(value));
            }
            else if (columnType == typeof(bool))
            {
                action(realm, propertyIndex, Operator.Convert<bool>(value));
            }
            else if (columnType == typeof(DateTimeOffset))
            {
                action(realm, propertyIndex, Operator.Convert<DateTimeOffset>(value));
            }
            else if (typeof(IRealmObjectBase).IsAssignableFrom(columnType))
            {
                var obj = Operator.Convert<IRealmObjectBase>(value);
                action(realm, propertyIndex, obj == null ? RealmValue.Null : RealmValue.Object(obj));
            }
            else if (columnType == typeof(RealmValue))
            {
                action(realm, propertyIndex, Operator.Convert<RealmValue>(value));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static bool GetComparisonCaseSensitive(MethodCallExpression m)
        {
            if (!TryExtractConstantValue(m.Arguments.Last(), out var argument) || argument is not StringComparison)
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
                    memberExpression!.Expression!.NodeType != ExpressionType.Parameter ||
                    memberExpression.Member is not PropertyInfo ||
                    !_metadata.Schema.TryFindProperty(name, out var property) ||
                    property.Type.HasFlag(PropertyType.Array) ||
                    property.Type.HasFlag(PropertyType.Set))
                {
                    throw new NotSupportedException($"The left-hand side of the {parentType} operator must be a direct access to a persisted property in Realm.\nUnable to process '{memberExpression}'.");
                }
            }

            return name!;
        }

        private bool IsRealmValueTypeExpression(MemberExpression memberExpression, [MaybeNullWhen(false)] out string leftName)
        {
            leftName = null;

            if (memberExpression.Type != typeof(RealmValueType))
            {
                return false;
            }

            if (memberExpression.Expression is MemberExpression innerExpression)
            {
                leftName = GetColumnName(innerExpression, memberExpression.NodeType);
                return innerExpression.Type == typeof(RealmValue);
            }

            return false;
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
