////////////////////////////////////////////////////////////////////////////
//
// Copyright 2018 Realm Inc.
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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Realms.Helpers
{
    internal static class LinqHelper
    {
        public static string[] ToStringPaths<TSource, TProperty>(this Expression<Func<TSource, TProperty>>[] expressions)
        {
            return expressions.Select(ToStringPath).ToArray();
        }

        public static string ToStringPath<TSource, TResult>(this Expression<Func<TSource, TResult>> expression)
        {
            var visitor = new PropertyVisitor();
            visitor.Visit(expression.Body);
            visitor.Path.Reverse();
            return string.Join(".", visitor.Path);
        }

        private class PropertyVisitor : ExpressionVisitor
        {
            public List<string> Path { get; } = new List<string>();

            protected override Expression VisitMember(MemberExpression node)
            {
                if (!(node.Member is PropertyInfo))
                {
                    throw new ArgumentException("The path can only contain properties", nameof(node));
                }

                Path.Add(node.Member.Name);
                return base.VisitMember(node);
            }
        }
    }
}
