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
using UnityEngine;

namespace RealmWeaver
{
    public class ResolutionResult : IDisposable
    {
        private readonly WeaverAssemblyResolver _resolver;
        private readonly FileStream _fileStream;
        private readonly WriterParameters _writerParameters;

        public ModuleDefinition Module { get; }

        public ResolutionResult(ModuleDefinition module, FileStream fileStream, WeaverAssemblyResolver resolver, bool writeSymbols)
        {
            Module = module;
            _resolver = resolver;
            _fileStream = fileStream;

            _writerParameters = new WriterParameters();
            if (writeSymbols)
            {
                _writerParameters.WriteSymbols = true;
                _writerParameters.SymbolWriterProvider = new PdbWriterProvider();
            }
        }

        public void SaveModuleUpdates()
        {
            Module.Write(_writerParameters);
        }

        public void Dispose()
        {
            Module.Dispose();
            _fileStream.Dispose();
            _resolver.Dispose();
        }
    }

    public class WeaverAssemblyResolver : BaseAssemblyResolver
    {
        private readonly IDictionary<string, AssemblyDefinition> _cache = new Dictionary<string, AssemblyDefinition>();

        private WeaverAssemblyResolver(IEnumerable<string> references)
        {
            var referenceDirectories = references.Select(Path.GetDirectoryName).Distinct().ToArray();
            foreach (var reference in referenceDirectories)
            {
                AddSearchDirectory(reference);
            }
        }

        public static ResolutionResult Resolve(string assemblyPath, IEnumerable<string> references)
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

            var assemblyStream = new FileStream(absolutePath, FileMode.Open, FileAccess.ReadWrite);
            var resolver = new WeaverAssemblyResolver(references);
            var readerParameters = new ReaderParameters
            {
                ReadingMode = ReadingMode.Immediate,
                ReadWrite = true,
                AssemblyResolver = resolver,
                ReadSymbols = false,
            };

            var hasDebugInfo = File.Exists(absolutePath.Replace(".dll", ".pdb"));
            if (hasDebugInfo)
            {
                readerParameters.ReadSymbols = true;
                readerParameters.SymbolReaderProvider = new PdbReaderProvider();
            }

            var module = ModuleDefinition.ReadModule(assemblyStream, readerParameters);

            return new ResolutionResult(module, assemblyStream, resolver, hasDebugInfo);
        }

        private static string GetAbsolutePath(string assemblyPath)
        {
            if (File.Exists(assemblyPath))
            {
                return assemblyPath;
            }

            return Path.Combine(Application.dataPath, "..", assemblyPath);
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            if (!_cache.TryGetValue(name.FullName, out var assemblyDef))
            {
                try
                {
                    assemblyDef = base.Resolve(name, parameters);
                    _cache[name.FullName] = assemblyDef;
                }
                catch
                {
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
