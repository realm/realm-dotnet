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
using System.IO;
using System.Threading.Tasks;
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

        protected string GetGeneratedForClass(string className) =>
            File.ReadAllText(Path.Combine(TestClassesPath, $"{className}_generated.cs"));

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
    }
}
