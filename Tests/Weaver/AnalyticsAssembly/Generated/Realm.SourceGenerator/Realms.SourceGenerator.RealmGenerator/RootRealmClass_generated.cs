﻿// <auto-generated />
#nullable enable

using Realms;
using Realms.Schema;
using Realms.Sync;
using Realms.Sync.ErrorHandling;
using Realms.Weaving;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TestAsymmetricObject = Realms.IAsymmetricObject;
using TestEmbeddedObject = Realms.IEmbeddedObject;
using TestRealmObject = Realms.IRealmObject;

[Generated]
[Woven(typeof(RootRealmClassObjectHelper)), Realms.Preserve(AllMembers = true)]
public partial class RootRealmClass : IRealmObject, INotifyPropertyChanged, IReflectableType
{
    /// <summary>
    /// Defines the schema for the <see cref="RootRealmClass"/> class.
    /// </summary>
    public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("RootRealmClass", ObjectSchema.ObjectType.RealmObject)
    {
        Realms.Schema.Property.Object("JustForRef", "JustForObjectReference", managedName: "JustForRef"),
        Realms.Schema.Property.ObjectList("ReferenceList", "JustForObjectReference", managedName: "ReferenceList"),
        Realms.Schema.Property.PrimitiveList("PrimitiveList", Realms.RealmValueType.Int, areElementsNullable: false, managedName: "PrimitiveList"),
        Realms.Schema.Property.ObjectDictionary("ReferenceDictionary", "JustForObjectReference", managedName: "ReferenceDictionary"),
        Realms.Schema.Property.PrimitiveDictionary("PrimitiveDictionary", Realms.RealmValueType.Int, areElementsNullable: false, managedName: "PrimitiveDictionary"),
        Realms.Schema.Property.ObjectSet("ReferenceSet", "JustForObjectReference", managedName: "ReferenceSet"),
        Realms.Schema.Property.PrimitiveSet("PrimitiveSet", Realms.RealmValueType.Int, areElementsNullable: false, managedName: "PrimitiveSet"),
        Realms.Schema.Property.Primitive("Counter", Realms.RealmValueType.Int, isPrimaryKey: false, indexType: IndexType.None, isNullable: false, managedName: "Counter"),
        Realms.Schema.Property.RealmValue("RealmValue", managedName: "RealmValue"),
        Realms.Schema.Property.Backlinks("JustBackLink", "JustForObjectReference", "UseAsBacklink", managedName: "JustBackLink"),
    }.Build();

    #region IRealmObject implementation

    private IRootRealmClassAccessor? _accessor;

    Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

    private IRootRealmClassAccessor Accessor => _accessor ??= new RootRealmClassUnmanagedAccessor(typeof(RootRealmClass));

    /// <inheritdoc />
    [IgnoreDataMember, XmlIgnore]
    public bool IsManaged => Accessor.IsManaged;

    /// <inheritdoc />
    [IgnoreDataMember, XmlIgnore]
    public bool IsValid => Accessor.IsValid;

    /// <inheritdoc />
    [IgnoreDataMember, XmlIgnore]
    public bool IsFrozen => Accessor.IsFrozen;

    /// <inheritdoc />
    [IgnoreDataMember, XmlIgnore]
    public Realms.Realm? Realm => Accessor.Realm;

    /// <inheritdoc />
    [IgnoreDataMember, XmlIgnore]
    public Realms.Schema.ObjectSchema ObjectSchema => Accessor.ObjectSchema!;

    /// <inheritdoc />
    [IgnoreDataMember, XmlIgnore]
    public Realms.DynamicObjectApi DynamicApi => Accessor.DynamicApi;

    /// <inheritdoc />
    [IgnoreDataMember, XmlIgnore]
    public int BacklinksCount => Accessor.BacklinksCount;

    void ISettableManagedAccessor.SetManagedAccessor(Realms.IRealmAccessor managedAccessor, Realms.Weaving.IRealmObjectHelper? helper, bool update, bool skipDefaults)
    {
        var newAccessor = (IRootRealmClassAccessor)managedAccessor;
        var oldAccessor = _accessor;
        _accessor = newAccessor;

        if (helper != null && oldAccessor != null)
        {
            if (!skipDefaults)
            {
                newAccessor.ReferenceList.Clear();
                newAccessor.PrimitiveList.Clear();
                newAccessor.ReferenceDictionary.Clear();
                newAccessor.PrimitiveDictionary.Clear();
                newAccessor.ReferenceSet.Clear();
                newAccessor.PrimitiveSet.Clear();
            }

            if (oldAccessor.JustForRef != null && newAccessor.Realm != null)
            {
                newAccessor.Realm.Add(oldAccessor.JustForRef, update);
            }
            newAccessor.JustForRef = oldAccessor.JustForRef;
            Realms.CollectionExtensions.PopulateCollection(oldAccessor.ReferenceList, newAccessor.ReferenceList, update, skipDefaults);
            Realms.CollectionExtensions.PopulateCollection(oldAccessor.PrimitiveList, newAccessor.PrimitiveList, update, skipDefaults);
            Realms.CollectionExtensions.PopulateCollection(oldAccessor.ReferenceDictionary, newAccessor.ReferenceDictionary, update, skipDefaults);
            Realms.CollectionExtensions.PopulateCollection(oldAccessor.PrimitiveDictionary, newAccessor.PrimitiveDictionary, update, skipDefaults);
            Realms.CollectionExtensions.PopulateCollection(oldAccessor.ReferenceSet, newAccessor.ReferenceSet, update, skipDefaults);
            Realms.CollectionExtensions.PopulateCollection(oldAccessor.PrimitiveSet, newAccessor.PrimitiveSet, update, skipDefaults);
            if (!skipDefaults || oldAccessor.Counter != default(Realms.RealmInteger<int>))
            {
                newAccessor.Counter = oldAccessor.Counter;
            }
            newAccessor.RealmValue = oldAccessor.RealmValue;
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

    /// <inheritdoc />
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

    /// <summary>
    /// Converts a <see cref="Realms.RealmValue"/> to <see cref="RootRealmClass"/>. Equivalent to <see cref="Realms.RealmValue.AsNullableRealmObject{T}"/>.
    /// </summary>
    /// <param name="val">The <see cref="Realms.RealmValue"/> to convert.</param>
    /// <returns>The <see cref="RootRealmClass"/> stored in the <see cref="Realms.RealmValue"/>.</returns>
    public static explicit operator RootRealmClass?(Realms.RealmValue val) => val.Type == Realms.RealmValueType.Null ? null : val.AsRealmObject<RootRealmClass>();

    /// <summary>
    /// Implicitly constructs a <see cref="Realms.RealmValue"/> from <see cref="RootRealmClass"/>.
    /// </summary>
    /// <param name="val">The value to store in the <see cref="Realms.RealmValue"/>.</param>
    /// <returns>A <see cref="Realms.RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
    public static implicit operator Realms.RealmValue(RootRealmClass? val) => val == null ? Realms.RealmValue.Null : Realms.RealmValue.Object(val);

    /// <summary>
    /// Implicitly constructs a <see cref="Realms.QueryArgument"/> from <see cref="RootRealmClass"/>.
    /// </summary>
    /// <param name="val">The value to store in the <see cref="Realms.QueryArgument"/>.</param>
    /// <returns>A <see cref="Realms.QueryArgument"/> containing the supplied <paramref name="val"/>.</returns>
    public static implicit operator Realms.QueryArgument(RootRealmClass? val) => (Realms.RealmValue)val;

    /// <inheritdoc />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public TypeInfo GetTypeInfo() => Accessor.GetTypeInfo(this);

    /// <inheritdoc />
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

    /// <inheritdoc />
    public override int GetHashCode() => IsManaged ? Accessor.GetHashCode() : base.GetHashCode();

    /// <inheritdoc />
    public override string? ToString() => Accessor.ToString();

    [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
    private class RootRealmClassObjectHelper : Realms.Weaving.IRealmObjectHelper
    {
        public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
        {
            throw new InvalidOperationException("This method should not be called for source generated classes.");
        }

        public Realms.ManagedAccessor CreateAccessor() => new RootRealmClassManagedAccessor();

        public Realms.IRealmObjectBase CreateInstance() => new RootRealmClass();

        public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out RealmValue value)
        {
            value = RealmValue.Null;
            return false;
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
    internal interface IRootRealmClassAccessor : Realms.IRealmAccessor
    {
        JustForObjectReference? JustForRef { get; set; }

        System.Collections.Generic.IList<JustForObjectReference> ReferenceList { get; }

        System.Collections.Generic.IList<int> PrimitiveList { get; }

        System.Collections.Generic.IDictionary<string, JustForObjectReference?> ReferenceDictionary { get; }

        System.Collections.Generic.IDictionary<string, int> PrimitiveDictionary { get; }

        System.Collections.Generic.ISet<JustForObjectReference> ReferenceSet { get; }

        System.Collections.Generic.ISet<int> PrimitiveSet { get; }

        Realms.RealmInteger<int> Counter { get; set; }

        Realms.RealmValue RealmValue { get; set; }

        System.Linq.IQueryable<JustForObjectReference> JustBackLink { get; }
    }

    [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
    internal class RootRealmClassManagedAccessor : Realms.ManagedAccessor, IRootRealmClassAccessor
    {
        public JustForObjectReference? JustForRef
        {
            get => (JustForObjectReference?)GetValue("JustForRef");
            set => SetValue("JustForRef", value);
        }

        private System.Collections.Generic.IList<JustForObjectReference> _referenceList = null!;
        public System.Collections.Generic.IList<JustForObjectReference> ReferenceList
        {
            get
            {
                if (_referenceList == null)
                {
                    _referenceList = GetListValue<JustForObjectReference>("ReferenceList");
                }

                return _referenceList;
            }
        }

        private System.Collections.Generic.IList<int> _primitiveList = null!;
        public System.Collections.Generic.IList<int> PrimitiveList
        {
            get
            {
                if (_primitiveList == null)
                {
                    _primitiveList = GetListValue<int>("PrimitiveList");
                }

                return _primitiveList;
            }
        }

        private System.Collections.Generic.IDictionary<string, JustForObjectReference?> _referenceDictionary = null!;
        public System.Collections.Generic.IDictionary<string, JustForObjectReference?> ReferenceDictionary
        {
            get
            {
                if (_referenceDictionary == null)
                {
                    _referenceDictionary = GetDictionaryValue<JustForObjectReference?>("ReferenceDictionary");
                }

                return _referenceDictionary;
            }
        }

        private System.Collections.Generic.IDictionary<string, int> _primitiveDictionary = null!;
        public System.Collections.Generic.IDictionary<string, int> PrimitiveDictionary
        {
            get
            {
                if (_primitiveDictionary == null)
                {
                    _primitiveDictionary = GetDictionaryValue<int>("PrimitiveDictionary");
                }

                return _primitiveDictionary;
            }
        }

        private System.Collections.Generic.ISet<JustForObjectReference> _referenceSet = null!;
        public System.Collections.Generic.ISet<JustForObjectReference> ReferenceSet
        {
            get
            {
                if (_referenceSet == null)
                {
                    _referenceSet = GetSetValue<JustForObjectReference>("ReferenceSet");
                }

                return _referenceSet;
            }
        }

        private System.Collections.Generic.ISet<int> _primitiveSet = null!;
        public System.Collections.Generic.ISet<int> PrimitiveSet
        {
            get
            {
                if (_primitiveSet == null)
                {
                    _primitiveSet = GetSetValue<int>("PrimitiveSet");
                }

                return _primitiveSet;
            }
        }

        public Realms.RealmInteger<int> Counter
        {
            get => (Realms.RealmInteger<int>)GetValue("Counter");
            set => SetValue("Counter", value);
        }

        public Realms.RealmValue RealmValue
        {
            get => (Realms.RealmValue)GetValue("RealmValue");
            set => SetValue("RealmValue", value);
        }

        private System.Linq.IQueryable<JustForObjectReference> _justBackLink = null!;
        public System.Linq.IQueryable<JustForObjectReference> JustBackLink
        {
            get
            {
                if (_justBackLink == null)
                {
                    _justBackLink = GetBacklinks<JustForObjectReference>("JustBackLink");
                }

                return _justBackLink;
            }
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
    internal class RootRealmClassUnmanagedAccessor : Realms.UnmanagedAccessor, IRootRealmClassAccessor
    {
        public override ObjectSchema ObjectSchema => RootRealmClass.RealmSchema;

        private JustForObjectReference? _justForRef;
        public JustForObjectReference? JustForRef
        {
            get => _justForRef;
            set
            {
                _justForRef = value;
                RaisePropertyChanged("JustForRef");
            }
        }

        public System.Collections.Generic.IList<JustForObjectReference> ReferenceList { get; } = new List<JustForObjectReference>();

        public System.Collections.Generic.IList<int> PrimitiveList { get; } = new List<int>();

        public System.Collections.Generic.IDictionary<string, JustForObjectReference?> ReferenceDictionary { get; } = new Dictionary<string, JustForObjectReference?>();

        public System.Collections.Generic.IDictionary<string, int> PrimitiveDictionary { get; } = new Dictionary<string, int>();

        public System.Collections.Generic.ISet<JustForObjectReference> ReferenceSet { get; } = new HashSet<JustForObjectReference>(RealmSet<JustForObjectReference>.Comparer);

        public System.Collections.Generic.ISet<int> PrimitiveSet { get; } = new HashSet<int>(RealmSet<int>.Comparer);

        private Realms.RealmInteger<int> _counter;
        public Realms.RealmInteger<int> Counter
        {
            get => _counter;
            set
            {
                _counter = value;
                RaisePropertyChanged("Counter");
            }
        }

        private Realms.RealmValue _realmValue;
        public Realms.RealmValue RealmValue
        {
            get => _realmValue;
            set
            {
                _realmValue = value;
                RaisePropertyChanged("RealmValue");
            }
        }

        public System.Linq.IQueryable<JustForObjectReference> JustBackLink => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects.");

        public RootRealmClassUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override Realms.RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "JustForRef" => _justForRef,
                "Counter" => _counter,
                "RealmValue" => _realmValue,
                "JustBackLink" => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects."),
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }

        public override void SetValue(string propertyName, Realms.RealmValue val)
        {
            switch (propertyName)
            {
                case "JustForRef":
                    JustForRef = (JustForObjectReference?)val;
                    return;
                case "Counter":
                    Counter = (Realms.RealmInteger<int>)val;
                    return;
                case "RealmValue":
                    RealmValue = (Realms.RealmValue)val;
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
                "ReferenceList" => (IList<T>)ReferenceList,
                "PrimitiveList" => (IList<T>)PrimitiveList,
                _ => throw new MissingMemberException($"The object does not have a Realm list property with name {propertyName}"),
            };
        }

        public override ISet<T> GetSetValue<T>(string propertyName)
        {
            return propertyName switch
            {
                "ReferenceSet" => (ISet<T>)ReferenceSet,
                "PrimitiveSet" => (ISet<T>)PrimitiveSet,
                _ => throw new MissingMemberException($"The object does not have a Realm set property with name {propertyName}"),
            };
        }

        public override IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
        {
            return propertyName switch
            {
                "ReferenceDictionary" => (IDictionary<string, TValue>)ReferenceDictionary,
                "PrimitiveDictionary" => (IDictionary<string, TValue>)PrimitiveDictionary,
                _ => throw new MissingMemberException($"The object does not have a Realm dictionary property with name {propertyName}"),
            };
        }
    }
}
