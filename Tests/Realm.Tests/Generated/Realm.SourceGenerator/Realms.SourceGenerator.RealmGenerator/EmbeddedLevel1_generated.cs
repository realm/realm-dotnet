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
    [Woven(typeof(EmbeddedLevel1ObjectHelper))]
    public partial class EmbeddedLevel1 : IEmbeddedObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("EmbeddedLevel1", ObjectSchema.ObjectType.EmbeddedObject)
        {
            Property.Primitive("String", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "String"),
            Property.Object("Child", "EmbeddedLevel2", managedName: "Child"),
            Property.ObjectList("Children", "EmbeddedLevel2", managedName: "Children"),
        }.Build();

        #region IEmbeddedObject implementation

        private IEmbeddedLevel1Accessor _accessor;

        IRealmAccessor IRealmObjectBase.Accessor => Accessor;

        internal IEmbeddedLevel1Accessor Accessor => _accessor = _accessor ?? new EmbeddedLevel1UnmanagedAccessor(typeof(EmbeddedLevel1));

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
            var newAccessor = (IEmbeddedLevel1Accessor)managedAccessor;
            var oldAccessor = _accessor as IEmbeddedLevel1Accessor;
            _accessor = newAccessor;

            if (helper != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.Children.Clear();
                }

                if(!skipDefaults || oldAccessor.String != default(string))
                {
                    newAccessor.String = oldAccessor.String;
                }
                newAccessor.Child = oldAccessor.Child;
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.Children, newAccessor.Children, update, skipDefaults);
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

        public static explicit operator EmbeddedLevel1(RealmValue val) => val.AsRealmObject<EmbeddedLevel1>();

        public static implicit operator RealmValue(EmbeddedLevel1 val) => RealmValue.Object(val);

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
        private class EmbeddedLevel1ObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public ManagedAccessor CreateAccessor() => new EmbeddedLevel1ManagedAccessor();

            public IRealmObjectBase CreateInstance() => new EmbeddedLevel1();

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
    internal interface IEmbeddedLevel1Accessor : IRealmAccessor
    {
        string String { get; set; }

        EmbeddedLevel2 Child { get; set; }

        IList<EmbeddedLevel2> Children { get; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class EmbeddedLevel1ManagedAccessor : ManagedAccessor, IEmbeddedLevel1Accessor
    {
        public string String
        {
            get => (string)GetValue("String");
            set => SetValue("String", value);
        }

        public EmbeddedLevel2 Child
        {
            get => (EmbeddedLevel2)GetValue("Child");
            set => SetValue("Child", value);
        }

        private IList<EmbeddedLevel2> _children;
        public IList<EmbeddedLevel2> Children
        {
            get
            {
                if (_children == null)
                {
                    _children = GetListValue<EmbeddedLevel2>("Children");
                }

                return _children;
            }
        }
    }

    internal class EmbeddedLevel1UnmanagedAccessor : UnmanagedAccessor, IEmbeddedLevel1Accessor
    {
        private string _string;
        public string String
        {
            get => _string;
            set
            {
                _string = value;
                RaisePropertyChanged("String");
            }
        }

        private EmbeddedLevel2 _child;
        public EmbeddedLevel2 Child
        {
            get => _child;
            set
            {
                _child = value;
                RaisePropertyChanged("Child");
            }
        }

        public IList<EmbeddedLevel2> Children { get; } = new List<EmbeddedLevel2>();

        public EmbeddedLevel1UnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "String" => _string,
                "Child" => _child,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }

        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "String":
                    String = (string)val;
                    return;
                case "Child":
                    Child = (EmbeddedLevel2)val;
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
            return propertyName switch
                        {
            "Children" => (IList<T>)Children,

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