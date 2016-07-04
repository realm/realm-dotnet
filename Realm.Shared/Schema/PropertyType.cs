﻿////////////////////////////////////////////////////////////////////////////
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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Realms.Schema
{
    internal enum PropertyType : byte
    {
        Int = 0,
        Bool = 1,
        Float = 9,
        Double = 10,
        String = 2,
        Data = 4,
        Any = 6,
        Date = 8,
        Object = 12,
        Array = 13
    }

    internal static class PropertyTypeEx
    {
        public static PropertyType ToPropertyType(this Type type, out bool isNullable, out Type innerType)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            Contract.EndContractBlock();

            innerType = null;

            var nullableType = Nullable.GetUnderlyingType(type);
            isNullable = nullableType != null;
            if (isNullable)
                type = nullableType;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return PropertyType.Bool;
                case TypeCode.Char:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return PropertyType.Int;
                case TypeCode.Single:
                    return PropertyType.Float;
                case TypeCode.Double:
                    return PropertyType.Double;
                case TypeCode.String:
                    isNullable = true;
                    return PropertyType.String;
            }

            if (type == typeof(DateTimeOffset))
                return PropertyType.Date;

            if (type == typeof(byte[]))
            {
                isNullable = true;
                return PropertyType.Data;
            }

            if (type.BaseType == typeof(RealmObject))
            {
                isNullable = true;
                innerType = type;
                return PropertyType.Object;
            }

            if (type.IsGenericType)
            {
                var definition = type.GetGenericTypeDefinition();
                if (definition == typeof(RealmList<>))
                {
                    innerType = type.GetGenericArguments().Single();
                    return PropertyType.Array;
                }
            }

            throw new ArgumentException($"The property type {type.Name} cannot be expressed as a Realm schema type", nameof(type));
        }
    }
}

