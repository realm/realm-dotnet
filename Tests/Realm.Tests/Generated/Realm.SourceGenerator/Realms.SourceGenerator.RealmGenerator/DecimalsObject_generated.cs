﻿// <auto-generated />
using Realms.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Reflection;
using System.ComponentModel;
using Realms;
using Realms.Weaving;
using Realms.Generated;
using Realms.Schema;
using MongoDB.Bson;

namespace Realms.Tests
{
    [Generated("IDecimalsObjectAccessor")]
    [Woven(typeof(DecimalsObjectObjectHelper))]
    public partial class DecimalsObject : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("DecimalsObject", isEmbedded: false)
        {
            Property.Primitive("DecimalValue", RealmValueType.Decimal128, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "DecimalValue"),
            Property.Primitive("Decimal128Value", RealmValueType.Decimal128, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "Decimal128Value"),
        }.Build();
        
        #region IRealmObject implementation
        
        private IDecimalsObjectAccessor _accessor;
        
        IRealmAccessor IRealmObjectBase.Accessor => Accessor;
        
        internal IDecimalsObjectAccessor Accessor => _accessor = _accessor ?? new DecimalsObjectUnmanagedAccessor(typeof(DecimalsObject));
        
        [IgnoreDataMember, XmlIgnore]
        public bool IsManaged => Accessor.IsManaged;
        
        [IgnoreDataMember, XmlIgnore]
        public bool IsValid => Accessor.IsValid;
        
        [IgnoreDataMember, XmlIgnore]
        public bool IsFrozen => Accessor.IsFrozen;
        
        [IgnoreDataMember, XmlIgnore]
        public Realm Realm => Accessor.Realm;
        
        [IgnoreDataMember, XmlIgnore]
        public ObjectSchema ObjectSchema => Accessor.ObjectSchema;
        
        [IgnoreDataMember, XmlIgnore]
        public RealmObjectBase.Dynamic DynamicApi => Accessor.DynamicApi;
        
        [IgnoreDataMember, XmlIgnore]
        public int BacklinksCount => Accessor.BacklinksCount;
        
        
        
        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var newAccessor = (IDecimalsObjectAccessor)managedAccessor;
            var oldAccessor = _accessor as IDecimalsObjectAccessor;
            _accessor = newAccessor;
        
            if (helper != null)
            {
                
                newAccessor.DecimalValue = oldAccessor.DecimalValue;
                newAccessor.Decimal128Value = oldAccessor.Decimal128Value;
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
        
        public static explicit operator DecimalsObject(RealmValue val) => val.AsRealmObject<DecimalsObject>();
        
        public static implicit operator RealmValue(DecimalsObject val) => RealmValue.Object(val);
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TypeInfo GetTypeInfo()
        {
            return Accessor.GetTypeInfo(this);
        }
        
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
        
        public override string ToString()
        {
            return Accessor.ToString();
        }
    
        [EditorBrowsable(EditorBrowsableState.Never)]
        private class DecimalsObjectObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }
        
            public ManagedAccessor CreateAccessor() => new DecimalsObjectManagedAccessor();
        
            public IRealmObjectBase CreateInstance()
            {
                return new DecimalsObject();
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
    internal interface IDecimalsObjectAccessor : IRealmAccessor
    {
        decimal DecimalValue { get; set; }
        
        Decimal128 Decimal128Value { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class DecimalsObjectManagedAccessor : ManagedAccessor, IDecimalsObjectAccessor
    {
        public decimal DecimalValue
        {
            get => (decimal)GetValue("DecimalValue");
            set => SetValue("DecimalValue", value);
        }
        
        public Decimal128 Decimal128Value
        {
            get => (Decimal128)GetValue("Decimal128Value");
            set => SetValue("Decimal128Value", value);
        }
    }

    internal class DecimalsObjectUnmanagedAccessor : UnmanagedAccessor, IDecimalsObjectAccessor
    {
        private decimal _decimalValue;
        public decimal DecimalValue
        {
            get => _decimalValue;
            set
            {
                _decimalValue = value;
                RaisePropertyChanged("DecimalValue");
            }
        }
        
        private Decimal128 _decimal128Value;
        public Decimal128 Decimal128Value
        {
            get => _decimal128Value;
            set
            {
                _decimal128Value = value;
                RaisePropertyChanged("Decimal128Value");
            }
        }
    
        public DecimalsObjectUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "DecimalValue" => _decimalValue,
                "Decimal128Value" => _decimal128Value,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }
    
        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "DecimalValue":
                    DecimalValue = (decimal)val;
                    return;
                case "Decimal128Value":
                    Decimal128Value = (Decimal128)val;
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

