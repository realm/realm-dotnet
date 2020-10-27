using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
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
            CompilationPipeline.assemblyCompilationFinished -= ComplicationComplete;
            CompilationPipeline.assemblyCompilationFinished += ComplicationComplete;
        }

        private static void ComplicationComplete(string assemblyPath, CompilerMessage[] compilerMessages)
        {
            WeaveAssembly(assemblyPath);
        }

        private static void WeaveAssembly(string assemblyPath)
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

                var report = new StringBuilder();
                report.AppendLine($"[{name}] Weaving completed in {timer.ElapsedMilliseconds} ms.");
                report.AppendLine(results.ToString());
                logger.Info(report.ToString());
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
