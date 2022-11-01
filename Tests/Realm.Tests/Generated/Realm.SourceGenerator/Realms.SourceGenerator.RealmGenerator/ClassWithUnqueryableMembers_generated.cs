﻿// <auto-generated />
using Realms;
using Realms.Schema;
using Realms.Tests;
using Realms.Tests.Database;
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
    [Woven(typeof(ClassWithUnqueryableMembersObjectHelper))]
    public partial class ClassWithUnqueryableMembers : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("ClassWithUnqueryableMembers", ObjectSchema.ObjectType.RealmObject)
        {
            Property.Primitive("RealPropertyToSatisfyWeaver", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "RealPropertyToSatisfyWeaver"),
            Property.Object("RealmObjectProperty", "Person", managedName: "RealmObjectProperty"),
            Property.ObjectList("RealmListProperty", "Person", managedName: "RealmListProperty"),
            Property.Primitive("FirstName", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "FirstName"),
            Property.Backlinks("BacklinkProperty", "UnqueryableBacklinks", "Parent", managedName: "BacklinkProperty"),
        }.Build();

        #region IRealmObject implementation

        private IClassWithUnqueryableMembersAccessor _accessor;

        IRealmAccessor IRealmObjectBase.Accessor => Accessor;

        internal IClassWithUnqueryableMembersAccessor Accessor => _accessor ?? (_accessor = new ClassWithUnqueryableMembersUnmanagedAccessor(typeof(ClassWithUnqueryableMembers)));

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
            var newAccessor = (IClassWithUnqueryableMembersAccessor)managedAccessor;
            var oldAccessor = (IClassWithUnqueryableMembersAccessor)_accessor;
            _accessor = newAccessor;

            if (helper != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.RealmListProperty.Clear();
                }

                if(!skipDefaults || oldAccessor.RealPropertyToSatisfyWeaver != default(string))
                {
                    newAccessor.RealPropertyToSatisfyWeaver = oldAccessor.RealPropertyToSatisfyWeaver;
                }
                if(oldAccessor.RealmObjectProperty != null)
                {
                    newAccessor.Realm.Add(oldAccessor.RealmObjectProperty, update);
                }
                newAccessor.RealmObjectProperty = oldAccessor.RealmObjectProperty;
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.RealmListProperty, newAccessor.RealmListProperty, update, skipDefaults);
                if(!skipDefaults || oldAccessor.FirstName != default(string))
                {
                    newAccessor.FirstName = oldAccessor.FirstName;
                }
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

        public static explicit operator ClassWithUnqueryableMembers(RealmValue val) => val.AsRealmObject<ClassWithUnqueryableMembers>();

        public static implicit operator RealmValue(ClassWithUnqueryableMembers val) => RealmValue.Object(val);

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
        private class ClassWithUnqueryableMembersObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public ManagedAccessor CreateAccessor() => new ClassWithUnqueryableMembersManagedAccessor();

            public IRealmObjectBase CreateInstance() => new ClassWithUnqueryableMembers();

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
    internal interface IClassWithUnqueryableMembersAccessor : IRealmAccessor
    {
        string RealPropertyToSatisfyWeaver { get; set; }

        Person RealmObjectProperty { get; set; }

        IList<Person> RealmListProperty { get; }

        string FirstName { get; set; }

        IQueryable<UnqueryableBacklinks> BacklinkProperty { get; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class ClassWithUnqueryableMembersManagedAccessor : ManagedAccessor, IClassWithUnqueryableMembersAccessor
    {
        public string RealPropertyToSatisfyWeaver
        {
            get => (string)GetValue("RealPropertyToSatisfyWeaver");
            set => SetValue("RealPropertyToSatisfyWeaver", value);
        }

        public Person RealmObjectProperty
        {
            get => (Person)GetValue("RealmObjectProperty");
            set => SetValue("RealmObjectProperty", value);
        }

        private IList<Person> _realmListProperty;
        public IList<Person> RealmListProperty
        {
            get
            {
                if (_realmListProperty == null)
                {
                    _realmListProperty = GetListValue<Person>("RealmListProperty");
                }

                return _realmListProperty;
            }
        }

        public string FirstName
        {
            get => (string)GetValue("FirstName");
            set => SetValue("FirstName", value);
        }

        private IQueryable<UnqueryableBacklinks> _backlinkProperty;
        public IQueryable<UnqueryableBacklinks> BacklinkProperty
        {
            get
            {
                if (_backlinkProperty == null)
                {
                    _backlinkProperty = GetBacklinks<UnqueryableBacklinks>("BacklinkProperty");
                }

                return _backlinkProperty;
            }
        }
    }

    internal class ClassWithUnqueryableMembersUnmanagedAccessor : UnmanagedAccessor, IClassWithUnqueryableMembersAccessor
    {
        private string _realPropertyToSatisfyWeaver;
        public string RealPropertyToSatisfyWeaver
        {
            get => _realPropertyToSatisfyWeaver;
            set
            {
                _realPropertyToSatisfyWeaver = value;
                RaisePropertyChanged("RealPropertyToSatisfyWeaver");
            }
        }

        private Person _realmObjectProperty;
        public Person RealmObjectProperty
        {
            get => _realmObjectProperty;
            set
            {
                _realmObjectProperty = value;
                RaisePropertyChanged("RealmObjectProperty");
            }
        }

        public IList<Person> RealmListProperty { get; } = new List<Person>();

        private string _firstName;
        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                RaisePropertyChanged("FirstName");
            }
        }

        public IQueryable<UnqueryableBacklinks> BacklinkProperty => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects.");

        public ClassWithUnqueryableMembersUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "RealPropertyToSatisfyWeaver" => _realPropertyToSatisfyWeaver,
                "RealmObjectProperty" => _realmObjectProperty,
                "FirstName" => _firstName,
                "BacklinkProperty" => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects."),
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }

        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "RealPropertyToSatisfyWeaver":
                    RealPropertyToSatisfyWeaver = (string)val;
                    return;
                case "RealmObjectProperty":
                    RealmObjectProperty = (Person)val;
                    return;
                case "FirstName":
                    FirstName = (string)val;
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
            "RealmListProperty" => (IList<T>)RealmListProperty,

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