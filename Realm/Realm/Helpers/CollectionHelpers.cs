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

using System.Diagnostics;

namespace Realms.Helpers;

internal static class CollectionHelpers
{
    internal static void PopulateCollection(Realm realm, CollectionHandleBase handle, RealmValue content)
    {
        if (handle is ListHandle listHandle)
        {
            var newList = new RealmList<RealmValue>(realm, listHandle, metadata: null);

            foreach (var item in content.AsList())
            {
                newList.Add(item);
            }
        }
        else if (handle is DictionaryHandle dictHandle)
        {
            var newDict = new RealmDictionary<RealmValue>(realm, dictHandle, metadata: null);

            foreach (var item in content.AsDictionary())
            {
                newDict.Add(item);
            }
        }
        else
        {
            Debug.Fail("Invalid collection type");
        }
    }
}
