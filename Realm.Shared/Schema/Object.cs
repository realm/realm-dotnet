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
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Realms.Schema
{
    [DebuggerDisplay("Name = {Name}, Properties = {Count}")]
    internal class Object : ICollection<Property>
    {
        private readonly Dictionary<string, Property> _properties = new Dictionary<string, Property>();

        public string Name { get; private set; }

        public int Count => _properties.Count;

        bool ICollection<Property>.IsReadOnly => false;

        internal IntPtr Handle;
        internal Type Type;

        internal IEnumerable<string> PropertyNames => _properties.Keys;

        public Object(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Object name cannot be empty", nameof(name));
            Contract.EndContractBlock();

            Name = name;
        }

        public Property Find(string name)
        {
            Property property;
            _properties.TryGetValue(name, out property);
            return property;
        }

        public IEnumerator<Property> GetEnumerator()
        {
            return _properties.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Remove(Property property)
        {
            if (string.IsNullOrEmpty(property.Name)) throw new ArgumentException("Property name must be a string", nameof(property));
            Contract.EndContractBlock();

            return _properties.Remove(property.Name);
        }

        public void Clear()
        {
            _properties.Clear();
        }

        public void Add(Property property)
        {
            if (string.IsNullOrEmpty(property.Name)) throw new ArgumentException("Property name must be a string", nameof(property));
            Contract.EndContractBlock();

            _properties.Add(property.Name, property);
        }

        public bool Contains(Property property)
        {
            return _properties.ContainsValue(property);
        }

        public void CopyTo(Property[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length - arrayIndex >= Count) throw new ArgumentException("The number of items in this collection is greater than the available space in the array");
            Contract.EndContractBlock();

            foreach (var property in this)
            {
                array[arrayIndex++] = property;
            }
        }

        internal Object Clone(IntPtr handle)
        {
            var ret = new Object(Name) { Type = Type, Handle = handle };
            foreach (var kvp in _properties)
            {
                ret._properties.Add(kvp.Key, kvp.Value.Clone());
            }
            return ret;
        }

        public static Object FromType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (type.BaseType != typeof(RealmObject)) throw new ArgumentException($"The class {type.FullName} must descend directly from RealmObject");
            Contract.EndContractBlock();

            var @object = new Object(type.Name);
            @object.Type = type;
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (property.GetCustomAttribute<WovenPropertyAttribute>() == null) continue;

                var schemaProperty = new Property(property.GetCustomAttribute<MapToAttribute>()?.Mapping ?? property.Name)
                {
                    IsObjectId = property.GetCustomAttribute<ObjectIdAttribute>() != null,
                    IsIndexed = property.GetCustomAttribute<IndexedAttribute>() != null,
                    PropertyInfo = property
                };

                Type innerType;
                bool isNullable;
                schemaProperty.Type = property.PropertyType.ToPropertyType(out isNullable, out innerType);
                schemaProperty.ObjectType = innerType?.Name;
                schemaProperty.IsNullable = isNullable;

                @object.Add(schemaProperty);
            }

            return @object;
        }
    }
}

