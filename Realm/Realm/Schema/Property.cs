﻿////////////////////////////////////////////////////////////////////////////
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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Realms.Helpers;
using Realms.Native;

namespace Realms.Schema
{
    /// <summary>
    /// Describes a single property of a class stored in a <see cref="Realm"/>.
    /// </summary>
    [DebuggerDisplay("Name = {Name}, Type = {Type}")]
    public struct Property
    {
        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <value>The name of the property.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the type of the property.
        /// </summary>
        /// <value>The type of the property.</value>
        public PropertyType Type { get; }

        /// <summary>
        /// Gets the type of the object if relevant. This will be <c>null</c>
        /// for properties where <see cref="Type"/> doesn't have the <see cref="PropertyType.Object"/>
        /// flag.
        /// </summary>
        /// <value>The type of the object.</value>
        public string ObjectType { get; }

        /// <summary>
        /// Gets the name of the property that links to the model containing this
        /// <see cref="PropertyType.LinkingObjects"/> property. This will be <c>null</c>
        /// for properties where <see cref="Type"/> doesn't have the <see cref="PropertyType.LinkingObjects"/>
        /// flag.
        /// </summary>
        /// <value>The name of the linking property.</value>
        public string LinkOriginPropertyName { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Property"/> is primary key.
        /// </summary>
        /// <value>
        /// <c>true</c> if the property is primary key (the matching property in the class definition is
        /// marked with <see cref="PrimaryKeyAttribute"/>); <c>false</c> otherwise.</value>
        public bool IsPrimaryKey { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Property"/> is indexed.
        /// </summary>
        /// <value>
        /// <c>true</c> if the property should be indexed (the matching property in the class definition is
        /// marked with <see cref="IndexedAttribute"/>); <c>false</c> otherwise.</value>
        public bool IsIndexed { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Property"/> struct.
        /// </summary>
        /// <param name="name">The name of the property. Sets <see cref="Name"/>.</param>
        /// <param name="type">The type of the property. Sets <see cref="Type"/>.</param>
        /// <param name="objectType">The object type of the property. Sets <see cref="ObjectType"/>.</param>
        /// <param name="linkOriginPropertyName">The name of the property that links to the model. Sets <see cref="LinkOriginPropertyName"/>.</param>
        /// <param name="isPrimaryKey">A flag indicating whether this property is a primary key. Sets <see cref="IsPrimaryKey"/>.</param>
        /// <param name="isIndexed">A flag indicating whether this property is indexed. Sets <see cref="IsIndexed"/>.</param>
        public Property(string name, PropertyType type, string objectType = null, string linkOriginPropertyName = null, bool isPrimaryKey = false, bool isIndexed = false)
        {
            Argument.NotNullOrEmpty(name, nameof(name));

            Name = name;
            Type = type;
            ObjectType = objectType;
            LinkOriginPropertyName = linkOriginPropertyName;
            IsPrimaryKey = isPrimaryKey;
            IsIndexed = isPrimaryKey || isIndexed;

            PropertyInfo = null;
        }

        internal Property(SchemaProperty nativeProperty)
        {
            Name = nativeProperty.name;
            Type = nativeProperty.type;
            ObjectType = nativeProperty.object_type;
            LinkOriginPropertyName = nativeProperty.link_origin_property_name;
            IsPrimaryKey = nativeProperty.is_primary;
            IsIndexed = nativeProperty.is_indexed;

            PropertyInfo = null;
        }

        public static Property FromType(string name, Type type, bool isPrimaryKey = false, bool isIndexed = false)
        {
            Argument.NotNull(type, nameof(type));
            var propertyType = type.ToPropertyType(out var objectType);
            var objectTypeName = objectType?.GetTypeInfo().GetMappedOrOriginalName();

            return new Property(name, propertyType, objectTypeName, isPrimaryKey: isPrimaryKey, isIndexed: isIndexed);
        }

        public static Property FromType<T>(string name, bool isPrimaryKey = false, bool isIndexed = false)
            => FromType(name, typeof(T), isPrimaryKey, isIndexed);

        public static Property Scalar(string name, RealmValueType type, bool isNullable = false, bool isPrimaryKey = false, bool isIndexed = false)
            => ScalarCore(name, type, isNullable: isNullable, isPrimaryKey: isPrimaryKey, isIndexed: isIndexed);

        public static Property ScalarList(string name, RealmValueType type, bool areElementsNullable = false)
            => ScalarCore(name, type, PropertyType.Array, areElementsNullable);

        public static Property ScalarSet(string name, RealmValueType type, bool areElementsNullable = false)
            => ScalarCore(name, type, PropertyType.Set, areElementsNullable);

        public static Property ScalarDictionary(string name, RealmValueType type, bool areElementsNullable = false)
            => ScalarCore(name, type, PropertyType.Dictionary, areElementsNullable);

        public static Property Object(string name, string objectType)
            => ObjectCore(name, objectType);

        public static Property ObjectList(string name, string objectType)
            => ObjectCore(name, objectType, PropertyType.Array);

        public static Property ObjectSet(string name, string objectType)
            => ObjectCore(name, objectType, PropertyType.Set);

        public static Property ObjectDictionary(string name, string objectType)
            => ObjectCore(name, objectType, PropertyType.Dictionary);

        public static Property LinkingObjects(string name, string originObjectType, string originPropertyName)
        {
            Argument.NotNullOrEmpty(originObjectType, nameof(originObjectType));
            Argument.NotNullOrEmpty(originPropertyName, nameof(originPropertyName));

            return new Property(name, PropertyType.Array | PropertyType.LinkingObjects, originObjectType, originPropertyName);
        }

        internal static Property FromPropertyInfo(PropertyInfo prop)
        {
            var propertyName = prop.GetMappedOrOriginalName();
            var backlinksAttribute = prop.GetCustomAttribute<BacklinkAttribute>();
            if (backlinksAttribute != null)
            {
                var innerType = prop.PropertyType.GenericTypeArguments.Single();
                var linkOriginProperty = innerType.GetProperty(backlinksAttribute.Property);

                return LinkingObjects(propertyName, innerType.GetTypeInfo().GetMappedOrOriginalName(), linkOriginProperty.GetMappedOrOriginalName());
            }

            var propertyType = prop.PropertyType.ToPropertyType(out var objectType);
            if (prop.HasCustomAttribute<RequiredAttribute>())
            {
                propertyType &= ~PropertyType.Nullable;
            }

            var objectTypeName = objectType?.GetTypeInfo().GetMappedOrOriginalName();
            var isPrimaryKey = prop.HasCustomAttribute<PrimaryKeyAttribute>();
            var isIndexed = prop.HasCustomAttribute<IndexedAttribute>();
            return new Property(propertyName, propertyType, objectTypeName, isPrimaryKey: isPrimaryKey, isIndexed: isIndexed);
        }

        private static Property ScalarCore(string name, RealmValueType type, PropertyType collectionModifier = PropertyType.Required, bool isNullable = false, bool isPrimaryKey = false, bool isIndexed = false)
        {
            Argument.Ensure(type != RealmValueType.Null, $"{nameof(type)} can't be {RealmValueType.Null}", nameof(type));
            Argument.Ensure(type != RealmValueType.Object, $"{nameof(type)} can't be {RealmValueType.Object}. Use Property.Object instead.", nameof(type));

            return new Property(name, type.ToPropertyType(isNullable) | collectionModifier, isPrimaryKey: isPrimaryKey, isIndexed: isIndexed);
        }

        private static Property ObjectCore(string name, string objectType, PropertyType collectionModifier = PropertyType.Required)
            => new Property(name, PropertyType.Object | PropertyType.Nullable | collectionModifier, objectType);

        internal PropertyInfo PropertyInfo;
    }
}
