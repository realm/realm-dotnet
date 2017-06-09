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
    internal static class Operator<T>
    {
        public static Func<T, T, T> Add { get; }

        static Operator()
        {
            var lhs = Expression.Parameter(typeof(T), "lhs");
            var rhs = Expression.Parameter(typeof(T), "rhs");
            try
            {
                Add = Expression.Lambda<Func<T, T, T>>(Expression.Add(lhs, rhs), lhs, rhs).Compile();
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                Add = delegate { throw new InvalidOperationException(message); };
            }
        }
    }
}