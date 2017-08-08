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
        private static readonly HashSet<Type> _integerTypes =
            new HashSet<Type> { typeof(char), typeof(byte), typeof(short), typeof(int), typeof(long) };

        public static bool IsRealmInteger(this Type type)
        {
            if (type.IsClosedGeneric(typeof(RealmInteger<>), out var typeArguments))
            {
                return IsRealmInteger(typeArguments.Single());
            }

            return _integerTypes.Contains(type);
        }

        public static PropertyType ToPropertyType(this Type type, out Type innerType)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            innerType = null;
            var nullabilityModifier = PropertyType.Required;

            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
            {
                type = nullableType;
                nullabilityModifier = PropertyType.Nullable;
            }

            switch (type)
            {
                case Type integerType when integerType.IsRealmInteger():
                    return PropertyType.Int | nullabilityModifier;

                case Type boolType when boolType == typeof(bool):
                    return PropertyType.Bool | nullabilityModifier;

                case Type stringType when stringType == typeof(string):
                    return PropertyType.String | PropertyType.Nullable;

                case Type dataType when dataType == typeof(byte[]):
                    return PropertyType.Data | PropertyType.Nullable;

                case Type dateType when dateType == typeof(DateTimeOffset):
                    return PropertyType.Date | nullabilityModifier;

                case Type floatType when floatType == typeof(float):
                    return PropertyType.Float | nullabilityModifier;

                case Type doubleType when doubleType == typeof(double):
                    return PropertyType.Double | nullabilityModifier;

                case Type objectType when objectType.GetTypeInfo().BaseType == typeof(RealmObject):
                    return PropertyType.Object | PropertyType.Nullable;

                case Type listType when listType.IsClosedGeneric(typeof(IList<>), out var typeArguments):
                    innerType = typeArguments.Single();
                    return PropertyType.Object | PropertyType.Array;

                default:
                    throw new ArgumentException($"The property type {type.Name} cannot be expressed as a Realm schema type", nameof(type));
            }
        }

        public static bool IsComputed(this PropertyType propertyType) => propertyType.HasFlag(PropertyType.LinkingObjects);

        public static bool IsNullable(this PropertyType propertyType) => propertyType.HasFlag(PropertyType.Nullable);

        public static bool IsArray(this PropertyType propertyType) => propertyType.HasFlag(PropertyType.Array);
    }
}
