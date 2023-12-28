﻿// <auto-generated />
#nullable enable

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NUnit.Framework;
using Realms;
using Realms.Exceptions;
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
    public partial class GeospatialTests
    {
        [Generated]
        [Woven(typeof(CompanyObjectHelper)), Realms.Preserve(AllMembers = true)]
        public partial class Company : IRealmObject, INotifyPropertyChanged, IReflectableType
        {

            [Realms.Preserve]
            static Company()
            {
                Realms.Serialization.RealmObjectSerializer.Register(new CompanySerializer());
                Realms.Sync.MongoClient.RegisterSchema(typeof(Company), RealmSchema);
            }

            /// <summary>
            /// Defines the schema for the <see cref="Company"/> class.
            /// </summary>
            public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("Company", ObjectSchema.ObjectType.RealmObject)
            {
                Realms.Schema.Property.Primitive("_id", Realms.RealmValueType.ObjectId, isPrimaryKey: false, indexType: IndexType.None, isNullable: false, managedName: "Id"),
                Realms.Schema.Property.Primitive("Name", Realms.RealmValueType.String, isPrimaryKey: false, indexType: IndexType.None, isNullable: false, managedName: "Name"),
                Realms.Schema.Property.Object("Location", "CustomGeoPoint", managedName: "Location"),
                Realms.Schema.Property.ObjectList("Offices", "CustomGeoPoint", managedName: "Offices"),
            }.Build();

            #region IRealmObject implementation

            private ICompanyAccessor? _accessor;

            Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

            private ICompanyAccessor Accessor => _accessor ??= new CompanyUnmanagedAccessor(typeof(Company));

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
                var newAccessor = (ICompanyAccessor)managedAccessor;
                var oldAccessor = _accessor;
                _accessor = newAccessor;

                if (helper != null && oldAccessor != null)
                {
                    if (!skipDefaults)
                    {
                        newAccessor.Offices.Clear();
                    }

                    if (!skipDefaults || oldAccessor.Id != default(MongoDB.Bson.ObjectId))
                    {
                        newAccessor.Id = oldAccessor.Id;
                    }
                    newAccessor.Name = oldAccessor.Name;
                    newAccessor.Location = oldAccessor.Location;
                    Realms.CollectionExtensions.PopulateCollection(oldAccessor.Offices, newAccessor.Offices, update, skipDefaults);
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
            /// Converts a <see cref="Realms.RealmValue"/> to <see cref="Company"/>. Equivalent to <see cref="Realms.RealmValue.AsNullableRealmObject{T}"/>.
            /// </summary>
            /// <param name="val">The <see cref="Realms.RealmValue"/> to convert.</param>
            /// <returns>The <see cref="Company"/> stored in the <see cref="Realms.RealmValue"/>.</returns>
            public static explicit operator Company?(Realms.RealmValue val) => val.Type == Realms.RealmValueType.Null ? null : val.AsRealmObject<Company>();

            /// <summary>
            /// Implicitly constructs a <see cref="Realms.RealmValue"/> from <see cref="Company"/>.
            /// </summary>
            /// <param name="val">The value to store in the <see cref="Realms.RealmValue"/>.</param>
            /// <returns>A <see cref="Realms.RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
            public static implicit operator Realms.RealmValue(Company? val) => val == null ? Realms.RealmValue.Null : Realms.RealmValue.Object(val);

            /// <summary>
            /// Implicitly constructs a <see cref="Realms.QueryArgument"/> from <see cref="Company"/>.
            /// </summary>
            /// <param name="val">The value to store in the <see cref="Realms.QueryArgument"/>.</param>
            /// <returns>A <see cref="Realms.QueryArgument"/> containing the supplied <paramref name="val"/>.</returns>
            public static implicit operator Realms.QueryArgument(Company? val) => (Realms.RealmValue)val;

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
            private class CompanyObjectHelper : Realms.Weaving.IRealmObjectHelper
            {
                public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
                {
                    throw new InvalidOperationException("This method should not be called for source generated classes.");
                }

                public Realms.ManagedAccessor CreateAccessor() => new CompanyManagedAccessor();

                public Realms.IRealmObjectBase CreateInstance() => new Company();

                public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out RealmValue value)
                {
                    value = RealmValue.Null;
                    return false;
                }
            }

            [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
            internal interface ICompanyAccessor : Realms.IRealmAccessor
            {
                MongoDB.Bson.ObjectId Id { get; set; }

                string Name { get; set; }

                Realms.Tests.Database.GeospatialTests.CustomGeoPoint? Location { get; set; }

                System.Collections.Generic.IList<Realms.Tests.Database.GeospatialTests.CustomGeoPoint> Offices { get; }
            }

            [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
            private class CompanyManagedAccessor : Realms.ManagedAccessor, ICompanyAccessor
            {
                public MongoDB.Bson.ObjectId Id
                {
                    get => (MongoDB.Bson.ObjectId)GetValue("_id");
                    set => SetValue("_id", value);
                }

                public string Name
                {
                    get => (string)GetValue("Name")!;
                    set => SetValue("Name", value);
                }

                public Realms.Tests.Database.GeospatialTests.CustomGeoPoint? Location
                {
                    get => (Realms.Tests.Database.GeospatialTests.CustomGeoPoint?)GetValue("Location");
                    set => SetValue("Location", value);
                }

                private System.Collections.Generic.IList<Realms.Tests.Database.GeospatialTests.CustomGeoPoint> _offices = null!;
                public System.Collections.Generic.IList<Realms.Tests.Database.GeospatialTests.CustomGeoPoint> Offices
                {
                    get
                    {
                        if (_offices == null)
                        {
                            _offices = GetListValue<Realms.Tests.Database.GeospatialTests.CustomGeoPoint>("Offices");
                        }

                        return _offices;
                    }
                }
            }

            [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
            private class CompanyUnmanagedAccessor : Realms.UnmanagedAccessor, ICompanyAccessor
            {
                public override ObjectSchema ObjectSchema => Company.RealmSchema;

                private MongoDB.Bson.ObjectId _id = ObjectId.GenerateNewId();
                public MongoDB.Bson.ObjectId Id
                {
                    get => _id;
                    set
                    {
                        _id = value;
                        RaisePropertyChanged("Id");
                    }
                }

                private string _name = null!;
                public string Name
                {
                    get => _name;
                    set
                    {
                        _name = value;
                        RaisePropertyChanged("Name");
                    }
                }

                private Realms.Tests.Database.GeospatialTests.CustomGeoPoint? _location;
                public Realms.Tests.Database.GeospatialTests.CustomGeoPoint? Location
                {
                    get => _location;
                    set
                    {
                        _location = value;
                        RaisePropertyChanged("Location");
                    }
                }

                public System.Collections.Generic.IList<Realms.Tests.Database.GeospatialTests.CustomGeoPoint> Offices { get; } = new List<Realms.Tests.Database.GeospatialTests.CustomGeoPoint>();

                public CompanyUnmanagedAccessor(Type objectType) : base(objectType)
                {
                }

                public override Realms.RealmValue GetValue(string propertyName)
                {
                    return propertyName switch
                    {
                        "_id" => _id,
                        "Name" => _name,
                        "Location" => _location,
                        _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
                    };
                }

                public override void SetValue(string propertyName, Realms.RealmValue val)
                {
                    switch (propertyName)
                    {
                        case "_id":
                            Id = (MongoDB.Bson.ObjectId)val;
                            return;
                        case "Name":
                            Name = (string)val!;
                            return;
                        case "Location":
                            Location = (Realms.Tests.Database.GeospatialTests.CustomGeoPoint?)val;
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
                        "Offices" => (IList<T>)Offices,
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
            private class CompanySerializer : Realms.Serialization.RealmObjectSerializerBase<Company>
            {
                public override string SchemaName => "Company";

                protected override void SerializeValue(MongoDB.Bson.Serialization.BsonSerializationContext context, BsonSerializationArgs args, Company value)
                {
                    context.Writer.WriteStartDocument();

                    WriteValue(context, args, "_id", value.Id);
                    WriteValue(context, args, "Name", value.Name);
                    WriteValue(context, args, "Location", value.Location);
                    WriteList(context, args, "Offices", value.Offices);

                    context.Writer.WriteEndDocument();
                }

                protected override Company CreateInstance() => new Company();

                protected override void ReadValue(Company instance, string name, BsonDeserializationContext context)
                {
                    switch (name)
                    {
                        case "_id":
                            instance.Id = BsonSerializer.LookupSerializer<MongoDB.Bson.ObjectId>().Deserialize(context);
                            break;
                        case "Name":
                            instance.Name = BsonSerializer.LookupSerializer<string>().Deserialize(context);
                            break;
                        case "Location":
                            instance.Location = BsonSerializer.LookupSerializer<Realms.Tests.Database.GeospatialTests.CustomGeoPoint?>().Deserialize(context);
                            break;
                        case "Offices":
                            ReadArray(instance, name, context);
                            break;
                        default:
                            context.Reader.SkipValue();
                            break;
                    }
                }

                protected override void ReadArrayElement(Company instance, string name, BsonDeserializationContext context)
                {
                    switch (name)
                    {
                        case "Offices":
                            instance.Offices.Add(BsonSerializer.LookupSerializer<Realms.Tests.Database.GeospatialTests.CustomGeoPoint>().Deserialize(context));
                            break;
                    }
                }

                protected override void ReadDocumentField(Company instance, string name, string fieldName, BsonDeserializationContext context)
                {
                    // No persisted dictionary properties to deserialize
                }
            }
        }
    }
}
