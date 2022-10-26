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
        private GeneratorExecutionContext _context;

        public DiagnosticsEmitter(GeneratorExecutionContext context)
        {
            _context = context;
        }

        public void Emit(ParsingResults parsingResults)
        {
            foreach (var classInfo in parsingResults.ClassInfo)
            {
                if (!classInfo.Diagnostics.Any())
                {
                    continue;
                }

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
            return new DiagnosticInfo
            {
                Id = diag.Id,
                Severity = diag.Severity,
                Message = diag.GetMessage(),
                Location = Convert(diag.Location),
            };
        }

        private static DiagnosticLocation Convert(Location location)
        {
            // The +1 are necessary because line position start counting at 0
            var mapped = location.GetLineSpan();
            return new DiagnosticLocation
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
        public string Id { get; set; }

        public DiagnosticSeverity Severity { get; set; }

        public string Message { get; set; }

        public DiagnosticLocation Location { get; set; }
    }

    internal class DiagnosticLocation
    {
        public string Path { get; set; }

        public int StartLine { get; set; }

        public int StartColumn { get; set; }

        public int EndLine { get; set; }

        public int EndColumn { get; set; }
    }
}
