﻿// <auto-generated />
using Realms.Tests;
using Realms.Tests.Generated;
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
    [Woven(typeof(ObjectWithEmbeddedPropertiesObjectHelper))]
    public partial class ObjectWithEmbeddedProperties : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("ObjectWithEmbeddedProperties", isEmbedded: false)
        {
            Property.Primitive("PrimaryKey", RealmValueType.Int, isPrimaryKey: true, isIndexed: false, isNullable: false, managedName: "PrimaryKey"),
            Property.Object("AllTypesObject", "EmbeddedAllTypesObject", managedName: "AllTypesObject"),
            Property.ObjectList("ListOfAllTypesObjects", "EmbeddedAllTypesObject", managedName: "ListOfAllTypesObjects"),
            Property.Object("RecursiveObject", "EmbeddedLevel1", managedName: "RecursiveObject"),
            Property.ObjectDictionary("DictionaryOfAllTypesObjects", "EmbeddedAllTypesObject", managedName: "DictionaryOfAllTypesObjects"),
        }.Build();

        #region IRealmObject implementation

        private IObjectWithEmbeddedPropertiesAccessor _accessor;

        IRealmAccessor IRealmObjectBase.Accessor => Accessor;

        internal IObjectWithEmbeddedPropertiesAccessor Accessor => _accessor = _accessor ?? new ObjectWithEmbeddedPropertiesUnmanagedAccessor(typeof(ObjectWithEmbeddedProperties));

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
            var newAccessor = (IObjectWithEmbeddedPropertiesAccessor)managedAccessor;
            var oldAccessor = _accessor as IObjectWithEmbeddedPropertiesAccessor;
            _accessor = newAccessor;

            if (helper != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.ListOfAllTypesObjects.Clear();
                    newAccessor.DictionaryOfAllTypesObjects.Clear();
                }

                if(!skipDefaults || oldAccessor.PrimaryKey != default(int))
                {
                    newAccessor.PrimaryKey = oldAccessor.PrimaryKey;
                }
                newAccessor.AllTypesObject = oldAccessor.AllTypesObject;

                CollectionExtensions.PopulateCollection(oldAccessor.ListOfAllTypesObjects, newAccessor.ListOfAllTypesObjects, update, skipDefaults);

                newAccessor.RecursiveObject = oldAccessor.RecursiveObject;

                CollectionExtensions.PopulateCollection(oldAccessor.DictionaryOfAllTypesObjects, newAccessor.DictionaryOfAllTypesObjects, update, skipDefaults);
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

        public static explicit operator ObjectWithEmbeddedProperties(RealmValue val) => val.AsRealmObject<ObjectWithEmbeddedProperties>();

        public static implicit operator RealmValue(ObjectWithEmbeddedProperties val) => RealmValue.Object(val);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public TypeInfo GetTypeInfo()
        {
            return Accessor.GetTypeInfo(this);
        }

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

        public override int GetHashCode()
        {
            return IsManaged ? Accessor.GetHashCode() : base.GetHashCode();
        }

        public override string ToString()
        {
            return Accessor.ToString();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        private class ObjectWithEmbeddedPropertiesObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public ManagedAccessor CreateAccessor() => new ObjectWithEmbeddedPropertiesManagedAccessor();

            public IRealmObjectBase CreateInstance()
            {
                return new ObjectWithEmbeddedProperties();
            }

            public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
            {
                value = ((IObjectWithEmbeddedPropertiesAccessor)instance.Accessor).PrimaryKey;
                return true;
            }
        }
    }
}

namespace Realms.Tests.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IObjectWithEmbeddedPropertiesAccessor : IRealmAccessor
    {
        int PrimaryKey { get; set; }

        EmbeddedAllTypesObject AllTypesObject { get; set; }

        IList<EmbeddedAllTypesObject> ListOfAllTypesObjects { get; }

        EmbeddedLevel1 RecursiveObject { get; set; }

        IDictionary<string, EmbeddedAllTypesObject> DictionaryOfAllTypesObjects { get; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class ObjectWithEmbeddedPropertiesManagedAccessor : ManagedAccessor, IObjectWithEmbeddedPropertiesAccessor
    {
        public int PrimaryKey
        {
            get => (int)GetValue("PrimaryKey");
            set => SetValueUnique("PrimaryKey", value);
        }

        public EmbeddedAllTypesObject AllTypesObject
        {
            get => (EmbeddedAllTypesObject)GetValue("AllTypesObject");
            set => SetValue("AllTypesObject", value);
        }

        private IList<EmbeddedAllTypesObject> _listOfAllTypesObjects;
        public IList<EmbeddedAllTypesObject> ListOfAllTypesObjects
        {
            get
            {
                if (_listOfAllTypesObjects == null)
                {
                    _listOfAllTypesObjects = GetListValue<EmbeddedAllTypesObject>("ListOfAllTypesObjects");
                }

                return _listOfAllTypesObjects;
            }
        }

        public EmbeddedLevel1 RecursiveObject
        {
            get => (EmbeddedLevel1)GetValue("RecursiveObject");
            set => SetValue("RecursiveObject", value);
        }

        private IDictionary<string, EmbeddedAllTypesObject> _dictionaryOfAllTypesObjects;
        public IDictionary<string, EmbeddedAllTypesObject> DictionaryOfAllTypesObjects
        {
            get
            {
                if (_dictionaryOfAllTypesObjects == null)
                {
                    _dictionaryOfAllTypesObjects = GetDictionaryValue<EmbeddedAllTypesObject>("DictionaryOfAllTypesObjects");
                }

                return _dictionaryOfAllTypesObjects;
            }
        }
    }

    internal class ObjectWithEmbeddedPropertiesUnmanagedAccessor : UnmanagedAccessor, IObjectWithEmbeddedPropertiesAccessor
    {
        private int _primaryKey;
        public int PrimaryKey
        {
            get => _primaryKey;
            set
            {
                _primaryKey = value;
                RaisePropertyChanged("PrimaryKey");
            }
        }

        private EmbeddedAllTypesObject _allTypesObject;
        public EmbeddedAllTypesObject AllTypesObject
        {
            get => _allTypesObject;
            set
            {
                _allTypesObject = value;
                RaisePropertyChanged("AllTypesObject");
            }
        }

        public IList<EmbeddedAllTypesObject> ListOfAllTypesObjects { get; } = new List<EmbeddedAllTypesObject>();

        private EmbeddedLevel1 _recursiveObject;
        public EmbeddedLevel1 RecursiveObject
        {
            get => _recursiveObject;
            set
            {
                _recursiveObject = value;
                RaisePropertyChanged("RecursiveObject");
            }
        }

        public IDictionary<string, EmbeddedAllTypesObject> DictionaryOfAllTypesObjects { get; } = new Dictionary<string, EmbeddedAllTypesObject>();

        public ObjectWithEmbeddedPropertiesUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "PrimaryKey" => _primaryKey,
                "AllTypesObject" => _allTypesObject,
                "RecursiveObject" => _recursiveObject,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }

        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "PrimaryKey":
                    throw new InvalidOperationException("Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique");
                case "AllTypesObject":
                    AllTypesObject = (EmbeddedAllTypesObject)val;
                    return;
                case "RecursiveObject":
                    RecursiveObject = (EmbeddedLevel1)val;
                    return;
                default:
                    throw new MissingMemberException($"The object does not have a settable Realm property with name {propertyName}");
            }
        }

        public override void SetValueUnique(string propertyName, RealmValue val)
        {
            if (propertyName != "PrimaryKey")
            {
                throw new InvalidOperationException($"Cannot set the value of non primary key property ({propertyName}) with SetValueUnique");
            }

            PrimaryKey = (int)val;
        }

        public override IList<T> GetListValue<T>(string propertyName)
        {
            return propertyName switch
                        {
            "ListOfAllTypesObjects" => (IList<T>)ListOfAllTypesObjects,

                            _ => throw new MissingMemberException($"The object does not have a Realm list property with name {propertyName}"),
                        };
        }

        public override ISet<T> GetSetValue<T>(string propertyName)
        {
            throw new MissingMemberException($"The object does not have a Realm set property with name {propertyName}");
        }

        public override IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
        {
            return propertyName switch
            {
                "DictionaryOfAllTypesObjects" => (IDictionary<string, TValue>)DictionaryOfAllTypesObjects,
                _ => throw new MissingMemberException($"The object does not have a Realm dictionary property with name {propertyName}"),
            };
        }
    }
}