using System;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Realms.Schema;

namespace Realms
{
    internal class WhereClauseVisitor : ExpressionVisitor
    {
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

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            ExpressionNode returnNode;
            if (node.Method.DeclaringType == typeof(string) ||
                node.Method.DeclaringType == typeof(StringExtensions))
            {
                ComparisonNode result;
                if (node.Method.Name.Equals("StartsWith"))
                {
                    result = new StartsWithNode();
                }
                else if (node.Method.Name.Equals("EndsWith"))
                {
                    result = new EndsWithNode();
                }
                else if (node.Method.Name.Equals("Contains"))
                {
                    result = new ContainsNode();
                }
                else if (node.Method.Name.Equals("Like"))
                {
                    ComparisonNodeStringCaseSensitivty result2 = new LikeNode();
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

                        if (node.Arguments[1] is ConstantExpression constantExpression)
                        {
                            result2.Right = new ConstantNode()
                            {
                                Value = constantExpression.Value,
                                Type = "string"
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
                        if (constantExpression.Value == null)
                        {
                            result.Right = new ConstantNode()
                            {
                                Value = constantExpression.Value,
                                Type = "string"
                            };
                        }
                        else
                        {
                            result.Right = new ConstantNode()
                            {
                                Value = constantExpression.Value,
                                Type = GetKind(constantExpression.Value.GetType())
                            };
                        }
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

                if (binaryExpression.Left is MemberExpression binaryLeftMemberExpression)
                {
                    if (binaryLeftMemberExpression.Expression != null && binaryLeftMemberExpression.Expression.NodeType == ExpressionType.Parameter)
                    {
                        comparisonNode.Left = new PropertyNode()
                        {
                            Name = GetColumnName(binaryLeftMemberExpression, binaryLeftMemberExpression.NodeType),
                            Type = GetKind(binaryLeftMemberExpression.Type)
                        };
                    }
                    else
                    {
                        throw new NotSupportedException(binaryLeftMemberExpression + " is null or not a supported node type.");
                    }
                }

                if (binaryExpression.Left is ConstantExpression binaryLeftConstantExpression)
                {
                    if (binaryLeftConstantExpression.Value == null)
                    {
                        comparisonNode.Left = new ConstantNode()
                        {
                            Value = binaryLeftConstantExpression.Value,
                            Type = "string"
                        };
                    }
                    else
                    {
                        comparisonNode.Left = new ConstantNode()
                        {
                            Value = binaryLeftConstantExpression.Value,
                            Type = GetKind(binaryLeftConstantExpression.Value.GetType())
                        };
                    }
                }

                if (binaryExpression.Right is MemberExpression binaryRightMemberExpression)
                {
                    if (binaryRightMemberExpression.Expression != null && binaryRightMemberExpression.Expression.NodeType == ExpressionType.Parameter)
                    {
                        comparisonNode.Right = new PropertyNode()
                        {
                            Name = GetColumnName(binaryRightMemberExpression, binaryRightMemberExpression.NodeType),
                            Type = GetKind(binaryRightMemberExpression.Type)
                        };
                    }
                    else
                    {
                        throw new NotSupportedException(binaryRightMemberExpression + " is null or not a supported node type.");
                    }
                }

                if (binaryExpression.Right is ConstantExpression binaryRightConstantExpression)
                {
                    if (binaryRightConstantExpression.Value == null)
                    {
                        comparisonNode.Right = new ConstantNode()
                        {
                            Value = binaryRightConstantExpression.Value,
                            Type = "string"
                        };
                    }
                    else
                    {
                        comparisonNode.Right = new ConstantNode()
                        {
                            Value = binaryRightConstantExpression.Value,
                            Type = GetKind(binaryRightConstantExpression.Value.GetType())
                        };
                    }
                }

                returnNode = comparisonNode;
            }

            return RealmLinqExpression.Create(returnNode);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            NegationNode returnNode = new NegationNode();
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    returnNode.Expression = Extract(node.Operand);
                    break;
                default:
                    throw new NotSupportedException($"The unary operator '{node.NodeType}' is not supported");
            }

            return RealmLinqExpression.Create(returnNode);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
            {
                ComparisonNode comparisonNode = new EqualityNode();
                if (node.Type == typeof(bool))
                {
                    comparisonNode.Left = new PropertyNode()
                    {
                        Name = GetColumnName(node, node.NodeType),
                        Type = GetKind(node.Type)
                    };
                    comparisonNode.Right = new ConstantNode()
                    {
                        Value = true,
                        Type = "bool"
                    };
                }

                return RealmLinqExpression.Create(comparisonNode);
            }

            throw new NotSupportedException($"The member '{node.Member.Name}' is not supported");
        }

        private static string GetKind(object valueType)
        {
            if (valueType == typeof(float))
            {
                return "float";
            }
            else if (valueType == typeof(long))
            {
                return "long";
            }
            else if (valueType == typeof(double))
            {
                return "double";
            }
            else if (valueType == typeof(string))
            {
                return "string";
            }
            else if (valueType == typeof(bool))
            {
                return "bool";
            }
            else if (valueType == null)
            {
                return "null";
            }
            else
            {
                throw new NotSupportedException(valueType + " is not a supported type.");
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
    }
}
