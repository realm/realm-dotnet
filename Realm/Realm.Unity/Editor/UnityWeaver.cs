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
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace RealmWeaver
{
    public class UnityWeaver
    {
        private static readonly ReaderParameters _readerParameters = new ReaderParameters
        {
            ReadingMode = ReadingMode.Immediate,
            ReadWrite = true,
            AssemblyResolver = new WeaverAssemblyResolver(),
            ReadSymbols = true,
            SymbolReaderProvider = new PdbReaderProvider()
        };

        private static readonly WriterParameters _writerParameters = new WriterParameters
        {
            WriteSymbols = true,
            SymbolWriterProvider = new PdbWriterProvider()
        };

        [UsedImplicitly]
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            CompilationPipeline.assemblyCompilationFinished -= CompilationComplete;
            CompilationPipeline.assemblyCompilationFinished += CompilationComplete;
        }

        private static void CompilationComplete(string assemblyPath, CompilerMessage[] compilerMessages)
        {
            if (string.IsNullOrEmpty(assemblyPath))
            {
                return;
            }

            var name = Path.GetFileNameWithoutExtension(assemblyPath);

            var filePath = GetAbsolutePath(assemblyPath);

            var logger = new UnityLogger();
            if (!File.Exists(filePath))
            {
                logger.Error($"[{name}] Unable to find assembly at path '{filePath}'.");
                return;
            }

            var timer = new Stopwatch();
            timer.Start();

            using (var assemblyStream = new FileStream(assemblyPath, FileMode.Open, FileAccess.ReadWrite))
            using (var moduleDefinition = ModuleDefinition.ReadModule(assemblyStream, _readerParameters))
            {
                // Unity doesn't add the [TargetFramework] attribute when compiling the assembly. However, it's
                // using NETStandard2, so we just hardcode this.
                var weaver = new Weaver(moduleDefinition, logger, new FrameworkName(".NETStandard,Version=v2.0"));
                var results = weaver.Execute();

                moduleDefinition.Write(_writerParameters);

                logger.Info($"[{name}] Weaving completed in {timer.ElapsedMilliseconds} ms.{Environment.NewLine}{results}");
            }

            // save any changes to our weavedAssembly objects
            AssetDatabase.SaveAssets();
        }

        private static string GetAbsolutePath(string assemblyPath)
        {
            return Path.Combine(Application.dataPath, "..", assemblyPath);
        }

        private class UnityLogger : ILogger
        {
            public void Debug(string message)
            {
                System.Diagnostics.Debug.WriteLine(message);
            }

            public void Error(string message, SequencePoint sequencePoint = null)
            {
                UnityEngine.Debug.LogError(GetMessage(message, sequencePoint));
            }

            public void Info(string message)
            {
                UnityEngine.Debug.Log(message);
            }

            public void Warning(string message, SequencePoint sequencePoint = null)
            {
                UnityEngine.Debug.LogWarning(GetMessage(message, sequencePoint));
            }

            private static string GetMessage(string message, SequencePoint sp)
            {
                if (sp == null)
                {
                    return message;
                }

                return $"{sp.Document.Url}({sp.StartLine}, {sp.StartColumn}): {message}";
            }
        }
    }
}
