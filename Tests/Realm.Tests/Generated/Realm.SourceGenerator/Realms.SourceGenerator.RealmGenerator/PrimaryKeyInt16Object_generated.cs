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

namespace Realms.Tests
{
    [Generated("IPrimaryKeyInt16ObjectAccessor")]
    [Woven(typeof(PrimaryKeyInt16ObjectObjectHelper))]
    public partial class PrimaryKeyInt16Object : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("PrimaryKeyInt16Object", isEmbedded: false)
        {
            Property.Primitive("_id", RealmValueType.Int, isPrimaryKey: true, isIndexed: false, isNullable: false),
        }.Build();
        
        #region IRealmObject implementation
        
        private IPrimaryKeyInt16ObjectAccessor _accessor;
        
        IRealmAccessor IRealmObjectBase.Accessor => Accessor;
        
        internal IPrimaryKeyInt16ObjectAccessor Accessor => _accessor = _accessor ?? new PrimaryKeyInt16ObjectUnmanagedAccessor(typeof(PrimaryKeyInt16Object));
        
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
            var newAccessor = (IPrimaryKeyInt16ObjectAccessor)managedAccessor;
            var oldAccessor = _accessor as IPrimaryKeyInt16ObjectAccessor;
            _accessor = newAccessor;
        
            if (helper != null)
            {
                
                if(!skipDefaults || oldAccessor.Id != default(short))
                {
                    newAccessor.Id = oldAccessor.Id;
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
        
        public static explicit operator PrimaryKeyInt16Object(RealmValue val) => val.AsRealmObject<PrimaryKeyInt16Object>();
        
        public static implicit operator RealmValue(PrimaryKeyInt16Object val) => RealmValue.Object(val);
        
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
        private class PrimaryKeyInt16ObjectObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }
        
            public ManagedAccessor CreateAccessor() => new PrimaryKeyInt16ObjectManagedAccessor();
        
            public IRealmObjectBase CreateInstance()
            {
                return new PrimaryKeyInt16Object();
            }
        
            public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
            {
                value = ((IPrimaryKeyInt16ObjectAccessor)instance.Accessor).Id;
                return true;
            }
        }
    }
}

namespace Realms.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IPrimaryKeyInt16ObjectAccessor : IRealmAccessor
    {
        short Id { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class PrimaryKeyInt16ObjectManagedAccessor : ManagedAccessor, IPrimaryKeyInt16ObjectAccessor
    {
        public short Id
        {
            get => (short)GetValue("_id");
            set => SetValueUnique("_id", value);
        }
    }

    internal class PrimaryKeyInt16ObjectUnmanagedAccessor : UnmanagedAccessor, IPrimaryKeyInt16ObjectAccessor
    {
        private short _id;
        public short Id
        {
            get => _id;
            set
            {
                _id = value;
                RaisePropertyChanged("Id");
            }
        }
    
        public PrimaryKeyInt16ObjectUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "_id" => _id,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }
    
        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "_id":
                    throw new InvalidOperationException("Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique");
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
            
            Id = (short)val;
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
