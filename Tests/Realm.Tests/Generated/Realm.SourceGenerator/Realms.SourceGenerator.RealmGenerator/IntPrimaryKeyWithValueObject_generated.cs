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

namespace Realms.Tests
{
    [Generated("IIntPrimaryKeyWithValueObjectAccessor")]
    [Woven(typeof(IntPrimaryKeyWithValueObjectObjectHelper))]
    public partial class IntPrimaryKeyWithValueObject : IRealmObject, INotifyPropertyChanged
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("IntPrimaryKeyWithValueObject", isEmbedded: false)
        {
            Property.Primitive("_id", RealmValueType.Int, isPrimaryKey: true, isIndexed: false, isNullable: false),
            Property.Primitive("StringValue", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true),
        }.Build();
        
        #region IRealmObject implementation
        
        private IIntPrimaryKeyWithValueObjectAccessor _accessor;
        
        IRealmAccessor IRealmObjectBase.Accessor => Accessor;
        
        internal IIntPrimaryKeyWithValueObjectAccessor Accessor => _accessor = _accessor ?? new IntPrimaryKeyWithValueObjectUnmanagedAccessor(typeof(IntPrimaryKeyWithValueObject));
        
        public bool IsManaged => Accessor.IsManaged;
        
        public bool IsValid => Accessor.IsValid;
        
        public bool IsFrozen => Accessor.IsFrozen;
        
        public Realm Realm => Accessor.Realm;
        
        public ObjectSchema ObjectSchema => Accessor.ObjectSchema;
        
        public RealmObjectBase.Dynamic DynamicApi => Accessor.DynamicApi;
        
        public int BacklinksCount => Accessor.BacklinksCount;
        
        
        
        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var newAccessor = (IIntPrimaryKeyWithValueObjectAccessor)managedAccessor;
            var oldAccessor = _accessor as IIntPrimaryKeyWithValueObjectAccessor;
            _accessor = newAccessor;
        
            if (helper != null)
            {
                
                if(!skipDefaults || oldAccessor.Id != default(int))
                {
                    newAccessor.Id = oldAccessor.Id;
                }
                if(!skipDefaults || oldAccessor.StringValue != default(string))
                {
                    newAccessor.StringValue = oldAccessor.StringValue;
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
        
        public static explicit operator IntPrimaryKeyWithValueObject(RealmValue val) => val.AsRealmObject<IntPrimaryKeyWithValueObject>();
        
        public static implicit operator RealmValue(IntPrimaryKeyWithValueObject val) => RealmValue.Object(val);
        
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
        private class IntPrimaryKeyWithValueObjectObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }
        
            public ManagedAccessor CreateAccessor() => new IntPrimaryKeyWithValueObjectManagedAccessor();
        
            public IRealmObjectBase CreateInstance()
            {
                return new IntPrimaryKeyWithValueObject();
            }
        
            public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
            {
                value = ((IIntPrimaryKeyWithValueObjectAccessor)instance.Accessor).Id;
                return true;
            }
        }
    }
}

namespace Realms.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IIntPrimaryKeyWithValueObjectAccessor : IRealmAccessor
    {
        int Id { get; set; }
        
        string StringValue { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class IntPrimaryKeyWithValueObjectManagedAccessor : ManagedAccessor, IIntPrimaryKeyWithValueObjectAccessor
    {
        public int Id
        {
            get => (int)GetValue("_id");
            set => SetValueUnique("_id", value);
        }
        
        public string StringValue
        {
            get => (string)GetValue("StringValue");
            set => SetValue("StringValue", value);
        }
    }

    internal class IntPrimaryKeyWithValueObjectUnmanagedAccessor : UnmanagedAccessor, IIntPrimaryKeyWithValueObjectAccessor
    {
        private int _id;
        public int Id
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
    
        public IntPrimaryKeyWithValueObjectUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "_id" => _id,
                "StringValue" => _stringValue,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }
    
        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "_id":
                    throw new InvalidOperationException("Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique");
                case "StringValue":
                    StringValue = (string)val;
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
            
            Id = (int)val;
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

