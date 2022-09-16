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
    [Generated("ILevel2Accessor")]
    [Woven(typeof(Level2ObjectHelper))]
    public partial class Level2 : IRealmObject, INotifyPropertyChanged
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("Level2", isEmbedded: false)
        {
            Property.Primitive("IntValue", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Object("Level3", "Level3"),
        }.Build();
        
        #region IRealmObject implementation
        
        private ILevel2Accessor _accessor;
        
        IRealmAccessor IRealmObjectBase.Accessor => Accessor;
        
        internal ILevel2Accessor Accessor => _accessor = _accessor ?? new Level2UnmanagedAccessor(typeof(Level2));
        
        public bool IsManaged => Accessor.IsManaged;
        
        public bool IsValid => Accessor.IsValid;
        
        public bool IsFrozen => Accessor.IsFrozen;
        
        public Realm Realm => Accessor.Realm;
        
        public ObjectSchema ObjectSchema => Accessor.ObjectSchema;
        
        public RealmObjectBase.Dynamic DynamicApi => Accessor.DynamicApi;
        
        public int BacklinksCount => Accessor.BacklinksCount;
        
        
        
        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var newAccessor = (ILevel2Accessor)managedAccessor;
            var oldAccessor = _accessor as ILevel2Accessor;
            _accessor = newAccessor;
        
            if (helper != null)
            {
                
                newAccessor.IntValue = oldAccessor.IntValue;
                if(oldAccessor.Level3 != null)
                {
                    newAccessor.Realm.Add(oldAccessor.Level3, update);
                }
                newAccessor.Level3 = oldAccessor.Level3;
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
        
        public static explicit operator Level2(RealmValue val) => val.AsRealmObject<Level2>();
        
        public static implicit operator RealmValue(Level2 val) => RealmValue.Object(val);
        
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
        private class Level2ObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }
        
            public ManagedAccessor CreateAccessor() => new Level2ManagedAccessor();
        
            public IRealmObjectBase CreateInstance()
            {
                return new Level2();
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
    internal interface ILevel2Accessor : IRealmAccessor
    {
        int IntValue { get; set; }
        
        Level3 Level3 { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class Level2ManagedAccessor : ManagedAccessor, ILevel2Accessor
    {
        public int IntValue
        {
            get => (int)GetValue("IntValue");
            set => SetValue("IntValue", value);
        }
        
        public Level3 Level3
        {
            get => (Level3)GetValue("Level3");
            set => SetValue("Level3", value);
        }
    }

    internal class Level2UnmanagedAccessor : UnmanagedAccessor, ILevel2Accessor
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
        
        private Level3 _level3;
        public Level3 Level3
        {
            get => _level3;
            set
            {
                _level3 = value;
                RaisePropertyChanged("Level3");
            }
        }
    
        public Level2UnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "IntValue" => _intValue,
                "Level3" => _level3,
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
                case "Level3":
                    Level3 = (Level3)val;
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

