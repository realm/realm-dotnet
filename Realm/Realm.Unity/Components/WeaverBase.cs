using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Weaver
{
    public abstract class WeaverBase
    {
        protected ILogger Logger { get; }

        protected ModuleDefinition ModuleDefinition { get; }

        protected WeaverBase(ModuleDefinition module, ILogger logger)
        {
            ModuleDefinition = module;
            Logger = logger;
        }

        protected abstract WeaveModuleResult Execute();

        public interface ILogger
        {
            void Debug(string message, SequencePoint sequencePoint = null);
            void Info(string message, SequencePoint sequencePoint = null);
            void Warning(string message, SequencePoint sequencePoint = null);
            void Error(string message, SequencePoint sequencePoint = null);
        }

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

            var logger = new Logger();

            var name = Path.GetFileNameWithoutExtension(assemblyPath);

            var filePath = GetAbsolutePath(assemblyPath);

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
                var weaver = new RealmWeaver(moduleDefinition, logger);
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

        protected class WeaveModuleResult
        {
            public static WeaveModuleResult Success(IEnumerable<WeaveTypeResult> types)
            {
                return new WeaveModuleResult(types.ToArray(), skipReason: null);
            }

            public static WeaveModuleResult Skipped(string reason)
            {
                return new WeaveModuleResult(types: null, skipReason: reason);
            }

            public WeaveTypeResult[] Types { get; }

            public string SkipReason { get; }

            private WeaveModuleResult(WeaveTypeResult[] types, string skipReason)
            {
                Types = types;
                SkipReason = skipReason;
            }

            public override string ToString()
            {
                if (SkipReason != null)
                {
                    return SkipReason;
                }

                var sb = new StringBuilder();
                sb.AppendLine($"{Types.Length} types were woven:");
                foreach (var type in Types)
                {
                    sb.AppendLine(type.ToString());
                }

                return sb.ToString();
            }
        }

        protected class WeaveTypeResult
        {
            public static WeaveTypeResult Success(string type, IEnumerable<WeavePropertyResult> properties)
            {
                return new WeaveTypeResult(type, properties.ToArray());
            }

            public string Type { get; }
            public WeavePropertyResult[] Properties { get; }

            private WeaveTypeResult(string type, WeavePropertyResult[] properties)
            {
                Properties = properties;
                Type = type;
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.AppendLine($"<b>{Type}</b>");
                foreach (var prop in Properties)
                {
                    sb.AppendLine($"  {prop}");
                }
                return sb.ToString();
            }
        }

        protected class WeavePropertyResult
        {
            public static WeavePropertyResult Success(PropertyDefinition property, FieldReference field, bool isPrimaryKey, bool isIndexed)
            {
                return new WeavePropertyResult(property, field, isPrimaryKey, isIndexed);
            }

            public static WeavePropertyResult Warning(string warning)
            {
                return new WeavePropertyResult(warning: warning);
            }

            public static WeavePropertyResult Error(string error)
            {
                return new WeavePropertyResult(error: error);
            }

            public static WeavePropertyResult Skipped()
            {
                return new WeavePropertyResult();
            }

            public string ErrorMessage { get; }

            public string WarningMessage { get; }

            public bool Woven { get; }

            public PropertyDefinition Property { get; }

            public FieldReference Field { get; }

            public bool IsPrimaryKey { get; }

            public bool IsIndexed { get; }

            private WeavePropertyResult(PropertyDefinition property, FieldReference field, bool isPrimaryKey, bool isIndexed)
            {
                Property = property;
                Field = field;
                IsPrimaryKey = isPrimaryKey;
                IsIndexed = isIndexed;
                Woven = true;
            }

            private WeavePropertyResult(string error = null, string warning = null)
            {
                ErrorMessage = error;
                WarningMessage = warning;
            }

            public override string ToString()
            {
                return $"    <i>{Property.Name}</i>: {Property.PropertyType.ToFriendlyString()}{(IsPrimaryKey ? " [PrimaryKey]" : string.Empty)}{(IsIndexed ? " [Indexed]" : string.Empty)}";
            }
        }

    }
}
