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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Realms.Helpers;

namespace Realms.Schema
{
    /// <summary>
    /// Describes the complete set of classes which may be stored in a Realm, either from assembly declarations or,
    /// dynamically, by evaluating a Realm from disk.
    /// </summary>
    /// <remarks>
    /// By default this will be all the <see cref="RealmObject"/>s and <see cref="EmbeddedObject"/>s in all your assemblies
    /// unless you restrict with <see cref="RealmConfigurationBase.ObjectClasses"/>. Just because a given class <em>may</em>
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
        /// Finds the definition of a class in this schema.
        /// </summary>
        /// <param name="name">A valid class name which may be in this schema.</param>
        /// <exception cref="ArgumentException">Thrown if a name is not supplied.</exception>
        /// <returns>An <see cref="ObjectSchema"/> or <c>null</c> to indicate not found.</returns>
        [Obsolete("This method is obsolete. Use TryFindObjectSchema instead.")]
        public ObjectSchema Find(string name)
        {
            Argument.NotNullOrEmpty(name, nameof(name));

            _objects.TryGetValue(name, out var obj);
            return obj;
        }

        public bool TryFindObjectSchema(string name, out ObjectSchema schema) => _objects.TryGetValue(name, out schema);

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "We don't need to document GetEnumerator.")]
        public IEnumerator<ObjectSchema> GetEnumerator()
        {
            return _objects.Values.GetEnumerator();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "We don't need to document GetEnumerator.")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal static RealmSchema CreateFromObjectStoreSchema(Native.Schema nativeSchema)
        {
            var builder = new Builder();
            for (var i = 0; i < nativeSchema.objects_len; i++)
            {
                var objectSchema = Marshal.PtrToStructure<Native.SchemaObject>(IntPtr.Add(nativeSchema.objects, i * Native.SchemaObject.Size));

                var osBuilder = new ObjectSchema.Builder(objectSchema.name, objectSchema.is_embedded);

                for (var n = objectSchema.properties_start; n < objectSchema.properties_end; n++)
                {
                    var nativeProperty = Marshal.PtrToStructure<Native.SchemaProperty>(IntPtr.Add(nativeSchema.properties, n * Native.SchemaProperty.Size));
                    osBuilder.Add(new Property(nativeProperty));
                }

                builder.Add(osBuilder);
            }

            return builder.Build();
        }

        public static implicit operator RealmSchema(ObjectSchema[] objects) => objects == null ? null : new RealmSchema(objects.ToDictionary(s => s.Name));

        public static implicit operator RealmSchema(List<ObjectSchema> objects) => objects == null ? null : new RealmSchema(objects.ToDictionary(s => s.Name));

        public static implicit operator RealmSchema(Type[] objects) => objects == null ? null : new Builder(objects).Build();

        public static implicit operator RealmSchema(List<Type> objects) => objects == null ? null : new Builder(objects).Build();

        public static implicit operator RealmSchema(HashSet<Type> objects) => objects == null ? null : new Builder(objects).Build();

        public static implicit operator RealmSchema(Builder builder) => builder?.Build();

        public class Builder : SchemaBuilderBase<ObjectSchema>
        {
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

            public RealmSchema Build()
            {
                return new RealmSchema(_values);
            }

            public Builder Add(ObjectSchema schema)
            {
                Argument.NotNull(schema, nameof(schema));

                base.Add(schema);
                return this;
            }

            public Builder Add(ObjectSchema.Builder schemaBuilder)
            {
                Argument.NotNull(schemaBuilder, nameof(schemaBuilder));

                base.Add(schemaBuilder.Build());
                return this;
            }

            public Builder Add(Type type)
            {
                Argument.NotNull(type, nameof(type));

                var objectSchema = ObjectSchema.FromType(type.GetTypeInfo());

                if (_values.TryGetValue(objectSchema.Name, out var existingOS) && existingOS.Type != null)
                {
                    var duplicateType = existingOS.Type;
                    if (objectSchema.Type.FullName != duplicateType.FullName)
                    {
                        var errorMessage = "The names (without namespace) of objects persisted in Realm must be unique." +
                            $"The duplicate types are {type.FullName} and {duplicateType.FullName}. Either rename one" +
                            " of them or explicitly specify ObjectClasses on your RealmConfiguration.";
                        throw new NotSupportedException(errorMessage);
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
