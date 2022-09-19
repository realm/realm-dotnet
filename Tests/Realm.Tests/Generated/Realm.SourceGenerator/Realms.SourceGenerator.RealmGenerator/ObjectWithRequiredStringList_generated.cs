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
    [Generated("IObjectWithRequiredStringListAccessor")]
    [Woven(typeof(ObjectWithRequiredStringListObjectHelper))]
    public partial class ObjectWithRequiredStringList : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("ObjectWithRequiredStringList", isEmbedded: false)
        {
            Property.PrimitiveList("Strings", RealmValueType.String, areElementsNullable: false),
        }.Build();
        
        #region IRealmObject implementation
        
        private IObjectWithRequiredStringListAccessor _accessor;
        
        IRealmAccessor IRealmObjectBase.Accessor => Accessor;
        
        internal IObjectWithRequiredStringListAccessor Accessor => _accessor = _accessor ?? new ObjectWithRequiredStringListUnmanagedAccessor(typeof(ObjectWithRequiredStringList));
        
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
            var newAccessor = (IObjectWithRequiredStringListAccessor)managedAccessor;
            var oldAccessor = _accessor as IObjectWithRequiredStringListAccessor;
            _accessor = newAccessor;
        
            if (helper != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.Strings.Clear();
                }
                
                
                CollectionExtensions.PopulateCollection(oldAccessor.Strings, newAccessor.Strings, update, skipDefaults);
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
        
        public static explicit operator ObjectWithRequiredStringList(RealmValue val) => val.AsRealmObject<ObjectWithRequiredStringList>();
        
        public static implicit operator RealmValue(ObjectWithRequiredStringList val) => RealmValue.Object(val);
        
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
        private class ObjectWithRequiredStringListObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }
        
            public ManagedAccessor CreateAccessor() => new ObjectWithRequiredStringListManagedAccessor();
        
            public IRealmObjectBase CreateInstance()
            {
                return new ObjectWithRequiredStringList();
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
    internal interface IObjectWithRequiredStringListAccessor : IRealmAccessor
    {
        IList<string> Strings { get; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class ObjectWithRequiredStringListManagedAccessor : ManagedAccessor, IObjectWithRequiredStringListAccessor
    {
        private IList<string> _strings;
        public IList<string> Strings
        {
            get
            {
                if (_strings == null)
                {
                    _strings = GetListValue<string>("Strings");
                }
        
                return _strings;
            }
        }
    }

    internal class ObjectWithRequiredStringListUnmanagedAccessor : UnmanagedAccessor, IObjectWithRequiredStringListAccessor
    {
        public IList<string> Strings { get; } = new List<string>();
    
        public ObjectWithRequiredStringListUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}");
        }
    
        public override void SetValue(string propertyName, RealmValue val)
        {
            throw new MissingMemberException($"The object does not have a settable Realm property with name {propertyName}");
        }
    
        public override void SetValueUnique(string propertyName, RealmValue val)
        {
            throw new InvalidOperationException("Cannot set the value of an non primary key property with SetValueUnique");
        }
    
        public override IList<T> GetListValue<T>(string propertyName)
        {
            return propertyName switch
                        {
            "Strings" => (IList<T>)Strings,
            
                            _ => throw new MissingMemberException($"The object does not have a Realm list property with name {propertyName}"),
                        };
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

