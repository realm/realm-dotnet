﻿// <auto-generated />
using Realms.Tests.Database.Generated;
using Realms.Tests.Database;
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

namespace Realms.Tests.Database
{
    [Generated]
    [Woven(typeof(PrimaryKeyWithPKListObjectHelper))]
    public partial class PrimaryKeyWithPKList : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("PrimaryKeyWithPKList", ObjectSchema.ObjectType.RealmObject)
        {
            Property.Primitive("Id", RealmValueType.Int, isPrimaryKey: true, isIndexed: false, isNullable: false, managedName: "Id"),
            Property.Primitive("StringValue", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "StringValue"),
            Property.ObjectList("ListValue", "PrimaryKeyObject", managedName: "ListValue"),
        }.Build();

        #region IRealmObject implementation

        private IPrimaryKeyWithPKListAccessor _accessor;

        IRealmAccessor IRealmObjectBase.Accessor => Accessor;

        internal IPrimaryKeyWithPKListAccessor Accessor => _accessor = _accessor ?? new PrimaryKeyWithPKListUnmanagedAccessor(typeof(PrimaryKeyWithPKList));

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
            var newAccessor = (IPrimaryKeyWithPKListAccessor)managedAccessor;
            var oldAccessor = _accessor as IPrimaryKeyWithPKListAccessor;
            _accessor = newAccessor;

            if (helper != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.ListValue.Clear();
                }

                if(!skipDefaults || oldAccessor.Id != default(long))
                {
                    newAccessor.Id = oldAccessor.Id;
                }
                if(!skipDefaults || oldAccessor.StringValue != default(string))
                {
                    newAccessor.StringValue = oldAccessor.StringValue;
                }
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.ListValue, newAccessor.ListValue, update, skipDefaults);
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

        public static explicit operator PrimaryKeyWithPKList(RealmValue val) => val.AsRealmObject<PrimaryKeyWithPKList>();

        public static implicit operator RealmValue(PrimaryKeyWithPKList val) => RealmValue.Object(val);

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
        private class PrimaryKeyWithPKListObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public ManagedAccessor CreateAccessor() => new PrimaryKeyWithPKListManagedAccessor();

            public IRealmObjectBase CreateInstance() => new PrimaryKeyWithPKList();

            public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
            {
                value = ((IPrimaryKeyWithPKListAccessor)instance.Accessor).Id;
                return true;
            }
        }
    }
}

namespace Realms.Tests.Database.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IPrimaryKeyWithPKListAccessor : IRealmAccessor
    {
        long Id { get; set; }

        string StringValue { get; set; }

        IList<PrimaryKeyObject> ListValue { get; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class PrimaryKeyWithPKListManagedAccessor : ManagedAccessor, IPrimaryKeyWithPKListAccessor
    {
        public long Id
        {
            get => (long)GetValue("Id");
            set => SetValueUnique("Id", value);
        }

        public string StringValue
        {
            get => (string)GetValue("StringValue");
            set => SetValue("StringValue", value);
        }

        private IList<PrimaryKeyObject> _listValue;
        public IList<PrimaryKeyObject> ListValue
        {
            get
            {
                if (_listValue == null)
                {
                    _listValue = GetListValue<PrimaryKeyObject>("ListValue");
                }

                return _listValue;
            }
        }
    }

    internal class PrimaryKeyWithPKListUnmanagedAccessor : UnmanagedAccessor, IPrimaryKeyWithPKListAccessor
    {
        private long _id;
        public long Id
        {
            get => _id;
            set
            {
                _id = value;
                RaisePropertyChanged("Id");
            }
        }

        private string _stringValue;
        public string StringValue
        {
            get => _stringValue;
            set
            {
                _stringValue = value;
                RaisePropertyChanged("StringValue");
            }
        }

        public IList<PrimaryKeyObject> ListValue { get; } = new List<PrimaryKeyObject>();

        public PrimaryKeyWithPKListUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "Id" => _id,
                "StringValue" => _stringValue,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }

        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "Id":
                    throw new InvalidOperationException("Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique");
                case "StringValue":
                    StringValue = (string)val;
                    return;
                default:
                    throw new MissingMemberException($"The object does not have a settable Realm property with name {propertyName}");
            }
        }

        public override void SetValueUnique(string propertyName, RealmValue val)
        {
            if (propertyName != "Id")
            {
                throw new InvalidOperationException($"Cannot set the value of non primary key property ({propertyName}) with SetValueUnique");
            }

            Id = (long)val;
        }

        public override IList<T> GetListValue<T>(string propertyName)
        {
            return propertyName switch
                        {
            "ListValue" => (IList<T>)ListValue,

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