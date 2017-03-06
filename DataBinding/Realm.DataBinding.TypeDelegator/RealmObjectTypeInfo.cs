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
using System.Reflection;

namespace Realms
{
    internal class RealmObjectTypeInfo : TypeDelegator
    {
        // Holds property name -> PropertyInfo map to avoid creating a new WovenPropertyInfo for each GetDeclaredProperty call.
        private readonly ConcurrentDictionary<string, PropertyInfo> _propertyCache = new ConcurrentDictionary<string, PropertyInfo>();

        internal RealmObjectTypeInfo(Type type) : base(type)
        {
        }

        public override PropertyInfo GetDeclaredProperty(string name)
        {
            return _propertyCache.GetOrAdd(name, n =>
            {
                var result = base.GetDeclaredProperty(name);
                var wovenAttribute = result.GetCustomAttribute<WovenPropertyAttribute>();
                if (wovenAttribute != null)
                {
                    result = new WovenPropertyInfo(result);
                }

                return result;
            });
        }
    }
}