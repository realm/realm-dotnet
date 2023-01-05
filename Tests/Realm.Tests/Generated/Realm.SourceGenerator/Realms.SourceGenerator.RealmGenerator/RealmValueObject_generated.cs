﻿// <auto-generated />
using MongoDB.Bson;
using Realms;
using Realms.IAsymmetricObject;
using Realms.IEmbeddedObject;
using Realms.IRealmObject;
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

namespace Realms.Tests
{
    [Generated]
    [Woven(typeof(RealmValueObjectObjectHelper))]
    public partial class RealmValueObject : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("RealmValueObject", ObjectSchema.ObjectType.RealmObject)
        {
            Realms.Schema.Property.Primitive("_id", Realms.RealmValueType.Int, isPrimaryKey: true, isIndexed: false, isNullable: false, managedName: "Id"),
            Realms.Schema.Property.RealmValue("RealmValueProperty", managedName: "RealmValueProperty"),
            Realms.Schema.Property.RealmValueList("RealmValueList", managedName: "RealmValueList"),
            Realms.Schema.Property.RealmValueSet("RealmValueSet", managedName: "RealmValueSet"),
            Realms.Schema.Property.RealmValueDictionary("RealmValueDictionary", managedName: "RealmValueDictionary"),
            Realms.Schema.Property.PrimitiveDictionary("TestDict", Realms.RealmValueType.Int, areElementsNullable: false, managedName: "TestDict"),
        }.Build();

        #region IRealmObject implementation

        private IRealmValueObjectAccessor _accessor;

        Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

        internal IRealmValueObjectAccessor Accessor => _accessor ?? (_accessor = new RealmValueObjectUnmanagedAccessor(typeof(RealmValueObject)));

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
            var newAccessor = (IRealmValueObjectAccessor)managedAccessor;
            var oldAccessor = (IRealmValueObjectAccessor)_accessor;
            _accessor = newAccessor;

            if (helper != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.RealmValueList.Clear();
                    newAccessor.RealmValueSet.Clear();
                    newAccessor.RealmValueDictionary.Clear();
                    newAccessor.TestDict.Clear();
                }

                if(!skipDefaults || oldAccessor.Id != default(int))
                {
                    newAccessor.Id = oldAccessor.Id;
                }
                newAccessor.RealmValueProperty = oldAccessor.RealmValueProperty;
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.RealmValueList, newAccessor.RealmValueList, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.RealmValueSet, newAccessor.RealmValueSet, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.RealmValueDictionary, newAccessor.RealmValueDictionary, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.TestDict, newAccessor.TestDict, update, skipDefaults);
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

        public static explicit operator RealmValueObject(Realms.RealmValue val) => val.AsRealmObject<RealmValueObject>();

        public static implicit operator Realms.RealmValue(RealmValueObject val) => Realms.RealmValue.Object(val);

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
        private class RealmValueObjectObjectHelper : Realms.Weaving.IRealmObjectHelper
        {
            public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public Realms.ManagedAccessor CreateAccessor() => new RealmValueObjectManagedAccessor();

            public Realms.IRealmObjectBase CreateInstance() => new RealmValueObject();

            public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out object value)
            {
                value = ((IRealmValueObjectAccessor)instance.Accessor).Id;
                return true;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal interface IRealmValueObjectAccessor : Realms.IRealmAccessor
        {
            int Id { get; set; }

            Realms.RealmValue RealmValueProperty { get; set; }

            System.Collections.Generic.IList<Realms.RealmValue> RealmValueList { get; }

            System.Collections.Generic.ISet<Realms.RealmValue> RealmValueSet { get; }

            System.Collections.Generic.IDictionary<string, Realms.RealmValue> RealmValueDictionary { get; }

            System.Collections.Generic.IDictionary<string, int> TestDict { get; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class RealmValueObjectManagedAccessor : Realms.ManagedAccessor, IRealmValueObjectAccessor
        {
            public int Id
            {
                get => (int)GetValue("_id");
                set => SetValueUnique("_id", value);
            }

            public Realms.RealmValue RealmValueProperty
            {
                get => (Realms.RealmValue)GetValue("RealmValueProperty");
                set => SetValue("RealmValueProperty", value);
            }

            private System.Collections.Generic.IList<Realms.RealmValue> _realmValueList;
            public System.Collections.Generic.IList<Realms.RealmValue> RealmValueList
            {
                get
                {
                    if (_realmValueList == null)
                    {
                        _realmValueList = GetListValue<Realms.RealmValue>("RealmValueList");
                    }

                    return _realmValueList;
                }
            }

            private System.Collections.Generic.ISet<Realms.RealmValue> _realmValueSet;
            public System.Collections.Generic.ISet<Realms.RealmValue> RealmValueSet
            {
                get
                {
                    if (_realmValueSet == null)
                    {
                        _realmValueSet = GetSetValue<Realms.RealmValue>("RealmValueSet");
                    }

                    return _realmValueSet;
                }
            }

            private System.Collections.Generic.IDictionary<string, Realms.RealmValue> _realmValueDictionary;
            public System.Collections.Generic.IDictionary<string, Realms.RealmValue> RealmValueDictionary
            {
                get
                {
                    if (_realmValueDictionary == null)
                    {
                        _realmValueDictionary = GetDictionaryValue<Realms.RealmValue>("RealmValueDictionary");
                    }

                    return _realmValueDictionary;
                }
            }

            private System.Collections.Generic.IDictionary<string, int> _testDict;
            public System.Collections.Generic.IDictionary<string, int> TestDict
            {
                get
                {
                    if (_testDict == null)
                    {
                        _testDict = GetDictionaryValue<int>("TestDict");
                    }

                    return _testDict;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class RealmValueObjectUnmanagedAccessor : Realms.UnmanagedAccessor, IRealmValueObjectAccessor
        {
            public override ObjectSchema ObjectSchema => RealmValueObject.RealmSchema;

            private int _id = TestHelpers.Random.Next();
            public int Id
            {
                get => _id;
                set
                {
                    _id = value;
                    RaisePropertyChanged("Id");
                }
            }

            private Realms.RealmValue _realmValueProperty;
            public Realms.RealmValue RealmValueProperty
            {
                get => _realmValueProperty;
                set
                {
                    _realmValueProperty = value;
                    RaisePropertyChanged("RealmValueProperty");
                }
            }

            public System.Collections.Generic.IList<Realms.RealmValue> RealmValueList { get; } = new List<Realms.RealmValue>();

            public System.Collections.Generic.ISet<Realms.RealmValue> RealmValueSet { get; } = new HashSet<Realms.RealmValue>(RealmSet<Realms.RealmValue>.Comparer);

            public System.Collections.Generic.IDictionary<string, Realms.RealmValue> RealmValueDictionary { get; } = new Dictionary<string, Realms.RealmValue>();

            public System.Collections.Generic.IDictionary<string, int> TestDict { get; } = new Dictionary<string, int>();

            public RealmValueObjectUnmanagedAccessor(Type objectType) : base(objectType)
            {
            }

            public override Realms.RealmValue GetValue(string propertyName)
            {
                return propertyName switch
                {
                    "_id" => _id,
                    "RealmValueProperty" => _realmValueProperty,
                    _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
                };
            }

            public override void SetValue(string propertyName, Realms.RealmValue val)
            {
                switch (propertyName)
                {
                    case "_id":
                        throw new InvalidOperationException("Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique");
                    case "RealmValueProperty":
                        RealmValueProperty = (Realms.RealmValue)val;
                        return;
                    default:
                        throw new MissingMemberException($"The object does not have a settable Realm property with name {propertyName}");
                }
            }

            public override void SetValueUnique(string propertyName, Realms.RealmValue val)
            {
                if (propertyName != "_id")
                {
                    throw new InvalidOperationException($"Cannot set the value of non primary key property ({propertyName}) with SetValueUnique");
                }

                Id = (int)val;
            }

            public override IList<T> GetListValue<T>(string propertyName)
            {
                return propertyName switch
                            {
                "RealmValueList" => (IList<T>)RealmValueList,

                                _ => throw new MissingMemberException($"The object does not have a Realm list property with name {propertyName}"),
                            };
            }

            public override ISet<T> GetSetValue<T>(string propertyName)
            {
                return propertyName switch
                            {
                "RealmValueSet" => (ISet<T>)RealmValueSet,

                                _ => throw new MissingMemberException($"The object does not have a Realm set property with name {propertyName}"),
                            };
            }

            public override IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
            {
                return propertyName switch
                {
                    "RealmValueDictionary" => (IDictionary<string, TValue>)RealmValueDictionary,
                    "TestDict" => (IDictionary<string, TValue>)TestDict,
                    _ => throw new MissingMemberException($"The object does not have a Realm dictionary property with name {propertyName}"),
                };
            }
        }
    }
}
