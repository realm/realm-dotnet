////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Realms.DataBinding;
using Realms.Schema;
using Realms.Weaving;

namespace Realms
{
    /// <summary>
    /// Base for any object that can be persisted in a <see cref="Realms.Realm"/>. Models inheriting from
    /// this class will be processed at compile time by the Fody weaver. It is recommended that you instead
    /// inherit from <see cref="IRealmObject"/> and use the Realm Source Generator to generate your models.
    /// </summary>
    [Preserve(AllMembers = true)]
    public abstract class RealmObjectBase
        : IRealmObjectBase,
          ISettableManagedAccessor,
          INotifyPropertyChanged,
          IReflectableType
    {
        private IRealmAccessor _accessor;

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "This is the private event - the public is uppercased.")]
        private event PropertyChangedEventHandler? _propertyChanged;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged
        {
            add
            {
                if (IsManaged && _propertyChanged == null)
                {
                    SubscribeForNotifications();
                }

                _propertyChanged += value;
            }

            remove
            {
                _propertyChanged -= value;

                if (IsManaged &&
                    _propertyChanged == null)
                {
                    UnsubscribeFromNotifications();
                }
            }
        }

        /// <summary>
        /// Gets the accessor that encapsulates the methods and properties used by the object for its functioning.
        /// </summary>
        [IgnoreDataMember, XmlIgnore]
        IRealmAccessor IRealmObjectBase.Accessor => _accessor;

        /// <summary>
        /// Gets a value indicating whether the object has been associated with a Realm, either at creation or via
        /// <see cref="Realm.Add{T}(T, bool)"/>.
        /// </summary>
        /// <value><c>true</c> if object belongs to a Realm; <c>false</c> if standalone.</value>
        [IgnoreDataMember]
        public bool IsManaged => _accessor.IsManaged;

        /// <summary>
        /// Gets an object encompassing the dynamic API for this RealmObjectBase instance.
        /// </summary>
        /// <value>A <see cref="Dynamic"/> instance that wraps this RealmObject.</value>
        [IgnoreDataMember]
        public DynamicObjectApi DynamicApi => _accessor.DynamicApi;

        /// <summary>
        /// Gets a value indicating whether this object is managed and represents a row in the database.
        /// If a managed object has been removed from the Realm, it is no longer valid and accessing properties on it
        /// will throw an exception.
        /// Unmanaged objects are always considered valid.
        /// </summary>
        /// <value><c>true</c> if managed and part of the Realm or unmanaged; <c>false</c> if managed but deleted.</value>
        [IgnoreDataMember]
        public bool IsValid => _accessor.IsValid;

        /// <summary>
        /// Gets a value indicating whether this object is frozen. Frozen objects are immutable
        /// and will not update when writes are made to the Realm. Unlike live objects, frozen
        /// objects can be used across threads.
        /// </summary>
        /// <value><c>true</c> if the object is frozen and immutable; <c>false</c> otherwise.</value>
        /// <seealso cref="FrozenObjectsExtensions.Freeze{T}(T)"/>
        [IgnoreDataMember]
        public bool IsFrozen => _accessor.IsFrozen;

        /// <summary>
        /// Gets the <see cref="Realm"/> instance this object belongs to, or <c>null</c> if it is unmanaged.
        /// </summary>
        /// <value>The <see cref="Realm"/> instance this object belongs to.</value>
        [IgnoreDataMember]
        public Realm? Realm => _accessor.Realm;

        /// <summary>
        /// Gets the <see cref="Schema.ObjectSchema"/> instance that describes how the <see cref="Realm"/> this object belongs to sees it.
        /// </summary>
        /// <value>A collection of properties describing the underlying schema of this object.</value>
        [IgnoreDataMember, XmlIgnore] // XmlIgnore seems to be needed here as IgnoreDataMember is not sufficient for XmlSerializer.
        public ObjectSchema? ObjectSchema => _accessor.ObjectSchema;

        /// <summary>
        /// Gets the number of objects referring to this one via either a to-one or to-many relationship.
        /// </summary>
        /// <remarks>
        /// This property is not observable so the <see cref="PropertyChanged"/> event will not fire when its value changes.
        /// </remarks>
        /// <value>The number of objects referring to this one.</value>
        [IgnoreDataMember]
        public int BacklinksCount => _accessor.BacklinksCount;

        internal RealmObjectBase()
        {
            _accessor = new GenericUnmanagedAccessor(GetType());
        }

        /// <summary>
        /// Sets the accessor for the newly managed object and possibly adds the object to the realm.
        /// </summary>
        /// <param name="accessor">The accessor to set.</param>
        /// <param name="helper">The<see cref="IRealmObjectHelper"/> implementation to use for copying the object to realm.</param>
        /// <param name="update">If set to <c>true</c>, update the existing value (if any). Otherwise, try to add and throw if an object with the same primary key already exists.</param>
        /// <param name="skipDefaults">
        /// If set to <c>true</c> will not invoke the setters of properties that have default values.
        /// Generally, should be <c>true</c> for newly created objects and <c>false</c> when updating existing ones.
        /// </param>
        void ISettableManagedAccessor.SetManagedAccessor(IRealmAccessor accessor, IRealmObjectHelper? helper, bool update, bool skipDefaults)
        {
            _accessor = accessor;

            helper?.CopyToRealm(this, update, skipDefaults);

            if (_propertyChanged != null)
            {
                SubscribeForNotifications();
            }

            OnManaged();
        }

#pragma warning disable SA1600 // Elements should be documented

        protected RealmValue GetValue(string propertyName)
        {
            return _accessor.GetValue(propertyName);
        }

        protected void SetValue(string propertyName, RealmValue val)
        {
            _accessor.SetValue(propertyName, val);
        }

        protected void SetValueUnique(string propertyName, RealmValue val)
        {
            _accessor.SetValueUnique(propertyName, val);
        }

        protected internal IList<T> GetListValue<T>(string propertyName)
        {
            return _accessor.GetListValue<T>(propertyName);
        }

        protected internal ISet<T> GetSetValue<T>(string propertyName)
        {
            return _accessor.GetSetValue<T>(propertyName);
        }

        protected internal IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
        {
            return _accessor.GetDictionaryValue<TValue>(propertyName);
        }

        protected IQueryable<T> GetBacklinks<T>(string propertyName)
            where T : IRealmObjectBase
        {
            return _accessor.GetBacklinks<T>(propertyName);
        }

#pragma warning restore SA1600 // Elements should be documented

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            // If parameter is null, return false.
            if (obj is null)
            {
                return false;
            }

            // Optimization for a common success case.
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            // Special case to cover possible bugs similar to WPF (#1903)
            if (obj is InvalidObject)
            {
                return !IsValid;
            }

            if (obj is not IRealmObjectBase iro)
            {
                return false;
            }

            return _accessor.Equals(iro.Accessor);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            // _hashCode is only set for managed objects - for unmanaged ones, we
            // fall back to the default behavior.
            return IsManaged ? _accessor.GetHashCode() : base.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string? ToString() => _accessor.ToString();

        /// <summary>
        /// Allows you to raise the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed. If not specified, we'll use the caller name.</param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            OnPropertyChanged(propertyName);
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
        /// class MyClass : RealmObject
        /// {
        ///     public int StatusCodeRaw { get; set; }
        ///     public StatusCodeEnum StatusCode => (StatusCodeEnum)StatusCodeRaw;
        ///     protected override void OnPropertyChanged(string propertyName)
        ///     {
        ///         if (propertyName == nameof(StatusCodeRaw))
        ///         {
        ///             RaisePropertyChanged(nameof(StatusCode));
        ///         }
        ///     }
        /// }
        /// </code>
        /// Here, we have a computed property that depends on a persisted one. In order to notify any <see cref="PropertyChanged"/>
        /// subscribers that <c>StatusCode</c> has changed, we override <see cref="OnPropertyChanged"/> and
        /// raise <see cref="PropertyChanged"/> manually by calling <see cref="RaisePropertyChanged"/>.
        /// </example>
        protected virtual void OnPropertyChanged(string propertyName)
        {
        }

        /// <summary>
        /// Called when the object has been managed by a Realm.
        /// </summary>
        /// <remarks>
        /// This method will be called either when a managed object is materialized or when an unmanaged object has been
        /// added to the Realm. It can be useful for providing some initialization logic as when the constructor is invoked,
        /// it is not yet clear whether the object is managed or not.
        /// </remarks>
        protected internal virtual void OnManaged()
        {
        }

        private void SubscribeForNotifications()
        {
            _accessor.SubscribeForNotifications(RaisePropertyChanged);
        }

        private void UnsubscribeFromNotifications()
        {
            _accessor.UnsubscribeFromNotifications();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "This should not be directly accessed by users.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TypeInfo GetTypeInfo()
        {
            return TypeInfoHelper.GetInfo(this);
        }
    }
}
