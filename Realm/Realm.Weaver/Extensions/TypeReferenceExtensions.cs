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

using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using RealmWeaver;

[EditorBrowsable(EditorBrowsableState.Never)]
internal static class TypeReferenceExtensions
{
    private static readonly Regex NullableRegex = new("^System.Nullable`1<(?<typeName>.*)>$");

    public static SequencePoint GetSequencePoint(this TypeDefinition @this)
    {
        return GetCtorSequencePoint() ?? GetPropSequencePoint();

        SequencePoint GetCtorSequencePoint()
        {
            return @this.GetConstructors()
                .OrderBy(c => c.Parameters.Count)
                .SelectMany(c => c.DebugInformation.SequencePoints)
                .FirstOrDefault();
        }

        SequencePoint GetPropSequencePoint()
        {
            return @this.Properties
                .Select(p => p.GetSequencePoint())
                .FirstOrDefault(sp => sp != null);
        }
    }

    public static bool IsAnyRealmObject(this TypeReference @this, ImportedReferences references)
        => @this.IsIRealmObjectBaseImplementor(references)
        || @this.IsRealmObjectDescendant(references);

    public static bool IsRealmObjectDescendant(this TypeReference @this, ImportedReferences references) =>
        IsDescendantOf(@this, references.RealmObject, references.EmbeddedObject, references.AsymmetricObject);

    public static bool IsAsymmetricObjectDescendant(this TypeReference @this, ImportedReferences references) =>
        IsDescendantOf(@this, references.AsymmetricObject);

    public static bool IsEmbeddedObjectDescendant(this TypeReference @this, ImportedReferences references) =>
        IsDescendantOf(@this, references.EmbeddedObject);

    public static bool IsSameAs(this TypeReference @this, TypeReference other)
    {
        if (@this is null || other is null)
        {
            return false;
        }

        return @this.FullName == other.FullName && @this.GetAssemblyName() == other.GetAssemblyName();
    }

    public static string GetAssemblyName(this TypeReference @this)
    {
        switch (@this.Scope.MetadataScopeType)
        {
            case MetadataScopeType.AssemblyNameReference:
                return ((AssemblyNameReference)@this.Scope).FullName;
            case MetadataScopeType.ModuleReference:
                return ((ModuleReference)@this.Scope).Name;
            default:
                return ((ModuleDefinition)@this.Scope).Assembly.FullName;
        }
    }

    public static string ToFriendlyString(this TypeReference type)
    {
        if (!string.IsNullOrEmpty(type.Namespace))
        {
            return type.ToString().Replace(type.Namespace, string.Empty).TrimStart('.');
        }

        return type.ToString();
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

    public static bool IsNullable(this TypeReference reference) =>
        NullableRegex.IsMatch(reference.FullName);

    public static bool IsIRealmObjectBaseImplementor(this TypeReference type, ImportedReferences references) =>
        IsImplementorOf(type, references.IRealmObjectBase);

    private static bool IsDescendantOf(TypeReference @this, params TypeReference[] targetTypes)
    {
        try
        {
            while (true)
            {
                if (@this == null || !Weaver.ShouldTraverseAssembly(@this.Module.Assembly.Name))
                {
                    return false;
                }

                var definition = @this?.Resolve();
                if (definition == null)
                {
                    return false;
                }

                foreach (var typeRef in targetTypes)
                {
                    if (definition.BaseType.IsSameAs(typeRef))
                    {
                        return true;
                    }
                }

                @this = definition.BaseType;
            }
        }
        catch
        {
            // Unity may fail to resolve some of its assemblies, but that's okay
            // they don't contain RealmObject classes.
        }

        return false;
    }

    private static bool IsImplementorOf(TypeReference @this, params TypeReference[] targetInterfaces)
    {
        try
        {
            if (@this == null || !Weaver.ShouldTraverseAssembly(@this.Module.Assembly.Name))
            {
                return false;
            }

            var definition = @this?.Resolve();
            if (definition == null)
            {
                return false;
            }

            return TypeDefinitionExtensions.IsImplementorOf(definition, targetInterfaces);
        }
        catch
        {
            // Unity may fail to resolve some of its assemblies, but that's okay
            // they don't contain RealmObject classes.
        }

        return false;
    }
}
