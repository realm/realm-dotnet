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
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson;
using Realms.Exceptions;
using Realms.Extensions;
using Realms.Schema;

namespace Realms.Dynamic
{
    internal class MetaRealmObject : DynamicMetaObject
    {
        private const BindingFlags PrivateBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private readonly Realm _realm;
        private readonly Metadata _metadata;

        private static readonly PropertyInfo RealmObjectRealmProperty = typeof(IRealmObjectBase).GetProperty(nameof(IRealmObjectBase.Realm), PrivateBindingFlags);
        private static readonly FieldInfo ObjectMetadataSchemaField = typeof(Metadata).GetField(nameof(Metadata.Schema), PrivateBindingFlags);
        private static readonly MethodInfo SchemaGetNameProperty = typeof(ObjectSchema).GetProperty(nameof(ObjectSchema.Name), PrivateBindingFlags).GetMethod;

        private static readonly MethodInfo RealmObjectGetBacklinksForHandle_RealmObject = typeof(RealmObjectExtensions)
            .GetMethod(nameof(RealmObjectExtensions.GetBacklinksForHandle), BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(typeof(DynamicRealmObject));

        private static readonly MethodInfo RealmObjectGetBacklinksForHandle_EmbeddedObject = typeof(RealmObjectExtensions)
           .GetMethod(nameof(RealmObjectExtensions.GetBacklinksForHandle), BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(typeof(DynamicEmbeddedObject));

        private static readonly MethodInfo RealmValueGetMethod = typeof(RealmValue).GetMethod(nameof(RealmValue.As), BindingFlags.Public | BindingFlags.Instance);
        private static readonly MethodInfo CreateRealmValueMethod = typeof(RealmValue).GetMethod(nameof(RealmValue.Create), BindingFlags.NonPublic | BindingFlags.Static);

        private static readonly MethodInfo RealmObjectGetObjectHandleMethod = typeof(RealmObjectExtensions)
            .GetMethod(nameof(RealmObjectExtensions.GetObjectHandle), BindingFlags.Public | BindingFlags.Static);

        private static readonly MethodInfo RealmObjectGetMetadataMethod = typeof(RealmObjectExtensions)
            .GetMethod(nameof(RealmObjectExtensions.GetObjectMetadata), BindingFlags.Public | BindingFlags.Static);

        private static readonly ObjectHandle DummyHandle = new ObjectHandle(null, IntPtr.Zero);

        public MetaRealmObject(Expression expression, IRealmObjectBase value)
            : base(expression, BindingRestrictions.Empty, value)
        {
            _realm = value.Realm;
            _metadata = value.GetObjectMetadata();
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            if (!_metadata.Schema.TryFindProperty(binder.Name, out var property))
            {
                return base.BindGetMember(binder);
            }

            var self = GetLimitedSelf();
            var arguments = new List<Expression>();
            MethodInfo getter = null;
            if (property.Type.UnderlyingType() == PropertyType.LinkingObjects)
            {
                arguments.Add(Expression.Constant(property.Name));
                arguments.Add(Expression.Constant(_metadata));
                getter = GetGetMethod(DummyHandle.GetBacklinks);
            }
            else if (property.Type.IsList())
            {
                arguments.Add(Expression.Property(self, RealmObjectRealmProperty));
                arguments.Add(Expression.Constant(property.Name));
                arguments.Add(Expression.Constant(_metadata));
                arguments.Add(Expression.Constant(property.ObjectType, typeof(string)));
                getter = property.Type.UnderlyingType() switch
                {
                    PropertyType.Int => property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetList<long?>) : GetGetMethod(DummyHandle.GetList<long>),
                    PropertyType.Bool => property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetList<bool?>) : GetGetMethod(DummyHandle.GetList<bool>),
                    PropertyType.Float => property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetList<float?>) : GetGetMethod(DummyHandle.GetList<float>),
                    PropertyType.Double => property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetList<double?>) : GetGetMethod(DummyHandle.GetList<double>),
                    PropertyType.String => GetGetMethod(DummyHandle.GetList<string>),
                    PropertyType.Data => GetGetMethod(DummyHandle.GetList<byte[]>),
                    PropertyType.Date => property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetList<DateTimeOffset?>) : GetGetMethod(DummyHandle.GetList<DateTimeOffset>),
                    PropertyType.ObjectId => property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetList<ObjectId?>) : GetGetMethod(DummyHandle.GetList<ObjectId>),
                    PropertyType.Decimal => property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetList<Decimal128?>) : GetGetMethod(DummyHandle.GetList<Decimal128>),
                    PropertyType.Guid => property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetList<Guid?>) : GetGetMethod(DummyHandle.GetList<Guid>),
                    PropertyType.Object => GetObjectGetCollectionMethod(property, CollectionType.List),
                    _ => throw new NotSupportedException($"Unable to get a list of {property.Type.UnderlyingType()}."),
                };
            }
            else if (property.Type.IsSet())
            {
                arguments.Add(Expression.Property(self, RealmObjectRealmProperty));
                arguments.Add(Expression.Constant(property.Name));
                arguments.Add(Expression.Constant(_metadata));
                arguments.Add(Expression.Constant(property.ObjectType, typeof(string)));
                getter = property.Type.UnderlyingType() switch
                {
                    PropertyType.Int => property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetSet<long?>) : GetGetMethod(DummyHandle.GetSet<long>),
                    PropertyType.Bool => property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetSet<bool?>) : GetGetMethod(DummyHandle.GetSet<bool>),
                    PropertyType.Float => property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetSet<float?>) : GetGetMethod(DummyHandle.GetSet<float>),
                    PropertyType.Double => property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetSet<double?>) : GetGetMethod(DummyHandle.GetSet<double>),
                    PropertyType.String => GetGetMethod(DummyHandle.GetSet<string>),
                    PropertyType.Data => GetGetMethod(DummyHandle.GetSet<byte[]>),
                    PropertyType.Date => property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetSet<DateTimeOffset?>) : GetGetMethod(DummyHandle.GetSet<DateTimeOffset>),
                    PropertyType.Object => GetObjectGetCollectionMethod(property, CollectionType.Set),
                    PropertyType.ObjectId => property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetSet<ObjectId?>) : GetGetMethod(DummyHandle.GetSet<ObjectId>),
                    PropertyType.Decimal => property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetSet<Decimal128?>) : GetGetMethod(DummyHandle.GetSet<Decimal128>),
                    PropertyType.Guid => property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetSet<Guid?>) : GetGetMethod(DummyHandle.GetSet<Guid>),
                    _ => throw new NotSupportedException($"Unable to get a set of {property.Type.UnderlyingType()}."),
                };
            }
            else if (property.Type.IsDictionary())
            {
                arguments.Add(Expression.Property(self, RealmObjectRealmProperty));
                arguments.Add(Expression.Constant(property.Name));
                arguments.Add(Expression.Constant(_metadata));
                arguments.Add(Expression.Constant(property.ObjectType, typeof(string)));
                getter = property.Type.UnderlyingType() switch
                {
                    PropertyType.Int => property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetDictionary<long?>) : GetGetMethod(DummyHandle.GetDictionary<long>),
                    PropertyType.Bool => property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetDictionary<bool?>) : GetGetMethod(DummyHandle.GetDictionary<bool>),
                    PropertyType.Float => property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetDictionary<float?>) : GetGetMethod(DummyHandle.GetDictionary<float>),
                    PropertyType.Double => property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetDictionary<double?>) : GetGetMethod(DummyHandle.GetDictionary<double>),
                    PropertyType.String => GetGetMethod(DummyHandle.GetDictionary<string>),
                    PropertyType.Data => GetGetMethod(DummyHandle.GetDictionary<byte[]>),
                    PropertyType.Date => property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetDictionary<DateTimeOffset?>) : GetGetMethod(DummyHandle.GetDictionary<DateTimeOffset>),
                    PropertyType.Object => GetObjectGetCollectionMethod(property, CollectionType.Dictionary),
                    PropertyType.ObjectId => property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetDictionary<ObjectId?>) : GetGetMethod(DummyHandle.GetDictionary<ObjectId>),
                    PropertyType.Decimal => property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetDictionary<Decimal128?>) : GetGetMethod(DummyHandle.GetDictionary<Decimal128>),
                    PropertyType.Guid => property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetDictionary<Guid?>) : GetGetMethod(DummyHandle.GetDictionary<Guid>),
                    _ => throw new NotSupportedException($"Unable to get a dictionary of {property.Type.UnderlyingType()}."),
                };
            }
            else
            {
                arguments.Add(Expression.Constant(property.Name));
                arguments.Add(Expression.Constant(_metadata));
                arguments.Add(Expression.Constant(_realm));

                getter = GetGetMethod(DummyHandle.GetValue);
            }

            var getHandleMethod = Expression.Call(RealmObjectGetObjectHandleMethod, self);
            Expression expression = Expression.Call(getHandleMethod, getter, arguments);

            if (property.Type.UnderlyingType() == PropertyType.LinkingObjects)
            {
                // no AsymmetricObjects involved here
                expression = IsTargetEmbedded(property)
                    ? Expression.Call(RealmObjectGetBacklinksForHandle_EmbeddedObject, self, Expression.Constant(binder.Name), expression)
                    : Expression.Call(RealmObjectGetBacklinksForHandle_RealmObject, self, Expression.Constant(binder.Name), expression);
            }

            if (expression.Type == typeof(RealmValue))
            {
                Type targetType;
                if (property.Type.UnderlyingType() == PropertyType.Object)
                {
                    targetType = IsTargetEmbedded(property) ? typeof(DynamicEmbeddedObject) : typeof(DynamicRealmObject);
                }
                else
                {
                    targetType = property.Type.ToType();
                }

                expression = Expression.Call(expression, RealmValueGetMethod.MakeGenericMethod(targetType));
            }

            if (binder.ReturnType != expression.Type)
            {
                expression = Expression.Convert(expression, binder.ReturnType);
            }

            return new DynamicMetaObject(expression, GetBindingRestrictions(self));
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            if (!_metadata.Schema.TryFindProperty(binder.Name, out var property) || property.Type.IsCollection(out _))
            {
                return base.BindSetMember(binder, value);
            }

            var arguments = new List<Expression>
            {
                Expression.Constant(property.Name),
                Expression.Constant(_metadata),
            };

            var self = GetLimitedSelf();
            var valueExpression = value.Expression;

            if (valueExpression.Type != typeof(RealmValue))
            {
                valueExpression = Expression.Call(CreateRealmValueMethod.MakeGenericMethod(valueExpression.Type), new[] { valueExpression, Expression.Constant(property.Type.ToRealmValueType()) });
            }

            var setter = property.IsPrimaryKey ? GetSetMethod<RealmValue>(DummyHandle.SetValueUnique) : GetSetMethod<RealmValue>(DummyHandle.SetValue);

            arguments.Add(valueExpression);

            if (!property.IsPrimaryKey)
            {
                arguments.Add(Expression.Constant(_realm));
            }

            var getHandleMethod = Expression.Call(RealmObjectGetObjectHandleMethod, self);

            var expression = Expression.Block(Expression.Call(getHandleMethod, setter, arguments), Expression.Default(binder.ReturnType));
            return new DynamicMetaObject(expression, GetBindingRestrictions(self));
        }

        private BindingRestrictions GetBindingRestrictions(Expression self)
        {
            var argumentShouldBeDynamicRealmObject = BindingRestrictions.GetTypeRestriction(Expression, GetDynamicObjectType(_metadata.Schema));
            var argumentShouldBeInTheSameRealm = BindingRestrictions.GetInstanceRestriction(Expression.Property(self, RealmObjectRealmProperty), _realm);
            var argumentShouldBeTheSameType = BindingRestrictions.GetExpressionRestriction(
                Expression.Equal(
                    Expression.Constant(_metadata.Schema.Name),
                    Expression.Property(
                        Expression.Field(
                            Expression.Call(RealmObjectGetMetadataMethod, self),
                            ObjectMetadataSchemaField),
                        SchemaGetNameProperty)));

            return argumentShouldBeDynamicRealmObject.Merge(argumentShouldBeInTheSameRealm).Merge(argumentShouldBeTheSameType);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
            => _metadata.Schema.Select(s => s.Name);

        private Expression GetLimitedSelf()
        {
            var convertedExpression = Expression;
            if (convertedExpression.Type != LimitType)
            {
                convertedExpression = Expression.Convert(convertedExpression, LimitType);
            }

            return convertedExpression;
        }

        private bool IsTargetEmbedded(Property property)
        {
            if (!_realm.Metadata.TryGetValue(property.ObjectType, out var metadata))
            {
                throw new RealmException($"Couldn't find metadata for type {property.ObjectType}.");
            }

            return metadata.Schema.BaseType == ObjectSchema.ObjectType.EmbeddedObject;
        }

        private static Type GetDynamicObjectType(ObjectSchema schema) =>
            schema.BaseType switch
            {
                ObjectSchema.ObjectType.RealmObject => typeof(DynamicRealmObject),
                ObjectSchema.ObjectType.EmbeddedObject => typeof(DynamicEmbeddedObject),
                ObjectSchema.ObjectType.AsymmetricObject => typeof(DynamicAsymmetricObject),
                _ => throw new NotSupportedException($"{schema.BaseType} not supported yet."),
            };

        private Type GetDynamicObjectType(Property property)
        {
            if (!_realm.Metadata.TryGetValue(property.ObjectType, out var metadata))
            {
                throw new RealmException($"Couldn't find metadata for type {property.ObjectType}.");
            }

            return GetDynamicObjectType(metadata.Schema);
        }

        private MethodInfo GetObjectGetCollectionMethod(Property property, CollectionType collectionType)
        {
            if (!_realm.Metadata.TryGetValue(property.ObjectType, out var metadata))
            {
                throw new RealmException($"Couldn't find metadata for type {property.ObjectType}.");
            }

            return metadata.Schema.BaseType switch
            {
                ObjectSchema.ObjectType.RealmObject => GetCollectionGetter<DynamicRealmObject>(collectionType),
                ObjectSchema.ObjectType.EmbeddedObject => GetCollectionGetter<DynamicEmbeddedObject>(collectionType),
                ObjectSchema.ObjectType.AsymmetricObject => GetCollectionGetter<DynamicAsymmetricObject>(collectionType),
                _ => throw new NotSupportedException($"{metadata.Schema.BaseType} not supported yet."),
            };

            MethodInfo GetCollectionGetter<T>(CollectionType collectionType)
                where T : IDynamicMetaObjectProvider =>
                collectionType switch
                {
                    CollectionType.List => GetGetMethod(DummyHandle.GetList<T>),
                    CollectionType.Set => GetGetMethod(DummyHandle.GetSet<T>),
                    CollectionType.Dictionary => GetGetMethod(DummyHandle.GetDictionary<T>),
                    _ => throw new NotSupportedException($"Collection {collectionType} not supported yet."),
                };
        }

        private enum CollectionType
        {
            List = 0,
            Set = 1,
            Dictionary = 2,
        }

        // GetBacklinks(propertyIndex)
        private static MethodInfo GetGetMethod<TResult>(Func<string, Metadata, TResult> @delegate) => @delegate.GetMethodInfo();

        // GetValue(propertyName, metadata)
        private static MethodInfo GetGetMethod<TResult>(Func<string, Metadata, Realm, TResult> @delegate) => @delegate.GetMethodInfo();

        // GetList(realm, propertyName, metadata, objectType)
        // GetSet(realm, propertyName, metadata, objectType)
        // GetDictionary(realm, propertyName, metadata, objectType)
        private static MethodInfo GetGetMethod<TResult>(Func<Realm, string, Metadata, string, TResult> @delegate) => @delegate.GetMethodInfo();

        private delegate void SetUniqueDelegate(string propertyName, Metadata metadata, in RealmValue value);

        // SetValueUnique(propertyName, metadata)
        private static MethodInfo GetSetMethod<TValue>(SetUniqueDelegate @delegate) => @delegate.GetMethodInfo();

        private delegate void SetValueDelegate(string propertyName, Metadata metadata, in RealmValue value, Realm realm);

        // SetValue
        private static MethodInfo GetSetMethod<TValue>(SetValueDelegate @delegate) => @delegate.GetMethodInfo();
    }
}
