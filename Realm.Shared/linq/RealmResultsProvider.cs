////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////
 
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Parsing.ExpressionVisitors;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;

namespace Realms
{
    internal class RealmResultsProvider : IQueryProvider
    {
        private Realm _realm;
        private readonly Schema.ObjectSchema _schema;

        internal RealmResultsProvider(Realm realm, Schema.ObjectSchema schema)
        {
            _realm = realm;
            _schema = schema;
        }

        internal RealmResultsVisitor MakeVisitor()
        {
            return new RealmResultsVisitor(_realm, _schema);
        }

        public IQueryable<T> CreateQuery<T>(Expression expression)
        {
            return new RealmResults<T>(_realm, this, expression, _schema, false);
        }

        public IQueryable CreateQuery(Expression expression)
        {
            Type elementType = TypeSystem.GetElementType(expression.Type);
            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(RealmResults<>).MakeGenericType(elementType), new object[] { this, expression });
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        public T Execute<T>(Expression expression)
        {
            expression = PartialEvaluatingExpressionVisitor.EvaluateIndependentSubtrees(expression, new EvaluatableExpressionFilter());
            var v = MakeVisitor();
            Expression visitResult = v.Visit(expression);
            var constExp = visitResult as ConstantExpression;
            T ret = (T)constExp?.Value;
            return ret;
        }

        public object Execute(Expression expression)
        {
            throw new Exception("Non-generic Execute() called...");
        }

        private class EvaluatableExpressionFilter : EvaluatableExpressionFilterBase
        {
        }

    }
}