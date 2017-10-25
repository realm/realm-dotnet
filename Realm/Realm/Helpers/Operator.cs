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
using System.Linq;
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
            private static readonly Lazy<Func<T, U, T>> _add;
            private static readonly Lazy<Func<T, U>> _convert;

            public static Func<T, U, T> Add => _add.Value;

            public static Func<T, U> Convert => _convert.Value;

            static GenericOperator()
            {
                _add = new Lazy<Func<T, U, T>>(CreateAdd);
                _convert = new Lazy<Func<T, U>>(CreateConvert);
            }

            private static Func<T, U, T> CreateAdd()
            {
                var lhs = Expression.Parameter(typeof(T), "lhs");
                var rhs = Expression.Parameter(typeof(U), "rhs");
                try
                {
                    if (typeof(T) == typeof(byte))
                    {
                        // Add is not defined for byte...
                        var addExpression = Expression.Add(Expression.Convert(lhs, typeof(int)), Expression.Convert(rhs, typeof(int)));
                        return Expression.Lambda<Func<T, U, T>>(Expression.Convert(addExpression, typeof(byte)), lhs, rhs).Compile();
                    }
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
                    Expression convertFrom = input;
                    var typeOfT = typeof(T);
                    var isTNullable = false;
                    if (typeOfT.IsClosedGeneric(typeof(Nullable<>), out var arguments))
                    {
                        typeOfT = arguments.Single();
                        isTNullable = true;
                    }

                    if (typeOfT.IsClosedGeneric(typeof(RealmInteger<>), out arguments))
                    {
                        var intermediateType = arguments.Single();
                        if (isTNullable)
                        {
                            intermediateType = typeof(Nullable<>).MakeGenericType(intermediateType);
                        }

                        convertFrom = Expression.Convert(input, intermediateType);
                    }

                    var typeOfU = typeof(U);
                    var isUNullable = false;
                    if (typeOfU.IsClosedGeneric(typeof(Nullable<>), out arguments))
                    {
                        typeOfU = arguments.Single();
                        isUNullable = true;
                    }

                    if (typeOfU.IsClosedGeneric(typeof(RealmInteger<>), out arguments))
                    {
                        var intermediateType = arguments.Single();
                        if (isUNullable)
                        {
                            intermediateType = typeof(Nullable<>).MakeGenericType(intermediateType);
                        }

                        convertFrom = Expression.Convert(input, intermediateType);
                    }

                    return Expression.Lambda<Func<T, U>>(Expression.Convert(convertFrom, typeof(U)), input).Compile();
                }
                catch (Exception ex)
                {
                    var msg = ex.Message; // avoid capture of ex itself
                    return delegate { throw new InvalidOperationException(msg); };
                }
            }
        }
    }
}