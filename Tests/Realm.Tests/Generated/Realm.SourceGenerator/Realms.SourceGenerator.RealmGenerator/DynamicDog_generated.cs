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
    [Generated("IDynamicDogAccessor")]
    [Woven(typeof(DynamicDogObjectHelper))]
    public partial class DynamicDog : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("DynamicDog", isEmbedded: false)
        {
            Property.Primitive("Name", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Primitive("Color", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Primitive("Vaccinated", RealmValueType.Bool, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Backlinks("Owners", "DynamicOwner", "Dogs"),
        }.Build();
        
        #region IRealmObject implementation
        
        private IDynamicDogAccessor _accessor;
        
        IRealmAccessor IRealmObjectBase.Accessor => Accessor;
        
        internal IDynamicDogAccessor Accessor => _accessor = _accessor ?? new DynamicDogUnmanagedAccessor(typeof(DynamicDog));
        
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
            var newAccessor = (IDynamicDogAccessor)managedAccessor;
            var oldAccessor = _accessor as IDynamicDogAccessor;
            _accessor = newAccessor;
        
            if (helper != null)
            {
                
                if(!skipDefaults || oldAccessor.Name != default(string))
                {
                    newAccessor.Name = oldAccessor.Name;
                }
                if(!skipDefaults || oldAccessor.Color != default(string))
                {
                    newAccessor.Color = oldAccessor.Color;
                }
                if(!skipDefaults || oldAccessor.Vaccinated != default(bool))
                {
                    newAccessor.Vaccinated = oldAccessor.Vaccinated;
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
        
        public static explicit operator DynamicDog(RealmValue val) => val.AsRealmObject<DynamicDog>();
        
        public static implicit operator RealmValue(DynamicDog val) => RealmValue.Object(val);
        
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
        private class DynamicDogObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }
        
            public ManagedAccessor CreateAccessor() => new DynamicDogManagedAccessor();
        
            public IRealmObjectBase CreateInstance()
            {
                return new DynamicDog();
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
    internal interface IDynamicDogAccessor : IRealmAccessor
    {
        string Name { get; set; }
        
        string Color { get; set; }
        
        bool Vaccinated { get; set; }
        
        IQueryable<DynamicOwner> Owners { get; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class DynamicDogManagedAccessor : ManagedAccessor, IDynamicDogAccessor
    {
        public string Name
        {
            get => (string)GetValue("Name");
            set => SetValue("Name", value);
        }
        
        public string Color
        {
            get => (string)GetValue("Color");
            set => SetValue("Color", value);
        }
        
        public bool Vaccinated
        {
            get => (bool)GetValue("Vaccinated");
            set => SetValue("Vaccinated", value);
        }
        
        private IQueryable<DynamicOwner> _owners;
        public IQueryable<DynamicOwner> Owners
        {
            get
            {
                if (_owners == null)
                {
                    _owners = GetBacklinks<DynamicOwner>("Owners");
                }
        
                return _owners;
            }
        }
    }

    internal class DynamicDogUnmanagedAccessor : UnmanagedAccessor, IDynamicDogAccessor
    {
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }
        
        private string _color;
        public string Color
        {
            get => _color;
            set
            {
                _color = value;
                RaisePropertyChanged("Color");
            }
        }
        
        private bool _vaccinated;
        public bool Vaccinated
        {
            get => _vaccinated;
            set
            {
                _vaccinated = value;
                RaisePropertyChanged("Vaccinated");
            }
        }
        
        public IQueryable<DynamicOwner> Owners => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects.");
    
        public DynamicDogUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "Name" => _name,
                "Color" => _color,
                "Vaccinated" => _vaccinated,
                "Owners" => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects."),
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }
    
        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "Name":
                    Name = (string)val;
                    return;
                case "Color":
                    Color = (string)val;
                    return;
                case "Vaccinated":
                    Vaccinated = (bool)val;
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

