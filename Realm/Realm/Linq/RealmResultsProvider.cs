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

using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Parsing.ExpressionVisitors;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;

namespace Realms
{
    internal class RealmResultsProvider : IQueryProvider
    {
        private readonly Realm _realm;
        private readonly RealmObjectBase.Metadata _metadata;

        internal RealmResultsProvider(Realm realm, RealmObjectBase.Metadata metadata)
        {
            _realm = realm;
            _metadata = metadata;
        }

        internal RealmResultsVisitor MakeVisitor()
        {
            return new RealmResultsVisitor(_realm, _metadata);
        }

        public IQueryable<T> CreateQuery<T>(Expression expression)
        {
            // If that line is changed, make sure to update the non-generic CreateQuery below!
            return new RealmResults<T>(_realm, _metadata, this, expression);
        }

        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = TypeSystem.GetElementType(expression.Type);
            try
            {
                var resultsType = typeof(RealmResults<>).MakeGenericType(elementType);
                var ctor = resultsType.GetTypeInfo().DeclaredConstructors.Single(c => c.GetParameters().Length == 4);
                return (IQueryable)ctor.Invoke(new object[] { _realm, _metadata, this, expression });
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        public T Execute<T>(Expression expression)
        {
            return (T)Execute(expression);
        }

        public object Execute(Expression expression)
        {
            expression = PartialEvaluatingExpressionVisitor.EvaluateIndependentSubtrees(expression, new EvaluatableExpressionFilter());
            var v = MakeVisitor();
            var visitResult = v.Visit(expression);
            var constExp = visitResult as ConstantExpression;
            return constExp?.Value;
        }

        private class EvaluatableExpressionFilter : EvaluatableExpressionFilterBase
        {
        }
    }
}