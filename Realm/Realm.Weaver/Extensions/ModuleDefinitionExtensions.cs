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

using System.ComponentModel;
using System.Linq;
using Mono.Cecil;

[EditorBrowsable(EditorBrowsableState.Never)]
internal static class ModuleDefinitionExtensions
{
    public static ModuleDefinition ResolveReference(this ModuleDefinition module, string assembly)
    {
        var assemblyNameReference = module.FindReference(assembly);
        if (assemblyNameReference != null)
        {
            return module.AssemblyResolver.Resolve(assemblyNameReference).MainModule;
        }

        return null;
    }

    public static AssemblyNameReference FindReference(this ModuleDefinition module, string assembly)
    {
        return module.AssemblyReferences.SingleOrDefault(a => a.Name == assembly);
    }

}
