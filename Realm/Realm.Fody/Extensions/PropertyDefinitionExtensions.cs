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

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Mono.Cecil;
using Mono.Cecil.Cil;

using static ModuleWeaver;

[EditorBrowsable(EditorBrowsableState.Never)]
internal static class PropertyDefinitionExtensions
{
    private static readonly Regex NullableRegex = new Regex("^System.Nullable`1<(?<typeName>.*)>$");

    private static readonly IEnumerable<string> _indexableTypes = new[]
    {
        StringTypeName,
        CharTypeName,
        ByteTypeName,
        Int16TypeName,
        Int32TypeName,
        Int64TypeName,
        BooleanTypeName,
        DateTimeOffsetTypeName
    };

    internal static bool IsAutomatic(this PropertyDefinition property)
    {
        return property.GetMethod.CustomAttributes.Any(attr => attr.AttributeType.FullName == typeof(CompilerGeneratedAttribute).FullName);
    }

    internal static bool IsIList(this PropertyDefinition property)
    {
        return property.IsType("IList`1", "System.Collections.Generic");
    }

    internal static bool IsIList(this PropertyDefinition property, TypeReference elementType)
    {
        return IsIList(property) && ((GenericInstanceType)property.PropertyType).GenericArguments.Single().IsSameAs(elementType);
    }

    internal static bool IsIList(this PropertyDefinition property, System.Type elementType)
    {
        return IsIList(property) && ((GenericInstanceType)property.PropertyType).GenericArguments.Single().FullName == elementType.FullName;
    }

    internal static bool IsIQueryable(this PropertyDefinition property)
    {
        return property.IsType("IQueryable`1", "System.Linq");
    }

    internal static bool IsDateTimeOffset(this PropertyDefinition property)
    {
        return property.PropertyType.FullName == DateTimeOffsetTypeName;
    }

    internal static bool IsNullable(this PropertyDefinition property)
    {
        return property.PropertyType.IsNullable();
    }

    internal static bool IsNullable(this TypeReference reference)
    {
        return NullableRegex.IsMatch(reference.FullName);
    }

    internal static bool IsSingle(this PropertyDefinition property)
    {
        return property.PropertyType.FullName == SingleTypeName;
    }

    internal static bool IsDouble(this PropertyDefinition property)
    {
        return property.PropertyType.FullName == DoubleTypeName;
    }

    internal static bool IsDecimal(this PropertyDefinition property)
    {
        return property.PropertyType.FullName == DecimalTypeName;
    }

    internal static bool IsDecimal128(this PropertyDefinition property)
    {
        return property.PropertyType.FullName == Decimal128TypeName;
    }

    internal static bool IsString(this PropertyDefinition property)
    {
        return property.PropertyType.FullName == StringTypeName;
    }

    internal static bool IsDescendantOf(this PropertyDefinition property, TypeReference other)
    {
        return property.PropertyType.Resolve().BaseType.IsSameAs(other);
    }

    internal static FieldReference GetBackingField(this PropertyDefinition property)
    {
        return property.GetMethod.Body.Instructions
            .Where(o => o.OpCode == OpCodes.Ldfld)
            .Select(o => o.Operand)
            .OfType<FieldReference>()
            .SingleOrDefault();
    }

    internal static bool IsPrimaryKey(this PropertyDefinition property)
    {
        Debug.Assert(property.DeclaringType.BaseType.Name == "RealmObject", "Primary key properties only make sense on RealmObject classes");
        return property.CustomAttributes.Any(a => a.AttributeType.Name == "PrimaryKeyAttribute");
    }

    internal static bool IsRequired(this PropertyDefinition property)
    {
        Debug.Assert(property.DeclaringType.BaseType.Name == "RealmObject", "Required properties only make sense on RealmObject classes");
        return property.CustomAttributes.Any(a => a.AttributeType.Name == "RequiredAttribute");
    }

    internal static bool IsIndexable(this PropertyDefinition property)
    {
        Debug.Assert(property.DeclaringType.BaseType.Name == "RealmObject", "Required properties only make sense on RealmObject classes");
        var propertyType = property.PropertyType;
        if (propertyType.IsRealmInteger(out var isNullable, out var backingType))
        {
            if (isNullable)
            {
                return false;
            }

            propertyType = backingType;
        }

        return _indexableTypes.Contains(propertyType.FullName);
    }

    public static bool IsRealmInteger(this TypeReference type, out bool isNullable, out TypeReference genericArgumentType)
    {
        var nullableMatch = NullableRegex.Match(type.FullName);
        isNullable = nullableMatch.Success;
        if (isNullable)
        {
            var genericType = (GenericInstanceType)type;
            type = genericType.GenericArguments.Single();
        }

        var result = type.Name == "RealmInteger`1" && type.Namespace == "Realms";
        if (result)
        {
            var genericType = (GenericInstanceType)type;
            genericArgumentType = genericType.GenericArguments.Single();
            return true;
        }

        genericArgumentType = null;
        return false;
    }

    private static bool IsType(this PropertyDefinition property, string name, string @namespace)
    {
        return property.PropertyType.Name == name && property.PropertyType.Namespace == @namespace;
    }
}
