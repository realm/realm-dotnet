﻿// <auto-generated />
using MongoDB.Bson;
using Realms;
using Realms.Schema;
using Realms.Tests;
using Realms.Tests.Sync;
using Realms.Tests.Sync.Generated;
using Realms.Weaving;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Realms.Tests.Sync
{
    [Generated]
    [Woven(typeof(AsymmetricObjectWithEmbeddedRecursiveObjectObjectHelper))]
    public partial class AsymmetricObjectWithEmbeddedRecursiveObject : IAsymmetricObject, INotifyPropertyChanged, IReflectableType
    {
        public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("AsymmetricObjectWithEmbeddedRecursiveObject", ObjectSchema.ObjectType.AsymmetricObject)
        {
            Realms.Schema.Property.Primitive("_id", Realms.RealmValueType.ObjectId, isPrimaryKey: true, isIndexed: false, isNullable: false, managedName: "Id"),
            Realms.Schema.Property.Object("RecursiveObject", "EmbeddedLevel1", managedName: "RecursiveObject"),
        }.Build();

        #region IAsymmetricObject implementation

        private IAsymmetricObjectWithEmbeddedRecursiveObjectAccessor _accessor;

        Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

        internal IAsymmetricObjectWithEmbeddedRecursiveObjectAccessor Accessor => _accessor ?? (_accessor = new AsymmetricObjectWithEmbeddedRecursiveObjectUnmanagedAccessor(typeof(AsymmetricObjectWithEmbeddedRecursiveObject)));

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

        public void SetManagedAccessor(Realms.IRealmAccessor managedAccessor, Realms.Weaving.IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var newAccessor = (IAsymmetricObjectWithEmbeddedRecursiveObjectAccessor)managedAccessor;
            var oldAccessor = (IAsymmetricObjectWithEmbeddedRecursiveObjectAccessor)_accessor;
            _accessor = newAccessor;

            if (helper != null)
            {
                newAccessor.Id = oldAccessor.Id;
                newAccessor.RecursiveObject = oldAccessor.RecursiveObject;
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

        private event PropertyChangedEventHandler _propertyChanged;

        public event PropertyChangedEventHandler PropertyChanged
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
        partial void OnPropertyChanged(string propertyName);

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
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

        public static explicit operator AsymmetricObjectWithEmbeddedRecursiveObject(Realms.RealmValue val) => val.AsRealmObject<AsymmetricObjectWithEmbeddedRecursiveObject>();

        public static implicit operator Realms.RealmValue(AsymmetricObjectWithEmbeddedRecursiveObject val) => Realms.RealmValue.Object(val);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public TypeInfo GetTypeInfo() => Accessor.GetTypeInfo(this);

        public override bool Equals(object obj)
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

        public override string ToString() => Accessor.ToString();

        [EditorBrowsable(EditorBrowsableState.Never)]
        private class AsymmetricObjectWithEmbeddedRecursiveObjectObjectHelper : Realms.Weaving.IRealmObjectHelper
        {
            public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public Realms.ManagedAccessor CreateAccessor() => new AsymmetricObjectWithEmbeddedRecursiveObjectManagedAccessor();

            public Realms.IRealmObjectBase CreateInstance() => new AsymmetricObjectWithEmbeddedRecursiveObject();

            public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out object value)
            {
                value = ((IAsymmetricObjectWithEmbeddedRecursiveObjectAccessor)instance.Accessor).Id;
                return true;
            }
        }
    }
}

namespace Realms.Tests.Sync.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IAsymmetricObjectWithEmbeddedRecursiveObjectAccessor : Realms.IRealmAccessor
    {
        MongoDB.Bson.ObjectId Id { get; set; }

        Realms.Tests.EmbeddedLevel1 RecursiveObject { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class AsymmetricObjectWithEmbeddedRecursiveObjectManagedAccessor : Realms.ManagedAccessor, IAsymmetricObjectWithEmbeddedRecursiveObjectAccessor
    {
        public MongoDB.Bson.ObjectId Id
        {
            get => (MongoDB.Bson.ObjectId)GetValue("_id");
            set => SetValueUnique("_id", value);
        }

        public Realms.Tests.EmbeddedLevel1 RecursiveObject
        {
            get => (Realms.Tests.EmbeddedLevel1)GetValue("RecursiveObject");
            set => SetValue("RecursiveObject", value);
        }
    }

    internal class AsymmetricObjectWithEmbeddedRecursiveObjectUnmanagedAccessor : Realms.UnmanagedAccessor, IAsymmetricObjectWithEmbeddedRecursiveObjectAccessor
    {
        public override ObjectSchema ObjectSchema => AsymmetricObjectWithEmbeddedRecursiveObject.RealmSchema;

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

        private Realms.Tests.EmbeddedLevel1 _recursiveObject;
        public Realms.Tests.EmbeddedLevel1 RecursiveObject
        {
            get => _recursiveObject;
            set
            {
                _recursiveObject = value;
                RaisePropertyChanged("RecursiveObject");
            }
        }

        public AsymmetricObjectWithEmbeddedRecursiveObjectUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override Realms.RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "_id" => _id,
                "RecursiveObject" => _recursiveObject,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }

        public override void SetValue(string propertyName, Realms.RealmValue val)
        {
            switch (propertyName)
            {
                case "_id":
                    throw new InvalidOperationException("Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique");
                case "RecursiveObject":
                    RecursiveObject = (Realms.Tests.EmbeddedLevel1)val;
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