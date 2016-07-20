////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
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
using Realms;

namespace AssemblyToProcess
{
    public class RealmListWithSetter : RealmObject
    {
        public IList<Person> People { get; set; } 

        public int PropertyToEnsureOtherwiseHealthyClass { get; set; }
    }

    public class IndexedProperties : RealmObject
    {
        // These should be allowed:

        [Indexed]
        public int IntProperty { get; set; }

        [Indexed]
        public string StringProperty { get; set; }

        [Indexed]
        public bool BooleanProperty { get; set; }

        [Indexed]
        public DateTimeOffset DateTimeOffsetProperty { get; set; }

        // This should cause an error:

        [Indexed]
        public float SingleProperty { get; set; }
    }

    public class ObjectIdProperties : RealmObject
    {
        // These should be allowed:

        [ObjectId]
        public int IntProperty { get; set; }

        [ObjectId]
        public string StringProperty { get; set; }

        // These should cause errors:

        [ObjectId]
        public bool BooleanProperty { get; set; }

        [ObjectId]
        public DateTimeOffset DateTimeOffsetProperty { get; set; }

        [ObjectId]
        public float SingleProperty { get; set; }
    }

    // This class has no default constructor which is necessary for Realm.CreateObject<>()
    public class DefaultConstructorMissing : RealmObject
    {
        public DefaultConstructorMissing(int parameter) { }

        public int PropertyToEnsureOtherwiseHealthyClass { get; set; }
    }

    // This class has no persisted properties. 
    public class NoPersistedProperties : RealmObject
    {
        public int PublicField;

        [Ignored]
        public int IgnoredProperty { get; set; }
    }
}
