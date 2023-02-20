﻿// <auto-generated />
#nullable enable

using MongoDB.Bson;
using Realms;
using Realms.Schema;
using Realms.Tests;
using Realms.Tests.Database;
using Realms.Weaving;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TestAsymmetricObject = Realms.IAsymmetricObject;
using TestEmbeddedObject = Realms.IEmbeddedObject;
using TestRealmObject = Realms.IRealmObject;

namespace Realms.Tests
{
    [Generated]
    [Woven(typeof(ClassWithUnqueryableMembersObjectHelper))]
    public partial class ClassWithUnqueryableMembers : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("ClassWithUnqueryableMembers", ObjectSchema.ObjectType.RealmObject)
        {
            Realms.Schema.Property.Primitive("RealPropertyToSatisfyWeaver", Realms.RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "RealPropertyToSatisfyWeaver"),
            Realms.Schema.Property.Object("RealmObjectProperty", "Person", managedName: "RealmObjectProperty"),
            Realms.Schema.Property.ObjectList("RealmListProperty", "Person", managedName: "RealmListProperty"),
            Realms.Schema.Property.Primitive("FirstName", Realms.RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "FirstName"),
            Realms.Schema.Property.Backlinks("BacklinkProperty", "UnqueryableBacklinks", "Parent", managedName: "BacklinkProperty"),
        }.Build();

        #region IRealmObject implementation

        private IClassWithUnqueryableMembersAccessor? _accessor;

        Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

        internal IClassWithUnqueryableMembersAccessor Accessor => _accessor ??= new ClassWithUnqueryableMembersUnmanagedAccessor(typeof(ClassWithUnqueryableMembers));

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
            var newAccessor = (IClassWithUnqueryableMembersAccessor)managedAccessor;
            var oldAccessor = _accessor;
            _accessor = newAccessor;

            if (helper != null && oldAccessor != null)
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

        private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
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

        public static explicit operator ClassWithUnqueryableMembers?(Realms.RealmValue val) => val.Type == Realms.RealmValueType.Null ? null : val.AsRealmObject<ClassWithUnqueryableMembers>();

        public static implicit operator Realms.RealmValue(ClassWithUnqueryableMembers? val) => val == null ? Realms.RealmValue.Null : Realms.RealmValue.Object(val);

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
        private class ClassWithUnqueryableMembersObjectHelper : Realms.Weaving.IRealmObjectHelper
        {
            public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public Realms.ManagedAccessor CreateAccessor() => new ClassWithUnqueryableMembersManagedAccessor();

            public Realms.IRealmObjectBase CreateInstance() => new ClassWithUnqueryableMembers();

            public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out RealmValue value)
            {
                value = RealmValue.Null;
                return false;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal interface IClassWithUnqueryableMembersAccessor : Realms.IRealmAccessor
        {
            string? RealPropertyToSatisfyWeaver { get; set; }

            Realms.Tests.Database.Person? RealmObjectProperty { get; set; }

            System.Collections.Generic.IList<Realms.Tests.Database.Person> RealmListProperty { get; }

            string? FirstName { get; set; }

            System.Linq.IQueryable<Realms.Tests.UnqueryableBacklinks> BacklinkProperty { get; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class ClassWithUnqueryableMembersManagedAccessor : Realms.ManagedAccessor, IClassWithUnqueryableMembersAccessor
        {
            public string? RealPropertyToSatisfyWeaver
            {
                get => (string?)GetValue("RealPropertyToSatisfyWeaver");
                set => SetValue("RealPropertyToSatisfyWeaver", value);
            }

            public Realms.Tests.Database.Person? RealmObjectProperty
            {
                get => (Realms.Tests.Database.Person?)GetValue("RealmObjectProperty");
                set => SetValue("RealmObjectProperty", value);
            }

            private System.Collections.Generic.IList<Realms.Tests.Database.Person> _realmListProperty = null!;
            public System.Collections.Generic.IList<Realms.Tests.Database.Person> RealmListProperty
            {
                get
                {
                    if (_realmListProperty == null)
                    {
                        _realmListProperty = GetListValue<Realms.Tests.Database.Person>("RealmListProperty");
                    }

                    return _realmListProperty;
                }
            }

            public string? FirstName
            {
                get => (string?)GetValue("FirstName");
                set => SetValue("FirstName", value);
            }

            private System.Linq.IQueryable<Realms.Tests.UnqueryableBacklinks> _backlinkProperty = null!;
            public System.Linq.IQueryable<Realms.Tests.UnqueryableBacklinks> BacklinkProperty
            {
                get
                {
                    if (_backlinkProperty == null)
                    {
                        _backlinkProperty = GetBacklinks<Realms.Tests.UnqueryableBacklinks>("BacklinkProperty");
                    }

                    return _backlinkProperty;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class ClassWithUnqueryableMembersUnmanagedAccessor : Realms.UnmanagedAccessor, IClassWithUnqueryableMembersAccessor
        {
            public override ObjectSchema ObjectSchema => ClassWithUnqueryableMembers.RealmSchema;

            private string? _realPropertyToSatisfyWeaver;
            public string? RealPropertyToSatisfyWeaver
            {
                get => _realPropertyToSatisfyWeaver;
                set
                {
                    _realPropertyToSatisfyWeaver = value;
                    RaisePropertyChanged("RealPropertyToSatisfyWeaver");
                }
            }

            private Realms.Tests.Database.Person? _realmObjectProperty;
            public Realms.Tests.Database.Person? RealmObjectProperty
            {
                get => _realmObjectProperty;
                set
                {
                    _realmObjectProperty = value;
                    RaisePropertyChanged("RealmObjectProperty");
                }
            }

            public System.Collections.Generic.IList<Realms.Tests.Database.Person> RealmListProperty { get; } = new List<Realms.Tests.Database.Person>();

            private string? _firstName;
            public string? FirstName
            {
                get => _firstName;
                set
                {
                    _firstName = value;
                    RaisePropertyChanged("FirstName");
                }
            }

            public System.Linq.IQueryable<Realms.Tests.UnqueryableBacklinks> BacklinkProperty => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects.");

            public ClassWithUnqueryableMembersUnmanagedAccessor(Type objectType) : base(objectType)
            {
            }

            public override Realms.RealmValue GetValue(string propertyName)
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

            public override void SetValue(string propertyName, Realms.RealmValue val)
            {
                switch (propertyName)
                {
                    case "RealPropertyToSatisfyWeaver":
                        RealPropertyToSatisfyWeaver = (string?)val;
                        return;
                    case "RealmObjectProperty":
                        RealmObjectProperty = (Realms.Tests.Database.Person?)val;
                        return;
                    case "FirstName":
                        FirstName = (string?)val;
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
}
