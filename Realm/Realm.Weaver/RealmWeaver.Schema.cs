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

namespace RealmWeaver
{
    internal partial class Weaver
    {
        // b77a5c561934e089
        private static readonly byte[] _systemAssemblyPublicKey = new byte[] { 183, 122, 92, 86, 25, 52, 224, 137 };

        private void WeaveSchema(TypeDefinition[] types)
        {
            if (_references.RealmSchema_AddDefaultTypes == null)
            {
                // Realm is added, but not used, so we don't need to weave schema
                return;
            }

            var referencedTypes = GetReferencedTypes().Where(t => t?.CustomAttributes.Any(a => a.AttributeType.IsSameAs(_references.WovenAttribute)) == true);

            if (ShouldInclude(_moduleDefinition.Assembly))
            {
                referencedTypes = referencedTypes.Concat(types);
            }

            var realmTypes = referencedTypes.Where(ShouldInclude)
                                            .Select(_moduleDefinition.ImportReference)
                                            .Distinct()
                                            .ToArray();

            if (realmTypes.Any())
            {
                var entryPoint = GetModuleInitializer();
                var start = entryPoint.Body.Instructions.First();
                var il = entryPoint.Body.GetILProcessor();
                il.InsertBefore(start, Instruction.Create(OpCodes.Ldc_I4, realmTypes.Length));
                il.InsertBefore(start, Instruction.Create(OpCodes.Newarr, _references.System_Type));

                for (var i = 0; i < realmTypes.Length; i++)
                {
                    il.InsertBefore(start, Instruction.Create(OpCodes.Dup));
                    il.InsertBefore(start, Instruction.Create(OpCodes.Ldc_I4, i));
                    il.InsertBefore(start, Instruction.Create(OpCodes.Ldtoken, realmTypes[i]));
                    il.InsertBefore(start, Instruction.Create(OpCodes.Call, _references.System_Type_GetTypeFromHandle));
                    il.InsertBefore(start, Instruction.Create(OpCodes.Stelem_Ref));
                }

                il.InsertBefore(start, Instruction.Create(OpCodes.Call, _references.RealmSchema_AddDefaultTypes));
            }
            else
            {
                _logger.Warning($"Default schema for {_moduleDefinition.Assembly.Name} appears to be empty. This is not an error if you don't have any RealmObject inheritors declared. Otherwise it may be a bug with the weaver.");
            }
        }

        private IEnumerable<TypeDefinition> GetReferencedTypes(ModuleDefinition module = null, HashSet<string> processedAssemblies = null)
        {
            module ??= _moduleDefinition;

            // Here we cannot use yield return iterator because of an issue with Mono which is used by the Unity weaver.
            // See issue https://github.com/realm/realm-dotnet/issues/3199 for context.
            var result = new List<TypeDefinition>();

            // If module has been marked [Explicit], ignore all types
            if (ShouldInclude(module.Assembly))
            {
                processedAssemblies ??= new HashSet<string>();

                if (module.AssemblyReferences.Any(a => a.Name == "Realm"))
                {
                    foreach (var type in module.GetTypes())
                    {
                        result.Add(type);
                    }
                }

                var referencedModules = module.AssemblyReferences
                                              .Where(ShouldTraverseAssembly)
                                              .Select(_moduleDefinition.AssemblyResolver.Resolve)
                                              .Where(a => a != null && processedAssemblies.Add(a.FullName))
                                              .Select(a => a.MainModule);

                foreach (var referencedModule in referencedModules)
                {
                    foreach (var type in GetReferencedTypes(referencedModule, processedAssemblies))
                    {
                        result.Add(type);
                    }
                }
            }

            return result;
        }

        private MethodDefinition GetModuleInitializer()
        {
            // Very similar to https://github.com/Fody/ModuleInit
            var initializerType = new TypeDefinition(null, "RealmModuleInitializer",
                                                     TypeAttributes.Class | TypeAttributes.BeforeFieldInit,
                                                     _moduleDefinition.TypeSystem.Object);

            var initialize = new MethodDefinition("Initialize", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static, _moduleDefinition.TypeSystem.Void)
            {
                HasThis = false
            };

            initialize.CustomAttributes.Add(new CustomAttribute(_references.PreserveAttribute_Constructor));

            initialize.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            initializerType.Methods.Add(initialize);
            _moduleDefinition.Types.Add(initializerType);

            var moduleClass = _moduleDefinition.Types.FirstOrDefault(x => x.Name == "<Module>");
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

                cctor = new MethodDefinition(".cctor", attributes, _moduleDefinition.TypeSystem.Void);
                moduleClass.Methods.Add(cctor);
                cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            }

            return cctor;
        }

        private bool ShouldInclude(ICustomAttributeProvider provider)
        {
            return provider.CustomAttributes.All(a => !a.AttributeType.IsSameAs(_references.ExplicitAttribute));
        }

        /// <summary>
        /// This is a bit of a hack to speed up compilation times, particularly of unity projects.
        /// It will return false for system assembly or assemblies that look like Unity ones.
        /// </summary>
        public static bool ShouldTraverseAssembly(AssemblyNameReference assembly)
        {
            // Filter out system assemblies
            if (assembly.PublicKeyToken.SequenceEqual(_systemAssemblyPublicKey))
            {
                return false;
            }

            // Attempt to filter out Unity assemblies - we're adding the Version(0.0.0.0)
            // as a best effort attempt to avoid filtering out legitimate user assemblies.
            if (assembly.Name == "UnityEditor" || assembly.Name.StartsWith("UnityEngine", StringComparison.OrdinalIgnoreCase) || assembly.Name.StartsWith("Unity.", StringComparison.OrdinalIgnoreCase))
            {
                return assembly.Version == default(Version);
            }

            return true;
        }
    }
}
