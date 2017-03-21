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
using Mono.Cecil;
using Mono.Cecil.Cil;

public partial class ModuleWeaver
{
    private void WeaveSchema(TypeDefinition[] types)
    {
        if (_references.RealmSchema_AddDefaultTypes == null)
        {
            // Realm is added, but not used, so we don't need to weave schema
            return;
        }

        var allTypes = GetReferencedTypes().Where(t => t != null)
                                           .Where(t => t.CustomAttributes.Any(a => a.AttributeType.Name == "WovenAttribute"))
                                           .Concat(types)
                                           .Where(t => !t.CustomAttributes.Any(a => a.AttributeType.Name == "ExplicitAttribute"))
                                           .Select(ModuleDefinition.ImportReference)
                                           .Distinct()
                                           .ToArray();

        if (allTypes.Any())
        {
            // TODO: test with more convoluted scenario
            LogDebug($"Default schema woven with the following classes:{string.Join(", ", allTypes.Select(t => t.Name))}");

            var entryPoint = ModuleDefinition.EntryPoint ?? GetModuleInitializer();
            var start = entryPoint.Body.Instructions.First();
            var il = entryPoint.Body.GetILProcessor();
            il.InsertBefore(start, Instruction.Create(OpCodes.Ldc_I4, allTypes.Length));
            il.InsertBefore(start, Instruction.Create(OpCodes.Newarr, _references.System_Type));

            for (var i = 0; i < allTypes.Length; i++)
            {
                il.InsertBefore(start, Instruction.Create(OpCodes.Dup));
                il.InsertBefore(start, Instruction.Create(OpCodes.Ldc_I4, i));
                il.InsertBefore(start, Instruction.Create(OpCodes.Ldtoken, allTypes[i]));
                il.InsertBefore(start, Instruction.Create(OpCodes.Call, _references.System_Type_GetTypeFromHandle));
                il.InsertBefore(start, Instruction.Create(OpCodes.Stelem_Ref));
            }

            il.InsertBefore(start, Instruction.Create(OpCodes.Call, _references.RealmSchema_AddDefaultTypes));
        }
        else
        {
            LogWarning("Default schema appears to be empty. This is not an error if you don't have any RealmObject inheritors declared. Otherwise it may be a bug with the weaver.");
        }
    }

    private IEnumerable<TypeDefinition> GetReferencedTypes(ModuleDefinition module = null, HashSet<string> processedAssemblies = null)
    {
        module = module ?? ModuleDefinition;
        processedAssemblies = processedAssemblies ?? new HashSet<string>();

        if (module.AssemblyReferences.Any(a => a.Name == "Realm"))
        {
            foreach (var type in module.GetTypes())
            {
                yield return type;
            }
        }

        var referencedModules = module.AssemblyReferences
                                      .Select(ModuleDefinition.AssemblyResolver.Resolve)
                                      .Where(a => a != null)
                                      .Where(a => processedAssemblies.Add(a.FullName))
                                      .Select(a => a.MainModule);

        foreach (var referencedModule in referencedModules)
        {
            foreach (var type in GetReferencedTypes(referencedModule, processedAssemblies))
            {
                yield return type;
            }
        }
    }

    private MethodDefinition GetModuleInitializer()
    {
        // Very similar to https://github.com/Fody/ModuleInit
        var initializerType = new TypeDefinition(null, "RealmModuleInitializer",
                                                 TypeAttributes.Class | TypeAttributes.BeforeFieldInit,
                                    ModuleDefinition.TypeSystem.Object);

        var initialize = new MethodDefinition("Initialize", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Static, ModuleDefinition.TypeSystem.Void)
        {
            HasThis = false
        };

        initialize.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        initializerType.Methods.Add(initialize);
        ModuleDefinition.Types.Add(initializerType);

        var moduleClass = ModuleDefinition.Types.FirstOrDefault(x => x.Name == "<Module>");
        if (moduleClass == null)
        {
            throw new Exception("Found no module class!");
        }

        var cctor = FindOrCreateCctor(moduleClass);

        var returnPoints = cctor.Body.Instructions
                                .Where(i => i.OpCode == OpCodes.Ret)
                                .ToArray();

        foreach (var returnPoint in returnPoints)
        {
            var index = cctor.Body.Instructions.IndexOf(returnPoint);
            cctor.Body.Instructions.Insert(index, Instruction.Create(OpCodes.Call, initialize));
        }

        return initialize;
    }

    private MethodDefinition FindOrCreateCctor(TypeDefinition moduleClass)
    {
        var cctor = moduleClass.Methods.FirstOrDefault(x => x.Name == ".cctor");
        if (cctor == null)
        {
            var attributes = MethodAttributes.Private
                             | MethodAttributes.HideBySig
                             | MethodAttributes.Static
                             | MethodAttributes.SpecialName
                             | MethodAttributes.RTSpecialName;

            cctor = new MethodDefinition(".cctor", attributes, ModuleDefinition.TypeSystem.Void);
            moduleClass.Methods.Add(cctor);
            cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        return cctor;
    }
}
