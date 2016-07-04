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

namespace Realms
{
    using Object = Schema.Object;

    internal class RealmSchema : IReadOnlyCollection<Object>
    {
        private readonly ReadOnlyDictionary<string, Object> _objects;

        internal readonly SchemaHandle Handle;

        public int Count => _objects.Count;

        private static readonly Lazy<RealmSchema> _default = new Lazy<RealmSchema>(BuildDefaultSchema);
        internal static RealmSchema Default => _default.Value;

        private RealmSchema(SchemaHandle handle, IEnumerable<Object> objects)
        {
            _objects = new ReadOnlyDictionary<string, Object>(objects.ToDictionary(o => o.Name));
            Handle = handle;
        }

        public Object Find(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Object schema name must be a non-empty string", nameof(name));
            Contract.EndContractBlock();

            Object obj;
            _objects.TryGetValue(name, out obj);
            return obj;
        }

        public IEnumerator<Object> GetEnumerator()
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
            var schemaHandle = new SchemaHandle(parent);

            unsafe
            {
                fixed (IntPtr* handlesPtr = handlesArray)
                {
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try { }
                    finally
                    {
                        schemaHandle.SetHandle(NativeSchema.clone(Handle, handlesPtr));
                    }
                }
            }

            var clones = objectsArray.Select((o, i) => o.Clone(handlesArray[i]));
            return new RealmSchema(schemaHandle, clones);
        }

        internal RealmSchema DynamicClone(SharedRealmHandle parent = null)
        {
            var clone = CloneForAdoption(parent);
            foreach (var type in clone)
            {
                type.Type = null;
                foreach (var property in type)
                {
                    property.PropertyInfo = null;
                }
            }

            return clone;
        }

        internal static RealmSchema CreateSchemaForClasses(IEnumerable<Type> classes, SchemaHandle schemaHandle = null)
        {
            var builder = new Builder();
            foreach (var @class in classes)
            {
                builder.Add(Object.FromType(@class)); 
            }

            return builder.Build(schemaHandle ?? new SchemaHandle());
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

        public class Builder : List<Object>
        {
            public RealmSchema Build()
            {
                return Build(new SchemaHandle());
            }

            internal RealmSchema Build(SchemaHandle schemaHandle)
            {
                var objects = new List<NativeSchema.Object>();
                var properties = new List<NativeSchema.Property>();

                foreach (var @object in this)
                {
                    var start = properties.Count;

                    properties.AddRange(@object.Select(ForMarshalling));

                    objects.Add(new NativeSchema.Object
                    {
                        name = @object.Name,
                        properties_start = start,
                        properties_end = properties.Count
                    }); 
                }

                var handles = new IntPtr[Count];

                unsafe
                {
                    fixed (IntPtr* handlesPtr = handles)
                    {
                        var ptr = NativeSchema.create(objects.ToArray(), objects.Count, properties.ToArray(), handlesPtr);

                        RuntimeHelpers.PrepareConstrainedRegions();
                        try { }
                        finally
                        {
                            schemaHandle.SetHandle(ptr);
                        }
                    }
                }

                return new RealmSchema(schemaHandle, this.Select((o, i) => o.Clone(handles[i])));
            }

            private static NativeSchema.Property ForMarshalling(Schema.Property property)
            {
                return new NativeSchema.Property
                {
                    name = property.Name,
                    type = property.Type,
                    objectType = property.ObjectType,
                    is_nullable = property.IsNullable,
                    is_indexed = property.IsIndexed,
                    is_primary = property.IsObjectId
                };
            }
        }
    }
}

