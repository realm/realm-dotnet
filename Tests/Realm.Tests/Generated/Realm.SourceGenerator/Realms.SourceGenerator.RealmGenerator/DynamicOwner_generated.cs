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
    [Woven(typeof(DynamicOwnerObjectHelper)), Realms.Preserve(AllMembers = true)]
    public partial class DynamicOwner : IRealmObject, INotifyPropertyChanged, IReflectableType
    {

        [Realms.Preserve]
        static DynamicOwner()
        {
            Realms.Serialization.RealmObjectSerializer.Register(new DynamicOwnerSerializer());
        }

        /// <summary>
        /// Defines the schema for the <see cref="DynamicOwner"/> class.
        /// </summary>
        public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("DynamicOwner", ObjectSchema.ObjectType.RealmObject)
        {
            Realms.Schema.Property.Primitive("Name", Realms.RealmValueType.String, isPrimaryKey: false, indexType: IndexType.None, isNullable: true, managedName: "Name"),
            Realms.Schema.Property.Object("TopDog", "DynamicDog", managedName: "TopDog"),
            Realms.Schema.Property.ObjectList("Dogs", "DynamicDog", managedName: "Dogs"),
            Realms.Schema.Property.PrimitiveList("Tags", Realms.RealmValueType.String, areElementsNullable: false, managedName: "Tags"),
            Realms.Schema.Property.ObjectDictionary("DogsDictionary", "DynamicDog", managedName: "DogsDictionary"),
            Realms.Schema.Property.PrimitiveDictionary("TagsDictionary", Realms.RealmValueType.String, areElementsNullable: true, managedName: "TagsDictionary"),
            Realms.Schema.Property.ObjectSet("DogsSet", "DynamicDog", managedName: "DogsSet"),
            Realms.Schema.Property.PrimitiveSet("TagsSet", Realms.RealmValueType.String, areElementsNullable: true, managedName: "TagsSet"),
        }.Build();

        #region IRealmObject implementation

        private IDynamicOwnerAccessor? _accessor;

        Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

        private IDynamicOwnerAccessor Accessor => _accessor ??= new DynamicOwnerUnmanagedAccessor(typeof(DynamicOwner));

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
            var newAccessor = (IDynamicOwnerAccessor)managedAccessor;
            var oldAccessor = _accessor;
            _accessor = newAccessor;

            if (helper != null && oldAccessor != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.Dogs.Clear();
                    newAccessor.Tags.Clear();
                    newAccessor.DogsDictionary.Clear();
                    newAccessor.TagsDictionary.Clear();
                    newAccessor.DogsSet.Clear();
                    newAccessor.TagsSet.Clear();
                }

                if (!skipDefaults || oldAccessor.Name != default(string?))
                {
                    newAccessor.Name = oldAccessor.Name;
                }
                if (oldAccessor.TopDog != null && newAccessor.Realm != null)
                {
                    newAccessor.Realm.Add(oldAccessor.TopDog, update);
                }
                newAccessor.TopDog = oldAccessor.TopDog;
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.Dogs, newAccessor.Dogs, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.Tags, newAccessor.Tags, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.DogsDictionary, newAccessor.DogsDictionary, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.TagsDictionary, newAccessor.TagsDictionary, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.DogsSet, newAccessor.DogsSet, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.TagsSet, newAccessor.TagsSet, update, skipDefaults);
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
        /// Converts a <see cref="Realms.RealmValue"/> to <see cref="DynamicOwner"/>. Equivalent to <see cref="Realms.RealmValue.AsNullableRealmObject{T}"/>.
        /// </summary>
        /// <param name="val">The <see cref="Realms.RealmValue"/> to convert.</param>
        /// <returns>The <see cref="DynamicOwner"/> stored in the <see cref="Realms.RealmValue"/>.</returns>
        public static explicit operator DynamicOwner?(Realms.RealmValue val) => val.Type == Realms.RealmValueType.Null ? null : val.AsRealmObject<DynamicOwner>();

        /// <summary>
        /// Implicitly constructs a <see cref="Realms.RealmValue"/> from <see cref="DynamicOwner"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="Realms.RealmValue"/>.</param>
        /// <returns>A <see cref="Realms.RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator Realms.RealmValue(DynamicOwner? val) => val == null ? Realms.RealmValue.Null : Realms.RealmValue.Object(val);

        /// <summary>
        /// Implicitly constructs a <see cref="Realms.QueryArgument"/> from <see cref="DynamicOwner"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="Realms.QueryArgument"/>.</param>
        /// <returns>A <see cref="Realms.QueryArgument"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator Realms.QueryArgument(DynamicOwner? val) => (Realms.RealmValue)val;

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
        private class DynamicOwnerObjectHelper : Realms.Weaving.IRealmObjectHelper
        {
            public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public Realms.ManagedAccessor CreateAccessor() => new DynamicOwnerManagedAccessor();

            public Realms.IRealmObjectBase CreateInstance() => new DynamicOwner();

            public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out RealmValue value)
            {
                value = RealmValue.Null;
                return false;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        internal interface IDynamicOwnerAccessor : Realms.IRealmAccessor
        {
            string? Name { get; set; }

            Realms.Tests.Database.DynamicDog? TopDog { get; set; }

            System.Collections.Generic.IList<Realms.Tests.Database.DynamicDog> Dogs { get; }

            System.Collections.Generic.IList<string> Tags { get; }

            System.Collections.Generic.IDictionary<string, Realms.Tests.Database.DynamicDog?> DogsDictionary { get; }

            System.Collections.Generic.IDictionary<string, string?> TagsDictionary { get; }

            System.Collections.Generic.ISet<Realms.Tests.Database.DynamicDog> DogsSet { get; }

            System.Collections.Generic.ISet<string?> TagsSet { get; }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class DynamicOwnerManagedAccessor : Realms.ManagedAccessor, IDynamicOwnerAccessor
        {
            public string? Name
            {
                get => (string?)GetValue("Name");
                set => SetValue("Name", value);
            }

            public Realms.Tests.Database.DynamicDog? TopDog
            {
                get => (Realms.Tests.Database.DynamicDog?)GetValue("TopDog");
                set => SetValue("TopDog", value);
            }

            private System.Collections.Generic.IList<Realms.Tests.Database.DynamicDog> _dogs = null!;
            public System.Collections.Generic.IList<Realms.Tests.Database.DynamicDog> Dogs
            {
                get
                {
                    if (_dogs == null)
                    {
                        _dogs = GetListValue<Realms.Tests.Database.DynamicDog>("Dogs");
                    }

                    return _dogs;
                }
            }

            private System.Collections.Generic.IList<string> _tags = null!;
            public System.Collections.Generic.IList<string> Tags
            {
                get
                {
                    if (_tags == null)
                    {
                        _tags = GetListValue<string>("Tags");
                    }

                    return _tags;
                }
            }

            private System.Collections.Generic.IDictionary<string, Realms.Tests.Database.DynamicDog?> _dogsDictionary = null!;
            public System.Collections.Generic.IDictionary<string, Realms.Tests.Database.DynamicDog?> DogsDictionary
            {
                get
                {
                    if (_dogsDictionary == null)
                    {
                        _dogsDictionary = GetDictionaryValue<Realms.Tests.Database.DynamicDog?>("DogsDictionary");
                    }

                    return _dogsDictionary;
                }
            }

            private System.Collections.Generic.IDictionary<string, string?> _tagsDictionary = null!;
            public System.Collections.Generic.IDictionary<string, string?> TagsDictionary
            {
                get
                {
                    if (_tagsDictionary == null)
                    {
                        _tagsDictionary = GetDictionaryValue<string?>("TagsDictionary");
                    }

                    return _tagsDictionary;
                }
            }

            private System.Collections.Generic.ISet<Realms.Tests.Database.DynamicDog> _dogsSet = null!;
            public System.Collections.Generic.ISet<Realms.Tests.Database.DynamicDog> DogsSet
            {
                get
                {
                    if (_dogsSet == null)
                    {
                        _dogsSet = GetSetValue<Realms.Tests.Database.DynamicDog>("DogsSet");
                    }

                    return _dogsSet;
                }
            }

            private System.Collections.Generic.ISet<string?> _tagsSet = null!;
            public System.Collections.Generic.ISet<string?> TagsSet
            {
                get
                {
                    if (_tagsSet == null)
                    {
                        _tagsSet = GetSetValue<string?>("TagsSet");
                    }

                    return _tagsSet;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class DynamicOwnerUnmanagedAccessor : Realms.UnmanagedAccessor, IDynamicOwnerAccessor
        {
            public override ObjectSchema ObjectSchema => DynamicOwner.RealmSchema;

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

            private Realms.Tests.Database.DynamicDog? _topDog;
            public Realms.Tests.Database.DynamicDog? TopDog
            {
                get => _topDog;
                set
                {
                    _topDog = value;
                    RaisePropertyChanged("TopDog");
                }
            }

            public System.Collections.Generic.IList<Realms.Tests.Database.DynamicDog> Dogs { get; } = new List<Realms.Tests.Database.DynamicDog>();

            public System.Collections.Generic.IList<string> Tags { get; } = new List<string>();

            public System.Collections.Generic.IDictionary<string, Realms.Tests.Database.DynamicDog?> DogsDictionary { get; } = new Dictionary<string, Realms.Tests.Database.DynamicDog?>();

            public System.Collections.Generic.IDictionary<string, string?> TagsDictionary { get; } = new Dictionary<string, string?>();

            public System.Collections.Generic.ISet<Realms.Tests.Database.DynamicDog> DogsSet { get; } = new HashSet<Realms.Tests.Database.DynamicDog>(RealmSet<Realms.Tests.Database.DynamicDog>.Comparer);

            public System.Collections.Generic.ISet<string?> TagsSet { get; } = new HashSet<string?>(RealmSet<string?>.Comparer);

            public DynamicOwnerUnmanagedAccessor(Type objectType) : base(objectType)
            {
            }

            public override Realms.RealmValue GetValue(string propertyName)
            {
                return propertyName switch
                {
                    "Name" => _name,
                    "TopDog" => _topDog,
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
                    case "TopDog":
                        TopDog = (Realms.Tests.Database.DynamicDog?)val;
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
                    "Dogs" => (IList<T>)Dogs,
                    "Tags" => (IList<T>)Tags,
                    _ => throw new MissingMemberException($"The object does not have a Realm list property with name {propertyName}"),
                };
            }

            public override ISet<T> GetSetValue<T>(string propertyName)
            {
                return propertyName switch
                {
                    "DogsSet" => (ISet<T>)DogsSet,
                    "TagsSet" => (ISet<T>)TagsSet,
                    _ => throw new MissingMemberException($"The object does not have a Realm set property with name {propertyName}"),
                };
            }

            public override IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
            {
                return propertyName switch
                {
                    "DogsDictionary" => (IDictionary<string, TValue>)DogsDictionary,
                    "TagsDictionary" => (IDictionary<string, TValue>)TagsDictionary,
                    _ => throw new MissingMemberException($"The object does not have a Realm dictionary property with name {propertyName}"),
                };
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class DynamicOwnerSerializer : Realms.Serialization.RealmObjectSerializer<DynamicOwner>
        {
            protected override void SerializeValue(MongoDB.Bson.Serialization.BsonSerializationContext context, BsonSerializationArgs args, DynamicOwner value)
            {
                context.Writer.WriteStartDocument();

                WriteValue(context, args, "Name", value.Name);
                WriteValue(context, args, "TopDog", value.TopDog);
                WriteList(context, args, "Dogs", value.Dogs);
                WriteList(context, args, "Tags", value.Tags);
                WriteDictionary(context, args, "DogsDictionary", value.DogsDictionary);
                WriteDictionary(context, args, "TagsDictionary", value.TagsDictionary);
                WriteSet(context, args, "DogsSet", value.DogsSet);
                WriteSet(context, args, "TagsSet", value.TagsSet);

                context.Writer.WriteEndDocument();
            }

            protected override DynamicOwner CreateInstance() => new DynamicOwner();

            protected override void ReadValue(DynamicOwner instance, string name, BsonDeserializationContext context)
            {
                switch (name)
                {
                    case "Name":
                        instance.Name = BsonSerializer.LookupSerializer<string?>().Deserialize(context);
                        break;
                    case "TopDog":
                        instance.TopDog = LookupSerializer<Realms.Tests.Database.DynamicDog?>()!.DeserializeById(context);
                        break;
                }
            }

            protected override void ReadArrayElement(DynamicOwner instance, string name, BsonDeserializationContext context)
            {
                switch (name)
                {
                    case "Dogs":
                        instance.Dogs.Add(LookupSerializer<Realms.Tests.Database.DynamicDog>()!.DeserializeById(context)!);
                        break;
                    case "Tags":
                        instance.Tags.Add(BsonSerializer.LookupSerializer<string>().Deserialize(context));
                        break;
                    case "DogsSet":
                        instance.DogsSet.Add(LookupSerializer<Realms.Tests.Database.DynamicDog>()!.DeserializeById(context)!);
                        break;
                    case "TagsSet":
                        instance.TagsSet.Add(BsonSerializer.LookupSerializer<string?>().Deserialize(context));
                        break;
                }
            }

            protected override void ReadDocumentField(DynamicOwner instance, string name, string fieldName, BsonDeserializationContext context)
            {
                switch (name)
                {
                    case "DogsDictionary":
                        instance.DogsDictionary[fieldName] = LookupSerializer<Realms.Tests.Database.DynamicDog?>()!.DeserializeById(context)!;
                        break;
                    case "TagsDictionary":
                        instance.TagsDictionary[fieldName] = BsonSerializer.LookupSerializer<string?>().Deserialize(context);
                        break;
                }
            }
        }
    }
}
