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

            MethodInfo NativeTableMethod = null;

            switch (property.Type)
            {
                case Schema.PropertyType.Int:
                    if (property.IsNullable)
                        NativeTableMethod = typeof(NativeTable).GetMethod(nameof(NativeTable.GetNullableInt64));
                    else
                        NativeTableMethod = typeof(NativeTable).GetMethod(nameof(NativeTable.GetInt64));
                    break;
                case Schema.PropertyType.Bool:
                    if (property.IsNullable)
                        NativeTableMethod = typeof(NativeTable).GetMethod(nameof(NativeTable.GetNullableBoolean));
                    else
                        NativeTableMethod = typeof(NativeTable).GetMethod(nameof(NativeTable.GetBoolean));
                    break;
                case Schema.PropertyType.Float:
                    if (property.IsNullable)
                        NativeTableMethod = typeof(NativeTable).GetMethod(nameof(NativeTable.GetNullableSingle));
                    else
                        NativeTableMethod = typeof(NativeTable).GetMethod(nameof(NativeTable.GetSingle));
                    break;
                case Schema.PropertyType.Double:
                    if (property.IsNullable)
                        NativeTableMethod = typeof(NativeTable).GetMethod(nameof(NativeTable.GetNullableDouble));
                    else
                        NativeTableMethod = typeof(NativeTable).GetMethod(nameof(NativeTable.GetDouble));
                    break;
                case Schema.PropertyType.String:
                    NativeTableMethod = typeof(NativeTable).GetMethod(nameof(NativeTable.GetString));
                    break;
                case Schema.PropertyType.Data:
                    NativeTableMethod = typeof(NativeTable).GetMethod(nameof(NativeTable.GetByteArray));
                    break;
                case Schema.PropertyType.Date:
                    if (property.IsNullable)
                        NativeTableMethod = typeof(NativeTable).GetMethod(nameof(NativeTable.GetNullableDateTimeOffset));
                    else
                        NativeTableMethod = typeof(NativeTable).GetMethod(nameof(NativeTable.GetDateTimeOffset));
                    break;
                case Schema.PropertyType.Object:
                    arguments.Insert(0, Expression.Field(GetLimitedSelf(), RealmObjectRealmField));
                    arguments.Add(Expression.Constant(property.ObjectType));
                    NativeTableMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.GetObject)).MakeGenericMethod(typeof(DynamicRealmObject));
                    break;
                case Schema.PropertyType.Array:
                    arguments.Insert(0, Expression.Field(GetLimitedSelf(), RealmObjectRealmField));
                    arguments.Add(Expression.Constant(property.ObjectType));
                    NativeTableMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.GetList)).MakeGenericMethod(typeof(DynamicRealmObject));
                    break;
            }

            Expression expression = Expression.Call(NativeTableMethod, arguments);
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

            MethodInfo NativeTableMethod = null;
            Type argumentType = null;

            switch (property.Type)
            {
                case Schema.PropertyType.Int:
                    argumentType = typeof(long);
                    if (property.IsNullable)
                        NativeTableMethod = typeof(NativeTable).GetMethod(nameof(NativeTable.SetNullableInt64));
                    else if (property.IsObjectId)
                        NativeTableMethod = typeof(NativeTable).GetMethod(nameof(NativeTable.SetInt64Unique));
                    else
                        NativeTableMethod = typeof(NativeTable).GetMethod(nameof(NativeTable.SetInt64));
                    break;
                case Schema.PropertyType.Bool:
                    argumentType = typeof(bool);
                    if (property.IsNullable)
                        NativeTableMethod = typeof(NativeTable).GetMethod(nameof(NativeTable.SetNullableBoolean));
                    else
                        NativeTableMethod = typeof(NativeTable).GetMethod(nameof(NativeTable.SetBoolean));
                    break;
                case Schema.PropertyType.Float:
                    argumentType = typeof(float);
                    if (property.IsNullable)
                        NativeTableMethod = typeof(NativeTable).GetMethod(nameof(NativeTable.SetNullableSingle));
                    else
                        NativeTableMethod = typeof(NativeTable).GetMethod(nameof(NativeTable.SetSingle));
                    break;
                case Schema.PropertyType.Double:
                    argumentType = typeof(double);
                    if (property.IsNullable)
                        NativeTableMethod = typeof(NativeTable).GetMethod(nameof(NativeTable.SetNullableDouble));
                    else
                        NativeTableMethod = typeof(NativeTable).GetMethod(nameof(NativeTable.SetDouble));
                    break;
                case Schema.PropertyType.String:
                    argumentType = typeof(string);
                    if (property.IsObjectId)
                        NativeTableMethod = typeof(NativeTable).GetMethod(nameof(NativeTable.SetStringUnique));
                    else
                        NativeTableMethod = typeof(NativeTable).GetMethod(nameof(NativeTable.SetString));
                    break;
                case Schema.PropertyType.Data:
                    argumentType = typeof(byte[]);
                    NativeTableMethod = typeof(NativeTable).GetMethod(nameof(NativeTable.SetByteArray));
                    break;
                case Schema.PropertyType.Date:
                    argumentType = typeof(DateTimeOffset);
                    if (property.IsNullable)
                        NativeTableMethod = typeof(NativeTable).GetMethod(nameof(NativeTable.SetNullableDateTimeOffset));
                    else
                        NativeTableMethod = typeof(NativeTable).GetMethod(nameof(NativeTable.SetDateTimeOffset));
                    break;
                case Schema.PropertyType.Object:
                    argumentType = typeof(RealmObject);
                    arguments.Insert(0, Expression.Field(GetLimitedSelf(), RealmObjectRealmField));
                    NativeTableMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.SetObject)).MakeGenericMethod(typeof(RealmObject));
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

            var expression = Expression.Block(Expression.Call(NativeTableMethod, arguments), Expression.Default(binder.ReturnType));

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
    }
}

