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

using Microsoft.CodeAnalysis.Testing;
using Newtonsoft.Json;
using Realms.SourceGenerator;
using RealmGeneratorVerifier = SourceGeneratorTests.CSharpSourceGeneratorVerifier<Realms.SourceGenerator.RealmGenerator>;

namespace SourceGeneratorTests
{
    internal abstract class SourceGenerationTest
    {
        private static readonly string _buildFolder = Path.GetDirectoryName(typeof(SourceGenerationTest).Assembly.Location)!;
        private static readonly string _testFolder = _buildFolder.Substring(0, _buildFolder.IndexOf("Realm.SourceGenerator.Test", StringComparison.InvariantCulture));
        private static readonly string _assemblyToProcessFolder = Path.Combine(_testFolder, "SourceGeneratorAssemblyToProcess");

        private static readonly string _testClassesPath = Path.Combine(_assemblyToProcessFolder, "TestClasses");
        protected static readonly string _errorClassesPath = Path.Combine(_assemblyToProcessFolder, "ErrorClasses");
        private static readonly string _supportClassesPath = Path.Combine(_assemblyToProcessFolder, "SupportClasses");
        private static readonly string _generatedFilesPath = Path.Combine(_assemblyToProcessFolder, "Generated",
            "Realm.SourceGenerator", "Realms.SourceGenerator.RealmGenerator");

        private readonly string[] _supportClasses = {
            "RealmObj",
            "EmbeddedObj",
        };

        [OneTimeSetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("NO_GENERATOR_DIAGNOSTICS", "true");
        }

        protected static string GetGeneratedFileNameForClass(string className) => $"{className}_generated.cs";

        protected static string GetDiagnosticFileNameForClass(string className) => $"{className}.diagnostics.cs";

        protected string GetSource(string filename, ClassFolder classFolder)
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

        protected string GetGeneratedForClass(string className)
        {
            var fileName = Path.Combine(_generatedFilesPath, GetGeneratedFileNameForClass(className));
            return File.Exists(fileName) ? File.ReadAllText(fileName) : throw new Exception($"File not found: {fileName}");
        }

        protected List<DiagnosticInfo> GetDiagnosticsForClass(string className)
        {
            var fileName = Path.Combine(_generatedFilesPath, GetDiagnosticFileNameForClass(className));
            return File.Exists(fileName) ? JsonConvert.DeserializeObject<List<DiagnosticInfo>>(File.ReadAllText(fileName))! : throw new Exception($"File not found: {fileName}");
        }

        protected async Task RunComparisonTest(string fileName, IEnumerable<string> classNames)
        {
            if (!classNames.Any())
            {
                classNames = new[] { fileName };
            }

            var source = GetSource(fileName, ClassFolder.Test);

            var test = new RealmGeneratorVerifier.Test();
            test.TestState.Sources.Add(source);

            foreach (var className in classNames)
            {
                var generated = GetGeneratedForClass(className);
                var generatedFileName = GetGeneratedFileNameForClass(className);

                test.TestState.GeneratedSources.Add((typeof(RealmGenerator), generatedFileName, generated));
            }

            await test.RunAsync();
        }

        protected async Task RunErrorTest(string fileName, IEnumerable<string> classNames)
        {
            if (!classNames.Any())
            {
                classNames = new[] { fileName };
            }

            var source = GetSource(fileName, ClassFolder.Error);

            var test = new RealmGeneratorVerifier.Test();
            test.TestState.Sources.Add(source);

            foreach (var className in classNames)
            {
                var diagnostics = GetDiagnosticsForClass(className);

                test.TestState.ExpectedDiagnostics.AddRange(diagnostics.Select(Convert));
            }

            AddSupportClasses(test);

            await test.RunAsync();
        }

        protected static string BuildGlobalOptions(Dictionary<string, string> globalOptions)
        {
            return string.Join(Environment.NewLine, globalOptions.Select(go => $"{go.Key} = {go.Value}"));
        }

        private void AddSupportClasses(RealmGeneratorVerifier.Test test)
        {
            foreach (var supportClassName in _supportClasses)
            {
                var source = GetSource(supportClassName, ClassFolder.Support);
                test.TestState.Sources.Add(source);

                var generated = GetGeneratedForClass(supportClassName);
                var generatedFileName = GetGeneratedFileNameForClass(supportClassName);

                test.TestState.GeneratedSources.Add((typeof(RealmGenerator), generatedFileName, generated));
            }
        }

        protected static DiagnosticResult Convert(DiagnosticInfo info)
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

        protected enum ClassFolder
        {
            Test,
            Error,
            Support,
        }
    }
}
