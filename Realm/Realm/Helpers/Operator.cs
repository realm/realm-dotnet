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
using MongoDB.Bson;

namespace Realms.Helpers
{
    // Heavily based on https://web.archive.org/web/20181011184205/http://www.yoda.arachsys.com/csharp/miscutil/index.html
    internal static class Operator
    {
        [Preserve]
        static Operator()
        {
            _ = (decimal)new Decimal128(123);
        }

        public static T Add<T>(T first, T second)
        {
            return GenericOperator<T, T>.Add(first, second);
        }

        public static TResult Convert<TFrom, TResult>(TFrom value)
        {
            /**
             * This is special cased due to a bug in the Xamarin.iOS interpreter. When value
             * is null, we end up with a NRE with the following stacktrace:
             *
             * <System.NullReferenceException: Object reference not set to an instance of an object
             * at System.Linq.Expressions.Interpreter.LightLambda.Run1[T0,TRet] (T0 arg0) [0x00038] in <ee28ffe65f2e47a98ea97b07327fb8f4>:0
             * at (wrapper delegate-invoke) System.Func`2[System.String,Realms.RealmValue].invoke_TResult_T(string)
             * at Realms.Helpers.Operator.Convert[TFrom,TResult] (TFrom value) [0x00005] in <675c1cc840764fcb9ab78b319ccfeee3>:0
             * at Realms.RealmList`1[T].<.ctor>b__5_1 (T item) [0x00000] in <675c1cc840764fcb9ab78b319ccfeee3>:0
             * at Realms.RealmList`1[T].Add (T item) [0x00000] in <675c1cc840764fcb9ab78b319ccfeee3>:0
             *
             * May or may not be related to https://github.com/mono/mono/issues/15852.
             */
            if (value is null && typeof(TResult) == typeof(RealmValue))
            {
                return Convert<RealmValue, TResult>(RealmValue.Null());
            }

            return GenericOperator<TFrom, TResult>.Convert(value);
        }

        private static class GenericOperator<T1, T2>
        {
            private static readonly Lazy<Func<T1, T2, T1>> _add;
            private static readonly Lazy<Func<T1, T2>> _convert;

            public static Func<T1, T2, T1> Add => _add.Value;

            public static Func<T1, T2> Convert => _convert.Value;

            static GenericOperator()
            {
                _add = new Lazy<Func<T1, T2, T1>>(CreateAdd);
                _convert = new Lazy<Func<T1, T2>>(CreateConvert);
            }

            private static Func<T1, T2, T1> CreateAdd()
            {
                var lhs = Expression.Parameter(typeof(T1), "lhs");
                var rhs = Expression.Parameter(typeof(T2), "rhs");
                try
                {
                    if (typeof(T1) == typeof(byte))
                    {
                        // Add is not defined for byte...
                        var addExpression = Expression.Add(Expression.Convert(lhs, typeof(int)), Expression.Convert(rhs, typeof(int)));
                        return Expression.Lambda<Func<T1, T2, T1>>(Expression.Convert(addExpression, typeof(byte)), lhs, rhs).Compile();
                    }

                    return Expression.Lambda<Func<T1, T2, T1>>(Expression.Add(lhs, rhs), lhs, rhs).Compile();
                }
                catch (Exception ex)
                {
                    var message = ex.Message;
                    return (_, __) => { throw new InvalidOperationException(message); };
                }
            }

            private static Func<T1, T2> CreateConvert()
            {
                var input = Expression.Parameter(typeof(T1), "input");
                try
                {
                    Expression convertFrom = input;
                    var typeOfT1 = typeof(T1);
                    var isT1Nullable = false;
                    if (typeOfT1.IsClosedGeneric(typeof(Nullable<>), out var arguments))
                    {
                        typeOfT1 = arguments.Single();
                        isT1Nullable = true;
                    }

                    if (typeOfT1.IsClosedGeneric(typeof(RealmInteger<>), out arguments))
                    {
                        var intermediateType = arguments.Single();
                        if (isT1Nullable)
                        {
                            intermediateType = typeof(Nullable<>).MakeGenericType(intermediateType);
                        }

                        convertFrom = Expression.Convert(input, intermediateType);
                    }

                    var typeOfT2 = typeof(T2);
                    var isT2Nullable = false;
                    if (typeOfT2.IsClosedGeneric(typeof(Nullable<>), out arguments))
                    {
                        typeOfT2 = arguments.Single();
                        isT2Nullable = true;
                    }

                    if (typeOfT2.IsClosedGeneric(typeof(RealmInteger<>), out arguments))
                    {
                        var intermediateType = arguments.Single();
                        if (isT2Nullable)
                        {
                            intermediateType = typeof(Nullable<>).MakeGenericType(intermediateType);
                        }

                        convertFrom = Expression.Convert(input, intermediateType);
                    }

                    return Expression.Lambda<Func<T1, T2>>(Expression.Convert(convertFrom, typeof(T2)), input).Compile();
                }
                catch (Exception ex)
                {
                    var msg = ex.Message; // avoid capture of ex itself
                    return _ => { throw new InvalidOperationException(msg); };
                }
            }
        }
    }
}