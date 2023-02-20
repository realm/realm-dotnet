﻿// <auto-generated />
#nullable enable

using Realms;
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
using TestRealmObject = Realms.IRealmObject;

namespace Realms.Tests.Database
{
    [Generated]
    [Woven(typeof(TestNotificationObjectObjectHelper))]
    public partial class TestNotificationObject : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("TestNotificationObject", ObjectSchema.ObjectType.RealmObject)
        {
            Realms.Schema.Property.Primitive("StringProperty", Realms.RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "StringProperty"),
            Realms.Schema.Property.ObjectList("ListSameType", "TestNotificationObject", managedName: "ListSameType"),
            Realms.Schema.Property.ObjectSet("SetSameType", "TestNotificationObject", managedName: "SetSameType"),
            Realms.Schema.Property.ObjectDictionary("DictionarySameType", "TestNotificationObject", managedName: "DictionarySameType"),
            Realms.Schema.Property.Object("LinkSameType", "TestNotificationObject", managedName: "LinkSameType"),
            Realms.Schema.Property.ObjectList("ListDifferentType", "Person", managedName: "ListDifferentType"),
            Realms.Schema.Property.ObjectSet("SetDifferentType", "Person", managedName: "SetDifferentType"),
            Realms.Schema.Property.ObjectDictionary("DictionaryDifferentType", "Person", managedName: "DictionaryDifferentType"),
            Realms.Schema.Property.Object("LinkDifferentType", "Person", managedName: "LinkDifferentType"),
            Realms.Schema.Property.Backlinks("Backlink", "TestNotificationObject", "LinkSameType", managedName: "Backlink"),
        }.Build();

        #region IRealmObject implementation

        private ITestNotificationObjectAccessor? _accessor;

        Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

        internal ITestNotificationObjectAccessor Accessor => _accessor ??= new TestNotificationObjectUnmanagedAccessor(typeof(TestNotificationObject));

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
            var newAccessor = (ITestNotificationObjectAccessor)managedAccessor;
            var oldAccessor = _accessor;
            _accessor = newAccessor;

            if (helper != null && oldAccessor != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.ListSameType.Clear();
                    newAccessor.SetSameType.Clear();
                    newAccessor.DictionarySameType.Clear();
                    newAccessor.ListDifferentType.Clear();
                    newAccessor.SetDifferentType.Clear();
                    newAccessor.DictionaryDifferentType.Clear();
                }

                if(!skipDefaults || oldAccessor.StringProperty != default(string))
                {
                    newAccessor.StringProperty = oldAccessor.StringProperty;
                }
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.ListSameType, newAccessor.ListSameType, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.SetSameType, newAccessor.SetSameType, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.DictionarySameType, newAccessor.DictionarySameType, update, skipDefaults);
                if(oldAccessor.LinkSameType != null)
                {
                    newAccessor.Realm.Add(oldAccessor.LinkSameType, update);
                }
                newAccessor.LinkSameType = oldAccessor.LinkSameType;
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.ListDifferentType, newAccessor.ListDifferentType, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.SetDifferentType, newAccessor.SetDifferentType, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.DictionaryDifferentType, newAccessor.DictionaryDifferentType, update, skipDefaults);
                if(oldAccessor.LinkDifferentType != null)
                {
                    newAccessor.Realm.Add(oldAccessor.LinkDifferentType, update);
                }
                newAccessor.LinkDifferentType = oldAccessor.LinkDifferentType;
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

        public static explicit operator TestNotificationObject?(Realms.RealmValue val) => val.Type == Realms.RealmValueType.Null ? null : val.AsRealmObject<TestNotificationObject>();

        public static implicit operator Realms.RealmValue(TestNotificationObject? val) => val == null ? Realms.RealmValue.Null : Realms.RealmValue.Object(val);

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
        private class TestNotificationObjectObjectHelper : Realms.Weaving.IRealmObjectHelper
        {
            public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public Realms.ManagedAccessor CreateAccessor() => new TestNotificationObjectManagedAccessor();

            public Realms.IRealmObjectBase CreateInstance() => new TestNotificationObject();

            public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out RealmValue value)
            {
                value = RealmValue.Null;
                return false;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal interface ITestNotificationObjectAccessor : Realms.IRealmAccessor
        {
            string? StringProperty { get; set; }

            System.Collections.Generic.IList<Realms.Tests.Database.TestNotificationObject> ListSameType { get; }

            System.Collections.Generic.ISet<Realms.Tests.Database.TestNotificationObject> SetSameType { get; }

            System.Collections.Generic.IDictionary<string, Realms.Tests.Database.TestNotificationObject?> DictionarySameType { get; }

            Realms.Tests.Database.TestNotificationObject? LinkSameType { get; set; }

            System.Collections.Generic.IList<Realms.Tests.Database.Person> ListDifferentType { get; }

            System.Collections.Generic.ISet<Realms.Tests.Database.Person> SetDifferentType { get; }

            System.Collections.Generic.IDictionary<string, Realms.Tests.Database.Person?> DictionaryDifferentType { get; }

            Realms.Tests.Database.Person? LinkDifferentType { get; set; }

            System.Linq.IQueryable<Realms.Tests.Database.TestNotificationObject> Backlink { get; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class TestNotificationObjectManagedAccessor : Realms.ManagedAccessor, ITestNotificationObjectAccessor
        {
            public string? StringProperty
            {
                get => (string?)GetValue("StringProperty");
                set => SetValue("StringProperty", value);
            }

            private System.Collections.Generic.IList<Realms.Tests.Database.TestNotificationObject> _listSameType = null!;
            public System.Collections.Generic.IList<Realms.Tests.Database.TestNotificationObject> ListSameType
            {
                get
                {
                    if (_listSameType == null)
                    {
                        _listSameType = GetListValue<Realms.Tests.Database.TestNotificationObject>("ListSameType");
                    }

                    return _listSameType;
                }
            }

            private System.Collections.Generic.ISet<Realms.Tests.Database.TestNotificationObject> _setSameType = null!;
            public System.Collections.Generic.ISet<Realms.Tests.Database.TestNotificationObject> SetSameType
            {
                get
                {
                    if (_setSameType == null)
                    {
                        _setSameType = GetSetValue<Realms.Tests.Database.TestNotificationObject>("SetSameType");
                    }

                    return _setSameType;
                }
            }

            private System.Collections.Generic.IDictionary<string, Realms.Tests.Database.TestNotificationObject?> _dictionarySameType = null!;
            public System.Collections.Generic.IDictionary<string, Realms.Tests.Database.TestNotificationObject?> DictionarySameType
            {
                get
                {
                    if (_dictionarySameType == null)
                    {
                        _dictionarySameType = GetDictionaryValue<Realms.Tests.Database.TestNotificationObject?>("DictionarySameType");
                    }

                    return _dictionarySameType;
                }
            }

            public Realms.Tests.Database.TestNotificationObject? LinkSameType
            {
                get => (Realms.Tests.Database.TestNotificationObject?)GetValue("LinkSameType");
                set => SetValue("LinkSameType", value);
            }

            private System.Collections.Generic.IList<Realms.Tests.Database.Person> _listDifferentType = null!;
            public System.Collections.Generic.IList<Realms.Tests.Database.Person> ListDifferentType
            {
                get
                {
                    if (_listDifferentType == null)
                    {
                        _listDifferentType = GetListValue<Realms.Tests.Database.Person>("ListDifferentType");
                    }

                    return _listDifferentType;
                }
            }

            private System.Collections.Generic.ISet<Realms.Tests.Database.Person> _setDifferentType = null!;
            public System.Collections.Generic.ISet<Realms.Tests.Database.Person> SetDifferentType
            {
                get
                {
                    if (_setDifferentType == null)
                    {
                        _setDifferentType = GetSetValue<Realms.Tests.Database.Person>("SetDifferentType");
                    }

                    return _setDifferentType;
                }
            }

            private System.Collections.Generic.IDictionary<string, Realms.Tests.Database.Person?> _dictionaryDifferentType = null!;
            public System.Collections.Generic.IDictionary<string, Realms.Tests.Database.Person?> DictionaryDifferentType
            {
                get
                {
                    if (_dictionaryDifferentType == null)
                    {
                        _dictionaryDifferentType = GetDictionaryValue<Realms.Tests.Database.Person?>("DictionaryDifferentType");
                    }

                    return _dictionaryDifferentType;
                }
            }

            public Realms.Tests.Database.Person? LinkDifferentType
            {
                get => (Realms.Tests.Database.Person?)GetValue("LinkDifferentType");
                set => SetValue("LinkDifferentType", value);
            }

            private System.Linq.IQueryable<Realms.Tests.Database.TestNotificationObject> _backlink = null!;
            public System.Linq.IQueryable<Realms.Tests.Database.TestNotificationObject> Backlink
            {
                get
                {
                    if (_backlink == null)
                    {
                        _backlink = GetBacklinks<Realms.Tests.Database.TestNotificationObject>("Backlink");
                    }

                    return _backlink;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class TestNotificationObjectUnmanagedAccessor : Realms.UnmanagedAccessor, ITestNotificationObjectAccessor
        {
            public override ObjectSchema ObjectSchema => TestNotificationObject.RealmSchema;

            private string? _stringProperty;
            public string? StringProperty
            {
                get => _stringProperty;
                set
                {
                    _stringProperty = value;
                    RaisePropertyChanged("StringProperty");
                }
            }

            public System.Collections.Generic.IList<Realms.Tests.Database.TestNotificationObject> ListSameType { get; } = new List<Realms.Tests.Database.TestNotificationObject>();

            public System.Collections.Generic.ISet<Realms.Tests.Database.TestNotificationObject> SetSameType { get; } = new HashSet<Realms.Tests.Database.TestNotificationObject>(RealmSet<Realms.Tests.Database.TestNotificationObject>.Comparer);

            public System.Collections.Generic.IDictionary<string, Realms.Tests.Database.TestNotificationObject?> DictionarySameType { get; } = new Dictionary<string, Realms.Tests.Database.TestNotificationObject?>();

            private Realms.Tests.Database.TestNotificationObject? _linkSameType;
            public Realms.Tests.Database.TestNotificationObject? LinkSameType
            {
                get => _linkSameType;
                set
                {
                    _linkSameType = value;
                    RaisePropertyChanged("LinkSameType");
                }
            }

            public System.Collections.Generic.IList<Realms.Tests.Database.Person> ListDifferentType { get; } = new List<Realms.Tests.Database.Person>();

            public System.Collections.Generic.ISet<Realms.Tests.Database.Person> SetDifferentType { get; } = new HashSet<Realms.Tests.Database.Person>(RealmSet<Realms.Tests.Database.Person>.Comparer);

            public System.Collections.Generic.IDictionary<string, Realms.Tests.Database.Person?> DictionaryDifferentType { get; } = new Dictionary<string, Realms.Tests.Database.Person?>();

            private Realms.Tests.Database.Person? _linkDifferentType;
            public Realms.Tests.Database.Person? LinkDifferentType
            {
                get => _linkDifferentType;
                set
                {
                    _linkDifferentType = value;
                    RaisePropertyChanged("LinkDifferentType");
                }
            }

            public System.Linq.IQueryable<Realms.Tests.Database.TestNotificationObject> Backlink => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects.");

            public TestNotificationObjectUnmanagedAccessor(Type objectType) : base(objectType)
            {
            }

            public override Realms.RealmValue GetValue(string propertyName)
            {
                return propertyName switch
                {
                    "StringProperty" => _stringProperty,
                    "LinkSameType" => _linkSameType,
                    "LinkDifferentType" => _linkDifferentType,
                    "Backlink" => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects."),
                    _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
                };
            }

            public override void SetValue(string propertyName, Realms.RealmValue val)
            {
                switch (propertyName)
                {
                    case "StringProperty":
                        StringProperty = (string?)val;
                        return;
                    case "LinkSameType":
                        LinkSameType = (Realms.Tests.Database.TestNotificationObject?)val;
                        return;
                    case "LinkDifferentType":
                        LinkDifferentType = (Realms.Tests.Database.Person?)val;
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
                "ListSameType" => (IList<T>)ListSameType,
                "ListDifferentType" => (IList<T>)ListDifferentType,

                                _ => throw new MissingMemberException($"The object does not have a Realm list property with name {propertyName}"),
                            };
            }

            public override ISet<T> GetSetValue<T>(string propertyName)
            {
                return propertyName switch
                            {
                "SetSameType" => (ISet<T>)SetSameType,
                "SetDifferentType" => (ISet<T>)SetDifferentType,

                                _ => throw new MissingMemberException($"The object does not have a Realm set property with name {propertyName}"),
                            };
            }

            public override IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
            {
                return propertyName switch
                {
                    "DictionarySameType" => (IDictionary<string, TValue>)DictionarySameType,
                    "DictionaryDifferentType" => (IDictionary<string, TValue>)DictionaryDifferentType,
                    _ => throw new MissingMemberException($"The object does not have a Realm dictionary property with name {propertyName}"),
                };
            }
        }
    }
}
