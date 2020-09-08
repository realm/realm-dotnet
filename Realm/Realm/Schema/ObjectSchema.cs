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
using System.Linq;
using System.Reflection;
using Realms.Helpers;

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
        /// Gets the name of the original class declaration from which the schema was built.
        /// </summary>
        /// <value>The name of the class.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the number of properties in the schema, which is the persistent properties from the original class.
        /// </summary>
        /// <value>The number of persistent properties for the object.</value>
        public int Count => _properties.Count;

        internal Property? PrimaryKeyProperty { get; }

        internal TypeInfo Type { get; private set; }

        internal bool IsEmbedded { get; private set; }

        internal IEnumerable<string> PropertyNames => _properties.Keys;

        private ObjectSchema(string name, IDictionary<string, Property> properties)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Object name cannot be empty", nameof(name));
            }

            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            Name = name;
            _properties = new ReadOnlyDictionary<string, Property>(properties);
            var primaryKeyKvp = properties.FirstOrDefault(kvp => kvp.Value.IsPrimaryKey);
            if (primaryKeyKvp.Key != null)
            {
                PrimaryKeyProperty = primaryKeyKvp.Value;
            }
        }

        /// <summary>
        /// Looks for a <see cref="Property"/> by <see cref="Property.Name"/>.
        /// Failure to find means it is not regarded as a property to persist in a <see cref="Realm"/>.
        /// </summary>
        /// <returns><c>true</c>, if a <see cref="Property"/> was found matching <see cref="Property.Name"/>;
        /// <c>false</c> otherwise.</returns>
        /// <param name="name"><see cref="Property.Name"/> of the <see cref="Property"/> to match exactly.</param>
        /// <param name="property"><see cref="Property"/> returned only if found matching Name.</param>
        public bool TryFindProperty(string name, out Property property) => _properties.TryGetValue(name, out property);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<Property> GetEnumerator() => _properties.Values.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Creates a schema describing a <see cref="RealmObject"/> or <see cref="EmbeddedObject"/> subclass in terms of its persisted members.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Thrown if no class Type is provided or if it doesn't descend directly from <see cref="RealmObject"/>/<see cref="EmbeddedObject"/>.
        /// </exception>
        /// <returns>An <see cref="ObjectSchema"/> describing the specified Type.</returns>
        /// <param name="type">Type of a <see cref="RealmObject"/>/<see cref="EmbeddedObject"/> descendant for which you want a schema.</param>
        public static ObjectSchema FromType(TypeInfo type)
        {
            Argument.NotNull(type, nameof(type));

            Argument.Ensure(type.BaseType == typeof(RealmObject) || type.BaseType == typeof(EmbeddedObject), $"The class {type.FullName} must descend directly from RealmObject", nameof(type));

            var builder = new Builder(type.GetMappedOrOriginalName(), isEmbedded: type.BaseType == typeof(EmbeddedObject));
            foreach (var property in type.DeclaredProperties.Where(p => !p.IsStatic() && p.HasCustomAttribute<WovenPropertyAttribute>()))
            {
                var isPrimaryKey = property.HasCustomAttribute<PrimaryKeyAttribute>();
                var schemaProperty = new Property
                {
                    Name = property.GetMappedOrOriginalName(),
                    IsPrimaryKey = isPrimaryKey,
                    IsIndexed = isPrimaryKey || property.HasCustomAttribute<IndexedAttribute>(),
                    PropertyInfo = property
                };

                var backlinks = property.GetCustomAttribute<BacklinkAttribute>();
                if (backlinks != null)
                {
                    var innerType = property.PropertyType.GenericTypeArguments.Single();
                    var linkOriginProperty = innerType.GetProperty(backlinks.Property);

                    schemaProperty.Type = PropertyType.LinkingObjects | PropertyType.Array;
                    schemaProperty.ObjectType = innerType.GetTypeInfo().GetMappedOrOriginalName();
                    schemaProperty.LinkOriginPropertyName = linkOriginProperty.GetMappedOrOriginalName();
                }
                else
                {
                    schemaProperty.Type = property.PropertyType.ToPropertyType(out var objectType);
                    schemaProperty.ObjectType = objectType?.GetTypeInfo().GetMappedOrOriginalName();
                }

                if (property.HasCustomAttribute<RequiredAttribute>())
                {
                    schemaProperty.Type &= ~PropertyType.Nullable;
                }

                builder.Add(schemaProperty);
            }

            var ret = builder.Build();
            ret.Type = type;
            return ret;
        }

        internal class Builder : List<Property>
        {
            public string Name { get; }

            private readonly bool _isEmbedded;

            public Builder(string name, bool isEmbedded)
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new ArgumentException("Object name cannot be empty", nameof(name));
                }

                Name = name;
                _isEmbedded = isEmbedded;
            }

            public ObjectSchema Build()
            {
                if (Count == 0)
                {
                    throw new InvalidOperationException(
                        $"No properties in {Name}, has linker stripped it? See https://realm.io/docs/xamarin/latest/#linker-stripped-schema");
                }

                return new ObjectSchema(Name, this.ToDictionary(p => p.Name))
                {
                    IsEmbedded = _isEmbedded
                };
            }
        }
    }
}