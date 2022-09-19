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
    [Generated("IOwnerAccessor")]
    [Woven(typeof(OwnerObjectHelper))]
    public partial class Owner : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("Owner", isEmbedded: false)
        {
            Property.Primitive("Name", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.Object("TopDog", "Dog"),
            Property.ObjectList("ListOfDogs", "Dog"),
            Property.ObjectSet("SetOfDogs", "Dog"),
        }.Build();
        
        #region IRealmObject implementation
        
        private IOwnerAccessor _accessor;
        
        IRealmAccessor IRealmObjectBase.Accessor => Accessor;
        
        internal IOwnerAccessor Accessor => _accessor = _accessor ?? new OwnerUnmanagedAccessor(typeof(Owner));
        
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
            var newAccessor = (IOwnerAccessor)managedAccessor;
            var oldAccessor = _accessor as IOwnerAccessor;
            _accessor = newAccessor;
        
            if (helper != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.ListOfDogs.Clear();
                    newAccessor.SetOfDogs.Clear();
                }
                
                if(!skipDefaults || oldAccessor.Name != default(string))
                {
                    newAccessor.Name = oldAccessor.Name;
                }
                if(oldAccessor.TopDog != null)
                {
                    newAccessor.Realm.Add(oldAccessor.TopDog, update);
                }
                newAccessor.TopDog = oldAccessor.TopDog;
                
                CollectionExtensions.PopulateCollection(oldAccessor.ListOfDogs, newAccessor.ListOfDogs, update, skipDefaults);
                
                
                CollectionExtensions.PopulateCollection(oldAccessor.SetOfDogs, newAccessor.SetOfDogs, update, skipDefaults);
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
        
        public static explicit operator Owner(RealmValue val) => val.AsRealmObject<Owner>();
        
        public static implicit operator RealmValue(Owner val) => RealmValue.Object(val);
        
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
        private class OwnerObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }
        
            public ManagedAccessor CreateAccessor() => new OwnerManagedAccessor();
        
            public IRealmObjectBase CreateInstance()
            {
                return new Owner();
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
    internal interface IOwnerAccessor : IRealmAccessor
    {
        string Name { get; set; }
        
        Dog TopDog { get; set; }
        
        IList<Dog> ListOfDogs { get; }
        
        ISet<Dog> SetOfDogs { get; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class OwnerManagedAccessor : ManagedAccessor, IOwnerAccessor
    {
        public string Name
        {
            get => (string)GetValue("Name");
            set => SetValue("Name", value);
        }
        
        public Dog TopDog
        {
            get => (Dog)GetValue("TopDog");
            set => SetValue("TopDog", value);
        }
        
        private IList<Dog> _listOfDogs;
        public IList<Dog> ListOfDogs
        {
            get
            {
                if (_listOfDogs == null)
                {
                    _listOfDogs = GetListValue<Dog>("ListOfDogs");
                }
        
                return _listOfDogs;
            }
        }
        
        private ISet<Dog> _setOfDogs;
        public ISet<Dog> SetOfDogs
        {
            get
            {
                if (_setOfDogs == null)
                {
                    _setOfDogs = GetSetValue<Dog>("SetOfDogs");
                }
        
                return _setOfDogs;
            }
        }
    }

    internal class OwnerUnmanagedAccessor : UnmanagedAccessor, IOwnerAccessor
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
        
        private Dog _topDog;
        public Dog TopDog
        {
            get => _topDog;
            set
            {
                _topDog = value;
                RaisePropertyChanged("TopDog");
            }
        }
        
        public IList<Dog> ListOfDogs { get; } = new List<Dog>();
        
        public ISet<Dog> SetOfDogs { get; } = new HashSet<Dog>(RealmSet<Dog>.Comparer);
    
        public OwnerUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "Name" => _name,
                "TopDog" => _topDog,
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
                case "TopDog":
                    TopDog = (Dog)val;
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
            "ListOfDogs" => (IList<T>)ListOfDogs,
            
                            _ => throw new MissingMemberException($"The object does not have a Realm list property with name {propertyName}"),
                        };
        }
    
        public override ISet<T> GetSetValue<T>(string propertyName)
        {
            return propertyName switch
                        {
            "SetOfDogs" => (ISet<T>)SetOfDogs,
            
                            _ => throw new MissingMemberException($"The object does not have a Realm set property with name {propertyName}"),
                        };
        }
    
        public override IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
        {
            throw new MissingMemberException($"The object does not have a Realm dictionary property with name {propertyName}");
        }
    }
}

