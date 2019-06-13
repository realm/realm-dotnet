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

using System;
using System.Linq;

namespace Realms.Server
{
    internal class ChangeSetDetails : IChangeSetDetails
    {
        private readonly Lazy<IQueryable<dynamic>> _previousQuery;
        private readonly Lazy<IQueryable<dynamic>> _currentQuery;

        public IModificationDetails[] Insertions { get; }

        public IModificationDetails[] Modifications { get; }

        public IModificationDetails[] Deletions { get; }

        public ChangeSetDetails(Realm previous, Realm current, string className, int[] insertions, int[] modifications, int[] currentModifications, int[] deletions)
        {
            _previousQuery = new Lazy<IQueryable<dynamic>>(() => previous.All(className));
            _currentQuery = new Lazy<IQueryable<dynamic>>(() => current.All(className));

            Insertions = insertions.Select(i => new ModificationDetails(-1, i, _ => null, _currentQuery.Value.ElementAt)).ToArray();
            Deletions = deletions.Select(i => new ModificationDetails(i, -1, _previousQuery.Value.ElementAt, _ => null)).ToArray();
            Modifications = modifications.Select((value, index) => new ModificationDetails(
                                                                            value,
                                                                            currentModifications[index],
                                                                            _previousQuery.Value.ElementAt,
                                                                            _currentQuery.Value.ElementAt))
                                         .ToArray();
        }
    }
}
