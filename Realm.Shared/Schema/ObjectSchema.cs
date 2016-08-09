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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Realms.Schema
{
    [DebuggerDisplay("Name = {Name}, Properties = {Count}")]
    public class ObjectSchema : IReadOnlyCollection<Property>
    {
        private readonly ReadOnlyDictionary<string, Property> _properties;

        public string Name { get; private set; }

        public int Count => _properties.Count;

        internal IntPtr Handle;
        internal Type Type;

        internal IEnumerable<string> PropertyNames => _properties.Keys;

        private ObjectSchema(string name, IDictionary<string, Property> properties)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Object name cannot be empty", nameof(name));
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            Contract.EndContractBlock();

            Name = name;
            _properties = new ReadOnlyDictionary<string, Property>(properties);
        }

        public bool TryFindProperty(string name, out Property property) => _properties.TryGetValue(name, out property);

        public IEnumerator<Property> GetEnumerator()
        {
            return _properties.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal ObjectSchema Clone(IntPtr handle)
        {
            return new ObjectSchema(Name, _properties) { Type = Type, Handle = handle };
        }

        public static ObjectSchema FromType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (type.BaseType != typeof(RealmObject)) throw new ArgumentException($"The class {type.FullName} must descend directly from RealmObject");
            Contract.EndContractBlock();

            var builder = new Builder(type.Name);
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (property.GetCustomAttribute<WovenPropertyAttribute>() == null) continue;

                var schemaProperty = new Property
                {
                    Name = property.GetCustomAttribute<MapToAttribute>()?.Mapping ?? property.Name,
                    IsObjectId = property.GetCustomAttribute<ObjectIdAttribute>() != null,
                    IsIndexed = property.GetCustomAttribute<IndexedAttribute>() != null,
                    PropertyInfo = property
                };

                Type innerType;
                bool isNullable;
                schemaProperty.Type = property.PropertyType.ToPropertyType(out isNullable, out innerType);
                schemaProperty.ObjectType = innerType?.Name;
                schemaProperty.IsNullable = isNullable;

                builder.Add(schemaProperty);
            }

            var ret = builder.Build();
            ret.Type = type;
            return ret;
        }

        public class Builder : List<Property>
        {
            public string Name { get; }

            public Builder(string name)
            {
                if (string.IsNullOrEmpty(name)) throw new ArgumentException("Object name cannot be empty", nameof(name));
                Contract.EndContractBlock();

                Name = name;
            }

            public ObjectSchema Build()
            {
                if (Count == 0) 
                {
                    throw new InvalidOperationException(
                        $"No properties in {Name}, has linker stripped it? See https://realm.io/docs/xamarin/latest/#linker-stripped-schema");
                }
                Contract.EndContractBlock();

                return new ObjectSchema(Name, this.ToDictionary(p => p.Name));
            }
        }
    }
}

