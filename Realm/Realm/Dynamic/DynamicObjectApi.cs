﻿////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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
using System.Linq;
using System.Runtime.CompilerServices;
using Realms.Helpers;
using Realms.Schema;

namespace Realms
{
    /// <summary>
    /// A class that exposes a set of API to access the data in a managed RealmObject dynamically.
    /// </summary>
    public struct DynamicObjectApi
    {
        private readonly ManagedAccessor _managedAccessor;

        internal DynamicObjectApi(ManagedAccessor managedAccessor)
        {
            _managedAccessor = managedAccessor;
        }

        /// <summary>
        /// Gets the value of the property <paramref name="propertyName"/> and casts it to
        /// <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The value of the property.</returns>
        /// <remarks>
        /// To get a list of all properties available on the object along with their types,
        /// use <see cref="ObjectSchema"/>.
        /// <br/>
        /// Casting to <see cref="RealmValue"/> is always valid. When the property is of type
        /// object, casting to <see cref="RealmObjectBase"/> is always valid.
        /// </remarks>
        public T Get<T>(string propertyName)
        {
            var property = GetProperty(propertyName);

            if (property.Type.IsComputed())
            {
                throw new NotSupportedException(
                    $"{_managedAccessor.ObjectSchema.Name}.{propertyName} is {property.GetDotnetTypeName()} (backlinks collection) and can't be accessed using {nameof(Dynamic)}.{nameof(Get)}. Use {nameof(GetBacklinks)} instead.");
            }

            if (property.Type.IsCollection(out var collectionType))
            {
                var collectionMethodName = collectionType switch
                {
                    PropertyType.Array => "GetList",
                    PropertyType.Set => "GetSet",
                    PropertyType.Dictionary => "GetDictionary",
                    _ => throw new NotSupportedException($"Invalid collection type received: {collectionType}")
                };

                throw new NotSupportedException(
                    $"{_managedAccessor.ObjectSchema.Name}.{propertyName} is {property.GetDotnetTypeName()} and can't be accessed using {nameof(Dynamic)}.{nameof(Get)}. Use {collectionMethodName} instead.");
            }

            return _managedAccessor.GetValue(propertyName).As<T>();
        }

        /// <summary>
        /// Sets the value of the property at <paramref name="propertyName"/> to
        /// <paramref name="value"/>.
        /// </summary>
        /// <param name="propertyName">The name of the property to set.</param>
        /// <param name="value">The new value of the property.</param>
        public void Set(string propertyName, RealmValue value)
        {
            var property = GetProperty(propertyName);

            if (property.Type.IsComputed())
            {
                throw new NotSupportedException(
                    $"{_managedAccessor.ObjectSchema.Name}.{propertyName} is {property.GetDotnetTypeName()} (backlinks collection) and can't be set directly");
            }

            if (property.Type.IsCollection(out _))
            {
                throw new NotSupportedException(
                    $"{_managedAccessor.ObjectSchema.Name}.{propertyName} is {property.GetDotnetTypeName()} (collection) and can't be set directly.");
            }

            if (!property.Type.IsNullable() && value.Type == RealmValueType.Null)
            {
                throw new ArgumentException($"{_managedAccessor.ObjectSchema.Name}.{propertyName} is {property.GetDotnetTypeName()} which is not nullable, but the supplied value is <null>.");
            }

            if (!property.Type.IsRealmValue() && value.Type != RealmValueType.Null && property.Type.ToRealmValueType() != value.Type)
            {
                throw new ArgumentException($"{_managedAccessor.ObjectSchema.Name}.{propertyName} is {property.GetDotnetTypeName()} but the supplied value is {value.AsAny().GetType().Name} ({value}).");
            }

            if (property.IsPrimaryKey)
            {
                _managedAccessor.SetValueUnique(propertyName, value);
            }
            else
            {
                _managedAccessor.SetValue(propertyName, value);
            }
        }

        /// <summary>
        /// Gets the value of a backlink property. This property must have been declared
        /// explicitly and annotated with <see cref="BacklinkAttribute"/>.
        /// </summary>
        /// <param name="propertyName">The name of the backlink property.</param>
        /// <returns>
        /// A queryable collection containing all objects pointing to this one via the
        /// property specified in <see cref="BacklinkAttribute.Property"/>.
        /// </returns>
        public IQueryable<IRealmObjectBase> GetBacklinks(string propertyName)
        {
            var property = GetProperty(propertyName, PropertyTypeEx.IsComputed);

            var resultsHandle = _managedAccessor.ObjectHandle.GetBacklinks(propertyName, _managedAccessor.Metadata);

            var relatedMeta = _managedAccessor.Realm.Metadata[property.ObjectType];
            if (relatedMeta.Schema.BaseType == ObjectSchema.ObjectType.EmbeddedObject)
            {
                return new RealmResults<IEmbeddedObject>(_managedAccessor.Realm, resultsHandle, relatedMeta);
            }

            return new RealmResults<IRealmObject>(_managedAccessor.Realm, resultsHandle, relatedMeta);
        }

        /// <summary>
        /// Gets a collection of all the objects that link to this object in the specified relationship.
        /// </summary>
        /// <param name="fromObjectType">The type of the object that is on the other end of the relationship.</param>
        /// <param name="fromPropertyName">The property that is on the other end of the relationship.</param>
        /// <returns>
        /// A queryable collection containing all objects of <paramref name="fromObjectType"/> that link
        /// to the current object via <paramref name="fromPropertyName"/>.
        /// </returns>
        public IQueryable<IRealmObjectBase> GetBacklinksFromType(string fromObjectType, string fromPropertyName)
        {
            Argument.Ensure(_managedAccessor.Realm.Metadata.TryGetValue(fromObjectType, out var relatedMeta), $"Could not find schema for type {fromObjectType}", nameof(fromObjectType));

            var resultsHandle = _managedAccessor.ObjectHandle.GetBacklinksForType(relatedMeta.TableKey, fromPropertyName, relatedMeta);
            if (relatedMeta.Schema.BaseType == ObjectSchema.ObjectType.EmbeddedObject)
            {
                return new RealmResults<IEmbeddedObject>(_managedAccessor.Realm, resultsHandle, relatedMeta);
            }

            return new RealmResults<IRealmObject>(_managedAccessor.Realm, resultsHandle, relatedMeta);
        }

        /// <summary>
        /// Gets a <see cref="IList{T}"/> property.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <param name="propertyName">The name of the list property.</param>
        /// <returns>The value of the list property.</returns>
        /// <remarks>
        /// To get a list of all properties available on the object along with their types,
        /// use <see cref="ObjectSchema"/>.
        /// <br/>
        /// Casting the elements to <see cref="RealmValue"/> is always valid. When the collection
        /// contains objects, casting to <see cref="RealmObjectBase"/> is always valid.
        /// </remarks>
        public IList<T> GetList<T>(string propertyName)
        {
            var property = GetProperty(propertyName, PropertyTypeEx.IsList);

            var result = _managedAccessor.ObjectHandle.GetList<T>(_managedAccessor.Realm, propertyName, _managedAccessor.Metadata, property.ObjectType);
            result.IsDynamic = true;
            return result;
        }

        /// <summary>
        /// Gets a <see cref="ISet{T}"/> property.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the Set.</typeparam>
        /// <param name="propertyName">The name of the Set property.</param>
        /// <returns>The value of the Set property.</returns>
        /// <remarks>
        /// To get a list of all properties available on the object along with their types,
        /// use <see cref="ObjectSchema"/>.
        /// <br/>
        /// Casting the elements to <see cref="RealmValue"/> is always valid. When the collection
        /// contains objects, casting to <see cref="RealmObjectBase"/> is always valid.
        /// </remarks>
        public ISet<T> GetSet<T>(string propertyName)
        {
            var property = GetProperty(propertyName, PropertyTypeEx.IsSet);

            var result = _managedAccessor.ObjectHandle.GetSet<T>(_managedAccessor.Realm, propertyName, _managedAccessor.Metadata, property.ObjectType);
            result.IsDynamic = true;
            return result;
        }

        /// <summary>
        /// Gets a <see cref="IDictionary{TKey, TValue}"/> property.
        /// </summary>
        /// <typeparam name="T">The type of the values in the dictionary.</typeparam>
        /// <param name="propertyName">The name of the dictionary property.</param>
        /// <returns>The value of the dictionary property.</returns>
        /// <remarks>
        /// To get a list of all properties available on the object along with their types,
        /// use <see cref="ObjectSchema"/>.
        /// <br/>
        /// Casting the values to <see cref="RealmValue"/> is always valid. When the collection
        /// contains objects, casting to <see cref="RealmObjectBase"/> is always valid.
        /// </remarks>
        public IDictionary<string, T> GetDictionary<T>(string propertyName)
        {
            var property = GetProperty(propertyName, PropertyTypeEx.IsDictionary);

            var result = _managedAccessor.ObjectHandle.GetDictionary<T>(_managedAccessor.Realm, propertyName, _managedAccessor.Metadata, property.ObjectType);
            result.IsDynamic = true;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Property GetProperty(string propertyName)
        {
            if (!_managedAccessor.ObjectSchema.TryFindProperty(propertyName, out var property))
            {
                throw new MissingMemberException($"Property {propertyName} does not exist on RealmObject of type {_managedAccessor.ObjectSchema.Name}", propertyName);
            }

            return property;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Property GetProperty(string propertyName, Func<PropertyType, bool> typeCheck, [CallerMemberName] string methodName = null)
        {
            Argument.NotNull(propertyName, nameof(propertyName));

            if (!_managedAccessor.ObjectSchema.TryFindProperty(propertyName, out var property))
            {
                throw new MissingMemberException($"Property {propertyName} does not exist on RealmObject of type {_managedAccessor.ObjectSchema.Name}", propertyName);
            }

            if (!typeCheck(property.Type))
            {
                throw new ArgumentException($"{_managedAccessor.ObjectSchema.Name}.{propertyName} is {property.GetDotnetTypeName()} which can't be accessed using {methodName}.");
            }

            return property;
        }
    }
}
