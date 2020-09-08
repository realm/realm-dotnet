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
using MongoDB.Bson;
using Realms.Helpers;

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

        public static PropertyType ToPropertyType(this Type type, out Type objectType)
        {
            Argument.NotNull(type, nameof(type));

            objectType = null;
            var nullabilityModifier = PropertyType.Required;

            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
            {
                type = nullableType;
                nullabilityModifier = PropertyType.Nullable;
            }

            switch (type)
            {
                case Type _ when type.IsRealmInteger():
                    return PropertyType.Int | nullabilityModifier;

                case Type _ when type == typeof(bool):
                    return PropertyType.Bool | nullabilityModifier;

                case Type _ when type == typeof(string):
                    return PropertyType.String | PropertyType.Nullable;

                case Type _ when type == typeof(byte[]):
                    return PropertyType.Data | PropertyType.Nullable;

                case Type _ when type == typeof(DateTimeOffset):
                    return PropertyType.Date | nullabilityModifier;

                case Type _ when type == typeof(float):
                    return PropertyType.Float | nullabilityModifier;

                case Type _ when type == typeof(double):
                    return PropertyType.Double | nullabilityModifier;

                case Type _ when type == typeof(decimal) || type == typeof(Decimal128):
                    return PropertyType.Decimal | nullabilityModifier;

                case Type _ when type == typeof(ObjectId):
                    return PropertyType.ObjectId | nullabilityModifier;

                case Type _ when type == typeof(RealmObject) || type.GetTypeInfo().BaseType == typeof(RealmObject) ||
                                 type == typeof(EmbeddedObject) || type.GetTypeInfo().BaseType == typeof(EmbeddedObject):
                    objectType = type;
                    return PropertyType.Object | PropertyType.Nullable;

                case Type _ when type.IsClosedGeneric(typeof(IList<>), out var typeArguments):
                    var result = PropertyType.Array | typeArguments.Single().ToPropertyType(out objectType);

                    if (result.HasFlag(PropertyType.Object))
                    {
                        // List<Object> can't contain nulls
                        result &= ~PropertyType.Nullable;
                    }

                    return result;

                default:
                    throw new ArgumentException($"The property type {type.Name} cannot be expressed as a Realm schema type", nameof(type));
            }
        }

        public static bool IsComputed(this PropertyType propertyType) => propertyType == (PropertyType.LinkingObjects | PropertyType.Array);

        public static bool IsNullable(this PropertyType propertyType) => propertyType.HasFlag(PropertyType.Nullable);

        public static bool IsArray(this PropertyType propertyType) => propertyType.HasFlag(PropertyType.Array);

        public static PropertyType UnderlyingType(this PropertyType propertyType) => propertyType & ~PropertyType.Flags;
    }
}
