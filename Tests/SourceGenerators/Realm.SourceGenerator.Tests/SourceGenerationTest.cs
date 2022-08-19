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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Newtonsoft.Json;
using Realms.SourceGenerator;
using RealmGeneratorVerifier = SourceGeneratorTests.CSharpSourceGeneratorVerifier<Realms.SourceGenerator.RealmGenerator>;

namespace SourceGeneratorTests
{
    internal abstract class SourceGenerationTest
    {
        private string _testClassesPath;
        private string _errorClassesPath;
        private string _supportClassesPath;
        private string _generatedFilesPath;

        [OneTimeSetUp]
        public void Setup()
        {
            var buildFolder = Path.GetDirectoryName(typeof(SourceGenerationTest).Assembly.Location);
            var testFolder = buildFolder.Substring(0, buildFolder.IndexOf("Realm.SourceGenerator.Test", StringComparison.InvariantCulture));
            var assemblyToProcessFolder = Path.Combine(testFolder, "SourceGeneratorAssemblyToProcess");
            _testClassesPath = Path.Combine(assemblyToProcessFolder, "TestClasses");
            _errorClassesPath = Path.Combine(assemblyToProcessFolder, "ErrorClasses");
            _supportClassesPath = Path.Combine(assemblyToProcessFolder, "SupportClasses");
            _generatedFilesPath = Path.Combine(assemblyToProcessFolder, "Generated",
                "Realm.SourceGenerator", "Realms.SourceGenerator.RealmGenerator");
            Environment.SetEnvironmentVariable("NO_GENERATOR_DIAGNOSTICS", "true");
        }

        private string GetSource(string filename, ClassFolder classFolder)
        {
            var folder = classFolder switch
            {
                ClassFolder.Test => _testClassesPath,
                ClassFolder.Error => _errorClassesPath,
                ClassFolder.Support => _supportClassesPath,
                _ => throw new NotImplementedException(),
            };

            return File.ReadAllText(Path.Combine(folder, $"{filename}.cs"));
        }

        private string GetGeneratedForClass(string className)
        {
            var fileName = Path.Combine(_generatedFilesPath, $"{className}_generated.cs");
            return File.Exists(fileName) ? File.ReadAllText(fileName) : string.Empty;
        }

        private List<DiagnosticInfo> GetDiagnosticsForClass(string className)
        {
            var fileName = Path.Combine(_generatedFilesPath, $"{className}.diagnostics.cs");

            if (!File.Exists(fileName))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<List<DiagnosticInfo>>(File.ReadAllText(fileName));
        }

        private string GetDiagnosticFile(string className)
        {
            var fileName = Path.Combine(_generatedFilesPath, $"{className}.diagnostics.cs");

            if (!File.Exists(fileName))
            {
                return null;
            }

            return File.ReadAllText(fileName);
        }

        protected async Task RunComparisonTest(string fileName, params string[] classNames)
        {
            var source = GetSource(fileName, ClassFolder.Test);

            var test = new RealmGeneratorVerifier.Test();
            test.TestState.Sources.Add(source);

            foreach (var className in classNames)
            {
                var generated = GetGeneratedForClass(className);
                var generatedFileName = $"{className}_generated.cs";

                test.TestState.GeneratedSources.Add((typeof(RealmGenerator), generatedFileName, generated));
            }

            await test.RunAsync();
        }

        protected async Task RunSimpleComparisonTest(string className)
        {
            await RunComparisonTest(className, className);
        }

        protected async Task RunErrorTest(string fileName, params string[] classNames)
        {
            var source = GetSource(fileName, ClassFolder.Error);

            var test = new RealmGeneratorVerifier.Test();
            test.TestState.Sources.Add(source);

            foreach (var className in classNames)
            {
                var diagnosticFileName = $"{className}.diagnostics.cs";
                var diagnostics = GetDiagnosticsForClass(className);

                test.TestState.ExpectedDiagnostics.AddRange(diagnostics.Select(Convert));
            }

            await test.RunAsync();
        }

        protected async Task RunSimpleErrorTest(string className)
        {
            await RunErrorTest(className, className);
        }

        private static DiagnosticResult Convert(DiagnosticInfo info)
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

        private enum ClassFolder
        {
            Test,
            Error,
            Support,
        }
    }
}
