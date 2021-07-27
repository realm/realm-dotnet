namespace Realms
{
    internal class OrderByClauseVisitor
    {
        private OrderbyClause _orderByClause;

        public OrderByClauseVisitor()
        {
            _orderByClause = new OrderbyClause();
        }

        public OrderbyClause VisitOrderBy(string methodName, bool isAscending, bool isReplacing)
        {
            _orderByClause.Property = methodName;
            _orderByClause.IsAscending = isAscending;
            _orderByClause.IsReplacing = isReplacing;
            return _orderByClause;
        }
    }
}