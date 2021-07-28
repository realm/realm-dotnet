using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Realms.Schema;
using LazyMethod = System.Lazy<System.Reflection.MethodInfo>;

namespace Realms
{
    internal class WhereClauseVisitor : ExpressionVisitor
    {
        private readonly RealmObjectBase.Metadata _metadata;

        //private List<WhereClauseProperties> _whereList = new List<WhereClauseProperties>();

        private WhereClause _whereClause;

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

        public WhereClauseVisitor(RealmObjectBase.Metadata metadata)
        {
            _metadata = metadata;
            _whereClause = new WhereClause();
        }

        public WhereClause VisitWhere(LambdaExpression whereClause)
        {
            _whereClause.ExpNode = ParseExpression(whereClause.Body);
            var json = JsonConvert.SerializeObject(_whereClause, formatting: Formatting.Indented);
            Visit(whereClause.Body);
            return _whereClause;
        }

        private ExpressionNode ParseExpression(Expression exp)
        {
            if (exp is BinaryExpression be)
            {
                if (be.NodeType == ExpressionType.AndAlso)
                {
                    var andNode = new AndNode();
                    andNode.Left = ParseExpression(be.Left);
                    andNode.Right = ParseExpression(be.Right);
                    return andNode;
                }

                if (be.NodeType == ExpressionType.OrElse)
                {
                    var orNode = new OrNode();
                    orNode.Left = ParseExpression(be.Left);
                    orNode.Right = ParseExpression(be.Right);
                    return orNode;
                }

                ComparisonNode comparisonNode;
                switch (be.NodeType)
                {
                    case ExpressionType.Equal:
                        comparisonNode = new EqualityNode();
                        break;
                    case ExpressionType.NotEqual:
                        comparisonNode = new NotEqualNode();
                        break;
                    case ExpressionType.LessThan:
                        comparisonNode = new LtNode();
                        break;
                    case ExpressionType.LessThanOrEqual:
                        comparisonNode = new LteNode();
                        break;
                    case ExpressionType.GreaterThan:
                        comparisonNode = new GtNode();
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        comparisonNode = new GteNode();
                        break;
                    default:
                        throw new NotSupportedException($"The binary operator '{be.NodeType}' is not supported");
                }

                if (be.Left is MemberExpression me)
                {
                    if (me.Expression != null && me.Expression.NodeType == ExpressionType.Parameter)
                    {
                        var leftName = GetColumnName(me, me.NodeType);

                        comparisonNode.Left.Value = leftName;
                        comparisonNode.Left.Kind = "property";
                        // TODO: Possible with switch statment in our current .NET version?
                        if (me.Type == typeof(float))
                        {
                            comparisonNode.Left.Type = "float";
                        }
                        else if (me.Type == typeof(long))
                        {
                            comparisonNode.Left.Type = "long";
                        }
                        else if (me.Type == typeof(double))
                        {
                            comparisonNode.Left.Type = "double";
                        }
                    }
                    else
                    {
                        throw new NotSupportedException(me + " is null or not a supported type.");
                    }
                }

                if (be.Right is ConstantExpression co)
                {
                    comparisonNode.Right.Value = co.Value;
                    comparisonNode.Right.Kind = "constant";
                    var valueType = co.Value.GetType();

                    if (valueType == typeof(float))
                    {
                        comparisonNode.Right.Type = "float";
                    }
                    else if (valueType == typeof(long))
                    {
                        comparisonNode.Right.Type = "long";
                    }
                    else if (valueType == typeof(double))
                    {
                        comparisonNode.Right.Type = "double";
                    }
                    else
                    {
                        throw new NotSupportedException(valueType + "is not a supported type.");
                    }
                    // Add all types
                }

                return comparisonNode;
            }
            else if (exp is MemberExpression)
            {
                if (exp.Type == typeof(bool))
                {
                    var boolNode = new BooleanPropertyNode();
                    var propName = ((MemberExpression)exp).Member.GetMappedOrOriginalName();
                    boolNode.Property = propName;
                    return boolNode;
                }
                throw new NotSupportedException($"The member '{((MemberExpression)exp).Member.Name}' is not supported");
            }
            else if (exp is MethodCallExpression)
            {
                if (((MethodCallExpression)exp).Method.DeclaringType == typeof(string) ||
                    ((MethodCallExpression)exp).Method.DeclaringType == typeof(StringExtensions))
                {
                    var expMethod = exp as MethodCallExpression;

                    ComparisonNode stringComparisonNode;

                    if (AreMethodsSame(expMethod.Method, Methods.String.Contains.Value))
                    {
                        Console.WriteLine("Test");
                    }
                    //else if (IsStringContainsWithComparison(node.Method, out var index))
                    //{
                    //    Console.WriteLine("Test");
                    //}
                    else if (AreMethodsSame(expMethod.Method, Methods.String.StartsWith.Value))
                    {
                        stringComparisonNode = new StartsWithNode();
                        if (expMethod.Arguments[0] is ConstantExpression c)
                        {
                            //stringComparisonNode.ValueNode.Value = c.Value;
                            //stringComparisonNode.ValueNode.Type = "string";
                        }

                        if (((MethodCallExpression)exp).Object is MemberExpression me)
                        {
                            if (me.Expression != null && me.Expression.NodeType == ExpressionType.Parameter)
                            {
                                var leftName = GetColumnName(me, me.NodeType);

                                //stringComparisonNode.PropertyNode.Property = leftName;
                                //stringComparisonNode.PropertyNode.Type = "string";
                            }
                        }

                        return stringComparisonNode;
                    }
                    else if (AreMethodsSame(expMethod.Method, Methods.String.StartsWithStringComparison.Value))
                    {
                        Console.WriteLine("Test");
                    }
                    else if (AreMethodsSame(expMethod.Method, Methods.String.EndsWith.Value))
                    {
                        Console.WriteLine("Test");
                    }
                    else if (AreMethodsSame(expMethod.Method, Methods.String.EndsWithStringComparison.Value))
                    {
                        Console.WriteLine("Test");
                    }
                    else if (AreMethodsSame(expMethod.Method, Methods.String.IsNullOrEmpty.Value))
                    {
                        Console.WriteLine("Test");
                    }
                    else if (AreMethodsSame(expMethod.Method, Methods.String.EqualsMethod.Value))
                    {
                        Console.WriteLine("Test");
                    }
                    else if (AreMethodsSame(expMethod.Method, Methods.String.EqualsStringComparison.Value))
                    {
                        Console.WriteLine("Test");
                    }
                    else if (AreMethodsSame(expMethod.Method, Methods.String.Like.Value))
                    {
                        Console.WriteLine("Test");
                    }
                }
            }

            throw new Exception("Expression not supported!");
        }

        //protected override Expression VisitMember(MemberExpression node)
        //{
        //    if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
        //    {
        //        if (node.Type == typeof(bool))
        //        {
        //            var leftName = GetColumnName(node, node.NodeType);

        //            AddQueryEqual(leftName, node.Type);
        //        }

        //        return node;
        //    }

        //    throw new NotSupportedException($"The member '{node.Member.Name}' is not supported");
        //}

        //protected override Expression VisitBinary(BinaryExpression node)
        //{
        //    if (node.NodeType == ExpressionType.AndAlso)
        //    {
        //        // Boolean And with short-circuit
        //        VisitCombination(node, (qh) => { /* noop -- AND is the default combinator */ });
        //    }
        //    else if (node.NodeType == ExpressionType.OrElse)
        //    {
        //        // Boolean Or with short-circuit
        //        VisitCombination(node, qh => qh.Or());
        //    }
        //    else
        //    {
        //        var leftExpression = node.Left;
        //        var memberExpression = leftExpression as MemberExpression;
        //        var rightExpression = node.Right;
        //        var where = new WhereClauseProperties();

        //        while (memberExpression == null && leftExpression.NodeType == ExpressionType.Convert)
        //        {
        //            leftExpression = ((UnaryExpression)leftExpression).Operand;
        //            memberExpression = leftExpression as MemberExpression;
        //        }

        //        if (TryExtractConstantValue(node.Right, out object rightValue))
        //        {
        //            where.Value = rightValue;
        //        }
        //        else
        //        {
        //            throw new NotSupportedException($"The rhs of the binary operator '{rightExpression.NodeType}' should be a constant or closure variable expression. \nUnable to process '{node.Right}'.");
        //        }

        //        string leftName = null;

        //        if (IsRealmValueTypeExpression(memberExpression, out leftName))
        //        {
        //            if (node.NodeType != ExpressionType.Equal && node.NodeType != ExpressionType.NotEqual)
        //            {
        //                throw new NotSupportedException($"Only expressions of type Equal and NotEqual can be used with RealmValueType.");
        //            }

        //            if (rightValue is int intValue)
        //            {
        //                rightValue = (RealmValueType)intValue;
        //            }
        //        }
        //        else
        //        {
        //            where.Property = GetColumnName(memberExpression, node.NodeType);
        //        }

        //        switch (node.NodeType)
        //        {
        //            case ExpressionType.Equal:
        //                where.Operator = "eq";
        //                break;
        //            case ExpressionType.NotEqual:
        //                where.Operator = "neq";
        //                break;
        //            case ExpressionType.LessThan:
        //                where.Operator = "lt";
        //                break;
        //            case ExpressionType.LessThanOrEqual:
        //                where.Operator = "lte";
        //                break;
        //            case ExpressionType.GreaterThan:
        //                where.Operator = "gt";
        //                break;
        //            case ExpressionType.GreaterThanOrEqual:
        //                where.Operator = "gte";
        //                break;
        //            default:
        //                throw new NotSupportedException($"The binary operator '{node.NodeType}' is not supported");
        //        }

        //        _whereList.Add(where);
        //    }

        //    return node;
        //}

        //private bool IsRealmValueTypeExpression(MemberExpression memberExpression, out string leftName)
        //{
        //    leftName = null;

        //    if (memberExpression?.Type != typeof(RealmValueType))
        //    {
        //        return false;
        //    }

        //    if (memberExpression.Expression is MemberExpression innerExpression)
        //    {
        //        leftName = GetColumnName(innerExpression, memberExpression.NodeType);
        //        return innerExpression.Type == typeof(RealmValue);
        //    }

        //    return false;
        //}

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

        //protected void VisitCombination(BinaryExpression b, Action<QueryHandle> combineWith)
        //{
        //    Visit(b.Left);
        //    Visit(b.Right);
        //}

        //private void AddQueryEqual(string columnName, object value)
        //{
        //    var where = new WhereClauseProperties();
        //    var propertyIndex = _metadata.PropertyIndices[columnName];

        //    where.Property = columnName;
        //    where.Operator = "eq";
        //    where.Value = value;
        //}

        //internal static bool TryExtractConstantValue(Expression expr, out object value)
        //{
        //    if (expr.NodeType == ExpressionType.Convert)
        //    {
        //        var operand = ((UnaryExpression)expr).Operand;
        //        return TryExtractConstantValue(operand, out value);
        //    }

        //    if (expr is ConstantExpression constant)
        //    {
        //        value = constant.Value;
        //        return true;
        //    }

        //    var memberAccess = expr as MemberExpression;
        //    if (memberAccess?.Member is FieldInfo fieldInfo)
        //    {
        //        if (fieldInfo.Attributes.HasFlag(FieldAttributes.Static))
        //        {
        //            // Handle static fields (e.g. string.Empty)
        //            value = fieldInfo.GetValue(null);
        //            return true;
        //        }

        //        if (TryExtractConstantValue(memberAccess.Expression, out object targetObject))
        //        {
        //            value = fieldInfo.GetValue(targetObject);
        //            return true;
        //        }
        //    }

        //    if (memberAccess?.Member is PropertyInfo propertyInfo)
        //    {
        //        if (propertyInfo.GetMethod != null && propertyInfo.GetMethod.Attributes.HasFlag(MethodAttributes.Static))
        //        {
        //            value = propertyInfo.GetValue(null);
        //            return true;
        //        }

        //        if (TryExtractConstantValue(memberAccess.Expression, out object targetObject))
        //        {
        //            value = propertyInfo.GetValue(targetObject);
        //            return true;
        //        }
        //    }

        //    value = null;
        //    return false;
        //}
    }
}
