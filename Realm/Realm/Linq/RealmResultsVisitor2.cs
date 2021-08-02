using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;

namespace Realms
{
    internal class RealmResultsVisitor2 : ExpressionVisitor
    {
        private readonly Realm _realm;
        private readonly RealmObjectBase.Metadata _metadata;
        private QueryModel _query;

        // TODO Remove these fields when the CreateResults method is rewritten
        private QueryHandle _coreQueryHandle;
        private SortDescriptorHandle _sortDescriptor;

        internal RealmResultsVisitor2(Realm realm, RealmObjectBase.Metadata metadata)
        {
            _realm = realm;
            _metadata = metadata;
            _query = new QueryModel();
            _query.WhereClauses = new List<WhereClause>();
            _query.OrderingClauses = new List<OrderingClause>();
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable))
            {
                if (node.Method.Name == nameof(Queryable.Where))
                {
                    Visit(node.Arguments[0]);
                    var whereClause = new WhereClauseVisitor(_metadata);
                    var lambda = (LambdaExpression)StripQuotes(node.Arguments[1]);
                    _query.WhereClauses.Add(whereClause.VisitWhere(lambda));
                    return node;
                }

                if (IsSortClause(node.Method.Name))
                {
                    Visit(node.Arguments[0]);
                    // Fix SOrtClause naming
                    var sortClause = new SortClause();
                    _query.OrderingClauses.Add(sortClause.VisitOrderClause(node));
                    return node;
                }
                else
                {
                    throw new NotSupportedException($"The method call '{node.Method.Name}' is not supported");
                }
            }
            else
            {
                throw new NotSupportedException($"The method '{node.Method.Name}' is not supported");
            }
        }

        internal static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }

            return e;
        }

        private static bool IsSortClause(string methodName)
        {
            return methodName == nameof(Queryable.OrderBy)
                || methodName == nameof(Queryable.ThenBy)
                || methodName == nameof(Queryable.OrderByDescending)
                || methodName == nameof(Queryable.ThenByDescending);
        }

        public ResultsHandle MakeResultsForQuery()
        {
            var json = JsonConvert.SerializeObject(_query, formatting: Formatting.Indented);
            return _coreQueryHandle.CreateResults(_realm.SharedRealmHandle, _sortDescriptor);
        }
    }
}
