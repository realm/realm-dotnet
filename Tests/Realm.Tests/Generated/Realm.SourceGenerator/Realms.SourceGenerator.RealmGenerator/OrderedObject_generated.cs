﻿// <auto-generated />
using Realms.Tests.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.ComponentModel;
using Realms;
using Realms.Weaving;
using Realms.Generated;
using Realms.Schema;

namespace Realms.Tests.Database
{
    [Generated("IOrderedObjectAccessor")]
    [Woven(typeof(OrderedObjectObjectHelper))]
    public partial class OrderedObject : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("OrderedObject", isEmbedded: false)
        {
            Property.Primitive("Order", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false),
            Property.Primitive("IsPartOfResults", RealmValueType.Bool, isPrimaryKey: false, isIndexed: false, isNullable: false),
        }.Build();
        
        #region IRealmObject implementation
        
        private IOrderedObjectAccessor _accessor;
        
        IRealmAccessor IRealmObjectBase.Accessor => Accessor;
        
        internal IOrderedObjectAccessor Accessor => _accessor = _accessor ?? new OrderedObjectUnmanagedAccessor(typeof(OrderedObject));
        
        public bool IsManaged => Accessor.IsManaged;
        
        public bool IsValid => Accessor.IsValid;
        
        public bool IsFrozen => Accessor.IsFrozen;
        
        public Realm Realm => Accessor.Realm;
        
        public ObjectSchema ObjectSchema => Accessor.ObjectSchema;
        
        public RealmObjectBase.Dynamic DynamicApi => Accessor.DynamicApi;
        
        public int BacklinksCount => Accessor.BacklinksCount;
        
        
        
        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var newAccessor = (IOrderedObjectAccessor)managedAccessor;
            var oldAccessor = _accessor as IOrderedObjectAccessor;
            _accessor = newAccessor;
        
            if (helper != null)
            {
                
                if(!skipDefaults || oldAccessor.Order != default(int))
                {
                    newAccessor.Order = oldAccessor.Order;
                }
                if(!skipDefaults || oldAccessor.IsPartOfResults != default(bool))
                {
                    newAccessor.IsPartOfResults = oldAccessor.IsPartOfResults;
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
        
        public static explicit operator OrderedObject(RealmValue val) => val.AsRealmObject<OrderedObject>();
        
        public static implicit operator RealmValue(OrderedObject val) => RealmValue.Object(val);
        
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
        
        
    
        [EditorBrowsable(EditorBrowsableState.Never)]
        private class OrderedObjectObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }
        
            public ManagedAccessor CreateAccessor() => new OrderedObjectManagedAccessor();
        
            public IRealmObjectBase CreateInstance()
            {
                return new OrderedObject();
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
    internal interface IOrderedObjectAccessor : IRealmAccessor
    {
        int Order { get; set; }
        
        bool IsPartOfResults { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class OrderedObjectManagedAccessor : ManagedAccessor, IOrderedObjectAccessor
    {
        public int Order
        {
            get => (int)GetValue("Order");
            set => SetValue("Order", value);
        }
        
        public bool IsPartOfResults
        {
            get => (bool)GetValue("IsPartOfResults");
            set => SetValue("IsPartOfResults", value);
        }
    }

    internal class OrderedObjectUnmanagedAccessor : UnmanagedAccessor, IOrderedObjectAccessor
    {
        private int _order;
        public int Order
        {
            get => _order;
            set
            {
                _order = value;
                RaisePropertyChanged("Order");
            }
        }
        
        private bool _isPartOfResults;
        public bool IsPartOfResults
        {
            get => _isPartOfResults;
            set
            {
                _isPartOfResults = value;
                RaisePropertyChanged("IsPartOfResults");
            }
        }
    
        public OrderedObjectUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }
    
        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "Order" => _order,
                "IsPartOfResults" => _isPartOfResults,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }
    
        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "Order":
                    Order = (int)val;
                    return;
                case "IsPartOfResults":
                    IsPartOfResults = (bool)val;
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

