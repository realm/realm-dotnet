﻿// <auto-generated />
using Realms.Tests.Sync.Generated;
using Realms.Tests.Sync;
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

namespace Realms.Tests.Sync
{
    [Generated]
    [Woven(typeof(SyncObjectWithRequiredStringListObjectHelper))]
    public partial class SyncObjectWithRequiredStringList : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("SyncObjectWithRequiredStringList", ObjectSchema.ObjectType.RealmObject)
        {
            Property.Primitive("_id", RealmValueType.String, isPrimaryKey: true, isIndexed: false, isNullable: true, managedName: "Id"),
            Property.PrimitiveList("Strings", RealmValueType.String, areElementsNullable: false, managedName: "Strings"),
        }.Build();

        #region IRealmObject implementation

        private ISyncObjectWithRequiredStringListAccessor _accessor;

        IRealmAccessor IRealmObjectBase.Accessor => Accessor;

        internal ISyncObjectWithRequiredStringListAccessor Accessor => _accessor = _accessor ?? new SyncObjectWithRequiredStringListUnmanagedAccessor(typeof(SyncObjectWithRequiredStringList));

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
            var newAccessor = (ISyncObjectWithRequiredStringListAccessor)managedAccessor;
            var oldAccessor = _accessor as ISyncObjectWithRequiredStringListAccessor;
            _accessor = newAccessor;

            if (helper != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.Strings.Clear();
                }

                if(!skipDefaults || oldAccessor.Id != default(string))
                {
                    newAccessor.Id = oldAccessor.Id;
                }
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.Strings, newAccessor.Strings, update, skipDefaults);
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

        public static explicit operator SyncObjectWithRequiredStringList(RealmValue val) => val.AsRealmObject<SyncObjectWithRequiredStringList>();

        public static implicit operator RealmValue(SyncObjectWithRequiredStringList val) => RealmValue.Object(val);

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
        private class SyncObjectWithRequiredStringListObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public ManagedAccessor CreateAccessor() => new SyncObjectWithRequiredStringListManagedAccessor();

            public IRealmObjectBase CreateInstance() => new SyncObjectWithRequiredStringList();

            public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
            {
                value = ((ISyncObjectWithRequiredStringListAccessor)instance.Accessor).Id;
                return true;
            }
        }
    }
}

namespace Realms.Tests.Sync.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface ISyncObjectWithRequiredStringListAccessor : IRealmAccessor
    {
        string Id { get; set; }

        IList<string> Strings { get; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class SyncObjectWithRequiredStringListManagedAccessor : ManagedAccessor, ISyncObjectWithRequiredStringListAccessor
    {
        public string Id
        {
            get => (string)GetValue("_id");
            set => SetValueUnique("_id", value);
        }

        private IList<string> _strings;
        public IList<string> Strings
        {
            get
            {
                if (_strings == null)
                {
                    _strings = GetListValue<string>("Strings");
                }

                return _strings;
            }
        }
    }

    internal class SyncObjectWithRequiredStringListUnmanagedAccessor : UnmanagedAccessor, ISyncObjectWithRequiredStringListAccessor
    {
        private string _id;
        public string Id
        {
            get => _id;
            set
            {
                _id = value;
                RaisePropertyChanged("Id");
            }
        }

        public IList<string> Strings { get; } = new List<string>();

        public SyncObjectWithRequiredStringListUnmanagedAccessor(Type objectType) : base(objectType)
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

            Id = (string)val;
        }

        public override IList<T> GetListValue<T>(string propertyName)
        {
            return propertyName switch
                        {
            "Strings" => (IList<T>)Strings,

                            _ => throw new MissingMemberException($"The object does not have a Realm list property with name {propertyName}"),
                        };
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