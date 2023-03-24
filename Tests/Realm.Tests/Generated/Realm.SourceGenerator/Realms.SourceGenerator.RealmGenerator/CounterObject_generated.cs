﻿// <auto-generated />
#nullable enable

using MongoDB.Bson;
using Realms;
using Realms.Schema;
using Realms.Tests;
using Realms.Tests.Database;
using Realms.Weaving;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TestAsymmetricObject = Realms.IAsymmetricObject;
using TestEmbeddedObject = Realms.IEmbeddedObject;
using TestRealmObject = Realms.IRealmObject;

namespace Realms.Tests
{
    [Generated]
    [Woven(typeof(CounterObjectObjectHelper))]
    public partial class CounterObject : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("CounterObject", ObjectSchema.ObjectType.RealmObject)
        {
            Realms.Schema.Property.Primitive("_id", Realms.RealmValueType.Int, isPrimaryKey: true, isIndexed: false, isNullable: false, managedName: "Id"),
            Realms.Schema.Property.Primitive("ByteProperty", Realms.RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "ByteProperty"),
            Realms.Schema.Property.Primitive("Int16Property", Realms.RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "Int16Property"),
            Realms.Schema.Property.Primitive("Int32Property", Realms.RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "Int32Property"),
            Realms.Schema.Property.Primitive("Int64Property", Realms.RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "Int64Property"),
            Realms.Schema.Property.Primitive("NullableByteProperty", Realms.RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "NullableByteProperty"),
            Realms.Schema.Property.Primitive("NullableInt16Property", Realms.RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "NullableInt16Property"),
            Realms.Schema.Property.Primitive("NullableInt32Property", Realms.RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "NullableInt32Property"),
            Realms.Schema.Property.Primitive("NullableInt64Property", Realms.RealmValueType.Int, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "NullableInt64Property"),
        }.Build();

        #region IRealmObject implementation

        private ICounterObjectAccessor? _accessor;

        Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

        internal ICounterObjectAccessor Accessor => _accessor ??= new CounterObjectUnmanagedAccessor(typeof(CounterObject));

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
            var newAccessor = (ICounterObjectAccessor)managedAccessor;
            var oldAccessor = _accessor;
            _accessor = newAccessor;

            if (helper != null && oldAccessor != null)
            {
                if(!skipDefaults || oldAccessor.Id != default(int))
                {
                    newAccessor.Id = oldAccessor.Id;
                }
                newAccessor.ByteProperty = oldAccessor.ByteProperty;
                newAccessor.Int16Property = oldAccessor.Int16Property;
                newAccessor.Int32Property = oldAccessor.Int32Property;
                newAccessor.Int64Property = oldAccessor.Int64Property;
                newAccessor.NullableByteProperty = oldAccessor.NullableByteProperty;
                newAccessor.NullableInt16Property = oldAccessor.NullableInt16Property;
                newAccessor.NullableInt32Property = oldAccessor.NullableInt32Property;
                newAccessor.NullableInt64Property = oldAccessor.NullableInt64Property;
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

        public static explicit operator CounterObject(Realms.RealmValue val) => val.AsRealmObject<CounterObject>();

        public static implicit operator Realms.RealmValue(CounterObject? val) => val == null ? Realms.RealmValue.Null : Realms.RealmValue.Object(val);

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

        [EditorBrowsable(EditorBrowsableState.Never)]
        private class CounterObjectObjectHelper : Realms.Weaving.IRealmObjectHelper
        {
            public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public Realms.ManagedAccessor CreateAccessor() => new CounterObjectManagedAccessor();

            public Realms.IRealmObjectBase CreateInstance() => new CounterObject();

            public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out RealmValue value)
            {
                value = ((ICounterObjectAccessor)instance.Accessor).Id;
                return true;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal interface ICounterObjectAccessor : Realms.IRealmAccessor
        {
            int Id { get; set; }

            Realms.RealmInteger<byte> ByteProperty { get; set; }

            Realms.RealmInteger<short> Int16Property { get; set; }

            Realms.RealmInteger<int> Int32Property { get; set; }

            Realms.RealmInteger<long> Int64Property { get; set; }

            Realms.RealmInteger<byte>? NullableByteProperty { get; set; }

            Realms.RealmInteger<short>? NullableInt16Property { get; set; }

            Realms.RealmInteger<int>? NullableInt32Property { get; set; }

            Realms.RealmInteger<long>? NullableInt64Property { get; set; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class CounterObjectManagedAccessor : Realms.ManagedAccessor, ICounterObjectAccessor
        {
            public int Id
            {
                get => (int)GetValue("_id");
                set => SetValueUnique("_id", value);
            }

            public Realms.RealmInteger<byte> ByteProperty
            {
                get => (Realms.RealmInteger<byte>)GetValue("ByteProperty");
                set => SetValue("ByteProperty", value);
            }

            public Realms.RealmInteger<short> Int16Property
            {
                get => (Realms.RealmInteger<short>)GetValue("Int16Property");
                set => SetValue("Int16Property", value);
            }

            public Realms.RealmInteger<int> Int32Property
            {
                get => (Realms.RealmInteger<int>)GetValue("Int32Property");
                set => SetValue("Int32Property", value);
            }

            public Realms.RealmInteger<long> Int64Property
            {
                get => (Realms.RealmInteger<long>)GetValue("Int64Property");
                set => SetValue("Int64Property", value);
            }

            public Realms.RealmInteger<byte>? NullableByteProperty
            {
                get => (Realms.RealmInteger<byte>?)GetValue("NullableByteProperty");
                set => SetValue("NullableByteProperty", value);
            }

            public Realms.RealmInteger<short>? NullableInt16Property
            {
                get => (Realms.RealmInteger<short>?)GetValue("NullableInt16Property");
                set => SetValue("NullableInt16Property", value);
            }

            public Realms.RealmInteger<int>? NullableInt32Property
            {
                get => (Realms.RealmInteger<int>?)GetValue("NullableInt32Property");
                set => SetValue("NullableInt32Property", value);
            }

            public Realms.RealmInteger<long>? NullableInt64Property
            {
                get => (Realms.RealmInteger<long>?)GetValue("NullableInt64Property");
                set => SetValue("NullableInt64Property", value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class CounterObjectUnmanagedAccessor : Realms.UnmanagedAccessor, ICounterObjectAccessor
        {
            public override ObjectSchema ObjectSchema => CounterObject.RealmSchema;

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

            private Realms.RealmInteger<byte> _byteProperty;
            public Realms.RealmInteger<byte> ByteProperty
            {
                get => _byteProperty;
                set
                {
                    _byteProperty = value;
                    RaisePropertyChanged("ByteProperty");
                }
            }

            private Realms.RealmInteger<short> _int16Property;
            public Realms.RealmInteger<short> Int16Property
            {
                get => _int16Property;
                set
                {
                    _int16Property = value;
                    RaisePropertyChanged("Int16Property");
                }
            }

            private Realms.RealmInteger<int> _int32Property;
            public Realms.RealmInteger<int> Int32Property
            {
                get => _int32Property;
                set
                {
                    _int32Property = value;
                    RaisePropertyChanged("Int32Property");
                }
            }

            private Realms.RealmInteger<long> _int64Property;
            public Realms.RealmInteger<long> Int64Property
            {
                get => _int64Property;
                set
                {
                    _int64Property = value;
                    RaisePropertyChanged("Int64Property");
                }
            }

            private Realms.RealmInteger<byte>? _nullableByteProperty;
            public Realms.RealmInteger<byte>? NullableByteProperty
            {
                get => _nullableByteProperty;
                set
                {
                    _nullableByteProperty = value;
                    RaisePropertyChanged("NullableByteProperty");
                }
            }

            private Realms.RealmInteger<short>? _nullableInt16Property;
            public Realms.RealmInteger<short>? NullableInt16Property
            {
                get => _nullableInt16Property;
                set
                {
                    _nullableInt16Property = value;
                    RaisePropertyChanged("NullableInt16Property");
                }
            }

            private Realms.RealmInteger<int>? _nullableInt32Property;
            public Realms.RealmInteger<int>? NullableInt32Property
            {
                get => _nullableInt32Property;
                set
                {
                    _nullableInt32Property = value;
                    RaisePropertyChanged("NullableInt32Property");
                }
            }

            private Realms.RealmInteger<long>? _nullableInt64Property;
            public Realms.RealmInteger<long>? NullableInt64Property
            {
                get => _nullableInt64Property;
                set
                {
                    _nullableInt64Property = value;
                    RaisePropertyChanged("NullableInt64Property");
                }
            }

            public CounterObjectUnmanagedAccessor(Type objectType) : base(objectType)
            {
            }

            public override Realms.RealmValue GetValue(string propertyName)
            {
                return propertyName switch
                {
                    "_id" => _id,
                    "ByteProperty" => _byteProperty,
                    "Int16Property" => _int16Property,
                    "Int32Property" => _int32Property,
                    "Int64Property" => _int64Property,
                    "NullableByteProperty" => _nullableByteProperty,
                    "NullableInt16Property" => _nullableInt16Property,
                    "NullableInt32Property" => _nullableInt32Property,
                    "NullableInt64Property" => _nullableInt64Property,
                    _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
                };
            }

            public override void SetValue(string propertyName, Realms.RealmValue val)
            {
                switch (propertyName)
                {
                    case "_id":
                        throw new InvalidOperationException("Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique");
                    case "ByteProperty":
                        ByteProperty = (Realms.RealmInteger<byte>)val;
                        return;
                    case "Int16Property":
                        Int16Property = (Realms.RealmInteger<short>)val;
                        return;
                    case "Int32Property":
                        Int32Property = (Realms.RealmInteger<int>)val;
                        return;
                    case "Int64Property":
                        Int64Property = (Realms.RealmInteger<long>)val;
                        return;
                    case "NullableByteProperty":
                        NullableByteProperty = (Realms.RealmInteger<byte>?)val;
                        return;
                    case "NullableInt16Property":
                        NullableInt16Property = (Realms.RealmInteger<short>?)val;
                        return;
                    case "NullableInt32Property":
                        NullableInt32Property = (Realms.RealmInteger<int>?)val;
                        return;
                    case "NullableInt64Property":
                        NullableInt64Property = (Realms.RealmInteger<long>?)val;
                        return;
                    default:
                        throw new MissingMemberException($"The object does not have a settable Realm property with name {propertyName}");
                }
            }

            public override void SetValueUnique(string propertyName, Realms.RealmValue val)
            {
                if (propertyName != "_id")
                {
                    throw new InvalidOperationException($"Cannot set the value of non primary key property ({propertyName}) with SetValueUnique");
                }

                Id = (int)val;
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
}
