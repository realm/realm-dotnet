﻿////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
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
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using MongoDB.Bson;
using Realms;
using Realms.Schema;

namespace SourceGeneratorAssemblyToProcess
{
    [Ignored]
    public partial class AllTypesClass2 : IRealmObject
    {
        public char CharProperty { get; set; }

        public byte ByteProperty { get; set; }

        public short Int16Property { get; set; }

        public int Int32Property { get; set; }

        public long Int64Property { get; set; }

        public float SingleProperty { get; set; }

        public double DoubleProperty { get; set; }

        public bool BooleanProperty { get; set; }

        public DateTimeOffset DateTimeOffsetProperty { get; set; }

        public decimal DecimalProperty { get; set; }

        public Decimal128 Decimal128Property { get; set; }

        public ObjectId ObjectIdProperty { get; set; }

        public Guid GuidProperty { get; set; }

        [Required]
        public string RequiredStringProperty { get; set; }

        public string StringProperty { get; set; }

        [Required]
        public byte[] RequiredByteArrayProperty { get; set; }

        public byte[] ByteArrayProperty { get; set; }

        public char? NullableCharProperty { get; set; }

        public byte? NullableByteProperty { get; set; }

        public short? NullableInt16Property { get; set; }

        public int? NullableInt32Property { get; set; }

        public long? NullableInt64Property { get; set; }

        public float? NullableSingleProperty { get; set; }

        public double? NullableDoubleProperty { get; set; }

        public bool? NullableBooleanProperty { get; set; }

        public DateTimeOffset? NullableDateTimeOffsetProperty { get; set; }

        public decimal? NullableDecimalProperty { get; set; }

        public Decimal128? NullableDecimal128Property { get; set; }

        public ObjectId? NullableObjectIdProperty { get; set; }

        public Guid? NullableGuidProperty { get; set; }

        public RealmInteger<byte> ByteCounterProperty { get; set; }

        public RealmInteger<short> Int16CounterProperty { get; set; }

        public RealmInteger<int> Int32CounterProperty { get; set; }

        public RealmInteger<long> Int64CounterProperty { get; set; }

        public RealmValue RealmValueProperty { get; set; }
    }

#nullable enable

    [Generated]
    [Woven(typeof(AllTypesClass2ObjectHelper))]
    public partial class AllTypesClass2 : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("AllTypesClass2", ObjectSchema.ObjectType.RealmObject)
        {
            Realms.Schema.Property.Primitive("CharProperty", Realms.RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "CharProperty"),
            Realms.Schema.Property.Primitive("ByteProperty", Realms.RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "ByteProperty"),
            Realms.Schema.Property.Primitive("Int16Property", Realms.RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "Int16Property"),
            Realms.Schema.Property.Primitive("Int32Property", Realms.RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "Int32Property"),
            Realms.Schema.Property.Primitive("Int64Property", Realms.RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "Int64Property"),
            Realms.Schema.Property.Primitive("SingleProperty", Realms.RealmValueType.Float, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "SingleProperty"),
            Realms.Schema.Property.Primitive("DoubleProperty", Realms.RealmValueType.Double, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "DoubleProperty"),
            Realms.Schema.Property.Primitive("BooleanProperty", Realms.RealmValueType.Bool, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "BooleanProperty"),
            Realms.Schema.Property.Primitive("DateTimeOffsetProperty", Realms.RealmValueType.Date, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "DateTimeOffsetProperty"),
            Realms.Schema.Property.Primitive("DecimalProperty", Realms.RealmValueType.Decimal128, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "DecimalProperty"),
            Realms.Schema.Property.Primitive("Decimal128Property", Realms.RealmValueType.Decimal128, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "Decimal128Property"),
            Realms.Schema.Property.Primitive("ObjectIdProperty", Realms.RealmValueType.ObjectId, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "ObjectIdProperty"),
            Realms.Schema.Property.Primitive("GuidProperty", Realms.RealmValueType.Guid, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "GuidProperty"),
            Realms.Schema.Property.Primitive("RequiredStringProperty", Realms.RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "RequiredStringProperty"),
            Realms.Schema.Property.Primitive("StringProperty", Realms.RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "StringProperty"),
            Realms.Schema.Property.Primitive("RequiredByteArrayProperty", Realms.RealmValueType.Data, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "RequiredByteArrayProperty"),
            Realms.Schema.Property.Primitive("ByteArrayProperty", Realms.RealmValueType.Data, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "ByteArrayProperty"),
            Realms.Schema.Property.Primitive("NullableCharProperty", Realms.RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "NullableCharProperty"),
            Realms.Schema.Property.Primitive("NullableByteProperty", Realms.RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "NullableByteProperty"),
            Realms.Schema.Property.Primitive("NullableInt16Property", Realms.RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "NullableInt16Property"),
            Realms.Schema.Property.Primitive("NullableInt32Property", Realms.RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "NullableInt32Property"),
            Realms.Schema.Property.Primitive("NullableInt64Property", Realms.RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "NullableInt64Property"),
            Realms.Schema.Property.Primitive("NullableSingleProperty", Realms.RealmValueType.Float, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "NullableSingleProperty"),
            Realms.Schema.Property.Primitive("NullableDoubleProperty", Realms.RealmValueType.Double, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "NullableDoubleProperty"),
            Realms.Schema.Property.Primitive("NullableBooleanProperty", Realms.RealmValueType.Bool, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "NullableBooleanProperty"),
            Realms.Schema.Property.Primitive("NullableDateTimeOffsetProperty", Realms.RealmValueType.Date, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "NullableDateTimeOffsetProperty"),
            Realms.Schema.Property.Primitive("NullableDecimalProperty", Realms.RealmValueType.Decimal128, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "NullableDecimalProperty"),
            Realms.Schema.Property.Primitive("NullableDecimal128Property", Realms.RealmValueType.Decimal128, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "NullableDecimal128Property"),
            Realms.Schema.Property.Primitive("NullableObjectIdProperty", Realms.RealmValueType.ObjectId, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "NullableObjectIdProperty"),
            Realms.Schema.Property.Primitive("NullableGuidProperty", Realms.RealmValueType.Guid, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "NullableGuidProperty"),
            Realms.Schema.Property.Primitive("ByteCounterProperty", Realms.RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "ByteCounterProperty"),
            Realms.Schema.Property.Primitive("Int16CounterProperty", Realms.RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "Int16CounterProperty"),
            Realms.Schema.Property.Primitive("Int32CounterProperty", Realms.RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "Int32CounterProperty"),
            Realms.Schema.Property.Primitive("Int64CounterProperty", Realms.RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "Int64CounterProperty"),
            Realms.Schema.Property.RealmValue("RealmValueProperty", managedName: "RealmValueProperty"),
        }.Build();

        #region IRealmObject implementation

        private IAllTypesClass2Accessor _accessor;

        Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

        internal IAllTypesClass2Accessor Accessor => _accessor ?? (_accessor = new AllTypesClass2UnmanagedAccessor(typeof(AllTypesClass2)));

        [IgnoreDataMember, XmlIgnore]
        public bool IsManaged => Accessor.IsManaged;

        [IgnoreDataMember, XmlIgnore]
        public bool IsValid => Accessor.IsValid;

        [IgnoreDataMember, XmlIgnore]
        public bool IsFrozen => Accessor.IsFrozen;

        [IgnoreDataMember, XmlIgnore]
        public Realms.Realm Realm => Accessor.Realm;

        [IgnoreDataMember, XmlIgnore]
        public Realms.Schema.ObjectSchema ObjectSchema => Accessor.ObjectSchema;

        [IgnoreDataMember, XmlIgnore]
        public Realms.DynamicObjectApi DynamicApi => Accessor.DynamicApi;

        [IgnoreDataMember, XmlIgnore]
        public int BacklinksCount => Accessor.BacklinksCount;

        public void SetManagedAccessor(Realms.IRealmAccessor managedAccessor, Realms.Weaving.IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var newAccessor = (IAllTypesClass2Accessor)managedAccessor;
            var oldAccessor = (IAllTypesClass2Accessor)_accessor;
            _accessor = newAccessor;

            if (helper != null)
            {
                if (!skipDefaults || oldAccessor.CharProperty != default(char))
                {
                    newAccessor.CharProperty = oldAccessor.CharProperty;
                }
                if (!skipDefaults || oldAccessor.ByteProperty != default(byte))
                {
                    newAccessor.ByteProperty = oldAccessor.ByteProperty;
                }
                if (!skipDefaults || oldAccessor.Int16Property != default(short))
                {
                    newAccessor.Int16Property = oldAccessor.Int16Property;
                }
                if (!skipDefaults || oldAccessor.Int32Property != default(int))
                {
                    newAccessor.Int32Property = oldAccessor.Int32Property;
                }
                if (!skipDefaults || oldAccessor.Int64Property != default(long))
                {
                    newAccessor.Int64Property = oldAccessor.Int64Property;
                }
                if (!skipDefaults || oldAccessor.SingleProperty != default(float))
                {
                    newAccessor.SingleProperty = oldAccessor.SingleProperty;
                }
                if (!skipDefaults || oldAccessor.DoubleProperty != default(double))
                {
                    newAccessor.DoubleProperty = oldAccessor.DoubleProperty;
                }
                if (!skipDefaults || oldAccessor.BooleanProperty != default(bool))
                {
                    newAccessor.BooleanProperty = oldAccessor.BooleanProperty;
                }
                newAccessor.DateTimeOffsetProperty = oldAccessor.DateTimeOffsetProperty;
                newAccessor.DecimalProperty = oldAccessor.DecimalProperty;
                newAccessor.Decimal128Property = oldAccessor.Decimal128Property;
                newAccessor.ObjectIdProperty = oldAccessor.ObjectIdProperty;
                newAccessor.GuidProperty = oldAccessor.GuidProperty;
                newAccessor.RequiredStringProperty = oldAccessor.RequiredStringProperty;
                if (!skipDefaults || oldAccessor.StringProperty != default(string))
                {
                    newAccessor.StringProperty = oldAccessor.StringProperty;
                }
                newAccessor.RequiredByteArrayProperty = oldAccessor.RequiredByteArrayProperty;
                if (!skipDefaults || oldAccessor.ByteArrayProperty != default(byte[]))
                {
                    newAccessor.ByteArrayProperty = oldAccessor.ByteArrayProperty;
                }
                newAccessor.NullableCharProperty = oldAccessor.NullableCharProperty;
                newAccessor.NullableByteProperty = oldAccessor.NullableByteProperty;
                newAccessor.NullableInt16Property = oldAccessor.NullableInt16Property;
                newAccessor.NullableInt32Property = oldAccessor.NullableInt32Property;
                newAccessor.NullableInt64Property = oldAccessor.NullableInt64Property;
                newAccessor.NullableSingleProperty = oldAccessor.NullableSingleProperty;
                newAccessor.NullableDoubleProperty = oldAccessor.NullableDoubleProperty;
                newAccessor.NullableBooleanProperty = oldAccessor.NullableBooleanProperty;
                newAccessor.NullableDateTimeOffsetProperty = oldAccessor.NullableDateTimeOffsetProperty;
                newAccessor.NullableDecimalProperty = oldAccessor.NullableDecimalProperty;
                newAccessor.NullableDecimal128Property = oldAccessor.NullableDecimal128Property;
                newAccessor.NullableObjectIdProperty = oldAccessor.NullableObjectIdProperty;
                newAccessor.NullableGuidProperty = oldAccessor.NullableGuidProperty;
                newAccessor.ByteCounterProperty = oldAccessor.ByteCounterProperty;
                newAccessor.Int16CounterProperty = oldAccessor.Int16CounterProperty;
                newAccessor.Int32CounterProperty = oldAccessor.Int32CounterProperty;
                newAccessor.Int64CounterProperty = oldAccessor.Int64CounterProperty;
                newAccessor.RealmValueProperty = oldAccessor.RealmValueProperty;
            }

            if (_propertyChanged != null)
            {
                SubscribeForNotifications();
            }

            OnManaged();
        }

        #endregion

        /// <summary>
        /// Called when the object has been managed by a Realm.
        /// </summary>
        /// <remarks>
        /// This method will be called either when a managed object is materialized or when an unmanaged object has been
        /// added to the Realm. It can be useful for providing some initialization logic as when the constructor is invoked,
        /// it is not yet clear whether the object is managed or not.
        /// </remarks>
        partial void OnManaged();

        private event PropertyChangedEventHandler _propertyChanged;

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                if (_propertyChanged == null)
                {
                    SubscribeForNotifications();
                }

                _propertyChanged += value;
            }

            remove
            {
                _propertyChanged -= value;

                if (_propertyChanged == null)
                {
                    UnsubscribeFromNotifications();
                }
            }
        }

        /// <summary>
        /// Called when a property has changed on this class.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <remarks>
        /// For this method to be called, you need to have first subscribed to <see cref="PropertyChanged"/>.
        /// This can be used to react to changes to the current object, e.g. raising <see cref="PropertyChanged"/> for computed properties.
        /// </remarks>
        /// <example>
        /// <code>
        /// class MyClass : IRealmObject
        /// {
        ///     public int StatusCodeRaw { get; set; }
        ///     public StatusCodeEnum StatusCode => (StatusCodeEnum)StatusCodeRaw;
        ///     partial void OnPropertyChanged(string propertyName)
        ///     {
        ///         if (propertyName == nameof(StatusCodeRaw))
        ///         {
        ///             RaisePropertyChanged(nameof(StatusCode));
        ///         }
        ///     }
        /// }
        /// </code>
        /// Here, we have a computed property that depends on a persisted one. In order to notify any <see cref="PropertyChanged"/>
        /// subscribers that <c>StatusCode</c> has changed, we implement <see cref="OnPropertyChanged"/> and
        /// raise <see cref="PropertyChanged"/> manually by calling <see cref="RaisePropertyChanged"/>.
        /// </example>
        partial void OnPropertyChanged(string propertyName);

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            OnPropertyChanged(propertyName);
        }

        private void SubscribeForNotifications()
        {
            Accessor.SubscribeForNotifications(RaisePropertyChanged);
        }

        private void UnsubscribeFromNotifications()
        {
            Accessor.UnsubscribeFromNotifications();
        }

        public static explicit operator AllTypesClass2(Realms.RealmValue val) => val.AsRealmObject<AllTypesClass2>();

        public static implicit operator Realms.RealmValue(AllTypesClass2 val) => Realms.RealmValue.Object(val);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public TypeInfo GetTypeInfo() => Accessor.GetTypeInfo(this);

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is InvalidObject)
            {
                return !IsValid;
            }

            if (obj is not Realms.IRealmObjectBase iro)
            {
                return false;
            }

            return Accessor.Equals(iro.Accessor);
        }

        public override int GetHashCode() => IsManaged ? Accessor.GetHashCode() : base.GetHashCode();

        public override string ToString() => Accessor.ToString();

        [EditorBrowsable(EditorBrowsableState.Never)]
        private class AllTypesClass2ObjectHelper : Realms.Weaving.IRealmObjectHelper
        {
            public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public Realms.ManagedAccessor CreateAccessor() => new AllTypesClass2ManagedAccessor();

            public Realms.IRealmObjectBase CreateInstance() => new AllTypesClass2();

            public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out object value)
            {
                value = null;
                return false;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal interface IAllTypesClass2Accessor : Realms.IRealmAccessor
        {
            char CharProperty { get; set; }

            byte ByteProperty { get; set; }

            short Int16Property { get; set; }

            int Int32Property { get; set; }

            long Int64Property { get; set; }

            float SingleProperty { get; set; }

            double DoubleProperty { get; set; }

            bool BooleanProperty { get; set; }

            System.DateTimeOffset DateTimeOffsetProperty { get; set; }

            decimal DecimalProperty { get; set; }

            MongoDB.Bson.Decimal128 Decimal128Property { get; set; }

            MongoDB.Bson.ObjectId ObjectIdProperty { get; set; }

            System.Guid GuidProperty { get; set; }

            string RequiredStringProperty { get; set; }

            string StringProperty { get; set; }

            byte[] RequiredByteArrayProperty { get; set; }

            byte[] ByteArrayProperty { get; set; }

            char? NullableCharProperty { get; set; }

            byte? NullableByteProperty { get; set; }

            short? NullableInt16Property { get; set; }

            int? NullableInt32Property { get; set; }

            long? NullableInt64Property { get; set; }

            float? NullableSingleProperty { get; set; }

            double? NullableDoubleProperty { get; set; }

            bool? NullableBooleanProperty { get; set; }

            System.DateTimeOffset? NullableDateTimeOffsetProperty { get; set; }

            decimal? NullableDecimalProperty { get; set; }

            MongoDB.Bson.Decimal128? NullableDecimal128Property { get; set; }

            MongoDB.Bson.ObjectId? NullableObjectIdProperty { get; set; }

            System.Guid? NullableGuidProperty { get; set; }

            Realms.RealmInteger<byte> ByteCounterProperty { get; set; }

            Realms.RealmInteger<short> Int16CounterProperty { get; set; }

            Realms.RealmInteger<int> Int32CounterProperty { get; set; }

            Realms.RealmInteger<long> Int64CounterProperty { get; set; }

            Realms.RealmValue RealmValueProperty { get; set; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class AllTypesClass2ManagedAccessor : Realms.ManagedAccessor, IAllTypesClass2Accessor
        {
            public char CharProperty
            {
                get => (char)GetValue("CharProperty");
                set => SetValue("CharProperty", value);
            }

            public byte ByteProperty
            {
                get => (byte)GetValue("ByteProperty");
                set => SetValue("ByteProperty", value);
            }

            public short Int16Property
            {
                get => (short)GetValue("Int16Property");
                set => SetValue("Int16Property", value);
            }

            public int Int32Property
            {
                get => (int)GetValue("Int32Property");
                set => SetValue("Int32Property", value);
            }

            public long Int64Property
            {
                get => (long)GetValue("Int64Property");
                set => SetValue("Int64Property", value);
            }

            public float SingleProperty
            {
                get => (float)GetValue("SingleProperty");
                set => SetValue("SingleProperty", value);
            }

            public double DoubleProperty
            {
                get => (double)GetValue("DoubleProperty");
                set => SetValue("DoubleProperty", value);
            }

            public bool BooleanProperty
            {
                get => (bool)GetValue("BooleanProperty");
                set => SetValue("BooleanProperty", value);
            }

            public System.DateTimeOffset DateTimeOffsetProperty
            {
                get => (System.DateTimeOffset)GetValue("DateTimeOffsetProperty");
                set => SetValue("DateTimeOffsetProperty", value);
            }

            public decimal DecimalProperty
            {
                get => (decimal)GetValue("DecimalProperty");
                set => SetValue("DecimalProperty", value);
            }

            public MongoDB.Bson.Decimal128 Decimal128Property
            {
                get => (MongoDB.Bson.Decimal128)GetValue("Decimal128Property");
                set => SetValue("Decimal128Property", value);
            }

            public MongoDB.Bson.ObjectId ObjectIdProperty
            {
                get => (MongoDB.Bson.ObjectId)GetValue("ObjectIdProperty");
                set => SetValue("ObjectIdProperty", value);
            }

            public System.Guid GuidProperty
            {
                get => (System.Guid)GetValue("GuidProperty");
                set => SetValue("GuidProperty", value);
            }

            public string RequiredStringProperty
            {
                get => (string)GetValue("RequiredStringProperty");
                set => SetValue("RequiredStringProperty", value);
            }

            public string StringProperty
            {
                get => (string)GetValue("StringProperty");
                set => SetValue("StringProperty", value);
            }

            public byte[] RequiredByteArrayProperty
            {
                get => (byte[])GetValue("RequiredByteArrayProperty");
                set => SetValue("RequiredByteArrayProperty", value);
            }

            public byte[] ByteArrayProperty
            {
                get => (byte[])GetValue("ByteArrayProperty");
                set => SetValue("ByteArrayProperty", value);
            }

            public char? NullableCharProperty
            {
                get => (char?)GetValue("NullableCharProperty");
                set => SetValue("NullableCharProperty", value);
            }

            public byte? NullableByteProperty
            {
                get => (byte?)GetValue("NullableByteProperty");
                set => SetValue("NullableByteProperty", value);
            }

            public short? NullableInt16Property
            {
                get => (short?)GetValue("NullableInt16Property");
                set => SetValue("NullableInt16Property", value);
            }

            public int? NullableInt32Property
            {
                get => (int?)GetValue("NullableInt32Property");
                set => SetValue("NullableInt32Property", value);
            }

            public long? NullableInt64Property
            {
                get => (long?)GetValue("NullableInt64Property");
                set => SetValue("NullableInt64Property", value);
            }

            public float? NullableSingleProperty
            {
                get => (float?)GetValue("NullableSingleProperty");
                set => SetValue("NullableSingleProperty", value);
            }

            public double? NullableDoubleProperty
            {
                get => (double?)GetValue("NullableDoubleProperty");
                set => SetValue("NullableDoubleProperty", value);
            }

            public bool? NullableBooleanProperty
            {
                get => (bool?)GetValue("NullableBooleanProperty");
                set => SetValue("NullableBooleanProperty", value);
            }

            public System.DateTimeOffset? NullableDateTimeOffsetProperty
            {
                get => (System.DateTimeOffset?)GetValue("NullableDateTimeOffsetProperty");
                set => SetValue("NullableDateTimeOffsetProperty", value);
            }

            public decimal? NullableDecimalProperty
            {
                get => (decimal?)GetValue("NullableDecimalProperty");
                set => SetValue("NullableDecimalProperty", value);
            }

            public MongoDB.Bson.Decimal128? NullableDecimal128Property
            {
                get => (MongoDB.Bson.Decimal128?)GetValue("NullableDecimal128Property");
                set => SetValue("NullableDecimal128Property", value);
            }

            public MongoDB.Bson.ObjectId? NullableObjectIdProperty
            {
                get => (MongoDB.Bson.ObjectId?)GetValue("NullableObjectIdProperty");
                set => SetValue("NullableObjectIdProperty", value);
            }

            public System.Guid? NullableGuidProperty
            {
                get => (System.Guid?)GetValue("NullableGuidProperty");
                set => SetValue("NullableGuidProperty", value);
            }

            public Realms.RealmInteger<byte> ByteCounterProperty
            {
                get => (Realms.RealmInteger<byte>)GetValue("ByteCounterProperty");
                set => SetValue("ByteCounterProperty", value);
            }

            public Realms.RealmInteger<short> Int16CounterProperty
            {
                get => (Realms.RealmInteger<short>)GetValue("Int16CounterProperty");
                set => SetValue("Int16CounterProperty", value);
            }

            public Realms.RealmInteger<int> Int32CounterProperty
            {
                get => (Realms.RealmInteger<int>)GetValue("Int32CounterProperty");
                set => SetValue("Int32CounterProperty", value);
            }

            public Realms.RealmInteger<long> Int64CounterProperty
            {
                get => (Realms.RealmInteger<long>)GetValue("Int64CounterProperty");
                set => SetValue("Int64CounterProperty", value);
            }

            public Realms.RealmValue RealmValueProperty
            {
                get => (Realms.RealmValue)GetValue("RealmValueProperty");
                set => SetValue("RealmValueProperty", value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class AllTypesClass2UnmanagedAccessor : Realms.UnmanagedAccessor, IAllTypesClass2Accessor
        {
            public override ObjectSchema ObjectSchema => AllTypesClass2.RealmSchema;

            private char _charProperty;
            public char CharProperty
            {
                get => _charProperty;
                set
                {
                    _charProperty = value;
                    RaisePropertyChanged("CharProperty");
                }
            }

            private byte _byteProperty;
            public byte ByteProperty
            {
                get => _byteProperty;
                set
                {
                    _byteProperty = value;
                    RaisePropertyChanged("ByteProperty");
                }
            }

            private short _int16Property;
            public short Int16Property
            {
                get => _int16Property;
                set
                {
                    _int16Property = value;
                    RaisePropertyChanged("Int16Property");
                }
            }

            private int _int32Property;
            public int Int32Property
            {
                get => _int32Property;
                set
                {
                    _int32Property = value;
                    RaisePropertyChanged("Int32Property");
                }
            }

            private long _int64Property;
            public long Int64Property
            {
                get => _int64Property;
                set
                {
                    _int64Property = value;
                    RaisePropertyChanged("Int64Property");
                }
            }

            private float _singleProperty;
            public float SingleProperty
            {
                get => _singleProperty;
                set
                {
                    _singleProperty = value;
                    RaisePropertyChanged("SingleProperty");
                }
            }

            private double _doubleProperty;
            public double DoubleProperty
            {
                get => _doubleProperty;
                set
                {
                    _doubleProperty = value;
                    RaisePropertyChanged("DoubleProperty");
                }
            }

            private bool _booleanProperty;
            public bool BooleanProperty
            {
                get => _booleanProperty;
                set
                {
                    _booleanProperty = value;
                    RaisePropertyChanged("BooleanProperty");
                }
            }

            private System.DateTimeOffset _dateTimeOffsetProperty;
            public System.DateTimeOffset DateTimeOffsetProperty
            {
                get => _dateTimeOffsetProperty;
                set
                {
                    _dateTimeOffsetProperty = value;
                    RaisePropertyChanged("DateTimeOffsetProperty");
                }
            }

            private decimal _decimalProperty;
            public decimal DecimalProperty
            {
                get => _decimalProperty;
                set
                {
                    _decimalProperty = value;
                    RaisePropertyChanged("DecimalProperty");
                }
            }

            private MongoDB.Bson.Decimal128 _decimal128Property;
            public MongoDB.Bson.Decimal128 Decimal128Property
            {
                get => _decimal128Property;
                set
                {
                    _decimal128Property = value;
                    RaisePropertyChanged("Decimal128Property");
                }
            }

            private MongoDB.Bson.ObjectId _objectIdProperty;
            public MongoDB.Bson.ObjectId ObjectIdProperty
            {
                get => _objectIdProperty;
                set
                {
                    _objectIdProperty = value;
                    RaisePropertyChanged("ObjectIdProperty");
                }
            }

            private System.Guid _guidProperty;
            public System.Guid GuidProperty
            {
                get => _guidProperty;
                set
                {
                    _guidProperty = value;
                    RaisePropertyChanged("GuidProperty");
                }
            }

            private string _requiredStringProperty;
            public string RequiredStringProperty
            {
                get => _requiredStringProperty;
                set
                {
                    _requiredStringProperty = value;
                    RaisePropertyChanged("RequiredStringProperty");
                }
            }

            private string _stringProperty;
            public string StringProperty
            {
                get => _stringProperty;
                set
                {
                    _stringProperty = value;
                    RaisePropertyChanged("StringProperty");
                }
            }

            private byte[] _requiredByteArrayProperty;
            public byte[] RequiredByteArrayProperty
            {
                get => _requiredByteArrayProperty;
                set
                {
                    _requiredByteArrayProperty = value;
                    RaisePropertyChanged("RequiredByteArrayProperty");
                }
            }

            private byte[] _byteArrayProperty;
            public byte[] ByteArrayProperty
            {
                get => _byteArrayProperty;
                set
                {
                    _byteArrayProperty = value;
                    RaisePropertyChanged("ByteArrayProperty");
                }
            }

            private char? _nullableCharProperty;
            public char? NullableCharProperty
            {
                get => _nullableCharProperty;
                set
                {
                    _nullableCharProperty = value;
                    RaisePropertyChanged("NullableCharProperty");
                }
            }

            private byte? _nullableByteProperty;
            public byte? NullableByteProperty
            {
                get => _nullableByteProperty;
                set
                {
                    _nullableByteProperty = value;
                    RaisePropertyChanged("NullableByteProperty");
                }
            }

            private short? _nullableInt16Property;
            public short? NullableInt16Property
            {
                get => _nullableInt16Property;
                set
                {
                    _nullableInt16Property = value;
                    RaisePropertyChanged("NullableInt16Property");
                }
            }

            private int? _nullableInt32Property;
            public int? NullableInt32Property
            {
                get => _nullableInt32Property;
                set
                {
                    _nullableInt32Property = value;
                    RaisePropertyChanged("NullableInt32Property");
                }
            }

            private long? _nullableInt64Property;
            public long? NullableInt64Property
            {
                get => _nullableInt64Property;
                set
                {
                    _nullableInt64Property = value;
                    RaisePropertyChanged("NullableInt64Property");
                }
            }

            private float? _nullableSingleProperty;
            public float? NullableSingleProperty
            {
                get => _nullableSingleProperty;
                set
                {
                    _nullableSingleProperty = value;
                    RaisePropertyChanged("NullableSingleProperty");
                }
            }

            private double? _nullableDoubleProperty;
            public double? NullableDoubleProperty
            {
                get => _nullableDoubleProperty;
                set
                {
                    _nullableDoubleProperty = value;
                    RaisePropertyChanged("NullableDoubleProperty");
                }
            }

            private bool? _nullableBooleanProperty;
            public bool? NullableBooleanProperty
            {
                get => _nullableBooleanProperty;
                set
                {
                    _nullableBooleanProperty = value;
                    RaisePropertyChanged("NullableBooleanProperty");
                }
            }

            private System.DateTimeOffset? _nullableDateTimeOffsetProperty;
            public System.DateTimeOffset? NullableDateTimeOffsetProperty
            {
                get => _nullableDateTimeOffsetProperty;
                set
                {
                    _nullableDateTimeOffsetProperty = value;
                    RaisePropertyChanged("NullableDateTimeOffsetProperty");
                }
            }

            private decimal? _nullableDecimalProperty;
            public decimal? NullableDecimalProperty
            {
                get => _nullableDecimalProperty;
                set
                {
                    _nullableDecimalProperty = value;
                    RaisePropertyChanged("NullableDecimalProperty");
                }
            }

            private MongoDB.Bson.Decimal128? _nullableDecimal128Property;
            public MongoDB.Bson.Decimal128? NullableDecimal128Property
            {
                get => _nullableDecimal128Property;
                set
                {
                    _nullableDecimal128Property = value;
                    RaisePropertyChanged("NullableDecimal128Property");
                }
            }

            private MongoDB.Bson.ObjectId? _nullableObjectIdProperty;
            public MongoDB.Bson.ObjectId? NullableObjectIdProperty
            {
                get => _nullableObjectIdProperty;
                set
                {
                    _nullableObjectIdProperty = value;
                    RaisePropertyChanged("NullableObjectIdProperty");
                }
            }

            private System.Guid? _nullableGuidProperty;
            public System.Guid? NullableGuidProperty
            {
                get => _nullableGuidProperty;
                set
                {
                    _nullableGuidProperty = value;
                    RaisePropertyChanged("NullableGuidProperty");
                }
            }

            private Realms.RealmInteger<byte> _byteCounterProperty;
            public Realms.RealmInteger<byte> ByteCounterProperty
            {
                get => _byteCounterProperty;
                set
                {
                    _byteCounterProperty = value;
                    RaisePropertyChanged("ByteCounterProperty");
                }
            }

            private Realms.RealmInteger<short> _int16CounterProperty;
            public Realms.RealmInteger<short> Int16CounterProperty
            {
                get => _int16CounterProperty;
                set
                {
                    _int16CounterProperty = value;
                    RaisePropertyChanged("Int16CounterProperty");
                }
            }

            private Realms.RealmInteger<int> _int32CounterProperty;
            public Realms.RealmInteger<int> Int32CounterProperty
            {
                get => _int32CounterProperty;
                set
                {
                    _int32CounterProperty = value;
                    RaisePropertyChanged("Int32CounterProperty");
                }
            }

            private Realms.RealmInteger<long> _int64CounterProperty;
            public Realms.RealmInteger<long> Int64CounterProperty
            {
                get => _int64CounterProperty;
                set
                {
                    _int64CounterProperty = value;
                    RaisePropertyChanged("Int64CounterProperty");
                }
            }

            private Realms.RealmValue _realmValueProperty;
            public Realms.RealmValue RealmValueProperty
            {
                get => _realmValueProperty;
                set
                {
                    _realmValueProperty = value;
                    RaisePropertyChanged("RealmValueProperty");
                }
            }

            public AllTypesClass2UnmanagedAccessor(Type objectType) : base(objectType)
            {
            }

            public override Realms.RealmValue GetValue(string propertyName)
            {
                return propertyName switch
                {
                    "CharProperty" => _charProperty,
                    "ByteProperty" => _byteProperty,
                    "Int16Property" => _int16Property,
                    "Int32Property" => _int32Property,
                    "Int64Property" => _int64Property,
                    "SingleProperty" => _singleProperty,
                    "DoubleProperty" => _doubleProperty,
                    "BooleanProperty" => _booleanProperty,
                    "DateTimeOffsetProperty" => _dateTimeOffsetProperty,
                    "DecimalProperty" => _decimalProperty,
                    "Decimal128Property" => _decimal128Property,
                    "ObjectIdProperty" => _objectIdProperty,
                    "GuidProperty" => _guidProperty,
                    "RequiredStringProperty" => _requiredStringProperty,
                    "StringProperty" => _stringProperty,
                    "RequiredByteArrayProperty" => _requiredByteArrayProperty,
                    "ByteArrayProperty" => _byteArrayProperty,
                    "NullableCharProperty" => _nullableCharProperty,
                    "NullableByteProperty" => _nullableByteProperty,
                    "NullableInt16Property" => _nullableInt16Property,
                    "NullableInt32Property" => _nullableInt32Property,
                    "NullableInt64Property" => _nullableInt64Property,
                    "NullableSingleProperty" => _nullableSingleProperty,
                    "NullableDoubleProperty" => _nullableDoubleProperty,
                    "NullableBooleanProperty" => _nullableBooleanProperty,
                    "NullableDateTimeOffsetProperty" => _nullableDateTimeOffsetProperty,
                    "NullableDecimalProperty" => _nullableDecimalProperty,
                    "NullableDecimal128Property" => _nullableDecimal128Property,
                    "NullableObjectIdProperty" => _nullableObjectIdProperty,
                    "NullableGuidProperty" => _nullableGuidProperty,
                    "ByteCounterProperty" => _byteCounterProperty,
                    "Int16CounterProperty" => _int16CounterProperty,
                    "Int32CounterProperty" => _int32CounterProperty,
                    "Int64CounterProperty" => _int64CounterProperty,
                    "RealmValueProperty" => _realmValueProperty,
                    _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
                };
            }

            public override void SetValue(string propertyName, Realms.RealmValue val)
            {
                switch (propertyName)
                {
                    case "CharProperty":
                        CharProperty = (char)val;
                        return;
                    case "ByteProperty":
                        ByteProperty = (byte)val;
                        return;
                    case "Int16Property":
                        Int16Property = (short)val;
                        return;
                    case "Int32Property":
                        Int32Property = (int)val;
                        return;
                    case "Int64Property":
                        Int64Property = (long)val;
                        return;
                    case "SingleProperty":
                        SingleProperty = (float)val;
                        return;
                    case "DoubleProperty":
                        DoubleProperty = (double)val;
                        return;
                    case "BooleanProperty":
                        BooleanProperty = (bool)val;
                        return;
                    case "DateTimeOffsetProperty":
                        DateTimeOffsetProperty = (System.DateTimeOffset)val;
                        return;
                    case "DecimalProperty":
                        DecimalProperty = (decimal)val;
                        return;
                    case "Decimal128Property":
                        Decimal128Property = (MongoDB.Bson.Decimal128)val;
                        return;
                    case "ObjectIdProperty":
                        ObjectIdProperty = (MongoDB.Bson.ObjectId)val;
                        return;
                    case "GuidProperty":
                        GuidProperty = (System.Guid)val;
                        return;
                    case "RequiredStringProperty":
                        RequiredStringProperty = (string)val;
                        return;
                    case "StringProperty":
                        StringProperty = (string)val;
                        return;
                    case "RequiredByteArrayProperty":
                        RequiredByteArrayProperty = (byte[])val;
                        return;
                    case "ByteArrayProperty":
                        ByteArrayProperty = (byte[])val;
                        return;
                    case "NullableCharProperty":
                        NullableCharProperty = (char?)val;
                        return;
                    case "NullableByteProperty":
                        NullableByteProperty = (byte?)val;
                        return;
                    case "NullableInt16Property":
                        NullableInt16Property = (short?)val;
                        return;
                    case "NullableInt32Property":
                        NullableInt32Property = (int?)val;
                        return;
                    case "NullableInt64Property":
                        NullableInt64Property = (long?)val;
                        return;
                    case "NullableSingleProperty":
                        NullableSingleProperty = (float?)val;
                        return;
                    case "NullableDoubleProperty":
                        NullableDoubleProperty = (double?)val;
                        return;
                    case "NullableBooleanProperty":
                        NullableBooleanProperty = (bool?)val;
                        return;
                    case "NullableDateTimeOffsetProperty":
                        NullableDateTimeOffsetProperty = (System.DateTimeOffset?)val;
                        return;
                    case "NullableDecimalProperty":
                        NullableDecimalProperty = (decimal?)val;
                        return;
                    case "NullableDecimal128Property":
                        NullableDecimal128Property = (MongoDB.Bson.Decimal128?)val;
                        return;
                    case "NullableObjectIdProperty":
                        NullableObjectIdProperty = (MongoDB.Bson.ObjectId?)val;
                        return;
                    case "NullableGuidProperty":
                        NullableGuidProperty = (System.Guid?)val;
                        return;
                    case "ByteCounterProperty":
                        ByteCounterProperty = (Realms.RealmInteger<byte>)val;
                        return;
                    case "Int16CounterProperty":
                        Int16CounterProperty = (Realms.RealmInteger<short>)val;
                        return;
                    case "Int32CounterProperty":
                        Int32CounterProperty = (Realms.RealmInteger<int>)val;
                        return;
                    case "Int64CounterProperty":
                        Int64CounterProperty = (Realms.RealmInteger<long>)val;
                        return;
                    case "RealmValueProperty":
                        RealmValueProperty = (Realms.RealmValue)val;
                        return;
                    default:
                        throw new MissingMemberException($"The object does not have a settable Realm property with name {propertyName}");
                }
            }

            public override void SetValueUnique(string propertyName, Realms.RealmValue val)
            {
                throw new InvalidOperationException("Cannot set the value of an non primary key property with SetValueUnique");
            }

            public override IList<T> GetListValue<T>(string propertyName)
            {
                throw new MissingMemberException($"The object does not have a Realm list property with name {propertyName}");
            }

            public override ISet<T> GetSetValue<T>(string propertyName)
            {
                throw new MissingMemberException($"The object does not have a Realm set property with name {propertyName}");
            }

            public override IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
            {
                throw new MissingMemberException($"The object does not have a Realm dictionary property with name {propertyName}");
            }
        }
    }
}
