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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Realms.Schema
{
    internal static class PropertyTypeEx
    {
        public static PropertyType ToPropertyType(this Type type, out bool isNullable, out Type innerType)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            innerType = null;

            var nullableType = Nullable.GetUnderlyingType(type);
            isNullable = nullableType != null;
            if (isNullable)
            {
                type = nullableType;
            }

            if (type == typeof(bool))
            {
                return PropertyType.Bool;
            }

            if (type == typeof(char) ||
                type == typeof(byte) ||
                type == typeof(short) ||
                type == typeof(int) ||
                type == typeof(long) ||
                type == typeof(RealmInteger<byte>) ||
                type == typeof(RealmInteger<short>) ||
                type == typeof(RealmInteger<int>) ||
                type == typeof(RealmInteger<long>))
            {
                return PropertyType.Int;
            }

            if (type == typeof(float))
            {
                return PropertyType.Float;
            }

            if (type == typeof(double))
            {
                return PropertyType.Double;
            }

            if (type == typeof(string))
            {
                isNullable = true;
                return PropertyType.String;
            }

            if (type == typeof(DateTimeOffset))
            {
                return PropertyType.Date;
            }

            if (type == typeof(byte[]))
            {
                isNullable = true;
                return PropertyType.Data;
            }

            if (type.GetTypeInfo().BaseType == typeof(RealmObject))
            {
                isNullable = true;
                innerType = type;
                return PropertyType.Object;
            }

            if (type.GetTypeInfo().IsGenericType)
            {
                var definition = type.GetGenericTypeDefinition();
                if (definition == typeof(IList<>))
                {
                    innerType = type.GetGenericArguments().Single();
                    return PropertyType.Array;
                }
            }

            throw new ArgumentException($"The property type {type.Name} cannot be expressed as a Realm schema type", nameof(type));
        }

        public static bool IsComputed(this PropertyType propertyType)
        {
            return propertyType == PropertyType.LinkingObjects;
        }
    }
}
