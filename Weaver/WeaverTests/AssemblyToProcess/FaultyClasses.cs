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
using System.Diagnostics.CodeAnalysis;
using Realms;

namespace AssemblyToProcess
{
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class RealmListWithSetter : RealmObject
    {
        public IList<Person> People { get; set; }

        public int PropertyToEnsureOtherwiseHealthyClass { get; set; }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
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

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class PrimaryKeyProperties : RealmObject
    {
        // These should be allowed:

        [PrimaryKey]
        public int IntProperty { get; set; }

        [PrimaryKey]
        public string StringProperty { get; set; }

        // These should cause errors:

        [PrimaryKey]
        public bool BooleanProperty { get; set; }

        [PrimaryKey]
        public DateTimeOffset DateTimeOffsetProperty { get; set; }

        [PrimaryKey]
        public float SingleProperty { get; set; }
    }

    // This class has no default constructor which is necessary for Realm.CreateObject<>()
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class DefaultConstructorMissing : RealmObject
    {
        public DefaultConstructorMissing(int parameter)
        {
        }

        public int PropertyToEnsureOtherwiseHealthyClass { get; set; }
    }

    // This class has no persisted properties. 
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class NoPersistedProperties : RealmObject
    {
        public int PublicField;

        [Ignored]
        public int IgnoredProperty { get; set; }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class NotSupportedProperties : RealmObject
    {
        public DateTime DateTimeProperty { get; set; }

        public DateTime? NullableDateTimeProperty { get; set; }

        public MyEnum EnumProperty { get; set; }

        public enum MyEnum
        {
            Value1,
            Value2
        }
    }
}
