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
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

internal static class TypeHelpers
{
    internal static TypeDefinition LookupType(string typeName, params AssemblyDefinition[] assemblies)
    {
        if (typeName == null)
        {
            throw new ArgumentNullException(nameof(typeName));
        }

        if (assemblies.Length == 0)
        {
            throw new ArgumentException("One or more assemblies must be specified to look up type: " + typeName, nameof(assemblies));
        }

        foreach (var assembly in assemblies)
        {
            var type = assembly?.MainModule.Types.FirstOrDefault(x => x.Name == typeName);

            if (type != null)
            {
                return type;
            }
        }

        throw new ApplicationException("Unable to find type: " + typeName);
    }

    internal static MethodDefinition LookupMethod(TypeDefinition typeDefinition, string methodName)
    {
        var method = typeDefinition.Methods.FirstOrDefault(x => x.Name == methodName);

        if (method == null)
        {
            throw new ApplicationException("Unable to find method: " + methodName);
        }

        return method;
    }

    internal static FieldReference GetBackingField(PropertyDefinition property)
    {
        return property.GetMethod.Body.Instructions
            .Where(o => o.OpCode == OpCodes.Ldfld)
            .Select(o => o.Operand)
            .OfType<FieldReference>()
            .SingleOrDefault();
    }

    internal static bool IsIList(PropertyDefinition property)
    {
        return property.PropertyType.Name == "IList`1" && property.PropertyType.Namespace == "System.Collections.Generic";
    }

    internal static bool IsRealmList(PropertyDefinition property)
    {
        return property.PropertyType.Name == "RealmList`1" && property.PropertyType.Namespace == "Realms";
    }

    internal static bool IsDateTimeOffset(PropertyDefinition property)
    {
        return property.PropertyType.Name == "DateTimeOffset" && property.PropertyType.Namespace == "System";
    }

    internal static bool IsNullable(PropertyDefinition property)
    {
        return property.PropertyType.Name.Contains("Nullable`1") && property.PropertyType.Namespace == "System";
    }

    internal static bool IsSingle(PropertyDefinition property)
    {
        return property.PropertyType.Name == "Single" && property.PropertyType.Namespace == "System";
    }

    internal static bool IsDouble(PropertyDefinition property)
    {
        return property.PropertyType.Name == "Double" && property.PropertyType.Namespace == "System";
    }
}