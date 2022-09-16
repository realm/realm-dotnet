﻿// <auto-generated />
using Realms.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using Realms;
using Realms.Weaving;
using Realms.Generated;
using Realms.Schema;
using MongoDB.Bson;

namespace Realms.Tests
{
    [Generated("IIntPropertyObjectAccessor")]
    [Woven(typeof(IntPropertyObjectObjectHelper))]
    public partial class IntPropertyObject : IRealmObject, INotifyPropertyChanged
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("IntPropertyObject", isEmbedded: false)
        {
            Property.Primitive("_id", RealmValueType.ObjectId, isPrimaryKey: true, isIndexed: false, isNullable: false),
            Property.Primitive("Int", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("GuidProperty", RealmValueType.Guid, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Backlinks("ContainingCollections", "SyncCollectionsObject", "ObjectList"),
        }.Build();
        
        #region IRealmObject implementation
        
        private IIntPropertyObjectAccessor _accessor;
        
        IRealmAccessor IRealmObjectBase.Accessor => Accessor;
        
        internal IIntPropertyObjectAccessor Accessor => _accessor = _accessor ?? new IntPropertyObjectUnmanagedAccessor(typeof(IntPropertyObject));
        
        public bool IsManaged => Accessor.IsManaged;
        
        public bool IsValid => Accessor.IsValid;
        
        public bool IsFrozen => Accessor.IsFrozen;
        
        public Realm Realm => Accessor.Realm;
        
        public ObjectSchema ObjectSchema => Accessor.ObjectSchema;
        
        public RealmObjectBase.Dynamic DynamicApi => Accessor.DynamicApi;
        
        public int BacklinksCount => Accessor.BacklinksCount;
        
        
        
        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var newAccessor = (IIntPropertyObjectAccessor)managedAccessor;
            var oldAccessor = _accessor as IIntPropertyObjectAccessor;
            _accessor = newAccessor;
        
            if (helper != null)
            {
                
                newAccessor.Id = oldAccessor.Id;
                newAccessor.Int = oldAccessor.Int;
                newAccessor.GuidProperty = oldAccessor.GuidProperty;
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
        
        public static explicit operator IntPropertyObject(RealmValue val) => val.AsRealmObject<IntPropertyObject>();
        
        public static implicit operator RealmValue(IntPropertyObject val) => RealmValue.Object(val);
        
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
        
        
    
        [EditorBrowsable(EditorBrowsableState.Never)]
        private class IntPropertyObjectObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }
        
            public ManagedAccessor CreateAccessor() => new IntPropertyObjectManagedAccessor();
        
            public IRealmObjectBase CreateInstance()
            {
                return new IntPropertyObject();
            }
        
            public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
            {
                value = ((IIntPropertyObjectAccessor)instance.Accessor).Id;
                return true;
            }
        }
    }
}

namespace Realms.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IIntPropertyObjectAccessor : IRealmAccessor
    {
        ObjectId Id { get; set; }
        
        int Int { get; set; }
        
        Guid GuidProperty { get; set; }
        
        IQueryable<SyncCollectionsObject> ContainingCollections { get; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class IntPropertyObjectManagedAccessor : ManagedAccessor, IIntPropertyObjectAccessor
    {
        public ObjectId Id
        {
            get => (ObjectId)GetValue("_id");
            set => SetValueUnique("_id", value);
        }
        
        public int Int
        {
            get => (int)GetValue("Int");
            set => SetValue("Int", value);
        }
        
        public Guid GuidProperty
        {
            get => (Guid)GetValue("GuidProperty");
            set => SetValue("GuidProperty", value);
        }
        
        private IQueryable<SyncCollectionsObject> _containingCollections;
        public IQueryable<SyncCollectionsObject> ContainingCollections
        {
            get
            {
                if (_containingCollections == null)
                {
                    _containingCollections = GetBacklinks<SyncCollectionsObject>("ContainingCollections");
                }
        
                return _containingCollections;
            }
        }
    }

    internal class IntPropertyObjectUnmanagedAccessor : UnmanagedAccessor, IIntPropertyObjectAccessor
    {
        private ObjectId _id = ObjectId.GenerateNewId();
        public ObjectId Id
        {
            get => _id;
            set
            {
                _id = value;
                RaisePropertyChanged("_id");
            }
        }
        
        private int _int;
        public int Int
        {
            get => _int;
            set
            {
                _int = value;
                RaisePropertyChanged("Int");
            }
        }
        
        private Guid _guidProperty;
        public Guid GuidProperty
        {
            get => _guidProperty;
            set
            {
                _guidProperty = value;
                RaisePropertyChanged("GuidProperty");
            }
        }
        
        public IQueryable<SyncCollectionsObject> ContainingCollections => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects.");
    
        public IntPropertyObjectUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "_id" => _id,
                "Int" => _int,
                "GuidProperty" => _guidProperty,
                "ContainingCollections" => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects."),
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }
    
        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "_id":
                    throw new InvalidOperationException("Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique");
                case "Int":
                    Int = (int)val;
                    return;
                case "GuidProperty":
                    GuidProperty = (Guid)val;
                    return;
                default:
                    throw new MissingMemberException($"The object does not have a settable Realm property with name {propertyName}");
            }
        }
    
        public override void SetValueUnique(string propertyName, RealmValue val)
        {
            if (propertyName != "_id")
            {
                throw new InvalidOperationException($"Cannot set the value of non primary key property ({propertyName}) with SetValueUnique");
            }
            
            Id = (ObjectId)val;
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

