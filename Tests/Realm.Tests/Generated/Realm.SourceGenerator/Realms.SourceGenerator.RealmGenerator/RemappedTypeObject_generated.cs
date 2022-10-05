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
    [Woven(typeof(RemappedTypeObjectObjectHelper))]
    public partial class RemappedTypeObject : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("__RemappedTypeObject", ObjectSchema.ObjectType.RealmObject)
        {
            Property.Primitive("_id", RealmValueType.Int, isPrimaryKey: true, isIndexed: false, isNullable: false, managedName: "Id"),
            Property.Primitive("StringValue", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "StringValue"),
            Property.Object("NormalLink", "__RemappedTypeObject", managedName: "NormalLink"),
            Property.Object("__mappedLink", "__RemappedTypeObject", managedName: "MappedLink"),
            Property.ObjectList("NormalList", "__RemappedTypeObject", managedName: "NormalList"),
            Property.ObjectList("__mappedList", "__RemappedTypeObject", managedName: "MappedList"),
            Property.Backlinks("NormalBacklink", "__RemappedTypeObject", "NormalLink", managedName: "NormalBacklink"),
            Property.Backlinks("__mappedBacklink", "__RemappedTypeObject", "__mappedLink", managedName: "MappedBacklink"),
        }.Build();

        ~RemappedTypeObject()
        {
            UnsubscribeFromNotifications();
        }

        #region IRealmObject implementation

        private IRemappedTypeObjectAccessor _accessor;

        IRealmAccessor IRealmObjectBase.Accessor => Accessor;

        internal IRemappedTypeObjectAccessor Accessor => _accessor = _accessor ?? new RemappedTypeObjectUnmanagedAccessor(typeof(RemappedTypeObject));

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
            var newAccessor = (IRemappedTypeObjectAccessor)managedAccessor;
            var oldAccessor = _accessor as IRemappedTypeObjectAccessor;
            _accessor = newAccessor;

            if (helper != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.NormalList.Clear();
                    newAccessor.MappedList.Clear();
                }

                if(!skipDefaults || oldAccessor.Id != default(int))
                {
                    newAccessor.Id = oldAccessor.Id;
                }
                if(!skipDefaults || oldAccessor.StringValue != default(string))
                {
                    newAccessor.StringValue = oldAccessor.StringValue;
                }
                if(oldAccessor.NormalLink != null)
                {
                    newAccessor.Realm.Add(oldAccessor.NormalLink, update);
                }
                newAccessor.NormalLink = oldAccessor.NormalLink;
                if(oldAccessor.MappedLink != null)
                {
                    newAccessor.Realm.Add(oldAccessor.MappedLink, update);
                }
                newAccessor.MappedLink = oldAccessor.MappedLink;
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.NormalList, newAccessor.NormalList, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.MappedList, newAccessor.MappedList, update, skipDefaults);
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

        public static explicit operator RemappedTypeObject(RealmValue val) => val.AsRealmObject<RemappedTypeObject>();

        public static implicit operator RealmValue(RemappedTypeObject val) => RealmValue.Object(val);

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
        private class RemappedTypeObjectObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public ManagedAccessor CreateAccessor() => new RemappedTypeObjectManagedAccessor();

            public IRealmObjectBase CreateInstance() => new RemappedTypeObject();

            public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
            {
                value = ((IRemappedTypeObjectAccessor)instance.Accessor).Id;
                return true;
            }
        }
    }
}

namespace Realms.Tests.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IRemappedTypeObjectAccessor : IRealmAccessor
    {
        int Id { get; set; }

        string StringValue { get; set; }

        RemappedTypeObject NormalLink { get; set; }

        RemappedTypeObject MappedLink { get; set; }

        IList<RemappedTypeObject> NormalList { get; }

        IList<RemappedTypeObject> MappedList { get; }

        IQueryable<RemappedTypeObject> NormalBacklink { get; }

        IQueryable<RemappedTypeObject> MappedBacklink { get; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class RemappedTypeObjectManagedAccessor : ManagedAccessor, IRemappedTypeObjectAccessor
    {
        public int Id
        {
            get => (int)GetValue("_id");
            set => SetValueUnique("_id", value);
        }

        public string StringValue
        {
            get => (string)GetValue("StringValue");
            set => SetValue("StringValue", value);
        }

        public RemappedTypeObject NormalLink
        {
            get => (RemappedTypeObject)GetValue("NormalLink");
            set => SetValue("NormalLink", value);
        }

        public RemappedTypeObject MappedLink
        {
            get => (RemappedTypeObject)GetValue("__mappedLink");
            set => SetValue("__mappedLink", value);
        }

        private IList<RemappedTypeObject> _normalList;
        public IList<RemappedTypeObject> NormalList
        {
            get
            {
                if (_normalList == null)
                {
                    _normalList = GetListValue<RemappedTypeObject>("NormalList");
                }

                return _normalList;
            }
        }

        private IList<RemappedTypeObject> _mappedList;
        public IList<RemappedTypeObject> MappedList
        {
            get
            {
                if (_mappedList == null)
                {
                    _mappedList = GetListValue<RemappedTypeObject>("__mappedList");
                }

                return _mappedList;
            }
        }

        private IQueryable<RemappedTypeObject> _normalBacklink;
        public IQueryable<RemappedTypeObject> NormalBacklink
        {
            get
            {
                if (_normalBacklink == null)
                {
                    _normalBacklink = GetBacklinks<RemappedTypeObject>("NormalBacklink");
                }

                return _normalBacklink;
            }
        }

        private IQueryable<RemappedTypeObject> _mappedBacklink;
        public IQueryable<RemappedTypeObject> MappedBacklink
        {
            get
            {
                if (_mappedBacklink == null)
                {
                    _mappedBacklink = GetBacklinks<RemappedTypeObject>("__mappedBacklink");
                }

                return _mappedBacklink;
            }
        }
    }

    internal class RemappedTypeObjectUnmanagedAccessor : UnmanagedAccessor, IRemappedTypeObjectAccessor
    {
        private int _id;
        public int Id
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

        private RemappedTypeObject _normalLink;
        public RemappedTypeObject NormalLink
        {
            get => _normalLink;
            set
            {
                _normalLink = value;
                RaisePropertyChanged("NormalLink");
            }
        }

        private RemappedTypeObject _mappedLink;
        public RemappedTypeObject MappedLink
        {
            get => _mappedLink;
            set
            {
                _mappedLink = value;
                RaisePropertyChanged("MappedLink");
            }
        }

        public IList<RemappedTypeObject> NormalList { get; } = new List<RemappedTypeObject>();

        public IList<RemappedTypeObject> MappedList { get; } = new List<RemappedTypeObject>();

        public IQueryable<RemappedTypeObject> NormalBacklink => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects.");

        public IQueryable<RemappedTypeObject> MappedBacklink => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects.");

        public RemappedTypeObjectUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "_id" => _id,
                "StringValue" => _stringValue,
                "NormalLink" => _normalLink,
                "__mappedLink" => _mappedLink,
                "NormalBacklink" => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects."),
                "__mappedBacklink" => throw new NotSupportedException("Using backlinks is only possible for managed(persisted) objects."),
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }

        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "_id":
                    throw new InvalidOperationException("Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique");
                case "StringValue":
                    StringValue = (string)val;
                    return;
                case "NormalLink":
                    NormalLink = (RemappedTypeObject)val;
                    return;
                case "__mappedLink":
                    MappedLink = (RemappedTypeObject)val;
                    return;
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

            Id = (int)val;
        }

        public override IList<T> GetListValue<T>(string propertyName)
        {
            return propertyName switch
                        {
            "NormalList" => (IList<T>)NormalList,
            "__mappedList" => (IList<T>)MappedList,

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