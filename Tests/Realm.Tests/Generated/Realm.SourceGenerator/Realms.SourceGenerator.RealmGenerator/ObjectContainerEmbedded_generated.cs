﻿// <auto-generated />
#nullable enable

using MongoDB.Bson.Serialization;
using NUnit.Framework;
using Realms;
using Realms.Exceptions;
using Realms.Extensions;
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
using TestEmbeddedObject = Realms.IEmbeddedObject;
using TestRealmObject = Realms.IRealmObject;

namespace Realms.Tests.Database
{
    [Generated]
    [Woven(typeof(ObjectContainerEmbeddedObjectHelper)), Realms.Preserve(AllMembers = true)]
    public partial class ObjectContainerEmbedded : IRealmObject, INotifyPropertyChanged, IReflectableType
    {

        [Realms.Preserve]
        static ObjectContainerEmbedded()
        {
            Realms.Serialization.RealmObjectSerializer.Register(new ObjectContainerEmbeddedSerializer());
        }

        /// <summary>
        /// Defines the schema for the <see cref="ObjectContainerEmbedded"/> class.
        /// </summary>
        [System.Reflection.Obfuscation]
        public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("ObjectContainer", ObjectSchema.ObjectType.RealmObject)
        {
            Realms.Schema.Property.Primitive("Value", Realms.RealmValueType.String, isPrimaryKey: false, indexType: IndexType.None, isNullable: true, managedName: "Value"),
            Realms.Schema.Property.Object("Link", "Object", managedName: "Link"),
            Realms.Schema.Property.ObjectList("List", "Object", managedName: "List"),
        }.Build();

        #region IRealmObject implementation

        private IObjectContainerEmbeddedAccessor? _accessor;

        Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

        private IObjectContainerEmbeddedAccessor Accessor => _accessor ??= new ObjectContainerEmbeddedUnmanagedAccessor(typeof(ObjectContainerEmbedded));

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
        public Realms.Schema.ExtendedObjectSchema ExtendedObjectSchema => Accessor.ExtendedObjectSchema!;

        /// <inheritdoc />
        [IgnoreDataMember, XmlIgnore]
        public Realms.DynamicObjectApi DynamicApi => Accessor.DynamicApi;

        /// <inheritdoc />
        [IgnoreDataMember, XmlIgnore]
        public int BacklinksCount => Accessor.BacklinksCount;

        void ISettableManagedAccessor.SetManagedAccessor(Realms.IRealmAccessor managedAccessor, Realms.Weaving.IRealmObjectHelper? helper, bool update, bool skipDefaults)
        {
            var newAccessor = (IObjectContainerEmbeddedAccessor)managedAccessor;
            var oldAccessor = _accessor;
            _accessor = newAccessor;

            if (helper != null && oldAccessor != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.List.Clear();
                }

                if (!skipDefaults || oldAccessor.Value != default(string?))
                {
                    newAccessor.Value = oldAccessor.Value;
                }
                newAccessor.Link = oldAccessor.Link;
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.List, newAccessor.List, update, skipDefaults);
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
        /// Converts a <see cref="Realms.RealmValue"/> to <see cref="ObjectContainerEmbedded"/>. Equivalent to <see cref="Realms.RealmValue.AsNullableRealmObject{T}"/>.
        /// </summary>
        /// <param name="val">The <see cref="Realms.RealmValue"/> to convert.</param>
        /// <returns>The <see cref="ObjectContainerEmbedded"/> stored in the <see cref="Realms.RealmValue"/>.</returns>
        public static explicit operator ObjectContainerEmbedded?(Realms.RealmValue val) => val.Type == Realms.RealmValueType.Null ? null : val.AsRealmObject<ObjectContainerEmbedded>();

        /// <summary>
        /// Implicitly constructs a <see cref="Realms.RealmValue"/> from <see cref="ObjectContainerEmbedded"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="Realms.RealmValue"/>.</param>
        /// <returns>A <see cref="Realms.RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator Realms.RealmValue(ObjectContainerEmbedded? val) => val == null ? Realms.RealmValue.Null : Realms.RealmValue.Object(val);

        /// <summary>
        /// Implicitly constructs a <see cref="Realms.QueryArgument"/> from <see cref="ObjectContainerEmbedded"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="Realms.QueryArgument"/>.</param>
        /// <returns>A <see cref="Realms.QueryArgument"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator Realms.QueryArgument(ObjectContainerEmbedded? val) => (Realms.RealmValue)val;

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
        private class ObjectContainerEmbeddedObjectHelper : Realms.Weaving.IRealmObjectHelper
        {
            public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public Realms.ManagedAccessor CreateAccessor() => new ObjectContainerEmbeddedManagedAccessor();

            public Realms.IRealmObjectBase CreateInstance() => new ObjectContainerEmbedded();

            public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out RealmValue value)
            {
                value = RealmValue.Null;
                return false;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        internal interface IObjectContainerEmbeddedAccessor : Realms.IRealmAccessor
        {
            string? Value { get; set; }

            Realms.Tests.Database.ObjectEmbedded? Link { get; set; }

            System.Collections.Generic.IList<Realms.Tests.Database.ObjectEmbedded> List { get; }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class ObjectContainerEmbeddedManagedAccessor : Realms.ManagedAccessor, IObjectContainerEmbeddedAccessor
        {
            public string? Value
            {
                get => (string?)GetValue("Value");
                set => SetValue("Value", value);
            }

            public Realms.Tests.Database.ObjectEmbedded? Link
            {
                get => (Realms.Tests.Database.ObjectEmbedded?)GetValue("Link");
                set => SetValue("Link", value);
            }

            private System.Collections.Generic.IList<Realms.Tests.Database.ObjectEmbedded> _list = null!;
            public System.Collections.Generic.IList<Realms.Tests.Database.ObjectEmbedded> List
            {
                get
                {
                    if (_list == null)
                    {
                        _list = GetListValue<Realms.Tests.Database.ObjectEmbedded>("List");
                    }

                    return _list;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class ObjectContainerEmbeddedUnmanagedAccessor : Realms.UnmanagedAccessor, IObjectContainerEmbeddedAccessor
        {
            public override ObjectSchema ObjectSchema => ObjectContainerEmbedded.RealmSchema;

            private string? _value;
            public string? Value
            {
                get => _value;
                set
                {
                    _value = value;
                    RaisePropertyChanged("Value");
                }
            }

            private Realms.Tests.Database.ObjectEmbedded? _link;
            public Realms.Tests.Database.ObjectEmbedded? Link
            {
                get => _link;
                set
                {
                    _link = value;
                    RaisePropertyChanged("Link");
                }
            }

            public System.Collections.Generic.IList<Realms.Tests.Database.ObjectEmbedded> List { get; } = new List<Realms.Tests.Database.ObjectEmbedded>();

            public ObjectContainerEmbeddedUnmanagedAccessor(Type objectType) : base(objectType)
            {
            }

            public override Realms.RealmValue GetValue(string propertyName)
            {
                return propertyName switch
                {
                    "Value" => _value,
                    "Link" => _link,
                    _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
                };
            }

            public override void SetValue(string propertyName, Realms.RealmValue val)
            {
                switch (propertyName)
                {
                    case "Value":
                        Value = (string?)val;
                        return;
                    case "Link":
                        Link = (Realms.Tests.Database.ObjectEmbedded?)val;
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
                    "List" => (IList<T>)List,
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
        private class ObjectContainerEmbeddedSerializer : Realms.Serialization.RealmObjectSerializerBase<ObjectContainerEmbedded>
        {
            public override string SchemaName => "ObjectContainer";

            protected override void SerializeValue(MongoDB.Bson.Serialization.BsonSerializationContext context, BsonSerializationArgs args, ObjectContainerEmbedded value)
            {
                context.Writer.WriteStartDocument();

                WriteValue(context, args, "Value", value.Value);
                WriteValue(context, args, "Link", value.Link);
                WriteList(context, args, "List", value.List);

                context.Writer.WriteEndDocument();
            }

            protected override ObjectContainerEmbedded CreateInstance() => new ObjectContainerEmbedded();

            protected override void ReadValue(ObjectContainerEmbedded instance, string name, BsonDeserializationContext context)
            {
                switch (name)
                {
                    case "Value":
                        instance.Value = BsonSerializer.LookupSerializer<string?>().Deserialize(context);
                        break;
                    case "Link":
                        instance.Link = BsonSerializer.LookupSerializer<Realms.Tests.Database.ObjectEmbedded?>().Deserialize(context);
                        break;
                    case "List":
                        ReadArray(instance, name, context);
                        break;
                    default:
                        context.Reader.SkipValue();
                        break;
                }
            }

            protected override void ReadArrayElement(ObjectContainerEmbedded instance, string name, BsonDeserializationContext context)
            {
                switch (name)
                {
                    case "List":
                        instance.List.Add(BsonSerializer.LookupSerializer<Realms.Tests.Database.ObjectEmbedded>().Deserialize(context));
                        break;
                }
            }

            protected override void ReadDocumentField(ObjectContainerEmbedded instance, string name, string fieldName, BsonDeserializationContext context)
            {
                // No persisted dictionary properties to deserialize
            }
        }
    }
}
