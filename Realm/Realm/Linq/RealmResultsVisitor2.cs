using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Realms
{
    internal class RealmResultsVisitor2 : ExpressionVisitor
    {
        private readonly Realm _realm;
        private readonly RealmObjectBase.Metadata _metadata;
        private readonly QueryModel _query;
        private readonly List<RealmValue> _arguments;

        private IQueryableCollection results;
        private int _argumentsCounter;

        internal RealmResultsVisitor2(Realm realm, RealmObjectBase.Metadata metadata)
        {
            _realm = realm;
            _metadata = metadata;
            _arguments = new List<RealmValue>();
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
                    _query.WhereClauses.Add(whereClauseVisitor.VisitWhere(node.Arguments[1], _argumentsCounter, out var whereArguments));
                    _argumentsCounter += whereArguments.Count;
                    _arguments.AddRange(whereArguments);
                    return node;
                }

                if (IsSortClause(node.Method.Name))
                {
                    Visit(node.Arguments[0]);
                    var sortClauseVisitor = new SortClauseVisitor();
                    _query.OrderingClauses.Add(sortClauseVisitor.VisitOrderClause(node));
                    return node;
                }

                throw new NotSupportedException($"The method call '{node.Method.Name}' is not supported");
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
            var query = results.GetQuery(json, _arguments.ToArray());
            return query.CreateResultsNew(_realm.SharedRealmHandle);
        }
    }
}
