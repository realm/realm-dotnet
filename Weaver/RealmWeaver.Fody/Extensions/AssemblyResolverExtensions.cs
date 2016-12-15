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
using System.ComponentModel;
using Mono.Cecil;

[EditorBrowsable(EditorBrowsableState.Never)]
internal static class AssemblyResolverExtensions
{
    public static TypeDefinition LookupTypeDefinition(this IAssemblyResolver resolver, string typeName, params string[] assemblyNames)
    {
        foreach (var name in assemblyNames)
        {
            var result = resolver.Resolve(name).MainModule.GetType(typeName);
            if (result != null)
            {
                return result;
            }
        }

        throw new Exception($"{typeName} not found in {string.Join(", ", assemblyNames)}");
    }
}