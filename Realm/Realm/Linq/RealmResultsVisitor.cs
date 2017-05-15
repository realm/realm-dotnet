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
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Realms.Native;
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

            var propertyChain = TraverseSort(body).Select(n =>
            {
                var metadata = _realm.Metadata[n.Item1.Name];
                return metadata.PropertyIndices[n.Item2];
            });

            OptionalSortDescriptorBuilder.AddClause(propertyChain, ascending);
        }

        private static IEnumerable<Tuple<Type, string>> TraverseSort(MemberExpression expression)
        {
            var chain = new List<Tuple<Type, string>>();

            while (expression != null)
            {
                chain.Add(Tuple.Create(expression.Member.DeclaringType, expression.Member.Name));
                expression = expression.Expression as MemberExpression;
            }

            chain.Reverse();

            return chain;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable))
            {
                if (m.Method.Name == nameof(Queryable.Where))
                {
                    Visit(m.Arguments[0]);
                    var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                    Visit(lambda.Body);
                    return m;
                }

                if (m.Method.Name == nameof(Queryable.OrderBy))
                {
                    Visit(m.Arguments[0]);
                    AddSort((LambdaExpression)StripQuotes(m.Arguments[1]), true, true);
                    return m;
                }

                if (m.Method.Name == nameof(Queryable.OrderByDescending))
                {
                    Visit(m.Arguments[0]);
                    AddSort((LambdaExpression)StripQuotes(m.Arguments[1]), true, false);
                    return m;
                }

                if (m.Method.Name == nameof(Queryable.ThenBy))
                {
                    Visit(m.Arguments[0]);
                    AddSort((LambdaExpression)StripQuotes(m.Arguments[1]), false, true);
                    return m;
                }

                if (m.Method.Name == nameof(Queryable.ThenByDescending))
                {
                    Visit(m.Arguments[0]);
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
                    Visit(m.Arguments.First());
                    object argument;
                    if (!TryExtractConstantValue(m.Arguments.Last(), out argument) || argument.GetType() != typeof(int))
                    {
                        throw new NotSupportedException($"The method '{m.Method}' has to be invoked with a single integer constant argument or closure variable");
                    }

                    IntPtr objectPtr;
                    var index = (int)argument;
                    if (OptionalSortDescriptorBuilder == null)
                    {
                        objectPtr = CoreQueryHandle.FindDirect(_realm.SharedRealmHandle, (IntPtr)index);
                    }
                    else
                    {
                        using (var rh = _realm.MakeResultsForQuery(CoreQueryHandle, OptionalSortDescriptorBuilder))
                        {
                            objectPtr = rh.GetObjectAtIndex(index);
                        }
                    }

                    if (objectPtr != IntPtr.Zero)
                    {
                        var objectHandle = Realm.CreateObjectHandle(objectPtr, _realm.SharedRealmHandle);
                        return Expression.Constant(_realm.MakeObject(_metadata, objectHandle));
                    }

                    if (m.Method.Name == nameof(Queryable.ElementAt))
                    {
                        throw new ArgumentOutOfRangeException();
                    }

                    Debug.Assert(m.Method.Name == nameof(Queryable.ElementAtOrDefault), $"The method {m.Method.Name}  is not supported. We expected {nameof(Queryable.ElementAtOrDefault)}.");
                    return Expression.Constant(null);
                }
            }

            if (m.Method.DeclaringType == typeof(string) ||
                m.Method.DeclaringType == typeof(StringExtensions))
            {
                QueryHandle.Operation<string> queryMethod = null;

                // For extension methods, member should be m.Arguments[0] as MemberExpression;
                MemberExpression member = null;

                // For extension methods, that should be 1
                var stringArgumentIndex = 0;

                if (AreMethodsSame(m.Method, Methods.String.Contains.Value))
                {
                    queryMethod = (q, c, v) => q.StringContains(c, v, caseSensitive: true);
                }
                else if (AreMethodsSame(m.Method, Methods.String.ContainsStringComparison.Value))
                {
                    member = m.Arguments[0] as MemberExpression;
                    stringArgumentIndex = 1;
                    queryMethod = (q, c, v) => q.StringContains(c, v, GetComparisonCaseSensitive(m));
                }
                else if (AreMethodsSame(m.Method, Methods.String.StartsWith.Value))
                {
                    queryMethod = (q, c, v) => q.StringStartsWith(c, v, caseSensitive: true);
                }
                else if (AreMethodsSame(m.Method, Methods.String.StartsWithStringComparison.Value))
                {
                    queryMethod = (q, c, v) => q.StringStartsWith(c, v, GetComparisonCaseSensitive(m));
                }
                else if (AreMethodsSame(m.Method, Methods.String.EndsWith.Value))
                {
                    queryMethod = (q, c, v) => q.StringEndsWith(c, v, caseSensitive: true);
                }
                else if (AreMethodsSame(m.Method, Methods.String.EndsWithStringComparison.Value))
                {
                    queryMethod = (q, c, v) => q.StringEndsWith(c, v, GetComparisonCaseSensitive(m));
                }
                else if (AreMethodsSame(m.Method, Methods.String.IsNullOrEmpty.Value))
                {
                    member = m.Arguments.SingleOrDefault() as MemberExpression;
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
                else if (AreMethodsSame(m.Method, Methods.String.EqualsMethod.Value))
                {
                    queryMethod = (q, c, v) => q.StringEqual(c, v, caseSensitive: true);
                }
                else if (AreMethodsSame(m.Method, Methods.String.EqualsStringComparison.Value))
                {
                    queryMethod = (q, c, v) => q.StringEqual(c, v, GetComparisonCaseSensitive(m));
                }
                else if (AreMethodsSame(m.Method, Methods.String.Like.Value))
                {
                    member = m.Arguments[0] as MemberExpression;
                    stringArgumentIndex = 1;
                    object caseSensitive;
                    if (!TryExtractConstantValue(m.Arguments.Last(), out caseSensitive) || !(caseSensitive is bool))
                    {
                        throw new NotSupportedException($"The method '{m.Method}' has to be invoked with a string and boolean constant arguments.");
                    }

                    queryMethod = (q, c, v) => q.StringLike(c, v, (bool)caseSensitive);
                }

                if (queryMethod != null)
                {
                    member = member ?? m.Object as MemberExpression;

                    if (member == null)
                    {
                        throw new NotSupportedException($"The method '{m.Method}' has to be invoked on a RealmObject member");
                    }

                    var columnIndex = CoreQueryHandle.GetColumnIndex(member.Member.Name);

                    object argument;
                    if (!TryExtractConstantValue(m.Arguments[stringArgumentIndex], out argument) || 
                        (argument != null && argument.GetType() != typeof(string)))
                    {
                        throw new NotSupportedException($"The method '{m.Method}' has to be invoked with a single string constant argument or closure variable");
                    }

                    queryMethod(CoreQueryHandle, columnIndex, (string)argument);
                    return m;
                }
            }

            throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");
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

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    CoreQueryHandle.Not();
                    Visit(u.Operand);  // recurse into richer expression, expect to VisitCombination
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

        protected override Expression VisitBinary(BinaryExpression b)
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

                var leftName = memberExpression?.Member.GetCustomAttribute<MapToAttribute>()?.Mapping ?? 
                               memberExpression?.Member.Name;

                if (leftName == null ||
                    !(memberExpression.Member is PropertyInfo) ||
                    !_metadata.Schema.PropertyNames.Contains(leftName))
                {
                    throw new NotSupportedException($"The left-hand side of the {b.NodeType} operator must be a direct access to a persisted property in Realm.\nUnable to process '{b.Left}'.");
                }

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
                        queryHandle.BinaryEqual(columnIndex, (IntPtr)bufferPtr, (IntPtr)buffer.LongCount());
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
                        queryHandle.BinaryNotEqual(columnIndex, (IntPtr)bufferPtr, (IntPtr)buffer.LongCount());
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
            if (!TryExtractConstantValue(m.Arguments.Last(), out argument) || !(argument is StringComparison))
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
        protected override Expression VisitConstant(ConstantExpression c)
        {
            var results = c.Value as IQueryableCollection;
            if (results != null)
            {
                // assume constant nodes w/ IQueryables are table references
                if (CoreQueryHandle != null)
                {
                    throw new Exception("We already have a table...");
                }

                CoreQueryHandle = results.CreateQuery();
            }
            else if (c.Value?.GetType() == typeof(object))
            {
                throw new NotSupportedException($"The constant for '{c.Value}' is not supported");
            }

            return c;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
            {
                if (node.Type == typeof(bool))
                {
                    object rhs = true;  // box value
                    var leftName = node.Member.Name;
                    AddQueryEqual(CoreQueryHandle, leftName, rhs);
                }

                return node;
            }

            throw new NotSupportedException($"The member '{node.Member.Name}' is not supported");
        }
    }
}