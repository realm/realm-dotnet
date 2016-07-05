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

        public MetaRealmObject(Expression expression, DynamicRealmObject value)
            : base(expression, BindingRestrictions.Empty, value)
        {
            _realm = value.Realm;
            _metadata = value.ObjectMetadata;
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            var property = _metadata.Schema.Find(binder.Name);
            if (property == null)
            {
                return base.BindGetMember(binder);
            }

            var arguments = new List<Expression>
            {
                WeakConstant(_metadata.Table),
                Expression.Field(GetLimitedSelf(), RealmObjectRowHandleField),
                Expression.Constant(_metadata.ColumnIndices[property.Name])
            };

            MethodInfo realmObjectOpsMethod = null;

            switch (property.Type)
            {
                case Schema.PropertyType.Int:
                    if (property.IsNullable)
                        realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.GetNullableInt64Value));
                    else
                        realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.GetInt64Value));
                    break;
                case Schema.PropertyType.Bool:
                    if (property.IsNullable)
                        realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.GetNullableBooleanValue));
                    else
                        realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.GetBooleanValue));
                    break;
                case Schema.PropertyType.Float:
                    if (property.IsNullable)
                        realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.GetNullableSingleValue));
                    else
                        realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.GetSingleValue));
                    break;
                case Schema.PropertyType.Double:
                    if (property.IsNullable)
                        realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.GetNullableDoubleValue));
                    else
                        realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.GetDoubleValue));
                    break;
                case Schema.PropertyType.String:
                    arguments.Insert(0, Expression.Field(GetLimitedSelf(), RealmObjectRealmField));
                    realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.GetStringValue));
                    break;
                case Schema.PropertyType.Data:
                    realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.GetByteArrayValue));
                    break;
                case Schema.PropertyType.Date:
                    if (property.IsNullable)
                        realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.GetNullableDateTimeOffsetValue));
                    else
                        realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.GetDateTimeOffsetValue));
                    break;
                case Schema.PropertyType.Object:
                    arguments.Insert(0, Expression.Field(GetLimitedSelf(), RealmObjectRealmField));
                    arguments.Add(Expression.Constant(property.ObjectType));
                    realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.GetObjectValue)).MakeGenericMethod(typeof(DynamicRealmObject));
                    break;
                case Schema.PropertyType.Array:
                    arguments.Insert(0, Expression.Field(GetLimitedSelf(), RealmObjectRealmField));
                    arguments.Add(Expression.Constant(property.ObjectType));
                    realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.GetListValue)).MakeGenericMethod(typeof(DynamicRealmObject));
                    break;
            }

            Expression expression = Expression.Call(realmObjectOpsMethod, arguments);
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
            var property = _metadata.Schema.Find(binder.Name);
            if (property == null)
            {
                return base.BindSetMember(binder, value);
            }

            var arguments = new List<Expression>
            {
                WeakConstant(_metadata.Table),
                Expression.Field(GetLimitedSelf(), RealmObjectRowHandleField),
                Expression.Constant(_metadata.ColumnIndices[property.Name]),
            };

            MethodInfo realmObjectOpsMethod = null;
            Type argumentType = null;

            switch (property.Type)
            {
                case Schema.PropertyType.Int:
                    argumentType = typeof(long);
                    if (property.IsNullable)
                        realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.SetNullableInt64Value));
                    else if (property.IsObjectId)
                        realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.SetInt64ValueUnique));
                    else
                        realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.SetInt64Value));
                    break;
                case Schema.PropertyType.Bool:
                    argumentType = typeof(bool);
                    if (property.IsNullable)
                        realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.SetNullableBooleanValue));
                    else
                        realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.SetBooleanValue));
                    break;
                case Schema.PropertyType.Float:
                    argumentType = typeof(float);
                    if (property.IsNullable)
                        realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.SetNullableSingleValue));
                    else
                        realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.SetSingleValue));
                    break;
                case Schema.PropertyType.Double:
                    argumentType = typeof(double);
                    if (property.IsNullable)
                        realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.SetNullableDoubleValue));
                    else
                        realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.SetDoubleValue));
                    break;
                case Schema.PropertyType.String:
                    argumentType = typeof(string);
                    if (property.IsObjectId)
                        realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.SetStringValueUnique));
                    else
                        realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.SetStringValue));
                    break;
                case Schema.PropertyType.Data:
                    argumentType = typeof(byte[]);
                    realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.SetByteArrayValue));
                    break;
                case Schema.PropertyType.Date:
                    argumentType = typeof(DateTimeOffset);
                    if (property.IsNullable)
                        realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.SetNullableDateTimeOffsetValue));
                    else
                        realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.SetDateTimeOffsetValue));
                    break;
                case Schema.PropertyType.Object:
                    argumentType = typeof(RealmObject);
                    arguments.Insert(0, Expression.Field(GetLimitedSelf(), RealmObjectRealmField));
                    realmObjectOpsMethod = typeof(RealmObjectOps).GetMethod(nameof(RealmObjectOps.SetObjectValue)).MakeGenericMethod(typeof(RealmObject));
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

            var expression = Expression.Block(Expression.Call(realmObjectOpsMethod, arguments), Expression.Default(binder.ReturnType));

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

