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
    [Generated("INonPrimaryKeyWithNonPKRelationAccessor")]
    [Woven(typeof(NonPrimaryKeyWithNonPKRelationObjectHelper))]
    public partial class NonPrimaryKeyWithNonPKRelation : IRealmObject, INotifyPropertyChanged
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("NonPrimaryKeyWithNonPKRelation", isEmbedded: false)
        {
            Property.Primitive("StringValue", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Object("OtherObject", "NonPrimaryKeyObject"),
        }.Build();
        
        #region IRealmObject implementation
        
        private INonPrimaryKeyWithNonPKRelationAccessor _accessor;
        
        IRealmAccessor IRealmObjectBase.Accessor => Accessor;
        
        internal INonPrimaryKeyWithNonPKRelationAccessor Accessor => _accessor = _accessor ?? new NonPrimaryKeyWithNonPKRelationUnmanagedAccessor(typeof(NonPrimaryKeyWithNonPKRelation));
        
        public bool IsManaged => Accessor.IsManaged;
        
        public bool IsValid => Accessor.IsValid;
        
        public bool IsFrozen => Accessor.IsFrozen;
        
        public Realm Realm => Accessor.Realm;
        
        public ObjectSchema ObjectSchema => Accessor.ObjectSchema;
        
        public RealmObjectBase.Dynamic DynamicApi => Accessor.DynamicApi;
        
        public int BacklinksCount => Accessor.BacklinksCount;
        
        
        
        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var newAccessor = (INonPrimaryKeyWithNonPKRelationAccessor)managedAccessor;
            var oldAccessor = _accessor as INonPrimaryKeyWithNonPKRelationAccessor;
            _accessor = newAccessor;
        
            if (helper != null)
            {
                
                newAccessor.StringValue = oldAccessor.StringValue;
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
        
        public static explicit operator NonPrimaryKeyWithNonPKRelation(RealmValue val) => val.AsRealmObject<NonPrimaryKeyWithNonPKRelation>();
        
        public static implicit operator RealmValue(NonPrimaryKeyWithNonPKRelation val) => RealmValue.Object(val);
        
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
        private class NonPrimaryKeyWithNonPKRelationObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }
        
            public ManagedAccessor CreateAccessor() => new NonPrimaryKeyWithNonPKRelationManagedAccessor();
        
            public IRealmObjectBase CreateInstance()
            {
                return new NonPrimaryKeyWithNonPKRelation();
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
    internal interface INonPrimaryKeyWithNonPKRelationAccessor : IRealmAccessor
    {
        string StringValue { get; set; }
        
        NonPrimaryKeyObject OtherObject { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class NonPrimaryKeyWithNonPKRelationManagedAccessor : ManagedAccessor, INonPrimaryKeyWithNonPKRelationAccessor
    {
        public string StringValue
        {
            get => (string)GetValue("StringValue");
            set => SetValue("StringValue", value);
        }
        
        public NonPrimaryKeyObject OtherObject
        {
            get => (NonPrimaryKeyObject)GetValue("OtherObject");
            set => SetValue("OtherObject", value);
        }
    }

    internal class NonPrimaryKeyWithNonPKRelationUnmanagedAccessor : UnmanagedAccessor, INonPrimaryKeyWithNonPKRelationAccessor
    {
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
        
        private NonPrimaryKeyObject _otherObject;
        public NonPrimaryKeyObject OtherObject
        {
            get => _otherObject;
            set
            {
                _otherObject = value;
                RaisePropertyChanged("OtherObject");
            }
        }
    
        public NonPrimaryKeyWithNonPKRelationUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "StringValue" => _stringValue,
                "OtherObject" => _otherObject,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }
    
        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "StringValue":
                    StringValue = (string)val;
                    return;
                case "OtherObject":
                    OtherObject = (NonPrimaryKeyObject)val;
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

