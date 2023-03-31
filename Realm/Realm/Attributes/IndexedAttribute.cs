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
using Realms.Helpers;

namespace Realms
{
    /// <summary>
    /// An attribute that indicates an indexed property. Indexed properties slightly slow down insertions,
    /// but can greatly speed up queries.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IndexedAttribute : Attribute
    {
        /// <summary>
        /// Gets a value indicating the type of indexing that the database will perform. Default is <see cref="IndexMode.General"/>.
        /// </summary>
        /// <value>The index mode for the property.</value>
        public IndexMode Mode { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexedAttribute"/> class.
        /// </summary>
        public IndexedAttribute() : this(IndexMode.General)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexedAttribute"/> class.
        /// </summary>
        /// <param name="mode">The type of index that will be created.</param>
        public IndexedAttribute(IndexMode mode)
        {
            Argument.Ensure(
                mode != IndexMode.None,
                $"IndexMode.None is not valid when constructing an {nameof(IndexedAttribute)}. If you don't wish to index the property, remove the attribute altogether.",
                nameof(mode));
            Mode = mode;
        }
    }
}
