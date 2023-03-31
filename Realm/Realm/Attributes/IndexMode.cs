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
    /// <summary>
    /// Describes the indexing mode for properties annotated with the <see cref="IndexedAttribute"/>.
    /// </summary>
    public enum IndexMode
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
        /// <example>
        /// <code>
        /// var cars = realm.All&lt;Car&gt;().Filter("Description TEXT 'vintage sport red'");
        /// </code>
        /// </example>
        FullText = 2,
    }
}
