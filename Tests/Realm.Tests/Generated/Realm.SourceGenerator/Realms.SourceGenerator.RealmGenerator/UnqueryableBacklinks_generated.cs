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
    [Woven(typeof(UnqueryableBacklinksObjectHelper))]
    public partial class UnqueryableBacklinks : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("UnqueryableBacklinks", ObjectSchema.ObjectType.RealmObject)
        {
            Property.Object("Parent", "ClassWithUnqueryableMembers", managedName: "Parent"),
        }.Build();

        #region IRealmObject implementation

        private IUnqueryableBacklinksAccessor _accessor;

        IRealmAccessor IRealmObjectBase.Accessor => Accessor;

        internal IUnqueryableBacklinksAccessor Accessor => _accessor ?? (_accessor = new UnqueryableBacklinksUnmanagedAccessor(typeof(UnqueryableBacklinks)));

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
            var newAccessor = (IUnqueryableBacklinksAccessor)managedAccessor;
            var oldAccessor = (IUnqueryableBacklinksAccessor)_accessor;
            _accessor = newAccessor;

            if (helper != null)
            {
                if(oldAccessor.Parent != null)
                {
                    newAccessor.Realm.Add(oldAccessor.Parent, update);
                }
                newAccessor.Parent = oldAccessor.Parent;
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

        public static explicit operator UnqueryableBacklinks(RealmValue val) => val.AsRealmObject<UnqueryableBacklinks>();

        public static implicit operator RealmValue(UnqueryableBacklinks val) => RealmValue.Object(val);

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
        private class UnqueryableBacklinksObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public ManagedAccessor CreateAccessor() => new UnqueryableBacklinksManagedAccessor();

            public IRealmObjectBase CreateInstance() => new UnqueryableBacklinks();

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
    internal interface IUnqueryableBacklinksAccessor : IRealmAccessor
    {
        ClassWithUnqueryableMembers Parent { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class UnqueryableBacklinksManagedAccessor : ManagedAccessor, IUnqueryableBacklinksAccessor
    {
        public ClassWithUnqueryableMembers Parent
        {
            get => (ClassWithUnqueryableMembers)GetValue("Parent");
            set => SetValue("Parent", value);
        }
    }

    internal class UnqueryableBacklinksUnmanagedAccessor : UnmanagedAccessor, IUnqueryableBacklinksAccessor
    {
        private ClassWithUnqueryableMembers _parent;
        public ClassWithUnqueryableMembers Parent
        {
            get => _parent;
            set
            {
                _parent = value;
                RaisePropertyChanged("Parent");
            }
        }

        public UnqueryableBacklinksUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "Parent" => _parent,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }

        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "Parent":
                    Parent = (ClassWithUnqueryableMembers)val;
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