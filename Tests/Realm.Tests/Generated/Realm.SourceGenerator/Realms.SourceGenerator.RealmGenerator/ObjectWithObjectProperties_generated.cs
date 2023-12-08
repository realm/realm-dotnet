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
    [Woven(typeof(ObjectWithObjectPropertiesObjectHelper)), Realms.Preserve(AllMembers = true)]
    public partial class ObjectWithObjectProperties : IRealmObject, INotifyPropertyChanged, IReflectableType
    {

        [Realms.Preserve]
        static ObjectWithObjectProperties()
        {
            Realms.Serialization.RealmObjectSerializer.Register(new ObjectWithObjectPropertiesSerializer());
        }

        /// <summary>
        /// Defines the schema for the <see cref="ObjectWithObjectProperties"/> class.
        /// </summary>
        public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("ObjectWithObjectProperties", ObjectSchema.ObjectType.RealmObject)
        {
            Realms.Schema.Property.Object("StandaloneObject", "IntPropertyObject", managedName: "StandaloneObject"),
            Realms.Schema.Property.Object("EmbeddedObject", "EmbeddedIntPropertyObject", managedName: "EmbeddedObject"),
        }.Build();

        #region IRealmObject implementation

        private IObjectWithObjectPropertiesAccessor? _accessor;

        Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

        private IObjectWithObjectPropertiesAccessor Accessor => _accessor ??= new ObjectWithObjectPropertiesUnmanagedAccessor(typeof(ObjectWithObjectProperties));

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
            var newAccessor = (IObjectWithObjectPropertiesAccessor)managedAccessor;
            var oldAccessor = _accessor;
            _accessor = newAccessor;

            if (helper != null && oldAccessor != null)
            {
                if (oldAccessor.StandaloneObject != null && newAccessor.Realm != null)
                {
                    newAccessor.Realm.Add(oldAccessor.StandaloneObject, update);
                }
                newAccessor.StandaloneObject = oldAccessor.StandaloneObject;
                newAccessor.EmbeddedObject = oldAccessor.EmbeddedObject;
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
        /// Converts a <see cref="Realms.RealmValue"/> to <see cref="ObjectWithObjectProperties"/>. Equivalent to <see cref="Realms.RealmValue.AsNullableRealmObject{T}"/>.
        /// </summary>
        /// <param name="val">The <see cref="Realms.RealmValue"/> to convert.</param>
        /// <returns>The <see cref="ObjectWithObjectProperties"/> stored in the <see cref="Realms.RealmValue"/>.</returns>
        public static explicit operator ObjectWithObjectProperties?(Realms.RealmValue val) => val.Type == Realms.RealmValueType.Null ? null : val.AsRealmObject<ObjectWithObjectProperties>();

        /// <summary>
        /// Implicitly constructs a <see cref="Realms.RealmValue"/> from <see cref="ObjectWithObjectProperties"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="Realms.RealmValue"/>.</param>
        /// <returns>A <see cref="Realms.RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator Realms.RealmValue(ObjectWithObjectProperties? val) => val == null ? Realms.RealmValue.Null : Realms.RealmValue.Object(val);

        /// <summary>
        /// Implicitly constructs a <see cref="Realms.QueryArgument"/> from <see cref="ObjectWithObjectProperties"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="Realms.QueryArgument"/>.</param>
        /// <returns>A <see cref="Realms.QueryArgument"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator Realms.QueryArgument(ObjectWithObjectProperties? val) => (Realms.RealmValue)val;

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
        private class ObjectWithObjectPropertiesObjectHelper : Realms.Weaving.IRealmObjectHelper
        {
            public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public Realms.ManagedAccessor CreateAccessor() => new ObjectWithObjectPropertiesManagedAccessor();

            public Realms.IRealmObjectBase CreateInstance() => new ObjectWithObjectProperties();

            public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out RealmValue value)
            {
                value = RealmValue.Null;
                return false;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        internal interface IObjectWithObjectPropertiesAccessor : Realms.IRealmAccessor
        {
            Realms.Tests.IntPropertyObject? StandaloneObject { get; set; }

            Realms.Tests.EmbeddedIntPropertyObject? EmbeddedObject { get; set; }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class ObjectWithObjectPropertiesManagedAccessor : Realms.ManagedAccessor, IObjectWithObjectPropertiesAccessor
        {
            public Realms.Tests.IntPropertyObject? StandaloneObject
            {
                get => (Realms.Tests.IntPropertyObject?)GetValue("StandaloneObject");
                set => SetValue("StandaloneObject", value);
            }

            public Realms.Tests.EmbeddedIntPropertyObject? EmbeddedObject
            {
                get => (Realms.Tests.EmbeddedIntPropertyObject?)GetValue("EmbeddedObject");
                set => SetValue("EmbeddedObject", value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class ObjectWithObjectPropertiesUnmanagedAccessor : Realms.UnmanagedAccessor, IObjectWithObjectPropertiesAccessor
        {
            public override ObjectSchema ObjectSchema => ObjectWithObjectProperties.RealmSchema;

            private Realms.Tests.IntPropertyObject? _standaloneObject;
            public Realms.Tests.IntPropertyObject? StandaloneObject
            {
                get => _standaloneObject;
                set
                {
                    _standaloneObject = value;
                    RaisePropertyChanged("StandaloneObject");
                }
            }

            private Realms.Tests.EmbeddedIntPropertyObject? _embeddedObject;
            public Realms.Tests.EmbeddedIntPropertyObject? EmbeddedObject
            {
                get => _embeddedObject;
                set
                {
                    _embeddedObject = value;
                    RaisePropertyChanged("EmbeddedObject");
                }
            }

            public ObjectWithObjectPropertiesUnmanagedAccessor(Type objectType) : base(objectType)
            {
            }

            public override Realms.RealmValue GetValue(string propertyName)
            {
                return propertyName switch
                {
                    "StandaloneObject" => _standaloneObject,
                    "EmbeddedObject" => _embeddedObject,
                    _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
                };
            }

            public override void SetValue(string propertyName, Realms.RealmValue val)
            {
                switch (propertyName)
                {
                    case "StandaloneObject":
                        StandaloneObject = (Realms.Tests.IntPropertyObject?)val;
                        return;
                    case "EmbeddedObject":
                        EmbeddedObject = (Realms.Tests.EmbeddedIntPropertyObject?)val;
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
                throw new MissingMemberException($"The object does not have a Realm list property with name {propertyName}");
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
        private class ObjectWithObjectPropertiesSerializer : Realms.Serialization.RealmObjectSerializerBase<ObjectWithObjectProperties>
        {
            public override string SchemaName => "ObjectWithObjectProperties";

            protected override void SerializeValue(MongoDB.Bson.Serialization.BsonSerializationContext context, BsonSerializationArgs args, ObjectWithObjectProperties value)
            {
                context.Writer.WriteStartDocument();

                WriteValue(context, args, "StandaloneObject", value.StandaloneObject);
                WriteValue(context, args, "EmbeddedObject", value.EmbeddedObject);

                context.Writer.WriteEndDocument();
            }

            protected override ObjectWithObjectProperties CreateInstance() => new ObjectWithObjectProperties();

            protected override void ReadValue(ObjectWithObjectProperties instance, string name, BsonDeserializationContext context)
            {
                switch (name)
                {
                    case "StandaloneObject":
                        instance.StandaloneObject = Realms.Serialization.RealmObjectSerializer.LookupSerializer<Realms.Tests.IntPropertyObject?>()!.DeserializeById(context);
                        break;
                    case "EmbeddedObject":
                        instance.EmbeddedObject = BsonSerializer.LookupSerializer<Realms.Tests.EmbeddedIntPropertyObject?>().Deserialize(context);
                        break;
                }
            }

            protected override void ReadArrayElement(ObjectWithObjectProperties instance, string name, BsonDeserializationContext context)
            {
                // No persisted list/set properties to deserialize
            }

            protected override void ReadDocumentField(ObjectWithObjectProperties instance, string name, string fieldName, BsonDeserializationContext context)
            {
                // No persisted dictionary properties to deserialize
            }
        }
    }
}
