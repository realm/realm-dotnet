﻿// <auto-generated />
using NUnit.Framework;
using Realms;
using Realms.IAsymmetricObject;
using Realms.IEmbeddedObject;
using Realms.IRealmObject;
using Realms.Schema;
using Realms.Tests.Database;
using Realms.Weaving;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TestAsymmetricObject = Realms.IAsymmetricObject;
using TestEmbeddedObject = Realms.IEmbeddedObject;
using TestRealmObject = Realms.IRealmObject;

namespace Realms.Tests.Database
{
    [Generated]
    [Woven(typeof(MixedProperties1ObjectHelper))]
    public partial class MixedProperties1 : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("MixedProperties1", ObjectSchema.ObjectType.RealmObject)
        {
            Realms.Schema.Property.Primitive("Name", Realms.RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "Name"),
            Realms.Schema.Property.ObjectList("Friends", "Person", managedName: "Friends"),
            Realms.Schema.Property.Primitive("Age", Realms.RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "Age"),
            Realms.Schema.Property.ObjectList("Enemies", "Person", managedName: "Enemies"),
        }.Build();

        #region IRealmObject implementation

        private IMixedProperties1Accessor _accessor;

        Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

        internal IMixedProperties1Accessor Accessor => _accessor ?? (_accessor = new MixedProperties1UnmanagedAccessor(typeof(MixedProperties1)));

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

        public void SetManagedAccessor(Realms.IRealmAccessor managedAccessor, Realms.Weaving.IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var newAccessor = (IMixedProperties1Accessor)managedAccessor;
            var oldAccessor = (IMixedProperties1Accessor)_accessor;
            _accessor = newAccessor;

            if (helper != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.Friends.Clear();
                    newAccessor.Enemies.Clear();
                }

                if(!skipDefaults || oldAccessor.Name != default(string))
                {
                    newAccessor.Name = oldAccessor.Name;
                }
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.Friends, newAccessor.Friends, update, skipDefaults);
                if(!skipDefaults || oldAccessor.Age != default(int))
                {
                    newAccessor.Age = oldAccessor.Age;
                }
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.Enemies, newAccessor.Enemies, update, skipDefaults);
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

        public static explicit operator MixedProperties1(Realms.RealmValue val) => val.AsRealmObject<MixedProperties1>();

        public static implicit operator Realms.RealmValue(MixedProperties1 val) => Realms.RealmValue.Object(val);

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

            if (obj is not Realms.IRealmObjectBase iro)
            {
                return false;
            }

            return Accessor.Equals(iro.Accessor);
        }

        public override int GetHashCode() => IsManaged ? Accessor.GetHashCode() : base.GetHashCode();

        public override string ToString() => Accessor.ToString();

        [EditorBrowsable(EditorBrowsableState.Never)]
        private class MixedProperties1ObjectHelper : Realms.Weaving.IRealmObjectHelper
        {
            public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public Realms.ManagedAccessor CreateAccessor() => new MixedProperties1ManagedAccessor();

            public Realms.IRealmObjectBase CreateInstance() => new MixedProperties1();

            public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out object value)
            {
                value = null;
                return false;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal interface IMixedProperties1Accessor : Realms.IRealmAccessor
        {
            string Name { get; set; }

            System.Collections.Generic.IList<Realms.Tests.Database.Person> Friends { get; }

            int Age { get; set; }

            System.Collections.Generic.IList<Realms.Tests.Database.Person> Enemies { get; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class MixedProperties1ManagedAccessor : Realms.ManagedAccessor, IMixedProperties1Accessor
        {
            public string Name
            {
                get => (string)GetValue("Name");
                set => SetValue("Name", value);
            }

            private System.Collections.Generic.IList<Realms.Tests.Database.Person> _friends;
            public System.Collections.Generic.IList<Realms.Tests.Database.Person> Friends
            {
                get
                {
                    if (_friends == null)
                    {
                        _friends = GetListValue<Realms.Tests.Database.Person>("Friends");
                    }

                    return _friends;
                }
            }

            public int Age
            {
                get => (int)GetValue("Age");
                set => SetValue("Age", value);
            }

            private System.Collections.Generic.IList<Realms.Tests.Database.Person> _enemies;
            public System.Collections.Generic.IList<Realms.Tests.Database.Person> Enemies
            {
                get
                {
                    if (_enemies == null)
                    {
                        _enemies = GetListValue<Realms.Tests.Database.Person>("Enemies");
                    }

                    return _enemies;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class MixedProperties1UnmanagedAccessor : Realms.UnmanagedAccessor, IMixedProperties1Accessor
        {
            public override ObjectSchema ObjectSchema => MixedProperties1.RealmSchema;

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

            public System.Collections.Generic.IList<Realms.Tests.Database.Person> Friends { get; } = new List<Realms.Tests.Database.Person>();

            private int _age;
            public int Age
            {
                get => _age;
                set
                {
                    _age = value;
                    RaisePropertyChanged("Age");
                }
            }

            public System.Collections.Generic.IList<Realms.Tests.Database.Person> Enemies { get; } = new List<Realms.Tests.Database.Person>();

            public MixedProperties1UnmanagedAccessor(Type objectType) : base(objectType)
            {
            }

            public override Realms.RealmValue GetValue(string propertyName)
            {
                return propertyName switch
                {
                    "Name" => _name,
                    "Age" => _age,
                    _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
                };
            }

            public override void SetValue(string propertyName, Realms.RealmValue val)
            {
                switch (propertyName)
                {
                    case "Name":
                        Name = (string)val;
                        return;
                    case "Age":
                        Age = (int)val;
                        return;
                    default:
                        throw new MissingMemberException($"The object does not have a settable Realm property with name {propertyName}");
                }
            }

            public override void SetValueUnique(string propertyName, Realms.RealmValue val)
            {
                throw new InvalidOperationException("Cannot set the value of an non primary key property with SetValueUnique");
            }

            public override IList<T> GetListValue<T>(string propertyName)
            {
                return propertyName switch
                            {
                "Friends" => (IList<T>)Friends,
                "Enemies" => (IList<T>)Enemies,

                                _ => throw new MissingMemberException($"The object does not have a Realm list property with name {propertyName}"),
                            };
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
}
