////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Realms.SourceGenerator
{
    internal class DiagnosticsEmitter
    {
        private readonly GeneratorExecutionContext _context;

        public DiagnosticsEmitter(GeneratorExecutionContext context, GeneratorConfig generatorConfig)
        {
            _context = context;

            var customIgnoreAttribute = generatorConfig.CustomIgnoreAttribute;
            if (!string.IsNullOrEmpty(customIgnoreAttribute))
            {
                if (!customIgnoreAttribute!.StartsWith("[") || !customIgnoreAttribute.EndsWith("]"))
                {
                    _context.ReportDiagnostic(Diagnostics.InvalidConfiguration(
                        field: "realm.custom_ignore_attribute",
                        description: $"The attribute(s) string should start with '[' and end with ']'. Actual value: {customIgnoreAttribute}."));

                    generatorConfig.CustomIgnoreAttribute = null;
                }
            }
        }

        public void Emit(ParsingResults parsingResults)
        {
            foreach (var classInfo in parsingResults.ClassInfo.Where(classInfo => classInfo.Diagnostics.Any()))
            {
                try
                {
                    SerializeDiagnostics(_context, classInfo);
                    classInfo.Diagnostics.ForEach(_context.ReportDiagnostic);
                }
                catch (Exception ex)
                {
                    _context.ReportDiagnostic(Diagnostics.UnexpectedError(classInfo.Name, ex.Message, ex.StackTrace));
                    throw;
                }
            }
        }

        private static void SerializeDiagnostics(GeneratorExecutionContext context, ClassInfo classInfo)
        {
#if DEBUG
            if (Environment.GetEnvironmentVariable("NO_GENERATOR_DIAGNOSTICS") != null)
            {
                return;
            }

            var diagnosticInfos = classInfo.Diagnostics.Select(Convert);
            var serializedJson = Newtonsoft.Json.JsonConvert.SerializeObject(diagnosticInfos, Newtonsoft.Json.Formatting.Indented);

            var className = classInfo.HasDuplicatedName ? $"{classInfo.NamespaceInfo.ComputedName}_{classInfo.Name}" : classInfo.Name;

            // Discussion about emitting non-source files: https://github.com/dotnet/roslyn/issues/57608
            // Because of this the emitted files will have ".cs" extension.
            context.AddSource($"{className}.diagnostics", serializedJson);
#else
            return;
#endif
        }

        private static DiagnosticInfo Convert(Diagnostic diag)
        {
            return new(diag.Id, diag.Severity, diag.GetMessage(), location: Convert(diag.Location));
        }

        private static DiagnosticLocation Convert(Location location)
        {
            // The +1 are necessary because line position start counting at 0
            var mapped = location.GetLineSpan();
            return new()
            {
                StartColumn = mapped.StartLinePosition.Character + 1,
                StartLine = mapped.StartLinePosition.Line + 1,
                EndColumn = mapped.EndLinePosition.Character + 1,
                EndLine = mapped.EndLinePosition.Line + 1,
            };
        }
    }

    internal class DiagnosticInfo
    {
        public string Id { get; }

        public DiagnosticSeverity Severity { get; }

        public string Message { get; }

        public DiagnosticLocation Location { get; }

        public DiagnosticInfo(string id, DiagnosticSeverity severity, string message, DiagnosticLocation location)
        {
            Id = id;
            Severity = severity;
            Message = message;
            Location = location;
        }
    }

    internal class DiagnosticLocation
    {
        public int StartLine { get; set; }

        public int StartColumn { get; set; }

        public int EndLine { get; set; }

        public int EndColumn { get; set; }
    }
}
