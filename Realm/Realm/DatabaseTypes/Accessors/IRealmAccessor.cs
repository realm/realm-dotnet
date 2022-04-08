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
using System.Linq;
using Realms.Schema;

namespace Realms
{
    public interface IRealmAccessor
    {
        bool IsManaged { get; }

        bool IsValid { get; }

        bool IsFrozen { get; }

        Realm Realm { get; }

        ObjectSchema ObjectSchema { get; }

        int? HashCode { get; }

        int BacklinksCount { get; }

        RealmValue GetValue(string propertyName);

        void SetValue(string propertyName, RealmValue val);

        void SetValueUnique(string propertyName, RealmValue val);

        IList<T> GetListValue<T>(string propertyName);

        ISet<T> GetSetValue<T>(string propertyName);

        IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName);

        IQueryable<T> GetBacklinks<T>(string propertyName)
            where T : RealmObjectBase;

        void SubscribeForNotifications();

        void UnsubscribeFromNotifications();

        string GetStringDescription(string typeName);

        bool ObjectEquals(object obj);
    }
}
