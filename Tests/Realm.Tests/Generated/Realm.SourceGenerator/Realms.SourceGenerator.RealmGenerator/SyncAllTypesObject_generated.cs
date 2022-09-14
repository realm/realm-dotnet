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
    [Woven(typeof(SyncAllTypesObjectObjectHelper))]
    public partial class SyncAllTypesObject : IRealmObject, INotifyPropertyChanged
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("SyncAllTypesObject", isEmbedded: false)
        {
            Property.Primitive("_id", RealmValueType.ObjectId, isPrimaryKey: true, isIndexed: false, isNullable: false),
            Property.Primitive("CharProperty", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("ByteProperty", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("Int16Property", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("Int32Property", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("Int64Property", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("FloatProperty", RealmValueType.Float, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("DoubleProperty", RealmValueType.Double, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("BooleanProperty", RealmValueType.Bool, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("DateTimeOffsetProperty", RealmValueType.Date, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("DecimalProperty", RealmValueType.Decimal128, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("Decimal128Property", RealmValueType.Decimal128, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("ObjectIdProperty", RealmValueType.ObjectId, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("GuidProperty", RealmValueType.Guid, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("StringProperty", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Primitive("ByteArrayProperty", RealmValueType.Data, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.RealmValue("RealmValueProperty"),
            Property.Object("ObjectProperty", "IntPropertyObject"),
            Property.Object("EmbeddedObjectProperty", "EmbeddedIntPropertyObject"),
        }.Build();
        
        #region IRealmObject implementation
        
        private ISyncAllTypesObjectAccessor _accessor;
        
        IRealmAccessor IRealmObjectBase.Accessor => Accessor;
        
        internal ISyncAllTypesObjectAccessor Accessor => _accessor = _accessor ?? new SyncAllTypesObjectUnmanagedAccessor(typeof(SyncAllTypesObject));
        
        public bool IsManaged => Accessor.IsManaged;
        
        public bool IsValid => Accessor.IsValid;
        
        public bool IsFrozen => Accessor.IsFrozen;
        
        public Realm Realm => Accessor.Realm;
        
        public ObjectSchema ObjectSchema => Accessor.ObjectSchema;
        
        public RealmObjectBase.Dynamic DynamicApi => Accessor.DynamicApi;
        
        
        
        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var newAccessor = (ISyncAllTypesObjectAccessor)managedAccessor;
            var oldAccessor = _accessor as ISyncAllTypesObjectAccessor;
            _accessor = newAccessor;
        
            if (helper != null)
            {
                
                newAccessor.Id = oldAccessor.Id;
                newAccessor.CharProperty = oldAccessor.CharProperty;
                newAccessor.ByteProperty = oldAccessor.ByteProperty;
                newAccessor.Int16Property = oldAccessor.Int16Property;
                newAccessor.Int32Property = oldAccessor.Int32Property;
                newAccessor.Int64Property = oldAccessor.Int64Property;
                newAccessor.FloatProperty = oldAccessor.FloatProperty;
                newAccessor.DoubleProperty = oldAccessor.DoubleProperty;
                newAccessor.BooleanProperty = oldAccessor.BooleanProperty;
                newAccessor.DateTimeOffsetProperty = oldAccessor.DateTimeOffsetProperty;
                newAccessor.DecimalProperty = oldAccessor.DecimalProperty;
                newAccessor.Decimal128Property = oldAccessor.Decimal128Property;
                newAccessor.ObjectIdProperty = oldAccessor.ObjectIdProperty;
                newAccessor.GuidProperty = oldAccessor.GuidProperty;
                newAccessor.StringProperty = oldAccessor.StringProperty;
                newAccessor.ByteArrayProperty = oldAccessor.ByteArrayProperty;
                newAccessor.RealmValueProperty = oldAccessor.RealmValueProperty;
                if(oldAccessor.ObjectProperty != null)
                {
                    newAccessor.Realm.Add(oldAccessor.ObjectProperty, update);
                }
                newAccessor.ObjectProperty = oldAccessor.ObjectProperty;
                newAccessor.EmbeddedObjectProperty = oldAccessor.EmbeddedObjectProperty;
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
        
        public static explicit operator SyncAllTypesObject(RealmValue val) => val.AsRealmObject<SyncAllTypesObject>();
        
        public static implicit operator RealmValue(SyncAllTypesObject val) => RealmValue.Object(val);
        
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
        private class SyncAllTypesObjectObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }
        
            public ManagedAccessor CreateAccessor() => new SyncAllTypesObjectManagedAccessor();
        
            public IRealmObjectBase CreateInstance()
            {
                return new SyncAllTypesObject();
            }
        
            public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
            {
                value = ((ISyncAllTypesObjectAccessor)instance.Accessor).Id;
                return true;
            }
        }
    }
}

namespace Realms.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface ISyncAllTypesObjectAccessor : IRealmAccessor
    {
        ObjectId Id { get; set; }
        
        char CharProperty { get; set; }
        
        byte ByteProperty { get; set; }
        
        short Int16Property { get; set; }
        
        int Int32Property { get; set; }
        
        long Int64Property { get; set; }
        
        float FloatProperty { get; set; }
        
        double DoubleProperty { get; set; }
        
        bool BooleanProperty { get; set; }
        
        DateTimeOffset DateTimeOffsetProperty { get; set; }
        
        decimal DecimalProperty { get; set; }
        
        Decimal128 Decimal128Property { get; set; }
        
        ObjectId ObjectIdProperty { get; set; }
        
        Guid GuidProperty { get; set; }
        
        string StringProperty { get; set; }
        
        byte[] ByteArrayProperty { get; set; }
        
        RealmValue RealmValueProperty { get; set; }
        
        IntPropertyObject ObjectProperty { get; set; }
        
        EmbeddedIntPropertyObject EmbeddedObjectProperty { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class SyncAllTypesObjectManagedAccessor : ManagedAccessor, ISyncAllTypesObjectAccessor
    {
        public ObjectId Id
        {
            get => (ObjectId)GetValue("_id");
            set => SetValueUnique("_id", value);
        }
        
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
        
        public float FloatProperty
        {
            get => (float)GetValue("FloatProperty");
            set => SetValue("FloatProperty", value);
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
        
        public RealmValue RealmValueProperty
        {
            get => (RealmValue)GetValue("RealmValueProperty");
            set => SetValue("RealmValueProperty", value);
        }
        
        public IntPropertyObject ObjectProperty
        {
            get => (IntPropertyObject)GetValue("ObjectProperty");
            set => SetValue("ObjectProperty", value);
        }
        
        public EmbeddedIntPropertyObject EmbeddedObjectProperty
        {
            get => (EmbeddedIntPropertyObject)GetValue("EmbeddedObjectProperty");
            set => SetValue("EmbeddedObjectProperty", value);
        }
    }

    internal class SyncAllTypesObjectUnmanagedAccessor : UnmanagedAccessor, ISyncAllTypesObjectAccessor
    {
        private ObjectId _id = ObjectId.GenerateNewId();
        public ObjectId Id
        {
            get => _id;
            set
            {
                _id = value;
                RaisePropertyChanged("_id");
            }
        }
        
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
        
        private float _floatProperty;
        public float FloatProperty
        {
            get => _floatProperty;
            set
            {
                _floatProperty = value;
                RaisePropertyChanged("FloatProperty");
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
        
        private IntPropertyObject _objectProperty;
        public IntPropertyObject ObjectProperty
        {
            get => _objectProperty;
            set
            {
                _objectProperty = value;
                RaisePropertyChanged("ObjectProperty");
            }
        }
        
        private EmbeddedIntPropertyObject _embeddedObjectProperty;
        public EmbeddedIntPropertyObject EmbeddedObjectProperty
        {
            get => _embeddedObjectProperty;
            set
            {
                _embeddedObjectProperty = value;
                RaisePropertyChanged("EmbeddedObjectProperty");
            }
        }
    
        public SyncAllTypesObjectUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "_id" => _id,
                "CharProperty" => _charProperty,
                "ByteProperty" => _byteProperty,
                "Int16Property" => _int16Property,
                "Int32Property" => _int32Property,
                "Int64Property" => _int64Property,
                "FloatProperty" => _floatProperty,
                "DoubleProperty" => _doubleProperty,
                "BooleanProperty" => _booleanProperty,
                "DateTimeOffsetProperty" => _dateTimeOffsetProperty,
                "DecimalProperty" => _decimalProperty,
                "Decimal128Property" => _decimal128Property,
                "ObjectIdProperty" => _objectIdProperty,
                "GuidProperty" => _guidProperty,
                "StringProperty" => _stringProperty,
                "ByteArrayProperty" => _byteArrayProperty,
                "RealmValueProperty" => _realmValueProperty,
                "ObjectProperty" => _objectProperty,
                "EmbeddedObjectProperty" => _embeddedObjectProperty,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }
    
        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "_id":
                    throw new InvalidOperationException("Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique");
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
                case "FloatProperty":
                    FloatProperty = (float)val;
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
                case "StringProperty":
                    StringProperty = (string)val;
                    return;
                case "ByteArrayProperty":
                    ByteArrayProperty = (byte[])val;
                    return;
                case "RealmValueProperty":
                    RealmValueProperty = (RealmValue)val;
                    return;
                case "ObjectProperty":
                    ObjectProperty = (IntPropertyObject)val;
                    return;
                case "EmbeddedObjectProperty":
                    EmbeddedObjectProperty = (EmbeddedIntPropertyObject)val;
                    return;
                default:
                    throw new MissingMemberException($"The object does not have a settable Realm property with name {propertyName}");
            }
        }
    
        public override void SetValueUnique(string propertyName, RealmValue val)
        {
            if (propertyName != "_id")
            {
                throw new InvalidOperationException($"Cannot set the value of non primary key property ({propertyName}) with SetValueUnique");
            }
            
            Id = (ObjectId)val;
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

