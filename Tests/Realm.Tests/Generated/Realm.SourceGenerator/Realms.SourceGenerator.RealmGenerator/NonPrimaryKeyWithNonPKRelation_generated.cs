﻿// <auto-generated />
#nullable enable

using NUnit.Framework;
using Realms;
using Realms.Exceptions;
using Realms.Schema;
using Realms.Tests.Database;
using Realms.Weaving;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TestRealmObject = Realms.IRealmObject;

namespace Realms.Tests.Database
{
    public partial class AddOrUpdateTests
    {
        [Generated]
        [Woven(typeof(NonPrimaryKeyWithNonPKRelationObjectHelper)), Preserve(AllMembers = true)]
        public partial class NonPrimaryKeyWithNonPKRelation : IRealmObject, INotifyPropertyChanged, IReflectableType
        {
            public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("NonPrimaryKeyWithNonPKRelation", ObjectSchema.ObjectType.RealmObject)
            {
                Realms.Schema.Property.Primitive("StringValue", Realms.RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "StringValue"),
                Realms.Schema.Property.Object("OtherObject", "NonPrimaryKeyObject", managedName: "OtherObject"),
            }.Build();

            #region IRealmObject implementation

            private INonPrimaryKeyWithNonPKRelationAccessor? _accessor;

            Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

            internal INonPrimaryKeyWithNonPKRelationAccessor Accessor => _accessor ??= new NonPrimaryKeyWithNonPKRelationUnmanagedAccessor(typeof(NonPrimaryKeyWithNonPKRelation));

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
                var newAccessor = (INonPrimaryKeyWithNonPKRelationAccessor)managedAccessor;
                var oldAccessor = _accessor;
                _accessor = newAccessor;

                if (helper != null && oldAccessor != null)
                {
                    if (!skipDefaults || oldAccessor.StringValue != default(string))
                    {
                        newAccessor.StringValue = oldAccessor.StringValue;
                    }
                    if (oldAccessor.OtherObject != null && newAccessor.Realm != null)
                    {
                        newAccessor.Realm.Add(oldAccessor.OtherObject, update);
                    }
                    newAccessor.OtherObject = oldAccessor.OtherObject;
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

            public static explicit operator NonPrimaryKeyWithNonPKRelation?(Realms.RealmValue val) => val.Type == Realms.RealmValueType.Null ? null : val.AsRealmObject<NonPrimaryKeyWithNonPKRelation>();

            public static implicit operator Realms.RealmValue(NonPrimaryKeyWithNonPKRelation? val) => val == null ? Realms.RealmValue.Null : Realms.RealmValue.Object(val);

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

            [EditorBrowsable(EditorBrowsableState.Never), Preserve(AllMembers = true)]
            private class NonPrimaryKeyWithNonPKRelationObjectHelper : Realms.Weaving.IRealmObjectHelper
            {
                public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
                {
                    throw new InvalidOperationException("This method should not be called for source generated classes.");
                }

                public Realms.ManagedAccessor CreateAccessor() => new NonPrimaryKeyWithNonPKRelationManagedAccessor();

                public Realms.IRealmObjectBase CreateInstance() => new NonPrimaryKeyWithNonPKRelation();

                public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out RealmValue value)
                {
                    value = RealmValue.Null;
                    return false;
                }
            }

            [EditorBrowsable(EditorBrowsableState.Never), Preserve(AllMembers = true)]
            internal interface INonPrimaryKeyWithNonPKRelationAccessor : Realms.IRealmAccessor
            {
                string? StringValue { get; set; }

                Realms.Tests.Database.AddOrUpdateTests.NonPrimaryKeyObject? OtherObject { get; set; }
            }

            [EditorBrowsable(EditorBrowsableState.Never), Preserve(AllMembers = true)]
            internal class NonPrimaryKeyWithNonPKRelationManagedAccessor : Realms.ManagedAccessor, INonPrimaryKeyWithNonPKRelationAccessor
            {
                public string? StringValue
                {
                    get => (string?)GetValue("StringValue");
                    set => SetValue("StringValue", value);
                }

                public Realms.Tests.Database.AddOrUpdateTests.NonPrimaryKeyObject? OtherObject
                {
                    get => (Realms.Tests.Database.AddOrUpdateTests.NonPrimaryKeyObject?)GetValue("OtherObject");
                    set => SetValue("OtherObject", value);
                }
            }

            [EditorBrowsable(EditorBrowsableState.Never), Preserve(AllMembers = true)]
            internal class NonPrimaryKeyWithNonPKRelationUnmanagedAccessor : Realms.UnmanagedAccessor, INonPrimaryKeyWithNonPKRelationAccessor
            {
                public override ObjectSchema ObjectSchema => NonPrimaryKeyWithNonPKRelation.RealmSchema;

                private string? _stringValue;
                public string? StringValue
                {
                    get => _stringValue;
                    set
                    {
                        _stringValue = value;
                        RaisePropertyChanged("StringValue");
                    }
                }

                private Realms.Tests.Database.AddOrUpdateTests.NonPrimaryKeyObject? _otherObject;
                public Realms.Tests.Database.AddOrUpdateTests.NonPrimaryKeyObject? OtherObject
                {
                    get => _otherObject;
                    set
                    {
                        _otherObject = value;
                        RaisePropertyChanged("OtherObject");
                    }
                }

                public NonPrimaryKeyWithNonPKRelationUnmanagedAccessor(Type objectType) : base(objectType)
                {
                }

                public override Realms.RealmValue GetValue(string propertyName)
                {
                    return propertyName switch
                    {
                        "StringValue" => _stringValue,
                        "OtherObject" => _otherObject,
                        _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
                    };
                }

                public override void SetValue(string propertyName, Realms.RealmValue val)
                {
                    switch (propertyName)
                    {
                        case "StringValue":
                            StringValue = (string?)val;
                            return;
                        case "OtherObject":
                            OtherObject = (Realms.Tests.Database.AddOrUpdateTests.NonPrimaryKeyObject?)val;
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
}
