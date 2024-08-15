////////////////////////////////////////////////////////////////////////////
//
// Copyright 2024 Realm Inc.
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
    /// <inheritdoc/>
    public class DynamicManagedObjectApi : DynamicObjectApi
    {
        private readonly ManagedAccessor _managedAccessor;

        private readonly bool _isRelaxedSchema;

        internal DynamicManagedObjectApi(ManagedAccessor managedAccessor)
        {
            _managedAccessor = managedAccessor;
            _isRelaxedSchema = managedAccessor.Realm.Config.RelaxedSchema;
        }

        /// <inheritdoc/>
        public override RealmValue Get(string propertyName)
        {
            CheckGetPropertySuitability(propertyName);

            return _managedAccessor.GetValue(propertyName);
        }

        /// <inheritdoc/>
        public override T Get<T>(string propertyName)
        {
            return Get(propertyName).As<T>();
        }

        /// <inheritdoc/>
        public override bool TryGet(string propertyName, out RealmValue propertyValue)
        {
            CheckGetPropertySuitability(propertyName);

            return _managedAccessor.TryGetValue(propertyName, out propertyValue);
        }

        /// <inheritdoc/>
        public override bool TryGet<T>(string propertyName, out T? propertyValue)
            where T : default
        {
            var foundValue = TryGet(propertyName, out var val);
            if (foundValue)
            {
                propertyValue = val.As<T>();
                return true;
            }

            propertyValue = default;
            return false;
        }

        /// <inheritdoc/>
        public override void Set(string propertyName, RealmValue value)
        {
            if (GetModelProperty(propertyName, throwOnMissing: !_isRelaxedSchema) is Property property)
            {
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
                    throw new ArgumentException($"{_managedAccessor.ObjectSchema.Name}.{propertyName} is {property.GetDotnetTypeName()} but the supplied value is {value.AsAny()?.GetType().Name} ({value}).");
                }

                if (property.IsPrimaryKey)
                {
                    _managedAccessor.SetValueUnique(propertyName, value);
                    return;
                }
            }

            _managedAccessor.SetValue(propertyName, value);
        }

        /// <inheritdoc/>
        public override bool Unset(string propertyName)
        {
            return _managedAccessor.UnsetProperty(propertyName);
        }

        /// <inheritdoc/>
        public override IQueryable<IRealmObjectBase> GetBacklinks(string propertyName)
        {
            var property = GetModelProperty(propertyName, PropertyTypeEx.IsComputed);

            var resultsHandle = _managedAccessor.ObjectHandle.GetBacklinks(propertyName, _managedAccessor.Metadata);

            var relatedMeta = _managedAccessor.Realm.Metadata[property.ObjectType!];
            if (relatedMeta.Schema.BaseType == ObjectSchema.ObjectType.EmbeddedObject)
            {
                return new RealmResults<IEmbeddedObject>(_managedAccessor.Realm, resultsHandle, relatedMeta);
            }

            return new RealmResults<IRealmObject>(_managedAccessor.Realm, resultsHandle, relatedMeta);
        }

        /// <inheritdoc/>
        public override IQueryable<IRealmObjectBase> GetBacklinksFromType(string fromObjectType, string fromPropertyName)
        {
            Argument.Ensure(_managedAccessor.Realm.Metadata.TryGetValue(fromObjectType, out var relatedMeta), $"Could not find schema for type {fromObjectType}", nameof(fromObjectType));

            var resultsHandle = _managedAccessor.ObjectHandle.GetBacklinksForType(relatedMeta.TableKey, fromPropertyName, relatedMeta);
            if (relatedMeta.Schema.BaseType == ObjectSchema.ObjectType.EmbeddedObject)
            {
                return new RealmResults<IEmbeddedObject>(_managedAccessor.Realm, resultsHandle, relatedMeta);
            }

            return new RealmResults<IRealmObject>(_managedAccessor.Realm, resultsHandle, relatedMeta);
        }

        /// <inheritdoc/>
        public override IList<T> GetList<T>(string propertyName)
        {
            var property = GetModelProperty(propertyName, PropertyTypeEx.IsList);

            var result = _managedAccessor.ObjectHandle.GetList<T>(_managedAccessor.Realm, propertyName, _managedAccessor.Metadata, property.ObjectType);
            result.IsDynamic = true;
            return result;
        }

        /// <inheritdoc/>
        public override ISet<T> GetSet<T>(string propertyName)
        {
            var property = GetModelProperty(propertyName, PropertyTypeEx.IsSet);

            var result = _managedAccessor.ObjectHandle.GetSet<T>(_managedAccessor.Realm, propertyName, _managedAccessor.Metadata, property.ObjectType);
            result.IsDynamic = true;
            return result;
        }

        /// <inheritdoc/>
        public override IDictionary<string, T> GetDictionary<T>(string propertyName)
        {
            var property = GetModelProperty(propertyName, PropertyTypeEx.IsDictionary);

            var result = _managedAccessor.ObjectHandle.GetDictionary<T>(_managedAccessor.Realm, propertyName, _managedAccessor.Metadata, property.ObjectType);
            result.IsDynamic = true;
            return result;
        }

        private void CheckGetPropertySuitability(string propertyName)
        {
            if (GetModelProperty(propertyName, throwOnMissing: !_isRelaxedSchema) is Property property)
            {
                if (property.Type.IsComputed())
                {
                    throw new NotSupportedException(
                        $"{_managedAccessor.ObjectSchema.Name}.{propertyName} is {property.GetDotnetTypeName()} (backlinks collection) and can't be accessed using {nameof(Dynamic)}.{nameof(Get)}. Use {nameof(GetBacklinks)} instead.");
                }

                if (property.Type.IsCollection(out var collectionType) && collectionType == PropertyType.Set)
                {
                    throw new NotSupportedException(
                        $"{_managedAccessor.ObjectSchema.Name}.{propertyName} is {property.GetDotnetTypeName()} and can't be accessed using {nameof(Dynamic)}.{nameof(Get)}. Use GetSet instead.");
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Property? GetModelProperty(string propertyName, bool throwOnMissing)
        {
            Argument.NotNull(propertyName, nameof(propertyName));

            if (!_managedAccessor.ObjectSchema.TryFindModelProperty(propertyName, out var property))
            {
                if (throwOnMissing)
                {
                    throw new MissingMemberException(_managedAccessor.ObjectSchema.Name, propertyName);
                }

                return null;
            }

            return property;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Property GetModelProperty(string propertyName, Func<PropertyType, bool> typeCheck, [CallerMemberName] string methodName = "")
        {
            Argument.NotNull(propertyName, nameof(propertyName));

            if (!_managedAccessor.ObjectSchema.TryFindModelProperty(propertyName, out var property))
            {
                throw new MissingMemberException(_managedAccessor.ObjectSchema.Name, propertyName);
            }

            if (!typeCheck(property.Type))
            {
                throw new ArgumentException($"{_managedAccessor.ObjectSchema.Name}.{propertyName} is {property.GetDotnetTypeName()} which can't be accessed using {methodName}.");
            }

            return property;
        }
    }
}
