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

namespace Realms.Tests
{
    [Generated("IRecursiveBacklinksObjectAccessor")]
    [Woven(typeof(RecursiveBacklinksObjectObjectHelper))]
    public partial class RecursiveBacklinksObject : IRealmObject, INotifyPropertyChanged
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("RecursiveBacklinksObject", isEmbedded: false)
        {
            Property.Primitive("Id", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Object("Parent", "RecursiveBacklinksObject"),
            Property.Backlinks("Children", "RecursiveBacklinksObject", "Parent"),
        }.Build();
        
        #region IRealmObject implementation
        
        private IRecursiveBacklinksObjectAccessor _accessor;
        
        IRealmAccessor IRealmObjectBase.Accessor => Accessor;
        
        internal IRecursiveBacklinksObjectAccessor Accessor => _accessor = _accessor ?? new RecursiveBacklinksObjectUnmanagedAccessor(typeof(RecursiveBacklinksObject));
        
        public bool IsManaged => Accessor.IsManaged;
        
        public bool IsValid => Accessor.IsValid;
        
        public bool IsFrozen => Accessor.IsFrozen;
        
        public Realm Realm => Accessor.Realm;
        
        public ObjectSchema ObjectSchema => Accessor.ObjectSchema;
        
        public RealmObjectBase.Dynamic DynamicApi => Accessor.DynamicApi;
        
        public int BacklinksCount => Accessor.BacklinksCount;
        
        
        
        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var newAccessor = (IRecursiveBacklinksObjectAccessor)managedAccessor;
            var oldAccessor = _accessor as IRecursiveBacklinksObjectAccessor;
            _accessor = newAccessor;
        
            if (helper != null)
            {
                
                newAccessor.Id = oldAccessor.Id;
                if(oldAccessor.Parent != null)
                {
                    newAccessor.Realm.Add(oldAccessor.Parent, update);
                }
                newAccessor.Parent = oldAccessor.Parent;
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
        
        public static explicit operator RecursiveBacklinksObject(RealmValue val) => val.AsRealmObject<RecursiveBacklinksObject>();
        
        public static implicit operator RealmValue(RecursiveBacklinksObject val) => RealmValue.Object(val);
        
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
        
        /***
        public override string ToString()
        {
            return Accessor.ToString();
        }
        **/
        
    
        [EditorBrowsable(EditorBrowsableState.Never)]
        private class RecursiveBacklinksObjectObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }
        
            public ManagedAccessor CreateAccessor() => new RecursiveBacklinksObjectManagedAccessor();
        
            public IRealmObjectBase CreateInstance()
            {
                return new RecursiveBacklinksObject();
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
    internal interface IRecursiveBacklinksObjectAccessor : IRealmAccessor
    {
        int Id { get; set; }
        
        RecursiveBacklinksObject Parent { get; set; }
        
        IQueryable<RecursiveBacklinksObject> Children { get; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class RecursiveBacklinksObjectManagedAccessor : ManagedAccessor, IRecursiveBacklinksObjectAccessor
    {
        public int Id
        {
            get => (int)GetValue("Id");
            set => SetValue("Id", value);
        }
        
        public RecursiveBacklinksObject Parent
        {
            get => (RecursiveBacklinksObject)GetValue("Parent");
            set => SetValue("Parent", value);
        }
        
        private IQueryable<RecursiveBacklinksObject> _children;
        public IQueryable<RecursiveBacklinksObject> Children
        {
            get
            {
                if (_children == null)
                {
                    _children = GetBacklinks<RecursiveBacklinksObject>("Children");
                }
        
                return _children;
            }
        }
    }

    internal class RecursiveBacklinksObjectUnmanagedAccessor : UnmanagedAccessor, IRecursiveBacklinksObjectAccessor
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
        
        private RecursiveBacklinksObject _parent;
        public RecursiveBacklinksObject Parent
        {
            get => _parent;
            set
            {
                _parent = value;
                RaisePropertyChanged("Parent");
            }
        }
        
        public IQueryable<RecursiveBacklinksObject> Children => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects.");
    
        public RecursiveBacklinksObjectUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "Id" => _id,
                "Parent" => _parent,
                "Children" => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects."),
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
                case "Parent":
                    Parent = (RecursiveBacklinksObject)val;
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

