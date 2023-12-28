﻿// <auto-generated />
#nullable enable

using MongoDB.Bson.Serialization;
using NUnit.Framework;
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
    [Woven(typeof(OnlyListPropertiesObjectHelper)), Realms.Preserve(AllMembers = true)]
    public partial class OnlyListProperties : IRealmObject, INotifyPropertyChanged, IReflectableType
    {

        [Realms.Preserve]
        static OnlyListProperties()
        {
            Realms.Serialization.RealmObjectSerializer.Register(new OnlyListPropertiesSerializer());
            Realms.Sync.MongoClient.RegisterSchema(typeof(OnlyListProperties), RealmSchema);
        }

        /// <summary>
        /// Defines the schema for the <see cref="OnlyListProperties"/> class.
        /// </summary>
        public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("OnlyListProperties", ObjectSchema.ObjectType.RealmObject)
        {
            Realms.Schema.Property.ObjectList("Friends", "Person", managedName: "Friends"),
            Realms.Schema.Property.ObjectList("Enemies", "Person", managedName: "Enemies"),
        }.Build();

        #region IRealmObject implementation

        private IOnlyListPropertiesAccessor? _accessor;

        Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

        private IOnlyListPropertiesAccessor Accessor => _accessor ??= new OnlyListPropertiesUnmanagedAccessor(typeof(OnlyListProperties));

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
            var newAccessor = (IOnlyListPropertiesAccessor)managedAccessor;
            var oldAccessor = _accessor;
            _accessor = newAccessor;

            if (helper != null && oldAccessor != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.Friends.Clear();
                    newAccessor.Enemies.Clear();
                }

                Realms.CollectionExtensions.PopulateCollection(oldAccessor.Friends, newAccessor.Friends, update, skipDefaults);
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
        /// Converts a <see cref="Realms.RealmValue"/> to <see cref="OnlyListProperties"/>. Equivalent to <see cref="Realms.RealmValue.AsNullableRealmObject{T}"/>.
        /// </summary>
        /// <param name="val">The <see cref="Realms.RealmValue"/> to convert.</param>
        /// <returns>The <see cref="OnlyListProperties"/> stored in the <see cref="Realms.RealmValue"/>.</returns>
        public static explicit operator OnlyListProperties?(Realms.RealmValue val) => val.Type == Realms.RealmValueType.Null ? null : val.AsRealmObject<OnlyListProperties>();

        /// <summary>
        /// Implicitly constructs a <see cref="Realms.RealmValue"/> from <see cref="OnlyListProperties"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="Realms.RealmValue"/>.</param>
        /// <returns>A <see cref="Realms.RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator Realms.RealmValue(OnlyListProperties? val) => val == null ? Realms.RealmValue.Null : Realms.RealmValue.Object(val);

        /// <summary>
        /// Implicitly constructs a <see cref="Realms.QueryArgument"/> from <see cref="OnlyListProperties"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="Realms.QueryArgument"/>.</param>
        /// <returns>A <see cref="Realms.QueryArgument"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator Realms.QueryArgument(OnlyListProperties? val) => (Realms.RealmValue)val;

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
        private class OnlyListPropertiesObjectHelper : Realms.Weaving.IRealmObjectHelper
        {
            public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public Realms.ManagedAccessor CreateAccessor() => new OnlyListPropertiesManagedAccessor();

            public Realms.IRealmObjectBase CreateInstance() => new OnlyListProperties();

            public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out RealmValue value)
            {
                value = RealmValue.Null;
                return false;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        internal interface IOnlyListPropertiesAccessor : Realms.IRealmAccessor
        {
            System.Collections.Generic.IList<Realms.Tests.Database.Person> Friends { get; }

            System.Collections.Generic.IList<Realms.Tests.Database.Person> Enemies { get; }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class OnlyListPropertiesManagedAccessor : Realms.ManagedAccessor, IOnlyListPropertiesAccessor
        {
            private System.Collections.Generic.IList<Realms.Tests.Database.Person> _friends = null!;
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

            private System.Collections.Generic.IList<Realms.Tests.Database.Person> _enemies = null!;
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

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class OnlyListPropertiesUnmanagedAccessor : Realms.UnmanagedAccessor, IOnlyListPropertiesAccessor
        {
            public override ObjectSchema ObjectSchema => OnlyListProperties.RealmSchema;

            public System.Collections.Generic.IList<Realms.Tests.Database.Person> Friends { get; } = new List<Realms.Tests.Database.Person>();

            public System.Collections.Generic.IList<Realms.Tests.Database.Person> Enemies { get; } = new List<Realms.Tests.Database.Person>();

            public OnlyListPropertiesUnmanagedAccessor(Type objectType) : base(objectType)
            {
            }

            public override Realms.RealmValue GetValue(string propertyName)
            {
                throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}");
            }

            public override void SetValue(string propertyName, Realms.RealmValue val)
            {
                throw new MissingMemberException($"The object does not have a settable Realm property with name {propertyName}");
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

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class OnlyListPropertiesSerializer : Realms.Serialization.RealmObjectSerializerBase<OnlyListProperties>
        {
            public override string SchemaName => "OnlyListProperties";

            protected override void SerializeValue(MongoDB.Bson.Serialization.BsonSerializationContext context, BsonSerializationArgs args, OnlyListProperties value)
            {
                context.Writer.WriteStartDocument();

                WriteList(context, args, "Friends", value.Friends);
                WriteList(context, args, "Enemies", value.Enemies);

                context.Writer.WriteEndDocument();
            }

            protected override OnlyListProperties CreateInstance() => new OnlyListProperties();

            protected override void ReadValue(OnlyListProperties instance, string name, BsonDeserializationContext context)
            {
                switch (name)
                {
                    case "Friends":
                    case "Enemies":
                        ReadArray(instance, name, context);
                        break;
                    default:
                        context.Reader.SkipValue();
                        break;
                }
            }

            protected override void ReadArrayElement(OnlyListProperties instance, string name, BsonDeserializationContext context)
            {
                switch (name)
                {
                    case "Friends":
                        instance.Friends.Add(Realms.Serialization.RealmObjectSerializer.LookupSerializer<Realms.Tests.Database.Person>()!.DeserializeById(context)!);
                        break;
                    case "Enemies":
                        instance.Enemies.Add(Realms.Serialization.RealmObjectSerializer.LookupSerializer<Realms.Tests.Database.Person>()!.DeserializeById(context)!);
                        break;
                }
            }

            protected override void ReadDocumentField(OnlyListProperties instance, string name, string fieldName, BsonDeserializationContext context)
            {
                // No persisted dictionary properties to deserialize
            }
        }
    }
}
