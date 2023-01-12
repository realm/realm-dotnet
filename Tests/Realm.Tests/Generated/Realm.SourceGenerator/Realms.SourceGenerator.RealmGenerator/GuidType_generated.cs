﻿// <auto-generated />
#nullable enable

using MongoDB.Bson;
using NUnit.Framework;
using Realms;
using Realms.Logging;
using Realms.Schema;
using Realms.Tests.Database;
using Realms.Tests.Sync;
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
    [Woven(typeof(GuidTypeObjectHelper))]
    public partial class GuidType : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("GuidType", ObjectSchema.ObjectType.RealmObject)
        {
            Realms.Schema.Property.Primitive("_id", Realms.RealmValueType.Guid, isPrimaryKey: true, isIndexed: false, isNullable: false, managedName: "Id"),
            Realms.Schema.Property.Primitive("RegularProperty", Realms.RealmValueType.Guid, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "RegularProperty"),
            Realms.Schema.Property.PrimitiveList("GuidList", Realms.RealmValueType.Guid, areElementsNullable: false, managedName: "GuidList"),
            Realms.Schema.Property.PrimitiveSet("GuidSet", Realms.RealmValueType.Guid, areElementsNullable: false, managedName: "GuidSet"),
            Realms.Schema.Property.PrimitiveDictionary("GuidDict", Realms.RealmValueType.Guid, areElementsNullable: false, managedName: "GuidDict"),
            Realms.Schema.Property.Primitive("OptionalProperty", Realms.RealmValueType.Guid, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "OptionalProperty"),
            Realms.Schema.Property.PrimitiveList("OptionalList", Realms.RealmValueType.Guid, areElementsNullable: true, managedName: "OptionalList"),
            Realms.Schema.Property.PrimitiveSet("OptionalSet", Realms.RealmValueType.Guid, areElementsNullable: true, managedName: "OptionalSet"),
            Realms.Schema.Property.PrimitiveDictionary("OptionalDict", Realms.RealmValueType.Guid, areElementsNullable: true, managedName: "OptionalDict"),
            Realms.Schema.Property.Object("LinkProperty", "GuidType", managedName: "LinkProperty"),
            Realms.Schema.Property.RealmValue("MixedProperty", managedName: "MixedProperty"),
            Realms.Schema.Property.RealmValueList("MixedList", managedName: "MixedList"),
            Realms.Schema.Property.RealmValueSet("MixedSet", managedName: "MixedSet"),
            Realms.Schema.Property.RealmValueDictionary("MixedDict", managedName: "MixedDict"),
            Realms.Schema.Property.Object("EmbeddedProperty", "EmbeddedGuidType", managedName: "EmbeddedProperty"),
        }.Build();

        #region IRealmObject implementation

        private IGuidTypeAccessor _accessor = null!;

        Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

        internal IGuidTypeAccessor Accessor => _accessor ??= new GuidTypeUnmanagedAccessor(typeof(GuidType));

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
            var newAccessor = (IGuidTypeAccessor)managedAccessor;
            var oldAccessor = _accessor;
            _accessor = newAccessor;

            if (helper != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.GuidList.Clear();
                    newAccessor.GuidSet.Clear();
                    newAccessor.GuidDict.Clear();
                    newAccessor.OptionalList.Clear();
                    newAccessor.OptionalSet.Clear();
                    newAccessor.OptionalDict.Clear();
                    newAccessor.MixedList.Clear();
                    newAccessor.MixedSet.Clear();
                    newAccessor.MixedDict.Clear();
                }

                newAccessor.Id = oldAccessor.Id;
                newAccessor.RegularProperty = oldAccessor.RegularProperty;
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.GuidList, newAccessor.GuidList, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.GuidSet, newAccessor.GuidSet, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.GuidDict, newAccessor.GuidDict, update, skipDefaults);
                newAccessor.OptionalProperty = oldAccessor.OptionalProperty;
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.OptionalList, newAccessor.OptionalList, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.OptionalSet, newAccessor.OptionalSet, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.OptionalDict, newAccessor.OptionalDict, update, skipDefaults);
                if(oldAccessor.LinkProperty != null)
                {
                    newAccessor.Realm.Add(oldAccessor.LinkProperty, update);
                }
                newAccessor.LinkProperty = oldAccessor.LinkProperty!;
                newAccessor.MixedProperty = oldAccessor.MixedProperty;
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.MixedList, newAccessor.MixedList, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.MixedSet, newAccessor.MixedSet, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.MixedDict, newAccessor.MixedDict, update, skipDefaults);
                newAccessor.EmbeddedProperty = oldAccessor.EmbeddedProperty!;
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

        public static explicit operator GuidType(Realms.RealmValue val) => val.AsRealmObject<GuidType>();

        public static implicit operator Realms.RealmValue(GuidType? val) => Realms.RealmValue.Object(val);

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
        private class GuidTypeObjectHelper : Realms.Weaving.IRealmObjectHelper
        {
            public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public Realms.ManagedAccessor CreateAccessor() => new GuidTypeManagedAccessor();

            public Realms.IRealmObjectBase CreateInstance() => new GuidType();

            public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out object? value)
            {
                value = ((IGuidTypeAccessor)instance.Accessor).Id;
                return true;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal interface IGuidTypeAccessor : Realms.IRealmAccessor
        {
            System.Guid Id { get; set; }

            System.Guid RegularProperty { get; set; }

            System.Collections.Generic.IList<System.Guid> GuidList { get; }

            System.Collections.Generic.ISet<System.Guid> GuidSet { get; }

            System.Collections.Generic.IDictionary<string, System.Guid> GuidDict { get; }

            System.Guid? OptionalProperty { get; set; }

            System.Collections.Generic.IList<System.Guid?> OptionalList { get; }

            System.Collections.Generic.ISet<System.Guid?> OptionalSet { get; }

            System.Collections.Generic.IDictionary<string, System.Guid?> OptionalDict { get; }

            Realms.Tests.Database.GuidType LinkProperty { get; set; }

            Realms.RealmValue MixedProperty { get; set; }

            System.Collections.Generic.IList<Realms.RealmValue> MixedList { get; }

            System.Collections.Generic.ISet<Realms.RealmValue> MixedSet { get; }

            System.Collections.Generic.IDictionary<string, Realms.RealmValue> MixedDict { get; }

            Realms.Tests.Database.EmbeddedGuidType EmbeddedProperty { get; set; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class GuidTypeManagedAccessor : Realms.ManagedAccessor, IGuidTypeAccessor
        {
            public System.Guid Id
            {
                get => (System.Guid)GetValue("_id");
                set => SetValueUnique("_id", value);
            }

            public System.Guid RegularProperty
            {
                get => (System.Guid)GetValue("RegularProperty");
                set => SetValue("RegularProperty", value);
            }

            private System.Collections.Generic.IList<System.Guid> _guidList = null!;
            public System.Collections.Generic.IList<System.Guid> GuidList
            {
                get
                {
                    if (_guidList == null)
                    {
                        _guidList = GetListValue<System.Guid>("GuidList");
                    }

                    return _guidList;
                }
            }

            private System.Collections.Generic.ISet<System.Guid> _guidSet = null!;
            public System.Collections.Generic.ISet<System.Guid> GuidSet
            {
                get
                {
                    if (_guidSet == null)
                    {
                        _guidSet = GetSetValue<System.Guid>("GuidSet");
                    }

                    return _guidSet;
                }
            }

            private System.Collections.Generic.IDictionary<string, System.Guid> _guidDict = null!;
            public System.Collections.Generic.IDictionary<string, System.Guid> GuidDict
            {
                get
                {
                    if (_guidDict == null)
                    {
                        _guidDict = GetDictionaryValue<System.Guid>("GuidDict");
                    }

                    return _guidDict;
                }
            }

            public System.Guid? OptionalProperty
            {
                get => (System.Guid?)GetValue("OptionalProperty");
                set => SetValue("OptionalProperty", value);
            }

            private System.Collections.Generic.IList<System.Guid?> _optionalList = null!;
            public System.Collections.Generic.IList<System.Guid?> OptionalList
            {
                get
                {
                    if (_optionalList == null)
                    {
                        _optionalList = GetListValue<System.Guid?>("OptionalList");
                    }

                    return _optionalList;
                }
            }

            private System.Collections.Generic.ISet<System.Guid?> _optionalSet = null!;
            public System.Collections.Generic.ISet<System.Guid?> OptionalSet
            {
                get
                {
                    if (_optionalSet == null)
                    {
                        _optionalSet = GetSetValue<System.Guid?>("OptionalSet");
                    }

                    return _optionalSet;
                }
            }

            private System.Collections.Generic.IDictionary<string, System.Guid?> _optionalDict = null!;
            public System.Collections.Generic.IDictionary<string, System.Guid?> OptionalDict
            {
                get
                {
                    if (_optionalDict == null)
                    {
                        _optionalDict = GetDictionaryValue<System.Guid?>("OptionalDict");
                    }

                    return _optionalDict;
                }
            }

            public Realms.Tests.Database.GuidType LinkProperty
            {
                get => (Realms.Tests.Database.GuidType)GetValue("LinkProperty");
                set => SetValue("LinkProperty", value);
            }

            public Realms.RealmValue MixedProperty
            {
                get => (Realms.RealmValue)GetValue("MixedProperty");
                set => SetValue("MixedProperty", value);
            }

            private System.Collections.Generic.IList<Realms.RealmValue> _mixedList = null!;
            public System.Collections.Generic.IList<Realms.RealmValue> MixedList
            {
                get
                {
                    if (_mixedList == null)
                    {
                        _mixedList = GetListValue<Realms.RealmValue>("MixedList");
                    }

                    return _mixedList;
                }
            }

            private System.Collections.Generic.ISet<Realms.RealmValue> _mixedSet = null!;
            public System.Collections.Generic.ISet<Realms.RealmValue> MixedSet
            {
                get
                {
                    if (_mixedSet == null)
                    {
                        _mixedSet = GetSetValue<Realms.RealmValue>("MixedSet");
                    }

                    return _mixedSet;
                }
            }

            private System.Collections.Generic.IDictionary<string, Realms.RealmValue> _mixedDict = null!;
            public System.Collections.Generic.IDictionary<string, Realms.RealmValue> MixedDict
            {
                get
                {
                    if (_mixedDict == null)
                    {
                        _mixedDict = GetDictionaryValue<Realms.RealmValue>("MixedDict");
                    }

                    return _mixedDict;
                }
            }

            public Realms.Tests.Database.EmbeddedGuidType EmbeddedProperty
            {
                get => (Realms.Tests.Database.EmbeddedGuidType)GetValue("EmbeddedProperty");
                set => SetValue("EmbeddedProperty", value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class GuidTypeUnmanagedAccessor : Realms.UnmanagedAccessor, IGuidTypeAccessor
        {
            public override ObjectSchema ObjectSchema => GuidType.RealmSchema;

            private System.Guid _id;
            public System.Guid Id
            {
                get => _id;
                set
                {
                    _id = value;
                    RaisePropertyChanged("Id");
                }
            }

            private System.Guid _regularProperty;
            public System.Guid RegularProperty
            {
                get => _regularProperty;
                set
                {
                    _regularProperty = value;
                    RaisePropertyChanged("RegularProperty");
                }
            }

            public System.Collections.Generic.IList<System.Guid> GuidList { get; } = new List<System.Guid>();

            public System.Collections.Generic.ISet<System.Guid> GuidSet { get; } = new HashSet<System.Guid>(RealmSet<System.Guid>.Comparer);

            public System.Collections.Generic.IDictionary<string, System.Guid> GuidDict { get; } = new Dictionary<string, System.Guid>();

            private System.Guid? _optionalProperty;
            public System.Guid? OptionalProperty
            {
                get => _optionalProperty;
                set
                {
                    _optionalProperty = value;
                    RaisePropertyChanged("OptionalProperty");
                }
            }

            public System.Collections.Generic.IList<System.Guid?> OptionalList { get; } = new List<System.Guid?>();

            public System.Collections.Generic.ISet<System.Guid?> OptionalSet { get; } = new HashSet<System.Guid?>(RealmSet<System.Guid?>.Comparer);

            public System.Collections.Generic.IDictionary<string, System.Guid?> OptionalDict { get; } = new Dictionary<string, System.Guid?>();

            private Realms.Tests.Database.GuidType _linkProperty = null!;
            public Realms.Tests.Database.GuidType LinkProperty
            {
                get => _linkProperty;
                set
                {
                    _linkProperty = value;
                    RaisePropertyChanged("LinkProperty");
                }
            }

            private Realms.RealmValue _mixedProperty;
            public Realms.RealmValue MixedProperty
            {
                get => _mixedProperty;
                set
                {
                    _mixedProperty = value;
                    RaisePropertyChanged("MixedProperty");
                }
            }

            public System.Collections.Generic.IList<Realms.RealmValue> MixedList { get; } = new List<Realms.RealmValue>();

            public System.Collections.Generic.ISet<Realms.RealmValue> MixedSet { get; } = new HashSet<Realms.RealmValue>(RealmSet<Realms.RealmValue>.Comparer);

            public System.Collections.Generic.IDictionary<string, Realms.RealmValue> MixedDict { get; } = new Dictionary<string, Realms.RealmValue>();

            private Realms.Tests.Database.EmbeddedGuidType _embeddedProperty = null!;
            public Realms.Tests.Database.EmbeddedGuidType EmbeddedProperty
            {
                get => _embeddedProperty;
                set
                {
                    _embeddedProperty = value;
                    RaisePropertyChanged("EmbeddedProperty");
                }
            }

            public GuidTypeUnmanagedAccessor(Type objectType) : base(objectType)
            {
            }

            public override Realms.RealmValue GetValue(string propertyName)
            {
                return propertyName switch
                {
                    "_id" => _id,
                    "RegularProperty" => _regularProperty,
                    "OptionalProperty" => _optionalProperty,
                    "LinkProperty" => _linkProperty,
                    "MixedProperty" => _mixedProperty,
                    "EmbeddedProperty" => _embeddedProperty,
                    _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
                };
            }

            public override void SetValue(string propertyName, Realms.RealmValue val)
            {
                switch (propertyName)
                {
                    case "_id":
                        throw new InvalidOperationException("Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique");
                    case "RegularProperty":
                        RegularProperty = (System.Guid)val;
                        return;
                    case "OptionalProperty":
                        OptionalProperty = (System.Guid?)val;
                        return;
                    case "LinkProperty":
                        LinkProperty = (Realms.Tests.Database.GuidType)val;
                        return;
                    case "MixedProperty":
                        MixedProperty = (Realms.RealmValue)val;
                        return;
                    case "EmbeddedProperty":
                        EmbeddedProperty = (Realms.Tests.Database.EmbeddedGuidType)val;
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

                Id = (System.Guid)val;
            }

            public override IList<T> GetListValue<T>(string propertyName)
            {
                return propertyName switch
                            {
                "GuidList" => (IList<T>)GuidList,
                "OptionalList" => (IList<T>)OptionalList,
                "MixedList" => (IList<T>)MixedList,

                                _ => throw new MissingMemberException($"The object does not have a Realm list property with name {propertyName}"),
                            };
            }

            public override ISet<T> GetSetValue<T>(string propertyName)
            {
                return propertyName switch
                            {
                "GuidSet" => (ISet<T>)GuidSet,
                "OptionalSet" => (ISet<T>)OptionalSet,
                "MixedSet" => (ISet<T>)MixedSet,

                                _ => throw new MissingMemberException($"The object does not have a Realm set property with name {propertyName}"),
                            };
            }

            public override IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
            {
                return propertyName switch
                {
                    "GuidDict" => (IDictionary<string, TValue>)GuidDict,
                    "OptionalDict" => (IDictionary<string, TValue>)OptionalDict,
                    "MixedDict" => (IDictionary<string, TValue>)MixedDict,
                    _ => throw new MissingMemberException($"The object does not have a Realm dictionary property with name {propertyName}"),
                };
            }
        }
    }
}
