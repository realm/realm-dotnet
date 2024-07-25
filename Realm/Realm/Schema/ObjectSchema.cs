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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Realms.Helpers;
using Realms.Native;

namespace Realms.Schema
{
    /// <summary>
    /// Public description of a class stored in a Realm, as a collection of managed Property objects. To construct
    /// a new instance, use the <see cref="Builder">ObjectSchema.Builder</see> API.
    /// </summary>
    [DebuggerDisplay("Name = {Name}, Properties = {Count}")]
    public class ObjectSchema : IReadOnlyCollection<Property>
    {
        /// <summary>
        /// Represents the object schema type of an <see cref="ObjectSchema"/>.
        /// </summary>
        public enum ObjectType : byte
        {
            /// <summary>
            /// The value represents a <see cref="RealmObject"/> schema type.
            /// </summary>
            RealmObject = 0,

            /// <summary>
            /// The value represents a <see cref="EmbeddedObject"/> schema type.
            /// </summary>
            EmbeddedObject = 1,

            /// <summary>
            /// The value represents a <see cref="AsymmetricObject"/> schema type.
            /// </summary>
            AsymmetricObject = 2,
        }

        private static readonly ConcurrentDictionary<Type, ObjectSchema> _cache = new();

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

        /// <summary>
        /// Gets a <see cref="ObjectType"/> indicating whether this <see cref="ObjectSchema"/> describes
        /// a top level object, an embedded object or an asymmetric object.
        /// </summary>
        /// <value>The type of ObjectSchema.</value>
        public ObjectType BaseType { get; }

        internal Property? PrimaryKeyProperty { get; }

        internal Type? Type { get; private set; }

        internal ReadOnlyDictionary<string, Property> Properties => _properties;

        /// <summary>
        /// Gets or sets the ObjectHandle. This should be set only if the realm is opened with
        /// the relaxed schema enabled.
        /// </summary>
        internal ObjectHandle? ObjectHandle { get; set; }

        internal ObjectSchema(string name, ObjectType schemaType, IDictionary<string, Property> properties)
        {
            Name = name;
            BaseType = schemaType;

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

        //TODO This seems to not have references, need to try to remove it and see if anything fails
        internal ObjectSchema(in SchemaObject native)
        {
            Name = native.name!;
            BaseType = native.table_type;
            _properties = new(native.properties.ToEnumerable().ToDictionary(p => (string)p.name!, p => new Property(p)));
            if (native.primary_key)
            {
                PrimaryKeyProperty = _properties[native.primary_key!];
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

            if (_properties.TryGetValue(name, out property))
            {
                return true;
            }
            else if (ObjectHandle?.HasProperty(name) is true)
            {
                property = Property.ExtraProperty(name);
                return true;
            }

            return false;
        }

        //TODO Docs + Improve
        public bool HasProperty(string name)
        {
            return TryFindProperty(name, out _);
        }

        /// <summary>
        /// Create a mutable <see cref="Builder"/> containing the properties in this schema.
        /// </summary>
        /// <returns>
        /// A <see cref="Builder"/> instance that can be used to mutate the schema and eventually
        /// produce a new one by calling <see cref="Builder.Build"/>.
        /// </returns>
        public Builder GetBuilder()
        {
            var builder = new Builder(Name, BaseType);
            foreach (var prop in this)
            {
                builder.Add(prop);
            }

            builder.Type = Type;

            return builder;
        }

        // TODO Check for correctness
        // Should we get the schema from core too?
        /// <inheritdoc/>
        public IEnumerator<Property> GetEnumerator()
        {
            var schemaEnumerable = _properties.Values.AsEnumerable();

            if (ObjectHandle is not null)
            {
                var extraEnumerable = ObjectHandle.GetExtraProperties().Select(Property.ExtraProperty);
                schemaEnumerable = schemaEnumerable.Concat(extraEnumerable);
            }

            return schemaEnumerable.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal static ObjectSchema FromType(Type type)
        {
            Argument.NotNull(type, nameof(type));

            return _cache.GetOrAdd(type, t => new Builder(t).Build());
        }

        internal SchemaObject ToNative(Arena arena) => new()
        {
            name = StringValue.AllocateFrom(Name, arena),
            table_type = BaseType,
            properties = MarshaledVector<SchemaProperty>.AllocateFrom(this.Select(p => p.ToNative(arena)).ToArray(), arena),
            primary_key = StringValue.AllocateFrom(PrimaryKeyProperty?.Name, arena)
        };

        /// <summary>
        /// A mutable builder that allows you to construct an <see cref="ObjectSchema"/> instance.
        /// </summary>
        public class Builder : SchemaBuilderBase<Property>
        {
            internal Type? Type;

            /// <summary>
            /// Gets or sets the name of the class described by the builder.
            /// </summary>
            /// <value>The name of the class.</value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets a value indicating the object's <see cref="ObjectType"/> this <see cref="Builder"/> describes.
            /// </summary>
            /// <value><see cref="ObjectType"/> of the schema of the object.</value>
            public ObjectType RealmSchemaType { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Builder"/> class with the provided name.
            /// </summary>
            /// <param name="name">The name of the <see cref="ObjectSchema"/> this builder describes.</param>
            /// <param name="schemaType">The <see cref="ObjectType"/> of the object this builder describes.</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
            /// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is the empty string.</exception>
            public Builder(string name, ObjectType schemaType = ObjectType.RealmObject)
            {
                Argument.NotNullOrEmpty(name, nameof(name));

                Name = name;
                RealmSchemaType = schemaType;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Builder"/> class populated with properties from the
            /// provided <paramref name="type"/>.
            /// </summary>
            /// <param name="type">
            /// The <see cref="System.Type"/> that will be used to populate the builder. It must be a <see cref="RealmObject"/>,
            /// an <see cref="EmbeddedObject"/>, or an <see cref="AsymmetricObject"/> inheritor.
            /// </param>
            /// <remarks>
            /// If you want to use strongly typed API, such as <see cref="Realm.Add{T}(T, bool)">Realm.Add&lt;T&gt;</see> or
            /// <see cref="Realm.All{T}">Realm.All&lt;T&gt;</see>, you must use this method to build your schema.
            /// <br/>
            /// Adding new properties is fully supported, but removing or changing properties defined on the class will result
            /// in runtime errors being thrown if those properties are accessed via the object property accessors.
            /// </remarks>
            /// <example>
            /// <code>
            /// class Person : RealmObject
            /// {
            ///     public string Name { get; set; }
            /// }
            ///
            /// var personSchema = new Builder(typeof(Person));
            ///
            /// // someTagsCollection is a collection of tags determined at runtime - e.g. obtained
            /// // from a REST API.
            /// foreach (var tag in someTagsCollection)
            /// {
            ///     personSchema.Add(Property.Primitive(tag, RealmValueType.Bool));
            /// }
            ///
            /// var config = new RealmConfiguration
            /// {
            ///     Schema = new[] { personSchema.Build() }
            /// }
            /// using var realm = Realm.GetInstance(config);
            ///
            /// // Query for all people with a particular tag
            /// var tag = "Tall";
            /// var matches = realm.All&lt;Person&gt;().Filter($"{tag} = TRUE");
            ///
            /// // Get/set the tag of a particular person
            /// var hasTag = person.DynamicApi.Get&lt;bool&gt;(tag);
            /// person.DynamicApi.Set(tag, true);
            /// </code>
            /// </example>
            public Builder(Type type)
            {
                Argument.NotNull(type, nameof(type));
                Argument.Ensure(type.IsRealmObject() || type.IsEmbeddedObject() || type.IsAsymmetricObject(),
                    $"The class {type.FullName} must descend directly from either RealmObject, EmbeddedObject, or AsymmetricObject", nameof(type));

                RealmSchemaType = type.GetRealmSchemaType();

                var schemaField = type.GetField("RealmSchema", BindingFlags.Public | BindingFlags.Static);
                if (schemaField != null)
                {
                    var objectSchema = (ObjectSchema)schemaField.GetValue(null)!;
                    Name = objectSchema.Name;

                    foreach (var prop in objectSchema)
                    {
                        Add(prop);
                    }
                }
                else
                {
                    Name = type.GetMappedOrOriginalName();
                    foreach (var property in type.GetTypeInfo().DeclaredProperties.Where(p => !p.IsStatic() && p.HasCustomAttribute<WovenPropertyAttribute>()))
                    {
                        Add(Property.FromPropertyInfo(property));
                    }
                }

                if (Count == 0)
                {
                    throw new InvalidOperationException(
                        $"No properties in {type.Name}, has linker stripped it? See https://www.mongodb.com/docs/atlas/device-sdks/sdk/dotnet/troubleshooting/#resolve-a--no-properties-in-class--exception");
                }

                Type = type;
            }

            /// <summary>
            /// Constructs an <see cref="ObjectSchema"/> from the properties added to this <see cref="Builder"/>.
            /// </summary>
            /// <returns>An immutable <see cref="ObjectSchema"/> instance that contains the properties added to the <see cref="Builder"/>.</returns>
            public ObjectSchema Build() => new(Name, RealmSchemaType, _values) { Type = Type };

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

            private protected override string GetKey(Property item) => item.Name;
        }
    }
}
