////////////////////////////////////////////////////////////////////////////
//
// Copyright 2019 Realm Inc.
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
using System.Linq;
using Realms.Server.Native;

namespace Realms.Server
{
    internal class ChangeDetails : IChangeDetails
    {
        public string RealmPath { get; }

        public Realm PreviousRealm { get; }

        public Realm CurrentRealm { get; }

        public IReadOnlyDictionary<string, IChangeSetDetails> Changes { get; }

        public Realm GetRealmForWriting()
        {
            var config = new NotifierRealmConfiguration(NotifierHandle.GetRealmForWriting(CurrentRealm.SharedRealmHandle), CurrentRealm.Config.DatabasePath);
            return Realm.GetInstance(config);
        }

        public ChangeDetails(string path, IEnumerable<NativeChangeSet> changeSets, Realm previousRealm, Realm currentRealm)
        {
            RealmPath = path;

            PreviousRealm = previousRealm;
            CurrentRealm = currentRealm;

            Changes = changeSets.ToDictionary(x => x.ClassName, x => (IChangeSetDetails)new ChangeSetDetails(
                                    previous: PreviousRealm,
                                    current: CurrentRealm,
                                    className: x.ClassName,
                                    insertions: x.insertions.AsEnumerable().Select(i => (int)i).ToArray(),
                                    modifications: x.previous_modifications.AsEnumerable().Select(i => (int)i).ToArray(),
                                    currentModifications: x.current_modifications.AsEnumerable().Select(i => (int)i).ToArray(),
                                    deletions: x.deletions.AsEnumerable().Select(i => (int)i).ToArray()));
        }
    }
}
