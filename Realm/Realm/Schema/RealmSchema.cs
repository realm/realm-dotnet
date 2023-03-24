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
using System.Linq;
using System.Runtime.InteropServices;
using Realms.Helpers;

namespace Realms.Schema
{
    /// <summary>
    /// Describes the complete set of classes which may be stored in a Realm, either from assembly declarations or,
    /// dynamically, by evaluating a Realm from disk. To construct a new <see cref="RealmSchema"/> instance, use the
    /// <see cref="Builder">RealmSchema.Builder</see> API.
    /// </summary>
    /// <remarks>
    /// By default this will be all the <see cref="RealmObject"/>s, <see cref="EmbeddedObject"/>s and <see cref="AsymmetricObject"/>s
    /// in all your assemblies. Unless you restrict with <see cref="RealmConfigurationBase.Schema"/>. Just because a given class <em>may</em>
    /// be stored in a Realm doesn't imply much overhead. There will be a small amount of metadata but objects only start to
    /// take up space once written.
    /// </remarks>
    public class RealmSchema : IReadOnlyCollection<ObjectSchema>
    {
        private static readonly HashSet<Type> _defaultTypes = new HashSet<Type>();
        private readonly ReadOnlyDictionary<string, ObjectSchema> _objects;
        private static readonly Lazy<RealmSchema> _default = new Lazy<RealmSchema>(() =>
        {
            if (_defaultTypes.Count == 0)
            {
                // this was introduced because Unity's IL2CPP won't behave as expected with module initializers
                // so we manually do what .Net-like frameworks usually do with module initializers
                try
                {
                    var moduleInitializers = AppDomain.CurrentDomain.GetAssemblies()
                        .Select(assembly => assembly.GetType("RealmModuleInitializer")?.GetMethod("Initialize"))
                        .Where(method => method != null);

                    foreach (var moduleInitializer in moduleInitializers)
                    {
                        moduleInitializer.Invoke(null, null);
                    }
                }
                catch
                {
                }
            }

            return _defaultTypes;
        });

        /// <summary>
        /// Adds a collection of types to the default schema.
        /// </summary>
        /// <param name="types">Types to be added to the default schema.</param>
        /// <exception cref="NotSupportedException">Thrown if the schema has already materialized.</exception>
        public static void AddDefaultTypes(IEnumerable<Type> types)
        {
            Argument.NotNull(types, nameof(types));

            foreach (var type in types)
            {
                if (!type.IsRealmObject() && !type.IsEmbeddedObject() && !type.IsAsymmetricObject())
                {
                    throw new ArgumentException($"The type {type.FullName} must inherit directly from RealmObject, AsymmetricObject or EmbeddedObject to be used in the Realm schema.");
                }

                if (_defaultTypes.Add(type) &&
                    _default.IsValueCreated)
                {
                    throw new NotSupportedException("AddDefaultTypes should be called before creating a Realm instance with the default schema. If you see this error, please report it to help@realm.io.");
                }
            }
        }

        /// <summary>
        /// Gets the number of known classes in the schema.
        /// </summary>
        /// <value>The number of known classes.</value>
        public int Count => _objects.Count;

        internal static RealmSchema Default => _default.Value;

        internal static RealmSchema Empty { get; } = new RealmSchema(new Dictionary<string, ObjectSchema>());

        /// <summary>
        /// Initializes a new instance of the <see cref="RealmSchema"/> class with a collection of <see cref="ObjectSchema"/>s.
        /// </summary>
        /// <param name="objectSchemas">
        /// A collection of <see cref="ObjectSchema"/> instances that will describe the types of objects contained in the <see cref="Realm"/>
        /// instance.
        /// </param>
        private RealmSchema(IDictionary<string, ObjectSchema> objectSchemas)
        {
            Argument.NotNull(objectSchemas, nameof(objectSchemas));

            _objects = new ReadOnlyDictionary<string, ObjectSchema>(objectSchemas);
        }

        /// <summary>
        /// Attempts to find the definition of a class in this schema.
        /// </summary>
        /// <param name="name">A valid class name which may be in this schema.</param>
        /// <param name="schema">The schema corresponding to the provided <paramref name="name"/> or <c>null</c> if the schema is not found.</param>
        /// <exception cref="ArgumentException">Thrown if a name is not supplied.</exception>
        /// <returns>
        /// <c>true</c> if this <see cref="RealmSchema"/> contains a class definition with the supplied <paramref name="name"/>; <c>false</c> otherwise.
        /// </returns>
        public bool TryFindObjectSchema(string name, out ObjectSchema schema)
        {
            Argument.NotNullOrEmpty(name, nameof(name));

            return _objects.TryGetValue(name, out schema);
        }

        /// <summary>
        /// Create a mutable <see cref="Builder"/> containing the object schemas in this Realm schema.
        /// </summary>
        /// <returns>
        /// A <see cref="Builder"/> instance that can be used to mutate the schema and eventually
        /// produce a new one by calling <see cref="Builder.Build"/>.
        /// </returns>
        public Builder GetBuilder()
        {
            var builder = new Builder();
            foreach (var schema in this)
            {
                builder.Add(schema);
            }

            return builder;
        }

        /// <inheritdoc/>
        public IEnumerator<ObjectSchema> GetEnumerator() => _objects.Values.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal static RealmSchema CreateFromObjectStoreSchema(Native.Schema nativeSchema)
        {
            var builder = new Builder();
            for (var i = 0; i < nativeSchema.objects_len; i++)
            {
                var objectSchema = Marshal.PtrToStructure<Native.SchemaObject>(IntPtr.Add(nativeSchema.objects, i * Native.SchemaObject.Size));

                var osBuilder = new ObjectSchema.Builder(objectSchema.name, objectSchema.table_type);

                for (var n = objectSchema.properties_start; n < objectSchema.properties_end; n++)
                {
                    var nativeProperty = Marshal.PtrToStructure<Native.SchemaProperty>(IntPtr.Add(nativeSchema.properties, n * Native.SchemaProperty.Size));
                    osBuilder.Add(new Property(nativeProperty));
                }

                builder.Add(osBuilder);
            }

            return builder.Build();
        }

        /// <summary>
        /// Constructs a <see cref="RealmSchema"/> from an array of <see cref="ObjectSchema"/> instances.
        /// </summary>
        /// <param name="objects">The object schemas that will be contained in the newly constructed <see cref="RealmSchema"/>.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the array contains multiple <see cref="ObjectSchema"/> instances with the same <see cref="ObjectSchema.Name"/>.
        /// </exception>
        /// <returns>
        /// <c>null</c> if <paramref name="objects"/> is <c>null</c>; a <see cref="RealmSchema"/> containing the supplied <see cref="ObjectSchema"/>s otherwise.
        /// </returns>
        public static implicit operator RealmSchema(ObjectSchema[] objects) => objects == null ? null : new Builder(objects).Build();

        /// <summary>
        /// Constructs a <see cref="RealmSchema"/> from a list of <see cref="ObjectSchema"/> instances.
        /// </summary>
        /// <param name="objects">The object schemas that will be contained in the newly constructed <see cref="RealmSchema"/>.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the list contains multiple <see cref="ObjectSchema"/> instances with the same <see cref="ObjectSchema.Name"/>.
        /// </exception>
        /// <returns>
        /// <c>null</c> if <paramref name="objects"/> is <c>null</c>; a <see cref="RealmSchema"/> containing the supplied <see cref="ObjectSchema"/>s otherwise.
        /// </returns>
        public static implicit operator RealmSchema(List<ObjectSchema> objects) => objects == null ? null : new Builder(objects).Build();

        /// <summary>
        /// Constructs a <see cref="RealmSchema"/> from an array of <see cref="Type"/> instances.
        /// </summary>
        /// <param name="objects">The <see cref="Type"/>s that will be converted to <see cref="ObjectSchema"/> and added to the resulting <see cref="RealmSchema"/>.</param>
        /// <returns>
        /// <c>null</c> if <paramref name="objects"/> is <c>null</c>; a <see cref="RealmSchema"/> containing the supplied <see cref="ObjectSchema"/>s otherwise.
        /// </returns>
        /// <seealso cref="Builder.Add(Type)"/>
        public static implicit operator RealmSchema(Type[] objects) => objects == null ? null : new Builder(objects).Build();

        /// <summary>
        /// Constructs a <see cref="RealmSchema"/> from a List of <see cref="Type"/> instances.
        /// </summary>
        /// <param name="objects">The <see cref="Type"/>s that will be converted to <see cref="ObjectSchema"/> and added to the resulting <see cref="RealmSchema"/>.</param>
        /// <returns>
        /// <c>null</c> if <paramref name="objects"/> is <c>null</c>; a <see cref="RealmSchema"/> containing the supplied <see cref="ObjectSchema"/>s otherwise.
        /// </returns>
        /// <seealso cref="Builder.Add(Type)"/>
        public static implicit operator RealmSchema(List<Type> objects) => objects == null ? null : new Builder(objects).Build();

        /// <summary>
        /// Constructs a <see cref="RealmSchema"/> from a HashSet of <see cref="Type"/> instances.
        /// </summary>
        /// <param name="objects">The <see cref="Type"/>s that will be converted to <see cref="ObjectSchema"/> and added to the resulting <see cref="RealmSchema"/>.</param>
        /// <returns>
        /// <c>null</c> if <paramref name="objects"/> is <c>null</c>; a <see cref="RealmSchema"/> containing the supplied <see cref="ObjectSchema"/>s otherwise.
        /// </returns>
        /// <seealso cref="Builder.Add(Type)"/>
        public static implicit operator RealmSchema(HashSet<Type> objects) => objects == null ? null : new Builder(objects).Build();

        /// <summary>
        /// A convenience operator to construct a <see cref="RealmSchema"/> from a <see cref="Builder"/> by calling the
        /// <see cref="Builder.Build"/> method.
        /// </summary>
        /// <param name="builder">The builder that describes the newly created schema.</param>
        /// <returns><c>null</c> if <paramref name="builder"/> is <c>null</c>; the result of <see cref="Builder.Build"/> otherwise.</returns>
        public static implicit operator RealmSchema(Builder builder) => builder?.Build();

        /// <summary>
        /// A mutable builder that allows you to construct a <see cref="RealmSchema"/> instance.
        /// </summary>
        public class Builder : SchemaBuilderBase<ObjectSchema>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Builder"/> class.
            /// </summary>
            public Builder()
            {
            }

            internal Builder(IEnumerable<Type> types)
            {
                Argument.NotNull(types, nameof(types));

                foreach (var type in types)
                {
                    Add(type);
                }
            }

            internal Builder(IEnumerable<ObjectSchema> schemas)
            {
                Argument.NotNull(schemas, nameof(schemas));

                foreach (var schema in schemas)
                {
                    Add(schema);
                }
            }

            /// <summary>
            /// Constructs a <see cref="RealmSchema"/> from the properties added to this <see cref="Builder"/>.
            /// </summary>
            /// <returns>An immutable <see cref="RealmSchema"/> instance that contains the properties added to the <see cref="Builder"/>.</returns>
            public RealmSchema Build() => new RealmSchema(_values);

            /// <summary>
            /// Adds a new <see cref="ObjectSchema"/> to this <see cref="Builder"/>.
            /// </summary>
            /// <param name="schema">The <see cref="ObjectSchema"/> to add.</param>
            /// <returns>The original <see cref="Builder"/> instance to enable chaining multiple <see cref="Add(ObjectSchema)"/> calls.</returns>
            public new Builder Add(ObjectSchema schema)
            {
                Argument.NotNull(schema, nameof(schema));

                base.Add(schema);
                return this;
            }

            /// <summary>
            /// Adds a new <see cref="ObjectSchema.Builder"/> to this <see cref="Builder"/>.
            /// </summary>
            /// <param name="schemaBuilder">The <see cref="ObjectSchema.Builder"/> to add.</param>
            /// <returns>The original <see cref="Builder"/> instance to enable chaining multiple <see cref="Add(ObjectSchema.Builder)"/> calls.</returns>
            /// <remarks>
            /// This is a convenience method that will call <see cref="ObjectSchema.Builder.Build"/> internally. It is intended to simplify declarative
            /// schema construction via collection initializers:
            /// <code>
            /// var schema = new RealmSchema.Builder
            /// {
            ///     new ObjectSchema.Builder("MyClass", isEmbedded: false)
            ///     {
            ///         Property.Primitive("MyProperty", RealmValueType.Int)
            ///     }
            /// }
            /// </code>
            /// </remarks>
            public Builder Add(ObjectSchema.Builder schemaBuilder)
            {
                Argument.NotNull(schemaBuilder, nameof(schemaBuilder));

                base.Add(schemaBuilder.Build());
                return this;
            }

            /// <summary>
            /// Adds a new <see cref="Type"/> to this <see cref="Builder"/>.
            /// </summary>
            /// <param name="type">The <see cref="Type"/> to add. It will be converted to <see cref="ObjectSchema"/> and added to the builder.</param>
            /// <returns>The original <see cref="Builder"/> instance to enable chaining multiple <see cref="Add(Type)"/> calls.</returns>
            public Builder Add(Type type)
            {
                Argument.NotNull(type, nameof(type));

                var objectSchema = ObjectSchema.FromType(type);

                if (_values.TryGetValue(objectSchema.Name, out var existingOS) && existingOS.Type != null)
                {
                    var duplicateType = existingOS.Type;
                    if (objectSchema.Type.FullName != duplicateType.FullName)
                    {
                        var errorMessage = "The names (without namespace) of objects persisted in Realm must be unique." +
                            $"The duplicate types are {type.FullName} and {duplicateType.FullName}. Either rename one" +
                            " of them or explicitly specify ObjectClasses on your RealmConfiguration.";
                        throw new ArgumentException(errorMessage);
                    }

                    return this;
                }

                base.Add(objectSchema);

                return this;
            }

            protected override string GetKey(ObjectSchema item) => item?.Name;
        }
    }
}
