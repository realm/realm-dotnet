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

        protected List<Diagnostic> GetDiagnosticsForClass(string className)
        {
            var fileName = Path.Combine(TestClassesPath, $"{className}.diagnostics");

            if (!File.Exists(fileName))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<List<Diagnostic>>(File.ReadAllText(fileName));
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

        //TODO This should be checking only diagnostics (need to find a way to retrieve the diagnostics generated, so I can serialize them to a file and 
        // automate this
        public async Task RunSimpleErrorTest(string className)
        {
            var source = GetSourceForClass(className);
            //var diagnostics = GetDiagnosticsForClass(className);

            //var single = diagnostics.First();

            var d = new DiagnosticResult("REALM001", DiagnosticSeverity.Error).WithSpan(22,5,26,6).WithMessage("TestMessage");

            await new RealmClassGeneratorVerifier.Test
            {
                TestState =
                {
                    Sources =
                    {
                        source
                    },
                    ExpectedDiagnostics =
                    {
                        d
                    }
                },
            }.RunAsync();
        }

        // Utility methods to retrieve only the list of diagnostics generated.
        public IEnumerable<Diagnostic> GetDiagnostics(string className)
        {
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

            var result = driver.GetRunResult();
            result.Diagnostics[0].

            //TODO We need to create a method that will create a diagnostic result struct from this.

            return diagnostics;
        }
    }
}
