////////////////////////////////////////////////////////////////////////////
//
// Copyright 2023 Realm Inc.
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

using System.Collections.Generic;

namespace Realms.Native
{
    internal struct MarshaledPair<TKey, TValue>
        where TKey : unmanaged
        where TValue : unmanaged
    {
        public TKey Key;

        public TValue Value;

        public MarshaledPair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public void Deconstruct(out TKey key, out TValue value)
        {
            key = Key;
            value = Value;
        }

        public static implicit operator KeyValuePair<TKey, TValue>(MarshaledPair<TKey, TValue> pair) => new (pair.Key, pair.Value);
    }
}
