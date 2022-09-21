﻿// <auto-generated />
using Realms.Tests.Sync;
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

namespace Realms.Tests.Sync
{
    [Generated("IHugeSyncObjectAccessor")]
    [Woven(typeof(HugeSyncObjectObjectHelper))]
    public partial class HugeSyncObject : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("HugeSyncObject", isEmbedded: false)
        {
            Property.Primitive("_id", RealmValueType.ObjectId, isPrimaryKey: true, isIndexed: false, isNullable: false),
            Property.Primitive("Data", RealmValueType.Data, isPrimaryKey: false, isIndexed: false, isNullable: true),
        }.Build();
        
        #region IRealmObject implementation
        
        private IHugeSyncObjectAccessor _accessor;
        
        IRealmAccessor IRealmObjectBase.Accessor => Accessor;
        
        internal IHugeSyncObjectAccessor Accessor => _accessor = _accessor ?? new HugeSyncObjectUnmanagedAccessor(typeof(HugeSyncObject));
        
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
            var newAccessor = (IHugeSyncObjectAccessor)managedAccessor;
            var oldAccessor = _accessor as IHugeSyncObjectAccessor;
            _accessor = newAccessor;
        
            if (helper != null)
            {
                
                newAccessor.Id = oldAccessor.Id;
                if(!skipDefaults || oldAccessor.Data != default(byte[]))
                {
                    newAccessor.Data = oldAccessor.Data;
                }
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
        
        public static explicit operator HugeSyncObject(RealmValue val) => val.AsRealmObject<HugeSyncObject>();
        
        public static implicit operator RealmValue(HugeSyncObject val) => RealmValue.Object(val);
        
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
        private class HugeSyncObjectObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }
        
            public ManagedAccessor CreateAccessor() => new HugeSyncObjectManagedAccessor();
        
            public IRealmObjectBase CreateInstance()
            {
                return new HugeSyncObject();
            }
        
            public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
            {
                value = ((IHugeSyncObjectAccessor)instance.Accessor).Id;
                return true;
            }
        }
    }
}

namespace Realms.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IHugeSyncObjectAccessor : IRealmAccessor
    {
        ObjectId Id { get; set; }
        
        byte[] Data { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class HugeSyncObjectManagedAccessor : ManagedAccessor, IHugeSyncObjectAccessor
    {
        public ObjectId Id
        {
            get => (ObjectId)GetValue("_id");
            set => SetValueUnique("_id", value);
        }
        
        public byte[] Data
        {
            get => (byte[])GetValue("Data");
            set => SetValue("Data", value);
        }
    }

    internal class HugeSyncObjectUnmanagedAccessor : UnmanagedAccessor, IHugeSyncObjectAccessor
    {
        private ObjectId _id = ObjectId.GenerateNewId();
        public ObjectId Id
        {
            get => _id;
            set
            {
                _id = value;
                RaisePropertyChanged("Id");
            }
        }
        
        private byte[] _data;
        public byte[] Data
        {
            get => _data;
            set
            {
                _data = value;
                RaisePropertyChanged("Data");
            }
        }
    
        public HugeSyncObjectUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "_id" => _id,
                "Data" => _data,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }
    
        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "_id":
                    throw new InvalidOperationException("Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique");
                case "Data":
                    Data = (byte[])val;
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

