﻿// <auto-generated />
#nullable enable

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
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
using TestEmbeddedObject = Realms.IEmbeddedObject;
using TestRealmObject = Realms.IRealmObject;

namespace Realms.Tests
{
    [Generated]
    [Woven(typeof(ObjectWithEmbeddedPropertiesObjectHelper)), Realms.Preserve(AllMembers = true)]
    public partial class ObjectWithEmbeddedProperties : IRealmObject, INotifyPropertyChanged, IReflectableType
    {

        [Realms.Preserve]
        static ObjectWithEmbeddedProperties()
        {
            Realms.Serialization.RealmObjectSerializer.Register(new ObjectWithEmbeddedPropertiesSerializer());
        }

        /// <summary>
        /// Defines the schema for the <see cref="ObjectWithEmbeddedProperties"/> class.
        /// </summary>
        public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("ObjectWithEmbeddedProperties", ObjectSchema.ObjectType.RealmObject)
        {
            Realms.Schema.Property.Primitive("PrimaryKey", Realms.RealmValueType.Int, isPrimaryKey: true, indexType: IndexType.None, isNullable: false, managedName: "PrimaryKey"),
            Realms.Schema.Property.Object("AllTypesObject", "EmbeddedAllTypesObject", managedName: "AllTypesObject"),
            Realms.Schema.Property.Object("RecursiveObject", "EmbeddedLevel1", managedName: "RecursiveObject"),
            Realms.Schema.Property.ObjectList("ListOfAllTypesObjects", "EmbeddedAllTypesObject", managedName: "ListOfAllTypesObjects"),
            Realms.Schema.Property.ObjectDictionary("DictionaryOfAllTypesObjects", "EmbeddedAllTypesObject", managedName: "DictionaryOfAllTypesObjects"),
        }.Build();

        #region IRealmObject implementation

        private IObjectWithEmbeddedPropertiesAccessor? _accessor;

        Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

        private IObjectWithEmbeddedPropertiesAccessor Accessor => _accessor ??= new ObjectWithEmbeddedPropertiesUnmanagedAccessor(typeof(ObjectWithEmbeddedProperties));

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
            var newAccessor = (IObjectWithEmbeddedPropertiesAccessor)managedAccessor;
            var oldAccessor = _accessor;
            _accessor = newAccessor;

            if (helper != null && oldAccessor != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.ListOfAllTypesObjects.Clear();
                    newAccessor.DictionaryOfAllTypesObjects.Clear();
                }

                if (!skipDefaults || oldAccessor.PrimaryKey != default(int))
                {
                    newAccessor.PrimaryKey = oldAccessor.PrimaryKey;
                }
                newAccessor.AllTypesObject = oldAccessor.AllTypesObject;
                newAccessor.RecursiveObject = oldAccessor.RecursiveObject;
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.ListOfAllTypesObjects, newAccessor.ListOfAllTypesObjects, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.DictionaryOfAllTypesObjects, newAccessor.DictionaryOfAllTypesObjects, update, skipDefaults);
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
        /// Converts a <see cref="Realms.RealmValue"/> to <see cref="ObjectWithEmbeddedProperties"/>. Equivalent to <see cref="Realms.RealmValue.AsNullableRealmObject{T}"/>.
        /// </summary>
        /// <param name="val">The <see cref="Realms.RealmValue"/> to convert.</param>
        /// <returns>The <see cref="ObjectWithEmbeddedProperties"/> stored in the <see cref="Realms.RealmValue"/>.</returns>
        public static explicit operator ObjectWithEmbeddedProperties?(Realms.RealmValue val) => val.Type == Realms.RealmValueType.Null ? null : val.AsRealmObject<ObjectWithEmbeddedProperties>();

        /// <summary>
        /// Implicitly constructs a <see cref="Realms.RealmValue"/> from <see cref="ObjectWithEmbeddedProperties"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="Realms.RealmValue"/>.</param>
        /// <returns>A <see cref="Realms.RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator Realms.RealmValue(ObjectWithEmbeddedProperties? val) => val == null ? Realms.RealmValue.Null : Realms.RealmValue.Object(val);

        /// <summary>
        /// Implicitly constructs a <see cref="Realms.QueryArgument"/> from <see cref="ObjectWithEmbeddedProperties"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="Realms.QueryArgument"/>.</param>
        /// <returns>A <see cref="Realms.QueryArgument"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator Realms.QueryArgument(ObjectWithEmbeddedProperties? val) => (Realms.RealmValue)val;

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
        private class ObjectWithEmbeddedPropertiesObjectHelper : Realms.Weaving.IRealmObjectHelper
        {
            public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public Realms.ManagedAccessor CreateAccessor() => new ObjectWithEmbeddedPropertiesManagedAccessor();

            public Realms.IRealmObjectBase CreateInstance() => new ObjectWithEmbeddedProperties();

            public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out RealmValue value)
            {
                value = ((IObjectWithEmbeddedPropertiesAccessor)instance.Accessor).PrimaryKey;
                return true;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        internal interface IObjectWithEmbeddedPropertiesAccessor : Realms.IRealmAccessor
        {
            int PrimaryKey { get; set; }

            Realms.Tests.EmbeddedAllTypesObject? AllTypesObject { get; set; }

            Realms.Tests.EmbeddedLevel1? RecursiveObject { get; set; }

            System.Collections.Generic.IList<Realms.Tests.EmbeddedAllTypesObject> ListOfAllTypesObjects { get; }

            System.Collections.Generic.IDictionary<string, Realms.Tests.EmbeddedAllTypesObject?> DictionaryOfAllTypesObjects { get; }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class ObjectWithEmbeddedPropertiesManagedAccessor : Realms.ManagedAccessor, IObjectWithEmbeddedPropertiesAccessor
        {
            public int PrimaryKey
            {
                get => (int)GetValue("PrimaryKey");
                set => SetValueUnique("PrimaryKey", value);
            }

            public Realms.Tests.EmbeddedAllTypesObject? AllTypesObject
            {
                get => (Realms.Tests.EmbeddedAllTypesObject?)GetValue("AllTypesObject");
                set => SetValue("AllTypesObject", value);
            }

            public Realms.Tests.EmbeddedLevel1? RecursiveObject
            {
                get => (Realms.Tests.EmbeddedLevel1?)GetValue("RecursiveObject");
                set => SetValue("RecursiveObject", value);
            }

            private System.Collections.Generic.IList<Realms.Tests.EmbeddedAllTypesObject> _listOfAllTypesObjects = null!;
            public System.Collections.Generic.IList<Realms.Tests.EmbeddedAllTypesObject> ListOfAllTypesObjects
            {
                get
                {
                    if (_listOfAllTypesObjects == null)
                    {
                        _listOfAllTypesObjects = GetListValue<Realms.Tests.EmbeddedAllTypesObject>("ListOfAllTypesObjects");
                    }

                    return _listOfAllTypesObjects;
                }
            }

            private System.Collections.Generic.IDictionary<string, Realms.Tests.EmbeddedAllTypesObject?> _dictionaryOfAllTypesObjects = null!;
            public System.Collections.Generic.IDictionary<string, Realms.Tests.EmbeddedAllTypesObject?> DictionaryOfAllTypesObjects
            {
                get
                {
                    if (_dictionaryOfAllTypesObjects == null)
                    {
                        _dictionaryOfAllTypesObjects = GetDictionaryValue<Realms.Tests.EmbeddedAllTypesObject?>("DictionaryOfAllTypesObjects");
                    }

                    return _dictionaryOfAllTypesObjects;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class ObjectWithEmbeddedPropertiesUnmanagedAccessor : Realms.UnmanagedAccessor, IObjectWithEmbeddedPropertiesAccessor
        {
            public override ObjectSchema ObjectSchema => ObjectWithEmbeddedProperties.RealmSchema;

            private int _primaryKey;
            public int PrimaryKey
            {
                get => _primaryKey;
                set
                {
                    _primaryKey = value;
                    RaisePropertyChanged("PrimaryKey");
                }
            }

            private Realms.Tests.EmbeddedAllTypesObject? _allTypesObject;
            public Realms.Tests.EmbeddedAllTypesObject? AllTypesObject
            {
                get => _allTypesObject;
                set
                {
                    _allTypesObject = value;
                    RaisePropertyChanged("AllTypesObject");
                }
            }

            private Realms.Tests.EmbeddedLevel1? _recursiveObject;
            public Realms.Tests.EmbeddedLevel1? RecursiveObject
            {
                get => _recursiveObject;
                set
                {
                    _recursiveObject = value;
                    RaisePropertyChanged("RecursiveObject");
                }
            }

            public System.Collections.Generic.IList<Realms.Tests.EmbeddedAllTypesObject> ListOfAllTypesObjects { get; } = new List<Realms.Tests.EmbeddedAllTypesObject>();

            public System.Collections.Generic.IDictionary<string, Realms.Tests.EmbeddedAllTypesObject?> DictionaryOfAllTypesObjects { get; } = new Dictionary<string, Realms.Tests.EmbeddedAllTypesObject?>();

            public ObjectWithEmbeddedPropertiesUnmanagedAccessor(Type objectType) : base(objectType)
            {
            }

            public override Realms.RealmValue GetValue(string propertyName)
            {
                return propertyName switch
                {
                    "PrimaryKey" => _primaryKey,
                    "AllTypesObject" => _allTypesObject,
                    "RecursiveObject" => _recursiveObject,
                    _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
                };
            }

            public override void SetValue(string propertyName, Realms.RealmValue val)
            {
                switch (propertyName)
                {
                    case "PrimaryKey":
                        throw new InvalidOperationException("Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique");
                    case "AllTypesObject":
                        AllTypesObject = (Realms.Tests.EmbeddedAllTypesObject?)val;
                        return;
                    case "RecursiveObject":
                        RecursiveObject = (Realms.Tests.EmbeddedLevel1?)val;
                        return;
                    default:
                        throw new MissingMemberException($"The object does not have a settable Realm property with name {propertyName}");
                }
            }

            public override void SetValueUnique(string propertyName, Realms.RealmValue val)
            {
                if (propertyName != "PrimaryKey")
                {
                    throw new InvalidOperationException($"Cannot set the value of non primary key property ({propertyName}) with SetValueUnique");
                }

                PrimaryKey = (int)val;
            }

            public override IList<T> GetListValue<T>(string propertyName)
            {
                return propertyName switch
                {
                    "ListOfAllTypesObjects" => (IList<T>)ListOfAllTypesObjects,
                    _ => throw new MissingMemberException($"The object does not have a Realm list property with name {propertyName}"),
                };
            }

            public override ISet<T> GetSetValue<T>(string propertyName)
            {
                throw new MissingMemberException($"The object does not have a Realm set property with name {propertyName}");
            }

            public override IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
            {
                return propertyName switch
                {
                    "DictionaryOfAllTypesObjects" => (IDictionary<string, TValue>)DictionaryOfAllTypesObjects,
                    _ => throw new MissingMemberException($"The object does not have a Realm dictionary property with name {propertyName}"),
                };
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class ObjectWithEmbeddedPropertiesSerializer : Realms.Serialization.RealmObjectSerializer<ObjectWithEmbeddedProperties>
        {
            public override string SchemaName => "ObjectWithEmbeddedProperties";

            protected override void SerializeValue(MongoDB.Bson.Serialization.BsonSerializationContext context, BsonSerializationArgs args, ObjectWithEmbeddedProperties value)
            {
                context.Writer.WriteStartDocument();

                WriteValue(context, args, "PrimaryKey", value.PrimaryKey);
                WriteValue(context, args, "AllTypesObject", value.AllTypesObject);
                WriteValue(context, args, "RecursiveObject", value.RecursiveObject);
                WriteList(context, args, "ListOfAllTypesObjects", value.ListOfAllTypesObjects);
                WriteDictionary(context, args, "DictionaryOfAllTypesObjects", value.DictionaryOfAllTypesObjects);

                context.Writer.WriteEndDocument();
            }

            protected override ObjectWithEmbeddedProperties CreateInstance() => new ObjectWithEmbeddedProperties();

            protected override void ReadValue(ObjectWithEmbeddedProperties instance, string name, BsonDeserializationContext context)
            {
                switch (name)
                {
                    case "PrimaryKey":
                        instance.PrimaryKey = BsonSerializer.LookupSerializer<int>().Deserialize(context);
                        break;
                    case "AllTypesObject":
                        instance.AllTypesObject = BsonSerializer.LookupSerializer<Realms.Tests.EmbeddedAllTypesObject?>().Deserialize(context);
                        break;
                    case "RecursiveObject":
                        instance.RecursiveObject = BsonSerializer.LookupSerializer<Realms.Tests.EmbeddedLevel1?>().Deserialize(context);
                        break;
                }
            }

            protected override void ReadArrayElement(ObjectWithEmbeddedProperties instance, string name, BsonDeserializationContext context)
            {
                switch (name)
                {
                    case "ListOfAllTypesObjects":
                        instance.ListOfAllTypesObjects.Add(BsonSerializer.LookupSerializer<Realms.Tests.EmbeddedAllTypesObject>().Deserialize(context));
                        break;
                }
            }

            protected override void ReadDocumentField(ObjectWithEmbeddedProperties instance, string name, string fieldName, BsonDeserializationContext context)
            {
                switch (name)
                {
                    case "DictionaryOfAllTypesObjects":
                        instance.DictionaryOfAllTypesObjects[fieldName] = BsonSerializer.LookupSerializer<Realms.Tests.EmbeddedAllTypesObject?>().Deserialize(context);
                        break;
                }
            }
        }
    }
}
