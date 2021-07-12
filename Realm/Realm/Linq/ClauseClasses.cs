using System;
using System.Linq.Expressions;
using System.Reflection;
using Realms.Schema;

namespace Realms
{
    internal class ClauseClasses : ExpressionVisitor
    {
        private readonly RealmObjectBase.Metadata _metadata;

        public ClauseClasses(RealmObjectBase.Metadata metadata)
        {
            _metadata = metadata;
        }

        public void VisitWhere(LambdaExpression whereClause)
        {
            Visit(whereClause.Body);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
            {
                if (node.Type == typeof(bool))
                {
                    var leftName = GetColumnName(node, node.NodeType);

                    AddQueryEqual(leftName, node.Type);
                }

                return node;
            }

            throw new NotSupportedException($"The member '{node.Member.Name}' is not supported");
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
                var where = new Where();

                while (memberExpression == null && leftExpression.NodeType == ExpressionType.Convert)
                {
                    leftExpression = ((UnaryExpression)leftExpression).Operand;
                    memberExpression = leftExpression as MemberExpression;
                }

                if (TryExtractConstantValue(node.Right, out object rightValue))
                {
                    where.value = rightValue;
                }
                else
                {
                    throw new NotSupportedException($"The rhs of the binary operator '{rightExpression.NodeType}' should be a constant or closure variable expression. \nUnable to process '{node.Right}'.");
                }

                string leftName = null;

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
                    where.property = GetColumnName(memberExpression, node.NodeType);
                }

                switch (node.NodeType)
                {
                    case ExpressionType.Equal:
                        where.op = "eq";
                        break;
                    case ExpressionType.NotEqual:
                        where.op = "neq";
                        break;
                    case ExpressionType.LessThan:
                        where.op = "lt";
                        break;
                    case ExpressionType.LessThanOrEqual:
                        where.op = "lte";
                        break;
                    case ExpressionType.GreaterThan:
                        where.op = "gt";
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        where.op = "gte";
                        break;
                    default:
                        throw new NotSupportedException($"The binary operator '{node.NodeType}' is not supported");
                }
            }
            return node;
        }

        private bool IsRealmValueTypeExpression(MemberExpression memberExpression, out string leftName)
        {
            leftName = null;

            if (memberExpression?.Type != typeof(RealmValueType))
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

        protected void VisitCombination(BinaryExpression b, Action<QueryHandle> combineWith)
        {
            // TODO: Where class solujtion that supports groups like these
            //_coreQueryHandle.GroupBegin();
            Visit(b.Left);
            //combineWith(_coreQueryHandle);
            Visit(b.Right);
            //test
            //_coreQueryHandle.GroupEnd();
        }

        private void AddQueryEqual(string columnName, object value)
        {
            var where = new Where();
            var propertyIndex = _metadata.PropertyIndices[columnName];

            where.property = columnName;
            where.op = "eq";
            where.value = value;
        }

        internal static bool TryExtractConstantValue(Expression expr, out object value)
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

                if (TryExtractConstantValue(memberAccess.Expression, out object targetObject))
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

                if (TryExtractConstantValue(memberAccess.Expression, out object targetObject))
                {
                    value = propertyInfo.GetValue(targetObject);
                    return true;
                }
            }

            value = null;
            return false;
        }
    }
}
