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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Realms.Schema;

namespace Realms
{
    public class RealmSchema : IReadOnlyCollection<ObjectSchema>
    {
        private readonly ReadOnlyDictionary<string, ObjectSchema> _objects;

        internal readonly SchemaHandle Handle;

        public int Count => _objects.Count;

        private static readonly Lazy<RealmSchema> _default = new Lazy<RealmSchema>(BuildDefaultSchema);
        internal static RealmSchema Default => _default.Value;

        private RealmSchema(SchemaHandle handle, IEnumerable<ObjectSchema> objects)
        {
            _objects = new ReadOnlyDictionary<string, ObjectSchema>(objects.ToDictionary(o => o.Name));
            Handle = handle;
        }

        public ObjectSchema Find(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Object schema name must be a non-empty string", nameof(name));
            Contract.EndContractBlock();

            ObjectSchema obj;
            _objects.TryGetValue(name, out obj);
            return obj;
        }

        public IEnumerator<ObjectSchema> GetEnumerator()
        {
            return _objects.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal RealmSchema CloneForAdoption(SharedRealmHandle parent = null)
        {
            var objectsArray = this.ToArray();
            var handlesArray = objectsArray.Select(o => o.Handle).ToArray();
            System.Diagnostics.Debug.Assert(!handlesArray.Contains(IntPtr.Zero));

            var schemaHandle = new SchemaHandle(parent);
            schemaHandle.InitializeCloneFrom(Handle, handlesArray);

            var clones = objectsArray.Select((o, i) => o.Clone(handlesArray[i]));
            return new RealmSchema(schemaHandle, clones);
        }

        internal RealmSchema DynamicClone(SharedRealmHandle parent = null)
        {
            var clone = CloneForAdoption(parent);
            foreach (var type in clone)
            {
                type.Type = null;
            }

            return clone;
        }

        internal static RealmSchema CreateSchemaForClasses(IEnumerable<Type> classes, SchemaHandle schemaHandle = null)
        {
            var builder = new Builder();
            foreach (var @class in classes)
            {
                builder.Add(ObjectSchema.FromType(@class)); 
            }

            return builder.Build(schemaHandle ?? new SchemaHandle());
        }

        internal static RealmSchema CreateFromObjectStoreSchema(Native.Schema nativeSchema, SchemaHandle schemaHandle = null)
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
                        IsObjectId = nativeProperty.is_primary,
                        IsNullable = nativeProperty.is_nullable,
                        IsIndexed = nativeProperty.is_indexed
                    });
                }

                objects[i] = builder.Build();
            }

            schemaHandle = schemaHandle ?? new SchemaHandle();
            RuntimeHelpers.PrepareConstrainedRegions();
            try { }
            finally
            {
                schemaHandle.SetHandle(nativeSchema.handle);
            }

            return new RealmSchema(schemaHandle, objects);
        }

        private static RealmSchema BuildDefaultSchema()
        {
            var realmObjectClasses = AppDomain.CurrentDomain.GetAssemblies()
                                              #if !__IOS__
                                              // we need to exclude dynamic assemblies. see https://bugzilla.xamarin.com/show_bug.cgi?id=39679
                                              .Where(a => !(a is System.Reflection.Emit.AssemblyBuilder))
                                              #endif
                                              // exclude the Realm assembly
                                              .Where(a => a != typeof(Realm).Assembly)
                                              .SelectMany(a => a.GetTypes())
                                              .Where(t => t.IsSubclassOf(typeof(RealmObject)));

            return CreateSchemaForClasses(realmObjectClasses);
        }

        public class Builder : List<ObjectSchema>
        {
            public RealmSchema Build()
            {
                return Build(new SchemaHandle());
            }

            internal RealmSchema Build(SchemaHandle schemaHandle)
            {
                if (Count == 0) 
                {
                    throw new InvalidOperationException(
                        "No RealmObjects. Has linker stripped them? See https://realm.io/docs/xamarin/latest/#linker-stripped-schema");
                }
                Contract.EndContractBlock();

                var objects = new List<Native.SchemaObject>();
                var properties = new List<Native.SchemaProperty>();

                foreach (var @object in this)
                {
                    var start = properties.Count;

                    properties.AddRange(@object.Select(ForMarshalling));

                    objects.Add(new Native.SchemaObject
                    {
                        name = @object.Name,
                        properties_start = start,
                        properties_end = properties.Count
                    }); 
                }

                var handles = new IntPtr[Count];
                schemaHandle.Initialize(objects.ToArray(), objects.Count, properties.ToArray(), handles);

                return new RealmSchema(schemaHandle, this.Select((o, i) => o.Clone(handles[i])));
            }

            private static Native.SchemaProperty ForMarshalling(Schema.Property property)
            {
                return new Native.SchemaProperty
                {
                    name = property.Name,
                    type = property.Type,
                    object_type = property.ObjectType,
                    is_nullable = property.IsNullable,
                    is_indexed = property.IsIndexed,
                    is_primary = property.IsObjectId
                };
            }
        }
    }
}

