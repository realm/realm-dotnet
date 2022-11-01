﻿// <auto-generated />
using Realms;
using Realms.Schema;
using Realms.Tests.Database;
using Realms.Tests.Database.Generated;
using Realms.Weaving;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Realms.Tests.Database
{
    [Generated]
    [Woven(typeof(SerializedObjectObjectHelper))]
    public partial class SerializedObject : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static ObjectSchema RealmSchema = new ObjectSchema.Builder("SerializedObject", ObjectSchema.ObjectType.RealmObject)
        {
            Property.Primitive("IntValue", RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "IntValue"),
            Property.Primitive("Name", RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "Name"),
            Property.PrimitiveDictionary("Dict", RealmValueType.Int, areElementsNullable: false, managedName: "Dict"),
            Property.PrimitiveList("List", RealmValueType.String, areElementsNullable: true, managedName: "List"),
            Property.PrimitiveSet("Set", RealmValueType.String, areElementsNullable: true, managedName: "Set"),
        }.Build();

        #region IRealmObject implementation

        private ISerializedObjectAccessor _accessor;

        IRealmAccessor IRealmObjectBase.Accessor => Accessor;

        internal ISerializedObjectAccessor Accessor => _accessor ?? (_accessor = new SerializedObjectUnmanagedAccessor(typeof(SerializedObject)));

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
            var newAccessor = (ISerializedObjectAccessor)managedAccessor;
            var oldAccessor = (ISerializedObjectAccessor)_accessor;
            _accessor = newAccessor;

            if (helper != null)
            {
                if (!skipDefaults)
                {
                    newAccessor.Dict.Clear();
                    newAccessor.List.Clear();
                    newAccessor.Set.Clear();
                }

                if(!skipDefaults || oldAccessor.IntValue != default(int))
                {
                    newAccessor.IntValue = oldAccessor.IntValue;
                }
                if(!skipDefaults || oldAccessor.Name != default(string))
                {
                    newAccessor.Name = oldAccessor.Name;
                }
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.Dict, newAccessor.Dict, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.List, newAccessor.List, update, skipDefaults);
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.Set, newAccessor.Set, update, skipDefaults);
            }

            if (_propertyChanged != null)
            {
                SubscribeForNotifications();
            }

            OnManaged();
        }

        #endregion

        /// <summary>
        /// Called when the object has been managed by a Realm.
        /// </summary>
        /// <remarks>
        /// This method will be called either when a managed object is materialized or when an unmanaged object has been
        /// added to the Realm. It can be useful for providing some initialization logic as when the constructor is invoked,
        /// it is not yet clear whether the object is managed or not.
        /// </remarks>
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

        /// <summary>
        /// Called when a property has changed on this class.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <remarks>
        /// For this method to be called, you need to have first subscribed to <see cref="PropertyChanged"/>.
        /// This can be used to react to changes to the current object, e.g. raising <see cref="PropertyChanged"/> for computed properties.
        /// </remarks>
        /// <example>
        /// <code>
        /// class MyClass : IRealmObject
        /// {
        ///     public int StatusCodeRaw { get; set; }
        ///     public StatusCodeEnum StatusCode => (StatusCodeEnum)StatusCodeRaw;
        ///     partial void OnPropertyChanged(string propertyName)
        ///     {
        ///         if (propertyName == nameof(StatusCodeRaw))
        ///         {
        ///             RaisePropertyChanged(nameof(StatusCode));
        ///         }
        ///     }
        /// }
        /// </code>
        /// Here, we have a computed property that depends on a persisted one. In order to notify any <see cref="PropertyChanged"/>
        /// subscribers that <c>StatusCode</c> has changed, we implement <see cref="OnPropertyChanged"/> and
        /// raise <see cref="PropertyChanged"/> manually by calling <see cref="RaisePropertyChanged"/>.
        /// </example>
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

        public static explicit operator SerializedObject(RealmValue val) => val.AsRealmObject<SerializedObject>();

        public static implicit operator RealmValue(SerializedObject val) => RealmValue.Object(val);

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
        private class SerializedObjectObjectHelper : IRealmObjectHelper
        {
            public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public ManagedAccessor CreateAccessor() => new SerializedObjectManagedAccessor();

            public IRealmObjectBase CreateInstance() => new SerializedObject();

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
    internal interface ISerializedObjectAccessor : IRealmAccessor
    {
        int IntValue { get; set; }

        string Name { get; set; }

        IDictionary<string, int> Dict { get; }

        IList<string> List { get; }

        ISet<string> Set { get; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class SerializedObjectManagedAccessor : ManagedAccessor, ISerializedObjectAccessor
    {
        public int IntValue
        {
            get => (int)GetValue("IntValue");
            set => SetValue("IntValue", value);
        }

        public string Name
        {
            get => (string)GetValue("Name");
            set => SetValue("Name", value);
        }

        private IDictionary<string, int> _dict;
        public IDictionary<string, int> Dict
        {
            get
            {
                if (_dict == null)
                {
                    _dict = GetDictionaryValue<int>("Dict");
                }

                return _dict;
            }
        }

        private IList<string> _list;
        public IList<string> List
        {
            get
            {
                if (_list == null)
                {
                    _list = GetListValue<string>("List");
                }

                return _list;
            }
        }

        private ISet<string> _set;
        public ISet<string> Set
        {
            get
            {
                if (_set == null)
                {
                    _set = GetSetValue<string>("Set");
                }

                return _set;
            }
        }
    }

    internal class SerializedObjectUnmanagedAccessor : UnmanagedAccessor, ISerializedObjectAccessor
    {
        private int _intValue;
        public int IntValue
        {
            get => _intValue;
            set
            {
                _intValue = value;
                RaisePropertyChanged("IntValue");
            }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        public IDictionary<string, int> Dict { get; } = new Dictionary<string, int>();

        public IList<string> List { get; } = new List<string>();

        public ISet<string> Set { get; } = new HashSet<string>(RealmSet<string>.Comparer);

        public SerializedObjectUnmanagedAccessor(Type objectType) : base(objectType)
        {
        }

        public override RealmValue GetValue(string propertyName)
        {
            return propertyName switch
            {
                "IntValue" => _intValue,
                "Name" => _name,
                _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
            };
        }

        public override void SetValue(string propertyName, RealmValue val)
        {
            switch (propertyName)
            {
                case "IntValue":
                    IntValue = (int)val;
                    return;
                case "Name":
                    Name = (string)val;
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
            "List" => (IList<T>)List,

                            _ => throw new MissingMemberException($"The object does not have a Realm list property with name {propertyName}"),
                        };
        }

        public override ISet<T> GetSetValue<T>(string propertyName)
        {
            return propertyName switch
                        {
            "Set" => (ISet<T>)Set,

                            _ => throw new MissingMemberException($"The object does not have a Realm set property with name {propertyName}"),
                        };
        }

        public override IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
        {
            return propertyName switch
            {
                "Dict" => (IDictionary<string, TValue>)Dict,
                _ => throw new MissingMemberException($"The object does not have a Realm dictionary property with name {propertyName}"),
            };
        }
    }
}