﻿// <auto-generated />
#nullable enable

using Baas;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NUnit.Framework;
using Realms;
using Realms.Schema;
using Realms.Sync;
using Realms.Sync.Exceptions;
using Realms.Tests.Sync;
using Realms.Weaving;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Realms.Tests.Sync
{
    [Generated]
    [Woven(typeof(NullablesV0ObjectHelper)), Realms.Preserve(AllMembers = true)]
    public partial class NullablesV0 : IRealmObject, INotifyPropertyChanged, IReflectableType
    {

        [Realms.Preserve]
        static NullablesV0()
        {
            Realms.Serialization.RealmObjectSerializer.Register(new NullablesV0Serializer());
        }

        /// <summary>
        /// Defines the schema for the <see cref="NullablesV0"/> class.
        /// </summary>
        [System.Reflection.Obfuscation]
        public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("Nullables", ObjectSchema.ObjectType.RealmObject)
        {
            Realms.Schema.Property.Primitive("_id", Realms.RealmValueType.ObjectId, isPrimaryKey: true, indexType: IndexType.None, isNullable: false, managedName: "Id"),
            Realms.Schema.Property.Primitive("Differentiator", Realms.RealmValueType.ObjectId, isPrimaryKey: false, indexType: IndexType.None, isNullable: false, managedName: "Differentiator"),
            Realms.Schema.Property.Primitive("BoolValue", Realms.RealmValueType.Bool, isPrimaryKey: false, indexType: IndexType.None, isNullable: true, managedName: "BoolValue"),
            Realms.Schema.Property.Primitive("IntValue", Realms.RealmValueType.Int, isPrimaryKey: false, indexType: IndexType.None, isNullable: true, managedName: "IntValue"),
            Realms.Schema.Property.Primitive("DoubleValue", Realms.RealmValueType.Double, isPrimaryKey: false, indexType: IndexType.None, isNullable: true, managedName: "DoubleValue"),
            Realms.Schema.Property.Primitive("DecimalValue", Realms.RealmValueType.Decimal128, isPrimaryKey: false, indexType: IndexType.None, isNullable: true, managedName: "DecimalValue"),
            Realms.Schema.Property.Primitive("DateValue", Realms.RealmValueType.Date, isPrimaryKey: false, indexType: IndexType.None, isNullable: true, managedName: "DateValue"),
            Realms.Schema.Property.Primitive("StringValue", Realms.RealmValueType.String, isPrimaryKey: false, indexType: IndexType.None, isNullable: true, managedName: "StringValue"),
            Realms.Schema.Property.Primitive("ObjectIdValue", Realms.RealmValueType.ObjectId, isPrimaryKey: false, indexType: IndexType.None, isNullable: true, managedName: "ObjectIdValue"),
            Realms.Schema.Property.Primitive("UuidValue", Realms.RealmValueType.Guid, isPrimaryKey: false, indexType: IndexType.None, isNullable: true, managedName: "UuidValue"),
            Realms.Schema.Property.Primitive("BinaryValue", Realms.RealmValueType.Data, isPrimaryKey: false, indexType: IndexType.None, isNullable: true, managedName: "BinaryValue"),
        }.Build();

        #region IRealmObject implementation

        private INullablesV0Accessor? _accessor;

        Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

        private INullablesV0Accessor Accessor => _accessor ??= new NullablesV0UnmanagedAccessor(typeof(NullablesV0));

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
            var newAccessor = (INullablesV0Accessor)managedAccessor;
            var oldAccessor = _accessor;
            _accessor = newAccessor;

            if (helper != null && oldAccessor != null)
            {
                if (!skipDefaults || oldAccessor.Id != default(MongoDB.Bson.ObjectId))
                {
                    newAccessor.Id = oldAccessor.Id;
                }
                if (!skipDefaults || oldAccessor.Differentiator != default(MongoDB.Bson.ObjectId))
                {
                    newAccessor.Differentiator = oldAccessor.Differentiator;
                }
                if (!skipDefaults || oldAccessor.BoolValue != default(bool?))
                {
                    newAccessor.BoolValue = oldAccessor.BoolValue;
                }
                if (!skipDefaults || oldAccessor.IntValue != default(int?))
                {
                    newAccessor.IntValue = oldAccessor.IntValue;
                }
                if (!skipDefaults || oldAccessor.DoubleValue != default(double?))
                {
                    newAccessor.DoubleValue = oldAccessor.DoubleValue;
                }
                if (!skipDefaults || oldAccessor.DecimalValue != default(MongoDB.Bson.Decimal128?))
                {
                    newAccessor.DecimalValue = oldAccessor.DecimalValue;
                }
                newAccessor.DateValue = oldAccessor.DateValue;
                if (!skipDefaults || oldAccessor.StringValue != default(string?))
                {
                    newAccessor.StringValue = oldAccessor.StringValue;
                }
                if (!skipDefaults || oldAccessor.ObjectIdValue != default(MongoDB.Bson.ObjectId?))
                {
                    newAccessor.ObjectIdValue = oldAccessor.ObjectIdValue;
                }
                if (!skipDefaults || oldAccessor.UuidValue != default(System.Guid?))
                {
                    newAccessor.UuidValue = oldAccessor.UuidValue;
                }
                if (!skipDefaults || oldAccessor.BinaryValue != default(byte[]?))
                {
                    newAccessor.BinaryValue = oldAccessor.BinaryValue;
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
        /// Converts a <see cref="Realms.RealmValue"/> to <see cref="NullablesV0"/>. Equivalent to <see cref="Realms.RealmValue.AsNullableRealmObject{T}"/>.
        /// </summary>
        /// <param name="val">The <see cref="Realms.RealmValue"/> to convert.</param>
        /// <returns>The <see cref="NullablesV0"/> stored in the <see cref="Realms.RealmValue"/>.</returns>
        public static explicit operator NullablesV0?(Realms.RealmValue val) => val.Type == Realms.RealmValueType.Null ? null : val.AsRealmObject<NullablesV0>();

        /// <summary>
        /// Implicitly constructs a <see cref="Realms.RealmValue"/> from <see cref="NullablesV0"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="Realms.RealmValue"/>.</param>
        /// <returns>A <see cref="Realms.RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator Realms.RealmValue(NullablesV0? val) => val == null ? Realms.RealmValue.Null : Realms.RealmValue.Object(val);

        /// <summary>
        /// Implicitly constructs a <see cref="Realms.QueryArgument"/> from <see cref="NullablesV0"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="Realms.QueryArgument"/>.</param>
        /// <returns>A <see cref="Realms.QueryArgument"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator Realms.QueryArgument(NullablesV0? val) => (Realms.RealmValue)val;

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
        private class NullablesV0ObjectHelper : Realms.Weaving.IRealmObjectHelper
        {
            public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public Realms.ManagedAccessor CreateAccessor() => new NullablesV0ManagedAccessor();

            public Realms.IRealmObjectBase CreateInstance() => new NullablesV0();

            public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out RealmValue value)
            {
                value = ((INullablesV0Accessor)instance.Accessor).Id;
                return true;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        internal interface INullablesV0Accessor : Realms.IRealmAccessor
        {
            MongoDB.Bson.ObjectId Id { get; set; }

            MongoDB.Bson.ObjectId Differentiator { get; set; }

            bool? BoolValue { get; set; }

            int? IntValue { get; set; }

            double? DoubleValue { get; set; }

            MongoDB.Bson.Decimal128? DecimalValue { get; set; }

            System.DateTimeOffset? DateValue { get; set; }

            string? StringValue { get; set; }

            MongoDB.Bson.ObjectId? ObjectIdValue { get; set; }

            System.Guid? UuidValue { get; set; }

            byte[]? BinaryValue { get; set; }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class NullablesV0ManagedAccessor : Realms.ManagedAccessor, INullablesV0Accessor
        {
            public MongoDB.Bson.ObjectId Id
            {
                get => (MongoDB.Bson.ObjectId)GetValue("_id");
                set => SetValueUnique("_id", value);
            }

            public MongoDB.Bson.ObjectId Differentiator
            {
                get => (MongoDB.Bson.ObjectId)GetValue("Differentiator");
                set => SetValue("Differentiator", value);
            }

            public bool? BoolValue
            {
                get => (bool?)GetValue("BoolValue");
                set => SetValue("BoolValue", value);
            }

            public int? IntValue
            {
                get => (int?)GetValue("IntValue");
                set => SetValue("IntValue", value);
            }

            public double? DoubleValue
            {
                get => (double?)GetValue("DoubleValue");
                set => SetValue("DoubleValue", value);
            }

            public MongoDB.Bson.Decimal128? DecimalValue
            {
                get => (MongoDB.Bson.Decimal128?)GetValue("DecimalValue");
                set => SetValue("DecimalValue", value);
            }

            public System.DateTimeOffset? DateValue
            {
                get => (System.DateTimeOffset?)GetValue("DateValue");
                set => SetValue("DateValue", value);
            }

            public string? StringValue
            {
                get => (string?)GetValue("StringValue");
                set => SetValue("StringValue", value);
            }

            public MongoDB.Bson.ObjectId? ObjectIdValue
            {
                get => (MongoDB.Bson.ObjectId?)GetValue("ObjectIdValue");
                set => SetValue("ObjectIdValue", value);
            }

            public System.Guid? UuidValue
            {
                get => (System.Guid?)GetValue("UuidValue");
                set => SetValue("UuidValue", value);
            }

            public byte[]? BinaryValue
            {
                get => (byte[]?)GetValue("BinaryValue");
                set => SetValue("BinaryValue", value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class NullablesV0UnmanagedAccessor : Realms.UnmanagedAccessor, INullablesV0Accessor
        {
            public override ObjectSchema ObjectSchema => NullablesV0.RealmSchema;

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

            private MongoDB.Bson.ObjectId _differentiator;
            public MongoDB.Bson.ObjectId Differentiator
            {
                get => _differentiator;
                set
                {
                    _differentiator = value;
                    RaisePropertyChanged("Differentiator");
                }
            }

            private bool? _boolValue;
            public bool? BoolValue
            {
                get => _boolValue;
                set
                {
                    _boolValue = value;
                    RaisePropertyChanged("BoolValue");
                }
            }

            private int? _intValue;
            public int? IntValue
            {
                get => _intValue;
                set
                {
                    _intValue = value;
                    RaisePropertyChanged("IntValue");
                }
            }

            private double? _doubleValue;
            public double? DoubleValue
            {
                get => _doubleValue;
                set
                {
                    _doubleValue = value;
                    RaisePropertyChanged("DoubleValue");
                }
            }

            private MongoDB.Bson.Decimal128? _decimalValue;
            public MongoDB.Bson.Decimal128? DecimalValue
            {
                get => _decimalValue;
                set
                {
                    _decimalValue = value;
                    RaisePropertyChanged("DecimalValue");
                }
            }

            private System.DateTimeOffset? _dateValue;
            public System.DateTimeOffset? DateValue
            {
                get => _dateValue;
                set
                {
                    _dateValue = value;
                    RaisePropertyChanged("DateValue");
                }
            }

            private string? _stringValue;
            public string? StringValue
            {
                get => _stringValue;
                set
                {
                    _stringValue = value;
                    RaisePropertyChanged("StringValue");
                }
            }

            private MongoDB.Bson.ObjectId? _objectIdValue;
            public MongoDB.Bson.ObjectId? ObjectIdValue
            {
                get => _objectIdValue;
                set
                {
                    _objectIdValue = value;
                    RaisePropertyChanged("ObjectIdValue");
                }
            }

            private System.Guid? _uuidValue;
            public System.Guid? UuidValue
            {
                get => _uuidValue;
                set
                {
                    _uuidValue = value;
                    RaisePropertyChanged("UuidValue");
                }
            }

            private byte[]? _binaryValue;
            public byte[]? BinaryValue
            {
                get => _binaryValue;
                set
                {
                    _binaryValue = value;
                    RaisePropertyChanged("BinaryValue");
                }
            }

            public NullablesV0UnmanagedAccessor(Type objectType) : base(objectType)
            {
            }

            public override Realms.RealmValue GetValue(string propertyName)
            {
                return propertyName switch
                {
                    "_id" => _id,
                    "Differentiator" => _differentiator,
                    "BoolValue" => _boolValue,
                    "IntValue" => _intValue,
                    "DoubleValue" => _doubleValue,
                    "DecimalValue" => _decimalValue,
                    "DateValue" => _dateValue,
                    "StringValue" => _stringValue,
                    "ObjectIdValue" => _objectIdValue,
                    "UuidValue" => _uuidValue,
                    "BinaryValue" => _binaryValue,
                    _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
                };
            }

            public override void SetValue(string propertyName, Realms.RealmValue val)
            {
                switch (propertyName)
                {
                    case "_id":
                        throw new InvalidOperationException("Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique");
                    case "Differentiator":
                        Differentiator = (MongoDB.Bson.ObjectId)val;
                        return;
                    case "BoolValue":
                        BoolValue = (bool?)val;
                        return;
                    case "IntValue":
                        IntValue = (int?)val;
                        return;
                    case "DoubleValue":
                        DoubleValue = (double?)val;
                        return;
                    case "DecimalValue":
                        DecimalValue = (MongoDB.Bson.Decimal128?)val;
                        return;
                    case "DateValue":
                        DateValue = (System.DateTimeOffset?)val;
                        return;
                    case "StringValue":
                        StringValue = (string?)val;
                        return;
                    case "ObjectIdValue":
                        ObjectIdValue = (MongoDB.Bson.ObjectId?)val;
                        return;
                    case "UuidValue":
                        UuidValue = (System.Guid?)val;
                        return;
                    case "BinaryValue":
                        BinaryValue = (byte[]?)val;
                        return;
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
        private class NullablesV0Serializer : Realms.Serialization.RealmObjectSerializerBase<NullablesV0>
        {
            public override string SchemaName => "Nullables";

            protected override void SerializeValue(MongoDB.Bson.Serialization.BsonSerializationContext context, BsonSerializationArgs args, NullablesV0 value)
            {
                context.Writer.WriteStartDocument();

                WriteValue(context, args, "_id", value.Id);
                WriteValue(context, args, "Differentiator", value.Differentiator);
                WriteValue(context, args, "BoolValue", value.BoolValue);
                WriteValue(context, args, "IntValue", value.IntValue);
                WriteValue(context, args, "DoubleValue", value.DoubleValue);
                WriteValue(context, args, "DecimalValue", value.DecimalValue);
                WriteValue(context, args, "DateValue", value.DateValue);
                WriteValue(context, args, "StringValue", value.StringValue);
                WriteValue(context, args, "ObjectIdValue", value.ObjectIdValue);
                WriteValue(context, args, "UuidValue", value.UuidValue);
                WriteValue(context, args, "BinaryValue", value.BinaryValue);

                context.Writer.WriteEndDocument();
            }

            protected override NullablesV0 CreateInstance() => new NullablesV0();

            protected override void ReadValue(NullablesV0 instance, string name, BsonDeserializationContext context)
            {
                switch (name)
                {
                    case "_id":
                        instance.Id = BsonSerializer.LookupSerializer<MongoDB.Bson.ObjectId>().Deserialize(context);
                        break;
                    case "Differentiator":
                        instance.Differentiator = BsonSerializer.LookupSerializer<MongoDB.Bson.ObjectId>().Deserialize(context);
                        break;
                    case "BoolValue":
                        instance.BoolValue = BsonSerializer.LookupSerializer<bool?>().Deserialize(context);
                        break;
                    case "IntValue":
                        instance.IntValue = BsonSerializer.LookupSerializer<int?>().Deserialize(context);
                        break;
                    case "DoubleValue":
                        instance.DoubleValue = BsonSerializer.LookupSerializer<double?>().Deserialize(context);
                        break;
                    case "DecimalValue":
                        instance.DecimalValue = BsonSerializer.LookupSerializer<MongoDB.Bson.Decimal128?>().Deserialize(context);
                        break;
                    case "DateValue":
                        instance.DateValue = BsonSerializer.LookupSerializer<System.DateTimeOffset?>().Deserialize(context);
                        break;
                    case "StringValue":
                        instance.StringValue = BsonSerializer.LookupSerializer<string?>().Deserialize(context);
                        break;
                    case "ObjectIdValue":
                        instance.ObjectIdValue = BsonSerializer.LookupSerializer<MongoDB.Bson.ObjectId?>().Deserialize(context);
                        break;
                    case "UuidValue":
                        instance.UuidValue = BsonSerializer.LookupSerializer<System.Guid?>().Deserialize(context);
                        break;
                    case "BinaryValue":
                        instance.BinaryValue = BsonSerializer.LookupSerializer<byte[]?>().Deserialize(context);
                        break;
                    default:
                        context.Reader.SkipValue();
                        break;
                }
            }

            protected override void ReadArrayElement(NullablesV0 instance, string name, BsonDeserializationContext context)
            {
                // No persisted list/set properties to deserialize
            }

            protected override void ReadDocumentField(NullablesV0 instance, string name, string fieldName, BsonDeserializationContext context)
            {
                // No persisted dictionary properties to deserialize
            }
        }
    }
}
