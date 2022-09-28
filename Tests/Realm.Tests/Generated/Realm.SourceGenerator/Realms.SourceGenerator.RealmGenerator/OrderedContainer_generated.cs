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
    [Woven(typeof(OrderedContainerObjectHelper))]
    public partial class OrderedContainer : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("OrderedContainer", isEmbedded: false)
        {
            Property.ObjectList("Items", "OrderedObject", managedName: "Items"),
            Property.ObjectDictionary("ItemsDictionary", "OrderedObject", managedName: "ItemsDictionary"),
        }.Build();

        #region IRealmObject implementation

        private IOrderedContainerAccessor _accessor;

        IRealmAccessor IRealmObjectBase.Accessor => Accessor;

        internal IOrderedContainerAccessor Accessor => _accessor = _accessor ?? new OrderedContainerUnmanagedAccessor(typeof(OrderedContainer));

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
            var newAccessor = (IOrderedContainerAccessor)managedAccessor;
            var oldAccessor = _accessor as IOrderedContainerAccessor;
            _accessor = newAccessor;

            if (helper != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.Items.Clear();
                    newAccessor.ItemsDictionary.Clear();
                }

                CollectionExtensions.PopulateCollection(oldAccessor.Items, newAccessor.Items, update, skipDefaults);

                CollectionExtensions.PopulateCollection(oldAccessor.ItemsDictionary, newAccessor.ItemsDictionary, update, skipDefaults);
            }

            if (_propertyChanged != null)
            {
                SubscribeForNotifications();
            }

            OnManaged();
        }

        #endregion

        partial void OnManaged();

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

        private void SubscribeForNotifications()
        {
            Accessor.SubscribeForNotifications(RaisePropertyChanged);
        }

        private void UnsubscribeFromNotifications()
        {
            Accessor.UnsubscribeFromNotifications();
        }

        public static explicit operator OrderedContainer(RealmValue val) => val.AsRealmObject<OrderedContainer>();

        public static implicit operator RealmValue(OrderedContainer val) => RealmValue.Object(val);

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
        private class OrderedContainerObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public ManagedAccessor CreateAccessor() => new OrderedContainerManagedAccessor();

            public IRealmObjectBase CreateInstance()
            {
                return new OrderedContainer();
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
    internal interface IOrderedContainerAccessor : IRealmAccessor
    {
        IList<OrderedObject> Items { get; }

        IDictionary<string, OrderedObject> ItemsDictionary { get; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class OrderedContainerManagedAccessor : ManagedAccessor, IOrderedContainerAccessor
    {
        private IList<OrderedObject> _items;
        public IList<OrderedObject> Items
        {
            get
            {
                if (_items == null)
                {
                    _items = GetListValue<OrderedObject>("Items");
                }

                return _items;
            }
        }

        private IDictionary<string, OrderedObject> _itemsDictionary;
        public IDictionary<string, OrderedObject> ItemsDictionary
        {
            get
            {
                if (_itemsDictionary == null)
                {
                    _itemsDictionary = GetDictionaryValue<OrderedObject>("ItemsDictionary");
                }

                return _itemsDictionary;
            }
        }
    }

    internal class OrderedContainerUnmanagedAccessor : UnmanagedAccessor, IOrderedContainerAccessor
    {
        public IList<OrderedObject> Items { get; } = new List<OrderedObject>();

        public IDictionary<string, OrderedObject> ItemsDictionary { get; } = new Dictionary<string, OrderedObject>();

        public OrderedContainerUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override RealmValue GetValue(string propertyName)
        {
            throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}");
        }

        public override void SetValue(string propertyName, RealmValue val)
        {
            throw new MissingMemberException($"The object does not have a settable Realm property with name {propertyName}");
        }

        public override void SetValueUnique(string propertyName, RealmValue val)
        {
            throw new InvalidOperationException("Cannot set the value of an non primary key property with SetValueUnique");
        }

        public override IList<T> GetListValue<T>(string propertyName)
        {
            return propertyName switch
                        {
            "Items" => (IList<T>)Items,

                            _ => throw new MissingMemberException($"The object does not have a Realm list property with name {propertyName}"),
                        };
        }

        public override ISet<T> GetSetValue<T>(string propertyName)
        {
            throw new MissingMemberException($"The object does not have a Realm set property with name {propertyName}");
        }

        public override IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
        {
            return propertyName switch
            {
                "ItemsDictionary" => (IDictionary<string, TValue>)ItemsDictionary,
                _ => throw new MissingMemberException($"The object does not have a Realm dictionary property with name {propertyName}"),
            };
        }
    }
}