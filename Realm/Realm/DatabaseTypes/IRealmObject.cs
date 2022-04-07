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

using System.Linq;
using Realms.Schema;

namespace Realms
{
    public interface IRealmObject
    {
        IRealmAccessor Accessor { get; } //Implemented explicitly for RealmObjectBase

        void SetManagedAccessor(IRealmAccessor acccessor);  // TODO Need to change name. This could be part of another interface "hidden" (as much as we can) from users. Not browsable.
        //Impl explicitly

        bool IsManaged { get; }

        bool IsValid { get; }

        bool IsFrozen { get; }

        Realm Realm { get; }

        ObjectSchema ObjectSchema { get; }

        int BacklinksCount { get; } //TODO Remove

        RealmObjectBase.Dynamic DynamicApi { get; } //TODO Remove

        IQueryable<dynamic> GetBacklinks(string objectType, string propertyName); //TODO Remove (no dynamic stuff)
    }

    // TODO Removing all of this could give us more flexibility if we want to geenerate those or not later
    // Otherwise we can:
    // 1) remove from intellisense (editorbrowsable.never)
    // 2) implement explicitly
    // 3) 
}
