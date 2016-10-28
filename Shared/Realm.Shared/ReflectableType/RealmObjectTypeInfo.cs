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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Realms
{
    internal class RealmObjectTypeInfo : TypeDelegator
    {
        private static readonly ConcurrentDictionary<Type, RealmObjectTypeInfo> typeCache = new ConcurrentDictionary<Type, RealmObjectTypeInfo>();
        private readonly IDictionary<string, PropertyInfo> _cache = new Dictionary<string, PropertyInfo>();

        public static TypeInfo FromType(Type type)
        {
            return typeCache.GetOrAdd(type, t => new RealmObjectTypeInfo(t));
        }

        private RealmObjectTypeInfo(Type type) : base(type)
        {
        }

        public override PropertyInfo GetDeclaredProperty(string name)
        {
            PropertyInfo result;
            if (!_cache.TryGetValue(name, out result))
            {
                result = base.GetDeclaredProperty(name);
                var wovenAttribute = result.GetCustomAttribute<WovenPropertyAttribute>();
                if (wovenAttribute != null)
                {
                    result = new WovenPropertyInfo(result);
                }

                _cache[name] = result;
            }

            return result;
        }
    }
}