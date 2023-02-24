﻿// <auto-generated />
#nullable enable

using Realms;
using Realms.Schema;
using Realms.Weaving;
using SourceGeneratorAssemblyToProcess;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace SourceGeneratorAssemblyToProcess
{
    [Generated]
    [Woven(typeof(NullableClassObjectHelper))]
    public partial class NullableClass : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("NullableClass", ObjectSchema.ObjectType.RealmObject)
        {
            Realms.Schema.Property.Primitive("NonNullableInt", Realms.RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "NonNullableInt"),
            Realms.Schema.Property.Primitive("NullableInt", Realms.RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "NullableInt"),
            Realms.Schema.Property.Primitive("NonNullableString", Realms.RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "NonNullableString"),
            Realms.Schema.Property.Primitive("NullableString", Realms.RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "NullableString"),
            Realms.Schema.Property.Primitive("NonNullableData", Realms.RealmValueType.Data, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "NonNullableData"),
            Realms.Schema.Property.Primitive("NullableData", Realms.RealmValueType.Data, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "NullableData"),
            Realms.Schema.Property.PrimitiveList("CollectionOfNullableInt", Realms.RealmValueType.Int, areElementsNullable: true, managedName: "CollectionOfNullableInt"),
            Realms.Schema.Property.PrimitiveList("CollectionOfNonNullableInt", Realms.RealmValueType.Int, areElementsNullable: false, managedName: "CollectionOfNonNullableInt"),
            Realms.Schema.Property.PrimitiveList("CollectionOfNullableString", Realms.RealmValueType.String, areElementsNullable: true, managedName: "CollectionOfNullableString"),
            Realms.Schema.Property.PrimitiveList("CollectionOfNonNullableString", Realms.RealmValueType.String, areElementsNullable: false, managedName: "CollectionOfNonNullableString"),
            Realms.Schema.Property.Primitive("NonNullableRealmInt", Realms.RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "NonNullableRealmInt"),
            Realms.Schema.Property.Primitive("NullableRealmInt", Realms.RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "NullableRealmInt"),
            Realms.Schema.Property.Object("NullableObject", "NullableClass", managedName: "NullableObject"),
            Realms.Schema.Property.ObjectList("ListNonNullabeObject", "NullableClass", managedName: "ListNonNullabeObject"),
            Realms.Schema.Property.ObjectSet("SetNonNullableObject", "NullableClass", managedName: "SetNonNullableObject"),
            Realms.Schema.Property.ObjectDictionary("DictionaryNullableObject", "NullableClass", managedName: "DictionaryNullableObject"),
            Realms.Schema.Property.RealmValue("NonNullableRealmValue", managedName: "NonNullableRealmValue"),
            Realms.Schema.Property.Backlinks("Backlink", "NullableClass", "NullableObject", managedName: "Backlink"),
        }.Build();

        #region IRealmObject implementation

        private INullableClassAccessor? _accessor;

        Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

        internal INullableClassAccessor Accessor => _accessor ??= new NullableClassUnmanagedAccessor(typeof(NullableClass));

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
            var newAccessor = (INullableClassAccessor)managedAccessor;
            var oldAccessor = _accessor;
            _accessor = newAccessor;

            if (helper != null && oldAccessor != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.CollectionOfNullableInt.Clear();
                    newAccessor.CollectionOfNonNullableInt.Clear();
                    newAccessor.CollectionOfNullableString.Clear();
                    newAccessor.CollectionOfNonNullableString.Clear();
                    newAccessor.ListNonNullabeObject.Clear();
                    newAccessor.SetNonNullableObject.Clear();
                    newAccessor.DictionaryNullableObject.Clear();
                }

                if(!skipDefaults || oldAccessor.NonNullableInt != default(int))
                {
                    newAccessor.NonNullableInt = oldAccessor.NonNullableInt;
                }
                newAccessor.NullableInt = oldAccessor.NullableInt;
                if(!skipDefaults || oldAccessor.NonNullableString != default(string))
                {
                    newAccessor.NonNullableString = oldAccessor.NonNullableString;
                }
                newAccessor.NullableString = oldAccessor.NullableString;
                if(!skipDefaults || oldAccessor.NonNullableData != default(byte[]))
                {
                    newAccessor.NonNullableData = oldAccessor.NonNullableData;
                }
                newAccessor.NullableData = oldAccessor.NullableData;
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.CollectionOfNullableInt, newAccessor.CollectionOfNullableInt, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.CollectionOfNonNullableInt, newAccessor.CollectionOfNonNullableInt, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.CollectionOfNullableString, newAccessor.CollectionOfNullableString, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.CollectionOfNonNullableString, newAccessor.CollectionOfNonNullableString, update, skipDefaults);
                newAccessor.NonNullableRealmInt = oldAccessor.NonNullableRealmInt;
                newAccessor.NullableRealmInt = oldAccessor.NullableRealmInt;
                if(oldAccessor.NullableObject != null)
                {
                    newAccessor.Realm.Add(oldAccessor.NullableObject, update);
                }
                newAccessor.NullableObject = oldAccessor.NullableObject;
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.ListNonNullabeObject, newAccessor.ListNonNullabeObject, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.SetNonNullableObject, newAccessor.SetNonNullableObject, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.DictionaryNullableObject, newAccessor.DictionaryNullableObject, update, skipDefaults);
                newAccessor.NonNullableRealmValue = oldAccessor.NonNullableRealmValue;
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

        public static explicit operator NullableClass(Realms.RealmValue val) => val.AsRealmObject<NullableClass>();

        public static implicit operator Realms.RealmValue(NullableClass? val) => val == null ? Realms.RealmValue.Null : Realms.RealmValue.Object(val);

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
        private class NullableClassObjectHelper : Realms.Weaving.IRealmObjectHelper
        {
            public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public Realms.ManagedAccessor CreateAccessor() => new NullableClassManagedAccessor();

            public Realms.IRealmObjectBase CreateInstance() => new NullableClass();

            public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out RealmValue value)
            {
                value = RealmValue.Null;
                return false;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal interface INullableClassAccessor : Realms.IRealmAccessor
        {
            int NonNullableInt { get; set; }

            int? NullableInt { get; set; }

            string NonNullableString { get; set; }

            string? NullableString { get; set; }

            byte[] NonNullableData { get; set; }

            byte[]? NullableData { get; set; }

            System.Collections.Generic.IList<int?> CollectionOfNullableInt { get; }

            System.Collections.Generic.IList<int> CollectionOfNonNullableInt { get; }

            System.Collections.Generic.IList<string?> CollectionOfNullableString { get; }

            System.Collections.Generic.IList<string> CollectionOfNonNullableString { get; }

            Realms.RealmInteger<int> NonNullableRealmInt { get; set; }

            Realms.RealmInteger<int>? NullableRealmInt { get; set; }

            SourceGeneratorAssemblyToProcess.NullableClass? NullableObject { get; set; }

            System.Collections.Generic.IList<SourceGeneratorAssemblyToProcess.NullableClass> ListNonNullabeObject { get; }

            System.Collections.Generic.ISet<SourceGeneratorAssemblyToProcess.NullableClass> SetNonNullableObject { get; }

            System.Collections.Generic.IDictionary<string, SourceGeneratorAssemblyToProcess.NullableClass?> DictionaryNullableObject { get; }

            Realms.RealmValue NonNullableRealmValue { get; set; }

            System.Linq.IQueryable<SourceGeneratorAssemblyToProcess.NullableClass> Backlink { get; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class NullableClassManagedAccessor : Realms.ManagedAccessor, INullableClassAccessor
        {
            public int NonNullableInt
            {
                get => (int)GetValue("NonNullableInt");
                set => SetValue("NonNullableInt", value);
            }

            public int? NullableInt
            {
                get => (int?)GetValue("NullableInt");
                set => SetValue("NullableInt", value);
            }

            public string NonNullableString
            {
                get => (string)GetValue("NonNullableString");
                set => SetValue("NonNullableString", value);
            }

            public string? NullableString
            {
                get => (string?)GetValue("NullableString");
                set => SetValue("NullableString", value);
            }

            public byte[] NonNullableData
            {
                get => (byte[])GetValue("NonNullableData");
                set => SetValue("NonNullableData", value);
            }

            public byte[]? NullableData
            {
                get => (byte[]?)GetValue("NullableData");
                set => SetValue("NullableData", value);
            }

            private System.Collections.Generic.IList<int?> _collectionOfNullableInt = null!;
            public System.Collections.Generic.IList<int?> CollectionOfNullableInt
            {
                get
                {
                    if (_collectionOfNullableInt == null)
                    {
                        _collectionOfNullableInt = GetListValue<int?>("CollectionOfNullableInt");
                    }

                    return _collectionOfNullableInt;
                }
            }

            private System.Collections.Generic.IList<int> _collectionOfNonNullableInt = null!;
            public System.Collections.Generic.IList<int> CollectionOfNonNullableInt
            {
                get
                {
                    if (_collectionOfNonNullableInt == null)
                    {
                        _collectionOfNonNullableInt = GetListValue<int>("CollectionOfNonNullableInt");
                    }

                    return _collectionOfNonNullableInt;
                }
            }

            private System.Collections.Generic.IList<string?> _collectionOfNullableString = null!;
            public System.Collections.Generic.IList<string?> CollectionOfNullableString
            {
                get
                {
                    if (_collectionOfNullableString == null)
                    {
                        _collectionOfNullableString = GetListValue<string?>("CollectionOfNullableString");
                    }

                    return _collectionOfNullableString;
                }
            }

            private System.Collections.Generic.IList<string> _collectionOfNonNullableString = null!;
            public System.Collections.Generic.IList<string> CollectionOfNonNullableString
            {
                get
                {
                    if (_collectionOfNonNullableString == null)
                    {
                        _collectionOfNonNullableString = GetListValue<string>("CollectionOfNonNullableString");
                    }

                    return _collectionOfNonNullableString;
                }
            }

            public Realms.RealmInteger<int> NonNullableRealmInt
            {
                get => (Realms.RealmInteger<int>)GetValue("NonNullableRealmInt");
                set => SetValue("NonNullableRealmInt", value);
            }

            public Realms.RealmInteger<int>? NullableRealmInt
            {
                get => (Realms.RealmInteger<int>?)GetValue("NullableRealmInt");
                set => SetValue("NullableRealmInt", value);
            }

            public SourceGeneratorAssemblyToProcess.NullableClass? NullableObject
            {
                get => (SourceGeneratorAssemblyToProcess.NullableClass?)GetValue("NullableObject");
                set => SetValue("NullableObject", value);
            }

            private System.Collections.Generic.IList<SourceGeneratorAssemblyToProcess.NullableClass> _listNonNullabeObject = null!;
            public System.Collections.Generic.IList<SourceGeneratorAssemblyToProcess.NullableClass> ListNonNullabeObject
            {
                get
                {
                    if (_listNonNullabeObject == null)
                    {
                        _listNonNullabeObject = GetListValue<SourceGeneratorAssemblyToProcess.NullableClass>("ListNonNullabeObject");
                    }

                    return _listNonNullabeObject;
                }
            }

            private System.Collections.Generic.ISet<SourceGeneratorAssemblyToProcess.NullableClass> _setNonNullableObject = null!;
            public System.Collections.Generic.ISet<SourceGeneratorAssemblyToProcess.NullableClass> SetNonNullableObject
            {
                get
                {
                    if (_setNonNullableObject == null)
                    {
                        _setNonNullableObject = GetSetValue<SourceGeneratorAssemblyToProcess.NullableClass>("SetNonNullableObject");
                    }

                    return _setNonNullableObject;
                }
            }

            private System.Collections.Generic.IDictionary<string, SourceGeneratorAssemblyToProcess.NullableClass?> _dictionaryNullableObject = null!;
            public System.Collections.Generic.IDictionary<string, SourceGeneratorAssemblyToProcess.NullableClass?> DictionaryNullableObject
            {
                get
                {
                    if (_dictionaryNullableObject == null)
                    {
                        _dictionaryNullableObject = GetDictionaryValue<SourceGeneratorAssemblyToProcess.NullableClass?>("DictionaryNullableObject");
                    }

                    return _dictionaryNullableObject;
                }
            }

            public Realms.RealmValue NonNullableRealmValue
            {
                get => (Realms.RealmValue)GetValue("NonNullableRealmValue");
                set => SetValue("NonNullableRealmValue", value);
            }

            private System.Linq.IQueryable<SourceGeneratorAssemblyToProcess.NullableClass> _backlink = null!;
            public System.Linq.IQueryable<SourceGeneratorAssemblyToProcess.NullableClass> Backlink
            {
                get
                {
                    if (_backlink == null)
                    {
                        _backlink = GetBacklinks<SourceGeneratorAssemblyToProcess.NullableClass>("Backlink");
                    }

                    return _backlink;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class NullableClassUnmanagedAccessor : Realms.UnmanagedAccessor, INullableClassAccessor
        {
            public override ObjectSchema ObjectSchema => NullableClass.RealmSchema;

            private int _nonNullableInt;
            public int NonNullableInt
            {
                get => _nonNullableInt;
                set
                {
                    _nonNullableInt = value;
                    RaisePropertyChanged("NonNullableInt");
                }
            }

            private int? _nullableInt;
            public int? NullableInt
            {
                get => _nullableInt;
                set
                {
                    _nullableInt = value;
                    RaisePropertyChanged("NullableInt");
                }
            }

            private string _nonNullableString = null!;
            public string NonNullableString
            {
                get => _nonNullableString;
                set
                {
                    _nonNullableString = value;
                    RaisePropertyChanged("NonNullableString");
                }
            }

            private string? _nullableString;
            public string? NullableString
            {
                get => _nullableString;
                set
                {
                    _nullableString = value;
                    RaisePropertyChanged("NullableString");
                }
            }

            private byte[] _nonNullableData = null!;
            public byte[] NonNullableData
            {
                get => _nonNullableData;
                set
                {
                    _nonNullableData = value;
                    RaisePropertyChanged("NonNullableData");
                }
            }

            private byte[]? _nullableData;
            public byte[]? NullableData
            {
                get => _nullableData;
                set
                {
                    _nullableData = value;
                    RaisePropertyChanged("NullableData");
                }
            }

            public System.Collections.Generic.IList<int?> CollectionOfNullableInt { get; } = new List<int?>();

            public System.Collections.Generic.IList<int> CollectionOfNonNullableInt { get; } = new List<int>();

            public System.Collections.Generic.IList<string?> CollectionOfNullableString { get; } = new List<string?>();

            public System.Collections.Generic.IList<string> CollectionOfNonNullableString { get; } = new List<string>();

            private Realms.RealmInteger<int> _nonNullableRealmInt;
            public Realms.RealmInteger<int> NonNullableRealmInt
            {
                get => _nonNullableRealmInt;
                set
                {
                    _nonNullableRealmInt = value;
                    RaisePropertyChanged("NonNullableRealmInt");
                }
            }

            private Realms.RealmInteger<int>? _nullableRealmInt;
            public Realms.RealmInteger<int>? NullableRealmInt
            {
                get => _nullableRealmInt;
                set
                {
                    _nullableRealmInt = value;
                    RaisePropertyChanged("NullableRealmInt");
                }
            }

            private SourceGeneratorAssemblyToProcess.NullableClass? _nullableObject;
            public SourceGeneratorAssemblyToProcess.NullableClass? NullableObject
            {
                get => _nullableObject;
                set
                {
                    _nullableObject = value;
                    RaisePropertyChanged("NullableObject");
                }
            }

            public System.Collections.Generic.IList<SourceGeneratorAssemblyToProcess.NullableClass> ListNonNullabeObject { get; } = new List<SourceGeneratorAssemblyToProcess.NullableClass>();

            public System.Collections.Generic.ISet<SourceGeneratorAssemblyToProcess.NullableClass> SetNonNullableObject { get; } = new HashSet<SourceGeneratorAssemblyToProcess.NullableClass>(RealmSet<SourceGeneratorAssemblyToProcess.NullableClass>.Comparer);

            public System.Collections.Generic.IDictionary<string, SourceGeneratorAssemblyToProcess.NullableClass?> DictionaryNullableObject { get; } = new Dictionary<string, SourceGeneratorAssemblyToProcess.NullableClass?>();

            private Realms.RealmValue _nonNullableRealmValue;
            public Realms.RealmValue NonNullableRealmValue
            {
                get => _nonNullableRealmValue;
                set
                {
                    _nonNullableRealmValue = value;
                    RaisePropertyChanged("NonNullableRealmValue");
                }
            }

            public System.Linq.IQueryable<SourceGeneratorAssemblyToProcess.NullableClass> Backlink => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects.");

            public NullableClassUnmanagedAccessor(Type objectType) : base(objectType)
            {
            }

            public override Realms.RealmValue GetValue(string propertyName)
            {
                return propertyName switch
                {
                    "NonNullableInt" => _nonNullableInt,
                    "NullableInt" => _nullableInt,
                    "NonNullableString" => _nonNullableString,
                    "NullableString" => _nullableString,
                    "NonNullableData" => _nonNullableData,
                    "NullableData" => _nullableData,
                    "NonNullableRealmInt" => _nonNullableRealmInt,
                    "NullableRealmInt" => _nullableRealmInt,
                    "NullableObject" => _nullableObject,
                    "NonNullableRealmValue" => _nonNullableRealmValue,
                    "Backlink" => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects."),
                    _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
                };
            }

            public override void SetValue(string propertyName, Realms.RealmValue val)
            {
                switch (propertyName)
                {
                    case "NonNullableInt":
                        NonNullableInt = (int)val;
                        return;
                    case "NullableInt":
                        NullableInt = (int?)val;
                        return;
                    case "NonNullableString":
                        NonNullableString = (string)val;
                        return;
                    case "NullableString":
                        NullableString = (string?)val;
                        return;
                    case "NonNullableData":
                        NonNullableData = (byte[])val;
                        return;
                    case "NullableData":
                        NullableData = (byte[]?)val;
                        return;
                    case "NonNullableRealmInt":
                        NonNullableRealmInt = (Realms.RealmInteger<int>)val;
                        return;
                    case "NullableRealmInt":
                        NullableRealmInt = (Realms.RealmInteger<int>?)val;
                        return;
                    case "NullableObject":
                        NullableObject = (SourceGeneratorAssemblyToProcess.NullableClass?)val;
                        return;
                    case "NonNullableRealmValue":
                        NonNullableRealmValue = (Realms.RealmValue)val;
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
                "CollectionOfNullableInt" => (IList<T>)CollectionOfNullableInt,
                "CollectionOfNonNullableInt" => (IList<T>)CollectionOfNonNullableInt,
                "CollectionOfNullableString" => (IList<T>)CollectionOfNullableString,
                "CollectionOfNonNullableString" => (IList<T>)CollectionOfNonNullableString,
                "ListNonNullabeObject" => (IList<T>)ListNonNullabeObject,

                                _ => throw new MissingMemberException($"The object does not have a Realm list property with name {propertyName}"),
                            };
            }

            public override ISet<T> GetSetValue<T>(string propertyName)
            {
                return propertyName switch
                            {
                "SetNonNullableObject" => (ISet<T>)SetNonNullableObject,

                                _ => throw new MissingMemberException($"The object does not have a Realm set property with name {propertyName}"),
                            };
            }

            public override IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
            {
                return propertyName switch
                {
                    "DictionaryNullableObject" => (IDictionary<string, TValue>)DictionaryNullableObject,
                    _ => throw new MissingMemberException($"The object does not have a Realm dictionary property with name {propertyName}"),
                };
            }
        }
    }
}