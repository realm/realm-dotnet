using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Realms
{
    internal class RealmResultsVisitor2 : ExpressionVisitor
    {
        private readonly Realm _realm;
        private readonly RealmObjectBase.Metadata _metadata;
        private QueryModel _query;

        private IQueryableCollection _results;
        private QueryHandle _queryHandle;

        internal RealmResultsVisitor2(Realm realm, RealmObjectBase.Metadata metadata)
        {
            _realm = realm;
            _metadata = metadata;
            _query = new QueryModel();

            // TODO: Discuss this with Ferdinando since this can allow us to not have empty lists
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
                    var sortClause = new SortClauseVisitor();
                    _query.OrderingClauses.Add(sortClause.VisitOrderClause(node));
                    return node;
                }

                if (node.Method.Name.StartsWith(nameof(Queryable.ElementAt), StringComparison.OrdinalIgnoreCase))
                {
                    Visit(node.Arguments.First());
                    if (!TryExtractConstantValue(node.Arguments.Last(), out object argument) || argument.GetType() != typeof(int))
                    {
                        throw new NotSupportedException($"The method '{node.Method}' has to be invoked with a single integer constant argument or closure variable");
                    }

                    var index = (int)argument;
                    using var rh = MakeResultsForQuery();

                    return GetObjectAtIndex(index, rh, node.Method.Name);
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

        private Expression GetObjectAtIndex(int index, ResultsHandle rh, string methodName)
        {
            try
            {
                var val = rh.GetValueAtIndex(index, _realm);
                return Expression.Constant(val.AsRealmObject());
            }
            catch (ArgumentOutOfRangeException ex)
            {
                if (methodName.EndsWith("OrDefault", StringComparison.OrdinalIgnoreCase))
                {
                    // For First/Last/Single/ElemetAtOrDefault - ignore
                    return Expression.Constant(null);
                }

                if (methodName == nameof(Queryable.ElementAt))
                {
                    // ElementAt should rethrow the ArgumentOutOfRangeException.
                    throw;
                }

                // All the rest should throw an InvalidOperationException.
                throw new InvalidOperationException("Sequence contains no matching element", ex);
            }
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

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value is IQueryableCollection)
            {
                _results = node.Value as IQueryableCollection;
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
            if (_queryHandle == null)
            {
                DefaultContractResolver contractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                };
                var jsonQuery = JsonConvert.SerializeObject(_query, new JsonSerializerSettings
                {
                    ContractResolver = contractResolver,
                    Formatting = Formatting.Indented
                });
                _queryHandle = _results.GetQuery(jsonQuery);
            }

            //So, I'm not really convinced about this... I don't like the idea to keep state in the visitor, even though it definitely works
            //It seems that RealmResults also are live and updating, so I could even cache that
            //Methods like Count/Sum/Min/Max just need the query cached.
            //Methods like First/Single/ElementAt need the results cached.
            //Do we have issues with that (memory wise)...?
            //It seems that when we enumerate a realmResult (like with foreach) we create a new realmResult that is connected to a snapshot of the realmHandle, so that the collection is frozen
            //This means that we could keep the realmResults? Even though I have a feeling we're going in circles

            return _queryHandle.CreateResultsNew(_realm.SharedRealmHandle);
        }
    }
}
