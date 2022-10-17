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
using System.Diagnostics;
using System.Linq;
using Realms.Schema;

namespace Realms
{
    internal class UnmanagedAccessor
        : IRealmAccessor
    {
        private Type _objectType;

        public UnmanagedAccessor(Type objectType)
        {
            _objectType = objectType;
        }

        public bool IsManaged => false;

        public bool IsValid => true;

        public bool IsFrozen => false;

        public Realm Realm => null;

        public ObjectSchema ObjectSchema => null;

        public int BacklinksCount => 0;

        public RealmObjectBase.Dynamic DynamicApi => throw new NotSupportedException("Using the dynamic API to access a RealmObject is only possible for managed (persisted) objects.");

        public IQueryable<T> GetBacklinks<T>(string propertyName)
            where T : IRealmObjectBase
        {
            Debug.Assert(false, "Object is not managed, but managed access was attempted");

            throw new InvalidOperationException("Object is not managed, but managed access was attempted");
        }

        public static ThreadSafeReference GetSafeReference()
        {
            Debug.Assert(false, "Object is not managed, but managed access was attempted");

            throw new InvalidOperationException("Object is not managed, but managed access was attempted");
        }

        public IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
        {
            return new Dictionary<string, TValue>();
        }

        public IList<T> GetListValue<T>(string propertyName)
        {
            return new List<T>();
        }

        public ISet<T> GetSetValue<T>(string propertyName)
        {
            return new HashSet<T>(RealmSet<T>.Comparer);
        }

        public RealmValue GetValue(string propertyName)
        {
            throw new NotImplementedException("This should not be used for now");
        }

        public void SetValue(string propertyName, RealmValue val)
        {
            throw new NotImplementedException("This should not be used for now");
        }

        public void SetValueUnique(string propertyName, RealmValue val)
        {
            throw new NotImplementedException("This should not be used for now");
        }

        public IRealmObjectBase GetParent()
        {
            return null;
        }

        public void SubscribeForNotifications(Action<string> notifyPropertyChangedDelegate)
        {
        }

        public void UnsubscribeFromNotifications()
        {
        }

        public override string ToString()
        {
            return $"{_objectType.Name} (unmanaged)";
        }

        public override bool Equals(object obj)
        {
            return false;
        }
    }
}
