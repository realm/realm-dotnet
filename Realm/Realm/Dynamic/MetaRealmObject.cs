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
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson;
using Realms.Exceptions;
using Realms.Schema;

namespace Realms.Dynamic
{
    internal class MetaRealmObject : DynamicMetaObject
    {
        private const BindingFlags PrivateBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private readonly Realm _realm;
        private readonly RealmObjectBase.Metadata _metadata;

        private static readonly FieldInfo RealmObjectRealmField = typeof(RealmObjectBase).GetField("_realm", PrivateBindingFlags);
        private static readonly FieldInfo RealmObjectObjectHandleField = typeof(RealmObjectBase).GetField("_objectHandle", PrivateBindingFlags);
        private static readonly FieldInfo RealmObjectMetadataField = typeof(RealmObjectBase).GetField("_metadata", PrivateBindingFlags);
        private static readonly FieldInfo ObjectMetadataSchemaField = typeof(RealmObjectBase.Metadata).GetField(nameof(RealmObjectBase.Metadata.Schema), PrivateBindingFlags);
        private static readonly MethodInfo SchemaGetNameProperty = typeof(ObjectSchema).GetProperty(nameof(ObjectSchema.Name), PrivateBindingFlags).GetMethod;

        private static readonly MethodInfo RealmObjectGetBacklinksForHandle_RealmObject = typeof(DynamicRealmObject).GetMethod("GetBacklinksForHandle", PrivateBindingFlags)
                                                                                            .MakeGenericMethod(typeof(DynamicRealmObject));

        private static readonly MethodInfo RealmObjectGetBacklinksForHandle_EmbeddedObject = typeof(DynamicRealmObject).GetMethod("GetBacklinksForHandle", PrivateBindingFlags)
                                                                                              .MakeGenericMethod(typeof(DynamicEmbeddedObject));

        private static readonly MethodInfo RealmValueGetMethod = typeof(RealmValue).GetMethod(nameof(RealmValue.As), BindingFlags.Public | BindingFlags.Instance);
        private static readonly MethodInfo CreateRealmValueMethod = typeof(RealmValue).GetMethod(nameof(RealmValue.Create), BindingFlags.NonPublic | BindingFlags.Static);

        private static readonly ObjectHandle DummyHandle = new ObjectHandle(null, IntPtr.Zero);

        public MetaRealmObject(Expression expression, RealmObjectBase value)
            : base(expression, BindingRestrictions.Empty, value)
        {
            _realm = value.Realm;
            _metadata = value.ObjectMetadata;
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
                arguments.Add(Expression.Constant(_metadata.PropertyIndices[property.Name]));
                getter = GetGetMethod(DummyHandle.GetBacklinks);
            }
            else if (property.Type.IsArray())
            {
                arguments.Add(Expression.Field(self, RealmObjectRealmField));
                arguments.Add(Expression.Constant(_metadata.PropertyIndices[property.Name]));
                arguments.Add(Expression.Constant(property.ObjectType, typeof(string)));
                switch (property.Type.UnderlyingType())
                {
                    case PropertyType.Int:
                        getter = property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetList<long?>) : GetGetMethod(DummyHandle.GetList<long>);
                        break;
                    case PropertyType.Bool:
                        getter = property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetList<bool?>) : GetGetMethod(DummyHandle.GetList<bool>);
                        break;
                    case PropertyType.Float:
                        getter = property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetList<float?>) : GetGetMethod(DummyHandle.GetList<float>);
                        break;
                    case PropertyType.Double:
                        getter = property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetList<double?>) : GetGetMethod(DummyHandle.GetList<double>);
                        break;
                    case PropertyType.String:
                        getter = GetGetMethod(DummyHandle.GetList<string>);
                        break;
                    case PropertyType.Data:
                        getter = GetGetMethod(DummyHandle.GetList<byte[]>);
                        break;
                    case PropertyType.Date:
                        getter = property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetList<DateTimeOffset?>) : GetGetMethod(DummyHandle.GetList<DateTimeOffset>);
                        break;
                    case PropertyType.ObjectId:
                        getter = property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetList<ObjectId?>) : GetGetMethod(DummyHandle.GetList<ObjectId>);
                        break;
                    case PropertyType.Decimal:
                        getter = property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetList<Decimal128?>) : GetGetMethod(DummyHandle.GetList<Decimal128>);
                        break;
                    case PropertyType.Object:
                        getter = IsTargetEmbedded(property) ? GetGetMethod(DummyHandle.GetList<DynamicEmbeddedObject>) : GetGetMethod(DummyHandle.GetList<DynamicRealmObject>);
                        break;
                }
            }
            else if (property.Type.IsSet())
            {
                arguments.Add(Expression.Field(self, RealmObjectRealmField));
                arguments.Add(Expression.Constant(_metadata.PropertyIndices[property.Name]));
                arguments.Add(Expression.Constant(property.ObjectType, typeof(string)));
                switch (property.Type.UnderlyingType())
                {
                    case PropertyType.Int:
                        getter = property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetSet<long?>) : GetGetMethod(DummyHandle.GetSet<long>);
                        break;
                    case PropertyType.Bool:
                        getter = property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetSet<bool?>) : GetGetMethod(DummyHandle.GetSet<bool>);
                        break;
                    case PropertyType.Float:
                        getter = property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetSet<float?>) : GetGetMethod(DummyHandle.GetSet<float>);
                        break;
                    case PropertyType.Double:
                        getter = property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetSet<double?>) : GetGetMethod(DummyHandle.GetSet<double>);
                        break;
                    case PropertyType.String:
                        getter = GetGetMethod(DummyHandle.GetSet<string>);
                        break;
                    case PropertyType.Data:
                        getter = GetGetMethod(DummyHandle.GetSet<byte[]>);
                        break;
                    case PropertyType.Date:
                        getter = property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetSet<DateTimeOffset?>) : GetGetMethod(DummyHandle.GetSet<DateTimeOffset>);
                        break;
                    case PropertyType.Object:
                        getter = IsTargetEmbedded(property) ? GetGetMethod(DummyHandle.GetSet<DynamicEmbeddedObject>) : GetGetMethod(DummyHandle.GetSet<DynamicRealmObject>);
                        break;
                    case PropertyType.ObjectId:
                        getter = property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetSet<ObjectId?>) : GetGetMethod(DummyHandle.GetSet<ObjectId>);
                        break;
                    case PropertyType.Decimal:
                        getter = property.Type.IsNullable() ? GetGetMethod(DummyHandle.GetSet<Decimal128?>) : GetGetMethod(DummyHandle.GetSet<Decimal128>);
                        break;
                }
            }
            else
            {
                arguments.Add(Expression.Constant(property.Name));
                arguments.Add(Expression.Constant(_metadata));
                arguments.Add(Expression.Constant(_realm));

                getter = GetGetMethod(DummyHandle.GetValue);
            }

            var instance = Expression.Field(self, RealmObjectObjectHandleField);
            Expression expression = Expression.Call(instance, getter, arguments);

            if (property.Type.UnderlyingType() == PropertyType.LinkingObjects)
            {
                if (IsTargetEmbedded(property))
                {
                    expression = Expression.Call(self, RealmObjectGetBacklinksForHandle_EmbeddedObject, Expression.Constant(binder.Name), expression);
                }
                else
                {
                    expression = Expression.Call(self, RealmObjectGetBacklinksForHandle_RealmObject, Expression.Constant(binder.Name), expression);
                }
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
                    targetType = property.PropertyInfo?.PropertyType ?? property.Type.ToType();
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
            if (!_metadata.Schema.TryFindProperty(binder.Name, out var property) || property.Type.IsArray() || property.Type.IsSet())
            {
                return base.BindSetMember(binder, value);
            }

            var arguments = new List<Expression>
            {
                Expression.Constant(_metadata.PropertyIndices[property.Name])
            };

            var self = GetLimitedSelf();
            var valueExpression = value.Expression;

            valueExpression = Expression.Call(CreateRealmValueMethod.MakeGenericMethod(valueExpression.Type), new[] { valueExpression, Expression.Constant(property.Type.ToRealmValueType()) });
            var setter = property.IsPrimaryKey ? GetSetMethod<RealmValue>(DummyHandle.SetValueUnique) : GetSetMethod<RealmValue>(DummyHandle.SetValue);

            if (valueExpression.Type != typeof(RealmValue))
            {
                valueExpression = Expression.Convert(valueExpression, typeof(RealmValue));
            }

            arguments.Add(valueExpression);

            if (!property.IsPrimaryKey)
            {
                arguments.Add(Expression.Constant(_realm));
            }

            var expression = Expression.Block(Expression.Call(Expression.Field(self, RealmObjectObjectHandleField), setter, arguments), Expression.Default(binder.ReturnType));
            return new DynamicMetaObject(expression, GetBindingRestrictions(self));
        }

        private BindingRestrictions GetBindingRestrictions(Expression self)
        {
            var argumentShouldBeDynamicRealmObject = BindingRestrictions.GetTypeRestriction(Expression, _metadata.Schema.IsEmbedded ? typeof(DynamicEmbeddedObject) : typeof(DynamicRealmObject));
            var argumentShouldBeInTheSameRealm = BindingRestrictions.GetInstanceRestriction(Expression.Field(self, RealmObjectRealmField), _realm);
            var argumentShouldBeTheSameType = BindingRestrictions.GetExpressionRestriction(
                Expression.Equal(
                    Expression.Constant(_metadata.Schema.Name),
                    Expression.Property(
                        Expression.Field(
                            Expression.Field(
                                self,
                                RealmObjectMetadataField),
                            ObjectMetadataSchemaField),
                        SchemaGetNameProperty)));

            return argumentShouldBeDynamicRealmObject.Merge(argumentShouldBeInTheSameRealm).Merge(argumentShouldBeTheSameType);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _metadata.Schema.PropertyNames;
        }

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

            return metadata.Schema.IsEmbedded;
        }

        // GetString(propertyIndex)
        // GetBacklinks(propertyIndex)
        private static MethodInfo GetGetMethod<TResult>(Func<IntPtr, TResult> @delegate) => @delegate.GetMethodInfo();

        // GetValue(propertyIndex)
        private static MethodInfo GetGetMethod<TResult>(Func<string, RealmObjectBase.Metadata, Realm, TResult> @delegate) => @delegate.GetMethodInfo();

        // GetList(realm, propertyIndex, objectType)
        // GetSet(realm, propertyIndex, objectType)
        // GetObject(realm, propertyIndex, objectType)
        private static MethodInfo GetGetMethod<TResult>(Func<Realm, IntPtr, string, TResult> @delegate) => @delegate.GetMethodInfo();

        private delegate void SetUniqueDelegate(IntPtr index, in RealmValue value);

        // SetValueUnique(propertyIndex)
        private static MethodInfo GetSetMethod<TValue>(SetUniqueDelegate @delegate) => @delegate.GetMethodInfo();

        private delegate void SetValueDelegate(IntPtr index, in RealmValue value, Realm realm);

        // SetValue
        private static MethodInfo GetSetMethod<TValue>(SetValueDelegate @delegate) => @delegate.GetMethodInfo();

        // SetObject(this, propertyIndex)
        private static MethodInfo GetSetMethod<TValue>(Action<Realm, IntPtr, TValue> @delegate) => @delegate.GetMethodInfo();
    }
}