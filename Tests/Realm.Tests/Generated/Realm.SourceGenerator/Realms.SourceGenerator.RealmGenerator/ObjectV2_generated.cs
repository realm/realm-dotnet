﻿// <auto-generated />
using Realms.Tests.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using Realms;
using Realms.Weaving;
using Realms.Generated;
using Realms.Schema;

namespace Realms.Tests.Database
{
    public partial class MigrationTests
    {
        [Generated("IObjectV2Accessor")]
        [Woven(typeof(ObjectV2ObjectHelper))]
        private partial class ObjectV2 : IRealmObject, INotifyPropertyChanged
        {
            public static ObjectSchema RealmSchema = new ObjectSchema.Builder("Object", isEmbedded: false)
            {
                Property.Primitive("Id", RealmValueType.String, isPrimaryKey: true, isIndexed: false, isNullable: true),
                Property.Primitive("Value", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true),
            }.Build();
            
            #region IRealmObject implementation
            
            private IObjectV2Accessor _accessor;
            
            IRealmAccessor IRealmObjectBase.Accessor => Accessor;
            
            internal IObjectV2Accessor Accessor => _accessor = _accessor ?? new ObjectV2UnmanagedAccessor(typeof(ObjectV2));
            
            public bool IsManaged => Accessor.IsManaged;
            
            public bool IsValid => Accessor.IsValid;
            
            public bool IsFrozen => Accessor.IsFrozen;
            
            public Realm Realm => Accessor.Realm;
            
            public ObjectSchema ObjectSchema => Accessor.ObjectSchema;
            
            public RealmObjectBase.Dynamic DynamicApi => Accessor.DynamicApi;
            
            public int BacklinksCount => Accessor.BacklinksCount;
            
            
            
            public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
            {
                var newAccessor = (IObjectV2Accessor)managedAccessor;
                var oldAccessor = _accessor as IObjectV2Accessor;
                _accessor = newAccessor;
            
                if (helper != null)
                {
                    
                    newAccessor.Id = oldAccessor.Id;
                    newAccessor.Value = oldAccessor.Value;
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
            
            public static explicit operator ObjectV2(RealmValue val) => val.AsRealmObject<ObjectV2>();
            
            public static implicit operator RealmValue(ObjectV2 val) => RealmValue.Object(val);
            
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
            private class ObjectV2ObjectHelper : IRealmObjectHelper
            {
                public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
                {
                    throw new InvalidOperationException("This method should not be called for source generated classes.");
                }
            
                public ManagedAccessor CreateAccessor() => new ObjectV2ManagedAccessor();
            
                public IRealmObjectBase CreateInstance()
                {
                    return new ObjectV2();
                }
            
                public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
                {
                    value = ((IObjectV2Accessor)instance.Accessor).Id;
                    return true;
                }
            }
        }
    }
    
}

namespace Realms.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IObjectV2Accessor : IRealmAccessor
    {
        string Id { get; set; }
        
        string Value { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class ObjectV2ManagedAccessor : ManagedAccessor, IObjectV2Accessor
    {
        public string Id
        {
            get => (string)GetValue("Id");
            set => SetValueUnique("Id", value);
        }
        
        public string Value
        {
            get => (string)GetValue("Value");
            set => SetValue("Value", value);
        }
    }

    internal class ObjectV2UnmanagedAccessor : UnmanagedAccessor, IObjectV2Accessor
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
    
        public ObjectV2UnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "Id" => _id,
                "Value" => _value,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }
    
        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "Id":
                    throw new InvalidOperationException("Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique");
                case "Value":
                    Value = (string)val;
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

