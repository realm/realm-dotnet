﻿// <auto-generated />
using Realms.Tests.Database;
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

namespace Realms.Tests.Database
{
    [Generated("IPrimaryKeyWithPKRelationAccessor")]
    [Woven(typeof(PrimaryKeyWithPKRelationObjectHelper))]
    public partial class PrimaryKeyWithPKRelation : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("PrimaryKeyWithPKRelation", isEmbedded: false)
        {
            Property.Primitive("Id", RealmValueType.Int, isPrimaryKey: true, isIndexed: false, isNullable: false),
            Property.Primitive("StringValue", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Object("OtherObject", "PrimaryKeyObject"),
        }.Build();
        
        #region IRealmObject implementation
        
        private IPrimaryKeyWithPKRelationAccessor _accessor;
        
        IRealmAccessor IRealmObjectBase.Accessor => Accessor;
        
        internal IPrimaryKeyWithPKRelationAccessor Accessor => _accessor = _accessor ?? new PrimaryKeyWithPKRelationUnmanagedAccessor(typeof(PrimaryKeyWithPKRelation));
        
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
            var newAccessor = (IPrimaryKeyWithPKRelationAccessor)managedAccessor;
            var oldAccessor = _accessor as IPrimaryKeyWithPKRelationAccessor;
            _accessor = newAccessor;
        
            if (helper != null)
            {
                
                if(!skipDefaults || oldAccessor.Id != default(long))
                {
                    newAccessor.Id = oldAccessor.Id;
                }
                if(!skipDefaults || oldAccessor.StringValue != default(string))
                {
                    newAccessor.StringValue = oldAccessor.StringValue;
                }
                if(oldAccessor.OtherObject != null)
                {
                    newAccessor.Realm.Add(oldAccessor.OtherObject, update);
                }
                newAccessor.OtherObject = oldAccessor.OtherObject;
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
        
        public static explicit operator PrimaryKeyWithPKRelation(RealmValue val) => val.AsRealmObject<PrimaryKeyWithPKRelation>();
        
        public static implicit operator RealmValue(PrimaryKeyWithPKRelation val) => RealmValue.Object(val);
        
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
        private class PrimaryKeyWithPKRelationObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }
        
            public ManagedAccessor CreateAccessor() => new PrimaryKeyWithPKRelationManagedAccessor();
        
            public IRealmObjectBase CreateInstance()
            {
                return new PrimaryKeyWithPKRelation();
            }
        
            public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
            {
                value = ((IPrimaryKeyWithPKRelationAccessor)instance.Accessor).Id;
                return true;
            }
        }
    }
}

namespace Realms.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IPrimaryKeyWithPKRelationAccessor : IRealmAccessor
    {
        long Id { get; set; }
        
        string StringValue { get; set; }
        
        PrimaryKeyObject OtherObject { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class PrimaryKeyWithPKRelationManagedAccessor : ManagedAccessor, IPrimaryKeyWithPKRelationAccessor
    {
        public long Id
        {
            get => (long)GetValue("Id");
            set => SetValueUnique("Id", value);
        }
        
        public string StringValue
        {
            get => (string)GetValue("StringValue");
            set => SetValue("StringValue", value);
        }
        
        public PrimaryKeyObject OtherObject
        {
            get => (PrimaryKeyObject)GetValue("OtherObject");
            set => SetValue("OtherObject", value);
        }
    }

    internal class PrimaryKeyWithPKRelationUnmanagedAccessor : UnmanagedAccessor, IPrimaryKeyWithPKRelationAccessor
    {
        private long _id;
        public long Id
        {
            get => _id;
            set
            {
                _id = value;
                RaisePropertyChanged("Id");
            }
        }
        
        private string _stringValue;
        public string StringValue
        {
            get => _stringValue;
            set
            {
                _stringValue = value;
                RaisePropertyChanged("StringValue");
            }
        }
        
        private PrimaryKeyObject _otherObject;
        public PrimaryKeyObject OtherObject
        {
            get => _otherObject;
            set
            {
                _otherObject = value;
                RaisePropertyChanged("OtherObject");
            }
        }
    
        public PrimaryKeyWithPKRelationUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "Id" => _id,
                "StringValue" => _stringValue,
                "OtherObject" => _otherObject,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }
    
        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "Id":
                    throw new InvalidOperationException("Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique");
                case "StringValue":
                    StringValue = (string)val;
                    return;
                case "OtherObject":
                    OtherObject = (PrimaryKeyObject)val;
                    return;
                default:
                    throw new MissingMemberException($"The object does not have a settable Realm property with name {propertyName}");
            }
        }
    
        public override void SetValueUnique(string propertyName, RealmValue val)
        {
            if (propertyName != "Id")
            {
                throw new InvalidOperationException($"Cannot set the value of non primary key property ({propertyName}) with SetValueUnique");
            }
            
            Id = (long)val;
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

