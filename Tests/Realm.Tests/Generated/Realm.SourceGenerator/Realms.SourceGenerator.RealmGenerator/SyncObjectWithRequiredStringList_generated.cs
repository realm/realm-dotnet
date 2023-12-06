﻿// <auto-generated />
#nullable enable

using Baas;
using MongoDB.Bson.Serialization;
using NUnit.Framework;
using Realms;
using Realms.Exceptions.Sync;
using Realms.Schema;
using Realms.Sync;
using Realms.Sync.ErrorHandling;
using Realms.Sync.Exceptions;
using Realms.Sync.Native;
using Realms.Sync.Testing;
using Realms.Tests.Sync;
using Realms.Weaving;
using static Realms.Sync.ErrorHandling.ClientResetHandlerBase;
using static Realms.Tests.TestHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TestRealmObject = Realms.IRealmObject;

namespace Realms.Tests.Sync
{
    [Generated]
    [Woven(typeof(SyncObjectWithRequiredStringListObjectHelper)), Realms.Preserve(AllMembers = true)]
    public partial class SyncObjectWithRequiredStringList : IRealmObject, INotifyPropertyChanged, IReflectableType
    {

        [Realms.Preserve]
        static SyncObjectWithRequiredStringList()
        {
            Realms.Serialization.RealmObjectSerializer.Register(new SyncObjectWithRequiredStringListSerializer());
        }

        /// <summary>
        /// Defines the schema for the <see cref="SyncObjectWithRequiredStringList"/> class.
        /// </summary>
        public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("SyncObjectWithRequiredStringList", ObjectSchema.ObjectType.RealmObject)
        {
            Realms.Schema.Property.Primitive("_id", Realms.RealmValueType.String, isPrimaryKey: true, indexType: IndexType.None, isNullable: true, managedName: "Id"),
            Realms.Schema.Property.PrimitiveList("Strings", Realms.RealmValueType.String, areElementsNullable: false, managedName: "Strings"),
        }.Build();

        #region IRealmObject implementation

        private ISyncObjectWithRequiredStringListAccessor? _accessor;

        Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

        private ISyncObjectWithRequiredStringListAccessor Accessor => _accessor ??= new SyncObjectWithRequiredStringListUnmanagedAccessor(typeof(SyncObjectWithRequiredStringList));

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
            var newAccessor = (ISyncObjectWithRequiredStringListAccessor)managedAccessor;
            var oldAccessor = _accessor;
            _accessor = newAccessor;

            if (helper != null && oldAccessor != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.Strings.Clear();
                }

                if (!skipDefaults || oldAccessor.Id != default(string?))
                {
                    newAccessor.Id = oldAccessor.Id;
                }
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.Strings, newAccessor.Strings, update, skipDefaults);
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
        /// Converts a <see cref="Realms.RealmValue"/> to <see cref="SyncObjectWithRequiredStringList"/>. Equivalent to <see cref="Realms.RealmValue.AsNullableRealmObject{T}"/>.
        /// </summary>
        /// <param name="val">The <see cref="Realms.RealmValue"/> to convert.</param>
        /// <returns>The <see cref="SyncObjectWithRequiredStringList"/> stored in the <see cref="Realms.RealmValue"/>.</returns>
        public static explicit operator SyncObjectWithRequiredStringList?(Realms.RealmValue val) => val.Type == Realms.RealmValueType.Null ? null : val.AsRealmObject<SyncObjectWithRequiredStringList>();

        /// <summary>
        /// Implicitly constructs a <see cref="Realms.RealmValue"/> from <see cref="SyncObjectWithRequiredStringList"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="Realms.RealmValue"/>.</param>
        /// <returns>A <see cref="Realms.RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator Realms.RealmValue(SyncObjectWithRequiredStringList? val) => val == null ? Realms.RealmValue.Null : Realms.RealmValue.Object(val);

        /// <summary>
        /// Implicitly constructs a <see cref="Realms.QueryArgument"/> from <see cref="SyncObjectWithRequiredStringList"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="Realms.QueryArgument"/>.</param>
        /// <returns>A <see cref="Realms.QueryArgument"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator Realms.QueryArgument(SyncObjectWithRequiredStringList? val) => (Realms.RealmValue)val;

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
        private class SyncObjectWithRequiredStringListObjectHelper : Realms.Weaving.IRealmObjectHelper
        {
            public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public Realms.ManagedAccessor CreateAccessor() => new SyncObjectWithRequiredStringListManagedAccessor();

            public Realms.IRealmObjectBase CreateInstance() => new SyncObjectWithRequiredStringList();

            public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out RealmValue value)
            {
                value = ((ISyncObjectWithRequiredStringListAccessor)instance.Accessor).Id;
                return true;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        internal interface ISyncObjectWithRequiredStringListAccessor : Realms.IRealmAccessor
        {
            string? Id { get; set; }

            System.Collections.Generic.IList<string> Strings { get; }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class SyncObjectWithRequiredStringListManagedAccessor : Realms.ManagedAccessor, ISyncObjectWithRequiredStringListAccessor
        {
            public string? Id
            {
                get => (string?)GetValue("_id");
                set => SetValueUnique("_id", value);
            }

            private System.Collections.Generic.IList<string> _strings = null!;
            public System.Collections.Generic.IList<string> Strings
            {
                get
                {
                    if (_strings == null)
                    {
                        _strings = GetListValue<string>("Strings");
                    }

                    return _strings;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class SyncObjectWithRequiredStringListUnmanagedAccessor : Realms.UnmanagedAccessor, ISyncObjectWithRequiredStringListAccessor
        {
            public override ObjectSchema ObjectSchema => SyncObjectWithRequiredStringList.RealmSchema;

            private string? _id;
            public string? Id
            {
                get => _id;
                set
                {
                    _id = value;
                    RaisePropertyChanged("Id");
                }
            }

            public System.Collections.Generic.IList<string> Strings { get; } = new List<string>();

            public SyncObjectWithRequiredStringListUnmanagedAccessor(Type objectType) : base(objectType)
            {
            }

            public override Realms.RealmValue GetValue(string propertyName)
            {
                return propertyName switch
                {
                    "_id" => _id,
                    _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
                };
            }

            public override void SetValue(string propertyName, Realms.RealmValue val)
            {
                switch (propertyName)
                {
                    case "_id":
                        throw new InvalidOperationException("Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique");
                    default:
                        throw new MissingMemberException($"The object does not have a settable Realm property with name {propertyName}");
                }
            }

            public override void SetValueUnique(string propertyName, Realms.RealmValue val)
            {
                if (propertyName != "_id")
                {
                    throw new InvalidOperationException($"Cannot set the value of non primary key property ({propertyName}) with SetValueUnique");
                }

                Id = (string?)val;
            }

            public override IList<T> GetListValue<T>(string propertyName)
            {
                return propertyName switch
                {
                    "Strings" => (IList<T>)Strings,
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
        private class SyncObjectWithRequiredStringListSerializer : Realms.Serialization.RealmObjectSerializer<SyncObjectWithRequiredStringList>
        {
            public override string SchemaName => "SyncObjectWithRequiredStringList";

            protected override void SerializeValue(MongoDB.Bson.Serialization.BsonSerializationContext context, BsonSerializationArgs args, SyncObjectWithRequiredStringList value)
            {
                context.Writer.WriteStartDocument();

                WriteValue(context, args, "_id", value.Id);
                WriteList(context, args, "Strings", value.Strings);

                context.Writer.WriteEndDocument();
            }

            protected override SyncObjectWithRequiredStringList CreateInstance() => new SyncObjectWithRequiredStringList();

            protected override void ReadValue(SyncObjectWithRequiredStringList instance, string name, BsonDeserializationContext context)
            {
                switch (name)
                {
                    case "_id":
                        instance.Id = BsonSerializer.LookupSerializer<string?>().Deserialize(context);
                        break;
                }
            }

            protected override void ReadArrayElement(SyncObjectWithRequiredStringList instance, string name, BsonDeserializationContext context)
            {
                switch (name)
                {
                    case "Strings":
                        instance.Strings.Add(BsonSerializer.LookupSerializer<string>().Deserialize(context));
                        break;
                }
            }

            protected override void ReadDocumentField(SyncObjectWithRequiredStringList instance, string name, string fieldName, BsonDeserializationContext context)
            {
                // No persisted dictionary properties to deserialize
            }
        }
    }
}
