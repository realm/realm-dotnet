﻿// <auto-generated />
using Realms.Tests.Generated;
using Realms.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Reflection;
using System.ComponentModel;
using Realms;
using Realms.Weaving;
using Realms.Schema;

namespace Realms.Tests
{
    [Generated]
    [Woven(typeof(PrimaryKeyNullableGuidObjectObjectHelper))]
    public partial class PrimaryKeyNullableGuidObject : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("PrimaryKeyNullableGuidObject", ObjectSchema.ObjectType.RealmObject)
        {
            Property.Primitive("_id", RealmValueType.Guid, isPrimaryKey: true, isIndexed: false, isNullable: true, managedName: "Id"),
        }.Build();

        #region IRealmObject implementation

        private IPrimaryKeyNullableGuidObjectAccessor _accessor;

        IRealmAccessor IRealmObjectBase.Accessor => Accessor;

        internal IPrimaryKeyNullableGuidObjectAccessor Accessor => _accessor ?? (_accessor = new PrimaryKeyNullableGuidObjectUnmanagedAccessor(typeof(PrimaryKeyNullableGuidObject)));

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
            var newAccessor = (IPrimaryKeyNullableGuidObjectAccessor)managedAccessor;
            var oldAccessor = (IPrimaryKeyNullableGuidObjectAccessor)_accessor;
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

        public static explicit operator PrimaryKeyNullableGuidObject(RealmValue val) => val.AsRealmObject<PrimaryKeyNullableGuidObject>();

        public static implicit operator RealmValue(PrimaryKeyNullableGuidObject val) => RealmValue.Object(val);

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
        private class PrimaryKeyNullableGuidObjectObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public ManagedAccessor CreateAccessor() => new PrimaryKeyNullableGuidObjectManagedAccessor();

            public IRealmObjectBase CreateInstance() => new PrimaryKeyNullableGuidObject();

            public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
            {
                value = ((IPrimaryKeyNullableGuidObjectAccessor)instance.Accessor).Id;
                return true;
            }
        }
    }
}

namespace Realms.Tests.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IPrimaryKeyNullableGuidObjectAccessor : IRealmAccessor
    {
        Guid? Id { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class PrimaryKeyNullableGuidObjectManagedAccessor : ManagedAccessor, IPrimaryKeyNullableGuidObjectAccessor
    {
        public Guid? Id
        {
            get => (Guid?)GetValue("_id");
            set => SetValueUnique("_id", value);
        }
    }

    internal class PrimaryKeyNullableGuidObjectUnmanagedAccessor : UnmanagedAccessor, IPrimaryKeyNullableGuidObjectAccessor
    {
        private Guid? _id;
        public Guid? Id
        {
            get => _id;
            set
            {
                _id = value;
                RaisePropertyChanged("Id");
            }
        }

        public PrimaryKeyNullableGuidObjectUnmanagedAccessor(Type objectType) : base(objectType)
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

            Id = (Guid?)val;
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