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
using System.Linq;
using System.Reflection;
using Realms.Schema;

namespace Realms.DataBinding
{
    internal class RealmObjectTypeDelegator : TypeDelegator
    {
        // Holds property name -> PropertyInfo map to avoid creating a new WovenPropertyInfo for each GetDeclaredProperty call.
        private readonly ConcurrentDictionary<string, PropertyInfo> _propertyCache = new ConcurrentDictionary<string, PropertyInfo>();

        private readonly ObjectSchema _schema;

        internal RealmObjectTypeDelegator(Type type, ObjectSchema schema) : base(type)
        {
            _schema = schema;
        }

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            var result = base.GetPropertyImpl(name, bindingAttr, binder, returnType, types, modifiers);

            if (result == null)
            {
                return null;
            }

            if (_schema != null)
            {
                if (_schema.Any(p => p.ManagedName == name))
                {
                    return _propertyCache.GetOrAdd(name, n => new WovenPropertyInfo(result));
                }

                return result;
            }

            // Schema is null only for woven unmanaged objects
            if (result?.GetCustomAttribute<WovenPropertyAttribute>() != null)
            {
                return _propertyCache.GetOrAdd(name, n => new WovenPropertyInfo(result));
            }

            return result;
        }
    }
}
