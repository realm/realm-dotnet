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

// ReSharper disable once CheckNamespace
namespace RealmWeaver
{
    public class ResolutionResult : IDisposable
    {
        private readonly WeaverAssemblyResolver _resolver;
        private readonly string _filePath;
        private readonly WriterParameters _writerParameters;

        public ModuleDefinition Module { get; }

        public ResolutionResult(ModuleDefinition module, string filePath, WeaverAssemblyResolver resolver, bool writeSymbols)
        {
            Module = module;
            _resolver = resolver;
            _filePath = filePath;

            _writerParameters = new WriterParameters();
            if (writeSymbols)
            {
                _writerParameters.WriteSymbols = true;
                _writerParameters.SymbolWriterProvider = new PdbWriterProvider();
            }
        }

        public void SaveModuleUpdates()
        {
            Module.Write(_filePath, _writerParameters);
        }

        public void Dispose()
        {
            Module.Dispose();
            _resolver.Dispose();
        }
    }

    public class WeaverAssemblyResolver : BaseAssemblyResolver
    {
        private readonly IDictionary<string, AssemblyDefinition> _cache = new Dictionary<string, AssemblyDefinition>();

        public static string ApplicationDataPath { get; set; } = null!;

        private WeaverAssemblyResolver(IEnumerable<string> references)
        {
            var referenceDirectories = references.Select(Path.GetDirectoryName).Distinct().ToArray();
            foreach (var reference in referenceDirectories)
            {
                AddSearchDirectory(reference);
            }
        }

        public static ResolutionResult? Resolve(string? assemblyPath, IEnumerable<string> references)
        {
            if (assemblyPath == null)
            {
                return null;
            }

            var absolutePath = GetAbsolutePath(assemblyPath);

            if (!File.Exists(absolutePath))
            {
                return null;
            }

            var resolver = new WeaverAssemblyResolver(references);
            var readerParameters = new ReaderParameters
            {
                AssemblyResolver = resolver,
                InMemory = true,
            };

            var hasDebugInfo = false;
            var module = ModuleDefinition.ReadModule(absolutePath, readerParameters);

            try
            {
                module.ReadSymbols();
                hasDebugInfo = true;
            }
            catch
            {
                // ignored
            }

            return new ResolutionResult(module, absolutePath, resolver, hasDebugInfo);
        }

        private static string GetAbsolutePath(string assemblyPath)
        {
            if (File.Exists(assemblyPath))
            {
                return assemblyPath;
            }

            return Path.Combine(ApplicationDataPath, "..", assemblyPath);
        }

        public override AssemblyDefinition? Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (!_cache.TryGetValue(name.FullName, out var assemblyDef))
            {
                try
                {
                    assemblyDef = base.Resolve(name, parameters);
                    _cache[name.FullName] = assemblyDef;
                }
                catch
                {
                    // ignored
                }
            }

            return assemblyDef;
        }

        protected override void Dispose(bool disposing)
        {
            foreach (var kvp in _cache)
            {
                kvp.Value.Dispose();
            }

            _cache.Clear();

            base.Dispose(disposing);
        }
    }
}
