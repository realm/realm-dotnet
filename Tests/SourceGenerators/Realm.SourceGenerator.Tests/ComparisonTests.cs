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

namespace SourceGeneratorTests
{
    [TestFixture]
    internal class ComparisonTests : SourceGenerationTest
    {
        [TestCase("AllTypesClass")]
        [TestCase("ClassWithoutParameterlessConstructor")]
        [TestCase("DifferentNamespaces", "NamespaceObj", "OtherNamespaceObj")]
        [TestCase("NoNamespaceClass")]
        [TestCase("PartialClass")]
        [TestCase("AutomaticPropertiesClass")]
        [TestCase("ConfusingNamespaceClass")]
        [TestCase("PersonWithDog", "Person", "Dog")]
        public async Task ComparisonTest(string filename, params string[] classNames)
        {
            await RunComparisonTest(filename, classNames);
        }

        [TestCase("ClassWithBaseType")]
        [TestCase("MultiplePrimaryKeys")]
        [TestCase("NoPartialClass")]
        [TestCase("RealmintegerErrors")]
        [TestCase("RealmObjectAndEmbeddedObjectClass")]
        [TestCase("UnsupportedIndexableTypes")]
        [TestCase("UnsupportedPrimaryKeyTypes")]
        [TestCase("UnsupportedRequiredTypes")]
        [TestCase("NestedClass")]
        [TestCase("UnsupportedBacklink", "UnsupportedBacklink", "BacklinkObj")]
        public async Task ErrorComparisonTest(string filename, params string[] classNames)
        {
            await RunErrorTest(filename, classNames);
        }
    }
}
