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
        private readonly Realm _realm;
        private readonly RealmObject.Metadata _metadata;

        private static readonly FieldInfo RealmObjectRealmField = typeof(RealmObject).GetTypeInfo().GetField("_realm", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo RealmObjectRowHandleField = typeof(RealmObject).GetTypeInfo().GetField("_rowHandle", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly PropertyInfo RowHandleRowIndexProperty = typeof(RowHandle).GetTypeInfo().GetProperty("RowIndex", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

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
                WeakConstant(_metadata.Table),
                Expression.Constant(_metadata.ColumnIndices[property.Name]),
                Expression.Property(Expression.Field(GetLimitedSelf(), RealmObjectRowHandleField), RowHandleRowIndexProperty),
            };

            MethodInfo getter = null;

            switch (property.Type)
            {
                case Schema.PropertyType.Int:
                    if (property.IsNullable)
                        getter = GetGetMethod(NativeTable.GetNullableInt64);
                    else
                        getter = GetGetMethod(NativeTable.GetInt64);
                    break;
                case Schema.PropertyType.Bool:
                    if (property.IsNullable)
                        getter = GetGetMethod(NativeTable.GetNullableBoolean);
                    else
                        getter = GetGetMethod(NativeTable.GetBoolean);
                    break;
                case Schema.PropertyType.Float:
                    if (property.IsNullable)
                        getter = GetGetMethod(NativeTable.GetNullableSingle);
                    else
                        getter = GetGetMethod(NativeTable.GetSingle);
                    break;
                case Schema.PropertyType.Double:
                    if (property.IsNullable)
                        getter = GetGetMethod(NativeTable.GetNullableDouble);
                    else
                        getter = GetGetMethod(NativeTable.GetDouble);
                    break;
                case Schema.PropertyType.String:
                    getter = GetGetMethod(NativeTable.GetString);
                    break;
                case Schema.PropertyType.Data:
                    getter = GetGetMethod(NativeTable.GetByteArray);
                    break;
                case Schema.PropertyType.Date:
                    if (property.IsNullable)
                        getter = GetGetMethod(NativeTable.GetNullableDateTimeOffset);
                    else
                        getter = GetGetMethod(NativeTable.GetDateTimeOffset);
                    break;
                case Schema.PropertyType.Object:
                    arguments.Insert(0, Expression.Field(GetLimitedSelf(), RealmObjectRealmField));
                    arguments.Add(Expression.Constant(property.ObjectType));
                    getter = GetGetMethod(RealmObjectOps.GetObject<DynamicRealmObject>);
                    break;
                case Schema.PropertyType.Array:
                    arguments.Insert(0, Expression.Field(GetLimitedSelf(), RealmObjectRealmField));
                    arguments.Add(Expression.Constant(property.ObjectType));
                    getter = GetGetMethod(RealmObjectOps.GetList<DynamicRealmObject>);
                    break;
            }

            Expression expression = Expression.Call(getter, arguments);
            if (binder.ReturnType != expression.Type)
            {
                expression = Expression.Convert(expression, binder.ReturnType);
            }

            var argumentShouldBeDynamicRealmObject = BindingRestrictions.GetTypeRestriction(Expression, typeof(DynamicRealmObject));
            var argumentShouldBeInTheSameRealm = BindingRestrictions.GetInstanceRestriction(Expression.Field(GetLimitedSelf(), RealmObjectRealmField), _realm);
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
                WeakConstant(_metadata.Table),
                Expression.Constant(_metadata.ColumnIndices[property.Name]),
                Expression.Property(Expression.Field(GetLimitedSelf(), RealmObjectRowHandleField), RowHandleRowIndexProperty),
            };

            MethodInfo setter = null;
            Type argumentType = null;

            switch (property.Type)
            {
                case Schema.PropertyType.Int:
                    argumentType = typeof(long);
                    if (property.IsNullable)
                        setter = GetSetMethod<long?>(NativeTable.SetNullableInt64);
                    else if (property.IsObjectId)
                        setter = GetSetMethod<long>(NativeTable.SetInt64Unique);
                    else
                        setter = GetSetMethod<long>(NativeTable.SetInt64);
                    break;
                case Schema.PropertyType.Bool:
                    argumentType = typeof(bool);
                    if (property.IsNullable)
                        setter = GetSetMethod<bool?>(NativeTable.SetNullableBoolean);
                    else
                        setter = GetSetMethod<bool>(NativeTable.SetBoolean);
                    break;
                case Schema.PropertyType.Float:
                    argumentType = typeof(float);
                    if (property.IsNullable)
                        setter = GetSetMethod<float?>(NativeTable.SetNullableSingle);
                    else
                        setter = GetSetMethod<float>(NativeTable.SetSingle);
                    break;
                case Schema.PropertyType.Double:
                    argumentType = typeof(double);
                    if (property.IsNullable)
                        setter = GetSetMethod<double?>(NativeTable.SetNullableDouble);
                    else
                        setter = GetSetMethod<double>(NativeTable.SetDouble);
                    break;
                case Schema.PropertyType.String:
                    argumentType = typeof(string);
                    if (property.IsObjectId)
                        setter = GetSetMethod<string>(NativeTable.SetStringUnique);
                    else
                        setter = GetSetMethod<string>(NativeTable.SetString);
                    break;
                case Schema.PropertyType.Data:
                    argumentType = typeof(byte[]);
                    setter = GetSetMethod<byte[]>(NativeTable.SetByteArray);
                    break;
                case Schema.PropertyType.Date:
                    argumentType = typeof(DateTimeOffset);
                    if (property.IsNullable)
                        setter = GetSetMethod<DateTimeOffset?>(NativeTable.SetNullableDateTimeOffset);
                    else
                        setter = GetSetMethod<DateTimeOffset>(NativeTable.SetDateTimeOffset);
                    break;
                case Schema.PropertyType.Object:
                    argumentType = typeof(RealmObject);
                    arguments.Insert(0, Expression.Field(GetLimitedSelf(), RealmObjectRealmField));
                    setter = GetSetMethod<RealmObject>(RealmObjectOps.SetObject);
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

            var expression = Expression.Block(Expression.Call(setter, arguments), Expression.Default(binder.ReturnType));

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

        private static MethodInfo GetGetMethod<TResult>(Func<TableHandle, IntPtr, IntPtr, TResult> @delegate) => @delegate.Method;

        private static MethodInfo GetGetMethod<TResult>(Func<Realm, TableHandle, IntPtr, IntPtr, string, TResult> @delegate) => @delegate.Method;

        private static MethodInfo GetSetMethod<TValue>(Action<TableHandle, IntPtr, IntPtr, TValue> @delegate) => @delegate.Method;

        private static MethodInfo GetSetMethod<TValue>(Action<Realm, TableHandle, IntPtr, IntPtr, TValue> @delegate) => @delegate.Method;
    }
}

