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
                var test = node.Method.Name;
                if (test.Equals("StartsWith"))
                {
                    result = new StartsWithNode();
                }
                else if (test.Equals("EndsWith"))
                {
                    result = new EndsWithNode();
                }
                else if (test.Equals("Contains"))
                {
                    result = new ContainsNode();
                }

                // TODO Undecided implementation
                else if (test.Equals("Like"))
                {
                    result = new LikeNode();
                }
                else
                {
                    throw new NotSupportedException(node.Method.Name + " is not supported string operation method");
                }

                if (node.Object is MemberExpression me)
                {
                    if (me.Expression != null && me.Expression.NodeType == ExpressionType.Parameter)
                    {
                        result.Left = new PropertyNode();
                        var leftName = GetColumnName(me, me.NodeType);
                        result.Left.Name = leftName;
                        result.Left.Type = GetKind(me.Type);
                    }
                    else
                    {
                        throw new NotSupportedException(me + " is null or not a supported type.");
                    }

                    if (node.Arguments[0] is ConstantExpression ce)
                    {
                        result.Right = new ConstantNode();
                        result.Right.Value = ce.Value;
                        result.Right.Type = GetKind(ce.Value.GetType());
                    }
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
                        comparisonNode.Left = new PropertyNode();
                        var leftName = GetColumnName(me, me.NodeType);

                        comparisonNode.Left.Name = leftName;
                        comparisonNode.Left.Type = GetKind(me.Type);
                    }
                    else
                    {
                        throw new NotSupportedException(me + " is null or not a supported node type.");
                    }
                }

                if (be.Left is ConstantExpression ce)
                {
                    comparisonNode.Left = new ConstantNode();
                    comparisonNode.Left.Value = ce.Value;
                    comparisonNode.Left.Type = GetKind(ce.Value.GetType());
                }

                if (be.Right is MemberExpression mo)
                {
                    if (mo.Expression != null && mo.Expression.NodeType == ExpressionType.Parameter)
                    {
                        comparisonNode.Right = new PropertyNode();
                        var leftName = GetColumnName(mo, mo.NodeType);

                        comparisonNode.Right.Name = leftName;
                        comparisonNode.Right.Type = GetKind(mo.Type);
                    }
                    else
                    {
                        throw new NotSupportedException(mo + " is null or not a supported node type.");
                    }
                }

                if (be.Right is ConstantExpression co)
                {
                    comparisonNode.Right = new ConstantNode();
                    comparisonNode.Right.Value = co.Value;
                    comparisonNode.Right.Type = GetKind(co.Value.GetType());
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
                    returnNode.Expression = Extract(node.Operand);  // recurse into richer expression, expect to VisitCombination
                    break;
                default:
                    throw new NotSupportedException($"The unary operator '{node.NodeType}' is not supported");
            }

            return RealmLinqExpression.Create(returnNode);
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
