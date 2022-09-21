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
    [Generated("IEmbeddedGuidTypeAccessor")]
    [Woven(typeof(EmbeddedGuidTypeObjectHelper))]
    public partial class EmbeddedGuidType : IEmbeddedObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("EmbeddedGuidType", isEmbedded: true)
        {
            Property.Primitive("RegularProperty", RealmValueType.Guid, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.PrimitiveList("GuidList", RealmValueType.Guid, areElementsNullable: false),
            Property.PrimitiveSet("GuidSet", RealmValueType.Guid, areElementsNullable: false),
            Property.PrimitiveDictionary("GuidDict", RealmValueType.Guid, areElementsNullable: false),
            Property.Primitive("OptionalProperty", RealmValueType.Guid, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.PrimitiveList("OptionalList", RealmValueType.Guid, areElementsNullable: true),
            Property.PrimitiveSet("OptionalSet", RealmValueType.Guid, areElementsNullable: true),
            Property.PrimitiveDictionary("OptionalDict", RealmValueType.Guid, areElementsNullable: true),
            Property.Object("LinkProperty", "GuidType"),
            Property.RealmValue("MixedProperty"),
            Property.RealmValueList("MixedList"),
            Property.RealmValueSet("MixedSet"),
            Property.RealmValueDictionary("MixedDict"),
        }.Build();
        
        #region IEmbeddedObject implementation
        
        private IEmbeddedGuidTypeAccessor _accessor;
        
        IRealmAccessor IRealmObjectBase.Accessor => Accessor;
        
        internal IEmbeddedGuidTypeAccessor Accessor => _accessor = _accessor ?? new EmbeddedGuidTypeUnmanagedAccessor(typeof(EmbeddedGuidType));
        
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
            var newAccessor = (IEmbeddedGuidTypeAccessor)managedAccessor;
            var oldAccessor = _accessor as IEmbeddedGuidTypeAccessor;
            _accessor = newAccessor;
        
            if (helper != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.GuidList.Clear();
                    newAccessor.GuidSet.Clear();
                    newAccessor.GuidDict.Clear();
                    newAccessor.OptionalList.Clear();
                    newAccessor.OptionalSet.Clear();
                    newAccessor.OptionalDict.Clear();
                    newAccessor.MixedList.Clear();
                    newAccessor.MixedSet.Clear();
                    newAccessor.MixedDict.Clear();
                }
                
                newAccessor.RegularProperty = oldAccessor.RegularProperty;
                
                CollectionExtensions.PopulateCollection(oldAccessor.GuidList, newAccessor.GuidList, update, skipDefaults);
                
                
                CollectionExtensions.PopulateCollection(oldAccessor.GuidSet, newAccessor.GuidSet, update, skipDefaults);
                
                
                CollectionExtensions.PopulateCollection(oldAccessor.GuidDict, newAccessor.GuidDict, update, skipDefaults);
                
                newAccessor.OptionalProperty = oldAccessor.OptionalProperty;
                
                CollectionExtensions.PopulateCollection(oldAccessor.OptionalList, newAccessor.OptionalList, update, skipDefaults);
                
                
                CollectionExtensions.PopulateCollection(oldAccessor.OptionalSet, newAccessor.OptionalSet, update, skipDefaults);
                
                
                CollectionExtensions.PopulateCollection(oldAccessor.OptionalDict, newAccessor.OptionalDict, update, skipDefaults);
                
                if(oldAccessor.LinkProperty != null)
                {
                    newAccessor.Realm.Add(oldAccessor.LinkProperty, update);
                }
                newAccessor.LinkProperty = oldAccessor.LinkProperty;
                newAccessor.MixedProperty = oldAccessor.MixedProperty;
                
                CollectionExtensions.PopulateCollection(oldAccessor.MixedList, newAccessor.MixedList, update, skipDefaults);
                
                
                CollectionExtensions.PopulateCollection(oldAccessor.MixedSet, newAccessor.MixedSet, update, skipDefaults);
                
                
                CollectionExtensions.PopulateCollection(oldAccessor.MixedDict, newAccessor.MixedDict, update, skipDefaults);
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
        
        public static explicit operator EmbeddedGuidType(RealmValue val) => val.AsRealmObject<EmbeddedGuidType>();
        
        public static implicit operator RealmValue(EmbeddedGuidType val) => RealmValue.Object(val);
        
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
        private class EmbeddedGuidTypeObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }
        
            public ManagedAccessor CreateAccessor() => new EmbeddedGuidTypeManagedAccessor();
        
            public IRealmObjectBase CreateInstance()
            {
                return new EmbeddedGuidType();
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
    internal interface IEmbeddedGuidTypeAccessor : IRealmAccessor
    {
        Guid RegularProperty { get; set; }
        
        IList<Guid> GuidList { get; }
        
        ISet<Guid> GuidSet { get; }
        
        IDictionary<string, Guid> GuidDict { get; }
        
        Guid? OptionalProperty { get; set; }
        
        IList<Guid?> OptionalList { get; }
        
        ISet<Guid?> OptionalSet { get; }
        
        IDictionary<string, Guid?> OptionalDict { get; }
        
        GuidType LinkProperty { get; set; }
        
        RealmValue MixedProperty { get; set; }
        
        IList<RealmValue> MixedList { get; }
        
        ISet<RealmValue> MixedSet { get; }
        
        IDictionary<string, RealmValue> MixedDict { get; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class EmbeddedGuidTypeManagedAccessor : ManagedAccessor, IEmbeddedGuidTypeAccessor
    {
        public Guid RegularProperty
        {
            get => (Guid)GetValue("RegularProperty");
            set => SetValue("RegularProperty", value);
        }
        
        private IList<Guid> _guidList;
        public IList<Guid> GuidList
        {
            get
            {
                if (_guidList == null)
                {
                    _guidList = GetListValue<Guid>("GuidList");
                }
        
                return _guidList;
            }
        }
        
        private ISet<Guid> _guidSet;
        public ISet<Guid> GuidSet
        {
            get
            {
                if (_guidSet == null)
                {
                    _guidSet = GetSetValue<Guid>("GuidSet");
                }
        
                return _guidSet;
            }
        }
        
        private IDictionary<string, Guid> _guidDict;
        public IDictionary<string, Guid> GuidDict
        {
            get
            {
                if (_guidDict == null)
                {
                    _guidDict = GetDictionaryValue<Guid>("GuidDict");
                }
        
                return _guidDict;
            }
        }
        
        public Guid? OptionalProperty
        {
            get => (Guid?)GetValue("OptionalProperty");
            set => SetValue("OptionalProperty", value);
        }
        
        private IList<Guid?> _optionalList;
        public IList<Guid?> OptionalList
        {
            get
            {
                if (_optionalList == null)
                {
                    _optionalList = GetListValue<Guid?>("OptionalList");
                }
        
                return _optionalList;
            }
        }
        
        private ISet<Guid?> _optionalSet;
        public ISet<Guid?> OptionalSet
        {
            get
            {
                if (_optionalSet == null)
                {
                    _optionalSet = GetSetValue<Guid?>("OptionalSet");
                }
        
                return _optionalSet;
            }
        }
        
        private IDictionary<string, Guid?> _optionalDict;
        public IDictionary<string, Guid?> OptionalDict
        {
            get
            {
                if (_optionalDict == null)
                {
                    _optionalDict = GetDictionaryValue<Guid?>("OptionalDict");
                }
        
                return _optionalDict;
            }
        }
        
        public GuidType LinkProperty
        {
            get => (GuidType)GetValue("LinkProperty");
            set => SetValue("LinkProperty", value);
        }
        
        public RealmValue MixedProperty
        {
            get => (RealmValue)GetValue("MixedProperty");
            set => SetValue("MixedProperty", value);
        }
        
        private IList<RealmValue> _mixedList;
        public IList<RealmValue> MixedList
        {
            get
            {
                if (_mixedList == null)
                {
                    _mixedList = GetListValue<RealmValue>("MixedList");
                }
        
                return _mixedList;
            }
        }
        
        private ISet<RealmValue> _mixedSet;
        public ISet<RealmValue> MixedSet
        {
            get
            {
                if (_mixedSet == null)
                {
                    _mixedSet = GetSetValue<RealmValue>("MixedSet");
                }
        
                return _mixedSet;
            }
        }
        
        private IDictionary<string, RealmValue> _mixedDict;
        public IDictionary<string, RealmValue> MixedDict
        {
            get
            {
                if (_mixedDict == null)
                {
                    _mixedDict = GetDictionaryValue<RealmValue>("MixedDict");
                }
        
                return _mixedDict;
            }
        }
    }

    internal class EmbeddedGuidTypeUnmanagedAccessor : UnmanagedAccessor, IEmbeddedGuidTypeAccessor
    {
        private Guid _regularProperty;
        public Guid RegularProperty
        {
            get => _regularProperty;
            set
            {
                _regularProperty = value;
                RaisePropertyChanged("RegularProperty");
            }
        }
        
        public IList<Guid> GuidList { get; } = new List<Guid>();
        
        public ISet<Guid> GuidSet { get; } = new HashSet<Guid>(RealmSet<Guid>.Comparer);
        
        public IDictionary<string, Guid> GuidDict { get; } = new Dictionary<string, Guid>();
        
        private Guid? _optionalProperty;
        public Guid? OptionalProperty
        {
            get => _optionalProperty;
            set
            {
                _optionalProperty = value;
                RaisePropertyChanged("OptionalProperty");
            }
        }
        
        public IList<Guid?> OptionalList { get; } = new List<Guid?>();
        
        public ISet<Guid?> OptionalSet { get; } = new HashSet<Guid?>(RealmSet<Guid?>.Comparer);
        
        public IDictionary<string, Guid?> OptionalDict { get; } = new Dictionary<string, Guid?>();
        
        private GuidType _linkProperty;
        public GuidType LinkProperty
        {
            get => _linkProperty;
            set
            {
                _linkProperty = value;
                RaisePropertyChanged("LinkProperty");
            }
        }
        
        private RealmValue _mixedProperty;
        public RealmValue MixedProperty
        {
            get => _mixedProperty;
            set
            {
                _mixedProperty = value;
                RaisePropertyChanged("MixedProperty");
            }
        }
        
        public IList<RealmValue> MixedList { get; } = new List<RealmValue>();
        
        public ISet<RealmValue> MixedSet { get; } = new HashSet<RealmValue>(RealmSet<RealmValue>.Comparer);
        
        public IDictionary<string, RealmValue> MixedDict { get; } = new Dictionary<string, RealmValue>();
    
        public EmbeddedGuidTypeUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "RegularProperty" => _regularProperty,
                "OptionalProperty" => _optionalProperty,
                "LinkProperty" => _linkProperty,
                "MixedProperty" => _mixedProperty,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }
    
        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "RegularProperty":
                    RegularProperty = (Guid)val;
                    return;
                case "OptionalProperty":
                    OptionalProperty = (Guid?)val;
                    return;
                case "LinkProperty":
                    LinkProperty = (GuidType)val;
                    return;
                case "MixedProperty":
                    MixedProperty = (RealmValue)val;
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
            "GuidList" => (IList<T>)GuidList,
            "OptionalList" => (IList<T>)OptionalList,
            "MixedList" => (IList<T>)MixedList,
            
                            _ => throw new MissingMemberException($"The object does not have a Realm list property with name {propertyName}"),
                        };
        }
    
        public override ISet<T> GetSetValue<T>(string propertyName)
        {
            return propertyName switch
                        {
            "GuidSet" => (ISet<T>)GuidSet,
            "OptionalSet" => (ISet<T>)OptionalSet,
            "MixedSet" => (ISet<T>)MixedSet,
            
                            _ => throw new MissingMemberException($"The object does not have a Realm set property with name {propertyName}"),
                        };
        }
    
        public override IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
        {
            return propertyName switch
            {
                "GuidDict" => (IDictionary<string, TValue>)GuidDict,
                "OptionalDict" => (IDictionary<string, TValue>)OptionalDict,
                "MixedDict" => (IDictionary<string, TValue>)MixedDict,
                _ => throw new MissingMemberException($"The object does not have a Realm dictionary property with name {propertyName}"),
            };
        }
    }
}

