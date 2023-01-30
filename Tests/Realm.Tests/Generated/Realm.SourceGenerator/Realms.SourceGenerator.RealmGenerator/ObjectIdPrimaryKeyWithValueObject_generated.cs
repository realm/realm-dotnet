﻿// <auto-generated />
#nullable enable

using MongoDB.Bson;
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
using TestAsymmetricObject = Realms.IAsymmetricObject;
using TestEmbeddedObject = Realms.IEmbeddedObject;
using TestRealmObject = Realms.IRealmObject;

namespace Realms.Tests
{
    [Generated]
    [Woven(typeof(ObjectIdPrimaryKeyWithValueObjectObjectHelper))]
    public partial class ObjectIdPrimaryKeyWithValueObject : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("ObjectIdPrimaryKeyWithValueObject", ObjectSchema.ObjectType.RealmObject)
        {
            Realms.Schema.Property.Primitive("_id", Realms.RealmValueType.ObjectId, isPrimaryKey: true, isIndexed: false, isNullable: false, managedName: "Id"),
            Realms.Schema.Property.Primitive("StringValue", Realms.RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "StringValue"),
        }.Build();

        #region IRealmObject implementation

        private IObjectIdPrimaryKeyWithValueObjectAccessor? _accessor;

        Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

        internal IObjectIdPrimaryKeyWithValueObjectAccessor Accessor => _accessor ??= new ObjectIdPrimaryKeyWithValueObjectUnmanagedAccessor(typeof(ObjectIdPrimaryKeyWithValueObject));

        [IgnoreDataMember, XmlIgnore]
        public bool IsManaged => Accessor.IsManaged;

        [IgnoreDataMember, XmlIgnore]
        public bool IsValid => Accessor.IsValid;

        [IgnoreDataMember, XmlIgnore]
        public bool IsFrozen => Accessor.IsFrozen;

        [IgnoreDataMember, XmlIgnore]
        public Realms.Realm Realm => Accessor.Realm;

        [IgnoreDataMember, XmlIgnore]
        public Realms.Schema.ObjectSchema ObjectSchema => Accessor.ObjectSchema;

        [IgnoreDataMember, XmlIgnore]
        public Realms.DynamicObjectApi DynamicApi => Accessor.DynamicApi;

        [IgnoreDataMember, XmlIgnore]
        public int BacklinksCount => Accessor.BacklinksCount;

        public void SetManagedAccessor(Realms.IRealmAccessor managedAccessor, Realms.Weaving.IRealmObjectHelper? helper = null, bool update = false, bool skipDefaults = false)
        {
            var newAccessor = (IObjectIdPrimaryKeyWithValueObjectAccessor)managedAccessor;
            var oldAccessor = _accessor;
            _accessor = newAccessor;

            if (helper != null && oldAccessor != null)
            {
                newAccessor.Id = oldAccessor.Id;
                if(!skipDefaults || oldAccessor.StringValue != default(string))
                {
                    newAccessor.StringValue = oldAccessor.StringValue;
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

        public static explicit operator ObjectIdPrimaryKeyWithValueObject(Realms.RealmValue val) => val.AsRealmObject<ObjectIdPrimaryKeyWithValueObject>();

        public static implicit operator Realms.RealmValue(ObjectIdPrimaryKeyWithValueObject? val) => val == null ? Realms.RealmValue.Null : Realms.RealmValue.Object(val);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public TypeInfo GetTypeInfo() => Accessor.GetTypeInfo(this);

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

            if (obj is not Realms.IRealmObjectBase iro)
            {
                return false;
            }

            return Accessor.Equals(iro.Accessor);
        }

        public override int GetHashCode() => IsManaged ? Accessor.GetHashCode() : base.GetHashCode();

        public override string? ToString() => Accessor.ToString();

        [EditorBrowsable(EditorBrowsableState.Never)]
        private class ObjectIdPrimaryKeyWithValueObjectObjectHelper : Realms.Weaving.IRealmObjectHelper
        {
            public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public Realms.ManagedAccessor CreateAccessor() => new ObjectIdPrimaryKeyWithValueObjectManagedAccessor();

            public Realms.IRealmObjectBase CreateInstance() => new ObjectIdPrimaryKeyWithValueObject();

            public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out object? value)
            {
                value = ((IObjectIdPrimaryKeyWithValueObjectAccessor)instance.Accessor).Id;
                return true;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal interface IObjectIdPrimaryKeyWithValueObjectAccessor : Realms.IRealmAccessor
        {
            MongoDB.Bson.ObjectId Id { get; set; }

            string? StringValue { get; set; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class ObjectIdPrimaryKeyWithValueObjectManagedAccessor : Realms.ManagedAccessor, IObjectIdPrimaryKeyWithValueObjectAccessor
        {
            public MongoDB.Bson.ObjectId Id
            {
                get => (MongoDB.Bson.ObjectId)GetValue("_id");
                set => SetValueUnique("_id", value);
            }

            public string? StringValue
            {
                get => (string?)GetValue("StringValue");
                set => SetValue("StringValue", value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class ObjectIdPrimaryKeyWithValueObjectUnmanagedAccessor : Realms.UnmanagedAccessor, IObjectIdPrimaryKeyWithValueObjectAccessor
        {
            public override ObjectSchema ObjectSchema => ObjectIdPrimaryKeyWithValueObject.RealmSchema;

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

            public ObjectIdPrimaryKeyWithValueObjectUnmanagedAccessor(Type objectType) : base(objectType)
            {
            }

            public override Realms.RealmValue GetValue(string propertyName)
            {
                return propertyName switch
                {
                    "_id" => _id,
                    "StringValue" => _stringValue,
                    _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
                };
            }

            public override void SetValue(string propertyName, Realms.RealmValue val)
            {
                switch (propertyName)
                {
                    case "_id":
                        throw new InvalidOperationException("Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique");
                    case "StringValue":
                        StringValue = (string?)val;
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
    }
}
