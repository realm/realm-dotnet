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
    [Generated("IObjectWithObjectPropertiesAccessor")]
    [Woven(typeof(ObjectWithObjectPropertiesObjectHelper))]
    public partial class ObjectWithObjectProperties : IRealmObject, INotifyPropertyChanged
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("ObjectWithObjectProperties", isEmbedded: false)
        {
            Property.Object("StandaloneObject", "IntPropertyObject"),
            Property.Object("EmbeddedObject", "EmbeddedIntPropertyObject"),
        }.Build();
        
        #region IRealmObject implementation
        
        private IObjectWithObjectPropertiesAccessor _accessor;
        
        IRealmAccessor IRealmObjectBase.Accessor => Accessor;
        
        internal IObjectWithObjectPropertiesAccessor Accessor => _accessor = _accessor ?? new ObjectWithObjectPropertiesUnmanagedAccessor(typeof(ObjectWithObjectProperties));
        
        public bool IsManaged => Accessor.IsManaged;
        
        public bool IsValid => Accessor.IsValid;
        
        public bool IsFrozen => Accessor.IsFrozen;
        
        public Realm Realm => Accessor.Realm;
        
        public ObjectSchema ObjectSchema => Accessor.ObjectSchema;
        
        public RealmObjectBase.Dynamic DynamicApi => Accessor.DynamicApi;
        
        public int BacklinksCount => Accessor.BacklinksCount;
        
        
        
        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var newAccessor = (IObjectWithObjectPropertiesAccessor)managedAccessor;
            var oldAccessor = _accessor as IObjectWithObjectPropertiesAccessor;
            _accessor = newAccessor;
        
            if (helper != null)
            {
                
                if(oldAccessor.StandaloneObject != null)
                {
                    newAccessor.Realm.Add(oldAccessor.StandaloneObject, update);
                }
                newAccessor.StandaloneObject = oldAccessor.StandaloneObject;
                newAccessor.EmbeddedObject = oldAccessor.EmbeddedObject;
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
        
        public static explicit operator ObjectWithObjectProperties(RealmValue val) => val.AsRealmObject<ObjectWithObjectProperties>();
        
        public static implicit operator RealmValue(ObjectWithObjectProperties val) => RealmValue.Object(val);
        
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
        private class ObjectWithObjectPropertiesObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }
        
            public ManagedAccessor CreateAccessor() => new ObjectWithObjectPropertiesManagedAccessor();
        
            public IRealmObjectBase CreateInstance()
            {
                return new ObjectWithObjectProperties();
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
    internal interface IObjectWithObjectPropertiesAccessor : IRealmAccessor
    {
        IntPropertyObject StandaloneObject { get; set; }
        
        EmbeddedIntPropertyObject EmbeddedObject { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class ObjectWithObjectPropertiesManagedAccessor : ManagedAccessor, IObjectWithObjectPropertiesAccessor
    {
        public IntPropertyObject StandaloneObject
        {
            get => (IntPropertyObject)GetValue("StandaloneObject");
            set => SetValue("StandaloneObject", value);
        }
        
        public EmbeddedIntPropertyObject EmbeddedObject
        {
            get => (EmbeddedIntPropertyObject)GetValue("EmbeddedObject");
            set => SetValue("EmbeddedObject", value);
        }
    }

    internal class ObjectWithObjectPropertiesUnmanagedAccessor : UnmanagedAccessor, IObjectWithObjectPropertiesAccessor
    {
        private IntPropertyObject _standaloneObject;
        public IntPropertyObject StandaloneObject
        {
            get => _standaloneObject;
            set
            {
                _standaloneObject = value;
                RaisePropertyChanged("StandaloneObject");
            }
        }
        
        private EmbeddedIntPropertyObject _embeddedObject;
        public EmbeddedIntPropertyObject EmbeddedObject
        {
            get => _embeddedObject;
            set
            {
                _embeddedObject = value;
                RaisePropertyChanged("EmbeddedObject");
            }
        }
    
        public ObjectWithObjectPropertiesUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "StandaloneObject" => _standaloneObject,
                "EmbeddedObject" => _embeddedObject,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }
    
        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "StandaloneObject":
                    StandaloneObject = (IntPropertyObject)val;
                    return;
                case "EmbeddedObject":
                    EmbeddedObject = (EmbeddedIntPropertyObject)val;
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

