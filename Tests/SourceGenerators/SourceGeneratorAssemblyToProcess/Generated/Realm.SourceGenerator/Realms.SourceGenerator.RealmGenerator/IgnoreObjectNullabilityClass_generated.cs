﻿// <auto-generated />
#nullable enable

using MongoDB.Bson.Serialization;
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
    [Woven(typeof(IgnoreObjectNullabilityClassObjectHelper)), Realms.Preserve(AllMembers = true)]
    public partial class IgnoreObjectNullabilityClass : IRealmObject, INotifyPropertyChanged, IReflectableType
    {

        [Realms.Preserve]
        static IgnoreObjectNullabilityClass()
        {
            Realms.Serialization.RealmObjectSerializer.Register(new IgnoreObjectNullabilityClassSerializer());
        }

        /// <summary>
        /// Defines the schema for the <see cref="IgnoreObjectNullabilityClass"/> class.
        /// </summary>
        [System.Reflection.Obfuscation]
        public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("IgnoreObjectNullabilityClass", ObjectSchema.ObjectType.RealmObject)
        {
            Realms.Schema.Property.Object("NullableObject", "IgnoreObjectNullabilityClass", managedName: "NullableObject"),
            Realms.Schema.Property.Object("NonNullableObject", "IgnoreObjectNullabilityClass", managedName: "NonNullableObject"),
            Realms.Schema.Property.ObjectList("ListNonNullableObject", "IgnoreObjectNullabilityClass", managedName: "ListNonNullableObject"),
            Realms.Schema.Property.ObjectList("ListNullableObject", "IgnoreObjectNullabilityClass", managedName: "ListNullableObject"),
            Realms.Schema.Property.ObjectSet("SetNonNullableObject", "IgnoreObjectNullabilityClass", managedName: "SetNonNullableObject"),
            Realms.Schema.Property.ObjectSet("SetNullableObject", "IgnoreObjectNullabilityClass", managedName: "SetNullableObject"),
            Realms.Schema.Property.ObjectDictionary("DictionaryNonNullableObject", "IgnoreObjectNullabilityClass", managedName: "DictionaryNonNullableObject"),
            Realms.Schema.Property.ObjectDictionary("DictionaryNullableObject", "IgnoreObjectNullabilityClass", managedName: "DictionaryNullableObject"),
            Realms.Schema.Property.Backlinks("BacklinkNullableObject", "IgnoreObjectNullabilityClass", "NullableObject", managedName: "BacklinkNullableObject"),
            Realms.Schema.Property.Backlinks("BacklinkNonNullableObject", "IgnoreObjectNullabilityClass", "NullableObject", managedName: "BacklinkNonNullableObject"),
        }.Build();

        #region IRealmObject implementation

        private IIgnoreObjectNullabilityClassAccessor? _accessor;

        Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

        private IIgnoreObjectNullabilityClassAccessor Accessor => _accessor ??= new IgnoreObjectNullabilityClassUnmanagedAccessor(typeof(IgnoreObjectNullabilityClass));

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
            var newAccessor = (IIgnoreObjectNullabilityClassAccessor)managedAccessor;
            var oldAccessor = _accessor;
            _accessor = newAccessor;

            if (helper != null && oldAccessor != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.ListNonNullableObject.Clear();
                    newAccessor.ListNullableObject.Clear();
                    newAccessor.SetNonNullableObject.Clear();
                    newAccessor.SetNullableObject.Clear();
                    newAccessor.DictionaryNonNullableObject.Clear();
                    newAccessor.DictionaryNullableObject.Clear();
                }

                if (oldAccessor.NullableObject != null && newAccessor.Realm != null)
                {
                    newAccessor.Realm.Add(oldAccessor.NullableObject, update);
                }
                newAccessor.NullableObject = oldAccessor.NullableObject;
                if (oldAccessor.NonNullableObject != null && newAccessor.Realm != null)
                {
                    newAccessor.Realm.Add(oldAccessor.NonNullableObject, update);
                }
                newAccessor.NonNullableObject = oldAccessor.NonNullableObject;
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.ListNonNullableObject, newAccessor.ListNonNullableObject, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.ListNullableObject, newAccessor.ListNullableObject, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.SetNonNullableObject, newAccessor.SetNonNullableObject, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.SetNullableObject, newAccessor.SetNullableObject, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.DictionaryNonNullableObject, newAccessor.DictionaryNonNullableObject, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.DictionaryNullableObject, newAccessor.DictionaryNullableObject, update, skipDefaults);
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
        /// Converts a <see cref="Realms.RealmValue"/> to <see cref="IgnoreObjectNullabilityClass"/>. Equivalent to <see cref="Realms.RealmValue.AsNullableRealmObject{T}"/>.
        /// </summary>
        /// <param name="val">The <see cref="Realms.RealmValue"/> to convert.</param>
        /// <returns>The <see cref="IgnoreObjectNullabilityClass"/> stored in the <see cref="Realms.RealmValue"/>.</returns>
        public static explicit operator IgnoreObjectNullabilityClass?(Realms.RealmValue val) => val.Type == Realms.RealmValueType.Null ? null : val.AsRealmObject<IgnoreObjectNullabilityClass>();

        /// <summary>
        /// Implicitly constructs a <see cref="Realms.RealmValue"/> from <see cref="IgnoreObjectNullabilityClass"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="Realms.RealmValue"/>.</param>
        /// <returns>A <see cref="Realms.RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator Realms.RealmValue(IgnoreObjectNullabilityClass? val) => val == null ? Realms.RealmValue.Null : Realms.RealmValue.Object(val);

        /// <summary>
        /// Implicitly constructs a <see cref="Realms.QueryArgument"/> from <see cref="IgnoreObjectNullabilityClass"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="Realms.QueryArgument"/>.</param>
        /// <returns>A <see cref="Realms.QueryArgument"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator Realms.QueryArgument(IgnoreObjectNullabilityClass? val) => (Realms.RealmValue)val;

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

            if (!(obj is Realms.IRealmObjectBase iro))
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
        private class IgnoreObjectNullabilityClassObjectHelper : Realms.Weaving.IRealmObjectHelper
        {
            public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public Realms.ManagedAccessor CreateAccessor() => new IgnoreObjectNullabilityClassManagedAccessor();

            public Realms.IRealmObjectBase CreateInstance() => new IgnoreObjectNullabilityClass();

            public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out RealmValue value)
            {
                value = RealmValue.Null;
                return false;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        internal interface IIgnoreObjectNullabilityClassAccessor : Realms.IRealmAccessor
        {
            SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass? NullableObject { get; set; }

            SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass NonNullableObject { get; set; }

            System.Collections.Generic.IList<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass> ListNonNullableObject { get; }

            System.Collections.Generic.IList<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass?> ListNullableObject { get; }

            System.Collections.Generic.ISet<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass> SetNonNullableObject { get; }

            System.Collections.Generic.ISet<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass?> SetNullableObject { get; }

            System.Collections.Generic.IDictionary<string, SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass> DictionaryNonNullableObject { get; }

            System.Collections.Generic.IDictionary<string, SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass> DictionaryNullableObject { get; }

            System.Linq.IQueryable<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass?> BacklinkNullableObject { get; }

            System.Linq.IQueryable<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass> BacklinkNonNullableObject { get; }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class IgnoreObjectNullabilityClassManagedAccessor : Realms.ManagedAccessor, IIgnoreObjectNullabilityClassAccessor
        {
            public SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass? NullableObject
            {
                get => (SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass?)GetValue("NullableObject");
                set => SetValue("NullableObject", value);
            }

            public SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass NonNullableObject
            {
                get => (SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass)GetValue("NonNullableObject");
                set => SetValue("NonNullableObject", value);
            }

            private System.Collections.Generic.IList<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass> _listNonNullableObject = null!;
            public System.Collections.Generic.IList<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass> ListNonNullableObject
            {
                get
                {
                    if (_listNonNullableObject == null)
                    {
                        _listNonNullableObject = GetListValue<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass>("ListNonNullableObject");
                    }

                    return _listNonNullableObject;
                }
            }

            private System.Collections.Generic.IList<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass?> _listNullableObject = null!;
            public System.Collections.Generic.IList<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass?> ListNullableObject
            {
                get
                {
                    if (_listNullableObject == null)
                    {
                        _listNullableObject = GetListValue<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass?>("ListNullableObject");
                    }

                    return _listNullableObject;
                }
            }

            private System.Collections.Generic.ISet<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass> _setNonNullableObject = null!;
            public System.Collections.Generic.ISet<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass> SetNonNullableObject
            {
                get
                {
                    if (_setNonNullableObject == null)
                    {
                        _setNonNullableObject = GetSetValue<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass>("SetNonNullableObject");
                    }

                    return _setNonNullableObject;
                }
            }

            private System.Collections.Generic.ISet<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass?> _setNullableObject = null!;
            public System.Collections.Generic.ISet<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass?> SetNullableObject
            {
                get
                {
                    if (_setNullableObject == null)
                    {
                        _setNullableObject = GetSetValue<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass?>("SetNullableObject");
                    }

                    return _setNullableObject;
                }
            }

            private System.Collections.Generic.IDictionary<string, SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass> _dictionaryNonNullableObject = null!;
            public System.Collections.Generic.IDictionary<string, SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass> DictionaryNonNullableObject
            {
                get
                {
                    if (_dictionaryNonNullableObject == null)
                    {
                        _dictionaryNonNullableObject = GetDictionaryValue<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass>("DictionaryNonNullableObject");
                    }

                    return _dictionaryNonNullableObject;
                }
            }

            private System.Collections.Generic.IDictionary<string, SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass> _dictionaryNullableObject = null!;
            public System.Collections.Generic.IDictionary<string, SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass> DictionaryNullableObject
            {
                get
                {
                    if (_dictionaryNullableObject == null)
                    {
                        _dictionaryNullableObject = GetDictionaryValue<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass>("DictionaryNullableObject");
                    }

                    return _dictionaryNullableObject;
                }
            }

            private System.Linq.IQueryable<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass?> _backlinkNullableObject = null!;
            public System.Linq.IQueryable<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass?> BacklinkNullableObject
            {
                get
                {
                    if (_backlinkNullableObject == null)
                    {
                        _backlinkNullableObject = GetBacklinks<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass?>("BacklinkNullableObject");
                    }

                    return _backlinkNullableObject;
                }
            }

            private System.Linq.IQueryable<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass> _backlinkNonNullableObject = null!;
            public System.Linq.IQueryable<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass> BacklinkNonNullableObject
            {
                get
                {
                    if (_backlinkNonNullableObject == null)
                    {
                        _backlinkNonNullableObject = GetBacklinks<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass>("BacklinkNonNullableObject");
                    }

                    return _backlinkNonNullableObject;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class IgnoreObjectNullabilityClassUnmanagedAccessor : Realms.UnmanagedAccessor, IIgnoreObjectNullabilityClassAccessor
        {
            public override ObjectSchema ObjectSchema => IgnoreObjectNullabilityClass.RealmSchema;

            private SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass? _nullableObject = null!;
            public SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass? NullableObject
            {
                get => _nullableObject;
                set
                {
                    _nullableObject = value;
                    RaisePropertyChanged("NullableObject");
                }
            }

            private SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass _nonNullableObject = null!;
            public SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass NonNullableObject
            {
                get => _nonNullableObject;
                set
                {
                    _nonNullableObject = value;
                    RaisePropertyChanged("NonNullableObject");
                }
            }

            public System.Collections.Generic.IList<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass> ListNonNullableObject { get; } = new List<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass>();

            public System.Collections.Generic.IList<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass?> ListNullableObject { get; } = new List<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass?>();

            public System.Collections.Generic.ISet<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass> SetNonNullableObject { get; } = new HashSet<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass>(RealmSet<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass>.Comparer);

            public System.Collections.Generic.ISet<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass?> SetNullableObject { get; } = new HashSet<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass?>(RealmSet<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass?>.Comparer);

            public System.Collections.Generic.IDictionary<string, SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass> DictionaryNonNullableObject { get; } = new Dictionary<string, SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass>();

            public System.Collections.Generic.IDictionary<string, SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass> DictionaryNullableObject { get; } = new Dictionary<string, SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass>();

            public System.Linq.IQueryable<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass?> BacklinkNullableObject => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects.");

            public System.Linq.IQueryable<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass> BacklinkNonNullableObject => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects.");

            public IgnoreObjectNullabilityClassUnmanagedAccessor(Type objectType) : base(objectType)
            {
            }

            public override Realms.RealmValue GetValue(string propertyName)
            {
                return propertyName switch
                {
                    "NullableObject" => _nullableObject,
                    "NonNullableObject" => _nonNullableObject,
                    "BacklinkNullableObject" => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects."),
                    "BacklinkNonNullableObject" => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects."),
                    _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
                };
            }

            public override void SetValue(string propertyName, Realms.RealmValue val)
            {
                switch (propertyName)
                {
                    case "NullableObject":
                        NullableObject = (SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass?)val;
                        return;
                    case "NonNullableObject":
                        NonNullableObject = (SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass)val;
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
                    "ListNonNullableObject" => (IList<T>)ListNonNullableObject,
                    "ListNullableObject" => (IList<T>)ListNullableObject,
                    _ => throw new MissingMemberException($"The object does not have a Realm list property with name {propertyName}"),
                };
            }

            public override ISet<T> GetSetValue<T>(string propertyName)
            {
                return propertyName switch
                {
                    "SetNonNullableObject" => (ISet<T>)SetNonNullableObject,
                    "SetNullableObject" => (ISet<T>)SetNullableObject,
                    _ => throw new MissingMemberException($"The object does not have a Realm set property with name {propertyName}"),
                };
            }

            public override IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
            {
                return propertyName switch
                {
                    "DictionaryNonNullableObject" => (IDictionary<string, TValue>)DictionaryNonNullableObject,
                    "DictionaryNullableObject" => (IDictionary<string, TValue>)DictionaryNullableObject,
                    _ => throw new MissingMemberException($"The object does not have a Realm dictionary property with name {propertyName}"),
                };
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class IgnoreObjectNullabilityClassSerializer : Realms.Serialization.RealmObjectSerializerBase<IgnoreObjectNullabilityClass>
        {
            public override string SchemaName => "IgnoreObjectNullabilityClass";

            protected override void SerializeValue(MongoDB.Bson.Serialization.BsonSerializationContext context, BsonSerializationArgs args, IgnoreObjectNullabilityClass value)
            {
                context.Writer.WriteStartDocument();

                WriteValue(context, args, "NullableObject", value.NullableObject);
                WriteValue(context, args, "NonNullableObject", value.NonNullableObject);
                WriteList(context, args, "ListNonNullableObject", value.ListNonNullableObject);
                WriteList(context, args, "ListNullableObject", value.ListNullableObject);
                WriteSet(context, args, "SetNonNullableObject", value.SetNonNullableObject);
                WriteSet(context, args, "SetNullableObject", value.SetNullableObject);
                WriteDictionary(context, args, "DictionaryNonNullableObject", value.DictionaryNonNullableObject);
                WriteDictionary(context, args, "DictionaryNullableObject", value.DictionaryNullableObject);

                context.Writer.WriteEndDocument();
            }

            protected override IgnoreObjectNullabilityClass CreateInstance() => new IgnoreObjectNullabilityClass();

            protected override void ReadValue(IgnoreObjectNullabilityClass instance, string name, BsonDeserializationContext context)
            {
                switch (name)
                {
                    case "NullableObject":
                        instance.NullableObject = Realms.Serialization.RealmObjectSerializer.LookupSerializer<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass?>()!.DeserializeById(context);
                        break;
                    case "NonNullableObject":
                        instance.NonNullableObject = Realms.Serialization.RealmObjectSerializer.LookupSerializer<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass>()!.DeserializeById(context);
                        break;
                    case "ListNonNullableObject":
                    case "ListNullableObject":
                    case "SetNonNullableObject":
                    case "SetNullableObject":
                        ReadArray(instance, name, context);
                        break;
                    case "DictionaryNonNullableObject":
                    case "DictionaryNullableObject":
                        ReadDictionary(instance, name, context);
                        break;
                    default:
                        context.Reader.SkipValue();
                        break;
                }
            }

            protected override void ReadArrayElement(IgnoreObjectNullabilityClass instance, string name, BsonDeserializationContext context)
            {
                switch (name)
                {
                    case "ListNonNullableObject":
                        instance.ListNonNullableObject.Add(Realms.Serialization.RealmObjectSerializer.LookupSerializer<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass>()!.DeserializeById(context)!);
                        break;
                    case "ListNullableObject":
                        instance.ListNullableObject.Add(Realms.Serialization.RealmObjectSerializer.LookupSerializer<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass?>()!.DeserializeById(context)!);
                        break;
                    case "SetNonNullableObject":
                        instance.SetNonNullableObject.Add(Realms.Serialization.RealmObjectSerializer.LookupSerializer<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass>()!.DeserializeById(context)!);
                        break;
                    case "SetNullableObject":
                        instance.SetNullableObject.Add(Realms.Serialization.RealmObjectSerializer.LookupSerializer<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass?>()!.DeserializeById(context)!);
                        break;
                }
            }

            protected override void ReadDocumentField(IgnoreObjectNullabilityClass instance, string name, string fieldName, BsonDeserializationContext context)
            {
                switch (name)
                {
                    case "DictionaryNonNullableObject":
                        instance.DictionaryNonNullableObject[fieldName] = Realms.Serialization.RealmObjectSerializer.LookupSerializer<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass>()!.DeserializeById(context)!;
                        break;
                    case "DictionaryNullableObject":
                        instance.DictionaryNullableObject[fieldName] = Realms.Serialization.RealmObjectSerializer.LookupSerializer<SourceGeneratorAssemblyToProcess.IgnoreObjectNullabilityClass>()!.DeserializeById(context)!;
                        break;
                }
            }
        }
    }
}
