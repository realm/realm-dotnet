using System;
using System.Linq;
using System.Linq.Expressions;

namespace Realms
{
    internal class SortClauseVisitor
    {
        public OrderingClause _orderingClause;

        public OrderingClause VisitOrderClause(MethodCallExpression orderClause)
        {
            _orderingClause = new OrderingClause();

            if (orderClause.Method.Name == nameof(Queryable.OrderBy)
                || orderClause.Method.Name == nameof(Queryable.ThenBy))
            {
                _orderingClause.IsAscending = true;
            }
            else if (orderClause.Method.Name == nameof(Queryable.OrderByDescending)
                || orderClause.Method.Name == nameof(Queryable.ThenByDescending))
            {
                _orderingClause.IsAscending = false;
            }
            else
            {
                throw new NotSupportedException(orderClause.Method.Name + " is not a supported ordering method");
            }

            var lambda = (LambdaExpression)RealmResultsVisitor2.StripQuotes(orderClause.Arguments[1]);
            if (lambda.Body is MemberExpression me)
            {
                _orderingClause.Property = me.Member.Name;
            }
            else
            {
                throw new NotSupportedException("Unable to sort on specified property");
            }

            return _orderingClause;
        }
    }
}