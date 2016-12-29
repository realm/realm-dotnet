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
using System.Collections;
using System.Collections.Generic;

namespace Realms.Schema
{
    /// <summary>
    /// Public description of a class stored in a <see cref="Realm"/>, as a collection of managed <see cref="Property"/> objects.
    /// </summary>
    public class ObjectSchema : IReadOnlyCollection<Property>
    {
        /// <summary>
        /// Gets the name of the original class declaration from which the schema was built.
        /// </summary>
        /// <value>The name of the class.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the number of properties in the schema, which is the persistent properties from the original class.
        /// </summary>
        /// <value>The number of persistent properties for the object.</value>
        public int Count { get; }

        private ObjectSchema()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        /// <summary>
        /// Looks for a <see cref="Property"/> by <see cref="Property.Name"/>.
        /// Failure to find means it is not regarded as a property to persist in a <see cref="Realm"/>.
        /// </summary>
        /// <returns><c>true</c>, if a <see cref="Property"/> was found matching <see cref="Property.Name"/>;
        /// <c>false</c> otherwise.</returns>
        /// <param name="name"><see cref="Property.Name"/> of the <see cref="Property"/> to match exactly.</param>
        /// <param name="property"><see cref="Property"/> returned only if found matching Name.</param>
        public bool TryFindProperty(string name, out Property property)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            property = new Property();
            return false;
        }

        /// <inheritdoc/>
        public IEnumerator<Property> GetEnumerator()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Creates a schema describing a <see cref="RealmObject"/> subclass in terms of its persisted members.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Thrown if no class Type is provided or if it doesn't descend directly from <see cref="RealmObject"/>.
        /// </exception>
        /// <returns>An <see cref="ObjectSchema"/> describing the specified Type.</returns>
        /// <param name="type">Type of a <see cref="RealmObject"/> descendant for which you want a schema.</param>
        public static ObjectSchema FromType(Type type)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }
    }
}