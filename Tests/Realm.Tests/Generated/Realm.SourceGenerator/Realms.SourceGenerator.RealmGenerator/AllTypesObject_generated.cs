﻿// <auto-generated />
using Realms.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using Realms;
using Realms.Weaving;
using Realms.Generated;
using Realms.Schema;
using MongoDB.Bson;

namespace Realms.Tests
{
    [Generated]
    [Woven(typeof(AllTypesObjectObjectHelper))]
    public partial class AllTypesObject : IRealmObject, INotifyPropertyChanged
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("AllTypesObject", isEmbedded: false)
        {
            Property.Primitive("CharProperty", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("ByteProperty", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("Int16Property", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("Int32Property", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("Int64Property", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("SingleProperty", RealmValueType.Float, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("DoubleProperty", RealmValueType.Double, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("BooleanProperty", RealmValueType.Bool, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("DateTimeOffsetProperty", RealmValueType.Date, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("DecimalProperty", RealmValueType.Decimal128, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("Decimal128Property", RealmValueType.Decimal128, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("ObjectIdProperty", RealmValueType.ObjectId, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("GuidProperty", RealmValueType.Guid, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("RequiredStringProperty", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Primitive("StringProperty", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Primitive("ByteArrayProperty", RealmValueType.Data, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Primitive("NullableCharProperty", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Primitive("NullableByteProperty", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Primitive("NullableInt16Property", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Primitive("NullableInt32Property", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Primitive("NullableInt64Property", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Primitive("NullableSingleProperty", RealmValueType.Float, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Primitive("NullableDoubleProperty", RealmValueType.Double, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Primitive("NullableBooleanProperty", RealmValueType.Bool, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Primitive("NullableDateTimeOffsetProperty", RealmValueType.Date, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Primitive("NullableDecimalProperty", RealmValueType.Decimal128, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Primitive("NullableDecimal128Property", RealmValueType.Decimal128, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Primitive("NullableObjectIdProperty", RealmValueType.ObjectId, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Primitive("NullableGuidProperty", RealmValueType.Guid, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Primitive("ByteCounterProperty", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("Int16CounterProperty", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("Int32CounterProperty", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("Int64CounterProperty", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.RealmValue("RealmValueProperty"),
        }.Build();
        
        #region IRealmObject implementation
        
        private IAllTypesObjectAccessor _accessor;
        
        IRealmAccessor IRealmObjectBase.Accessor => Accessor;
        
        internal IAllTypesObjectAccessor Accessor => _accessor = _accessor ?? new AllTypesObjectUnmanagedAccessor(typeof(AllTypesObject));
        
        public bool IsManaged => Accessor.IsManaged;
        
        public bool IsValid => Accessor.IsValid;
        
        public bool IsFrozen => Accessor.IsFrozen;
        
        public Realm Realm => Accessor.Realm;
        
        public ObjectSchema ObjectSchema => Accessor.ObjectSchema;
        
        public RealmObjectBase.Dynamic DynamicApi => Accessor.DynamicApi;
        
        
        
        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var newAccessor = (IAllTypesObjectAccessor)managedAccessor;
            var oldAccessor = _accessor as IAllTypesObjectAccessor;
            _accessor = newAccessor;
        
            if (helper != null)
            {
                
                newAccessor.CharProperty = oldAccessor.CharProperty;
                newAccessor.ByteProperty = oldAccessor.ByteProperty;
                newAccessor.Int16Property = oldAccessor.Int16Property;
                newAccessor.Int32Property = oldAccessor.Int32Property;
                newAccessor.Int64Property = oldAccessor.Int64Property;
                newAccessor.SingleProperty = oldAccessor.SingleProperty;
                newAccessor.DoubleProperty = oldAccessor.DoubleProperty;
                newAccessor.BooleanProperty = oldAccessor.BooleanProperty;
                newAccessor.DateTimeOffsetProperty = oldAccessor.DateTimeOffsetProperty;
                newAccessor.DecimalProperty = oldAccessor.DecimalProperty;
                newAccessor.Decimal128Property = oldAccessor.Decimal128Property;
                newAccessor.ObjectIdProperty = oldAccessor.ObjectIdProperty;
                newAccessor.GuidProperty = oldAccessor.GuidProperty;
                newAccessor.RequiredStringProperty = oldAccessor.RequiredStringProperty;
                newAccessor.StringProperty = oldAccessor.StringProperty;
                newAccessor.ByteArrayProperty = oldAccessor.ByteArrayProperty;
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
        
        partial void OnPropertyChanged(string propertyName);
        
        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            OnPropertyChanged(propertyName);
        }
        
        partial void OnManaged();
        
        private void SubscribeForNotifications()
        {
            Accessor.SubscribeForNotifications(RaisePropertyChanged);
        }
        
        private void UnsubscribeFromNotifications()
        {
            Accessor.UnsubscribeFromNotifications();
        }
        
        public static explicit operator AllTypesObject(RealmValue val) => val.AsRealmObject<AllTypesObject>();
        
        public static implicit operator RealmValue(AllTypesObject val) => RealmValue.Object(val);
        
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
        
            if (obj is not IRealmObjectBase iro)
            {
                return false;
            }
        
            return Accessor.Equals(iro.Accessor);
        }
        
        public override int GetHashCode()
        {
            return IsManaged ? Accessor.GetHashCode() : base.GetHashCode();
        }
        
        /***
        public override string ToString()
        {
            return Accessor.ToString();
        }
        **/
        
    
        [EditorBrowsable(EditorBrowsableState.Never)]
        private class AllTypesObjectObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }
        
            public ManagedAccessor CreateAccessor() => new AllTypesObjectManagedAccessor();
        
            public IRealmObjectBase CreateInstance()
            {
                return new AllTypesObject();
            }
        
            public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
            {
                value = null;
                return false;
            }
        }
    }
}

namespace Realms.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IAllTypesObjectAccessor : IRealmAccessor
    {
        char CharProperty { get; set; }
        
        byte ByteProperty { get; set; }
        
        short Int16Property { get; set; }
        
        int Int32Property { get; set; }
        
        long Int64Property { get; set; }
        
        float SingleProperty { get; set; }
        
        double DoubleProperty { get; set; }
        
        bool BooleanProperty { get; set; }
        
        DateTimeOffset DateTimeOffsetProperty { get; set; }
        
        decimal DecimalProperty { get; set; }
        
        Decimal128 Decimal128Property { get; set; }
        
        ObjectId ObjectIdProperty { get; set; }
        
        Guid GuidProperty { get; set; }
        
        string RequiredStringProperty { get; set; }
        
        string StringProperty { get; set; }
        
        byte[] ByteArrayProperty { get; set; }
        
        char? NullableCharProperty { get; set; }
        
        byte? NullableByteProperty { get; set; }
        
        short? NullableInt16Property { get; set; }
        
        int? NullableInt32Property { get; set; }
        
        long? NullableInt64Property { get; set; }
        
        float? NullableSingleProperty { get; set; }
        
        double? NullableDoubleProperty { get; set; }
        
        bool? NullableBooleanProperty { get; set; }
        
        DateTimeOffset? NullableDateTimeOffsetProperty { get; set; }
        
        decimal? NullableDecimalProperty { get; set; }
        
        Decimal128? NullableDecimal128Property { get; set; }
        
        ObjectId? NullableObjectIdProperty { get; set; }
        
        Guid? NullableGuidProperty { get; set; }
        
        RealmInteger<byte> ByteCounterProperty { get; set; }
        
        RealmInteger<short> Int16CounterProperty { get; set; }
        
        RealmInteger<int> Int32CounterProperty { get; set; }
        
        RealmInteger<long> Int64CounterProperty { get; set; }
        
        RealmValue RealmValueProperty { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class AllTypesObjectManagedAccessor : ManagedAccessor, IAllTypesObjectAccessor
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
        
        public DateTimeOffset DateTimeOffsetProperty
        {
            get => (DateTimeOffset)GetValue("DateTimeOffsetProperty");
            set => SetValue("DateTimeOffsetProperty", value);
        }
        
        public decimal DecimalProperty
        {
            get => (decimal)GetValue("DecimalProperty");
            set => SetValue("DecimalProperty", value);
        }
        
        public Decimal128 Decimal128Property
        {
            get => (Decimal128)GetValue("Decimal128Property");
            set => SetValue("Decimal128Property", value);
        }
        
        public ObjectId ObjectIdProperty
        {
            get => (ObjectId)GetValue("ObjectIdProperty");
            set => SetValue("ObjectIdProperty", value);
        }
        
        public Guid GuidProperty
        {
            get => (Guid)GetValue("GuidProperty");
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
        
        public DateTimeOffset? NullableDateTimeOffsetProperty
        {
            get => (DateTimeOffset?)GetValue("NullableDateTimeOffsetProperty");
            set => SetValue("NullableDateTimeOffsetProperty", value);
        }
        
        public decimal? NullableDecimalProperty
        {
            get => (decimal?)GetValue("NullableDecimalProperty");
            set => SetValue("NullableDecimalProperty", value);
        }
        
        public Decimal128? NullableDecimal128Property
        {
            get => (Decimal128?)GetValue("NullableDecimal128Property");
            set => SetValue("NullableDecimal128Property", value);
        }
        
        public ObjectId? NullableObjectIdProperty
        {
            get => (ObjectId?)GetValue("NullableObjectIdProperty");
            set => SetValue("NullableObjectIdProperty", value);
        }
        
        public Guid? NullableGuidProperty
        {
            get => (Guid?)GetValue("NullableGuidProperty");
            set => SetValue("NullableGuidProperty", value);
        }
        
        public RealmInteger<byte> ByteCounterProperty
        {
            get => (RealmInteger<byte>)GetValue("ByteCounterProperty");
            set => SetValue("ByteCounterProperty", value);
        }
        
        public RealmInteger<short> Int16CounterProperty
        {
            get => (RealmInteger<short>)GetValue("Int16CounterProperty");
            set => SetValue("Int16CounterProperty", value);
        }
        
        public RealmInteger<int> Int32CounterProperty
        {
            get => (RealmInteger<int>)GetValue("Int32CounterProperty");
            set => SetValue("Int32CounterProperty", value);
        }
        
        public RealmInteger<long> Int64CounterProperty
        {
            get => (RealmInteger<long>)GetValue("Int64CounterProperty");
            set => SetValue("Int64CounterProperty", value);
        }
        
        public RealmValue RealmValueProperty
        {
            get => (RealmValue)GetValue("RealmValueProperty");
            set => SetValue("RealmValueProperty", value);
        }
    }

    internal class AllTypesObjectUnmanagedAccessor : UnmanagedAccessor, IAllTypesObjectAccessor
    {
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
        
        private DateTimeOffset _dateTimeOffsetProperty;
        public DateTimeOffset DateTimeOffsetProperty
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
        
        private Decimal128 _decimal128Property;
        public Decimal128 Decimal128Property
        {
            get => _decimal128Property;
            set
            {
                _decimal128Property = value;
                RaisePropertyChanged("Decimal128Property");
            }
        }
        
        private ObjectId _objectIdProperty;
        public ObjectId ObjectIdProperty
        {
            get => _objectIdProperty;
            set
            {
                _objectIdProperty = value;
                RaisePropertyChanged("ObjectIdProperty");
            }
        }
        
        private Guid _guidProperty;
        public Guid GuidProperty
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
        
        private DateTimeOffset? _nullableDateTimeOffsetProperty;
        public DateTimeOffset? NullableDateTimeOffsetProperty
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
        
        private Decimal128? _nullableDecimal128Property;
        public Decimal128? NullableDecimal128Property
        {
            get => _nullableDecimal128Property;
            set
            {
                _nullableDecimal128Property = value;
                RaisePropertyChanged("NullableDecimal128Property");
            }
        }
        
        private ObjectId? _nullableObjectIdProperty;
        public ObjectId? NullableObjectIdProperty
        {
            get => _nullableObjectIdProperty;
            set
            {
                _nullableObjectIdProperty = value;
                RaisePropertyChanged("NullableObjectIdProperty");
            }
        }
        
        private Guid? _nullableGuidProperty;
        public Guid? NullableGuidProperty
        {
            get => _nullableGuidProperty;
            set
            {
                _nullableGuidProperty = value;
                RaisePropertyChanged("NullableGuidProperty");
            }
        }
        
        private RealmInteger<byte> _byteCounterProperty;
        public RealmInteger<byte> ByteCounterProperty
        {
            get => _byteCounterProperty;
            set
            {
                _byteCounterProperty = value;
                RaisePropertyChanged("ByteCounterProperty");
            }
        }
        
        private RealmInteger<short> _int16CounterProperty;
        public RealmInteger<short> Int16CounterProperty
        {
            get => _int16CounterProperty;
            set
            {
                _int16CounterProperty = value;
                RaisePropertyChanged("Int16CounterProperty");
            }
        }
        
        private RealmInteger<int> _int32CounterProperty;
        public RealmInteger<int> Int32CounterProperty
        {
            get => _int32CounterProperty;
            set
            {
                _int32CounterProperty = value;
                RaisePropertyChanged("Int32CounterProperty");
            }
        }
        
        private RealmInteger<long> _int64CounterProperty;
        public RealmInteger<long> Int64CounterProperty
        {
            get => _int64CounterProperty;
            set
            {
                _int64CounterProperty = value;
                RaisePropertyChanged("Int64CounterProperty");
            }
        }
        
        private RealmValue _realmValueProperty;
        public RealmValue RealmValueProperty
        {
            get => _realmValueProperty;
            set
            {
                _realmValueProperty = value;
                RaisePropertyChanged("RealmValueProperty");
            }
        }
    
        public AllTypesObjectUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
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
    
        public override void SetValue(string propertyName, RealmValue val)
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
                    DateTimeOffsetProperty = (DateTimeOffset)val;
                    return;
                case "DecimalProperty":
                    DecimalProperty = (decimal)val;
                    return;
                case "Decimal128Property":
                    Decimal128Property = (Decimal128)val;
                    return;
                case "ObjectIdProperty":
                    ObjectIdProperty = (ObjectId)val;
                    return;
                case "GuidProperty":
                    GuidProperty = (Guid)val;
                    return;
                case "RequiredStringProperty":
                    RequiredStringProperty = (string)val;
                    return;
                case "StringProperty":
                    StringProperty = (string)val;
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
                    NullableDateTimeOffsetProperty = (DateTimeOffset?)val;
                    return;
                case "NullableDecimalProperty":
                    NullableDecimalProperty = (decimal?)val;
                    return;
                case "NullableDecimal128Property":
                    NullableDecimal128Property = (Decimal128?)val;
                    return;
                case "NullableObjectIdProperty":
                    NullableObjectIdProperty = (ObjectId?)val;
                    return;
                case "NullableGuidProperty":
                    NullableGuidProperty = (Guid?)val;
                    return;
                case "ByteCounterProperty":
                    ByteCounterProperty = (RealmInteger<byte>)val;
                    return;
                case "Int16CounterProperty":
                    Int16CounterProperty = (RealmInteger<short>)val;
                    return;
                case "Int32CounterProperty":
                    Int32CounterProperty = (RealmInteger<int>)val;
                    return;
                case "Int64CounterProperty":
                    Int64CounterProperty = (RealmInteger<long>)val;
                    return;
                case "RealmValueProperty":
                    RealmValueProperty = (RealmValue)val;
                    return;
                default:
                    throw new MissingMemberException($"The object does not have a settable Realm property with name {propertyName}");
            }
        }
    
        public override void SetValueUnique(string propertyName, RealmValue val)
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

