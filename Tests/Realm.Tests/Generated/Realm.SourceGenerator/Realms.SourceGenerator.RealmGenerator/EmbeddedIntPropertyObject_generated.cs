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
    [Woven(typeof(EmbeddedIntPropertyObjectObjectHelper))]
    public partial class EmbeddedIntPropertyObject : IEmbeddedObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("EmbeddedIntPropertyObject", ObjectSchema.ObjectType.EmbeddedObject)
        {
            Property.Primitive("Int", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "Int"),
        }.Build();

        #region IEmbeddedObject implementation

        private IEmbeddedIntPropertyObjectAccessor _accessor;

        IRealmAccessor IRealmObjectBase.Accessor => Accessor;

        internal IEmbeddedIntPropertyObjectAccessor Accessor => _accessor ?? (_accessor = new EmbeddedIntPropertyObjectUnmanagedAccessor(typeof(EmbeddedIntPropertyObject)));

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
            var newAccessor = (IEmbeddedIntPropertyObjectAccessor)managedAccessor;
            var oldAccessor = (IEmbeddedIntPropertyObjectAccessor)_accessor;
            _accessor = newAccessor;

            if (helper != null)
            {

                if(!skipDefaults || oldAccessor.Int != default(int))
                {
                    newAccessor.Int = oldAccessor.Int;
                }
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

        public static explicit operator EmbeddedIntPropertyObject(RealmValue val) => val.AsRealmObject<EmbeddedIntPropertyObject>();

        public static implicit operator RealmValue(EmbeddedIntPropertyObject val) => RealmValue.Object(val);

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

        [EditorBrowsable(EditorBrowsableState.Never)]
        private class EmbeddedIntPropertyObjectObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public ManagedAccessor CreateAccessor() => new EmbeddedIntPropertyObjectManagedAccessor();

            public IRealmObjectBase CreateInstance() => new EmbeddedIntPropertyObject();

            public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
            {
                value = null;
                return false;
            }
        }
    }
}

namespace Realms.Tests.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IEmbeddedIntPropertyObjectAccessor : IRealmAccessor
    {
        int Int { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class EmbeddedIntPropertyObjectManagedAccessor : ManagedAccessor, IEmbeddedIntPropertyObjectAccessor
    {
        public int Int
        {
            get => (int)GetValue("Int");
            set => SetValue("Int", value);
        }
    }

    internal class EmbeddedIntPropertyObjectUnmanagedAccessor : UnmanagedAccessor, IEmbeddedIntPropertyObjectAccessor
    {
        private int _int;
        public int Int
        {
            get => _int;
            set
            {
                _int = value;
                RaisePropertyChanged("Int");
            }
        }

        public EmbeddedIntPropertyObjectUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "Int" => _int,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }

        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "Int":
                    Int = (int)val;
                    return;
                default:
                    throw new MissingMemberException($"The object does not have a settable Realm property with name {propertyName}");
            }
        }

        public override void SetValueUnique(string propertyName, RealmValue val)
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
}