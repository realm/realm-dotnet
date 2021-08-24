using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
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
            _whereClause.Expression = Extract(lambda.Body);
            arguments = _arguments;
            return _whereClause;
        }

        private ExpressionNode Extract(Expression node)  //TODO need to modify this method to accept a boolean "shouldExpandMemberExpression" to solve the issue with boolean properties
        {
            var realmLinqExpression = Visit(node) as RealmLinqExpression;
            return realmLinqExpression.ExpressionNode;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            ExpressionNode returnNode;
            if (node.Method.DeclaringType == typeof(string) ||
                node.Method.DeclaringType == typeof(StringExtensions))
            {
                StringComparisonNode result;
                if (AreMethodsSame(node.Method, Methods.String.StartsWith.Value))  //TODO Can we just use string comparison with the name...? 
                {
                    result = new StartsWithNode();
                }
                else if (AreMethodsSame(node.Method, Methods.String.StartsWithStringComparison.Value))
                {
                    result = new StartsWithNode();
                    if (node.Arguments[1] is ConstantExpression constantExpression)
                    {
                        if (constantExpression.Value.Equals(StringComparison.OrdinalIgnoreCase))
                        {
                            result.CaseSensitivity = false;
                        }
                    }
                }
                else if (AreMethodsSame(node.Method, Methods.String.EndsWith.Value))
                {
                    result = new EndsWithNode();
                }
                else if (AreMethodsSame(node.Method, Methods.String.EndsWithStringComparison.Value))
                {
                    result = new EndsWithNode();
                    if (node.Arguments[1] is ConstantExpression constantExpression)
                    {
                        if (constantExpression.Value.Equals(StringComparison.OrdinalIgnoreCase))
                        {
                            result.CaseSensitivity = false;
                        }
                    }
                }
                else if (AreMethodsSame(node.Method, Methods.String.Contains.Value))
                {
                    result = new ContainsNode();
                }
                else if (AreMethodsSame(node.Method, Methods.String.ContainsStringComparison.Value))
                {
                    result = new ContainsNode();
                    if (node.Arguments[1] is ConstantExpression constantExpression)
                    {
                        if (constantExpression.Value.Equals(StringComparison.OrdinalIgnoreCase))
                        {
                            result.CaseSensitivity = false;
                        }
                    }
                }
                else if (AreMethodsSame(node.Method, Methods.String.EqualsMethod.Value))
                {
                    result = new StringEqualityNode();
                }
                else if (AreMethodsSame(node.Method, Methods.String.EqualsStringComparison.Value))
                {
                    result = new StringEqualityNode();
                    if (node.Arguments[1] is ConstantExpression constantExpression)
                    {
                        if (constantExpression.Value.Equals(StringComparison.OrdinalIgnoreCase))
                        {
                            result.CaseSensitivity = false;
                        }
                    }
                }
                else if (AreMethodsSame(node.Method, Methods.String.Like.Value))
                {
                    StringComparisonNode result2 = new LikeNode();
                    if (node.Arguments.Count == 3)
                    {
                        if (node.Arguments[0] is MemberExpression stringMemberExpression)
                        {
                            result2.Left = new PropertyNode()
                            {
                                Name = GetColumnName(stringMemberExpression, stringMemberExpression.NodeType),
                                Type = GetKind(stringMemberExpression.Type)
                            };
                        }

                        if (node.Arguments[1] is ConstantExpression constantExpression)  //TODO Need to reuse the visit and extrac methods for this
                        {
                            result2.Right = new ConstantNode()
                            {
                                Value = ExtractValue(constantExpression.Value),
                            };
                        }

                        if (node.Arguments[2] is ConstantExpression isCaseSensetive)
                        {
                            result2.CaseSensitivity = (bool)isCaseSensetive.Value;
                        }
                    }
                    else
                    {
                        throw new NotSupportedException("Not a supported 'Like' string query.");
                    }

                    returnNode = result2;
                    return RealmLinqExpression.Create(returnNode);
                }
                else
                {
                    throw new NotSupportedException(node.Method.Name + " is not supported string operation method");
                }

                if (node.Object is MemberExpression memberExpression)
                {
                    if (memberExpression.Expression != null && memberExpression.Expression.NodeType == ExpressionType.Parameter)
                    {
                        result.Left = new PropertyNode()
                        {
                            Name = GetColumnName(memberExpression, memberExpression.NodeType),
                            Type = GetKind(memberExpression.Type)
                        };
                    }
                    else
                    {
                        throw new NotSupportedException(memberExpression + " is null or not a supported type.");
                    }

                    if (node.Arguments[0] is ConstantExpression constantExpression)
                    {
                        result.Right = new ConstantNode()
                        {
                            Value = ExtractValue(constantExpression.Value),
                        };
                    }
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
                andNode.Left = Extract(binaryExpression.Left);
                andNode.Right = Extract(binaryExpression.Right);
                returnNode = andNode;
            }
            else if (binaryExpression.NodeType == ExpressionType.OrElse)
            {
                var orNode = new OrNode();
                orNode.Left = Extract(binaryExpression.Left);
                orNode.Right = Extract(binaryExpression.Right);
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

                var leftExpression = Extract(binaryExpression.Left);
                var rightExpression = Extract(binaryExpression.Right);

                if (!((leftExpression is PropertyNode && rightExpression is ConstantNode) || (leftExpression is ConstantNode && rightExpression is PropertyNode)))
                {
                    throw new Exception("WHAT DO I WRITE HERE?"); //TODO;
                }

                comparisonNode.Left = leftExpression;
                comparisonNode.Right = rightExpression;

                returnNode = comparisonNode;
            }

            return RealmLinqExpression.Create(returnNode);
        }

        private string ExtractValue(object value)
        {
            _arguments.Add(ExtractRealmValue(value));
            return $"${_argumentsCounter++}";
        }

        private RealmValue ExtractRealmValue(object value)  //TODO Probably we can move this to RealmValue itself
        {
            return value switch
            {
                int intValue => RealmValue.Create(intValue, RealmValueType.Int),
                bool boolValue => RealmValue.Create(boolValue, RealmValueType.Bool),
                string stringValue => RealmValue.Create(stringValue, RealmValueType.String),
                float floatValue => RealmValue.Create(floatValue, RealmValueType.Float),
                double doubleValue => RealmValue.Create(doubleValue, RealmValueType.Double),
                null => RealmValue.Null,
                RealmValue realmValue => realmValue,
                _ => throw new Exception("TODOOOOOO")
            };
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            ExpressionNode returnNode;
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    NegationNode negationNode = new NegationNode();
                    negationNode.Expression = Extract(node.Operand);
                    returnNode = negationNode;
                    break;
                default:
                    throw new NotSupportedException($"The unary operator '{node.NodeType}' is not supported");
            }

            return RealmLinqExpression.Create(returnNode);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            //TODO The problem is that sometimes this needs to be expanded, sometimes this needs to be kept as it is...
            if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
            {
                ExpressionNode result;

                //if (node.Type == typeof(bool))

                if (false)
                {
                    var comparisonNode = new EqualityNode();
                    comparisonNode.Left = new PropertyNode()
                    {
                        Name = GetColumnName(node, node.NodeType),
                        Type = GetKind(node.Type)
                    };
                    comparisonNode.Right = new ConstantNode()
                    {
                        Value = ExtractValue(true),
                    };
                    result = comparisonNode;
                }
                else
                {
                    result = new PropertyNode()
                    {
                        Name = GetColumnName(node, node.NodeType),
                        Type = GetKind(node.Type)
                    };
                }

                return RealmLinqExpression.Create(result);
            }

            throw new NotSupportedException($"The member '{node.Member.Name}' is not supported");
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            var constantNode = new ConstantNode()
            {
                Value = ExtractValue(node.Value),
            };
            return RealmLinqExpression.Create(constantNode);
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
