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
        private static WriterParameters WriterParameters => new WriterParameters
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

            var logger = new UnityLogger();
            var name = Path.GetFileNameWithoutExtension(assemblyPath);

            try
            {
                var timer = new Stopwatch();
                timer.Start();

                var (moduleDefinition, fileStream) = WeaverAssemblyResolver.Resolve(assemblyPath);
                if (moduleDefinition == null)
                {
                    return;
                }
                using (fileStream)
                using (moduleDefinition)
                {
                    // Unity doesn't add the [TargetFramework] attribute when compiling the assembly. However, it's
                    // using NETStandard2, so we just hardcode this.
                    var weaver = new Weaver(moduleDefinition, logger, new FrameworkName(".NETStandard,Version=v2.0"));
                    var results = weaver.Execute();

                    moduleDefinition.Write(WriterParameters);

                    logger.Info($"[{name}] Weaving completed in {timer.ElapsedMilliseconds} ms.{Environment.NewLine}{results}");
                }

                // save any changes to our weavedAssembly objects
                AssetDatabase.SaveAssets();
            }
            catch (Exception ex)
            {
                logger.Warning($"[{name}] Weaving failed: {ex.Message}");
            }
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
