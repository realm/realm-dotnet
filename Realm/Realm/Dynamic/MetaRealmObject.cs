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
using Realms.Native;
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
        private static readonly MethodInfo RealmObjectGetBacklinksForHandleMethod = typeof(RealmObjectBase).GetMethod("GetBacklinksForHandle", PrivateBindingFlags)
                                                                                              .MakeGenericMethod(typeof(DynamicRealmObject));

        private static readonly MethodInfo PrimitiveValueGetMethod = typeof(PrimitiveValue).GetMethod(nameof(PrimitiveValue.Get), BindingFlags.Public | BindingFlags.Instance);
        private static readonly MethodInfo CreatePrimitiveMethod = typeof(PrimitiveValue).GetMethod(nameof(PrimitiveValue.Create), BindingFlags.Public | BindingFlags.Static);

        private static readonly ObjectHandle DummyHandle = new ObjectHandle(null, IntPtr.Zero);

        public MetaRealmObject(Expression expression, DynamicRealmObject value)
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

            var arguments = new List<Expression>();
            MethodInfo getter = null;
            if (property.Type.UnderlyingType() == PropertyType.LinkingObjects)
            {
                arguments.Add(Expression.Constant(_metadata.ComputedProperties[property.Name]));
                getter = GetGetMethod(DummyHandle.GetBacklinks);
            }
            else if (property.Type.IsArray())
            {
                arguments.Add(Expression.Field(GetLimitedSelf(), RealmObjectRealmField));
                arguments.Add(Expression.Constant(_metadata.ColumnKeys[property.Name]));
                arguments.Add(Expression.Constant(property.ObjectType, typeof(string)));
                switch (property.Type.UnderlyingType())
                {
                    case PropertyType.Int:
                        if (property.Type.IsNullable())
                        {
                            getter = GetGetMethod(DummyHandle.GetList<long?>);
                        }
                        else
                        {
                            getter = GetGetMethod(DummyHandle.GetList<long>);
                        }

                        break;
                    case PropertyType.Bool:
                        if (property.Type.IsNullable())
                        {
                            getter = GetGetMethod(DummyHandle.GetList<bool?>);
                        }
                        else
                        {
                            getter = GetGetMethod(DummyHandle.GetList<bool>);
                        }

                        break;
                    case PropertyType.Float:
                        if (property.Type.IsNullable())
                        {
                            getter = GetGetMethod(DummyHandle.GetList<float?>);
                        }
                        else
                        {
                            getter = GetGetMethod(DummyHandle.GetList<float>);
                        }

                        break;
                    case PropertyType.Double:
                        if (property.Type.IsNullable())
                        {
                            getter = GetGetMethod(DummyHandle.GetList<double?>);
                        }
                        else
                        {
                            getter = GetGetMethod(DummyHandle.GetList<double>);
                        }

                        break;
                    case PropertyType.String:
                        getter = GetGetMethod(DummyHandle.GetList<string>);
                        break;
                    case PropertyType.Data:
                        getter = GetGetMethod(DummyHandle.GetList<byte[]>);
                        break;
                    case PropertyType.Date:
                        if (property.Type.IsNullable())
                        {
                            getter = GetGetMethod(DummyHandle.GetList<DateTimeOffset?>);
                        }
                        else
                        {
                            getter = GetGetMethod(DummyHandle.GetList<DateTimeOffset>);
                        }

                        break;
                    case PropertyType.Object:
                        getter = GetGetMethod(DummyHandle.GetList<DynamicRealmObject>);
                        break;
                }
            }
            else
            {
                arguments.Add(Expression.Constant(_metadata.ColumnKeys[property.Name]));
                switch (property.Type.UnderlyingType())
                {
                    case PropertyType.Int:
                    case PropertyType.Bool:
                    case PropertyType.Float:
                    case PropertyType.Double:
                    case PropertyType.Date:
                    case PropertyType.Decimal:
                    case PropertyType.ObjectId:
                        arguments.Add(Expression.Constant(property.Type));
                        getter = GetGetMethod(DummyHandle.GetPrimitive);
                        break;
                    case PropertyType.String:
                        getter = GetGetMethod(DummyHandle.GetString);
                        break;
                    case PropertyType.Data:
                        getter = GetGetMethod(DummyHandle.GetByteArray);
                        break;
                    case PropertyType.Object:
                        arguments.Insert(0, Expression.Field(GetLimitedSelf(), RealmObjectRealmField));
                        arguments.Add(Expression.Constant(property.ObjectType));
                        getter = GetGetMethod(DummyHandle.GetObject<DynamicRealmObject>);
                        break;
                }
            }

            var self = GetLimitedSelf();
            var instance = Expression.Field(self, RealmObjectObjectHandleField);
            Expression expression = Expression.Call(instance, getter, arguments);

            if (property.Type.UnderlyingType() == PropertyType.LinkingObjects)
            {
                expression = Expression.Call(self, RealmObjectGetBacklinksForHandleMethod, Expression.Constant(binder.Name), expression);
            }

            if (expression.Type == typeof(PrimitiveValue))
            {
                expression = Expression.Call(expression, PrimitiveValueGetMethod.MakeGenericMethod(property.PropertyInfo.PropertyType));
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
            if (!_metadata.Schema.TryFindProperty(binder.Name, out var property) || property.Type.IsArray())
            {
                return base.BindSetMember(binder, value);
            }

            var arguments = new List<Expression>
            {
                Expression.Constant(_metadata.ColumnKeys[property.Name])
            };

            MethodInfo setter = null;
            Type argumentType = null;

            var valueExpression = value.Expression;
            switch (property.Type.UnderlyingType())
            {
                // V10TODO: split these into individual cases to avoid calling the generic method
                case PropertyType.Int:
                case PropertyType.Bool:
                case PropertyType.Float:
                case PropertyType.Double:
                case PropertyType.Date:
                case PropertyType.Decimal:
                case PropertyType.ObjectId:
                    argumentType = typeof(PrimitiveValue);
                    valueExpression = Expression.Call(CreatePrimitiveMethod.MakeGenericMethod(valueExpression.Type), new[] { valueExpression, Expression.Constant(property.Type) });
                    if (property.IsPrimaryKey)
                    {
                        setter = GetSetMethod<PrimitiveValue>(DummyHandle.SetPrimitiveUnique);
                    }
                    else
                    {
                        setter = GetSetMethod<PrimitiveValue>(DummyHandle.SetPrimitive);
                    }

                    break;
                case PropertyType.String:
                    argumentType = typeof(string);
                    if (property.IsPrimaryKey)
                    {
                        setter = GetSetMethod<string>(DummyHandle.SetStringUnique);
                    }
                    else
                    {
                        setter = GetSetMethod<string>(DummyHandle.SetString);
                    }

                    break;
                case PropertyType.Data:
                    argumentType = typeof(byte[]);
                    setter = GetSetMethod<byte[]>(DummyHandle.SetByteArray);
                    break;
                case PropertyType.Object:
                    argumentType = typeof(RealmObjectBase);
                    arguments.Insert(0, Expression.Constant(GetLimitedSelf()));
                    setter = GetSetMethod<RealmObjectBase>(DummyHandle.SetObject);
                    break;
            }

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

        private Expression GetLimitedSelf()
        {
            var convertedExpression = Expression;
            if (convertedExpression.Type != LimitType)
            {
                convertedExpression = Expression.Convert(convertedExpression, LimitType);
            }

            return convertedExpression;
        }

        // GetString(colKey)
        // GetByteArray(colKey)
        private static MethodInfo GetGetMethod<TResult>(Func<ColumnKey, TResult> @delegate) => @delegate.GetMethodInfo();

        // GetPrimitive(colKey, propertyType)
        private static MethodInfo GetGetMethod<TResult>(Func<ColumnKey, PropertyType, TResult> @delegate) => @delegate.GetMethodInfo();

        // GetBacklinks(propertyIndex)
        private static MethodInfo GetGetMethod<TResult>(Func<IntPtr, TResult> @delegate) => @delegate.GetMethodInfo();

        // GetList(realm, colKey, objectType)
        // GetObject(realm, colKey, objectType)
        private static MethodInfo GetGetMethod<TResult>(Func<Realm, ColumnKey, string, TResult> @delegate) => @delegate.GetMethodInfo();

        // SetXXX(colKey)
        private static MethodInfo GetSetMethod<TValue>(Action<ColumnKey, TValue> @delegate) => @delegate.GetMethodInfo();

        // SetObject(this, colKey)
        private static MethodInfo GetSetMethod<TValue>(Action<RealmObjectBase, ColumnKey, TValue> @delegate) => @delegate.GetMethodInfo();
    }
}