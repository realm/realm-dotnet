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
    /// Public description of a class stored in a Realm, as a collection of managed Property objects. To construct
    /// a new instance, use the <see cref="Builder">ObjectSchema.Builder</see> API.
    /// </summary>
    [DebuggerDisplay("Name = {Name}, Properties = {Count}")]
    public class ObjectSchema : IReadOnlyCollection<Property>
    {
        private static readonly IDictionary<TypeInfo, ObjectSchema> _cache = new Dictionary<TypeInfo, ObjectSchema>();

        private readonly ReadOnlyDictionary<string, Property> _properties;

        /// <summary>
        /// Gets the name of the original class declaration from which the schema was built.
        /// </summary>
        /// <value>The name of the class.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the number of properties in the schema, which is the persistent properties from the original class.
        /// </summary>
        /// <value>The number of persistent properties for the object.</value>
        public int Count => _properties.Count;

        internal Property? PrimaryKeyProperty { get; }

        internal TypeInfo Type { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ObjectSchema"/> describes an embedded object.
        /// </summary>
        /// <value><c>true</c> if the schema pertains to an <see cref="EmbeddedObject"/> instance; <c>false</c> otherwise.</value>
        public bool IsEmbedded { get; }

        private ObjectSchema(string name, bool isEmbedded, IDictionary<string, Property> properties)
        {
            Name = name;
            IsEmbedded = isEmbedded;

            _properties = new ReadOnlyDictionary<string, Property>(properties);

            foreach (var kvp in _properties.Where(kvp => kvp.Value.IsPrimaryKey))
            {
                if (PrimaryKeyProperty != null)
                {
                    var pkProperties = _properties.Where(p => p.Value.IsPrimaryKey).Select(p => p.Value.Name);
                    throw new ArgumentException($"This schema already contains more than one property that is designated as primary key: {string.Join(",", pkProperties)}", nameof(properties));
                }

                PrimaryKeyProperty = kvp.Value;
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
        public bool TryFindProperty(string name, out Property property)
        {
            Argument.NotNullOrEmpty(name, nameof(name));

            return _properties.TryGetValue(name, out property);
        }

        /// <inheritdoc/>
        public IEnumerator<Property> GetEnumerator() => _properties.Values.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal static ObjectSchema FromType(TypeInfo type)
        {
            Argument.NotNull(type, nameof(type));
            if (_cache.TryGetValue(type, out var result))
            {
                return result;
            }

            Argument.Ensure(type.IsRealmObject() || type.IsEmbeddedObject(), $"The class {type.FullName} must descend directly from RealmObject or EmbeddedObject", nameof(type));

            var builder = new Builder(type.GetMappedOrOriginalName(), type.IsEmbeddedObject());
            foreach (var property in type.DeclaredProperties.Where(p => !p.IsStatic() && p.HasCustomAttribute<WovenPropertyAttribute>()))
            {
                builder.Add(Property.FromPropertyInfo(property));
            }

            if (builder.Count == 0)
            {
                throw new InvalidOperationException(
                    $"No properties in {type.Name}, has linker stripped it? See https://docs.mongodb.com/realm/sdk/dotnet/troubleshooting/#resolve-a--no-properties-in-class--exception");
            }

            result = builder.Build();
            result.Type = type;
            _cache[type] = result;
            return result;
        }

        /// <summary>
        /// A mutable builder that allows you to construct an <see cref="ObjectSchema"/> instance.
        /// </summary>
        public class Builder : SchemaBuilderBase<Property>
        {
            /// <summary>
            /// Gets the name of the class described by the builder.
            /// </summary>
            /// <value>The name of the class.</value>
            public string Name { get; }

            /// <summary>
            /// Gets a value indicating whether this <see cref="Builder"/> describes an embedded object.
            /// </summary>
            /// <value><c>true</c> if the schema pertains to an <see cref="EmbeddedObject"/> instance; <c>false</c> otherwise.</value>
            public bool IsEmbedded { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Builder"/> class with the provided name.
            /// </summary>
            /// <param name="name">The name of the <see cref="ObjectSchema"/> this builder describes.</param>
            /// <param name="isEmbedded">A flag indicating whether the object is embedded or not.</param>
            /// <seealso cref="EmbeddedObject"/>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
            /// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is the empty string.</exception>
            public Builder(string name, bool isEmbedded = false)
            {
                Argument.NotNullOrEmpty(name, nameof(name));

                Name = name;
                IsEmbedded = isEmbedded;
            }

            /// <summary>
            /// Constructs an <see cref="ObjectSchema"/> from the properties added to this <see cref="Builder"/>.
            /// </summary>
            /// <returns>An immutable <see cref="ObjectSchema"/> instance that contains the properties added to the <see cref="Builder"/>.</returns>
            public ObjectSchema Build() => new ObjectSchema(Name, IsEmbedded, _values);

            /// <summary>
            /// Adds a new <see cref="Property"/> to this <see cref="Builder"/>.
            /// </summary>
            /// <param name="item">The <see cref="Property"/> to add.</param>
            /// <returns>The original <see cref="Builder"/> instance to enable chaining multiple <see cref="Add(Property)"/> calls.</returns>
            public new Builder Add(Property item)
            {
                base.Add(item);

                return this;
            }

            protected override string GetKey(Property item) => item.Name;
        }
    }
}
