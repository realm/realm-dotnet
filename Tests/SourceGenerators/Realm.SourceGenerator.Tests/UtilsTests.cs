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

using Realms.SourceGenerator;

namespace Realm.SourceGenerator.Tests
{
    [TestFixture]
    internal class UtilsTests
    {
        public static (string Input, string Expected)[] IndentTestCases = new[]
        {
            ("", ""),
            ("abc", $"{Utils.DefaultIndent}abc"),
            ($"a{Environment.NewLine}b", $"{Utils.DefaultIndent}a{Environment.NewLine}{Utils.DefaultIndent}b"),
            ($"  abc{Environment.NewLine}{Environment.NewLine}", $"{Utils.DefaultIndent}  abc{Environment.NewLine}{Environment.NewLine}"),
            ($"a{Environment.NewLine}{Environment.NewLine}b", $"{Utils.DefaultIndent}a{Environment.NewLine}{Environment.NewLine}{Utils.DefaultIndent}b"),
        };

        [TestCaseSource(nameof(IndentTestCases))]
        public void IndentWorksAsIntended((string Input, string Expected) test)
        {
            var output = test.Input.Indent();
            Assert.That(output, Is.EqualTo(test.Expected));
        }
    }
}
