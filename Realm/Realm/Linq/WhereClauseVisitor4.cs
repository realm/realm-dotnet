using System;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Realms.Schema;

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

    internal class WhereClauseVisitor4 : ExpressionVisitor
    {
        private readonly RealmObjectBase.Metadata _metadata;

        private WhereClause _whereClause;

        public WhereClauseVisitor4(RealmObjectBase.Metadata metadata)
        {
            _metadata = metadata;
        }

        public WhereClause VisitWhere(LambdaExpression whereClause)
        {
            _whereClause = new WhereClause();
            _whereClause.ExpNode = Extract(whereClause.Body);
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

                        comparisonNode.Property = leftName;
                    }
                }

                if (be.Right is ConstantExpression co)
                {
                    comparisonNode.Value = co.Value;
                }

                returnNode = comparisonNode;
            }

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
    }
}
