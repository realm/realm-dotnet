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
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Realms.SourceGenerator
{
    internal class CodeEmitter
    {
        private GeneratorExecutionContext _context;

        public CodeEmitter(GeneratorExecutionContext context)
        {
            _context = context;
        }

        public void Emit(ParsingResults parsingResults)
        {
            foreach (var classInfo in parsingResults.ClassInfo)
            {
                if (!ShouldEmit(classInfo))
                {
                    continue;
                }

                try
                {
                    var generatedSource = new ClassCodeBuilder(classInfo).GenerateSource();

                    // Replace all occurrences of at least 3 newlines with only 2
                    var formattedSource = Regex.Replace(generatedSource, @$"[{Environment.NewLine}]{{3,}}", $"{Environment.NewLine}{Environment.NewLine}");

                    var sourceText = SourceText.From(formattedSource, Encoding.UTF8);

                    // Discussion on allowing duplicate hint names: https://github.com/dotnet/roslyn/discussions/60272
                    var className = classInfo.HasDuplicatedName ? $"{classInfo.NamespaceInfo.ComputedName}_{classInfo.Name}" : classInfo.Name;

                    _context.AddSource($"{className}_generated.cs", sourceText);
                }
                catch (Exception ex)
                {
                    _context.ReportDiagnostic(Diagnostics.UnexpectedError(classInfo.Name, ex.Message, ex.StackTrace));
                    throw;
                }
            }
        }

        private static bool ShouldEmit(ClassInfo classInfo)
        {
            return !classInfo.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
        }
    }
}
