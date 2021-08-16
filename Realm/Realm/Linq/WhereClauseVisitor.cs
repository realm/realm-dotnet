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
                    if (node.Arguments.Count > 2)
                    {
                        if (node.Arguments[0] is MemberExpression me2)
                        {
                            result2.Left = new PropertyNode()
                            {
                                Name = GetColumnName(me2, me2.NodeType),
                                Type = GetKind(me2.Type)
                            };
                        }

                        if (node.Arguments[1] is ConstantExpression ce)
                        {
                            result2.Right = new ConstantNode()
                            {
                                Value = ce.Value,
                                Type = "string"
                            };
                        }

                        if (node.Arguments[2] is ConstantExpression ce2)
                        {
                            result2.CaseSensitivity = (bool)ce2.Value;
                        }
                    }

                    returnNode = result2;
                    return RealmLinqExpression.Create(returnNode);
                }
                else
                {
                    throw new NotSupportedException(node.Method.Name + " is not supported string operation method");
                }

                if (node.Object is MemberExpression me)
                {
                    if (me.Expression != null && me.Expression.NodeType == ExpressionType.Parameter)
                    {
                        result.Left = new PropertyNode()
                        {
                            Name = GetColumnName(me, me.NodeType),
                            Type = GetKind(me.Type)
                        };
                    }
                    else
                    {
                        throw new NotSupportedException(me + " is null or not a supported type.");
                    }

                    if (node.Arguments[0] is ConstantExpression ce)
                    {
                        if (ce.Value == null)
                        {
                            result.Right = new ConstantNode()
                            {
                                Value = ce.Value,
                                Type = "string"
                            };
                        }
                        else
                        {
                            result.Right = new ConstantNode()
                            {
                                Value = ce.Value,
                                Type = GetKind(ce.Value.GetType())
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
                        throw new NotSupportedException($"The operator '{be.NodeType}' is not supported");
                }

                if (be.Left is MemberExpression me)
                {
                    if (me.Expression != null && me.Expression.NodeType == ExpressionType.Parameter)
                    {
                        comparisonNode.Left = new PropertyNode()
                        {
                            Name = GetColumnName(me, me.NodeType),
                            Type = GetKind(me.Type)
                        };
                    }
                    else
                    {
                        throw new NotSupportedException(me + " is null or not a supported node type.");
                    }
                }

                if (be.Left is ConstantExpression ce)
                {
                    if (ce.Value == null)
                    {
                        comparisonNode.Left = new ConstantNode()
                        {
                            Value = ce.Value,
                            Type = "string"
                        };
                    }
                    else
                    {
                        comparisonNode.Left = new ConstantNode()
                        {
                            Value = ce.Value,
                            Type = GetKind(ce.Value.GetType())
                        };
                    }
                }

                if (be.Right is MemberExpression mo)
                {
                    if (mo.Expression != null && mo.Expression.NodeType == ExpressionType.Parameter)
                    {
                        comparisonNode.Right = new PropertyNode()
                        {
                            Name = GetColumnName(mo, mo.NodeType),
                            Type = GetKind(mo.Type)
                        };
                    }
                    else
                    {
                        throw new NotSupportedException(mo + " is null or not a supported node type.");
                    }
                }

                if (be.Right is ConstantExpression co)
                {
                    if (co.Value == null)
                    {
                        comparisonNode.Right = new ConstantNode()
                        {
                            Value = co.Value,
                            Type = "string"
                        };
                    }
                    else
                    {
                        comparisonNode.Right = new ConstantNode()
                        {
                            Value = co.Value,
                            Type = GetKind(co.Value.GetType())
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
