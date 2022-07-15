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
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Newtonsoft.Json;
using NUnit.Framework;
using Realms.SourceGenerator;
using RealmClassGeneratorVerifier = Realms.Tests.SourceGeneration.CSharpSourceGeneratorVerifier<Realms.SourceGenerator.RealmClassGenerator>;

namespace Realms.Tests.SourceGeneration
{
    public abstract class SourceGenerationTest
    {
        private string _testClassesPath;

        private string _generatedFilesPath;

        [OneTimeSetUp]
        public void Setup()
        {
            var buildFolder = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            var testFolder = buildFolder.Substring(0, buildFolder.IndexOf("Realm.Tests", StringComparison.InvariantCulture));
            _testClassesPath = Path.Combine(testFolder, "Realm.Tests.SourceGeneratorPlayground");
            _generatedFilesPath = Path.Combine(_testClassesPath, "Generated",
                "Realm.SourceGenerator", "Realm.SourceGenerator.RealmClassGenerator");
            Environment.SetEnvironmentVariable("NO_GENERATOR_DIAGNOSTICS", "true");
        }

        protected string GetSourceForClass(string className) =>
            File.ReadAllText(Path.Combine(_testClassesPath, $"{className}.cs"));

        protected string GetGeneratedForClass(string className)
        {
            var fileName = Path.Combine(_testClassesPath, $"{className}_generated.cs");
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

        //TODO Probably it makes sense to make methods that can accept multiple class names (in case there are multiple classes in the same file)
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

        //TODO Probably I can make a generic method and make thise and the previous in relation to that
        public async Task RunSimpleErrorTest(string className)
        {
            var source = GetSourceForClass(className);
            var diagnosticFileName = $"{className}.diagnostics.cs";

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

            test.TestState.ExpectedDiagnostics.AddRange(diagnostics.Select(Convert));

            await test.RunAsync();
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

        //TODO Can we keep this in one place? (it's in the source generators project)
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
