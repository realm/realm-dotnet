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
using System.Reflection;
using System.Runtime.InteropServices;

namespace Realms.Schema
{
    /// <summary>
    /// Describes the complete set of classes which may be stored in a Realm, either from assembly declarations or,
    /// dynamically, by evaluating a Realm from disk.
    /// </summary>
    /// <remarks>
    /// By default this will be all the <see cref="RealmObject"/>s in all your assemblies unless you restrict with
    /// <see cref="RealmConfigurationBase.ObjectClasses"/>. Just because a given class <em>may</em> be stored in a
    /// Realm doesn't imply much overhead. There will be a small amount of metadata but objects only start to
    /// take up space once written.
    /// </remarks>
    public class RealmSchema : IReadOnlyCollection<ObjectSchema>
    {
        private static readonly HashSet<Type> _defaultTypes = new HashSet<Type>();
        private static readonly Lazy<RealmSchema> _default = new Lazy<RealmSchema>(() => CreateSchemaForClasses(_defaultTypes));
        private readonly ReadOnlyDictionary<string, ObjectSchema> _objects;

        /// <summary>
        /// Adds a collection of types to the default schema.
        /// </summary>
        /// <param name="types">Types to be added to the default schema.</param>
        /// <exception cref="NotSupportedException">Thrown if the schema has already materialized.</exception>
        public static void AddDefaultTypes(IEnumerable<Type> types)
        {
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

        internal static RealmSchema Empty => new RealmSchema(Enumerable.Empty<ObjectSchema>());

        private RealmSchema(IEnumerable<ObjectSchema> objects)
        {
            _objects = new ReadOnlyDictionary<string, ObjectSchema>(objects.ToDictionary(o => o.Name));
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

        /// <inheritdoc/>
        public IEnumerator<ObjectSchema> GetEnumerator()
        {
            return _objects.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal static RealmSchema CreateSchemaForClasses(IEnumerable<Type> classes, RealmSchema existing = null)
        {
            var builder = new Builder();
            var classNames = new HashSet<string>();
            if (existing != null)
            {
                builder.AddRange(existing);
                foreach (var item in existing)
                {
                    classNames.Add(item.Name);
                }
            }

            foreach (var @class in classes)
            {
                var typeInfo = @class.GetTypeInfo();
                var objectSchema = ObjectSchema.FromType(@class.GetTypeInfo());

                if (classNames.Add(objectSchema.Name))
                {
                    builder.Add(objectSchema);
                }
                else
                {
                    var duplicateType = builder.FirstOrDefault(s => s.Name == objectSchema.Name).Type;
                    if (typeInfo.FullName != duplicateType.FullName)
                    {
                        var errorMessage = "The names (without namespace) of objects persisted in Realm must be unique." +
                            $"The duplicate types are {@class.FullName} and {duplicateType.FullName}. Either rename one" +
                            " of them or explicitly specify ObjectClasses on your RealmConfiguration.";
                        throw new NotSupportedException(errorMessage);
                    }
                }
            }

            return builder.Build();
        }

        internal static RealmSchema CreateFromObjectStoreSchema(Native.Schema nativeSchema)
        {
            var objects = new ObjectSchema[nativeSchema.objects_len];
            for (var i = 0; i < nativeSchema.objects_len; i++)
            {
                var objectSchema = Marshal.PtrToStructure<Native.SchemaObject>(IntPtr.Add(nativeSchema.objects, i * Native.SchemaObject.Size));
                var builder = new ObjectSchema.Builder(objectSchema.name);

                for (var n = objectSchema.properties_start; n < objectSchema.properties_end; n++)
                {
                    var nativeProperty = Marshal.PtrToStructure<Native.SchemaProperty>(IntPtr.Add(nativeSchema.properties, n * Native.SchemaProperty.Size));
                    builder.Add(new Property
                    {
                        Name = nativeProperty.name,
                        Type = nativeProperty.type,
                        ObjectType = nativeProperty.object_type,
                        LinkOriginPropertyName = nativeProperty.link_origin_property_name,
                        IsPrimaryKey = nativeProperty.is_primary,
                        IsIndexed = nativeProperty.is_indexed
                    });
                }

                objects[i] = builder.Build();
            }

            return new RealmSchema(objects);
        }

        private class Builder : List<ObjectSchema>
        {
            public RealmSchema Build()
            {
                if (Count == 0)
                {
                    throw new InvalidOperationException(
                        "No RealmObjects. Has linker stripped them? See https://realm.io/docs/xamarin/latest/#linker-stripped-schema");
                }

                return new RealmSchema(this);
            }
        }
    }
}