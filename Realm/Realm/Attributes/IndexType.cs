////////////////////////////////////////////////////////////////////////////
//
// Copyright 2023 Realm Inc.
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

namespace Realms
{
#if PRIVATE_INDEXTYPE
    internal enum IndexType : int

#else
    /// <summary>
    /// Describes the indexing mode for properties annotated with the <see cref="IndexedAttribute"/>.
    /// </summary>
    public enum IndexType : int
#endif
    {
        /// <summary>
        /// Indicates that the property is not indexed.
        /// </summary>
        None = 0,

        /// <summary>
        /// Describes a regular index with no special capabilities.
        /// </summary>
        General = 1,

        /// <summary>
        /// Describes a Full-Text index on a string property.
        /// </summary>
        /// <remarks>
        /// Only <see cref="string"/> properties can be marked with this attribute.
        /// <br/>
        /// The full-text index currently support this set of features:
        /// <list type="bullet">
        /// <item>
        /// Only token or word search, e.g. <c>QueryMethods.FullTextSearch(o.Bio, "computer dancing")</c>
        /// will find all objects that contains the words <c>computer</c> and <c>dancing</c> in their <c>Bio</c> property.
        /// </item>
        /// <item>
        /// Tokens are diacritics- and case-insensitive, e.g. <c>QueryMethods.FullTextSearch(o.Bio, "cafe dancing")</c>
        /// and <c>QueryMethods.FullTextSearch(o.Bio, "café DANCING")</c> will return the same set of matches.
        /// </item>
        /// <item>
        /// Ignoring results with certain tokens is done using <c>-</c>, e.g. <c>QueryMethods.FullTextSearch(o.Bio, "computer -dancing")</c>
        /// will find all objects that contain <c>computer</c> but not <c>dancing</c>.
        /// </item>
        /// <item>
        /// Tokens only consist of alphanumerical characters from ASCII and the Latin-1 supplement. All other characters
        /// are considered whitespace. In particular words using <c>-</c> like <c>full-text</c> are split into two tokens.
        /// </item>
        /// </list>
        /// <br/>
        /// Note the following constraints before using full-text search:
        /// <list type="bullet">
        /// <item>
        /// Token prefix or suffix search like <c>QueryMethods.FullTextSearch(o.Bio, "comp* *cing")</c> is not supported.
        /// </item>
        /// <item>
        /// Only ASCII and Latin-1 alphanumerical chars are included in the index (most western languages).
        /// </item>
        /// <item>
        /// Only boolean match is supported, i.e. "found" or "not found". It is not possible to sort results by "relevance".
        /// </item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// var cars = realm.All&lt;Car&gt;().Where(c => QueryMethods.FullTextSearch(o.Description, "vintage sport red"));
        /// var cars = realm.All&lt;Car&gt;().Filter("Description TEXT $0", "vintage sport red");
        /// </code>
        /// </example>
        FullText = 2,
    }
}
