﻿// <auto-generated />
using SourceGeneratorAssemblyToProcess.Generated;
using SourceGeneratorAssemblyToProcess;
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
using OtherNamespace;

namespace SourceGeneratorAssemblyToProcess
{
    [Generated]
    [Woven(typeof(NamespaceObjObjectHelper))]
    public partial class NamespaceObj : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("NamespaceObj", ObjectSchema.ObjectType.RealmObject)
        {
            Property.Primitive("Id", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "Id"),
            Property.Object("OtherNamespaceObj", "OtherNamespaceObj", managedName: "OtherNamespaceObj"),
        }.Build();

        ~NamespaceObj()
        {
            UnsubscribeFromNotifications();
        }

        #region IRealmObject implementation

        private INamespaceObjAccessor _accessor;

        IRealmAccessor IRealmObjectBase.Accessor => Accessor;

        internal INamespaceObjAccessor Accessor => _accessor = _accessor ?? new NamespaceObjUnmanagedAccessor(typeof(NamespaceObj));

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
            var newAccessor = (INamespaceObjAccessor)managedAccessor;
            var oldAccessor = _accessor as INamespaceObjAccessor;
            _accessor = newAccessor;

            if (helper != null)
            {

                if(!skipDefaults || oldAccessor.Id != default(int))
                {
                    newAccessor.Id = oldAccessor.Id;
                }
                if(oldAccessor.OtherNamespaceObj != null)
                {
                    newAccessor.Realm.Add(oldAccessor.OtherNamespaceObj, update);
                }
                newAccessor.OtherNamespaceObj = oldAccessor.OtherNamespaceObj;
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

        public static explicit operator NamespaceObj(RealmValue val) => val.AsRealmObject<NamespaceObj>();

        public static implicit operator RealmValue(NamespaceObj val) => RealmValue.Object(val);

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
        private class NamespaceObjObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public ManagedAccessor CreateAccessor() => new NamespaceObjManagedAccessor();

            public IRealmObjectBase CreateInstance() => new NamespaceObj();

            public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
            {
                value = null;
                return false;
            }
        }
    }
}

namespace SourceGeneratorAssemblyToProcess.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface INamespaceObjAccessor : IRealmAccessor
    {
        int Id { get; set; }

        OtherNamespaceObj OtherNamespaceObj { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class NamespaceObjManagedAccessor : ManagedAccessor, INamespaceObjAccessor
    {
        public int Id
        {
            get => (int)GetValue("Id");
            set => SetValue("Id", value);
        }

        public OtherNamespaceObj OtherNamespaceObj
        {
            get => (OtherNamespaceObj)GetValue("OtherNamespaceObj");
            set => SetValue("OtherNamespaceObj", value);
        }
    }

    internal class NamespaceObjUnmanagedAccessor : UnmanagedAccessor, INamespaceObjAccessor
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

        private OtherNamespaceObj _otherNamespaceObj;
        public OtherNamespaceObj OtherNamespaceObj
        {
            get => _otherNamespaceObj;
            set
            {
                _otherNamespaceObj = value;
                RaisePropertyChanged("OtherNamespaceObj");
            }
        }

        public NamespaceObjUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "Id" => _id,
                "OtherNamespaceObj" => _otherNamespaceObj,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }

        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "Id":
                    Id = (int)val;
                    return;
                case "OtherNamespaceObj":
                    OtherNamespaceObj = (OtherNamespaceObj)val;
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