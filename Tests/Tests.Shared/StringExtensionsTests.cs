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

namespace Tests.Database
{
    /// <summary>
    /// Tests for the standalone (non-query) versions of our string extensions.
    /// </summary>
    [TestFixture, Preserve(AllMembers = true)]
    public class StringExtensionsTests
    {
        [TestCaseSource(nameof(ContainsTestValues))]
        public void ContainsTests(string str, string value, StringComparison comparisonType, bool expected)
        {
            var result = StringExtensions.Contains(str, value, comparisonType);
            Assert.That(result, Is.EqualTo(expected));
        }

        public static object[] ContainsTestValues =
        {
            new object[] { string.Empty, string.Empty, StringComparison.Ordinal, true },
            new object[] { string.Empty, string.Empty, StringComparison.OrdinalIgnoreCase, true },
            new object[] { "abc", "b", StringComparison.OrdinalIgnoreCase, true },
            new object[] { "abc", "B", StringComparison.OrdinalIgnoreCase, true },
            new object[] { "abc", "B", StringComparison.Ordinal, false },
            new object[] { "abc", "d", StringComparison.OrdinalIgnoreCase, false },
        };

        [TestCaseSource(nameof(LikeTestValues))]
        public void LikeTests(string str, string value, bool caseSensitive, bool expected)
        {
            var result = StringExtensions.Like(str, value, caseSensitive);
            Assert.That(result, Is.EqualTo(expected));
        }

        public static object[] LikeTestValues =
        {
            new object[] { string.Empty, string.Empty, true, true },
            new object[] { string.Empty, string.Empty, false, true },
            new object[] { null, null, true, true },
            new object[] { null, null, false, true },
            new object[] { "abc", string.Empty, true, false },
            new object[] { string.Empty, "abc", true, false },
            new object[] { "abcd", "abc", true, false },

            new object[] { "abc", "*a*", true, true },
            new object[] { "abc", "*b*", true, true },
            new object[] { "abc", "*c", true, true },
            new object[] { "abc", "ab*", true, true },
            new object[] { "abc", "*bc", true, true },
            new object[] { "abc", "a*bc", true, true },
            new object[] { "abc", "*abc*", true, true },
            new object[] { "abc", "*d*", true, false },
            new object[] { "abc", "aabc", true, false },
            new object[] { "abc", "b*bc", true, false },

            new object[] { "abc", "a??", true, true },
            new object[] { "abc", "?b?", true, true },
            new object[] { "abc", "*?c", true, true },
            new object[] { "abc", "ab?", true, true },
            new object[] { "abc", "?bc", true, true },
            new object[] { "abc", "?d?", true, false },
            new object[] { "abc", "?abc", true, false },
            new object[] { "abc", "b?bc", true, false },

            new object[] { "abc", "*C*", true, false },
            new object[] { "abc", "*c*", false, true },
            new object[] { "abc", "*C*", false, true },
        };
    }
}
