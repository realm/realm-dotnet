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
    [Generated("IRealmValueObjectAccessor")]
    [Woven(typeof(RealmValueObjectObjectHelper))]
    public partial class RealmValueObject : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("RealmValueObject", isEmbedded: false)
        {
            Property.Primitive("Id", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.RealmValue("RealmValueProperty"),
            Property.RealmValueList("RealmValueList"),
            Property.RealmValueSet("RealmValueSet"),
            Property.RealmValueDictionary("RealmValueDictionary"),
            Property.PrimitiveDictionary("TestDict", RealmValueType.Int, areElementsNullable: false),
        }.Build();
        
        #region IRealmObject implementation
        
        private IRealmValueObjectAccessor _accessor;
        
        IRealmAccessor IRealmObjectBase.Accessor => Accessor;
        
        internal IRealmValueObjectAccessor Accessor => _accessor = _accessor ?? new RealmValueObjectUnmanagedAccessor(typeof(RealmValueObject));
        
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
            var newAccessor = (IRealmValueObjectAccessor)managedAccessor;
            var oldAccessor = _accessor as IRealmValueObjectAccessor;
            _accessor = newAccessor;
        
            if (helper != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.RealmValueList.Clear();
                    newAccessor.RealmValueSet.Clear();
                    newAccessor.RealmValueDictionary.Clear();
                    newAccessor.TestDict.Clear();
                }
                
                if(!skipDefaults || oldAccessor.Id != default(int))
                {
                    newAccessor.Id = oldAccessor.Id;
                }
                newAccessor.RealmValueProperty = oldAccessor.RealmValueProperty;
                
                CollectionExtensions.PopulateCollection(oldAccessor.RealmValueList, newAccessor.RealmValueList, update, skipDefaults);
                
                
                CollectionExtensions.PopulateCollection(oldAccessor.RealmValueSet, newAccessor.RealmValueSet, update, skipDefaults);
                
                
                CollectionExtensions.PopulateCollection(oldAccessor.RealmValueDictionary, newAccessor.RealmValueDictionary, update, skipDefaults);
                
                
                CollectionExtensions.PopulateCollection(oldAccessor.TestDict, newAccessor.TestDict, update, skipDefaults);
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
        
        public static explicit operator RealmValueObject(RealmValue val) => val.AsRealmObject<RealmValueObject>();
        
        public static implicit operator RealmValue(RealmValueObject val) => RealmValue.Object(val);
        
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
        private class RealmValueObjectObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }
        
            public ManagedAccessor CreateAccessor() => new RealmValueObjectManagedAccessor();
        
            public IRealmObjectBase CreateInstance()
            {
                return new RealmValueObject();
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
    internal interface IRealmValueObjectAccessor : IRealmAccessor
    {
        int Id { get; set; }
        
        RealmValue RealmValueProperty { get; set; }
        
        IList<RealmValue> RealmValueList { get; }
        
        ISet<RealmValue> RealmValueSet { get; }
        
        IDictionary<string, RealmValue> RealmValueDictionary { get; }
        
        IDictionary<string, int> TestDict { get; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class RealmValueObjectManagedAccessor : ManagedAccessor, IRealmValueObjectAccessor
    {
        public int Id
        {
            get => (int)GetValue("Id");
            set => SetValue("Id", value);
        }
        
        public RealmValue RealmValueProperty
        {
            get => (RealmValue)GetValue("RealmValueProperty");
            set => SetValue("RealmValueProperty", value);
        }
        
        private IList<RealmValue> _realmValueList;
        public IList<RealmValue> RealmValueList
        {
            get
            {
                if (_realmValueList == null)
                {
                    _realmValueList = GetListValue<RealmValue>("RealmValueList");
                }
        
                return _realmValueList;
            }
        }
        
        private ISet<RealmValue> _realmValueSet;
        public ISet<RealmValue> RealmValueSet
        {
            get
            {
                if (_realmValueSet == null)
                {
                    _realmValueSet = GetSetValue<RealmValue>("RealmValueSet");
                }
        
                return _realmValueSet;
            }
        }
        
        private IDictionary<string, RealmValue> _realmValueDictionary;
        public IDictionary<string, RealmValue> RealmValueDictionary
        {
            get
            {
                if (_realmValueDictionary == null)
                {
                    _realmValueDictionary = GetDictionaryValue<RealmValue>("RealmValueDictionary");
                }
        
                return _realmValueDictionary;
            }
        }
        
        private IDictionary<string, int> _testDict;
        public IDictionary<string, int> TestDict
        {
            get
            {
                if (_testDict == null)
                {
                    _testDict = GetDictionaryValue<int>("TestDict");
                }
        
                return _testDict;
            }
        }
    }

    internal class RealmValueObjectUnmanagedAccessor : UnmanagedAccessor, IRealmValueObjectAccessor
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
        
        private RealmValue _realmValueProperty;
        public RealmValue RealmValueProperty
        {
            get => _realmValueProperty;
            set
            {
                _realmValueProperty = value;
                RaisePropertyChanged("RealmValueProperty");
            }
        }
        
        public IList<RealmValue> RealmValueList { get; } = new List<RealmValue>();
        
        public ISet<RealmValue> RealmValueSet { get; } = new HashSet<RealmValue>(RealmSet<RealmValue>.Comparer);
        
        public IDictionary<string, RealmValue> RealmValueDictionary { get; } = new Dictionary<string, RealmValue>();
        
        public IDictionary<string, int> TestDict { get; } = new Dictionary<string, int>();
    
        public RealmValueObjectUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "Id" => _id,
                "RealmValueProperty" => _realmValueProperty,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }
    
        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "Id":
                    Id = (int)val;
                    return;
                case "RealmValueProperty":
                    RealmValueProperty = (RealmValue)val;
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
            "RealmValueList" => (IList<T>)RealmValueList,
            
                            _ => throw new MissingMemberException($"The object does not have a Realm list property with name {propertyName}"),
                        };
        }
    
        public override ISet<T> GetSetValue<T>(string propertyName)
        {
            return propertyName switch
                        {
            "RealmValueSet" => (ISet<T>)RealmValueSet,
            
                            _ => throw new MissingMemberException($"The object does not have a Realm set property with name {propertyName}"),
                        };
        }
    
        public override IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
        {
            return propertyName switch
            {
                "RealmValueDictionary" => (IDictionary<string, TValue>)RealmValueDictionary,
                "TestDict" => (IDictionary<string, TValue>)TestDict,
                _ => throw new MissingMemberException($"The object does not have a Realm dictionary property with name {propertyName}"),
            };
        }
    }
}

