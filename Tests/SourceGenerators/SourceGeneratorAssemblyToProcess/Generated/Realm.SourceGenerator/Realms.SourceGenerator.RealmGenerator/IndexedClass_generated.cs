﻿// <auto-generated />
#nullable enable

using Realms;
using Realms.Schema;
using Realms.Weaving;
using SourceGeneratorAssemblyToProcess;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace SourceGeneratorAssemblyToProcess
{
    [Generated]
    [Woven(typeof(IndexedClassObjectHelper)), Realms.Preserve(AllMembers = true)]
    internal partial class IndexedClass : IRealmObject, INotifyPropertyChanged, IReflectableType
    {
        /// <summary>
        /// Defines the schema for the <see cref="IndexedClass"/> class.
        /// </summary>
        public static Realms.Schema.ObjectSchema RealmSchema = new Realms.Schema.ObjectSchema.Builder("IndexedClass", ObjectSchema.ObjectType.RealmObject)
        {
            Realms.Schema.Property.Primitive("Id", Realms.RealmValueType.Int, isPrimaryKey: true, indexType: IndexType.None, isNullable: false, managedName: "Id"),
            Realms.Schema.Property.Primitive("FullTextProp", Realms.RealmValueType.String, isPrimaryKey: false, indexType: IndexType.FullText, isNullable: false, managedName: "FullTextProp"),
            Realms.Schema.Property.Primitive("NullableFullTextProp", Realms.RealmValueType.String, isPrimaryKey: false, indexType: IndexType.FullText, isNullable: true, managedName: "NullableFullTextProp"),
            Realms.Schema.Property.Primitive("IntProp", Realms.RealmValueType.Int, isPrimaryKey: false, indexType: IndexType.General, isNullable: false, managedName: "IntProp"),
            Realms.Schema.Property.Primitive("GuidProp", Realms.RealmValueType.Guid, isPrimaryKey: false, indexType: IndexType.General, isNullable: false, managedName: "GuidProp"),
            Realms.Schema.Property.Primitive("GeneralGuidProp", Realms.RealmValueType.Guid, isPrimaryKey: false, indexType: IndexType.General, isNullable: false, managedName: "GeneralGuidProp"),
        }.Build();

        #region IRealmObject implementation

        private IIndexedClassAccessor? _accessor;

        Realms.IRealmAccessor Realms.IRealmObjectBase.Accessor => Accessor;

        private IIndexedClassAccessor Accessor => _accessor ??= new IndexedClassUnmanagedAccessor(typeof(IndexedClass));

        /// <inheritdoc />
        [IgnoreDataMember, XmlIgnore]
        public bool IsManaged => Accessor.IsManaged;

        /// <inheritdoc />
        [IgnoreDataMember, XmlIgnore]
        public bool IsValid => Accessor.IsValid;

        /// <inheritdoc />
        [IgnoreDataMember, XmlIgnore]
        public bool IsFrozen => Accessor.IsFrozen;

        /// <inheritdoc />
        [IgnoreDataMember, XmlIgnore]
        public Realms.Realm? Realm => Accessor.Realm;

        /// <inheritdoc />
        [IgnoreDataMember, XmlIgnore]
        public Realms.Schema.ObjectSchema ObjectSchema => Accessor.ObjectSchema!;

        /// <inheritdoc />
        [IgnoreDataMember, XmlIgnore]
        public Realms.DynamicObjectApi DynamicApi => Accessor.DynamicApi;

        /// <inheritdoc />
        [IgnoreDataMember, XmlIgnore]
        public int BacklinksCount => Accessor.BacklinksCount;

        void ISettableManagedAccessor.SetManagedAccessor(Realms.IRealmAccessor managedAccessor, Realms.Weaving.IRealmObjectHelper? helper, bool update, bool skipDefaults)
        {
            var newAccessor = (IIndexedClassAccessor)managedAccessor;
            var oldAccessor = _accessor;
            _accessor = newAccessor;

            if (helper != null && oldAccessor != null)
            {
                if (!skipDefaults || oldAccessor.Id != default(int))
                {
                    newAccessor.Id = oldAccessor.Id;
                }
                newAccessor.FullTextProp = oldAccessor.FullTextProp;
                if (!skipDefaults || oldAccessor.NullableFullTextProp != default(string?))
                {
                    newAccessor.NullableFullTextProp = oldAccessor.NullableFullTextProp;
                }
                if (!skipDefaults || oldAccessor.IntProp != default(int))
                {
                    newAccessor.IntProp = oldAccessor.IntProp;
                }
                if (!skipDefaults || oldAccessor.GuidProp != default(System.Guid))
                {
                    newAccessor.GuidProp = oldAccessor.GuidProp;
                }
                if (!skipDefaults || oldAccessor.GeneralGuidProp != default(System.Guid))
                {
                    newAccessor.GeneralGuidProp = oldAccessor.GeneralGuidProp;
                }
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

        /// <inheritdoc />
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

        /// <summary>
        /// Converts a <see cref="Realms.RealmValue"/> to <see cref="IndexedClass"/>. Equivalent to <see cref="Realms.RealmValue.AsNullableRealmObject{T}"/>.
        /// </summary>
        /// <param name="val">The <see cref="Realms.RealmValue"/> to convert.</param>
        /// <returns>The <see cref="IndexedClass"/> stored in the <see cref="Realms.RealmValue"/>.</returns>
        public static explicit operator IndexedClass?(Realms.RealmValue val) => val.Type == Realms.RealmValueType.Null ? null : val.AsRealmObject<IndexedClass>();

        /// <summary>
        /// Implicitly constructs a <see cref="Realms.RealmValue"/> from <see cref="IndexedClass"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="Realms.RealmValue"/>.</param>
        /// <returns>A <see cref="Realms.RealmValue"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator Realms.RealmValue(IndexedClass? val) => val == null ? Realms.RealmValue.Null : Realms.RealmValue.Object(val);

        /// <summary>
        /// Implicitly constructs a <see cref="Realms.QueryArgument"/> from <see cref="IndexedClass"/>.
        /// </summary>
        /// <param name="val">The value to store in the <see cref="Realms.QueryArgument"/>.</param>
        /// <returns>A <see cref="Realms.QueryArgument"/> containing the supplied <paramref name="val"/>.</returns>
        public static implicit operator Realms.QueryArgument(IndexedClass? val) => (Realms.RealmValue)val;

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TypeInfo GetTypeInfo() => Accessor.GetTypeInfo(this);

        /// <inheritdoc />
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

            if (!(obj is Realms.IRealmObjectBase iro))
            {
                return false;
            }

            return Accessor.Equals(iro.Accessor);
        }

        /// <inheritdoc />
        public override int GetHashCode() => IsManaged ? Accessor.GetHashCode() : base.GetHashCode();

        /// <inheritdoc />
        public override string? ToString() => Accessor.ToString();

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        private class IndexedClassObjectHelper : Realms.Weaving.IRealmObjectHelper
        {
            public void CopyToRealm(Realms.IRealmObjectBase instance, bool update, bool skipDefaults)
            {
                throw new InvalidOperationException("This method should not be called for source generated classes.");
            }

            public Realms.ManagedAccessor CreateAccessor() => new IndexedClassManagedAccessor();

            public Realms.IRealmObjectBase CreateInstance() => new IndexedClass();

            public bool TryGetPrimaryKeyValue(Realms.IRealmObjectBase instance, out RealmValue value)
            {
                value = ((IIndexedClassAccessor)instance.Accessor).Id;
                return true;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        internal interface IIndexedClassAccessor : Realms.IRealmAccessor
        {
            int Id { get; set; }

            string FullTextProp { get; set; }

            string? NullableFullTextProp { get; set; }

            int IntProp { get; set; }

            System.Guid GuidProp { get; set; }

            System.Guid GeneralGuidProp { get; set; }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        internal class IndexedClassManagedAccessor : Realms.ManagedAccessor, IIndexedClassAccessor
        {
            public int Id
            {
                get => (int)GetValue("Id");
                set => SetValueUnique("Id", value);
            }

            public string FullTextProp
            {
                get => (string)GetValue("FullTextProp")!;
                set => SetValue("FullTextProp", value);
            }

            public string? NullableFullTextProp
            {
                get => (string?)GetValue("NullableFullTextProp");
                set => SetValue("NullableFullTextProp", value);
            }

            public int IntProp
            {
                get => (int)GetValue("IntProp");
                set => SetValue("IntProp", value);
            }

            public System.Guid GuidProp
            {
                get => (System.Guid)GetValue("GuidProp");
                set => SetValue("GuidProp", value);
            }

            public System.Guid GeneralGuidProp
            {
                get => (System.Guid)GetValue("GeneralGuidProp");
                set => SetValue("GeneralGuidProp", value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Realms.Preserve(AllMembers = true)]
        internal class IndexedClassUnmanagedAccessor : Realms.UnmanagedAccessor, IIndexedClassAccessor
        {
            public override ObjectSchema ObjectSchema => IndexedClass.RealmSchema;

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

            private string _fullTextProp = "";
            public string FullTextProp
            {
                get => _fullTextProp;
                set
                {
                    _fullTextProp = value;
                    RaisePropertyChanged("FullTextProp");
                }
            }

            private string? _nullableFullTextProp;
            public string? NullableFullTextProp
            {
                get => _nullableFullTextProp;
                set
                {
                    _nullableFullTextProp = value;
                    RaisePropertyChanged("NullableFullTextProp");
                }
            }

            private int _intProp;
            public int IntProp
            {
                get => _intProp;
                set
                {
                    _intProp = value;
                    RaisePropertyChanged("IntProp");
                }
            }

            private System.Guid _guidProp;
            public System.Guid GuidProp
            {
                get => _guidProp;
                set
                {
                    _guidProp = value;
                    RaisePropertyChanged("GuidProp");
                }
            }

            private System.Guid _generalGuidProp;
            public System.Guid GeneralGuidProp
            {
                get => _generalGuidProp;
                set
                {
                    _generalGuidProp = value;
                    RaisePropertyChanged("GeneralGuidProp");
                }
            }

            public IndexedClassUnmanagedAccessor(Type objectType) : base(objectType)
            {
            }

            public override Realms.RealmValue GetValue(string propertyName)
            {
                return propertyName switch
                {
                    "Id" => _id,
                    "FullTextProp" => _fullTextProp,
                    "NullableFullTextProp" => _nullableFullTextProp,
                    "IntProp" => _intProp,
                    "GuidProp" => _guidProp,
                    "GeneralGuidProp" => _generalGuidProp,
                    _ => throw new MissingMemberException($"The object does not have a gettable Realm property with name {propertyName}"),
                };
            }

            public override void SetValue(string propertyName, Realms.RealmValue val)
            {
                switch (propertyName)
                {
                    case "Id":
                        throw new InvalidOperationException("Cannot set the value of a primary key property with SetValue. You need to use SetValueUnique");
                    case "FullTextProp":
                        FullTextProp = (string)val!;
                        return;
                    case "NullableFullTextProp":
                        NullableFullTextProp = (string?)val;
                        return;
                    case "IntProp":
                        IntProp = (int)val;
                        return;
                    case "GuidProp":
                        GuidProp = (System.Guid)val;
                        return;
                    case "GeneralGuidProp":
                        GeneralGuidProp = (System.Guid)val;
                        return;
                    default:
                        throw new MissingMemberException($"The object does not have a settable Realm property with name {propertyName}");
                }
            }

            public override void SetValueUnique(string propertyName, Realms.RealmValue val)
            {
                if (propertyName != "Id")
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
