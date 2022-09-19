﻿// <auto-generated />
using Realms.Tests.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.ComponentModel;
using Realms;
using Realms.Weaving;
using Realms.Generated;
using Realms.Schema;

namespace Realms.Tests.Database
{
    [Generated("IInternalObjectAccessor")]
    [Woven(typeof(InternalObjectObjectHelper))]
    public partial class InternalObject : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("InternalObject", isEmbedded: false)
        {
            Property.Primitive("IntProperty", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("StringProperty", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true),
        }.Build();
        
        #region IRealmObject implementation
        
        private IInternalObjectAccessor _accessor;
        
        IRealmAccessor IRealmObjectBase.Accessor => Accessor;
        
        internal IInternalObjectAccessor Accessor => _accessor = _accessor ?? new InternalObjectUnmanagedAccessor(typeof(InternalObject));
        
        public bool IsManaged => Accessor.IsManaged;
        
        public bool IsValid => Accessor.IsValid;
        
        public bool IsFrozen => Accessor.IsFrozen;
        
        public Realm Realm => Accessor.Realm;
        
        public ObjectSchema ObjectSchema => Accessor.ObjectSchema;
        
        public RealmObjectBase.Dynamic DynamicApi => Accessor.DynamicApi;
        
        public int BacklinksCount => Accessor.BacklinksCount;
        
        
        
        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var newAccessor = (IInternalObjectAccessor)managedAccessor;
            var oldAccessor = _accessor as IInternalObjectAccessor;
            _accessor = newAccessor;
        
            if (helper != null)
            {
                
                if(!skipDefaults || oldAccessor.IntProperty != default(int))
                {
                    newAccessor.IntProperty = oldAccessor.IntProperty;
                }
                if(!skipDefaults || oldAccessor.StringProperty != default(string))
                {
                    newAccessor.StringProperty = oldAccessor.StringProperty;
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
        
        public static explicit operator InternalObject(RealmValue val) => val.AsRealmObject<InternalObject>();
        
        public static implicit operator RealmValue(InternalObject val) => RealmValue.Object(val);
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TypeInfo GetTypeInfo()
        {
            return Accessor.GetTypeInfo(this);
        }
        
        
        
        public override int GetHashCode()
        {
            return IsManaged ? Accessor.GetHashCode() : base.GetHashCode();
        }
        
        
    
        [EditorBrowsable(EditorBrowsableState.Never)]
        private class InternalObjectObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }
        
            public ManagedAccessor CreateAccessor() => new InternalObjectManagedAccessor();
        
            public IRealmObjectBase CreateInstance()
            {
                return new InternalObject();
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
    internal interface IInternalObjectAccessor : IRealmAccessor
    {
        int IntProperty { get; set; }
        
        string StringProperty { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class InternalObjectManagedAccessor : ManagedAccessor, IInternalObjectAccessor
    {
        public int IntProperty
        {
            get => (int)GetValue("IntProperty");
            set => SetValue("IntProperty", value);
        }
        
        public string StringProperty
        {
            get => (string)GetValue("StringProperty");
            set => SetValue("StringProperty", value);
        }
    }

    internal class InternalObjectUnmanagedAccessor : UnmanagedAccessor, IInternalObjectAccessor
    {
        private int _intProperty;
        public int IntProperty
        {
            get => _intProperty;
            set
            {
                _intProperty = value;
                RaisePropertyChanged("IntProperty");
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
    
        public InternalObjectUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "IntProperty" => _intProperty,
                "StringProperty" => _stringProperty,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }
    
        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "IntProperty":
                    IntProperty = (int)val;
                    return;
                case "StringProperty":
                    StringProperty = (string)val;
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

