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
    [Woven(typeof(Level1ObjectHelper))]
    public partial class Level1 : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("Level1", ObjectSchema.ObjectType.RealmObject)
        {
            Property.Primitive("StringValue", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "StringValue"),
            Property.Object("Level2", "Level2", managedName: "Level2"),
        }.Build();

        #region IRealmObject implementation

        private ILevel1Accessor _accessor;

        IRealmAccessor IRealmObjectBase.Accessor => Accessor;

        internal ILevel1Accessor Accessor => _accessor = _accessor ?? new Level1UnmanagedAccessor(typeof(Level1));

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
            var newAccessor = (ILevel1Accessor)managedAccessor;
            var oldAccessor = _accessor as ILevel1Accessor;
            _accessor = newAccessor;

            if (helper != null)
            {

                if(!skipDefaults || oldAccessor.StringValue != default(string))
                {
                    newAccessor.StringValue = oldAccessor.StringValue;
                }
                if(oldAccessor.Level2 != null)
                {
                    newAccessor.Realm.Add(oldAccessor.Level2, update);
                }
                newAccessor.Level2 = oldAccessor.Level2;
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

        public static explicit operator Level1(RealmValue val) => val.AsRealmObject<Level1>();

        public static implicit operator RealmValue(Level1 val) => RealmValue.Object(val);

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
        private class Level1ObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public ManagedAccessor CreateAccessor() => new Level1ManagedAccessor();

            public IRealmObjectBase CreateInstance() => new Level1();

            public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
            {
                value = null;
                return false;
            }
        }
    }
}

namespace Realms.Tests.Database.Generated
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface ILevel1Accessor : IRealmAccessor
    {
        string StringValue { get; set; }

        Level2 Level2 { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class Level1ManagedAccessor : ManagedAccessor, ILevel1Accessor
    {
        public string StringValue
        {
            get => (string)GetValue("StringValue");
            set => SetValue("StringValue", value);
        }

        public Level2 Level2
        {
            get => (Level2)GetValue("Level2");
            set => SetValue("Level2", value);
        }
    }

    internal class Level1UnmanagedAccessor : UnmanagedAccessor, ILevel1Accessor
    {
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

        private Level2 _level2;
        public Level2 Level2
        {
            get => _level2;
            set
            {
                _level2 = value;
                RaisePropertyChanged("Level2");
            }
        }

        public Level1UnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "StringValue" => _stringValue,
                "Level2" => _level2,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }

        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "StringValue":
                    StringValue = (string)val;
                    return;
                case "Level2":
                    Level2 = (Level2)val;
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