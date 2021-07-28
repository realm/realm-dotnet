using System.Linq.Expressions;
using Newtonsoft.Json;

namespace Realms
{
    internal class OrderByClauseVisitor
    {
        private OrderbyClause _orderByClause;

        public OrderByClauseVisitor()
        {
            _orderByClause = new OrderbyClause();
        }

        public OrderbyClause VisitOrderBy(LambdaExpression orderByClause)
        {
            _orderByClause.OrderingNode = ParseExpression(orderByClause.Body);
            var json = JsonConvert.SerializeObject(_orderByClause, formatting: Formatting.Indented);
            return _orderByClause;
        }

        private OrderingNode ParseExpression(Expression exp)
        {
            var orderingNode = new OrderingNode();
            orderingNode.IsAscending = true;
            orderingNode.IsReplacing = true;
            if (exp is MemberExpression me)
            {
                orderingNode.Property = me.Member.Name.ToString();
            }

            return orderingNode;
        }

        //private static bool IsSortClause(string methodName, out bool isAscending, out bool isReplacing)
        //{
        //    switch (methodName)
        //    {
        //        case nameof(Queryable.OrderBy):
        //            isAscending = true;
        //            isReplacing = true;
        //            return true;
        //        case nameof(Queryable.ThenBy):
        //            isAscending = true;
        //            isReplacing = false;
        //            return true;
        //        case nameof(Queryable.OrderByDescending):
        //            isAscending = false;
        //            isReplacing = true;
        //            return true;
        //        case nameof(Queryable.ThenByDescending):
        //            isAscending = false;
        //            isReplacing = false;
        //            return true;
        //        default:
        //            isAscending = false;
        //            isReplacing = false;
        //            return false;
        //    }
        //}
    }
}