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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Newtonsoft.Json;
using Realms.SourceGenerator;
using RealmClassGeneratorVerifier = SourceGeneratorTests.CSharpSourceGeneratorVerifier<Realms.SourceGenerator.RealmClassGenerator>;

namespace SourceGeneratorTests
{
    internal abstract class SourceGenerationTest
    {
        private string _testClassesPath;

        private string _generatedFilesPath;

        [OneTimeSetUp]
        public void Setup()
        {
            var buildFolder = Path.GetDirectoryName(typeof(SourceGenerationTest).Assembly.Location);
            var testFolder = buildFolder.Substring(0, buildFolder.IndexOf("Realm.SourceGenerator.Test", StringComparison.InvariantCulture));
            _testClassesPath = Path.Combine(testFolder, "SourceGeneratorAssemblyToProcess");
            _generatedFilesPath = Path.Combine(_testClassesPath, "Generated",
                "Realm.SourceGenerator", "Realms.SourceGenerator.RealmClassGenerator");
            Environment.SetEnvironmentVariable("NO_GENERATOR_DIAGNOSTICS", "true");
        }

        protected string GetSource(string filename) =>
            File.ReadAllText(Path.Combine(_testClassesPath, $"{filename}.cs"));

        protected string GetGeneratedForClass(string className)
        {
            var fileName = Path.Combine(_generatedFilesPath, $"{className}_generated.cs");
            return File.Exists(fileName) ? File.ReadAllText(fileName) : string.Empty;
        }

        protected List<DiagnosticInfo> GetDiagnosticsForClass(string className)
        {
            var fileName = Path.Combine(_generatedFilesPath, $"{className}.diagnostics.cs");

            if (!File.Exists(fileName))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<List<DiagnosticInfo>>(File.ReadAllText(fileName));
        }

        protected string GetDiagnosticFile(string className)
        {
            var fileName = Path.Combine(_generatedFilesPath, $"{className}.diagnostics.cs");

            if (!File.Exists(fileName))
            {
                return null;
            }

            return File.ReadAllText(fileName);
        }

        public async Task RunComparisonTest(string fileName, params string[] classNames)
        {
            var source = GetSource(fileName);

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

            foreach (var className in classNames)
            {
                var generated = GetGeneratedForClass(className);
                var generatedFileName = $"{className}_generated.cs";

                test.TestState.GeneratedSources.Add((typeof(RealmClassGenerator), generatedFileName, generated));
            }

            await test.RunAsync();
        }

        public async Task RunSimpleComparisonTest(string className)
        {
            await RunComparisonTest(className, className);
        }

        public async Task RunErrorTest(string fileName, params string[] classNames)
        {
            var source = GetSource(fileName);

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

            foreach (var className in classNames)
            {
                var diagnosticFileName = $"{className}.diagnostics.cs";
                var diagnostics = GetDiagnosticsForClass(className);

                test.TestState.ExpectedDiagnostics.AddRange(diagnostics.Select(Convert));
            }

            await test.RunAsync();
        }

        public async Task RunSimpleErrorTest(string className)
        {
            await RunErrorTest(className, className);
        }

        public static DiagnosticResult Convert(DiagnosticInfo info)
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
    }
}
