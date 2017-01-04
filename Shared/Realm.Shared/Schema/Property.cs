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

using System.Diagnostics;
using System.Reflection;

namespace Realms.Schema
{
    /// <summary>
    /// Describes a single property of a class stored in a <see cref="Realm"/>.
    /// </summary>
    [DebuggerDisplay("Name = {Name}, Type = {Type}")]
    public struct Property
    {
        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>The name of the property.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the property.
        /// </summary>
        /// <value>The type of the property.</value>
        public PropertyType Type { get; set; }

        /// <summary>
        /// Gets or sets the type of the object.
        /// </summary>
        /// <value>The type of the object.</value>
        public string ObjectType { get; set; }

        /// <summary>
        /// Gets or sets the name of the property that links to the model containing this 
        /// <see cref="PropertyType.LinkingObjects"/> property.
        /// </summary>
        /// <value>The name of the linking property.</value>
        public string LinkOriginPropertyName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Property"/> can be <c>null</c>.
        /// </summary>
        /// <value>
        /// <c>true</c> if the property type allows <c>null</c> values and the matching property in the class definition
        /// is not marked with <see cref="RequiredAttribute"/>; <c>false</c> otherwise.
        /// </value>
        public bool IsNullable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Property"/> is primary key.
        /// </summary>
        /// <value>
        /// <c>true</c> if the property is primary key (the matching property in the class definition is
        /// marked with <see cref="PrimaryKeyAttribute"/>); <c>false</c> otherwise.</value>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Property"/> is indexed.
        /// </summary>
        /// <value>
        /// <c>true</c> if the property should be indexed (the matching property in the class definition is 
        /// marked with <see cref="IndexedAttribute"/>); <c>false</c> otherwise.</value>
        public bool IsIndexed { get; set; }

        internal PropertyInfo PropertyInfo;
    }
}