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
    [Woven(typeof(EmbeddedLevel2ObjectHelper))]
    public partial class EmbeddedLevel2 : IEmbeddedObject, INotifyPropertyChanged, IReflectableType
    {
        public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("EmbeddedLevel2", ObjectSchema.ObjectType.EmbeddedObject)
        {
            Realms.Schema.Property.Primitive("String", Realms.RealmValueType.String, isPrimaryKey: false, isIndexed: false, isNullable: true, managedName: "String"),
            Realms.Schema.Property.Object("Child", "EmbeddedLevel3", managedName: "Child"),
            Realms.Schema.Property.ObjectList("Children", "EmbeddedLevel3", managedName: "Children"),
        }.Build();

        #region IEmbeddedObject implementation

        private IEmbeddedLevel2Accessor _accessor = null!;

        Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

        internal IEmbeddedLevel2Accessor Accessor => _accessor ??= new EmbeddedLevel2UnmanagedAccessor(typeof(EmbeddedLevel2));

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

        [IgnoreDataMember, XmlIgnore]
        public Realms.IRealmObjectBase Parent => Accessor.GetParent();

        public void SetManagedAccessor(Realms.IRealmAccessor managedAccessor, Realms.Weaving.IRealmObjectHelper? helper = null, bool update = false, bool skipDefaults = false)
        {
            var newAccessor = (IEmbeddedLevel2Accessor)managedAccessor;
            var oldAccessor = _accessor;
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
                newAccessor.Child = oldAccessor.Child!;
                Realms.CollectionExtensions.PopulateCollection(oldAccessor.Children, newAccessor.Children, update, skipDefaults);
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

        private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
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

        public static explicit operator EmbeddedLevel2(Realms.RealmValue val) => val.AsRealmObject<EmbeddedLevel2>();

        public static implicit operator Realms.RealmValue(EmbeddedLevel2? val) => Realms.RealmValue.Object(val);

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
        private class EmbeddedLevel2ObjectHelper : Realms.Weaving.IRealmObjectHelper
        {
            public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public Realms.ManagedAccessor CreateAccessor() => new EmbeddedLevel2ManagedAccessor();

            public Realms.IRealmObjectBase CreateInstance() => new EmbeddedLevel2();

            public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out object? value)
            {
                value = null;
                return false;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal interface IEmbeddedLevel2Accessor : Realms.IRealmAccessor
        {
            string String { get; set; }

            Realms.Tests.EmbeddedLevel3 Child { get; set; }

            System.Collections.Generic.IList<Realms.Tests.EmbeddedLevel3> Children { get; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class EmbeddedLevel2ManagedAccessor : Realms.ManagedAccessor, IEmbeddedLevel2Accessor
        {
            public string String
            {
                get => (string)GetValue("String");
                set => SetValue("String", value);
            }

            public Realms.Tests.EmbeddedLevel3 Child
            {
                get => (Realms.Tests.EmbeddedLevel3)GetValue("Child");
                set => SetValue("Child", value);
            }

            private System.Collections.Generic.IList<Realms.Tests.EmbeddedLevel3> _children = null!;
            public System.Collections.Generic.IList<Realms.Tests.EmbeddedLevel3> Children
            {
                get
                {
                    if (_children == null)
                    {
                        _children = GetListValue<Realms.Tests.EmbeddedLevel3>("Children");
                    }

                    return _children;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class EmbeddedLevel2UnmanagedAccessor : Realms.UnmanagedAccessor, IEmbeddedLevel2Accessor
        {
            public override ObjectSchema ObjectSchema => EmbeddedLevel2.RealmSchema;

            private string _string = null!;
            public string String
            {
                get => _string;
                set
                {
                    _string = value;
                    RaisePropertyChanged("String");
                }
            }

            private Realms.Tests.EmbeddedLevel3 _child = null!;
            public Realms.Tests.EmbeddedLevel3 Child
            {
                get => _child;
                set
                {
                    _child = value;
                    RaisePropertyChanged("Child");
                }
            }

            public System.Collections.Generic.IList<Realms.Tests.EmbeddedLevel3> Children { get; } = new List<Realms.Tests.EmbeddedLevel3>();

            public EmbeddedLevel2UnmanagedAccessor(Type objectType) : base(objectType)
            {
            }

            public override Realms.RealmValue GetValue(string propertyName)
            {
                return propertyName switch
                {
                    "String" => _string,
                    "Child" => _child,
                    _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
                };
            }

            public override void SetValue(string propertyName, Realms.RealmValue val)
            {
                switch (propertyName)
                {
                    case "String":
                        String = (string)val;
                        return;
                    case "Child":
                        Child = (Realms.Tests.EmbeddedLevel3)val;
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
}
