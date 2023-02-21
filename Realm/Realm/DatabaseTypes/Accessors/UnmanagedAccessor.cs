////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Realms.DataBinding;
using Realms.Schema;

namespace Realms
{
    /// <summary>
    /// Represents the base class for an accessor to be used with an unmanaged object.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class UnmanagedAccessor : IRealmAccessor
    {
        private readonly Type _objectType;

        private Action<string>? _onNotifyPropertyChanged;

        /// <inheritdoc/>
        public bool IsManaged => false;

        /// <inheritdoc/>
        public bool IsValid => true;

        /// <inheritdoc/>
        public bool IsFrozen => false;

        /// <inheritdoc/>
        public Realm? Realm => null;

        /// <inheritdoc/>
        public virtual ObjectSchema? ObjectSchema => null;

        /// <inheritdoc/>
        public int BacklinksCount => 0;

        /// <inheritdoc/>
        public DynamicObjectApi DynamicApi => throw new NotSupportedException("Using the dynamic API to access a RealmObject is only possible for managed (persisted) objects.");

        /// <inheritdoc/>
        public IRealmObjectBase? GetParent() => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnmanagedAccessor"/> class.
        /// </summary>
        /// <param name="objectType">The runtype type of the realm object.</param>
        public UnmanagedAccessor(Type objectType)
        {
            _objectType = objectType;
        }

        /// <inheritdoc/>
        public IQueryable<T> GetBacklinks<T>(string propertyName)
            where T : IRealmObjectBase
            => throw new NotSupportedException("Using the GetBacklinks is only possible for managed (persisted) objects.");

        /// <inheritdoc/>
        public abstract IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName);

        /// <inheritdoc/>
        public abstract IList<T> GetListValue<T>(string propertyName);

        /// <inheritdoc/>
        public abstract ISet<T> GetSetValue<T>(string propertyName);

        /// <inheritdoc/>
        public abstract RealmValue GetValue(string propertyName);

        /// <inheritdoc/>
        public abstract void SetValue(string propertyName, RealmValue val);

        /// <inheritdoc/>
        public abstract void SetValueUnique(string propertyName, RealmValue val);

        /// <inheritdoc/>
        public virtual void SubscribeForNotifications(Action<string> notifyPropertyChangedDelegate)
        {
            _onNotifyPropertyChanged = notifyPropertyChangedDelegate;
        }

        /// <inheritdoc/>
        public virtual void UnsubscribeFromNotifications()
        {
            _onNotifyPropertyChanged = null;
        }

        /// <summary>
        /// Invokes the property changed delegate.
        /// </summary>
        /// <param name="propertyName">The name of the property to notify about.</param>
        protected void RaisePropertyChanged(string propertyName)
        {
            _onNotifyPropertyChanged?.Invoke(propertyName);
        }

        /// <inheritdoc/>
        public TypeInfo GetTypeInfo(IRealmObjectBase obj)
        {
#pragma warning disable CA1062 // Validate arguments of public methods
            return TypeInfoHelper.GetInfo(obj);
#pragma warning restore CA1062 // Validate arguments of public methods
        }

        /// <inheritdoc/>
        public override string? ToString() => $"{_objectType.Name} (unmanaged)";

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj);
        }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Better code organisation")]
    internal class GenericUnmanagedAccessor : UnmanagedAccessor
    {
        public GenericUnmanagedAccessor(Type type) : base(type)
        {
        }

        public override IList<T> GetListValue<T>(string propertyName)
        {
            return new List<T>();
        }

        public override ISet<T> GetSetValue<T>(string propertyName)
        {
            return new HashSet<T>(RealmSet<T>.Comparer);
        }

        public override IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
        {
            return new Dictionary<string, TValue>();
        }

        public override RealmValue GetValue(string propertyName)
        {
            throw new NotSupportedException("This should not be used for now");
        }

        public override void SetValue(string propertyName, RealmValue val)
        {
            throw new NotSupportedException("This should not be used for now");
        }

        public override void SetValueUnique(string propertyName, RealmValue val)
        {
            throw new NotSupportedException("This should not be used for now");
        }
    }
}
