using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Realms.Helpers;
using Realms.Schema;
using LazyMethod = System.Lazy<System.Reflection.MethodInfo>;

namespace Realms
{
    internal class WhereClauseVisitor : ClauseVisitor
    {
        private readonly RealmObjectBase.Metadata _metadata;
        private readonly List<RealmValue> _arguments;

        private WhereClause _whereClause;
        private int _argumentsCounter;

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
            _arguments = new List<RealmValue>();
            _whereClause = new WhereClause();
        }

        public WhereClause VisitWhere(Expression whereClause, int argumentsCounter, out List<RealmValue> arguments)
        {
            _argumentsCounter = argumentsCounter;
            var lambda = (LambdaExpression)StripQuotes(whereClause);
            _whereClause.Expression = ExtractNode(lambda.Body);
            arguments = _arguments;
            return _whereClause;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            ExpressionNode returnNode;
            if (node.Method.DeclaringType == typeof(string) ||
                node.Method.DeclaringType == typeof(StringExtensions))
            {
                StringComparisonNode result;
                if (AreMethodsSame(node.Method, Methods.String.StartsWith.Value) || AreMethodsSame(node.Method, Methods.String.StartsWithStringComparison.Value))
                {
                    result = new StartsWithNode();
                }
                else if (AreMethodsSame(node.Method, Methods.String.EndsWith.Value) || AreMethodsSame(node.Method, Methods.String.EndsWithStringComparison.Value))
                {
                    result = new EndsWithNode();
                }
                else if (AreMethodsSame(node.Method, Methods.String.Contains.Value) || AreMethodsSame(node.Method, Methods.String.ContainsStringComparison.Value))
                {
                    result = new ContainsNode();
                }
                else if (AreMethodsSame(node.Method, Methods.String.EqualsMethod.Value) || AreMethodsSame(node.Method, Methods.String.EqualsStringComparison.Value))
                {
                    result = new StringEqualityNode();
                }
                else if (AreMethodsSame(node.Method, Methods.String.Like.Value))
                {
                    result = new LikeNode();
                }
                else
                {
                    throw new NotSupportedException(node.Method.Name + " is not a supported string operation method");
                }

                result.CaseSensitivity = GetCaseSensitivity(node);

                MemberExpression memberExpression;
                ConstantExpression constantExpression;

                // This means that it's a static method (from StringExtensions)
                if (node.Object == null)
                {
                    memberExpression = node.Arguments[0] as MemberExpression;
                    constantExpression = node.Arguments[1] as ConstantExpression;
                }
                else
                {
                    memberExpression = node.Object as MemberExpression;
                    constantExpression = node.Arguments[0] as ConstantExpression;
                }

                if (memberExpression != null && constantExpression != null)
                {
                    result.Left = ExtractNode(memberExpression);
                    result.Right = ExtractNode(constantExpression);
                }
                else
                {
                    throw new NotSupportedException(node + " is not a supported a supported string expression.");
                }

                returnNode = result;
            }
            else
            {
                throw new NotSupportedException(node.Method + " is not a supported method.");
            }

            return RealmLinqExpression.Create(returnNode);
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            ExpressionNode returnNode;
            if (binaryExpression.NodeType == ExpressionType.AndAlso)
            {
                var andNode = new AndNode();
                andNode.Left = ExtractNode(binaryExpression.Left);
                andNode.Right = ExtractNode(binaryExpression.Right);
                returnNode = andNode;
            }
            else if (binaryExpression.NodeType == ExpressionType.OrElse)
            {
                var orNode = new OrNode();
                orNode.Left = ExtractNode(binaryExpression.Left);
                orNode.Right = ExtractNode(binaryExpression.Right);
                returnNode = orNode;
            }
            else
            {
                ComparisonNode comparisonNode;
                switch (binaryExpression.NodeType)
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
                        throw new NotSupportedException($"The operator '{binaryExpression.NodeType}' is not supported");
                }

                var leftExpression = ExtractNode(binaryExpression.Left, false);
                var rightExpression = ExtractNode(binaryExpression.Right, false);

                comparisonNode.Left = leftExpression;
                comparisonNode.Right = rightExpression;

                returnNode = comparisonNode;
            }

            return RealmLinqExpression.Create(returnNode);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            ExpressionNode returnNode;
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    NegationNode negationNode = new NegationNode();
                    negationNode.Expression = ExtractNode(node.Operand);
                    returnNode = negationNode;
                    break;
                default:
                    throw new NotSupportedException($"The unary operator '{node.NodeType}' is not supported");
            }

            return RealmLinqExpression.Create(returnNode);
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            ExpressionNode result = null;
            if (IsParameter(memberExpression, out var path))
            {
                var propertyNode = new PropertyNode()
                {
                    Type = GetKind(memberExpression.Type)
                };
                propertyNode.Path = path;
                result = propertyNode;
            }
            else
            {
                //TODO Easy way to get constant values from member expressions. Good idea?
                var objectMember = Expression.Convert(memberExpression, typeof(object));
                var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                var value = getterLambda.Compile().Invoke();
                result = new ConstantNode()
                {
                    Value = ExtractValue(value)
                };
            }

            return RealmLinqExpression.Create(result);
        }

        private bool IsParameter(MemberExpression memberExpression, out List<string> path)
        {
            if (memberExpression.Expression != null && memberExpression.Expression.NodeType == ExpressionType.Parameter)
            {
                path = new List<string>();
                path.Add(GetColumnName(memberExpression, memberExpression.NodeType));
                return true;
            }
            else if (memberExpression.Expression is MemberExpression innerExpression && IsParameter(innerExpression, out var innerPath))
            {
                path = innerPath;
                path.Add(GetColumnName(memberExpression, memberExpression.NodeType));
                return true;
            }
            else
            {
                path = null;
                return false;
            }
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            var constantNode = new ConstantNode()
            {
                Value = ExtractValue(node.Value),
            };
            return RealmLinqExpression.Create(constantNode);
        }

        private static bool GetCaseSensitivity(MethodCallExpression methodExpression)
        {
            int argumentIndex;

            // This means that it's a static method (coming from StringExtensions)
            if (methodExpression.Object == null && methodExpression.Arguments.Count == 3)
            {
                argumentIndex = 2;
            }
            else if (methodExpression.Arguments.Count == 2)
            {
                argumentIndex = 1;
            }
            else
            {
                return true;
            }

            if (methodExpression.Arguments[argumentIndex] is ConstantExpression constantExpression)
            {
                // Special case for "Like" -- //TODO Have I made this method too generic?
                if (constantExpression.Value is bool boolValue)
                {
                    return boolValue;
                }

                if (constantExpression.Value is StringComparison comparison)
                {
                    return comparison switch
                    {
                        StringComparison.Ordinal => true,
                        StringComparison.OrdinalIgnoreCase => false,
                        _ => throw new NotSupportedException($"The comparison {comparison} is not yet supported. Use {StringComparison.Ordinal} or {StringComparison.OrdinalIgnoreCase}."),
                    };
                }
                else
                {
                    throw new NotSupportedException(methodExpression.Method.Name + " is not a supported string operation method");
                }
            }
            else
            {
                throw new NotSupportedException(methodExpression.Method.Name + " is not a supported string operation method");
            }
        }

        private static string GetKind(Type type)
        {
            if (type == typeof(float))
            {
                return "float";
            }
            else if (type == typeof(long))
            {
                return "long";
            }
            else if (type == typeof(double))
            {
                 return "double";
            }
            else if (type == typeof(string))
            {
                return "string";
            }
            else if (type == typeof(bool))
            {
                return "bool";
            }  //TODO needs to be completed
            else
            {
                throw new NotSupportedException(type + " is not a supported type.");
            }
        }

        private ExpressionNode ExtractNode(Expression node, bool expandMemberExpression = true)
        {
            var expressionNode = (Visit(node) as RealmLinqExpression).ExpressionNode;
            if (expandMemberExpression && expressionNode is PropertyNode propertyNode && propertyNode.Type == "bool")
            {
                var comparisonNode = new EqualityNode();
                comparisonNode.Left = expressionNode;
                comparisonNode.Right = new ConstantNode()
                {
                    Value = ExtractValue(true),
                };
                return comparisonNode;
            }

            return expressionNode;
        }

        private string ExtractValue(object value)
        {
            var realmValue = Operator.Convert<RealmValue>(value);  //TODO Should check if exceptions are thrown
            _arguments.Add(realmValue);
            return $"${_argumentsCounter++}";
        }

        private string GetColumnName(MemberExpression memberExpression, ExpressionType? parentType = null)
        {
            var name = memberExpression?.Member.GetMappedOrOriginalName();

            if (parentType.HasValue)
            {
                if (name == null ||
                    memberExpression.Expression.NodeType != ExpressionType.Parameter ||
                    !_metadata.Schema.TryFindProperty(name, out var property) ||
                    property.Type.HasFlag(PropertyType.Array) ||
                    property.Type.HasFlag(PropertyType.Set))
                {
                    throw new NotSupportedException($"The {parentType} operator must be a direct access to a persisted property in Realm.\nUnable to process '{memberExpression}'.");
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
