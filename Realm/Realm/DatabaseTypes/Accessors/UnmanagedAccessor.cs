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
using Realms.Exceptions;
using Realms.Schema;

namespace Realms
{
    internal class UnmanagedAccessor
        : IRealmAccessor
    {
        private Dictionary<string, object> _container = new();

        public bool IsManaged => false;

        public bool IsValid => true;

        public bool IsFrozen => false;

        public Realm Realm => null;

        public ObjectSchema ObjectSchema => null;

        public int? HashCode => null;

        public int BacklinksCount => 0;

        public RealmObjectBase.Dynamic DynamicApi => throw new NotSupportedException("Using the dynamic API to access a RealmObject is only possible for managed (persisted) objects.");

        public IQueryable<dynamic> GetBacklinks(string objectType, string property) => throw new NotSupportedException("Using the dynamic API to access a RealmObject is only possible for managed (persisted) objects.");

        public IQueryable<T> GetBacklinks<T>(string propertyName)
            where T : RealmObjectBase
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
            if (!_container.ContainsKey(propertyName))
            {
                _container[propertyName] = new Dictionary<string, TValue>();
            }

            return (IDictionary<string, TValue>)_container[propertyName];
        }

        public IList<T> GetListValue<T>(string propertyName)
        {
            if (!_container.ContainsKey(propertyName))
            {
                _container[propertyName] = new List<T>();
            }

            return (IList<T>)_container[propertyName];
        }

        public ISet<T> GetSetValue<T>(string propertyName)
        {
            if (!_container.ContainsKey(propertyName))
            {
                _container[propertyName] = new HashSet<T>(RealmSet<T>.Comparer);
            }

            return (ISet<T>)_container[propertyName];
        }

        public string GetStringDescription(string typeName)
        {
            return $"{typeName} (unmanaged)";
        }

        public RealmValue GetValue(string propertyName)
        {
            return (RealmValue)_container[propertyName];
        }

        public bool ObjectEquals(object obj)
        {
            return false;
        }

        public void SetValue(string propertyName, RealmValue val)
        {
            _container[propertyName] = val;
        }

        public void SetValueUnique(string propertyName, RealmValue val)
        {
            _container[propertyName] = val;
        }

        public void SubscribeForNotifications()
        {
        }

        public void UnsubscribeFromNotifications()
        {
        }
    }
}
