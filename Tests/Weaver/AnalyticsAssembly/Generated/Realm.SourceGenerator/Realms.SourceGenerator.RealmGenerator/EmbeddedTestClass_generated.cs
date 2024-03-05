﻿// <auto-generated />
#nullable enable

using MongoDB.Bson.Serialization;
using Realms;
using Realms.Schema;
using Realms.Sync;
using Realms.Sync.ErrorHandling;
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
using TestAsymmetricObject = Realms.IAsymmetricObject;
using TestEmbeddedObject = Realms.IEmbeddedObject;
using TestRealmObject = Realms.IRealmObject;

[Generated]
[Woven(typeof(EmbeddedTestClassObjectHelper)), Realms.Preserve(AllMembers = true)]
public partial class EmbeddedTestClass : IEmbeddedObject, INotifyPropertyChanged, IReflectableType
{

    [Realms.Preserve]
    static EmbeddedTestClass()
    {
        Realms.Serialization.RealmObjectSerializer.Register(new EmbeddedTestClassSerializer());
    }

    /// <summary>
    /// Defines the schema for the <see cref="EmbeddedTestClass"/> class.
    /// </summary>
    public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("EmbeddedTestClass", ObjectSchema.ObjectType.EmbeddedObject)
    {
        Realms.Schema.Property.Primitive("Int32Property", Realms.RealmValueType.Int, isPrimaryKey: false, indexType: IndexType.None, isNullable: false, managedName: "Int32Property"),
    }.Build();

    #region IEmbeddedObject implementation

    private IEmbeddedTestClassAccessor? _accessor;

    Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

    private IEmbeddedTestClassAccessor Accessor => _accessor ??= new EmbeddedTestClassUnmanagedAccessor(typeof(EmbeddedTestClass));

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

    /// <inheritdoc />
    [IgnoreDataMember, XmlIgnore]
    public Realms.IRealmObjectBase? Parent => Accessor.GetParent();

    void ISettableManagedAccessor.SetManagedAccessor(Realms.IRealmAccessor managedAccessor, Realms.Weaving.IRealmObjectHelper? helper, bool update, bool skipDefaults)
    {
        var newAccessor = (IEmbeddedTestClassAccessor)managedAccessor;
        var oldAccessor = _accessor;
        _accessor = newAccessor;

        if (helper != null && oldAccessor != null)
        {
            if (!skipDefaults || oldAccessor.Int32Property != default(int))
            {
                newAccessor.Int32Property = oldAccessor.Int32Property;
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
    /// Converts a <see cref="Realms.RealmValue"/> to <see cref="EmbeddedTestClass"/>. Equivalent to <see cref="Realms.RealmValue.AsNullableRealmObject{T}"/>.
    /// </summary>
    /// <param name="val">The <see cref="Realms.RealmValue"/> to convert.</param>
    /// <returns>The <see cref="EmbeddedTestClass"/> stored in the <see cref="Realms.RealmValue"/>.</returns>
    public static explicit operator EmbeddedTestClass?(Realms.RealmValue val) => val.Type == Realms.RealmValueType.Null ? null : val.AsRealmObject<EmbeddedTestClass>();

    /// <summary>
    /// Implicitly constructs a <see cref="Realms.RealmValue"/> from <see cref="EmbeddedTestClass"/>.
    /// </summary>
    /// <param name="val">The value to store in the <see cref="Realms.RealmValue"/>.</param>
    /// <returns>A <see cref="Realms.RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
    public static implicit operator Realms.RealmValue(EmbeddedTestClass? val) => val == null ? Realms.RealmValue.Null : Realms.RealmValue.Object(val);

    /// <summary>
    /// Implicitly constructs a <see cref="Realms.QueryArgument"/> from <see cref="EmbeddedTestClass"/>.
    /// </summary>
    /// <param name="val">The value to store in the <see cref="Realms.QueryArgument"/>.</param>
    /// <returns>A <see cref="Realms.QueryArgument"/> containing the supplied <paramref name="val"/>.</returns>
    public static implicit operator Realms.QueryArgument(EmbeddedTestClass? val) => (Realms.RealmValue)val;

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
    private class EmbeddedTestClassObjectHelper : Realms.Weaving.IRealmObjectHelper
    {
        public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
        {
            throw new InvalidOperationException("This method should not be called for source generated classes.");
        }

        public Realms.ManagedAccessor CreateAccessor() => new EmbeddedTestClassManagedAccessor();

        public Realms.IRealmObjectBase CreateInstance() => new EmbeddedTestClass();

        public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out RealmValue value)
        {
            value = RealmValue.Null;
            return false;
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
    internal interface IEmbeddedTestClassAccessor : Realms.IRealmAccessor
    {
        int Int32Property { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
    private class EmbeddedTestClassManagedAccessor : Realms.ManagedAccessor, IEmbeddedTestClassAccessor
    {
        public int Int32Property
        {
            get => (int)GetValue("Int32Property");
            set => SetValue("Int32Property", value);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
    private class EmbeddedTestClassUnmanagedAccessor : Realms.UnmanagedAccessor, IEmbeddedTestClassAccessor
    {
        public override ObjectSchema ObjectSchema => EmbeddedTestClass.RealmSchema;

        private int _int32Property;
        public int Int32Property
        {
            get => _int32Property;
            set
            {
                _int32Property = value;
                RaisePropertyChanged("Int32Property");
            }
        }

        public EmbeddedTestClassUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override Realms.RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "Int32Property" => _int32Property,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }

        public override void SetValue(string propertyName, Realms.RealmValue val)
        {
            switch (propertyName)
            {
                case "Int32Property":
                    Int32Property = (int)val;
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
    private class EmbeddedTestClassSerializer : Realms.Serialization.RealmObjectSerializerBase<EmbeddedTestClass>
    {
        public override string SchemaName => "EmbeddedTestClass";

        protected override void SerializeValue(MongoDB.Bson.Serialization.BsonSerializationContext context, BsonSerializationArgs args, EmbeddedTestClass value)
        {
            context.Writer.WriteStartDocument();

            WriteValue(context, args, "Int32Property", value.Int32Property);

            context.Writer.WriteEndDocument();
        }

        protected override EmbeddedTestClass CreateInstance() => new EmbeddedTestClass();

        protected override void ReadValue(EmbeddedTestClass instance, string name, BsonDeserializationContext context)
        {
            switch (name)
            {
                case "Int32Property":
                    instance.Int32Property = BsonSerializer.LookupSerializer<int>().Deserialize(context);
                    break;
                default:
                    context.Reader.SkipValue();
                    break;
            }
        }

        protected override void ReadArrayElement(EmbeddedTestClass instance, string name, BsonDeserializationContext context)
        {
            // No persisted list/set properties to deserialize
        }

        protected override void ReadDocumentField(EmbeddedTestClass instance, string name, string fieldName, BsonDeserializationContext context)
        {
            // No persisted dictionary properties to deserialize
        }
    }
}
