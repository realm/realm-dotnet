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
    [Woven(typeof(ObjectWithFtsIndexObjectHelper)), Realms.Preserve(AllMembers = true)]
    public partial class ObjectWithFtsIndex : IRealmObject, INotifyPropertyChanged, IReflectableType
    {

        [Realms.Preserve]
        static ObjectWithFtsIndex()
        {
            Realms.Serialization.RealmObjectSerializer.Register(new ObjectWithFtsIndexSerializer());
        }

        /// <summary>
        /// Defines the schema for the <see cref="ObjectWithFtsIndex"/> class.
        /// </summary>
        public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("ObjectWithFtsIndex", ObjectSchema.ObjectType.RealmObject)
        {
            Realms.Schema.Property.Primitive("Title", Realms.RealmValueType.String, isPrimaryKey: true, indexType: IndexType.None, isNullable: false, managedName: "Title"),
            Realms.Schema.Property.Primitive("Summary", Realms.RealmValueType.String, isPrimaryKey: false, indexType: IndexType.FullText, isNullable: false, managedName: "Summary"),
            Realms.Schema.Property.Primitive("NullableSummary", Realms.RealmValueType.String, isPrimaryKey: false, indexType: IndexType.FullText, isNullable: true, managedName: "NullableSummary"),
        }.Build();

        #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private ObjectWithFtsIndex() {}
        #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        #region IRealmObject implementation

        private IObjectWithFtsIndexAccessor? _accessor;

        Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

        private IObjectWithFtsIndexAccessor Accessor => _accessor ??= new ObjectWithFtsIndexUnmanagedAccessor(typeof(ObjectWithFtsIndex));

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
            var newAccessor = (IObjectWithFtsIndexAccessor)managedAccessor;
            var oldAccessor = _accessor;
            _accessor = newAccessor;

            if (helper != null && oldAccessor != null)
            {
                newAccessor.Title = oldAccessor.Title;
                newAccessor.Summary = oldAccessor.Summary;
                if (!skipDefaults || oldAccessor.NullableSummary != default(string?))
                {
                    newAccessor.NullableSummary = oldAccessor.NullableSummary;
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
        /// Converts a <see cref="Realms.RealmValue"/> to <see cref="ObjectWithFtsIndex"/>. Equivalent to <see cref="Realms.RealmValue.AsNullableRealmObject{T}"/>.
        /// </summary>
        /// <param name="val">The <see cref="Realms.RealmValue"/> to convert.</param>
        /// <returns>The <see cref="ObjectWithFtsIndex"/> stored in the <see cref="Realms.RealmValue"/>.</returns>
        public static explicit operator ObjectWithFtsIndex?(Realms.RealmValue val) => val.Type == Realms.RealmValueType.Null ? null : val.AsRealmObject<ObjectWithFtsIndex>();

        /// <summary>
        /// Implicitly constructs a <see cref="Realms.RealmValue"/> from <see cref="ObjectWithFtsIndex"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="Realms.RealmValue"/>.</param>
        /// <returns>A <see cref="Realms.RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator Realms.RealmValue(ObjectWithFtsIndex? val) => val == null ? Realms.RealmValue.Null : Realms.RealmValue.Object(val);

        /// <summary>
        /// Implicitly constructs a <see cref="Realms.QueryArgument"/> from <see cref="ObjectWithFtsIndex"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="Realms.QueryArgument"/>.</param>
        /// <returns>A <see cref="Realms.QueryArgument"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator Realms.QueryArgument(ObjectWithFtsIndex? val) => (Realms.RealmValue)val;

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
        private class ObjectWithFtsIndexObjectHelper : Realms.Weaving.IRealmObjectHelper
        {
            public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public Realms.ManagedAccessor CreateAccessor() => new ObjectWithFtsIndexManagedAccessor();

            public Realms.IRealmObjectBase CreateInstance() => new ObjectWithFtsIndex();

            public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out RealmValue value)
            {
                value = ((IObjectWithFtsIndexAccessor)instance.Accessor).Title;
                return true;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        internal interface IObjectWithFtsIndexAccessor : Realms.IRealmAccessor
        {
            string Title { get; set; }

            string Summary { get; set; }

            string? NullableSummary { get; set; }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class ObjectWithFtsIndexManagedAccessor : Realms.ManagedAccessor, IObjectWithFtsIndexAccessor
        {
            public string Title
            {
                get => (string)GetValue("Title")!;
                set => SetValueUnique("Title", value);
            }

            public string Summary
            {
                get => (string)GetValue("Summary")!;
                set => SetValue("Summary", value);
            }

            public string? NullableSummary
            {
                get => (string?)GetValue("NullableSummary");
                set => SetValue("NullableSummary", value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class ObjectWithFtsIndexUnmanagedAccessor : Realms.UnmanagedAccessor, IObjectWithFtsIndexAccessor
        {
            public override ObjectSchema ObjectSchema => ObjectWithFtsIndex.RealmSchema;

            private string _title = null!;
            public string Title
            {
                get => _title;
                set
                {
                    _title = value;
                    RaisePropertyChanged("Title");
                }
            }

            private string _summary = string.Empty;
            public string Summary
            {
                get => _summary;
                set
                {
                    _summary = value;
                    RaisePropertyChanged("Summary");
                }
            }

            private string? _nullableSummary;
            public string? NullableSummary
            {
                get => _nullableSummary;
                set
                {
                    _nullableSummary = value;
                    RaisePropertyChanged("NullableSummary");
                }
            }

            public ObjectWithFtsIndexUnmanagedAccessor(Type objectType) : base(objectType)
            {
            }

            public override Realms.RealmValue GetValue(string propertyName)
            {
                return propertyName switch
                {
                    "Title" => _title,
                    "Summary" => _summary,
                    "NullableSummary" => _nullableSummary,
                    _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
                };
            }

            public override void SetValue(string propertyName, Realms.RealmValue val)
            {
                switch (propertyName)
                {
                    case "Title":
                        throw new InvalidOperationException("Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique");
                    case "Summary":
                        Summary = (string)val!;
                        return;
                    case "NullableSummary":
                        NullableSummary = (string?)val;
                        return;
                    default:
                        throw new MissingMemberException($"The object does not have a settable Realm property with name {propertyName}");
                }
            }

            public override void SetValueUnique(string propertyName, Realms.RealmValue val)
            {
                if (propertyName != "Title")
                {
                    throw new InvalidOperationException($"Cannot set the value of non primary key property ({propertyName}) with SetValueUnique");
                }

                Title = (string)val!;
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
        private class ObjectWithFtsIndexSerializer : Realms.Serialization.RealmObjectSerializer<ObjectWithFtsIndex>
        {
            public override string SchemaName => "ObjectWithFtsIndex";

            protected override void SerializeValue(MongoDB.Bson.Serialization.BsonSerializationContext context, BsonSerializationArgs args, ObjectWithFtsIndex value)
            {
                context.Writer.WriteStartDocument();

                WriteValue(context, args, "Title", value.Title);
                WriteValue(context, args, "Summary", value.Summary);
                WriteValue(context, args, "NullableSummary", value.NullableSummary);

                context.Writer.WriteEndDocument();
            }

            protected override ObjectWithFtsIndex CreateInstance() => new ObjectWithFtsIndex();

            protected override void ReadValue(ObjectWithFtsIndex instance, string name, BsonDeserializationContext context)
            {
                switch (name)
                {
                    case "Title":
                        instance.Title = BsonSerializer.LookupSerializer<string>().Deserialize(context);
                        break;
                    case "Summary":
                        instance.Summary = BsonSerializer.LookupSerializer<string>().Deserialize(context);
                        break;
                    case "NullableSummary":
                        instance.NullableSummary = BsonSerializer.LookupSerializer<string?>().Deserialize(context);
                        break;
                }
            }

            protected override void ReadArrayElement(ObjectWithFtsIndex instance, string name, BsonDeserializationContext context)
            {
                // No persisted list/set properties to deserialize
            }

            protected override void ReadDocumentField(ObjectWithFtsIndex instance, string name, string fieldName, BsonDeserializationContext context)
            {
                // No persisted dictionary properties to deserialize
            }
        }
    }
}
