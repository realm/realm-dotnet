﻿using SourceGeneratorPlayground;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using Realms;
using Realms.Weaving;
using Realms.Generated;
using Realms.Schema;

namespace SourceGeneratorPlayground
{
    [Generated]
    [Woven(typeof(RealmObjObjectHelper))]
    public partial class RealmObj : IRealmObject, INotifyPropertyChanged
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("RealmObj", isEmbedded: false)
        {
            Property.Primitive("Id", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false),
        }.Build();
        
        #region IRealmObject implementation
        
        private IRealmObjAccessor _accessor;
        
        IRealmAccessor IRealmObjectBase.Accessor => Accessor;
        
        internal IRealmObjAccessor Accessor => _accessor = _accessor ?? new RealmObjUnmanagedAccessor(typeof(RealmObj));
        
        public bool IsManaged => Accessor.IsManaged;
        
        public bool IsValid => Accessor.IsValid;
        
        public bool IsFrozen => Accessor.IsFrozen;
        
        public Realm Realm => Accessor.Realm;
        
        public ObjectSchema ObjectSchema => Accessor.ObjectSchema;
        
        
        
        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var newAccessor = (IRealmObjAccessor)managedAccessor;
        
            if (helper != null)
            {
                var oldAccessor = (IRealmObjAccessor)Accessor;
                
                newAccessor.Id = oldAccessor.Id;
            }
        
            _accessor = newAccessor;
        
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
        
        public static explicit operator RealmObj(RealmValue val) => val.AsRealmObject<RealmObj>();
        
        public static implicit operator RealmValue(RealmObj val) => RealmValue.Object(val);
        
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
        private class RealmObjObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }
        
            public ManagedAccessor CreateAccessor() => new RealmObjManagedAccessor();
        
            public IRealmObjectBase CreateInstance()
            {
                return new RealmObj();
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
    internal interface IRealmObjAccessor : IRealmAccessor
    {
        int Id { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class RealmObjManagedAccessor : ManagedAccessor, IRealmObjAccessor
    {
        public int Id
        {
            get => (int)GetValue("Id");
            set => SetValue("Id", value);
        }
    }

    internal class RealmObjUnmanagedAccessor : UnmanagedAccessor, IRealmObjAccessor
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
    
        public RealmObjUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "Id" => _id,
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