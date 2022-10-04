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
    [Woven(typeof(OnManagedTestClassObjectHelper))]
    public partial class OnManagedTestClass : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("OnManagedTestClass", ObjectSchema.ObjectType.RealmObject)
        {
            Property.Primitive("Id", RealmValueType.Int, isPrimaryKey: true, isIndexed: false, isNullable: false, managedName: "Id"),
            Property.Object("RelatedObject", "OnManagedTestClass", managedName: "RelatedObject"),
            Property.ObjectList("RelatedCollection", "OnManagedTestClass", managedName: "RelatedCollection"),
        }.Build();

        #region IRealmObject implementation

        private IOnManagedTestClassAccessor _accessor;

        IRealmAccessor IRealmObjectBase.Accessor => Accessor;

        internal IOnManagedTestClassAccessor Accessor => _accessor = _accessor ?? new OnManagedTestClassUnmanagedAccessor(typeof(OnManagedTestClass));

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
            var newAccessor = (IOnManagedTestClassAccessor)managedAccessor;
            var oldAccessor = _accessor as IOnManagedTestClassAccessor;
            _accessor = newAccessor;

            if (helper != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.RelatedCollection.Clear();
                }

                if(!skipDefaults || oldAccessor.Id != default(int))
                {
                    newAccessor.Id = oldAccessor.Id;
                }
                if(oldAccessor.RelatedObject != null)
                {
                    newAccessor.Realm.Add(oldAccessor.RelatedObject, update);
                }
                newAccessor.RelatedObject = oldAccessor.RelatedObject;
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.RelatedCollection, newAccessor.RelatedCollection, update, skipDefaults);
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

        public static explicit operator OnManagedTestClass(RealmValue val) => val.AsRealmObject<OnManagedTestClass>();

        public static implicit operator RealmValue(OnManagedTestClass val) => RealmValue.Object(val);

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
        private class OnManagedTestClassObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public ManagedAccessor CreateAccessor() => new OnManagedTestClassManagedAccessor();

            public IRealmObjectBase CreateInstance() => new OnManagedTestClass();

            public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
            {
                value = ((IOnManagedTestClassAccessor)instance.Accessor).Id;
                return true;
            }
        }
    }
}

namespace Realms.Tests.Database.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IOnManagedTestClassAccessor : IRealmAccessor
    {
        int Id { get; set; }

        OnManagedTestClass RelatedObject { get; set; }

        IList<OnManagedTestClass> RelatedCollection { get; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class OnManagedTestClassManagedAccessor : ManagedAccessor, IOnManagedTestClassAccessor
    {
        public int Id
        {
            get => (int)GetValue("Id");
            set => SetValueUnique("Id", value);
        }

        public OnManagedTestClass RelatedObject
        {
            get => (OnManagedTestClass)GetValue("RelatedObject");
            set => SetValue("RelatedObject", value);
        }

        private IList<OnManagedTestClass> _relatedCollection;
        public IList<OnManagedTestClass> RelatedCollection
        {
            get
            {
                if (_relatedCollection == null)
                {
                    _relatedCollection = GetListValue<OnManagedTestClass>("RelatedCollection");
                }

                return _relatedCollection;
            }
        }
    }

    internal class OnManagedTestClassUnmanagedAccessor : UnmanagedAccessor, IOnManagedTestClassAccessor
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

        private OnManagedTestClass _relatedObject;
        public OnManagedTestClass RelatedObject
        {
            get => _relatedObject;
            set
            {
                _relatedObject = value;
                RaisePropertyChanged("RelatedObject");
            }
        }

        public IList<OnManagedTestClass> RelatedCollection { get; } = new List<OnManagedTestClass>();

        public OnManagedTestClassUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "Id" => _id,
                "RelatedObject" => _relatedObject,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }

        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "Id":
                    throw new InvalidOperationException("Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique");
                case "RelatedObject":
                    RelatedObject = (OnManagedTestClass)val;
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

            Id = (int)val;
        }

        public override IList<T> GetListValue<T>(string propertyName)
        {
            return propertyName switch
                        {
            "RelatedCollection" => (IList<T>)RelatedCollection,

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