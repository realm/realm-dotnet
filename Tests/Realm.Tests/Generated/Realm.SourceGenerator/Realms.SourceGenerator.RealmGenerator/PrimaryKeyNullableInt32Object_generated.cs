﻿// <auto-generated />
using Realms;
using Realms.Schema;
using Realms.Tests;
using Realms.Tests.Generated;
using Realms.Weaving;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Realms.Tests
{
    [Generated]
    [Woven(typeof(PrimaryKeyNullableInt32ObjectObjectHelper))]
    public partial class PrimaryKeyNullableInt32Object : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("PrimaryKeyNullableInt32Object", ObjectSchema.ObjectType.RealmObject)
        {
            Property.Primitive("_id", RealmValueType.Int, isPrimaryKey: true, isIndexed: false, isNullable: true, managedName: "Id"),
        }.Build();

        #region IRealmObject implementation

        private IPrimaryKeyNullableInt32ObjectAccessor _accessor;

        IRealmAccessor IRealmObjectBase.Accessor => Accessor;

        internal IPrimaryKeyNullableInt32ObjectAccessor Accessor => _accessor ?? (_accessor = new PrimaryKeyNullableInt32ObjectUnmanagedAccessor(typeof(PrimaryKeyNullableInt32Object)));

        [IgnoreDataMember, XmlIgnore]
        public bool IsManaged => Accessor.IsManaged;

        [IgnoreDataMember, XmlIgnore]
        public bool IsValid => Accessor.IsValid;

        [IgnoreDataMember, XmlIgnore]
        public bool IsFrozen => Accessor.IsFrozen;

        [IgnoreDataMember, XmlIgnore]
        public Realm Realm => Accessor.Realm;

        [IgnoreDataMember, XmlIgnore]
        public ObjectSchema ObjectSchema => Accessor.ObjectSchema;

        [IgnoreDataMember, XmlIgnore]
        public DynamicObjectApi DynamicApi => Accessor.DynamicApi;

        [IgnoreDataMember, XmlIgnore]
        public int BacklinksCount => Accessor.BacklinksCount;

        public void SetManagedAccessor(IRealmAccessor managedAccessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false)
        {
            var newAccessor = (IPrimaryKeyNullableInt32ObjectAccessor)managedAccessor;
            var oldAccessor = (IPrimaryKeyNullableInt32ObjectAccessor)_accessor;
            _accessor = newAccessor;

            if (helper != null)
            {
                newAccessor.Id = oldAccessor.Id;
            }

            if (_propertyChanged != null)
            {
                SubscribeForNotifications();
            }

            OnManaged();
        }

        #endregion

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

        public static explicit operator PrimaryKeyNullableInt32Object(RealmValue val) => val.AsRealmObject<PrimaryKeyNullableInt32Object>();

        public static implicit operator RealmValue(PrimaryKeyNullableInt32Object val) => RealmValue.Object(val);

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

            if (obj is not IRealmObjectBase iro)
            {
                return false;
            }

            return Accessor.Equals(iro.Accessor);
        }

        public override int GetHashCode() => IsManaged ? Accessor.GetHashCode() : base.GetHashCode();

        public override string ToString() => Accessor.ToString();

        [EditorBrowsable(EditorBrowsableState.Never)]
        private class PrimaryKeyNullableInt32ObjectObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public ManagedAccessor CreateAccessor() => new PrimaryKeyNullableInt32ObjectManagedAccessor();

            public IRealmObjectBase CreateInstance() => new PrimaryKeyNullableInt32Object();

            public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
            {
                value = ((IPrimaryKeyNullableInt32ObjectAccessor)instance.Accessor).Id;
                return true;
            }
        }
    }
}

namespace Realms.Tests.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IPrimaryKeyNullableInt32ObjectAccessor : IRealmAccessor
    {
        int? Id { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class PrimaryKeyNullableInt32ObjectManagedAccessor : ManagedAccessor, IPrimaryKeyNullableInt32ObjectAccessor
    {
        public int? Id
        {
            get => (int?)GetValue("_id");
            set => SetValueUnique("_id", value);
        }
    }

    internal class PrimaryKeyNullableInt32ObjectUnmanagedAccessor : UnmanagedAccessor, IPrimaryKeyNullableInt32ObjectAccessor
    {
        private int? _id;
        public int? Id
        {
            get => _id;
            set
            {
                _id = value;
                RaisePropertyChanged("Id");
            }
        }

        public PrimaryKeyNullableInt32ObjectUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "_id" => _id,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }

        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "_id":
                    throw new InvalidOperationException("Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique");
                default:
                    throw new MissingMemberException($"The object does not have a settable Realm property with name {propertyName}");
            }
        }

        public override void SetValueUnique(string propertyName, RealmValue val)
        {
            if (propertyName != "_id")
            {
                throw new InvalidOperationException($"Cannot set the value of non primary key property ({propertyName}) with SetValueUnique");
            }

            Id = (int?)val;
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