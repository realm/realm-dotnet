using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using LazyMethod = System.Lazy<System.Reflection.MethodInfo>;

namespace Realms
{
    internal class RealmResultsVisitor2 : ExpressionVisitor
    {
        private readonly Realm _realm;
        private readonly RealmObjectBase.Metadata _metadata;
        private readonly QueryModel _query;

        private IQueryableCollection results;

        internal RealmResultsVisitor2(Realm realm, RealmObjectBase.Metadata metadata)
        {
            _realm = realm;
            _metadata = metadata;
            _query = new QueryModel();
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable))
            {
                if (node.Method.Name == nameof(Queryable.Where))
                {
                    Visit(node.Arguments[0]);
                    var whereClauseVisitor = new WhereClauseVisitor(_metadata);
                    var lambda = (LambdaExpression)StripQuotes(node.Arguments[1]);
                    _query.WhereClauses.Add(whereClauseVisitor.VisitWhere(lambda));
                    return node;
                }

                if (IsSortClause(node.Method.Name))
                {
                    Visit(node.Arguments[0]);
                    var sortClauseVisitor = new SortClauseVisitor();
                    _query.OrderingClauses.Add(sortClauseVisitor.VisitOrderClause(node));
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

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value is IQueryableCollection)
            {
                results = node.Value as IQueryableCollection;
            }
            else if (node.Value?.GetType() == typeof(object))
            {
                throw new NotSupportedException($"The constant for '{node.Value}' is not supported");
            }

            return node;
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
            string json = JsonConvert.SerializeObject(_query, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            });
            var query = results.GetQuery(json);
            return query.CreateResultsNew(_realm.SharedRealmHandle);
        }
    }
}
