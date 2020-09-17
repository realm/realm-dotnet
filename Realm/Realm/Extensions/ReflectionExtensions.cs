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
using System.Reflection;

namespace Realms
{
    internal static class ReflectionExtensions
    {
        public static bool IsClosedGeneric(this Type type, Type genericType, out Type[] arguments)
        {
            arguments = null;

            if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == genericType)
            {
                arguments = type.GenericTypeArguments;
                return true;
            }

            return false;
        }

        public static bool IsStatic(this PropertyInfo property) => property.GetAccessors(true)[0].IsStatic;

        public static bool HasCustomAttribute<T>(this MemberInfo member)
            where T : Attribute
            => member.CustomAttributes.Any(a => a.AttributeType == typeof(T));

        public static string GetMappedOrOriginalName(this MemberInfo member) => member?.GetCustomAttribute<MapToAttribute>()?.Mapping ?? member?.Name;

        public static bool IsEmbeddedObject(this Type type) => type == typeof(EmbeddedObject) || type.BaseType == typeof(EmbeddedObject);

        public static bool IsRealmObject(this Type type) => type == typeof(RealmObject) || type.BaseType == typeof(RealmObject);
    }
}
