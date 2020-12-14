////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq.Expressions;

namespace Realms
{
    [Preserve(AllMembers = true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "This should not be directly accessed by users.")]
    [DebuggerDisplay("Count = {Count}")]
    public class RealmDictionary : RealmCollectionBase<KeyValuePair<string, RealmValue>>, IDictionary<string, RealmValue>, IDynamicMetaObjectProvider
    {
        private readonly DictionaryHandle _dictionaryHandle;

        public RealmValue this[string key]
        {
            get
            {
                if (_dictionaryHandle.TryGet(key, Realm, out var result))
                {
                    return result;
                }

                throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");
            }
            set => _dictionaryHandle.Set(key, value);
        }

        public ICollection<string> Keys => throw new System.NotImplementedException();

        public ICollection<RealmValue> Values => throw new System.NotImplementedException();

        internal RealmDictionary(Realm realm, DictionaryHandle adoptedDictionary)
            : base(realm, metadata: null)
        {
            _dictionaryHandle = adoptedDictionary;
        }

        public void Add(string key, RealmValue value) => _dictionaryHandle.Add(key, value);

        public void Add(KeyValuePair<string, RealmValue> item) => _dictionaryHandle.Add(item.Key, item.Value);

        public bool ContainsKey(string key) => _dictionaryHandle.ContainsKey(key);

        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            throw new NotImplementedException();
        }

        public override int IndexOf(KeyValuePair<string, RealmValue> value) => throw new NotSupportedException();

        public bool Remove(string key) => _dictionaryHandle.Remove(key);

        public bool Remove(KeyValuePair<string, RealmValue> item) => _dictionaryHandle.Remove(item.Key);

        public bool TryGetValue(string key, out RealmValue value) => _dictionaryHandle.TryGet(key, Realm, out value);

        internal override RealmCollectionBase<KeyValuePair<string, RealmValue>> CreateCollection(Realm realm, CollectionHandleBase handle) => new RealmDictionary(realm, (DictionaryHandle)handle);

        internal override CollectionHandleBase GetOrCreateHandle() => _dictionaryHandle;
    }
}
