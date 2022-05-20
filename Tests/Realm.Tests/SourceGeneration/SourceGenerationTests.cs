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
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using Realm.SourceGenerator;
using VerifyCS = Realms.Tests.SourceGeneration.CSharpSourceGeneratorVerifier<Realm.SourceGenerator.CustomGenerator>;

namespace Realms.Tests.SourceGeneration
{
    [TestFixture, Preserve(AllMembers = true)]
    public class SourceGenerationTests
    {
        [Test]
        public void SimpleGeneratorTest()
        {
            // Create the 'input' compilation that the generator will act on
            Compilation inputCompilation = CreateCompilation(@"
namespace MyCode
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }
}
");

            // directly create an instance of the generator
            // (Note: in the compiler this is loaded from an assembly, and created via reflection at runtime)
            CustomGenerator generator = new CustomGenerator();

            // Create the driver that will control the generation, passing in our generator
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

            // Run the generation pass
            // (Note: the generator driver itself is immutable, and all calls return an updated version of the driver that you should use for subsequent calls)
            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            // We can now assert things about the resulting compilation:
            Assert.That(diagnostics, Is.Empty); // there were no diagnostics created by the generators
            Assert.That(outputCompilation.SyntaxTrees.Count(), Is.EqualTo(2)); // we have two syntax trees, the original 'user' provided one, and the one added by the generator
            Assert.That(outputCompilation.GetDiagnostics(), Is.Empty); // verify the compilation with the added source has no diagnostics

            // Or we can look at the results directly:
            GeneratorDriverRunResult runResult = driver.GetRunResult();

            // The runResult contains the combined results of all generators passed to the driver
            Assert.That(runResult.GeneratedTrees.Length == 1);
            Assert.That(runResult.Diagnostics, Is.Empty);

            // Or you can access the individual results on a by-generator basis
            GeneratorRunResult generatorResult = runResult.Results[0];
            Assert.That(generatorResult.Generator, Is.EqualTo(generator));
            Assert.That(generatorResult.Diagnostics, Is.Empty);
            Assert.That(generatorResult.GeneratedSources.Length, Is.EqualTo(1));
            Assert.That(generatorResult.Exception, Is.Null);
        }

        private static Compilation CreateCompilation(string source)
            => CSharpCompilation.Create("compilation",
                new[] { CSharpSyntaxTree.ParseText(source) },
                new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));

        [Test]
        public async Task RecommendedGeneratorTest()
        {
            var code = "initial code";
            var generated = SourceText.From(@"
namespace GeneratedNamespace
{
    public class GeneratedClass
    {
        public static void GeneratedMethod()
        {
            // generated code
        }
    }
}", Encoding.UTF8);

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        code
                    },
                    GeneratedSources =
                    {
                        (typeof(CustomGenerator), "myGeneratedFile,cs", generated),
                    },
                },
            }.RunAsync();
        }
    }
}
