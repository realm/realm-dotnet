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
using Realms.Helpers;

namespace Realms
{
    [Preserve(AllMembers = true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "This should not be directly accessed by users.")]
    [DebuggerDisplay("Count = {Count}")]
    public class RealmDictionary<TValue> : RealmCollectionBase<KeyValuePair<string, TValue>>, IDictionary<string, TValue>, IDynamicMetaObjectProvider
    {
        private readonly DictionaryHandle _dictionaryHandle;

        public TValue this[string key]
        {
            get
            {
                if (_dictionaryHandle.TryGet(key, Metadata, Realm, out var result))
                {
                    return Operator.Convert<RealmValue, TValue>(result);
                }

                throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");
            }
            set => _dictionaryHandle.Set(key, Operator.Convert<TValue, RealmValue>(value));
        }

        public ICollection<string> Keys
        {
            get
            {
                var resultsHandle = _dictionaryHandle.GetKeys();
                return new RealmResults<string>(Realm, resultsHandle, Metadata);
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                var resultsHandle = _dictionaryHandle.GetValues();
                return new RealmResults<TValue>(Realm, resultsHandle, Metadata);
            }
        }

        internal RealmDictionary(Realm realm, DictionaryHandle adoptedDictionary, RealmObjectBase.Metadata metadata)
            : base(realm, metadata)
        {
            _dictionaryHandle = adoptedDictionary;
        }

        public void Add(string key, TValue value) => _dictionaryHandle.Add(key, Operator.Convert<TValue, RealmValue>(value));

        public void Add(KeyValuePair<string, TValue> item) => _dictionaryHandle.Add(item.Key, Operator.Convert<TValue, RealmValue>(item.Value));

        public bool ContainsKey(string key) => _dictionaryHandle.ContainsKey(key);

        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            throw new NotImplementedException();
        }

        public override int IndexOf(KeyValuePair<string, TValue> value) => throw new NotSupportedException();

        public bool Remove(string key) => _dictionaryHandle.Remove(key);

        public bool Remove(KeyValuePair<string, TValue> item) => _dictionaryHandle.Remove(item.Key);

        public bool TryGetValue(string key, out TValue value)
        {
            if (_dictionaryHandle.TryGet(key, Metadata, Realm, out var realmValue))
            {
                value = Operator.Convert<RealmValue, TValue>(realmValue);
                return true;
            }

            value = default;
            return false;
        }

        internal override RealmCollectionBase<KeyValuePair<string, TValue>> CreateCollection(Realm realm, CollectionHandleBase handle) => new RealmDictionary<TValue>(realm, (DictionaryHandle)handle, Metadata);

        internal override CollectionHandleBase GetOrCreateHandle() => _dictionaryHandle;

        protected override KeyValuePair<string, TValue> GetValueAtIndex(int index) => _dictionaryHandle.GetValueAtIndex<TValue>(index, Metadata, Realm);
    }
}
