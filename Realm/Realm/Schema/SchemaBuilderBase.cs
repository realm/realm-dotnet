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
using Realms.Helpers;

namespace Realms.Schema
{
    /// <summary>
    /// A base class for the schema builders exposed by Realm.
    /// </summary>
    /// <typeparam name="T">The type of the elements contained in the builder.</typeparam>
    public abstract class SchemaBuilderBase<T> : IEnumerable<T>
    {
        private protected readonly IDictionary<string, T> _values = new Dictionary<string, T>();

        /// <summary>
        /// Gets or sets an element in the builder by name.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <returns>The element with the specified name.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        /// <exception cref="KeyNotFoundException">Thrown by the getter if the builder doesn't contain an element with the specified <paramref name="name"/>.</exception>
        public T this[string name]
        {
            get => _values[name];
            set
            {
                Argument.NotNull(name, nameof(name));
                Argument.NotNull(value, nameof(value));

                if (name != GetKey(value))
                {
                    throw new ArgumentException($"The name of the element ('{GetKey(value)}') doesn't match the provided name ('{name}'). ", nameof(name));
                }

                _values[name] = value;
            }
        }

        /// <summary>
        /// Gets the number of elements the builder contains.
        /// </summary>
        /// <value>The number of elements in the builder.</value>
        public int Count => _values.Count;

        protected void Add(T item)
        {
            Argument.NotNull(item, nameof(item));

            var key = GetKey(item);
            if (_values.ContainsKey(key))
            {
                throw new ArgumentException($"This builder already contains an item with the name '{key}'", nameof(item));
            }

            _values.Add(key, item);
        }

        /// <summary>
        /// Removes an element from the builder.
        /// </summary>
        /// <param name="item">The element to remove.</param>
        /// <returns><c>true</c> if the element was found and removed; <c>false</c> otherwise.</returns>
        public bool Remove(T item) => _values.Remove(new KeyValuePair<string, T>(GetKey(item), item));

        /// <summary>
        /// Removes an element from the builder by name.
        /// </summary>
        /// <param name="name">The name of the element to remove.</param>
        /// <returns><c>true</c> if the element was found and removed; <c>false</c> otherwise.</returns>
        public bool Remove(string name) => _values.Remove(name);

        /// <summary>
        /// Checks if the builder contains the provided element.
        /// </summary>
        /// <param name="item">The item to check for existence.</param>
        /// <returns><c>true</c> if the builder contains the specified item; <c>false</c> otherwise.</returns>
        public bool Contains(T item) => _values.Contains(new KeyValuePair<string, T>(GetKey(item), item));

        /// <summary>
        /// Checks if the builder contains the provided element by name.
        /// </summary>
        /// <param name="name">The name of the element being searched for.</param>
        /// <returns><c>true</c> if the builder contains the specified item; <c>false</c> otherwise.</returns>
        public bool Contains(string name) => _values.ContainsKey(name);

        private protected abstract string GetKey(T item);

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator() => _values.Values.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => _values.Values.GetEnumerator();
    }
}
