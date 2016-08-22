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
    /// Public description of a class stored in a Realm, as a collection of managed Property objects.
    /// </summary>
    public class ObjectSchema : IReadOnlyCollection<Property>
    {
        /// <summary>
        /// Name of the original class declaration from which the schema was built.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Number of properties in the schema, which is the persistent properties from the original class.
        /// </summary>
        public int Count { get; }

        private ObjectSchema()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
        }

        /// <summary>
        /// Looks for a Property by Name. Failure to find means it is not regarded as a property to persist in a Realm.
        /// </summary>
        /// <returns><c>true</c>, if a property was found matching Name, <c>false</c> otherwise.</returns>
        /// <param name="name">Name of the Property to match exactly.</param>
        /// <param name="property">Property returned only if found matching Name.</param>
        public bool TryFindProperty(string name, out Property property)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            property = new Property();
            return false;
        }

        /// <summary>
        /// Property enumerator factory for an iterator to be called explicitly or used in a foreach loop.
        /// </summary>
        /// <returns>An enumerator over the list of Property instances described in the schema.</returns>
        public IEnumerator<Property> GetEnumerator()
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Creates a schema describing a RealmObject subclass in terms of its persisted members.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if no class Type is provided or if it doesn't descend directly from RealmObject.</exception>
        /// <returns>An ObjectSchema describing the specified Type.</returns>
        /// <param name="type">Type of a RealmObject descendant for which you want a schema.</param>
        public static ObjectSchema FromType(Type type)
        {
            RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            return null;
        }

        /// <summary>
        /// Helper class used to construct an ObjectSchema.
        /// </summary>
        public class Builder : List<Property>
        {
            /// <summary>
            /// Name of the class to be returned in the ObjectSchema.
            /// </summary>
            public string Name { get; }

            public Builder(string name)
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
            }

            /// <summary>
            /// Build the ObjectSchema to include all Property instances added to this Builder.
            /// </summary>
            /// <exception cref="InvalidOperationException">Thrown if the Builder is empty.</exception>
            /// <returns>A completed ObjectSchema, suitable for composing a RealmSchema that will be used to create a new Realm.</returns>
            public ObjectSchema Build()
            {
                RealmPCLHelpers.ThrowProxyShouldNeverBeUsed();
                return null;
            }
        }
    }
}

