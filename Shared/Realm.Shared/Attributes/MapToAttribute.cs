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

namespace Realms
{
    /// <summary>
    /// An attribute that indicates that a property should be persisted under a different name.
    /// </summary>
    /// <remarks>
    /// This is useful when opening a Realm across different bindings where code style conventions might differ.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public class MapToAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the property in the database.
        /// </summary>
        /// <value>The property name.</value>
        public string Mapping { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapToAttribute"/> class.
        /// </summary>
        /// <param name="mapping">The name of the property in the database.</param>
        public MapToAttribute(string mapping)
        {
            Mapping = mapping;
        }
    }
}