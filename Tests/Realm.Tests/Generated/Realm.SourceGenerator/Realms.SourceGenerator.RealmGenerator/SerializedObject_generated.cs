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
    [Generated("ISerializedObjectAccessor")]
    [Woven(typeof(SerializedObjectObjectHelper))]
    public partial class SerializedObject : IRealmObject, INotifyPropertyChanged
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("SerializedObject", isEmbedded: false)
        {
            Property.Primitive("IntValue", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("Name", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true),
            Property.PrimitiveDictionary("Dict", RealmValueType.Int, areElementsNullable: false),
            Property.PrimitiveList("List", RealmValueType.String, areElementsNullable: true),
            Property.PrimitiveSet("Set", RealmValueType.String, areElementsNullable: true),
        }.Build();
        
        #region IRealmObject implementation
        
        private ISerializedObjectAccessor _accessor;
        
        IRealmAccessor IRealmObjectBase.Accessor => Accessor;
        
        internal ISerializedObjectAccessor Accessor => _accessor = _accessor ?? new SerializedObjectUnmanagedAccessor(typeof(SerializedObject));
        
        public bool IsManaged => Accessor.IsManaged;
        
        public bool IsValid => Accessor.IsValid;
        
        public bool IsFrozen => Accessor.IsFrozen;
        
        public Realm Realm => Accessor.Realm;
        
        public ObjectSchema ObjectSchema => Accessor.ObjectSchema;
        
        public RealmObjectBase.Dynamic DynamicApi => Accessor.DynamicApi;
        
        public int BacklinksCount => Accessor.BacklinksCount;
        
        
        
        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var newAccessor = (ISerializedObjectAccessor)managedAccessor;
            var oldAccessor = _accessor as ISerializedObjectAccessor;
            _accessor = newAccessor;
        
            if (helper != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.Dict.Clear();
                    newAccessor.List.Clear();
                    newAccessor.Set.Clear();
                }
                
                if(!skipDefaults || oldAccessor.IntValue != default(int))
                {
                    newAccessor.IntValue = oldAccessor.IntValue;
                }
                if(!skipDefaults || oldAccessor.Name != default(string))
                {
                    newAccessor.Name = oldAccessor.Name;
                }
                foreach(var val in oldAccessor.Dict)
                {
                    
                    newAccessor.Dict.Add(val);
                }
                foreach(var val in oldAccessor.List)
                {
                    
                    newAccessor.List.Add(val);
                }
                foreach(var val in oldAccessor.Set)
                {
                    
                    newAccessor.Set.Add(val);
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
        
        public static explicit operator SerializedObject(RealmValue val) => val.AsRealmObject<SerializedObject>();
        
        public static implicit operator RealmValue(SerializedObject val) => RealmValue.Object(val);
        
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
        private class SerializedObjectObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }
        
            public ManagedAccessor CreateAccessor() => new SerializedObjectManagedAccessor();
        
            public IRealmObjectBase CreateInstance()
            {
                return new SerializedObject();
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
    internal interface ISerializedObjectAccessor : IRealmAccessor
    {
        int IntValue { get; set; }
        
        string Name { get; set; }
        
        IDictionary<string, int> Dict { get; }
        
        IList<string> List { get; }
        
        ISet<string> Set { get; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class SerializedObjectManagedAccessor : ManagedAccessor, ISerializedObjectAccessor
    {
        public int IntValue
        {
            get => (int)GetValue("IntValue");
            set => SetValue("IntValue", value);
        }
        
        public string Name
        {
            get => (string)GetValue("Name");
            set => SetValue("Name", value);
        }
        
        private IDictionary<string, int> _dict;
        public IDictionary<string, int> Dict
        {
            get
            {
                if (_dict == null)
                {
                    _dict = GetDictionaryValue<int>("Dict");
                }
        
                return _dict;
            }
        }
        
        private IList<string> _list;
        public IList<string> List
        {
            get
            {
                if (_list == null)
                {
                    _list = GetListValue<string>("List");
                }
        
                return _list;
            }
        }
        
        private ISet<string> _set;
        public ISet<string> Set
        {
            get
            {
                if (_set == null)
                {
                    _set = GetSetValue<string>("Set");
                }
        
                return _set;
            }
        }
    }

    internal class SerializedObjectUnmanagedAccessor : UnmanagedAccessor, ISerializedObjectAccessor
    {
        private int _intValue;
        public int IntValue
        {
            get => _intValue;
            set
            {
                _intValue = value;
                RaisePropertyChanged("IntValue");
            }
        }
        
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
        
        public IDictionary<string, int> Dict { get; } = new Dictionary<string, int>();
        
        public IList<string> List { get; } = new List<string>();
        
        public ISet<string> Set { get; } = new HashSet<string>(RealmSet<string>.Comparer);
    
        public SerializedObjectUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "IntValue" => _intValue,
                "Name" => _name,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }
    
        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "IntValue":
                    IntValue = (int)val;
                    return;
                case "Name":
                    Name = (string)val;
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
            "List" => (IList<T>)List,
            
                            _ => throw new MissingMemberException($"The object does not have a Realm list property with name {propertyName}"),
                        };
        }
    
        public override ISet<T> GetSetValue<T>(string propertyName)
        {
            return propertyName switch
                        {
            "Set" => (ISet<T>)Set,
            
                            _ => throw new MissingMemberException($"The object does not have a Realm set property with name {propertyName}"),
                        };
        }
    
        public override IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
        {
            return propertyName switch
            {
                "Dict" => (IDictionary<string, TValue>)Dict,
                _ => throw new MissingMemberException($"The object does not have a Realm dictionary property with name {propertyName}"),
            };
        }
    }
}

