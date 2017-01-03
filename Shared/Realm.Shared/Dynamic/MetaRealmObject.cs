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
using System.Linq.Expressions;
using System.Reflection;

namespace Realms.Dynamic
{
    internal class MetaRealmObject : DynamicMetaObject
    {
        private const BindingFlags PrivateBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private readonly Realm _realm;
        private readonly RealmObject.Metadata _metadata;

        private static readonly FieldInfo RealmObjectRealmField = typeof(RealmObject).GetTypeInfo().GetField("_realm", PrivateBindingFlags);
        private static readonly FieldInfo RealmObjectObjectHandleField = typeof(RealmObject).GetTypeInfo().GetField("_objectHandle", PrivateBindingFlags);
        private static readonly MethodInfo RealmObjectGetBacklinksForHandleMethod = typeof(RealmObject).GetMethod("GetBacklinksForHandle", PrivateBindingFlags, null, new[] { typeof(string), typeof(ResultsHandle) }, null)
                                                                                              .MakeGenericMethod(typeof(DynamicRealmObject));

        private static readonly ObjectHandle dummyHandle = new ObjectHandle(null);

        public MetaRealmObject(Expression expression, DynamicRealmObject value)
            : base(expression, BindingRestrictions.Empty, value)
        {
            _realm = value.Realm;
            _metadata = value.ObjectMetadata;
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            Schema.Property property;
            if (!_metadata.Schema.TryFindProperty(binder.Name, out property))
            {
                return base.BindGetMember(binder);
            }

            var arguments = new List<Expression>
            {
                Expression.Constant(_metadata.PropertyIndices[property.Name])
            };

            MethodInfo getter = null;
            switch (property.Type)
            {
                case Schema.PropertyType.Int:
                    if (property.IsNullable)
                    {
                        getter = GetGetMethod(dummyHandle.GetNullableInt64);
                    }
                    else
                    {
                        getter = GetGetMethod(dummyHandle.GetInt64);
                    }

                    break;
                case Schema.PropertyType.Bool:
                    if (property.IsNullable)
                    {
                        getter = GetGetMethod(dummyHandle.GetNullableBoolean);
                    }
                    else
                    {
                        getter = GetGetMethod(dummyHandle.GetBoolean);
                    }

                    break;
                case Schema.PropertyType.Float:
                    if (property.IsNullable)
                    {
                        getter = GetGetMethod(dummyHandle.GetNullableSingle);
                    }
                    else
                    {
                        getter = GetGetMethod(dummyHandle.GetSingle);
                    }

                    break;
                case Schema.PropertyType.Double:
                    if (property.IsNullable)
                    {
                        getter = GetGetMethod(dummyHandle.GetNullableDouble);
                    }
                    else
                    {
                        getter = GetGetMethod(dummyHandle.GetDouble);
                    }

                    break;
                case Schema.PropertyType.String:
                    getter = GetGetMethod(dummyHandle.GetString);
                    break;
                case Schema.PropertyType.Data:
                    getter = GetGetMethod(dummyHandle.GetByteArray);
                    break;
                case Schema.PropertyType.Date:
                    if (property.IsNullable)
                    {
                        getter = GetGetMethod(dummyHandle.GetNullableDateTimeOffset);
                    }
                    else
                    {
                        getter = GetGetMethod(dummyHandle.GetDateTimeOffset);
                    }

                    break;
                case Schema.PropertyType.Object:
                    arguments.Insert(0, Expression.Field(GetLimitedSelf(), RealmObjectRealmField));
                    arguments.Add(Expression.Constant(property.ObjectType));
                    getter = GetGetMethod(dummyHandle.GetObject<DynamicRealmObject>);
                    break;
                case Schema.PropertyType.Array:
                    arguments.Insert(0, Expression.Field(GetLimitedSelf(), RealmObjectRealmField));
                    arguments.Add(Expression.Constant(property.ObjectType));
                    getter = GetGetMethod(dummyHandle.GetList<DynamicRealmObject>);
                    break;
                case Schema.PropertyType.LinkingObjects:
                    getter = GetGetMethod(dummyHandle.GetBacklinks);
                    break;
            }

            var self = GetLimitedSelf();
            var instance = Expression.Field(self, RealmObjectObjectHandleField);
            Expression expression = Expression.Call(instance, getter, arguments);

            if (property.Type == Schema.PropertyType.LinkingObjects)
            {
                expression = Expression.Call(self, RealmObjectGetBacklinksForHandleMethod, Expression.Constant(binder.Name), expression);
            }

            if (binder.ReturnType != expression.Type)
            {
                expression = Expression.Convert(expression, binder.ReturnType);
            }

            var argumentShouldBeDynamicRealmObject = BindingRestrictions.GetTypeRestriction(Expression, typeof(DynamicRealmObject));
            var argumentShouldBeInTheSameRealm = BindingRestrictions.GetInstanceRestriction(Expression.Field(self, RealmObjectRealmField), _realm);
            return new DynamicMetaObject(expression, argumentShouldBeDynamicRealmObject.Merge(argumentShouldBeInTheSameRealm));
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            Schema.Property property;
            if (!_metadata.Schema.TryFindProperty(binder.Name, out property))
            {
                return base.BindSetMember(binder, value);
            }

            var arguments = new List<Expression>
            {
                Expression.Constant(_metadata.PropertyIndices[property.Name])
            };

            MethodInfo setter = null;
            Type argumentType = null;

            switch (property.Type)
            {
                case Schema.PropertyType.Int:
                    argumentType = typeof(long);
                    if (property.IsNullable)
                    {
                        setter = GetSetMethod<long?>(dummyHandle.SetNullableInt64);
                    }
                    else if (property.IsPrimaryKey)
                    {
                        setter = GetSetMethod<long>(dummyHandle.SetInt64Unique);
                    }
                    else
                    {
                        setter = GetSetMethod<long>(dummyHandle.SetInt64);
                    }

                    break;
                case Schema.PropertyType.Bool:
                    argumentType = typeof(bool);
                    if (property.IsNullable)
                    {
                        setter = GetSetMethod<bool?>(dummyHandle.SetNullableBoolean);
                    }
                    else
                    {
                        setter = GetSetMethod<bool>(dummyHandle.SetBoolean);
                    }

                    break;
                case Schema.PropertyType.Float:
                    argumentType = typeof(float);
                    if (property.IsNullable)
                    {
                        setter = GetSetMethod<float?>(dummyHandle.SetNullableSingle);
                    }
                    else
                    {
                        setter = GetSetMethod<float>(dummyHandle.SetSingle);
                    }

                    break;
                case Schema.PropertyType.Double:
                    argumentType = typeof(double);
                    if (property.IsNullable)
                    {
                        setter = GetSetMethod<double?>(dummyHandle.SetNullableDouble);
                    }
                    else
                    {
                        setter = GetSetMethod<double>(dummyHandle.SetDouble);
                    }

                    break;
                case Schema.PropertyType.String:
                    argumentType = typeof(string);
                    if (property.IsPrimaryKey)
                    {
                        setter = GetSetMethod<string>(dummyHandle.SetStringUnique);
                    }
                    else
                    {
                        setter = GetSetMethod<string>(dummyHandle.SetString);
                    }

                    break;
                case Schema.PropertyType.Data:
                    argumentType = typeof(byte[]);
                    setter = GetSetMethod<byte[]>(dummyHandle.SetByteArray);
                    break;
                case Schema.PropertyType.Date:
                    argumentType = typeof(DateTimeOffset);
                    if (property.IsNullable)
                    {
                        setter = GetSetMethod<DateTimeOffset?>(dummyHandle.SetNullableDateTimeOffset);
                    }
                    else
                    {
                        setter = GetSetMethod<DateTimeOffset>(dummyHandle.SetDateTimeOffset);
                    }

                    break;
                case Schema.PropertyType.Object:
                    argumentType = typeof(RealmObject);
                    arguments.Insert(0, Expression.Field(GetLimitedSelf(), RealmObjectRealmField));
                    setter = GetSetMethod<RealmObject>(dummyHandle.SetObject);
                    break;
            }

            if (property.IsNullable && argumentType.IsValueType)
            {
                argumentType = typeof(Nullable<>).MakeGenericType(argumentType);
            }

            var valueExpression = value.Expression;
            if (valueExpression.Type != argumentType)
            {
                valueExpression = Expression.Convert(valueExpression, argumentType);
            }

            arguments.Add(valueExpression);

            var expression = Expression.Block(Expression.Call(Expression.Field(GetLimitedSelf(), RealmObjectObjectHandleField), setter, arguments), Expression.Default(binder.ReturnType));

            var argumentShouldBeDynamicRealmObject = BindingRestrictions.GetTypeRestriction(Expression, typeof(DynamicRealmObject));
            var argumentShouldBeInTheSameRealm = BindingRestrictions.GetInstanceRestriction(Expression.Field(GetLimitedSelf(), RealmObjectRealmField), _realm);
            return new DynamicMetaObject(expression, argumentShouldBeDynamicRealmObject.Merge(argumentShouldBeInTheSameRealm));
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _metadata.Schema.PropertyNames;
        }

        private static Expression WeakConstant<T>(T value) where T : class
        {
            var weakReference = new WeakReference(value);
            var constant = Expression.Constant(weakReference);
            var property = Expression.Property(constant, nameof(weakReference.Target));
            return Expression.Convert(property, typeof(T));
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

        private static MethodInfo GetGetMethod<TResult>(Func<IntPtr, TResult> @delegate) => @delegate.Method;

        private static MethodInfo GetSetMethod<TValue>(Action<IntPtr, TValue> @delegate) => @delegate.Method;

        private static MethodInfo GetGetMethod<TResult>(Func<Realm, IntPtr, string, TResult> @delegate) => @delegate.Method;

        private static MethodInfo GetSetMethod<TValue>(Action<Realm, IntPtr, TValue> @delegate) => @delegate.Method;
    }
}