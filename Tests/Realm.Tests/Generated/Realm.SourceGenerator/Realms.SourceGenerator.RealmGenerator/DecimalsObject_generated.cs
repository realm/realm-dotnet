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
    [Woven(typeof(DecimalsObjectObjectHelper)), Realms.Preserve(AllMembers = true)]
    public partial class DecimalsObject : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("DecimalsObject", ObjectSchema.ObjectType.RealmObject)
        {
            Realms.Schema.Property.Primitive("DecimalValue", Realms.RealmValueType.Decimal128, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "DecimalValue"),
            Realms.Schema.Property.Primitive("Decimal128Value", Realms.RealmValueType.Decimal128, isPrimaryKey: false, isIndexed: false, isNullable: false, managedName: "Decimal128Value"),
        }.Build();

        #region IRealmObject implementation

        private IDecimalsObjectAccessor? _accessor;

        Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

        internal IDecimalsObjectAccessor Accessor => _accessor ??= new DecimalsObjectUnmanagedAccessor(typeof(DecimalsObject));

        [IgnoreDataMember, XmlIgnore]
        public bool IsManaged => Accessor.IsManaged;

        [IgnoreDataMember, XmlIgnore]
        public bool IsValid => Accessor.IsValid;

        [IgnoreDataMember, XmlIgnore]
        public bool IsFrozen => Accessor.IsFrozen;

        [IgnoreDataMember, XmlIgnore]
        public Realms.Realm? Realm => Accessor.Realm;

        [IgnoreDataMember, XmlIgnore]
        public Realms.Schema.ObjectSchema ObjectSchema => Accessor.ObjectSchema!;

        [IgnoreDataMember, XmlIgnore]
        public Realms.DynamicObjectApi DynamicApi => Accessor.DynamicApi;

        [IgnoreDataMember, XmlIgnore]
        public int BacklinksCount => Accessor.BacklinksCount;

        void ISettableManagedAccessor.SetManagedAccessor(Realms.IRealmAccessor managedAccessor, Realms.Weaving.IRealmObjectHelper? helper, bool update, bool skipDefaults)
        {
            var newAccessor = (IDecimalsObjectAccessor)managedAccessor;
            var oldAccessor = _accessor;
            _accessor = newAccessor;

            if (helper != null && oldAccessor != null)
            {
                newAccessor.DecimalValue = oldAccessor.DecimalValue;
                newAccessor.Decimal128Value = oldAccessor.Decimal128Value;
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

        public static explicit operator DecimalsObject?(Realms.RealmValue val) => val.Type == Realms.RealmValueType.Null ? null : val.AsRealmObject<DecimalsObject>();

        public static implicit operator Realms.RealmValue(DecimalsObject? val) => val == null ? Realms.RealmValue.Null : Realms.RealmValue.Object(val);

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

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class DecimalsObjectObjectHelper : Realms.Weaving.IRealmObjectHelper
        {
            public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public Realms.ManagedAccessor CreateAccessor() => new DecimalsObjectManagedAccessor();

            public Realms.IRealmObjectBase CreateInstance() => new DecimalsObject();

            public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out RealmValue value)
            {
                value = RealmValue.Null;
                return false;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        internal interface IDecimalsObjectAccessor : Realms.IRealmAccessor
        {
            decimal DecimalValue { get; set; }

            MongoDB.Bson.Decimal128 Decimal128Value { get; set; }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        internal class DecimalsObjectManagedAccessor : Realms.ManagedAccessor, IDecimalsObjectAccessor
        {
            public decimal DecimalValue
            {
                get => (decimal)GetValue("DecimalValue");
                set => SetValue("DecimalValue", value);
            }

            public MongoDB.Bson.Decimal128 Decimal128Value
            {
                get => (MongoDB.Bson.Decimal128)GetValue("Decimal128Value");
                set => SetValue("Decimal128Value", value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        internal class DecimalsObjectUnmanagedAccessor : Realms.UnmanagedAccessor, IDecimalsObjectAccessor
        {
            public override ObjectSchema ObjectSchema => DecimalsObject.RealmSchema;

            private decimal _decimalValue;
            public decimal DecimalValue
            {
                get => _decimalValue;
                set
                {
                    _decimalValue = value;
                    RaisePropertyChanged("DecimalValue");
                }
            }

            private MongoDB.Bson.Decimal128 _decimal128Value;
            public MongoDB.Bson.Decimal128 Decimal128Value
            {
                get => _decimal128Value;
                set
                {
                    _decimal128Value = value;
                    RaisePropertyChanged("Decimal128Value");
                }
            }

            public DecimalsObjectUnmanagedAccessor(Type objectType) : base(objectType)
            {
            }

            public override Realms.RealmValue GetValue(string propertyName)
            {
                return propertyName switch
                {
                    "DecimalValue" => _decimalValue,
                    "Decimal128Value" => _decimal128Value,
                    _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
                };
            }

            public override void SetValue(string propertyName, Realms.RealmValue val)
            {
                switch (propertyName)
                {
                    case "DecimalValue":
                        DecimalValue = (decimal)val;
                        return;
                    case "Decimal128Value":
                        Decimal128Value = (MongoDB.Bson.Decimal128)val;
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
