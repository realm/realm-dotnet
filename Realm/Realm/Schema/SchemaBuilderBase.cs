////////////////////////////////////////////////////////////////////////////
//
// Copyright 2021 Realm Inc.
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

using System;
using System.Collections;
using System.Collections.Generic;

namespace Realms.Schema
{
    public abstract class SchemaBuilderBase<T> : IEnumerable<T>
    {
        protected IDictionary<string, T> _values = new Dictionary<string, T>();

        public T this[string name]
        {
            get => _values[name];
            set => _values[name] = value;
        }

        public int Count => _values.Count;

        protected void Add(T item)
        {
            var key = GetKey(item);
            if (_values.ContainsKey(key))
            {
                throw new ArgumentException($"This builder already contains an item with the name '{key}'", nameof(item));
            }

            _values.Add(key, item);
        }

        public bool Remove(T item) => _values.Remove(new KeyValuePair<string, T>(GetKey(item), item));

        public bool Remove(string propertyName) => _values.Remove(propertyName);

        public bool Contains(T item) => _values.Contains(new KeyValuePair<string, T>(GetKey(item), item));

        public bool Contains(string propertyName) => _values.ContainsKey(propertyName);

        protected abstract string GetKey(T item);

        public IEnumerator<T> GetEnumerator() => _values.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _values.Values.GetEnumerator();
    }
}
