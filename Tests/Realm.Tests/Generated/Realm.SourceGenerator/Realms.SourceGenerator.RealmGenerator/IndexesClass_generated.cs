﻿// <auto-generated />
#nullable enable

using MongoDB.Bson;
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
    [Woven(typeof(IndexesClassObjectHelper)), Realms.Preserve(AllMembers = true)]
    public partial class IndexesClass : IRealmObject, INotifyPropertyChanged, IReflectableType
    {

        [Realms.Preserve]
        static IndexesClass()
        {
            Realms.Serialization.RealmObjectSerializer.Register(new IndexesClassSerializer());
        }

        /// <summary>
        /// Defines the schema for the <see cref="IndexesClass"/> class.
        /// </summary>
        public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("IndexesClass", ObjectSchema.ObjectType.RealmObject)
        {
            Realms.Schema.Property.Primitive("Id", Realms.RealmValueType.ObjectId, isPrimaryKey: true, indexType: IndexType.None, isNullable: false, managedName: "Id"),
            Realms.Schema.Property.Primitive("StringFts", Realms.RealmValueType.String, isPrimaryKey: false, indexType: IndexType.FullText, isNullable: true, managedName: "StringFts"),
            Realms.Schema.Property.Primitive("StringGeneral", Realms.RealmValueType.String, isPrimaryKey: false, indexType: IndexType.General, isNullable: true, managedName: "StringGeneral"),
            Realms.Schema.Property.Primitive("StringDefault", Realms.RealmValueType.String, isPrimaryKey: false, indexType: IndexType.General, isNullable: true, managedName: "StringDefault"),
            Realms.Schema.Property.Primitive("StringNone", Realms.RealmValueType.String, isPrimaryKey: false, indexType: IndexType.None, isNullable: true, managedName: "StringNone"),
            Realms.Schema.Property.Primitive("IntGeneral", Realms.RealmValueType.Int, isPrimaryKey: false, indexType: IndexType.General, isNullable: false, managedName: "IntGeneral"),
            Realms.Schema.Property.Primitive("IntDefault", Realms.RealmValueType.Int, isPrimaryKey: false, indexType: IndexType.General, isNullable: false, managedName: "IntDefault"),
            Realms.Schema.Property.Primitive("IntNone", Realms.RealmValueType.Int, isPrimaryKey: false, indexType: IndexType.None, isNullable: false, managedName: "IntNone"),
        }.Build();

        #region IRealmObject implementation

        private IIndexesClassAccessor? _accessor;

        Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

        private IIndexesClassAccessor Accessor => _accessor ??= new IndexesClassUnmanagedAccessor(typeof(IndexesClass));

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
            var newAccessor = (IIndexesClassAccessor)managedAccessor;
            var oldAccessor = _accessor;
            _accessor = newAccessor;

            if (helper != null && oldAccessor != null)
            {
                if (!skipDefaults || oldAccessor.Id != default(MongoDB.Bson.ObjectId))
                {
                    newAccessor.Id = oldAccessor.Id;
                }
                if (!skipDefaults || oldAccessor.StringFts != default(string?))
                {
                    newAccessor.StringFts = oldAccessor.StringFts;
                }
                if (!skipDefaults || oldAccessor.StringGeneral != default(string?))
                {
                    newAccessor.StringGeneral = oldAccessor.StringGeneral;
                }
                if (!skipDefaults || oldAccessor.StringDefault != default(string?))
                {
                    newAccessor.StringDefault = oldAccessor.StringDefault;
                }
                if (!skipDefaults || oldAccessor.StringNone != default(string?))
                {
                    newAccessor.StringNone = oldAccessor.StringNone;
                }
                if (!skipDefaults || oldAccessor.IntGeneral != default(int))
                {
                    newAccessor.IntGeneral = oldAccessor.IntGeneral;
                }
                if (!skipDefaults || oldAccessor.IntDefault != default(int))
                {
                    newAccessor.IntDefault = oldAccessor.IntDefault;
                }
                if (!skipDefaults || oldAccessor.IntNone != default(int))
                {
                    newAccessor.IntNone = oldAccessor.IntNone;
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
        /// Converts a <see cref="Realms.RealmValue"/> to <see cref="IndexesClass"/>. Equivalent to <see cref="Realms.RealmValue.AsNullableRealmObject{T}"/>.
        /// </summary>
        /// <param name="val">The <see cref="Realms.RealmValue"/> to convert.</param>
        /// <returns>The <see cref="IndexesClass"/> stored in the <see cref="Realms.RealmValue"/>.</returns>
        public static explicit operator IndexesClass?(Realms.RealmValue val) => val.Type == Realms.RealmValueType.Null ? null : val.AsRealmObject<IndexesClass>();

        /// <summary>
        /// Implicitly constructs a <see cref="Realms.RealmValue"/> from <see cref="IndexesClass"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="Realms.RealmValue"/>.</param>
        /// <returns>A <see cref="Realms.RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator Realms.RealmValue(IndexesClass? val) => val == null ? Realms.RealmValue.Null : Realms.RealmValue.Object(val);

        /// <summary>
        /// Implicitly constructs a <see cref="Realms.QueryArgument"/> from <see cref="IndexesClass"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="Realms.QueryArgument"/>.</param>
        /// <returns>A <see cref="Realms.QueryArgument"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator Realms.QueryArgument(IndexesClass? val) => (Realms.RealmValue)val;

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
        private class IndexesClassObjectHelper : Realms.Weaving.IRealmObjectHelper
        {
            public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public Realms.ManagedAccessor CreateAccessor() => new IndexesClassManagedAccessor();

            public Realms.IRealmObjectBase CreateInstance() => new IndexesClass();

            public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out RealmValue value)
            {
                value = ((IIndexesClassAccessor)instance.Accessor).Id;
                return true;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        internal interface IIndexesClassAccessor : Realms.IRealmAccessor
        {
            MongoDB.Bson.ObjectId Id { get; set; }

            string? StringFts { get; set; }

            string? StringGeneral { get; set; }

            string? StringDefault { get; set; }

            string? StringNone { get; set; }

            int IntGeneral { get; set; }

            int IntDefault { get; set; }

            int IntNone { get; set; }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class IndexesClassManagedAccessor : Realms.ManagedAccessor, IIndexesClassAccessor
        {
            public MongoDB.Bson.ObjectId Id
            {
                get => (MongoDB.Bson.ObjectId)GetValue("Id");
                set => SetValueUnique("Id", value);
            }

            public string? StringFts
            {
                get => (string?)GetValue("StringFts");
                set => SetValue("StringFts", value);
            }

            public string? StringGeneral
            {
                get => (string?)GetValue("StringGeneral");
                set => SetValue("StringGeneral", value);
            }

            public string? StringDefault
            {
                get => (string?)GetValue("StringDefault");
                set => SetValue("StringDefault", value);
            }

            public string? StringNone
            {
                get => (string?)GetValue("StringNone");
                set => SetValue("StringNone", value);
            }

            public int IntGeneral
            {
                get => (int)GetValue("IntGeneral");
                set => SetValue("IntGeneral", value);
            }

            public int IntDefault
            {
                get => (int)GetValue("IntDefault");
                set => SetValue("IntDefault", value);
            }

            public int IntNone
            {
                get => (int)GetValue("IntNone");
                set => SetValue("IntNone", value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class IndexesClassUnmanagedAccessor : Realms.UnmanagedAccessor, IIndexesClassAccessor
        {
            public override ObjectSchema ObjectSchema => IndexesClass.RealmSchema;

            private MongoDB.Bson.ObjectId _id;
            public MongoDB.Bson.ObjectId Id
            {
                get => _id;
                set
                {
                    _id = value;
                    RaisePropertyChanged("Id");
                }
            }

            private string? _stringFts;
            public string? StringFts
            {
                get => _stringFts;
                set
                {
                    _stringFts = value;
                    RaisePropertyChanged("StringFts");
                }
            }

            private string? _stringGeneral;
            public string? StringGeneral
            {
                get => _stringGeneral;
                set
                {
                    _stringGeneral = value;
                    RaisePropertyChanged("StringGeneral");
                }
            }

            private string? _stringDefault;
            public string? StringDefault
            {
                get => _stringDefault;
                set
                {
                    _stringDefault = value;
                    RaisePropertyChanged("StringDefault");
                }
            }

            private string? _stringNone;
            public string? StringNone
            {
                get => _stringNone;
                set
                {
                    _stringNone = value;
                    RaisePropertyChanged("StringNone");
                }
            }

            private int _intGeneral;
            public int IntGeneral
            {
                get => _intGeneral;
                set
                {
                    _intGeneral = value;
                    RaisePropertyChanged("IntGeneral");
                }
            }

            private int _intDefault;
            public int IntDefault
            {
                get => _intDefault;
                set
                {
                    _intDefault = value;
                    RaisePropertyChanged("IntDefault");
                }
            }

            private int _intNone;
            public int IntNone
            {
                get => _intNone;
                set
                {
                    _intNone = value;
                    RaisePropertyChanged("IntNone");
                }
            }

            public IndexesClassUnmanagedAccessor(Type objectType) : base(objectType)
            {
            }

            public override Realms.RealmValue GetValue(string propertyName)
            {
                return propertyName switch
                {
                    "Id" => _id,
                    "StringFts" => _stringFts,
                    "StringGeneral" => _stringGeneral,
                    "StringDefault" => _stringDefault,
                    "StringNone" => _stringNone,
                    "IntGeneral" => _intGeneral,
                    "IntDefault" => _intDefault,
                    "IntNone" => _intNone,
                    _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
                };
            }

            public override void SetValue(string propertyName, Realms.RealmValue val)
            {
                switch (propertyName)
                {
                    case "Id":
                        throw new InvalidOperationException("Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique");
                    case "StringFts":
                        StringFts = (string?)val;
                        return;
                    case "StringGeneral":
                        StringGeneral = (string?)val;
                        return;
                    case "StringDefault":
                        StringDefault = (string?)val;
                        return;
                    case "StringNone":
                        StringNone = (string?)val;
                        return;
                    case "IntGeneral":
                        IntGeneral = (int)val;
                        return;
                    case "IntDefault":
                        IntDefault = (int)val;
                        return;
                    case "IntNone":
                        IntNone = (int)val;
                        return;
                    default:
                        throw new MissingMemberException($"The object does not have a settable Realm property with name {propertyName}");
                }
            }

            public override void SetValueUnique(string propertyName, Realms.RealmValue val)
            {
                if (propertyName != "Id")
                {
                    throw new InvalidOperationException($"Cannot set the value of non primary key property ({propertyName}) with SetValueUnique");
                }

                Id = (MongoDB.Bson.ObjectId)val;
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
        private class IndexesClassSerializer : Realms.Serialization.RealmObjectSerializerBase<IndexesClass>
        {
            public override string SchemaName => "IndexesClass";

            protected override void SerializeValue(MongoDB.Bson.Serialization.BsonSerializationContext context, BsonSerializationArgs args, IndexesClass value)
            {
                context.Writer.WriteStartDocument();

                WriteValue(context, args, "Id", value.Id);
                WriteValue(context, args, "StringFts", value.StringFts);
                WriteValue(context, args, "StringGeneral", value.StringGeneral);
                WriteValue(context, args, "StringDefault", value.StringDefault);
                WriteValue(context, args, "StringNone", value.StringNone);
                WriteValue(context, args, "IntGeneral", value.IntGeneral);
                WriteValue(context, args, "IntDefault", value.IntDefault);
                WriteValue(context, args, "IntNone", value.IntNone);

                context.Writer.WriteEndDocument();
            }

            protected override IndexesClass CreateInstance() => new IndexesClass();

            protected override void ReadValue(IndexesClass instance, string name, BsonDeserializationContext context)
            {
                switch (name)
                {
                    case "Id":
                        instance.Id = BsonSerializer.LookupSerializer<MongoDB.Bson.ObjectId>().Deserialize(context);
                        break;
                    case "StringFts":
                        instance.StringFts = BsonSerializer.LookupSerializer<string?>().Deserialize(context);
                        break;
                    case "StringGeneral":
                        instance.StringGeneral = BsonSerializer.LookupSerializer<string?>().Deserialize(context);
                        break;
                    case "StringDefault":
                        instance.StringDefault = BsonSerializer.LookupSerializer<string?>().Deserialize(context);
                        break;
                    case "StringNone":
                        instance.StringNone = BsonSerializer.LookupSerializer<string?>().Deserialize(context);
                        break;
                    case "IntGeneral":
                        instance.IntGeneral = BsonSerializer.LookupSerializer<int>().Deserialize(context);
                        break;
                    case "IntDefault":
                        instance.IntDefault = BsonSerializer.LookupSerializer<int>().Deserialize(context);
                        break;
                    case "IntNone":
                        instance.IntNone = BsonSerializer.LookupSerializer<int>().Deserialize(context);
                        break;
                }
            }

            protected override void ReadArrayElement(IndexesClass instance, string name, BsonDeserializationContext context)
            {
                // No persisted list/set properties to deserialize
            }

            protected override void ReadDocumentField(IndexesClass instance, string name, string fieldName, BsonDeserializationContext context)
            {
                // No persisted dictionary properties to deserialize
            }
        }
    }
}
