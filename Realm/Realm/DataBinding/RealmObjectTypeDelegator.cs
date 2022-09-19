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

namespace Realms.DataBinding
{
    internal class RealmObjectTypeDelegator : TypeDelegator
    {
        // Holds property name -> PropertyInfo map to avoid creating a new WovenPropertyInfo for each GetDeclaredProperty call.
        private readonly ConcurrentDictionary<string, PropertyInfo> _propertyCache = new ConcurrentDictionary<string, PropertyInfo>();

        internal RealmObjectTypeDelegator(Type type) : base(type)
        {
        }

        //TODO The problem here is that this works only for properties that have the WovenAttribute (I suppose to avoid creating an implementation
        //for properties we don't care about). Probably we should add something similar for the generated properties.
        //Maybe we can check if it's the same name of a property that's on the interface (that we pass in the Generated Attribute)
        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            var result = base.GetPropertyImpl(name, bindingAttr, binder, returnType, types, modifiers);
            var wovenAttribute = result?.GetCustomAttribute<WovenPropertyAttribute>();
            if (wovenAttribute != null)
            {
                return _propertyCache.GetOrAdd(name, n => new WovenPropertyInfo(result));
            }

            return result;
        }
    }
}
