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

using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Realms.SourceGenerator;
using RealmGeneratorVerifier = SourceGeneratorTests.CSharpSourceGeneratorVerifier<Realms.SourceGenerator.RealmGenerator>;

namespace SourceGeneratorTests
{
    [TestFixture]
    internal class ComparisonTests : SourceGenerationTest
    {
        public record ComparisonTestInfo(string File, string[] ClassNames)
        {
            public override string ToString() => File;
        }

        private static readonly Regex _classNameRegex = new(@"class (?<ClassName>[^\s]*) :.*I(Realm|Embedded|Asymmetric)Object");

        public static ComparisonTestInfo[] SuccessTestCases => GetTestInfos(_testClassesPath);
        public static ComparisonTestInfo[] ErrorTestCases => GetTestInfos(_errorClassesPath);

        [TestCaseSource(nameof(SuccessTestCases))]
        public async Task ComparisonTest(ComparisonTestInfo testCase)
        {
            await RunComparisonTest(testCase.File, testCase.ClassNames);
        }

        [TestCaseSource(nameof(ErrorTestCases))]
        public async Task ErrorComparisonTest(ComparisonTestInfo testCase)
        {
            await RunErrorTest(testCase.File, testCase.ClassNames);
        }

        [Test]
        public async Task IgnoreObjectNullabilityTest()
        {
            var options = new Dictionary<string, string>
            {
                ["realm.ignore_objects_nullability"] = "true"
            };

            var className = "IgnoreObjectNullabilityClass";
            var generated = GetGeneratedForClass(className);
            var generatedFileName = GetGeneratedFileNameForClass(className);

            var source = GetSource(className, ClassFolder.Error);

            var test = new RealmGeneratorVerifier.Test();
            test.TestState.Sources.Add(source);
            test.TestState.GeneratedSources.Add((typeof(RealmGenerator), generatedFileName, generated));
            test.TestState.AnalyzerConfigFiles.Add(("/.globalConfig", BuildGlobalOptions(options)));

            await test.RunAsync();
        }

        [Test]
        public async Task OldCSharpVersionTest()
        {
            var className = "AllTypesClass";
            var source = GetSource(className, ClassFolder.Test);
            var error = new DiagnosticResult("RLM100", DiagnosticSeverity.Error)
        .WithMessage("It is not possible to use the Realm source generator with C# versions older than 8.0.");

            var test = new RealmGeneratorVerifier.Test();
            test.LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp7;
            test.TestState.Sources.Add(source);
            test.TestState.ExpectedDiagnostics.Add(error);

            await test.RunAsync();
        }

        private static ComparisonTestInfo[] GetTestInfos(string folder)
            => Directory.GetFiles(folder)
                .Select(f =>
                {
                    var filename = Path.GetFileNameWithoutExtension(f);
                    var content = File.ReadAllText(f);
                    var classNames = _classNameRegex.Matches(content)
                        .Select(m => m.Groups["ClassName"].Value)
                        .Distinct()
                        .ToArray();

                    return new ComparisonTestInfo(filename, classNames);
                })
                .ToArray();
    }
}
