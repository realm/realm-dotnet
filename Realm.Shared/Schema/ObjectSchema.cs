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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Realms.Schema
{
    /// <summary>
    /// Public description of a class stored in a Realm, as a collection of managed Property objects.
    /// </summary>
    [DebuggerDisplay("Name = {Name}, Properties = {Count}")]
    public class ObjectSchema : IReadOnlyCollection<Property>
    {
        private readonly ReadOnlyDictionary<string, Property> _properties;

        /// <summary>
        /// Name of the original class declaration from which the schema was built.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Number of properties in the schema, which is the persistent properties from the original class.
        /// </summary>
        public int Count => _properties.Count;

        internal Type Type;

        internal IEnumerable<string> PropertyNames => _properties.Keys;

        private ObjectSchema(string name, IDictionary<string, Property> properties)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Object name cannot be empty", nameof(name));
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            Contract.EndContractBlock();

            Name = name;
            _properties = new ReadOnlyDictionary<string, Property>(properties);
        }

        /// <summary>
        /// Looks for a Property by Name. Failure to find means it is not regarded as a property to persist in a Realm.
        /// </summary>
        /// <returns><c>true</c>, if a property was found matching Name, <c>false</c> otherwise.</returns>
        /// <param name="name">Name of the Property to match exactly.</param>
        /// <param name="property">Property returned only if found matching Name.</param>
        public bool TryFindProperty(string name, out Property property) => _properties.TryGetValue(name, out property);

        /// <summary>
        /// Property enumerator factory for an iterator to be called explicitly or used in a foreach loop.
        /// </summary>
        /// <returns>An enumerator over the list of Property instances described in the schema.</returns>
        public IEnumerator<Property> GetEnumerator()
        {
            return _properties.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Creates a schema describing a RealmObject subclass in terms of its persisted members.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if no class Type is provided or if it doesn't descend directly from RealmObject.</exception>
        /// <returns>An ObjectSchema describing the specified Type.</returns>
        /// <param name="type">Type of a RealmObject descendant for which you want a schema.</param>
        public static ObjectSchema FromType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (type.BaseType != typeof(RealmObject)) throw new ArgumentException($"The class {type.FullName} must descend directly from RealmObject");
            Contract.EndContractBlock();

            var builder = new Builder(type.Name);
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (property.GetCustomAttribute<WovenPropertyAttribute>() == null) continue;

                bool isPrimaryKey = property.GetCustomAttribute<PrimaryKeyAttribute>() != null;
                var schemaProperty = new Property
                {
                    Name = property.GetCustomAttribute<MapToAttribute>()?.Mapping ?? property.Name,
                    IsPrimaryKey = isPrimaryKey,
                    IsIndexed = isPrimaryKey || property.GetCustomAttribute<IndexedAttribute>() != null,
                    PropertyInfo = property
                };

                Type innerType;
                bool isNullable;
                schemaProperty.Type = property.PropertyType.ToPropertyType(out isNullable, out innerType);
                schemaProperty.ObjectType = innerType?.Name;
                schemaProperty.IsNullable = isNullable;

                builder.Add(schemaProperty);
            }

            var ret = builder.Build();
            ret.Type = type;
            return ret;
        }

        /// <summary>
        /// Helper class used to construct an ObjectSchema.
        /// </summary>
        public class Builder : List<Property>
        {
            public string Name { get; }

            public Builder(string name)
            {
                if (string.IsNullOrEmpty(name)) throw new ArgumentException("Object name cannot be empty", nameof(name));
                Contract.EndContractBlock();

                Name = name;
            }

            /// <summary>
            /// Build the ObjectSchema to include all Property instances added to this Builder.
            /// </summary>
            /// <exception cref="InvalidOperationException">Thrown if the Builder is empty.</exception>
            /// <returns>A completed ObjectSchema, suitable for composing a RealmSchema that will be used to create a new Realm.</returns>
            public ObjectSchema Build()
            {
                if (Count == 0) 
                {
                    throw new InvalidOperationException(
                        $"No properties in {Name}, has linker stripped it? See https://realm.io/docs/xamarin/latest/#linker-stripped-schema");
                }
                Contract.EndContractBlock();

                return new ObjectSchema(Name, this.ToDictionary(p => p.Name));
            }
        }
    }
}

