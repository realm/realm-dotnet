﻿// <auto-generated />
#nullable enable

using MongoDB.Bson;
using NUnit.Framework;
using Realms;
using Realms.Exceptions;
using Realms.Schema;
using Realms.Tests.Database;
using Realms.Weaving;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using TestAsymmetricObject = Realms.IAsymmetricObject;
using TestEmbeddedObject = Realms.IEmbeddedObject;
using TestRealmObject = Realms.IRealmObject;

namespace Realms.Tests.Database
{
    [Generated]
    [Woven(typeof(SerializedObjectObjectHelper))]
    public partial class SerializedObject : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("SerializedObject", ObjectSchema.ObjectType.RealmObject)
        {
            Realms.Schema.Property.Primitive("IntValue", Realms.RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "IntValue"),
            Realms.Schema.Property.Primitive("Name", Realms.RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "Name"),
            Realms.Schema.Property.PrimitiveDictionary("Dict", Realms.RealmValueType.Int, areElementsNullable: false, managedName: "Dict"),
            Realms.Schema.Property.PrimitiveList("List", Realms.RealmValueType.String, areElementsNullable: true, managedName: "List"),
            Realms.Schema.Property.PrimitiveSet("Set", Realms.RealmValueType.String, areElementsNullable: true, managedName: "Set"),
        }.Build();

        #region IRealmObject implementation

        private ISerializedObjectAccessor? _accessor;

        Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

        internal ISerializedObjectAccessor Accessor => _accessor ??= new SerializedObjectUnmanagedAccessor(typeof(SerializedObject));

        [IgnoreDataMember, XmlIgnore]
        public bool IsManaged => Accessor.IsManaged;

        [IgnoreDataMember, XmlIgnore]
        public bool IsValid => Accessor.IsValid;

        [IgnoreDataMember, XmlIgnore]
        public bool IsFrozen => Accessor.IsFrozen;

        [IgnoreDataMember, XmlIgnore]
        public Realms.Realm Realm => Accessor.Realm;

        [IgnoreDataMember, XmlIgnore]
        public Realms.Schema.ObjectSchema ObjectSchema => Accessor.ObjectSchema;

        [IgnoreDataMember, XmlIgnore]
        public Realms.DynamicObjectApi DynamicApi => Accessor.DynamicApi;

        [IgnoreDataMember, XmlIgnore]
        public int BacklinksCount => Accessor.BacklinksCount;

        public void SetManagedAccessor(Realms.IRealmAccessor managedAccessor, Realms.Weaving.IRealmObjectHelper? helper = null, bool update = false, bool skipDefaults = false)
        {
            var newAccessor = (ISerializedObjectAccessor)managedAccessor;
            var oldAccessor = _accessor;
            _accessor = newAccessor;

            if (helper != null && oldAccessor != null)
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

        private event PropertyChangedEventHandler? _propertyChanged;

        public event PropertyChangedEventHandler? PropertyChanged
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
        partial void OnPropertyChanged(string? propertyName);

        private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
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

        public static explicit operator SerializedObject(Realms.RealmValue val) => val.AsRealmObject<SerializedObject>();

        public static implicit operator Realms.RealmValue(SerializedObject? val) => val == null ? Realms.RealmValue.Null : Realms.RealmValue.Object(val);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public TypeInfo GetTypeInfo() => Accessor.GetTypeInfo(this);

        public override bool Equals(object? obj)
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

            if (obj is not Realms.IRealmObjectBase iro)
            {
                return false;
            }

            return Accessor.Equals(iro.Accessor);
        }

        public override int GetHashCode() => IsManaged ? Accessor.GetHashCode() : base.GetHashCode();

        public override string? ToString() => Accessor.ToString();

        [EditorBrowsable(EditorBrowsableState.Never)]
        private class SerializedObjectObjectHelper : Realms.Weaving.IRealmObjectHelper
        {
            public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public Realms.ManagedAccessor CreateAccessor() => new SerializedObjectManagedAccessor();

            public Realms.IRealmObjectBase CreateInstance() => new SerializedObject();

            public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out object? value)
            {
                value = null;
                return false;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal interface ISerializedObjectAccessor : Realms.IRealmAccessor
        {
            int IntValue { get; set; }

            string Name { get; set; }

            System.Collections.Generic.IDictionary<string, int> Dict { get; }

            System.Collections.Generic.IList<string> List { get; }

            System.Collections.Generic.ISet<string> Set { get; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class SerializedObjectManagedAccessor : Realms.ManagedAccessor, ISerializedObjectAccessor
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

            private System.Collections.Generic.IDictionary<string, int> _dict = null!;
            public System.Collections.Generic.IDictionary<string, int> Dict
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

            private System.Collections.Generic.IList<string> _list = null!;
            public System.Collections.Generic.IList<string> List
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

            private System.Collections.Generic.ISet<string> _set = null!;
            public System.Collections.Generic.ISet<string> Set
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class SerializedObjectUnmanagedAccessor : Realms.UnmanagedAccessor, ISerializedObjectAccessor
        {
            public override ObjectSchema ObjectSchema => SerializedObject.RealmSchema;

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

            private string _name = null!;
            public string Name
            {
                get => _name;
                set
                {
                    _name = value;
                    RaisePropertyChanged("Name");
                }
            }

            public System.Collections.Generic.IDictionary<string, int> Dict { get; } = new Dictionary<string, int>();

            public System.Collections.Generic.IList<string> List { get; } = new List<string>();

            public System.Collections.Generic.ISet<string> Set { get; } = new HashSet<string>(RealmSet<string>.Comparer);

            public SerializedObjectUnmanagedAccessor(Type objectType) : base(objectType)
            {
            }

            public override Realms.RealmValue GetValue(string propertyName)
            {
                return propertyName switch
                {
                    "IntValue" => _intValue,
                    "Name" => _name,
                    _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
                };
            }

            public override void SetValue(string propertyName, Realms.RealmValue val)
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

            public override void SetValueUnique(string propertyName, Realms.RealmValue val)
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
}
