////////////////////////////////////////////////////////////////////////////
//
// Copyright 2017 Realm Inc.
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
using System.Linq.Expressions;

namespace Realms.Helpers
{
    // Heavily based on http://www.yoda.arachsys.com/csharp/miscutil/index.html
    internal static class Operator
    {
        public static T Add<T>(T first, T second)
        {
            return GenericOperator<T, T>.Add(first, second);
        }

        public static TResult Convert<TFrom, TResult>(TFrom value)
        {
            return GenericOperator<TFrom, TResult>.Convert(value);
        }

        private static class GenericOperator<T, U>
        {
            public static Func<T, U, T> Add { get; }

            public static Func<T, U> Convert { get; }

            static GenericOperator()
            {
                Add = CreateAdd();
                Convert = CreateConvert();
            }

            private static Func<T, U, T> CreateAdd()
            {
                var lhs = Expression.Parameter(typeof(T), "lhs");
                var rhs = Expression.Parameter(typeof(U), "rhs");
                try
                {
                    return Expression.Lambda<Func<T, U, T>>(Expression.Add(lhs, rhs), lhs, rhs).Compile();
                }
                catch (Exception ex)
                {
                    var message = ex.Message;
                    return delegate { throw new InvalidOperationException(message); };
                }
            }

            private static Func<T, U> CreateConvert()
            {
                var input = Expression.Parameter(typeof(T), "input");
                try
                {
                    return Expression.Lambda<Func<T, U>>(Expression.Convert(input, typeof(U)), input).Compile();
                }
                catch (Exception ex)
                {
                    string msg = ex.Message; // avoid capture of ex itself
                    return delegate { throw new InvalidOperationException(msg); };
                }
            }
        }
    }
}