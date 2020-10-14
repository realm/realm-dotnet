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
using Realms.Helpers;

namespace Realms.DataBinding
{
    internal static class TypeInfoHelper
    {
        // Holds Type -> RealmObjectTypeInfo map to avoid creating a new TypeDelegator for each IReflectableType.GetTypeInfo invocation.
        private static readonly ConcurrentDictionary<Type, RealmObjectTypeDelegator> TypeCache = new ConcurrentDictionary<Type, RealmObjectTypeDelegator>();

        public static TypeInfo GetInfo(RealmObjectBase obj)
        {
            Argument.NotNull(obj, nameof(obj));
            return TypeCache.GetOrAdd(obj.GetType(), t => new RealmObjectTypeDelegator(t));
        }
    }
}
