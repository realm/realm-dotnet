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

using System.Threading.Tasks;
using NUnit.Framework;

namespace SourceGeneratorTests
{
    //TODO Move them out, we don't need to test them on each platform
    [TestFixture]
    internal class ComparisonTests : SourceGenerationTest
    {
        [TestCase("AllTypesClass")]
        public async Task SimpleComparisonTest(string className)
        {
            await RunSimpleComparisonTest(className);
        }

        [TestCase("PersonWithDog", "Person", "Dog")]
        public async Task ComparisonTest(string filename, params string[] classNames)
        {
            await RunComparisonTest(filename, classNames);
        }

        [Test]
        public async Task ErrorTest()
        {
            await RunSimpleErrorTest("NoPartialClass");
        }
    }
}
