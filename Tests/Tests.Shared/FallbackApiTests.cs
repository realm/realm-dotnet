////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
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

using System;
using NUnit.Framework;
using Realms;

namespace IntegrationTests
{
    [TestFixture, Preserve(AllMembers = true)]
    public class FallbackApiTests
    {
        public static object[] ContainsTestCases =
        {
            new object[] { "text", "x", StringComparison.Ordinal, true },
            new object[] { "teXt", "x", StringComparison.Ordinal, false },
            new object[] { "teXt", "X", StringComparison.Ordinal, true },
            new object[] { "teXt", "x", StringComparison.OrdinalIgnoreCase, true },
            new object[] { "teXt", "X", StringComparison.OrdinalIgnoreCase, true },
            new object[] { "Text", "t", StringComparison.Ordinal, true },
            new object[] { "Text", "T", StringComparison.Ordinal, true },
            new object[] { "texT", "ext", StringComparison.Ordinal, false },
            new object[] { "texT", "ext", StringComparison.OrdinalIgnoreCase, true },
            new object[] { "teXt", "test", StringComparison.Ordinal, false },
            new object[] { "teXt", "test", StringComparison.OrdinalIgnoreCase, false },
            new object[] { "teXt", string.Empty, StringComparison.OrdinalIgnoreCase, true },
            new object[] { "teXt", string.Empty, StringComparison.Ordinal, true },
            new object[] { string.Empty, "a", StringComparison.Ordinal, false },
            new object[] { string.Empty, "a", StringComparison.OrdinalIgnoreCase, false },
            new object[] { string.Empty, string.Empty, StringComparison.Ordinal, true },
            new object[] { string.Empty, string.Empty, StringComparison.OrdinalIgnoreCase, true }
        };

        public static object[] InvalidContainsTestCases =
        {
            new object[] { null, "x", StringComparison.Ordinal, typeof(ArgumentNullException) },
            new object[] { "text", null, StringComparison.Ordinal, typeof(ArgumentNullException) },
            new object[] { "teXt", "X", (StringComparison)123, typeof(ArgumentException) }
        };

        [TestCaseSource(nameof(ContainsTestCases))]
        public void TestContainsExtensionMethod(string original, string value, StringComparison comparisonType, bool expected)
        {
            var result = original.Contains(value, comparisonType);

            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(InvalidContainsTestCases))]
        public void Contains_TestInvalidArgumentCases(string original, string value, StringComparison comparisonType, Type exceptionType)
        {
            Assert.That(() => original.Contains(value, comparisonType), Throws.TypeOf(exceptionType));
        }
    }
}