////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
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

#if TEST_WEAVER
using TestRealmObject = Realms.RealmObject;
#else
using TestRealmObject = Realms.IRealmObject;
#endif
using System.Collections.Generic;
using System.Linq;

namespace Realms.Tests.Database
{
    public partial class TestNotificationObject : TestRealmObject
    {
        public string? StringProperty { get; set; }

        public int IntProperty { get; set; }

        public IList<TestNotificationObject> ListSameType { get; } = null!;

        public ISet<TestNotificationObject> SetSameType { get; } = null!;

        public IDictionary<string, TestNotificationObject?> DictionarySameType { get; } = null!;

        public TestNotificationObject? LinkSameType { get; set; }

        public IList<Person> ListDifferentType { get; } = null!;

        public ISet<Person> SetDifferentType { get; } = null!;

        public IDictionary<string, Person?> DictionaryDifferentType { get; } = null!;

        public Person? LinkDifferentType { get; set; }

        public Owner? LinkAnotherType { get; set; }

        [Backlink(nameof(LinkSameType))]
        public IQueryable<TestNotificationObject> Backlink { get; } = null!;
    }
}
