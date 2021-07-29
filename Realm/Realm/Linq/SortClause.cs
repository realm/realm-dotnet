using System;
using System.Linq;
using System.Linq.Expressions;

namespace Realms
{
    internal class SortClause
    {
        public OrderingClause _orderingClause;

        public OrderingClause VisitOrderClause(MethodCallExpression orderClause)
        {
            if (orderClause.Method.Name == nameof(Queryable.OrderBy))
            {
                _orderingClause = new OrderByNode();
                _orderingClause.IsAscending = true;
                _orderingClause.IsReplacing = true;
            }
            else if (orderClause.Method.Name == nameof(Queryable.ThenBy))
            {
                _orderingClause = new ThenByNode();
                _orderingClause.IsAscending = true;
                _orderingClause.IsReplacing = false;
            }
            else if (orderClause.Method.Name == nameof(Queryable.OrderByDescending))
            {
                _orderingClause = new OrderByDescendingNode();
                _orderingClause.IsAscending = false;
                _orderingClause.IsReplacing = true;

            }
            else if (orderClause.Method.Name == nameof(Queryable.ThenByDescending))
            {
                _orderingClause = new ThenByDescendingNode();
                _orderingClause.IsAscending = false;
                _orderingClause.IsReplacing = false;

            }
            else
            {
                throw new NotSupportedException(orderClause.Method.Name + " is not a support order method");
            }

            var lambda = (LambdaExpression)StripQuotes(orderClause.Arguments[1]);
            if (lambda.Body is MemberExpression me)
            {
                _orderingClause.Property = me.Member.Name;
            }
            else
            {
                throw new NotSupportedException("Orderclause must have memberexpression");
            }

            return _orderingClause;
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }

            return e;
        }
    }
}