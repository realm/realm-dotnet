﻿// <auto-generated />
using Realms.Tests.Sync;
using Realms.Tests.Sync.Generated;
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
using Realms.Schema;

namespace Realms.Tests.Sync
{
    [Generated]
    [Woven(typeof(ObjectWithPartitionValueObjectHelper))]
    public partial class ObjectWithPartitionValue : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("ObjectWithPartitionValue", isEmbedded: false)
        {
            Property.Primitive("_id", RealmValueType.String, isPrimaryKey: true, isIndexed: false, isNullable: true, managedName: "Id"),
            Property.Primitive("Value", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "Value"),
            Property.Primitive("realm_id", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "Partition"),
        }.Build();
        
        #region IRealmObject implementation
        
        private IObjectWithPartitionValueAccessor _accessor;
        
        IRealmAccessor IRealmObjectBase.Accessor => Accessor;
        
        internal IObjectWithPartitionValueAccessor Accessor => _accessor = _accessor ?? new ObjectWithPartitionValueUnmanagedAccessor(typeof(ObjectWithPartitionValue));
        
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
        public DynamicObjectApi DynamicApi => Accessor.DynamicApi;
        
        [IgnoreDataMember, XmlIgnore]
        public int BacklinksCount => Accessor.BacklinksCount;
        
        
        
        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var newAccessor = (IObjectWithPartitionValueAccessor)managedAccessor;
            var oldAccessor = _accessor as IObjectWithPartitionValueAccessor;
            _accessor = newAccessor;
        
            if (helper != null)
            {
                
                if(!skipDefaults || oldAccessor.Id != default(string))
                {
                    newAccessor.Id = oldAccessor.Id;
                }
                if(!skipDefaults || oldAccessor.Value != default(string))
                {
                    newAccessor.Value = oldAccessor.Value;
                }
                if(!skipDefaults || oldAccessor.Partition != default(string))
                {
                    newAccessor.Partition = oldAccessor.Partition;
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
        
        public static explicit operator ObjectWithPartitionValue(RealmValue val) => val.AsRealmObject<ObjectWithPartitionValue>();
        
        public static implicit operator RealmValue(ObjectWithPartitionValue val) => RealmValue.Object(val);
        
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
        private class ObjectWithPartitionValueObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }
        
            public ManagedAccessor CreateAccessor() => new ObjectWithPartitionValueManagedAccessor();
        
            public IRealmObjectBase CreateInstance()
            {
                return new ObjectWithPartitionValue();
            }
        
            public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
            {
                value = ((IObjectWithPartitionValueAccessor)instance.Accessor).Id;
                return true;
            }
        }
    }
}

namespace Realms.Tests.Sync.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IObjectWithPartitionValueAccessor : IRealmAccessor
    {
        string Id { get; set; }
        
        string Value { get; set; }
        
        string Partition { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class ObjectWithPartitionValueManagedAccessor : ManagedAccessor, IObjectWithPartitionValueAccessor
    {
        public string Id
        {
            get => (string)GetValue("_id");
            set => SetValueUnique("_id", value);
        }
        
        public string Value
        {
            get => (string)GetValue("Value");
            set => SetValue("Value", value);
        }
        
        public string Partition
        {
            get => (string)GetValue("realm_id");
            set => SetValue("realm_id", value);
        }
    }

    internal class ObjectWithPartitionValueUnmanagedAccessor : UnmanagedAccessor, IObjectWithPartitionValueAccessor
    {
        private string _id;
        public string Id
        {
            get => _id;
            set
            {
                _id = value;
                RaisePropertyChanged("Id");
            }
        }
        
        private string _value;
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                RaisePropertyChanged("Value");
            }
        }
        
        private string _partition;
        public string Partition
        {
            get => _partition;
            set
            {
                _partition = value;
                RaisePropertyChanged("Partition");
            }
        }
    
        public ObjectWithPartitionValueUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "_id" => _id,
                "Value" => _value,
                "realm_id" => _partition,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }
    
        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "_id":
                    throw new InvalidOperationException("Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique");
                case "Value":
                    Value = (string)val;
                    return;
                case "realm_id":
                    Partition = (string)val;
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
            
            Id = (string)val;
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

