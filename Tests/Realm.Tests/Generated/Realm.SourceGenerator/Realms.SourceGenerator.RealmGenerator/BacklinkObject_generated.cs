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
    [Generated("IBacklinkObjectAccessor")]
    [Woven(typeof(BacklinkObjectObjectHelper))]
    public partial class BacklinkObject : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("BacklinkObject", isEmbedded: false)
        {
            Property.Primitive("BeforeBacklinks", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Backlinks("Links", "SomeClass", "BacklinkObject"),
            Property.Primitive("AfterBacklinks", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true),
        }.Build();
        
        #region IRealmObject implementation
        
        private IBacklinkObjectAccessor _accessor;
        
        IRealmAccessor IRealmObjectBase.Accessor => Accessor;
        
        internal IBacklinkObjectAccessor Accessor => _accessor = _accessor ?? new BacklinkObjectUnmanagedAccessor(typeof(BacklinkObject));
        
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
            var newAccessor = (IBacklinkObjectAccessor)managedAccessor;
            var oldAccessor = _accessor as IBacklinkObjectAccessor;
            _accessor = newAccessor;
        
            if (helper != null)
            {
                
                if(!skipDefaults || oldAccessor.BeforeBacklinks != default(string))
                {
                    newAccessor.BeforeBacklinks = oldAccessor.BeforeBacklinks;
                }
                if(!skipDefaults || oldAccessor.AfterBacklinks != default(string))
                {
                    newAccessor.AfterBacklinks = oldAccessor.AfterBacklinks;
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
        
        public static explicit operator BacklinkObject(RealmValue val) => val.AsRealmObject<BacklinkObject>();
        
        public static implicit operator RealmValue(BacklinkObject val) => RealmValue.Object(val);
        
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
        private class BacklinkObjectObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }
        
            public ManagedAccessor CreateAccessor() => new BacklinkObjectManagedAccessor();
        
            public IRealmObjectBase CreateInstance()
            {
                return new BacklinkObject();
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
    internal interface IBacklinkObjectAccessor : IRealmAccessor
    {
        string BeforeBacklinks { get; set; }
        
        IQueryable<SomeClass> Links { get; }
        
        string AfterBacklinks { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class BacklinkObjectManagedAccessor : ManagedAccessor, IBacklinkObjectAccessor
    {
        public string BeforeBacklinks
        {
            get => (string)GetValue("BeforeBacklinks");
            set => SetValue("BeforeBacklinks", value);
        }
        
        private IQueryable<SomeClass> _links;
        public IQueryable<SomeClass> Links
        {
            get
            {
                if (_links == null)
                {
                    _links = GetBacklinks<SomeClass>("Links");
                }
        
                return _links;
            }
        }
        
        public string AfterBacklinks
        {
            get => (string)GetValue("AfterBacklinks");
            set => SetValue("AfterBacklinks", value);
        }
    }

    internal class BacklinkObjectUnmanagedAccessor : UnmanagedAccessor, IBacklinkObjectAccessor
    {
        private string _beforeBacklinks;
        public string BeforeBacklinks
        {
            get => _beforeBacklinks;
            set
            {
                _beforeBacklinks = value;
                RaisePropertyChanged("BeforeBacklinks");
            }
        }
        
        public IQueryable<SomeClass> Links => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects.");
        
        private string _afterBacklinks;
        public string AfterBacklinks
        {
            get => _afterBacklinks;
            set
            {
                _afterBacklinks = value;
                RaisePropertyChanged("AfterBacklinks");
            }
        }
    
        public BacklinkObjectUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "BeforeBacklinks" => _beforeBacklinks,
                "Links" => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects."),
                "AfterBacklinks" => _afterBacklinks,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }
    
        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "BeforeBacklinks":
                    BeforeBacklinks = (string)val;
                    return;
                case "AfterBacklinks":
                    AfterBacklinks = (string)val;
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
