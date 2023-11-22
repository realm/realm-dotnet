﻿// <auto-generated />
#nullable enable

using MongoDB.Bson.Serialization;
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
    [Woven(typeof(PersonObjectHelper)), Realms.Preserve(AllMembers = true)]
    public partial class Person : IRealmObject, INotifyPropertyChanged, IReflectableType
    {

        [Realms.Preserve]
        static Person()
        {
            Realms.Serialization.RealmObjectSerializer.Register(new PersonSerializer());
        }

        /// <summary>
        /// Defines the schema for the <see cref="Person"/> class.
        /// </summary>
        public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("Person", ObjectSchema.ObjectType.RealmObject)
        {
            Realms.Schema.Property.Primitive("FirstName", Realms.RealmValueType.String, isPrimaryKey: false, indexType: IndexType.None, isNullable: true, managedName: "FirstName"),
            Realms.Schema.Property.Primitive("LastName", Realms.RealmValueType.String, isPrimaryKey: false, indexType: IndexType.None, isNullable: true, managedName: "LastName"),
            Realms.Schema.Property.Primitive("Score", Realms.RealmValueType.Float, isPrimaryKey: false, indexType: IndexType.None, isNullable: false, managedName: "Score"),
            Realms.Schema.Property.Primitive("Latitude", Realms.RealmValueType.Double, isPrimaryKey: false, indexType: IndexType.None, isNullable: false, managedName: "Latitude"),
            Realms.Schema.Property.Primitive("Longitude", Realms.RealmValueType.Double, isPrimaryKey: false, indexType: IndexType.None, isNullable: false, managedName: "Longitude"),
            Realms.Schema.Property.Primitive("Salary", Realms.RealmValueType.Int, isPrimaryKey: false, indexType: IndexType.None, isNullable: false, managedName: "Salary"),
            Realms.Schema.Property.Primitive("IsAmbivalent", Realms.RealmValueType.Bool, isPrimaryKey: false, indexType: IndexType.None, isNullable: true, managedName: "IsAmbivalent"),
            Realms.Schema.Property.Primitive("Birthday", Realms.RealmValueType.Date, isPrimaryKey: false, indexType: IndexType.None, isNullable: false, managedName: "Birthday"),
            Realms.Schema.Property.Primitive("PublicCertificateBytes", Realms.RealmValueType.Data, isPrimaryKey: false, indexType: IndexType.None, isNullable: true, managedName: "PublicCertificateBytes"),
            Realms.Schema.Property.Primitive("OptionalAddress", Realms.RealmValueType.String, isPrimaryKey: false, indexType: IndexType.None, isNullable: true, managedName: "OptionalAddress"),
            Realms.Schema.Property.Primitive("Email", Realms.RealmValueType.String, isPrimaryKey: false, indexType: IndexType.None, isNullable: true, managedName: "Email_"),
            Realms.Schema.Property.Primitive("IsInteresting", Realms.RealmValueType.Bool, isPrimaryKey: false, indexType: IndexType.None, isNullable: false, managedName: "IsInteresting"),
            Realms.Schema.Property.ObjectList("Friends", "Person", managedName: "Friends"),
        }.Build();

        #region IRealmObject implementation

        private IPersonAccessor? _accessor;

        Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

        private IPersonAccessor Accessor => _accessor ??= new PersonUnmanagedAccessor(typeof(Person));

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
            var newAccessor = (IPersonAccessor)managedAccessor;
            var oldAccessor = _accessor;
            _accessor = newAccessor;

            if (helper != null && oldAccessor != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.Friends.Clear();
                }

                if (!skipDefaults || oldAccessor.FirstName != default(string?))
                {
                    newAccessor.FirstName = oldAccessor.FirstName;
                }
                if (!skipDefaults || oldAccessor.LastName != default(string?))
                {
                    newAccessor.LastName = oldAccessor.LastName;
                }
                if (!skipDefaults || oldAccessor.Score != default(float))
                {
                    newAccessor.Score = oldAccessor.Score;
                }
                if (!skipDefaults || oldAccessor.Latitude != default(double))
                {
                    newAccessor.Latitude = oldAccessor.Latitude;
                }
                if (!skipDefaults || oldAccessor.Longitude != default(double))
                {
                    newAccessor.Longitude = oldAccessor.Longitude;
                }
                if (!skipDefaults || oldAccessor.Salary != default(long))
                {
                    newAccessor.Salary = oldAccessor.Salary;
                }
                if (!skipDefaults || oldAccessor.IsAmbivalent != default(bool?))
                {
                    newAccessor.IsAmbivalent = oldAccessor.IsAmbivalent;
                }
                newAccessor.Birthday = oldAccessor.Birthday;
                if (!skipDefaults || oldAccessor.PublicCertificateBytes != default(byte[]?))
                {
                    newAccessor.PublicCertificateBytes = oldAccessor.PublicCertificateBytes;
                }
                if (!skipDefaults || oldAccessor.OptionalAddress != default(string?))
                {
                    newAccessor.OptionalAddress = oldAccessor.OptionalAddress;
                }
                if (!skipDefaults || oldAccessor.Email_ != default(string?))
                {
                    newAccessor.Email_ = oldAccessor.Email_;
                }
                if (!skipDefaults || oldAccessor.IsInteresting != default(bool))
                {
                    newAccessor.IsInteresting = oldAccessor.IsInteresting;
                }
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.Friends, newAccessor.Friends, update, skipDefaults);
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
        /// Converts a <see cref="Realms.RealmValue"/> to <see cref="Person"/>. Equivalent to <see cref="Realms.RealmValue.AsNullableRealmObject{T}"/>.
        /// </summary>
        /// <param name="val">The <see cref="Realms.RealmValue"/> to convert.</param>
        /// <returns>The <see cref="Person"/> stored in the <see cref="Realms.RealmValue"/>.</returns>
        public static explicit operator Person?(Realms.RealmValue val) => val.Type == Realms.RealmValueType.Null ? null : val.AsRealmObject<Person>();

        /// <summary>
        /// Implicitly constructs a <see cref="Realms.RealmValue"/> from <see cref="Person"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="Realms.RealmValue"/>.</param>
        /// <returns>A <see cref="Realms.RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator Realms.RealmValue(Person? val) => val == null ? Realms.RealmValue.Null : Realms.RealmValue.Object(val);

        /// <summary>
        /// Implicitly constructs a <see cref="Realms.QueryArgument"/> from <see cref="Person"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="Realms.QueryArgument"/>.</param>
        /// <returns>A <see cref="Realms.QueryArgument"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator Realms.QueryArgument(Person? val) => (Realms.RealmValue)val;

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

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class PersonObjectHelper : Realms.Weaving.IRealmObjectHelper
        {
            public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public Realms.ManagedAccessor CreateAccessor() => new PersonManagedAccessor();

            public Realms.IRealmObjectBase CreateInstance() => new Person();

            public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out RealmValue value)
            {
                value = RealmValue.Null;
                return false;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        internal interface IPersonAccessor : Realms.IRealmAccessor
        {
            string? FirstName { get; set; }

            string? LastName { get; set; }

            float Score { get; set; }

            double Latitude { get; set; }

            double Longitude { get; set; }

            long Salary { get; set; }

            bool? IsAmbivalent { get; set; }

            System.DateTimeOffset Birthday { get; set; }

            byte[]? PublicCertificateBytes { get; set; }

            string? OptionalAddress { get; set; }

            string? Email_ { get; set; }

            bool IsInteresting { get; set; }

            System.Collections.Generic.IList<Realms.Tests.Database.Person> Friends { get; }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class PersonManagedAccessor : Realms.ManagedAccessor, IPersonAccessor
        {
            public string? FirstName
            {
                get => (string?)GetValue("FirstName");
                set => SetValue("FirstName", value);
            }

            public string? LastName
            {
                get => (string?)GetValue("LastName");
                set => SetValue("LastName", value);
            }

            public float Score
            {
                get => (float)GetValue("Score");
                set => SetValue("Score", value);
            }

            public double Latitude
            {
                get => (double)GetValue("Latitude");
                set => SetValue("Latitude", value);
            }

            public double Longitude
            {
                get => (double)GetValue("Longitude");
                set => SetValue("Longitude", value);
            }

            public long Salary
            {
                get => (long)GetValue("Salary");
                set => SetValue("Salary", value);
            }

            public bool? IsAmbivalent
            {
                get => (bool?)GetValue("IsAmbivalent");
                set => SetValue("IsAmbivalent", value);
            }

            public System.DateTimeOffset Birthday
            {
                get => (System.DateTimeOffset)GetValue("Birthday");
                set => SetValue("Birthday", value);
            }

            public byte[]? PublicCertificateBytes
            {
                get => (byte[]?)GetValue("PublicCertificateBytes");
                set => SetValue("PublicCertificateBytes", value);
            }

            public string? OptionalAddress
            {
                get => (string?)GetValue("OptionalAddress");
                set => SetValue("OptionalAddress", value);
            }

            public string? Email_
            {
                get => (string?)GetValue("Email");
                set => SetValue("Email", value);
            }

            public bool IsInteresting
            {
                get => (bool)GetValue("IsInteresting");
                set => SetValue("IsInteresting", value);
            }

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
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class PersonUnmanagedAccessor : Realms.UnmanagedAccessor, IPersonAccessor
        {
            public override ObjectSchema ObjectSchema => Person.RealmSchema;

            private string? _firstName;
            public string? FirstName
            {
                get => _firstName;
                set
                {
                    _firstName = value;
                    RaisePropertyChanged("FirstName");
                }
            }

            private string? _lastName;
            public string? LastName
            {
                get => _lastName;
                set
                {
                    _lastName = value;
                    RaisePropertyChanged("LastName");
                }
            }

            private float _score;
            public float Score
            {
                get => _score;
                set
                {
                    _score = value;
                    RaisePropertyChanged("Score");
                }
            }

            private double _latitude;
            public double Latitude
            {
                get => _latitude;
                set
                {
                    _latitude = value;
                    RaisePropertyChanged("Latitude");
                }
            }

            private double _longitude;
            public double Longitude
            {
                get => _longitude;
                set
                {
                    _longitude = value;
                    RaisePropertyChanged("Longitude");
                }
            }

            private long _salary;
            public long Salary
            {
                get => _salary;
                set
                {
                    _salary = value;
                    RaisePropertyChanged("Salary");
                }
            }

            private bool? _isAmbivalent;
            public bool? IsAmbivalent
            {
                get => _isAmbivalent;
                set
                {
                    _isAmbivalent = value;
                    RaisePropertyChanged("IsAmbivalent");
                }
            }

            private System.DateTimeOffset _birthday;
            public System.DateTimeOffset Birthday
            {
                get => _birthday;
                set
                {
                    _birthday = value;
                    RaisePropertyChanged("Birthday");
                }
            }

            private byte[]? _publicCertificateBytes;
            public byte[]? PublicCertificateBytes
            {
                get => _publicCertificateBytes;
                set
                {
                    _publicCertificateBytes = value;
                    RaisePropertyChanged("PublicCertificateBytes");
                }
            }

            private string? _optionalAddress;
            public string? OptionalAddress
            {
                get => _optionalAddress;
                set
                {
                    _optionalAddress = value;
                    RaisePropertyChanged("OptionalAddress");
                }
            }

            private string? _email_;
            public string? Email_
            {
                get => _email_;
                set
                {
                    _email_ = value;
                    RaisePropertyChanged("Email_");
                }
            }

            private bool _isInteresting;
            public bool IsInteresting
            {
                get => _isInteresting;
                set
                {
                    _isInteresting = value;
                    RaisePropertyChanged("IsInteresting");
                }
            }

            public System.Collections.Generic.IList<Realms.Tests.Database.Person> Friends { get; } = new List<Realms.Tests.Database.Person>();

            public PersonUnmanagedAccessor(Type objectType) : base(objectType)
            {
            }

            public override Realms.RealmValue GetValue(string propertyName)
            {
                return propertyName switch
                {
                    "FirstName" => _firstName,
                    "LastName" => _lastName,
                    "Score" => _score,
                    "Latitude" => _latitude,
                    "Longitude" => _longitude,
                    "Salary" => _salary,
                    "IsAmbivalent" => _isAmbivalent,
                    "Birthday" => _birthday,
                    "PublicCertificateBytes" => _publicCertificateBytes,
                    "OptionalAddress" => _optionalAddress,
                    "Email" => _email_,
                    "IsInteresting" => _isInteresting,
                    _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
                };
            }

            public override void SetValue(string propertyName, Realms.RealmValue val)
            {
                switch (propertyName)
                {
                    case "FirstName":
                        FirstName = (string?)val;
                        return;
                    case "LastName":
                        LastName = (string?)val;
                        return;
                    case "Score":
                        Score = (float)val;
                        return;
                    case "Latitude":
                        Latitude = (double)val;
                        return;
                    case "Longitude":
                        Longitude = (double)val;
                        return;
                    case "Salary":
                        Salary = (long)val;
                        return;
                    case "IsAmbivalent":
                        IsAmbivalent = (bool?)val;
                        return;
                    case "Birthday":
                        Birthday = (System.DateTimeOffset)val;
                        return;
                    case "PublicCertificateBytes":
                        PublicCertificateBytes = (byte[]?)val;
                        return;
                    case "OptionalAddress":
                        OptionalAddress = (string?)val;
                        return;
                    case "Email":
                        Email_ = (string?)val;
                        return;
                    case "IsInteresting":
                        IsInteresting = (bool)val;
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
                    "Friends" => (IList<T>)Friends,
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
        private class PersonSerializer : Realms.Serialization.RealmObjectSerializer<Person>
        {
            protected override void SerializeValue(MongoDB.Bson.Serialization.BsonSerializationContext context, BsonSerializationArgs args, Person value)
            {
                context.Writer.WriteStartDocument();

                WriteValue(context, args, "FirstName", value.FirstName);
                WriteValue(context, args, "LastName", value.LastName);
                WriteValue(context, args, "Score", value.Score);
                WriteValue(context, args, "Latitude", value.Latitude);
                WriteValue(context, args, "Longitude", value.Longitude);
                WriteValue(context, args, "Salary", value.Salary);
                WriteValue(context, args, "IsAmbivalent", value.IsAmbivalent);
                WriteValue(context, args, "Birthday", value.Birthday);
                WriteValue(context, args, "PublicCertificateBytes", value.PublicCertificateBytes);
                WriteValue(context, args, "OptionalAddress", value.OptionalAddress);
                WriteValue(context, args, "Email", value.Email_);
                WriteValue(context, args, "IsInteresting", value.IsInteresting);
                WriteList(context, args, "Friends", value.Friends);

                context.Writer.WriteEndDocument();
            }

            protected override Person CreateInstance() => new Person();

            protected override void ReadValue(Person instance, string name, BsonDeserializationContext context)
            {
                switch (name)
                {
                    case "FirstName":
                        instance.FirstName = BsonSerializer.LookupSerializer<string?>().Deserialize(context);
                        break;
                    case "LastName":
                        instance.LastName = BsonSerializer.LookupSerializer<string?>().Deserialize(context);
                        break;
                    case "Score":
                        instance.Score = BsonSerializer.LookupSerializer<float>().Deserialize(context);
                        break;
                    case "Latitude":
                        instance.Latitude = BsonSerializer.LookupSerializer<double>().Deserialize(context);
                        break;
                    case "Longitude":
                        instance.Longitude = BsonSerializer.LookupSerializer<double>().Deserialize(context);
                        break;
                    case "Salary":
                        instance.Salary = BsonSerializer.LookupSerializer<long>().Deserialize(context);
                        break;
                    case "IsAmbivalent":
                        instance.IsAmbivalent = BsonSerializer.LookupSerializer<bool?>().Deserialize(context);
                        break;
                    case "Birthday":
                        instance.Birthday = BsonSerializer.LookupSerializer<System.DateTimeOffset>().Deserialize(context);
                        break;
                    case "PublicCertificateBytes":
                        instance.PublicCertificateBytes = BsonSerializer.LookupSerializer<byte[]?>().Deserialize(context);
                        break;
                    case "OptionalAddress":
                        instance.OptionalAddress = BsonSerializer.LookupSerializer<string?>().Deserialize(context);
                        break;
                    case "Email":
                        instance.Email_ = BsonSerializer.LookupSerializer<string?>().Deserialize(context);
                        break;
                    case "IsInteresting":
                        instance.IsInteresting = BsonSerializer.LookupSerializer<bool>().Deserialize(context);
                        break;
                }
            }

            protected override void ReadArrayElement(Person instance, string name, BsonDeserializationContext context)
            {
                switch (name)
                {
                    case "Friends":
                        instance.Friends.Add(LookupSerializer<Realms.Tests.Database.Person>()!.DeserializeById(context)!);
                        break;
                }
            }

            protected override void ReadDocumentField(Person instance, string name, string fieldName, BsonDeserializationContext context)
            {
                // No persisted dictionary properties to deserialize
            }
        }
    }
}
