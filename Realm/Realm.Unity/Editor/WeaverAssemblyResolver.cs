////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using UnityEditor.Compilation;
using UnityEngine;

namespace RealmWeaver
{
    public class WeaverAssemblyResolver : BaseAssemblyResolver
    {
        private readonly IDictionary<string, string> _appDomainAssemblyLocations = new Dictionary<string, string>();
        private readonly IDictionary<string, AssemblyDefinition> _cache = new Dictionary<string, AssemblyDefinition>();

        private WeaverAssemblyResolver(string[] references)
        {
            foreach (var reference in references)
            {
                AddSearchDirectory(reference);
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.ReflectionOnly && !a.IsDynamic))
            {
                _appDomainAssemblyLocations[assembly.FullName] = assembly.Location;
            }
        }

        public static (ModuleDefinition, IDisposable) Resolve(Assembly assembly)
        {
            var absolutePath = GetAbsolutePath(assembly.outputPath);

            if (!File.Exists(absolutePath))
            {
                return (null, null);
            }

            var systemAssemblies = CompilationPipeline.GetSystemAssemblyDirectories(assembly.compilerOptions.ApiCompatibilityLevel);

            var assemblyStream = new FileStream(assembly.outputPath, FileMode.Open, FileAccess.ReadWrite);
            var module = ModuleDefinition.ReadModule(assemblyStream, new ReaderParameters
            {
                ReadingMode = ReadingMode.Immediate,
                ReadWrite = true,
                AssemblyResolver = new WeaverAssemblyResolver(systemAssemblies),
                ReadSymbols = true,
                SymbolReaderProvider = new PdbReaderProvider()
            });

            return (module, assemblyStream);
        }

        private static string GetAbsolutePath(string assemblyPath)
        {
            return Path.Combine(Application.dataPath, "..", assemblyPath);
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            if (_cache.TryGetValue(name.FullName, out var assemblyDef))
            {
                return assemblyDef;
            }

            try
            {
                assemblyDef = base.Resolve(name, parameters);
            }
            catch
            {
            }

            if (assemblyDef == null)
            {
                assemblyDef = FindAssemblyDefinition(name.FullName, parameters);
            }

            _cache[name.FullName] = assemblyDef;
            return assemblyDef;
        }

        private AssemblyDefinition FindAssemblyDefinition(string fullName, ReaderParameters parameters)
        {
            if (_appDomainAssemblyLocations.TryGetValue(fullName, out var location))
            {
                return parameters != null ? AssemblyDefinition.ReadAssembly(location, parameters) : AssemblyDefinition.ReadAssembly(location);
            }

            return null;
        }
    }
}
