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
    [Woven(typeof(DynamicDogObjectHelper)), Realms.Preserve(AllMembers = true)]
    public partial class DynamicDog : IRealmObject, INotifyPropertyChanged, IReflectableType
    {

        [Realms.Preserve]
        static DynamicDog()
        {
            Realms.Serialization.RealmObjectSerializer.Register(new DynamicDogSerializer());
        }

        /// <summary>
        /// Defines the schema for the <see cref="DynamicDog"/> class.
        /// </summary>
        public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("DynamicDog", ObjectSchema.ObjectType.RealmObject)
        {
            Realms.Schema.Property.Primitive("Name", Realms.RealmValueType.String, isPrimaryKey: false, indexType: IndexType.None, isNullable: true, managedName: "Name"),
            Realms.Schema.Property.Primitive("Color", Realms.RealmValueType.String, isPrimaryKey: false, indexType: IndexType.None, isNullable: true, managedName: "Color"),
            Realms.Schema.Property.Primitive("Vaccinated", Realms.RealmValueType.Bool, isPrimaryKey: false, indexType: IndexType.None, isNullable: false, managedName: "Vaccinated"),
            Realms.Schema.Property.Backlinks("Owners", "DynamicOwner", "Dogs", managedName: "Owners"),
        }.Build();

        #region IRealmObject implementation

        private IDynamicDogAccessor? _accessor;

        Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

        private IDynamicDogAccessor Accessor => _accessor ??= new DynamicDogUnmanagedAccessor(typeof(DynamicDog));

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
            var newAccessor = (IDynamicDogAccessor)managedAccessor;
            var oldAccessor = _accessor;
            _accessor = newAccessor;

            if (helper != null && oldAccessor != null)
            {
                if (!skipDefaults || oldAccessor.Name != default(string?))
                {
                    newAccessor.Name = oldAccessor.Name;
                }
                if (!skipDefaults || oldAccessor.Color != default(string?))
                {
                    newAccessor.Color = oldAccessor.Color;
                }
                if (!skipDefaults || oldAccessor.Vaccinated != default(bool))
                {
                    newAccessor.Vaccinated = oldAccessor.Vaccinated;
                }
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
        /// Converts a <see cref="Realms.RealmValue"/> to <see cref="DynamicDog"/>. Equivalent to <see cref="Realms.RealmValue.AsNullableRealmObject{T}"/>.
        /// </summary>
        /// <param name="val">The <see cref="Realms.RealmValue"/> to convert.</param>
        /// <returns>The <see cref="DynamicDog"/> stored in the <see cref="Realms.RealmValue"/>.</returns>
        public static explicit operator DynamicDog?(Realms.RealmValue val) => val.Type == Realms.RealmValueType.Null ? null : val.AsRealmObject<DynamicDog>();

        /// <summary>
        /// Implicitly constructs a <see cref="Realms.RealmValue"/> from <see cref="DynamicDog"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="Realms.RealmValue"/>.</param>
        /// <returns>A <see cref="Realms.RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator Realms.RealmValue(DynamicDog? val) => val == null ? Realms.RealmValue.Null : Realms.RealmValue.Object(val);

        /// <summary>
        /// Implicitly constructs a <see cref="Realms.QueryArgument"/> from <see cref="DynamicDog"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="Realms.QueryArgument"/>.</param>
        /// <returns>A <see cref="Realms.QueryArgument"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator Realms.QueryArgument(DynamicDog? val) => (Realms.RealmValue)val;

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
        private class DynamicDogObjectHelper : Realms.Weaving.IRealmObjectHelper
        {
            public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public Realms.ManagedAccessor CreateAccessor() => new DynamicDogManagedAccessor();

            public Realms.IRealmObjectBase CreateInstance() => new DynamicDog();

            public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out RealmValue value)
            {
                value = RealmValue.Null;
                return false;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        internal interface IDynamicDogAccessor : Realms.IRealmAccessor
        {
            string? Name { get; set; }

            string? Color { get; set; }

            bool Vaccinated { get; set; }

            System.Linq.IQueryable<Realms.Tests.Database.DynamicOwner> Owners { get; }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class DynamicDogManagedAccessor : Realms.ManagedAccessor, IDynamicDogAccessor
        {
            public string? Name
            {
                get => (string?)GetValue("Name");
                set => SetValue("Name", value);
            }

            public string? Color
            {
                get => (string?)GetValue("Color");
                set => SetValue("Color", value);
            }

            public bool Vaccinated
            {
                get => (bool)GetValue("Vaccinated");
                set => SetValue("Vaccinated", value);
            }

            private System.Linq.IQueryable<Realms.Tests.Database.DynamicOwner> _owners = null!;
            public System.Linq.IQueryable<Realms.Tests.Database.DynamicOwner> Owners
            {
                get
                {
                    if (_owners == null)
                    {
                        _owners = GetBacklinks<Realms.Tests.Database.DynamicOwner>("Owners");
                    }

                    return _owners;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class DynamicDogUnmanagedAccessor : Realms.UnmanagedAccessor, IDynamicDogAccessor
        {
            public override ObjectSchema ObjectSchema => DynamicDog.RealmSchema;

            private string? _name;
            public string? Name
            {
                get => _name;
                set
                {
                    _name = value;
                    RaisePropertyChanged("Name");
                }
            }

            private string? _color;
            public string? Color
            {
                get => _color;
                set
                {
                    _color = value;
                    RaisePropertyChanged("Color");
                }
            }

            private bool _vaccinated;
            public bool Vaccinated
            {
                get => _vaccinated;
                set
                {
                    _vaccinated = value;
                    RaisePropertyChanged("Vaccinated");
                }
            }

            public System.Linq.IQueryable<Realms.Tests.Database.DynamicOwner> Owners => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects.");

            public DynamicDogUnmanagedAccessor(Type objectType) : base(objectType)
            {
            }

            public override Realms.RealmValue GetValue(string propertyName)
            {
                return propertyName switch
                {
                    "Name" => _name,
                    "Color" => _color,
                    "Vaccinated" => _vaccinated,
                    "Owners" => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects."),
                    _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
                };
            }

            public override void SetValue(string propertyName, Realms.RealmValue val)
            {
                switch (propertyName)
                {
                    case "Name":
                        Name = (string?)val;
                        return;
                    case "Color":
                        Color = (string?)val;
                        return;
                    case "Vaccinated":
                        Vaccinated = (bool)val;
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
        private class DynamicDogSerializer : Realms.Serialization.RealmObjectSerializerBase<DynamicDog>
        {
            public override string SchemaName => "DynamicDog";

            protected override void SerializeValue(MongoDB.Bson.Serialization.BsonSerializationContext context, BsonSerializationArgs args, DynamicDog value)
            {
                context.Writer.WriteStartDocument();

                WriteValue(context, args, "Name", value.Name);
                WriteValue(context, args, "Color", value.Color);
                WriteValue(context, args, "Vaccinated", value.Vaccinated);

                context.Writer.WriteEndDocument();
            }

            protected override DynamicDog CreateInstance() => new DynamicDog();

            protected override void ReadValue(DynamicDog instance, string name, BsonDeserializationContext context)
            {
                switch (name)
                {
                    case "Name":
                        instance.Name = BsonSerializer.LookupSerializer<string?>().Deserialize(context);
                        break;
                    case "Color":
                        instance.Color = BsonSerializer.LookupSerializer<string?>().Deserialize(context);
                        break;
                    case "Vaccinated":
                        instance.Vaccinated = BsonSerializer.LookupSerializer<bool>().Deserialize(context);
                        break;
                    default:
                        context.Reader.SkipValue();
                        break;
                }
            }

            protected override void ReadArrayElement(DynamicDog instance, string name, BsonDeserializationContext context)
            {
                // No persisted list/set properties to deserialize
            }

            protected override void ReadDocumentField(DynamicDog instance, string name, string fieldName, BsonDeserializationContext context)
            {
                // No persisted dictionary properties to deserialize
            }
        }
    }
}
