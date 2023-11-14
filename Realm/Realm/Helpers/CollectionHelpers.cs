////////////////////////////////////////////////////////////////////////////
//
// Copyright 2023 Realm Inc.
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

namespace Realms.Helpers;

internal static class CollectionHelpers
{
    internal static RealmList<RealmValue> ListCreateAndPopulate(Realm realm, ListHandle handle, RealmValue content)
    {
        var newList = new RealmList<RealmValue>(realm, handle, metadata: null);

        foreach (var item in content.AsList())
        {
            newList.Add(item);
        }

        return newList;
    }

    internal static RealmDictionary<RealmValue> DictionaryCreatePopulate(Realm realm, DictionaryHandle handle, RealmValue content)
    {
        var newDict = new RealmDictionary<RealmValue>(realm, handle, metadata: null);

        foreach (var item in content.AsDictionary())
        {
            newDict.Add(item);
        }

        return newDict;
    }
}
