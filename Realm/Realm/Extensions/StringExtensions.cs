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
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace Realms
{
    /// <summary>
    /// A set of extensions methods over strings, useable in LINQ queries.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class StringExtensions
    {
#if !NETCOREAPP2_1_OR_GREATER
        /// <summary>
        /// Returns a value indicating whether a specified substring occurs within this string.
        /// </summary>
        /// <param name="str">The original string.</param>
        /// <param name="value">The string to seek.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how this string and value are compared.</param>
        /// <returns><c>true</c> if the value parameter occurs within this string, or if value is the empty string (""); otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <c>str</c> or <c>value</c> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <c>comparisonType</c> is not a valid <see cref="StringComparison"/> value.</exception>
        [Obsolete("Use QueryMethods.Contains instead")]
        public static bool Contains(this string str, string value, StringComparison comparisonType)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            return str.IndexOf(value, comparisonType) >= 0;
        }
#endif

        /// <summary>
        /// Performs a 'like' comparison between the specified string and pattern.
        /// </summary>
        /// <remarks>
        /// <c>?</c> and <c>*</c> are allowed where <c>?</c> matches a single character and <c>*</c> matches zero or
        /// more characters, such that <c>?bc*</c> matches <c>abcde</c> and <c>bbc</c>, but does not match <c>bcd</c>.
        /// <para/>
        /// This extension method can be used in LINQ queries against the <see cref="IQueryable"/> returned from
        /// <see cref="Realm.All{T}"/>. If used outside of a query context, it will use a <see cref="Regex"/> to perform
        /// the comparison using the same rules.
        /// </remarks>
        /// <param name="str">The string to compare against the pattern.</param>
        /// <param name="pattern">The pattern to compare against.</param>
        /// <param name="caseSensitive">If set to <c>true</c> performs a case sensitive comparison.</param>
        /// <returns><c>true</c>  if the string matches the pattern, <c>false</c> otherwise.</returns>
        [Obsolete("Use QueryMethods.Like instead")]
        public static bool Like(this string str, string pattern, bool caseSensitive = true)
        {
            if (str == null)
            {
                return pattern == null;
            }

            pattern = Regex.Escape(pattern)
                           .Replace("\\?", ".")
                           .Replace("\\*", ".*");

            var options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
            return Regex.Match(str, $"^{pattern}$", options).Success;
        }

        internal static IntPtr IntPtrLength(this string? str) => (IntPtr)(str?.Length ?? 0);
    }
}
