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
        private static readonly Lazy<RealmSchema> _default = new Lazy<RealmSchema>(GetSchema);
        private readonly ReadOnlyDictionary<string, ObjectSchema> _objects;

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

        internal static RealmSchema Empty { get; } = new RealmSchema(Enumerable.Empty<ObjectSchema>());

        /// <summary>
        /// Initializes a new instance of the <see cref="RealmSchema"/> class with a collection of <see cref="ObjectSchema"/>s.
        /// </summary>
        /// <param name="objectSchemas">
        /// A collection of <see cref="ObjectSchema"/> instances that will describe the types of objects contained in the <see cref="Realm"/>
        /// instance.
        /// </param>
        public RealmSchema(IEnumerable<ObjectSchema> objectSchemas)
        {
            Argument.NotNull(objectSchemas, nameof(objectSchemas));

            _objects = new ReadOnlyDictionary<string, ObjectSchema>(objectSchemas.ToDictionary(o => o.Name));
        }

        public RealmSchema(IEnumerable<Type> types)
        {
            Argument.NotNull(types, nameof(types));

            var objectSchemas = new Dictionary<string, ObjectSchema>();

            foreach (var type in types)
            {
                var typeInfo = type.GetTypeInfo();
                var objectSchema = ObjectSchema.FromType(type.GetTypeInfo());

                if (objectSchemas.TryGetValue(objectSchema.Name, out var existingOS))
                {
                    var duplicateType = existingOS.Type;
                    if (typeInfo.FullName != duplicateType.FullName)
                    {
                        var errorMessage = "The names (without namespace) of objects persisted in Realm must be unique." +
                            $"The duplicate types are {type.FullName} and {duplicateType.FullName}. Either rename one" +
                            " of them or explicitly specify ObjectClasses on your RealmConfiguration.";
                        throw new NotSupportedException(errorMessage);
                    }
                }
                else
                {
                    objectSchemas.Add(objectSchema.Name, objectSchema);
                }
            }

            if (objectSchemas.Count == 0)
            {
                var message = InteropConfig.Platform switch
                {
                    InteropConfig.UnityPlatform => "Try weaving assemblies again ('Realm' -> 'Weave Assemblies' from the editor or simply make a code change) and make sure you don't have any Realm-related errors in the logs.",
                    InteropConfig.DotNetPlatform => "Has linker stripped them? See https://docs.mongodb.com/realm/sdk/dotnet/troubleshooting/",
                    _ => string.Empty
                };

                throw new InvalidOperationException($"No objects in the schema. {message}");
            }

            _objects = new ReadOnlyDictionary<string, ObjectSchema>(objectSchemas);
        }

        /// <summary>
        /// Finds the definition of a class in this schema.
        /// </summary>
        /// <param name="name">A valid class name which may be in this schema.</param>
        /// <exception cref="ArgumentException">Thrown if a name is not supplied.</exception>
        /// <returns>An <see cref="ObjectSchema"/> or <c>null</c> to indicate not found.</returns>
        public ObjectSchema Find(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Object schema name must be a non-empty string", nameof(name));
            }

            _objects.TryGetValue(name, out var obj);
            return obj;
        }

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

        internal static RealmSchema GetSchema()
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

            return new RealmSchema(_defaultTypes);
        }

        internal static RealmSchema CreateFromObjectStoreSchema(Native.Schema nativeSchema)
        {
            var objects = new ObjectSchema[nativeSchema.objects_len];
            for (var i = 0; i < nativeSchema.objects_len; i++)
            {
                var objectSchema = Marshal.PtrToStructure<Native.SchemaObject>(IntPtr.Add(nativeSchema.objects, i * Native.SchemaObject.Size));

                var properties = new List<Property>();

                for (var n = objectSchema.properties_start; n < objectSchema.properties_end; n++)
                {
                    var nativeProperty = Marshal.PtrToStructure<Native.SchemaProperty>(IntPtr.Add(nativeSchema.properties, n * Native.SchemaProperty.Size));
                    properties.Add(new Property
                    {
                        Name = nativeProperty.name,
                        Type = nativeProperty.type,
                        ObjectType = nativeProperty.object_type,
                        LinkOriginPropertyName = nativeProperty.link_origin_property_name,
                        IsPrimaryKey = nativeProperty.is_primary,
                        IsIndexed = nativeProperty.is_indexed
                    });
                }

                objects[i] = new ObjectSchema(objectSchema.name, objectSchema.is_embedded, properties);
            }

            return new RealmSchema(objects);
        }

        public static implicit operator RealmSchema(ObjectSchema[] objects) => new RealmSchema(objects);

        public static implicit operator RealmSchema(List<ObjectSchema> objects) => new RealmSchema(objects);

        public static implicit operator RealmSchema(Type[] objects) => new RealmSchema(objects);

        public static implicit operator RealmSchema(List<Type> objects) => new RealmSchema(objects);
    }
}
