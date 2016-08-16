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
using Realms.Schema;

namespace Realms
{
    /// <summary>
    /// Describe the complete set of classes which may be stored in a Realm, either from assembly declarations or, dynamically, by evaluating a Realm from disk.
    /// </summary>
    /// <remarks>
    /// By default this will be all the RealmObjects in all your assemblies unless you restrict with RealmConfiguration.ObjectClasses. 
    /// Just because a given class <em>may</em> be stored in a Realm doesn't imply much overhead. There will be a small amount of metadata 
    /// but objects only start to take up space once written. 
    /// </remarks>
    public class RealmSchema : IReadOnlyCollection<ObjectSchema>
    {
        private readonly ReadOnlyDictionary<string, ObjectSchema> _objects;

        internal readonly SchemaHandle Handle;

        /// <summary>
        /// Number of known classes in the schema.
        /// </summary>
        /// <value>Count of known classes specified in this Schema.</value>
        public int Count => _objects.Count;

        private static readonly Lazy<RealmSchema> _default = new Lazy<RealmSchema>(BuildDefaultSchema);
        internal static RealmSchema Default => _default.Value;

        private RealmSchema(SchemaHandle handle, IEnumerable<ObjectSchema> objects)
        {
            _objects = new ReadOnlyDictionary<string, ObjectSchema>(objects.ToDictionary(o => o.Name));
            Handle = handle;
        }

        /// <summary>
        /// Find the definition of a class in this schema.
        /// </summary>
        /// <param name="name">A valid class name which may be in this schema.</param>
        /// <exception cref="ArgumentException">Thrown if an empty name is supplied.</exception>
        /// <returns>An object or null to indicate not found.</returns>
        public ObjectSchema Find(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Object schema name must be a non-empty string", nameof(name));
            Contract.EndContractBlock();

            ObjectSchema obj;
            _objects.TryGetValue(name, out obj);
            return obj;
        }

        /// <summary>
        /// Standard method from interface IEnumerable allows the RealmSchema to be used in a <c>foreach</c> or <c>ToList()</c>.
        /// </summary>
        /// <returns>An IEnumerator which will iterate through ObjectSchema declarations in this RealmSchema.</returns>
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

        /// <summary>
        /// Helper class used to construct a RealmSchema.
        /// </summary>
        public class Builder : List<ObjectSchema>
        {
            /// <summary>
            /// Build the RealmSchema to include all ObjectSchema added to this Builder.
            /// </summary>
            /// <exception cref="InvalidOperationException">Thrown if the Builder is empty.</exception>
            /// <returns>A completed RealmSchema, suitable for creating a new Realm.</returns>
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

                var objects = new List<SchemaObject>();
                var properties = new List<SchemaProperty>();

                foreach (var @object in this)
                {
                    var start = properties.Count;

                    properties.AddRange(@object.Select(ForMarshalling));

                    objects.Add(new SchemaObject
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

            private static SchemaProperty ForMarshalling(Schema.Property property)
            {
                return new SchemaProperty
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

