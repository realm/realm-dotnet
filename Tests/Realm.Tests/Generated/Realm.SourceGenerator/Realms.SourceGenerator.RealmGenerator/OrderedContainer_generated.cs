﻿// <auto-generated />
#nullable enable

using NUnit.Framework;
using Realms;
using Realms.Logging;
using Realms.Schema;
using Realms.Tests.Database;
using Realms.Weaving;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TestAsymmetricObject = Realms.IAsymmetricObject;
using TestEmbeddedObject = Realms.IEmbeddedObject;
using TestRealmObject = Realms.IRealmObject;

namespace Realms.Tests.Database
{
    [Generated]
    [Woven(typeof(OrderedContainerObjectHelper))]
    public partial class OrderedContainer : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("OrderedContainer", ObjectSchema.ObjectType.RealmObject)
        {
            Realms.Schema.Property.ObjectList("Items", "OrderedObject", managedName: "Items"),
            Realms.Schema.Property.ObjectDictionary("ItemsDictionary", "OrderedObject", managedName: "ItemsDictionary"),
        }.Build();

        #region IRealmObject implementation

        private IOrderedContainerAccessor _accessor = null!;

        Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

        internal IOrderedContainerAccessor Accessor => _accessor ??= new OrderedContainerUnmanagedAccessor(typeof(OrderedContainer));

        [IgnoreDataMember, XmlIgnore]
        public bool IsManaged => Accessor.IsManaged;

        [IgnoreDataMember, XmlIgnore]
        public bool IsValid => Accessor.IsValid;

        [IgnoreDataMember, XmlIgnore]
        public bool IsFrozen => Accessor.IsFrozen;

        [IgnoreDataMember, XmlIgnore]
        public Realms.Realm Realm => Accessor.Realm;

        [IgnoreDataMember, XmlIgnore]
        public Realms.Schema.ObjectSchema ObjectSchema => Accessor.ObjectSchema;

        [IgnoreDataMember, XmlIgnore]
        public Realms.DynamicObjectApi DynamicApi => Accessor.DynamicApi;

        [IgnoreDataMember, XmlIgnore]
        public int BacklinksCount => Accessor.BacklinksCount;

        public void SetManagedAccessor(Realms.IRealmAccessor managedAccessor, Realms.Weaving.IRealmObjectHelper? helper = null, bool update = false, bool skipDefaults = false)
        {
            var newAccessor = (IOrderedContainerAccessor)managedAccessor;
            var oldAccessor = _accessor;
            _accessor = newAccessor;

            if (helper != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.Items.Clear();
                    newAccessor.ItemsDictionary.Clear();
                }

                Realms.CollectionExtensions.PopulateCollection(oldAccessor.Items, newAccessor.Items, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.ItemsDictionary, newAccessor.ItemsDictionary, update, skipDefaults);
            }

            if (_propertyChanged != null)
            {
                SubscribeForNotifications();
            }

            OnManaged();
        }

        #endregion

        /// <summary>
        /// Called when the object has been managed by a Realm.
        /// </summary>
        /// <remarks>
        /// This method will be called either when a managed object is materialized or when an unmanaged object has been
        /// added to the Realm. It can be useful for providing some initialization logic as when the constructor is invoked,
        /// it is not yet clear whether the object is managed or not.
        /// </remarks>
        partial void OnManaged();

        private event PropertyChangedEventHandler? _propertyChanged;

        public event PropertyChangedEventHandler? PropertyChanged
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

        /// <summary>
        /// Called when a property has changed on this class.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <remarks>
        /// For this method to be called, you need to have first subscribed to <see cref="PropertyChanged"/>.
        /// This can be used to react to changes to the current object, e.g. raising <see cref="PropertyChanged"/> for computed properties.
        /// </remarks>
        /// <example>
        /// <code>
        /// class MyClass : IRealmObject
        /// {
        ///     public int StatusCodeRaw { get; set; }
        ///     public StatusCodeEnum StatusCode => (StatusCodeEnum)StatusCodeRaw;
        ///     partial void OnPropertyChanged(string propertyName)
        ///     {
        ///         if (propertyName == nameof(StatusCodeRaw))
        ///         {
        ///             RaisePropertyChanged(nameof(StatusCode));
        ///         }
        ///     }
        /// }
        /// </code>
        /// Here, we have a computed property that depends on a persisted one. In order to notify any <see cref="PropertyChanged"/>
        /// subscribers that <c>StatusCode</c> has changed, we implement <see cref="OnPropertyChanged"/> and
        /// raise <see cref="PropertyChanged"/> manually by calling <see cref="RaisePropertyChanged"/>.
        /// </example>
        partial void OnPropertyChanged(string? propertyName);

        private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
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

        public static explicit operator OrderedContainer(Realms.RealmValue val) => val.AsRealmObject<OrderedContainer>();

        public static implicit operator Realms.RealmValue(OrderedContainer? val) => Realms.RealmValue.Object(val);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public TypeInfo GetTypeInfo() => Accessor.GetTypeInfo(this);

        public override bool Equals(object? obj)
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

            if (obj is not Realms.IRealmObjectBase iro)
            {
                return false;
            }

            return Accessor.Equals(iro.Accessor);
        }

        public override int GetHashCode() => IsManaged ? Accessor.GetHashCode() : base.GetHashCode();

        public override string? ToString() => Accessor.ToString();

        [EditorBrowsable(EditorBrowsableState.Never)]
        private class OrderedContainerObjectHelper : Realms.Weaving.IRealmObjectHelper
        {
            public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public Realms.ManagedAccessor CreateAccessor() => new OrderedContainerManagedAccessor();

            public Realms.IRealmObjectBase CreateInstance() => new OrderedContainer();

            public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out object? value)
            {
                value = null;
                return false;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal interface IOrderedContainerAccessor : Realms.IRealmAccessor
        {
            System.Collections.Generic.IList<Realms.Tests.Database.OrderedObject> Items { get; }

            System.Collections.Generic.IDictionary<string, Realms.Tests.Database.OrderedObject> ItemsDictionary { get; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class OrderedContainerManagedAccessor : Realms.ManagedAccessor, IOrderedContainerAccessor
        {
            private System.Collections.Generic.IList<Realms.Tests.Database.OrderedObject> _items = null!;
            public System.Collections.Generic.IList<Realms.Tests.Database.OrderedObject> Items
            {
                get
                {
                    if (_items == null)
                    {
                        _items = GetListValue<Realms.Tests.Database.OrderedObject>("Items");
                    }

                    return _items;
                }
            }

            private System.Collections.Generic.IDictionary<string, Realms.Tests.Database.OrderedObject> _itemsDictionary = null!;
            public System.Collections.Generic.IDictionary<string, Realms.Tests.Database.OrderedObject> ItemsDictionary
            {
                get
                {
                    if (_itemsDictionary == null)
                    {
                        _itemsDictionary = GetDictionaryValue<Realms.Tests.Database.OrderedObject>("ItemsDictionary");
                    }

                    return _itemsDictionary;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class OrderedContainerUnmanagedAccessor : Realms.UnmanagedAccessor, IOrderedContainerAccessor
        {
            public override ObjectSchema ObjectSchema => OrderedContainer.RealmSchema;

            public System.Collections.Generic.IList<Realms.Tests.Database.OrderedObject> Items { get; } = new List<Realms.Tests.Database.OrderedObject>();

            public System.Collections.Generic.IDictionary<string, Realms.Tests.Database.OrderedObject> ItemsDictionary { get; } = new Dictionary<string, Realms.Tests.Database.OrderedObject>();

            public OrderedContainerUnmanagedAccessor(Type objectType) : base(objectType)
            {
            }

            public override Realms.RealmValue GetValue(string propertyName)
            {
                throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}");
            }

            public override void SetValue(string propertyName, Realms.RealmValue val)
            {
                throw new MissingMemberException($"The object does not have a settable Realm property with name {propertyName}");
            }

            public override void SetValueUnique(string propertyName, Realms.RealmValue val)
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
}
