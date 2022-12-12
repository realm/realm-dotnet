// ////////////////////////////////////////////////////////////////////////////
// //
// // Copyright 2022 Realm Inc.
// //
// // Licensed under the Apache License, Version 2.0 (the "License")
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// // http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //
// ////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Realms
{
    public abstract class ThreadSafeReference
    {
        public static Query<T> Create<T>(IQueryable<T> value) => default;

        public static Object<T> Create<T>(T value) where T : IRealmObjectBase => default;

        public static List<T> Create<T>(IList<T> value) => default;

        public static Dictionary<TValue> Create<TValue>(IDictionary<string, TValue> value) => default;

        public class Object<T> : ThreadSafeReference
            where T : IRealmObjectBase
        { }

        public class Query<T> : ThreadSafeReference
        { }

        public class Dictionary<TValue> : ThreadSafeReference
        { }
    }
}
