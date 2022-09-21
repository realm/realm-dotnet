﻿// <auto-generated />
using Realms.Tests.Database;
using Realms.Tests.Database.Generated;
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

namespace Realms.Tests.Database
{
    [Generated]
    [Woven(typeof(MixedProperties1ObjectHelper))]
    public partial class MixedProperties1 : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("MixedProperties1", isEmbedded: false)
        {
            Property.Primitive("Name", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "Name"),
            Property.ObjectList("Friends", "Person", managedName: "Friends"),
            Property.Primitive("Age", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "Age"),
            Property.ObjectList("Enemies", "Person", managedName: "Enemies"),
        }.Build();
        
        #region IRealmObject implementation
        
        private IMixedProperties1Accessor _accessor;
        
        IRealmAccessor IRealmObjectBase.Accessor => Accessor;
        
        internal IMixedProperties1Accessor Accessor => _accessor = _accessor ?? new MixedProperties1UnmanagedAccessor(typeof(MixedProperties1));
        
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
            var newAccessor = (IMixedProperties1Accessor)managedAccessor;
            var oldAccessor = _accessor as IMixedProperties1Accessor;
            _accessor = newAccessor;
        
            if (helper != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.Friends.Clear();
                    newAccessor.Enemies.Clear();
                }
                
                if(!skipDefaults || oldAccessor.Name != default(string))
                {
                    newAccessor.Name = oldAccessor.Name;
                }
                
                CollectionExtensions.PopulateCollection(oldAccessor.Friends, newAccessor.Friends, update, skipDefaults);
                
                if(!skipDefaults || oldAccessor.Age != default(int))
                {
                    newAccessor.Age = oldAccessor.Age;
                }
                
                CollectionExtensions.PopulateCollection(oldAccessor.Enemies, newAccessor.Enemies, update, skipDefaults);
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
        
        public static explicit operator MixedProperties1(RealmValue val) => val.AsRealmObject<MixedProperties1>();
        
        public static implicit operator RealmValue(MixedProperties1 val) => RealmValue.Object(val);
        
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
        private class MixedProperties1ObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }
        
            public ManagedAccessor CreateAccessor() => new MixedProperties1ManagedAccessor();
        
            public IRealmObjectBase CreateInstance()
            {
                return new MixedProperties1();
            }
        
            public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
            {
                value = null;
                return false;
            }
        }
    }
}

namespace Realms.Tests.Database.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IMixedProperties1Accessor : IRealmAccessor
    {
        string Name { get; set; }
        
        IList<Person> Friends { get; }
        
        int Age { get; set; }
        
        IList<Person> Enemies { get; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class MixedProperties1ManagedAccessor : ManagedAccessor, IMixedProperties1Accessor
    {
        public string Name
        {
            get => (string)GetValue("Name");
            set => SetValue("Name", value);
        }
        
        private IList<Person> _friends;
        public IList<Person> Friends
        {
            get
            {
                if (_friends == null)
                {
                    _friends = GetListValue<Person>("Friends");
                }
        
                return _friends;
            }
        }
        
        public int Age
        {
            get => (int)GetValue("Age");
            set => SetValue("Age", value);
        }
        
        private IList<Person> _enemies;
        public IList<Person> Enemies
        {
            get
            {
                if (_enemies == null)
                {
                    _enemies = GetListValue<Person>("Enemies");
                }
        
                return _enemies;
            }
        }
    }

    internal class MixedProperties1UnmanagedAccessor : UnmanagedAccessor, IMixedProperties1Accessor
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
        
        public IList<Person> Friends { get; } = new List<Person>();
        
        private int _age;
        public int Age
        {
            get => _age;
            set
            {
                _age = value;
                RaisePropertyChanged("Age");
            }
        }
        
        public IList<Person> Enemies { get; } = new List<Person>();
    
        public MixedProperties1UnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "Name" => _name,
                "Age" => _age,
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
                case "Age":
                    Age = (int)val;
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
            return propertyName switch
                        {
            "Friends" => (IList<T>)Friends,
            "Enemies" => (IList<T>)Enemies,
            
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

