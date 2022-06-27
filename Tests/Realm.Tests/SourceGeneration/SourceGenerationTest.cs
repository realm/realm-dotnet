// ////////////////////////////////////////////////////////////////////////////
// //
// // Copyright 2022 Realm Inc.
// //
// // Licensed under the Apache License, Version 2.0 (the "License")
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// // http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //
// ////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Newtonsoft.Json;
using Realm.SourceGenerator;
using RealmClassGeneratorVerifier = Realms.Tests.SourceGeneration.CSharpSourceGeneratorVerifier<Realm.SourceGenerator.RealmClassGenerator>;

namespace Realms.Tests.SourceGeneration
{
    public abstract class SourceGenerationTest
    {
        protected string TestClassesPath { get; }

        public SourceGenerationTest()
        {
            var buildFolder = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            var testFolder = buildFolder.Substring(0, buildFolder.IndexOf("bin", StringComparison.InvariantCulture));
            TestClassesPath = Path.Combine(testFolder, "SourceGeneration", "TestClasses");
        }

        protected string GetSourceForClass(string className) =>
            File.ReadAllText(Path.Combine(TestClassesPath, $"{className}.cs"));

        protected string GetGeneratedForClass(string className)
        {
            var fileName = Path.Combine(TestClassesPath, $"{className}_generated.cs");
            return File.Exists(fileName) ? File.ReadAllText(fileName) : string.Empty;
        }

        protected List<DiagnosticInfo> GetDiagnosticsForClass(string className)
        {
            var fileName = Path.Combine(TestClassesPath, $"{className}.diagnostics");

            if (!File.Exists(fileName))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<List<DiagnosticInfo>>(File.ReadAllText(fileName));
        }

        public async Task RunSimpleComparisonTest(string className)
        {
            var source = GetSourceForClass(className);
            var generated = GetGeneratedForClass(className);
            var generatedFileName = $"{className}_generated.cs";

            await new RealmClassGeneratorVerifier.Test
            {
                TestState =
                {
                    Sources =
                    {
                        source
                    },
                    GeneratedSources =
                    {
                        (typeof(RealmClassGenerator), generatedFileName, generated),
                    },
                },
            }.RunAsync();
        }

        public async Task RunSimpleErrorTest(string className)
        {
            var source = GetSourceForClass(className);
            var diagnostics = GetDiagnosticsForClass(className);

            var test = new RealmClassGeneratorVerifier.Test
            {
                TestState =
                {
                    Sources =
                    {
                        source
                    },
                },
            };

            //TODO Can we write it better?
            test.TestState.ExpectedDiagnostics.AddRange(diagnostics.Select(Convert));

            await test.RunAsync();
        }

        // Utility methods to retrieve only the list of diagnostics generated.
        public IEnumerable<DiagnosticInfo> GetDiagnostics(string className)
        {
            //TODO Need to check how other libraries are working with testing sg, and diagnostics (json for example)
            var source = GetSourceForClass(className);
            var inputCompilation = CSharpCompilation.Create("compilation",
                new[] { CSharpSyntaxTree.ParseText(source) },
                new[]
                {
                    MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Realm).Assembly.Location)
                },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));

            var generator = new RealmClassGenerator();

            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var _, out var diagnostics);

            return diagnostics.Select(Convert);
        }

        // TODO All of the following works, but feels a little clumsy
        // We can't simply serialize/deserialize Diagnostics/DiagnosticResults and some other classes (like Location)
        // Because they either are abstract, or they have get only properties
        public DiagnosticInfo Convert(Diagnostic diag)
        {
            return new DiagnosticInfo
            {
                Id = diag.Id,
                Severity = diag.Severity,
                Message = diag.GetMessage(),
                Location = Convert(diag.Location),
            };
        }

        public DiagnosticResult Convert(DiagnosticInfo info)
        {
            var dr = new DiagnosticResult(info.Id, info.Severity)
                .WithMessage(info.Message);

            if (info.Location != null)
            {
                dr = dr.WithSpan(info.Location.StartLine,
                    info.Location.StartColumn,
                    info.Location.EndLine,
                    info.Location.EndColumn);
            }

            return dr;
        }

        public DiagnosticLocation Convert(Location location)
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

        public class DiagnosticInfo
        {
            public string Id { get; set; }

            public DiagnosticSeverity Severity { get; set; }

            public string Message { get; set; }

            public DiagnosticLocation Location { get; set; }
        }

        public class DiagnosticLocation
        {
            public string Path { get; set; }

            public int StartLine { get; set; }

            public int StartColumn { get; set; }

            public int EndLine { get; set; }

            public int EndColumn { get; set; }
        }
    }
}
