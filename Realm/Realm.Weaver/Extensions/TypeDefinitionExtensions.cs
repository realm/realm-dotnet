////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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
using System.ComponentModel;
using System.Linq;
using System.Text;
using Mono.Cecil;
using RealmWeaver;

[EditorBrowsable(EditorBrowsableState.Never)]
internal static class TypeDefinitionExtensions
{
    public static bool IsValidRealmType(this TypeDefinition type, ImportedReferences references) =>
            type.IsRealmObjectDescendant(references) || type.IsIRealmObjectBaseImplementor(references);

    public static bool IsEmbeddedObjectInheritor(this TypeDefinition type, ImportedReferences references) =>
       type.BaseType.IsSameAs(references.EmbeddedObject);

    public static bool IsRealmObjectInheritor(this TypeDefinition type, ImportedReferences references) =>
        type.BaseType.IsSameAs(references.RealmObject);

    public static bool IsAsymmetricObjectInheritor(this TypeDefinition type, ImportedReferences references) =>
        type.BaseType.IsSameAs(references.AsymmetricObject);

    public static bool IsValidRealmObjectBaseInheritor(this TypeDefinition type, ImportedReferences references) =>
        type.IsRealmObjectInheritor(references) ||
        type.IsEmbeddedObjectInheritor(references) ||
        type.IsAsymmetricObjectInheritor(references);

    public static bool IsIEmbeddedObjectImplementor(this TypeDefinition type, ImportedReferences references) =>
        IsImplementorOf(type, references.IEmbeddedObject);

    public static bool IsIAsymmetricObjectImplementor(this TypeDefinition type, ImportedReferences references) =>
        IsImplementorOf(type, references.IAsymmetricObject);

    public static bool IsImplementorOf(TypeDefinition @this, params TypeReference[] targetInterfaces)
    {
        foreach (var @interface in @this.Interfaces)
        {
            foreach (var targetInterface in targetInterfaces)
            {
                if (@interface.InterfaceType.IsSameAs(targetInterface))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
