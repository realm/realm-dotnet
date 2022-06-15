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
using System.Diagnostics;
using System.Linq;
using Realms.Schema;

namespace Realms
{
    // Should be used by generator and undocumented
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class UnmanagedAccessor : IRealmAccessor
    {
        private readonly Type _objectType;

        private Action<string> _onNotifyPropertyChanged;

        public bool IsManaged => false;

        public bool IsValid => true;

        public bool IsFrozen => false;

        public Realm Realm => null;

        public ObjectSchema ObjectSchema => null;

        public int BacklinksCount => 0;

        public RealmObjectBase.Dynamic DynamicApi => throw new NotSupportedException("Using the dynamic API to access a RealmObject is only possible for managed (persisted) objects.");

        public UnmanagedAccessor(Type objectType)
        {
            _objectType = objectType;
        }

        public IQueryable<T> GetBacklinks<T>(string propertyName) where T : IRealmObjectBase
            => throw new NotSupportedException("Using the GetBacklinks is only possible for managed (persisted) objects.");

        public virtual IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName) => GetPropertyValue<IDictionary<string, TValue>>(propertyName);

        public virtual IList<T> GetListValue<T>(string propertyName) => GetPropertyValue<IList<T>>(propertyName);

        public virtual ISet<T> GetSetValue<T>(string propertyName) => GetPropertyValue<ISet<T>>(propertyName);

        public virtual RealmValue GetValue(string propertyName) => GetPropertyValue<RealmValue>(propertyName);

        public virtual void SetValue(string propertyName, RealmValue val) => SetPropertyValue(propertyName, val);

        public virtual void SetValueUnique(string propertyName, RealmValue val) => SetPropertyValue(propertyName, val);

        public virtual void SubscribeForNotifications(Action<string> notifyPropertyChangedDelegate)
        {
            _onNotifyPropertyChanged = notifyPropertyChangedDelegate;
        }

        public virtual void UnsubscribeFromNotifications()
        {
            _onNotifyPropertyChanged = null;
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            _onNotifyPropertyChanged?.Invoke(propertyName);
        }

        public override string ToString()
        {
            return $"{_objectType.Name} (unmanaged)";
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj);
        }

        protected virtual T GetPropertyValue<T>(string propertyName)
        {
            return (T)GetType().GetProperty(propertyName).GetValue(this);
        }

        //TODO This does not work because value is usually RealmValue and the property is of a specific type. 
        //We could make this abstract and implement it in the class
        protected virtual void SetPropertyValue(string propertyName, object value)
        {
            GetType().GetProperty(propertyName).SetValue(this, value);
        }
    }

    // Should be used by the weaver and undocumented
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

        protected override T GetPropertyValue<T>(string propertyName)
        {
            throw new NotSupportedException("This should not be used for now");
        }

        protected override void SetPropertyValue(string propertyName, object value)
        {
            throw new NotSupportedException("This should not be used for now");
        }
    }
}
