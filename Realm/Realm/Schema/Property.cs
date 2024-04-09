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
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
    public readonly struct Property
    {
        internal static readonly HashSet<PropertyType> IndexableTypes = new()
        {
            PropertyType.String,
            PropertyType.Int,
            PropertyType.Bool,
            PropertyType.ObjectId,
            PropertyType.Guid,
            PropertyType.Date,
            PropertyType.RealmValue
        };

        internal static readonly HashSet<PropertyType> PrimaryKeyTypes = new()
        {
            PropertyType.String,
            PropertyType.Int,
            PropertyType.ObjectId,
            PropertyType.Guid,
        };

        /// <summary>
        /// Gets the name of the property as saved into realm.
        /// </summary>
        /// <value>The name of the property.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the managed name of the property.
        /// If not set, it is considered to be the same as <see cref="Name"/>.
        /// </summary>
        /// <value>The name of the property.</value>
        public string ManagedName { get; }

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
        public string? ObjectType { get; }

        /// <summary>
        /// Gets the name of the property that links to the model containing this
        /// <see cref="PropertyType.LinkingObjects"/> property. This will be <c>null</c>
        /// for properties where <see cref="Type"/> doesn't have the <see cref="PropertyType.LinkingObjects"/>
        /// flag.
        /// </summary>
        /// <value>The name of the linking property.</value>
        public string? LinkOriginPropertyName { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Property"/> is primary key.
        /// </summary>
        /// <value>
        /// <c>true</c> if the property is primary key (the matching property in the class definition is
        /// marked with <see cref="PrimaryKeyAttribute"/>); <c>false</c> otherwise.</value>
        public bool IsPrimaryKey { get; }

        /// <summary>
        /// Gets a value indicating the index mode for this <see cref="Property"/>.
        /// </summary>
        public IndexType IndexType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Property"/> struct.
        /// </summary>
        /// <param name="name">The name of the property. Sets <see cref="Name"/>.</param>
        /// <param name="type">The type of the property. Sets <see cref="Type"/>.</param>
        /// <param name="objectType">The object type of the property. Sets <see cref="ObjectType"/>.</param>
        /// <param name="linkOriginPropertyName">The name of the property that links to the model. Sets <see cref="LinkOriginPropertyName"/>.</param>
        /// <param name="isPrimaryKey">A flag indicating whether this property is a primary key. Sets <see cref="IsPrimaryKey"/>.</param>
        /// <param name="indexType">An enum indicating whether this property is indexed and the type of the index used. Sets <see cref="IndexType"/>.</param>
        /// <param name="managedName">The managed name of the property. Sets <see cref="ManagedName"/>.</param>
        public Property(string name, PropertyType type, string? objectType = null, string? linkOriginPropertyName = null, bool isPrimaryKey = false, IndexType indexType = IndexType.None, string? managedName = null)
        {
            Argument.NotNullOrEmpty(name, nameof(name));

            Name = name;
            Type = type;
            ObjectType = objectType;
            LinkOriginPropertyName = linkOriginPropertyName;
            ManagedName = managedName ?? name;

            var nonNullableType = type & ~PropertyType.Nullable;
            if (isPrimaryKey)
            {
                if (!PrimaryKeyTypes.Contains(nonNullableType))
                {
                    throw new ArgumentException($"Property of type {type} cannot be primary key. The only valid primary key types are {string.Join(", ", PrimaryKeyTypes)}.");
                }

                if (indexType == IndexType.FullText)
                {
                    throw new ArgumentException("PrimaryKey properties cannot have a FullText index added to them.");
                }

                indexType = IndexType.General;
            }

            IsPrimaryKey = isPrimaryKey;

            if (indexType == IndexType.General && !IndexableTypes.Contains(nonNullableType))
            {
                throw new ArgumentException($"Property of type {type} cannot be indexed. The only valid indexable types are {string.Join(", ", IndexableTypes)}.");
            }

            if (indexType == IndexType.FullText && nonNullableType != PropertyType.String)
            {
                throw new ArgumentException($"Property of type {type} cannot have a FullText index added to it. Only string properties support Full-Text indexing.");
            }

            if (type.HasFlag(PropertyType.Object))
            {
                var shouldBeNullable = !type.HasFlag(PropertyType.Array) && !type.HasFlag(PropertyType.Set);
                var isTypeNullable = type.HasFlag(PropertyType.Nullable);
                if (isTypeNullable && !shouldBeNullable)
                {
                    throw new ArgumentException("Property of type IList<RealmObject> or ISet<RealmObject> cannot be nullable.");
                }

                if (!isTypeNullable && shouldBeNullable)
                {
                    throw new ArgumentException("Property of type RealmObject or IDictionary<string, RealmObject> cannot be required.");
                }
            }

            IndexType = indexType;
        }

        internal Property(in SchemaProperty nativeProperty)
        {
            Name = nativeProperty.name!;
            string? managedName = nativeProperty.managed_name;
            ManagedName = !string.IsNullOrEmpty(managedName) ? managedName! : Name;
            Type = nativeProperty.type;
            ObjectType = nativeProperty.object_type;
            LinkOriginPropertyName = nativeProperty.link_origin_property_name;
            IsPrimaryKey = nativeProperty.is_primary;
            IndexType = nativeProperty.index;
        }

        internal SchemaProperty ToNative(Arena arena) => new()
        {
            name = StringValue.AllocateFrom(Name, arena),
            managed_name = StringValue.AllocateFrom(ManagedName != Name ? ManagedName : null, arena),
            type = Type,
            object_type = StringValue.AllocateFrom(ObjectType, arena),
            link_origin_property_name = StringValue.AllocateFrom(LinkOriginPropertyName, arena),
            is_primary = IsPrimaryKey,
            index = IndexType,
        };

        /// <summary>
        /// Initializes a new property from a <see cref="System.Type"/> value.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="type">
        /// The <see cref="System.Type"/> value that will be used to infer the <see cref="PropertyType"/>. Nullability
        /// will be inferred for value types, but must be specified via <paramref name="isNullable"/> for reference types.
        /// </param>
        /// <param name="isPrimaryKey">A flag indicating whether the property is primary key.</param>
        /// <param name="indexType">An enum indicating whether this property is indexed and the type of the index used. Sets <see cref="IndexType"/>.</param>
        /// <param name="isNullable">
        /// A flag indicating whether the property is nullable. Pass <c>null</c> to infer nullability from the <paramref name="type"/> argument.
        /// Pass a non-null value to override it.
        /// </param>
        /// <param name="managedName">The managed name of the property.</param>
        /// <returns>A <see cref="Property"/> instance that can be used to construct an <see cref="ObjectSchema"/>.</returns>
        public static Property FromType(string name, Type type, bool isPrimaryKey = false, IndexType indexType = IndexType.None, bool? isNullable = null, string? managedName = null)
        {
            Argument.NotNull(type, nameof(type));
            var propertyType = type.ToPropertyType(out var objectType);
            var objectTypeName = objectType?.GetMappedOrOriginalName();

            switch (isNullable)
            {
                case true:
                    propertyType |= PropertyType.Nullable;
                    break;
                case false:
                    propertyType &= ~PropertyType.Nullable;
                    break;
            }

            return new Property(name, propertyType, objectTypeName, isPrimaryKey: isPrimaryKey, indexType: indexType, managedName: managedName);
        }

        /// <summary>
        /// Initializes a new property describing the provided type.
        /// </summary>
        /// <typeparam name="T">
        /// The type that will be used to infer the <see cref="PropertyType"/>. Nullability
        /// will be inferred for value types, but must be specified via <paramref name="isNullable"/> for reference types.
        /// </typeparam>
        /// <param name="name">The name of the property.</param>
        /// <param name="isPrimaryKey">A flag indicating whether the property is primary key.</param>
        /// <param name="indexType">An enum indicating whether this property is indexed and the type of the index used. Sets <see cref="IndexType"/>.</param>
        /// <param name="isNullable">
        /// A flag indicating whether the property is nullable. Pass <c>null</c> to infer nullability from the <typeparamref name="T"/> argument.
        /// Pass a non-null value to override it.
        /// </param>
        /// <param name="managedName">The managed name of the property.</param>
        /// <returns>A <see cref="Property"/> instance that can be used to construct an <see cref="ObjectSchema"/>.</returns>
        public static Property FromType<T>(string name, bool isPrimaryKey = false, IndexType indexType = IndexType.None, bool? isNullable = null, string? managedName = null)
            => FromType(name, typeof(T), isPrimaryKey, indexType, isNullable, managedName: managedName);

        /// <summary>
        /// Initializes a new property of a primitive (string, int, date, etc.) type.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="type">
        /// The type of the property. Note that using <see cref="RealmValueType.Null"/> or <see cref="RealmValueType.Object"/> will result
        /// in an exception being thrown. If you want to create an object property, use <see cref="Object(string, string, string)"/>.
        /// </param>
        /// <param name="isPrimaryKey">A flag indicating whether the property is primary key.</param>
        /// <param name="indexType">An enum indicating whether this property is indexed and the type of the index used. Sets <see cref="IndexType"/>.</param>
        /// <param name="isNullable">A flag indicating whether the property is nullable.</param>
        /// <param name="managedName">The managed name of the property.</param>
        /// <returns>A <see cref="Property"/> instance that can be used to construct an <see cref="ObjectSchema"/>.</returns>
        public static Property Primitive(string name, RealmValueType type, bool isPrimaryKey = false, IndexType indexType = IndexType.None, bool isNullable = false, string? managedName = null)
            => PrimitiveCore(name, type, isPrimaryKey: isPrimaryKey, indexType: indexType, isNullable: isNullable, managedName: managedName);

        /// <summary>
        /// Initializes a new property describing a list of primitive values.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="type">
        /// The type of the property. Note that using <see cref="RealmValueType.Null"/> or <see cref="RealmValueType.Object"/> will result
        /// in an exception being thrown. If you want to create a list of objects property, use <see cref="ObjectList(string, string, string)"/>.
        /// </param>
        /// <param name="areElementsNullable">A flag indicating whether the elements of the list are nullable.</param>
        /// <param name="managedName">The managed name of the property.</param>
        /// <returns>A <see cref="Property"/> instance that can be used to construct an <see cref="ObjectSchema"/>.</returns>
        public static Property PrimitiveList(string name, RealmValueType type, bool areElementsNullable = false, string? managedName = null)
            => PrimitiveCore(name, type, PropertyType.Array, isNullable: areElementsNullable, managedName: managedName);

        /// <summary>
        /// Initializes a new property describing a set of primitive values.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="type">
        /// The type of the property. Note that using <see cref="RealmValueType.Null"/> or <see cref="RealmValueType.Object"/> will result
        /// in an exception being thrown. If you want to create a set of objects property, use <see cref="ObjectSet(string, string, string)"/>.
        /// </param>
        /// <param name="areElementsNullable">A flag indicating whether the elements of the list are nullable.</param>
        /// <param name="managedName">The managed name of the property.</param>
        /// <returns>A <see cref="Property"/> instance that can be used to construct an <see cref="ObjectSchema"/>.</returns>
        public static Property PrimitiveSet(string name, RealmValueType type, bool areElementsNullable = false, string? managedName = null)
            => PrimitiveCore(name, type, PropertyType.Set, isNullable: areElementsNullable, managedName: managedName);

        /// <summary>
        /// Initializes a new property describing a dictionary of strings to primitive values.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="type">
        /// The type of the property. Note that using <see cref="RealmValueType.Null"/> or <see cref="RealmValueType.Object"/> will result
        /// in an exception being thrown. If you want to create a dictionary of objects property, use <see cref="ObjectDictionary(string, string, string)"/>.
        /// </param>
        /// <param name="areElementsNullable">A flag indicating whether the elements of the list are nullable.</param>
        /// <param name="managedName">The managed name of the property.</param>
        /// <returns>A <see cref="Property"/> instance that can be used to construct an <see cref="ObjectSchema"/>.</returns>
        public static Property PrimitiveDictionary(string name, RealmValueType type, bool areElementsNullable = false, string? managedName = null)
            => PrimitiveCore(name, type, PropertyType.Dictionary, isNullable: areElementsNullable, managedName: managedName);

        /// <summary>
        /// Initializes a new property linking to a RealmObject.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="objectType">The object type. Both standalone and embedded objects are valid.</param>
        /// <param name="managedName">The managed name of the property.</param>
        /// <returns>A <see cref="Property"/> instance that can be used to construct an <see cref="ObjectSchema"/>.</returns>
        [SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "The property type describes an object.")]
        public static Property Object(string name, string objectType, string? managedName = null)
            => ObjectCore(name, objectType, PropertyType.Nullable, managedName: managedName);

        /// <summary>
        /// Initializes a new property describing a list of RealmObjects.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="objectType">The object type. Both standalone and embedded objects are valid.</param>
        /// <param name="managedName">The managed name of the property.</param>
        /// <returns>A <see cref="Property"/> instance that can be used to construct an <see cref="ObjectSchema"/>.</returns>
        public static Property ObjectList(string name, string objectType, string? managedName = null)
            => ObjectCore(name, objectType, PropertyType.Array, managedName: managedName);

        /// <summary>
        /// Initializes a new property describing a set of RealmObjects.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="objectType">The object type. Both standalone and embedded objects are valid.</param>
        /// <param name="managedName">The managed name of the property.</param>
        /// <returns>A <see cref="Property"/> instance that can be used to construct an <see cref="ObjectSchema"/>.</returns>
        public static Property ObjectSet(string name, string objectType, string? managedName = null)
            => ObjectCore(name, objectType, PropertyType.Set, managedName: managedName);

        /// <summary>
        /// Initializes a new property describing a dictionary of strings to RealmObjects.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="objectType">The object type. Both standalone and embedded objects are valid.</param>
        /// <param name="managedName">The managed name of the property.</param>
        /// <returns>A <see cref="Property"/> instance that can be used to construct an <see cref="ObjectSchema"/>.</returns>
        public static Property ObjectDictionary(string name, string objectType, string? managedName = null)
            => ObjectCore(name, objectType, PropertyType.Dictionary | PropertyType.Nullable, managedName: managedName);

        /// <summary>
        /// Initializes a new property of RealmValue type.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="managedName">The managed name of the property.</param>
        /// <returns>A <see cref="Property"/> instance that can be used to construct an <see cref="ObjectSchema"/>.</returns>
        public static Property RealmValue(string name, string? managedName = null)
        {
            Argument.NotNullOrEmpty(name, nameof(name));

            return new Property(name, PropertyType.RealmValue | PropertyType.Nullable, managedName: managedName);
        }

        /// <summary>
        /// Initializes a new property describing a list of RealmValues.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="managedName">The managed name of the property.</param>
        /// <returns>A <see cref="Property"/> instance that can be used to construct an <see cref="ObjectSchema"/>.</returns>
        public static Property RealmValueList(string name, string? managedName = null)
        {
            Argument.NotNullOrEmpty(name, nameof(name));

            return new Property(name, PropertyType.RealmValue | PropertyType.Array | PropertyType.Nullable, managedName: managedName);
        }

        /// <summary>
        /// Initializes a new property describing a set of RealmValues.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="managedName">The managed name of the property.</param>
        /// <returns>A <see cref="Property"/> instance that can be used to construct an <see cref="ObjectSchema"/>.</returns>
        public static Property RealmValueSet(string name, string? managedName = null)
        {
            Argument.NotNullOrEmpty(name, nameof(name));

            return new Property(name, PropertyType.RealmValue | PropertyType.Set | PropertyType.Nullable, managedName: managedName);
        }

        /// <summary>
        /// Initializes a new property describing a dictionary of RealmValues.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="managedName">The managed name of the property.</param>
        /// <returns>A <see cref="Property"/> instance that can be used to construct an <see cref="ObjectSchema"/>.</returns>
        public static Property RealmValueDictionary(string name, string? managedName = null)
        {
            Argument.NotNullOrEmpty(name, nameof(name));

            return new Property(name, PropertyType.RealmValue | PropertyType.Dictionary | PropertyType.Nullable, managedName: managedName);
        }

        /// <summary>
        /// Initializes a new property describing a collection of backlinks (all objects linking to this one via the specified property).
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="originObjectType">The object on the other side of the relationship.</param>
        /// <param name="originPropertyName">The property that is on the other end of the relationship.</param>
        /// <param name="managedName">The managed name of the property.</param>
        /// <returns>A <see cref="Property"/> instance that can be used to construct an <see cref="ObjectSchema"/>.</returns>
        /// <seealso cref="BacklinkAttribute"/>
        public static Property Backlinks(string name, string originObjectType, string originPropertyName, string? managedName = null)
        {
            Argument.NotNullOrEmpty(originObjectType, nameof(originObjectType));
            Argument.NotNullOrEmpty(originPropertyName, nameof(originPropertyName));

            return new Property(name, PropertyType.Array | PropertyType.LinkingObjects, originObjectType, originPropertyName, managedName: managedName);
        }

        internal static Property FromPropertyInfo(PropertyInfo prop)
        {
            var propertyName = prop.GetMappedOrOriginalName();
            var backlinksAttribute = prop.GetCustomAttribute<BacklinkAttribute>();
            if (backlinksAttribute != null)
            {
                var innerType = prop.PropertyType.GenericTypeArguments.Single();
                var linkOriginProperty = innerType.GetProperty(backlinksAttribute.Property)!;

                return Backlinks(propertyName, innerType.GetMappedOrOriginalName(), linkOriginProperty.GetMappedOrOriginalName());
            }

            var propertyType = prop.PropertyType.ToPropertyType(out var objectType);
            if (prop.HasCustomAttribute<RequiredAttribute>())
            {
                propertyType &= ~PropertyType.Nullable;
            }

            var objectTypeName = objectType?.GetMappedOrOriginalName();
            var isPrimaryKey = prop.HasCustomAttribute<PrimaryKeyAttribute>();
            var indexType = prop.GetCustomAttribute<IndexedAttribute>()?.Type ?? IndexType.None;
            return new Property(propertyName, propertyType, objectTypeName, isPrimaryKey: isPrimaryKey, indexType: indexType, managedName: prop.Name);
        }

        private static Property PrimitiveCore(string name, RealmValueType type, PropertyType collectionModifier = default, bool isPrimaryKey = false, IndexType indexType = IndexType.None,
            bool isNullable = false, string? managedName = null)
        {
            Argument.Ensure(type != RealmValueType.Null, $"{nameof(type)} can't be {RealmValueType.Null}", nameof(type));
            Argument.Ensure(type != RealmValueType.Object, $"{nameof(type)} can't be {RealmValueType.Object}. Use Property.Object instead.", nameof(type));

            return new Property(name, type.ToPropertyType(isNullable) | collectionModifier, isPrimaryKey: isPrimaryKey, indexType: indexType, managedName: managedName);
        }

        private static Property ObjectCore(string name, string objectType, PropertyType typeModifier = default, string? managedName = null)
        {
            Argument.NotNullOrEmpty(objectType, nameof(objectType));

            return new Property(name, PropertyType.Object | typeModifier, objectType, managedName: managedName);
        }
    }
}
