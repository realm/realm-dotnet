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
        private readonly RealmObject.Metadata _metadata;

        private static readonly FieldInfo RealmObjectRealmField = typeof(RealmObject).GetField("_realm", PrivateBindingFlags);
        private static readonly FieldInfo RealmObjectObjectHandleField = typeof(RealmObject).GetField("_objectHandle", PrivateBindingFlags);
        private static readonly MethodInfo RealmObjectGetBacklinksForHandleMethod = typeof(RealmObject).GetMethod("GetBacklinksForHandle", PrivateBindingFlags)
                                                                                              .MakeGenericMethod(typeof(DynamicRealmObject));

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

            var arguments = new List<Expression>
            {
                Expression.Constant(_metadata.ColumnKeys[property.Name])
            };

            MethodInfo getter = null;
            if (property.Type.IsArray())
            {
                arguments.Insert(0, Expression.Field(GetLimitedSelf(), RealmObjectRealmField));
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
                    case Schema.PropertyType.Float:
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
                    case PropertyType.LinkingObjects:
                        // ObjectHandle.GetBacklinks has only one argument.
                        arguments.Clear();
                        arguments.Add(Expression.Constant(_metadata.ColumnKeys[property.Name]));
                        getter = GetGetMethod(DummyHandle.GetBacklinks);
                        break;
                }
            }
            else
            {
                switch (property.Type.UnderlyingType())
                {
                    case PropertyType.Int:
                        if (property.Type.IsNullable())
                        {
                            getter = GetGetMethod(DummyHandle.GetNullableInt64);
                        }
                        else
                        {
                            getter = GetGetMethod(DummyHandle.GetInt64);
                        }

                        break;
                    case PropertyType.Bool:
                        if (property.Type.IsNullable())
                        {
                            getter = GetGetMethod(DummyHandle.GetNullableBoolean);
                        }
                        else
                        {
                            getter = GetGetMethod(DummyHandle.GetBoolean);
                        }

                        break;
                    case Schema.PropertyType.Float:
                        if (property.Type.IsNullable())
                        {
                            getter = GetGetMethod(DummyHandle.GetNullableSingle);
                        }
                        else
                        {
                            getter = GetGetMethod(DummyHandle.GetSingle);
                        }

                        break;
                    case PropertyType.Double:
                        if (property.Type.IsNullable())
                        {
                            getter = GetGetMethod(DummyHandle.GetNullableDouble);
                        }
                        else
                        {
                            getter = GetGetMethod(DummyHandle.GetDouble);
                        }

                        break;
                    case PropertyType.String:
                        getter = GetGetMethod(DummyHandle.GetString);
                        break;
                    case PropertyType.Data:
                        getter = GetGetMethod(DummyHandle.GetByteArray);
                        break;
                    case PropertyType.Date:
                        if (property.Type.IsNullable())
                        {
                            getter = GetGetMethod(DummyHandle.GetNullableDateTimeOffset);
                        }
                        else
                        {
                            getter = GetGetMethod(DummyHandle.GetDateTimeOffset);
                        }

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

            switch (property.Type.UnderlyingType())
            {
                case PropertyType.Int:
                    argumentType = typeof(long);
                    if (property.Type.IsNullable())
                    {
                        setter = GetSetMethod<long?>(DummyHandle.SetNullableInt64);
                    }
                    else if (property.IsPrimaryKey)
                    {
                        setter = GetSetMethod<long>(DummyHandle.SetInt64Unique);
                    }
                    else
                    {
                        setter = GetSetMethod<long>(DummyHandle.SetInt64);
                    }

                    break;
                case PropertyType.Bool:
                    argumentType = typeof(bool);
                    if (property.Type.IsNullable())
                    {
                        setter = GetSetMethod<bool?>(DummyHandle.SetNullableBoolean);
                    }
                    else
                    {
                        setter = GetSetMethod<bool>(DummyHandle.SetBoolean);
                    }

                    break;
                case PropertyType.Float:
                    argumentType = typeof(float);
                    if (property.Type.IsNullable())
                    {
                        setter = GetSetMethod<float?>(DummyHandle.SetNullableSingle);
                    }
                    else
                    {
                        setter = GetSetMethod<float>(DummyHandle.SetSingle);
                    }

                    break;
                case PropertyType.Double:
                    argumentType = typeof(double);
                    if (property.Type.IsNullable())
                    {
                        setter = GetSetMethod<double?>(DummyHandle.SetNullableDouble);
                    }
                    else
                    {
                        setter = GetSetMethod<double>(DummyHandle.SetDouble);
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
                case PropertyType.Date:
                    argumentType = typeof(DateTimeOffset);
                    if (property.Type.IsNullable())
                    {
                        setter = GetSetMethod<DateTimeOffset?>(DummyHandle.SetNullableDateTimeOffset);
                    }
                    else
                    {
                        setter = GetSetMethod<DateTimeOffset>(DummyHandle.SetDateTimeOffset);
                    }

                    break;
                case PropertyType.Object:
                    argumentType = typeof(RealmObject);
                    arguments.Insert(0, Expression.Field(GetLimitedSelf(), RealmObjectRealmField));
                    setter = GetSetMethod<RealmObject>(DummyHandle.SetObject);
                    break;
            }

            if (property.Type.IsNullable() && argumentType.GetTypeInfo().IsValueType)
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

        private Expression GetLimitedSelf()
        {
            var convertedExpression = Expression;
            if (convertedExpression.Type != LimitType)
            {
                convertedExpression = Expression.Convert(convertedExpression, LimitType);
            }

            return convertedExpression;
        }

        private static MethodInfo GetGetMethod<TResult>(Func<ColumnKey, TResult> @delegate) => @delegate.GetMethodInfo();

        private static MethodInfo GetGetMethod<TResult>(Func<IntPtr, TResult> @delegate) => @delegate.GetMethodInfo();

        private static MethodInfo GetSetMethod<TValue>(Action<ColumnKey, TValue> @delegate) => @delegate.GetMethodInfo();

        private static MethodInfo GetGetMethod<TResult>(Func<Realm, ColumnKey, string, TResult> @delegate) => @delegate.GetMethodInfo();

        private static MethodInfo GetSetMethod<TValue>(Action<Realm, ColumnKey, TValue> @delegate) => @delegate.GetMethodInfo();
    }
}