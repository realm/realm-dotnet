﻿// <auto-generated />
using Realms;
using Realms.Schema;
using Realms.Tests;
using Realms.Tests.Generated;
using Realms.Weaving;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Realms.Tests
{
    [Generated]
    [Woven(typeof(WalkerObjectHelper))]
    public partial class Walker : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("Walker", ObjectSchema.ObjectType.RealmObject)
        {
            Property.Primitive("Name", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "Name"),
            Property.Object("TopDog", "Dog", managedName: "TopDog"),
            Property.ObjectList("ListOfDogs", "Dog", managedName: "ListOfDogs"),
            Property.ObjectSet("SetOfDogs", "Dog", managedName: "SetOfDogs"),
        }.Build();

        #region IRealmObject implementation

        private IWalkerAccessor _accessor;

        IRealmAccessor IRealmObjectBase.Accessor => Accessor;

        internal IWalkerAccessor Accessor => _accessor ?? (_accessor = new WalkerUnmanagedAccessor(typeof(Walker)));

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
            var newAccessor = (IWalkerAccessor)managedAccessor;
            var oldAccessor = (IWalkerAccessor)_accessor;
            _accessor = newAccessor;

            if (helper != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.ListOfDogs.Clear();
                    newAccessor.SetOfDogs.Clear();
                }

                if(!skipDefaults || oldAccessor.Name != default(string))
                {
                    newAccessor.Name = oldAccessor.Name;
                }
                if(oldAccessor.TopDog != null)
                {
                    newAccessor.Realm.Add(oldAccessor.TopDog, update);
                }
                newAccessor.TopDog = oldAccessor.TopDog;
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.ListOfDogs, newAccessor.ListOfDogs, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.SetOfDogs, newAccessor.SetOfDogs, update, skipDefaults);
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

        public static explicit operator Walker(RealmValue val) => val.AsRealmObject<Walker>();

        public static implicit operator RealmValue(Walker val) => RealmValue.Object(val);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public TypeInfo GetTypeInfo() => Accessor.GetTypeInfo(this);

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

        public override int GetHashCode() => IsManaged ? Accessor.GetHashCode() : base.GetHashCode();

        public override string ToString() => Accessor.ToString();

        [EditorBrowsable(EditorBrowsableState.Never)]
        private class WalkerObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public ManagedAccessor CreateAccessor() => new WalkerManagedAccessor();

            public IRealmObjectBase CreateInstance() => new Walker();

            public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
            {
                value = null;
                return false;
            }
        }
    }
}

namespace Realms.Tests.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IWalkerAccessor : IRealmAccessor
    {
        string Name { get; set; }

        Dog TopDog { get; set; }

        IList<Dog> ListOfDogs { get; }

        ISet<Dog> SetOfDogs { get; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class WalkerManagedAccessor : ManagedAccessor, IWalkerAccessor
    {
        public string Name
        {
            get => (string)GetValue("Name");
            set => SetValue("Name", value);
        }

        public Dog TopDog
        {
            get => (Dog)GetValue("TopDog");
            set => SetValue("TopDog", value);
        }

        private IList<Dog> _listOfDogs;
        public IList<Dog> ListOfDogs
        {
            get
            {
                if (_listOfDogs == null)
                {
                    _listOfDogs = GetListValue<Dog>("ListOfDogs");
                }

                return _listOfDogs;
            }
        }

        private ISet<Dog> _setOfDogs;
        public ISet<Dog> SetOfDogs
        {
            get
            {
                if (_setOfDogs == null)
                {
                    _setOfDogs = GetSetValue<Dog>("SetOfDogs");
                }

                return _setOfDogs;
            }
        }
    }

    internal class WalkerUnmanagedAccessor : UnmanagedAccessor, IWalkerAccessor
    {
        public override ObjectSchema ObjectSchema => Walker.RealmSchema;

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

        private Dog _topDog;
        public Dog TopDog
        {
            get => _topDog;
            set
            {
                _topDog = value;
                RaisePropertyChanged("TopDog");
            }
        }

        public IList<Dog> ListOfDogs { get; } = new List<Dog>();

        public ISet<Dog> SetOfDogs { get; } = new HashSet<Dog>(RealmSet<Dog>.Comparer);

        public WalkerUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "Name" => _name,
                "TopDog" => _topDog,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }

        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "Name":
                    Name = (string)val;
                    return;
                case "TopDog":
                    TopDog = (Dog)val;
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
            "ListOfDogs" => (IList<T>)ListOfDogs,

                            _ => throw new MissingMemberException($"The object does not have a Realm list property with name {propertyName}"),
                        };
        }

        public override ISet<T> GetSetValue<T>(string propertyName)
        {
            return propertyName switch
                        {
            "SetOfDogs" => (ISet<T>)SetOfDogs,

                            _ => throw new MissingMemberException($"The object does not have a Realm set property with name {propertyName}"),
                        };
        }

        public override IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
        {
            throw new MissingMemberException($"The object does not have a Realm dictionary property with name {propertyName}");
        }
    }
}