using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Realms.Schema;
using LazyMethod = System.Lazy<System.Reflection.MethodInfo>;

namespace Realms
{
    internal class RealmLinqExpression : Expression
    {
        public ExpressionNode ExpressionNode { get; private set; }

        public static RealmLinqExpression Create(ExpressionNode exp)
        {
            return new RealmLinqExpression { ExpressionNode = exp };
        }
    }

    internal class WhereClauseVisitor : ExpressionVisitor
    {
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

        private readonly RealmObjectBase.Metadata _metadata;

        private WhereClause _whereClause;

        public WhereClauseVisitor(RealmObjectBase.Metadata metadata)
        {
            _metadata = metadata;
            _whereClause = new WhereClause();
        }

        public WhereClause VisitWhere(LambdaExpression whereClause)
        {
            _whereClause.Expression = Extract(whereClause.Body);
            var json = JsonConvert.SerializeObject(_whereClause, formatting: Formatting.Indented);
            return _whereClause;
        }

        private ExpressionNode Extract(Expression node)
        {
            var realmLinqExpression = Visit(node) as RealmLinqExpression;
            return realmLinqExpression.ExpressionNode;
        }

        protected override Expression VisitBinary(BinaryExpression be)
        {
            ExpressionNode returnNode;
            if (be.NodeType == ExpressionType.AndAlso)
            {
                var andNode = new AndNode();
                andNode.Left = Extract(be.Left);
                andNode.Right = Extract(be.Right);
                returnNode = andNode;
            }
            else if (be.NodeType == ExpressionType.OrElse)
            {
                var orNode = new OrNode();
                orNode.Left = Extract(be.Left);
                orNode.Right = Extract(be.Right);
                returnNode = orNode;
            }
            else
            {
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
                        else
                        {
                            throw new NotSupportedException(me.Type + "is not a supported type.");
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
                returnNode = comparisonNode;
            }
            //else if (exp is MemberExpression)
            //{
            //    if (exp.Type == typeof(bool))
            //    {
            //        var boolNode = new BooleanPropertyNode();
            //        var propName = ((MemberExpression)exp).Member.GetMappedOrOriginalName();
            //        boolNode.Property = propName;
            //        return boolNode;
            //    }
            //    throw new NotSupportedException($"The member '{((MemberExpression)exp).Member.Name}' is not supported");
            //}
            //else if (exp is MethodCallExpression)
            //{
            //    if (((MethodCallExpression)exp).Method.DeclaringType == typeof(string) ||
            //        ((MethodCallExpression)exp).Method.DeclaringType == typeof(StringExtensions))
            //    {
            //        var expMethod = exp as MethodCallExpression;

            //        ComparisonNode stringComparisonNode;

            //        if (AreMethodsSame(expMethod.Method, Methods.String.Contains.Value))
            //        {
            //            Console.WriteLine("Test");
            //        }
            //        //else if (IsStringContainsWithComparison(node.Method, out var index))
            //        //{
            //        //    Console.WriteLine("Test");
            //        //}
            //        else if (AreMethodsSame(expMethod.Method, Methods.String.StartsWith.Value))
            //        {
            //            stringComparisonNode = new StartsWithNode();
            //            if (expMethod.Arguments[0] is ConstantExpression c)
            //            {
            //                //stringComparisonNode.ValueNode.Value = c.Value;
            //                //stringComparisonNode.ValueNode.Type = "string";
            //            }

            //            if (((MethodCallExpression)exp).Object is MemberExpression me)
            //            {
            //                if (me.Expression != null && me.Expression.NodeType == ExpressionType.Parameter)
            //                {
            //                    var leftName = GetColumnName(me, me.NodeType);

            //                    //stringComparisonNode.PropertyNode.Property = leftName;
            //                    //stringComparisonNode.PropertyNode.Type = "string";
            //                }
            //            }

            //            return stringComparisonNode;
            //        }
            //        else if (AreMethodsSame(expMethod.Method, Methods.String.StartsWithStringComparison.Value))
            //        {
            //            Console.WriteLine("Test");
            //        }
            //        else if (AreMethodsSame(expMethod.Method, Methods.String.EndsWith.Value))
            //        {
            //            Console.WriteLine("Test");
            //        }
            //        else if (AreMethodsSame(expMethod.Method, Methods.String.EndsWithStringComparison.Value))
            //        {
            //            Console.WriteLine("Test");
            //        }
            //        else if (AreMethodsSame(expMethod.Method, Methods.String.IsNullOrEmpty.Value))
            //        {
            //            Console.WriteLine("Test");
            //        }
            //        else if (AreMethodsSame(expMethod.Method, Methods.String.EqualsMethod.Value))
            //        {
            //            Console.WriteLine("Test");
            //        }
            //        else if (AreMethodsSame(expMethod.Method, Methods.String.EqualsStringComparison.Value))
            //        {
            //            Console.WriteLine("Test");
            //        }
            //        else if (AreMethodsSame(expMethod.Method, Methods.String.Like.Value))
            //        {
            //            Console.WriteLine("Test");
            //        }
            //    }
            //}

            return RealmLinqExpression.Create(returnNode);
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
    }
}
