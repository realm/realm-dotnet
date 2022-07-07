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

        protected string GeneratedFilesPath { get; }

        public SourceGenerationTest()
        {
            var buildFolder = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            var testFolder = buildFolder.Substring(0, buildFolder.IndexOf("Realm.Tests", StringComparison.InvariantCulture));
            TestClassesPath = Path.Combine(testFolder, "Realm.Tests.SourceGeneratorPlayground");
            GeneratedFilesPath = Path.Combine(TestClassesPath, "GeneratedFiles");
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
            var fileName = Path.Combine(TestClassesPath, $"{className}.diagnostics.cs");

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

        //TODO Can we keep this in one place?
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
