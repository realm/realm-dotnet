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
    /// Describes a single property of a class stored in a Realm.
    /// </summary>
    [DebuggerDisplay("Name = {Name}, Type = {Type}")]
    public struct Property
    {
        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the property.
        /// </summary>
        public PropertyType Type { get; set; }

        /// <summary>
        /// Gets or sets the type of the object.
        /// </summary>
        public string ObjectType { get; set; }

        /// <summary>
        /// Gets or sets the name of the property that links to the model containing this <see cref="PropertyType.LinkingObjects"/> property.
        /// </summary>
        public string LinkOriginPropertyName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Property"/> is nullable.
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Property"/> is primary key.
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Property"/> is indexed.
        /// </summary>
        public bool IsIndexed { get; set; }

        internal PropertyInfo PropertyInfo;
    }
}