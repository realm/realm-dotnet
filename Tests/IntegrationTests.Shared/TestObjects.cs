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
using Realms;

namespace IntegrationTests.Shared
{
    [Preserve(AllMembers = true)]
    public class AllTypesObject : RealmObject
    {
        public char CharProperty { get; set; }
        public byte ByteProperty { get; set; }
        public short Int16Property { get; set; }
        public int Int32Property { get; set; }
        public long Int64Property { get; set; }
        public float SingleProperty { get; set; }
        public double DoubleProperty { get; set; }
        public bool BooleanProperty { get; set; }
        public DateTimeOffset DateTimeOffsetProperty { get; set; }

        public string StringProperty { get; set; }
        public byte[] ByteArrayProperty { get; set; }

        public char? NullableCharProperty { get; set; }
        public byte? NullableByteProperty { get; set; }
        public short? NullableInt16Property { get; set; }
        public int? NullableInt32Property { get; set; }
        public long? NullableInt64Property { get; set; }
        public float? NullableSingleProperty { get; set; }
        public double? NullableDoubleProperty { get; set; }
        public bool? NullableBooleanProperty { get; set; }
        public DateTimeOffset? NullableDateTimeOffsetProperty { get; set; }
    }

    [Preserve(AllMembers = true)]
    public class PrimaryKeyCharObject : RealmObject
    {
        [PrimaryKey]
        public char CharProperty { get; set; }
    }

    [Preserve(AllMembers = true)]
    public class PrimaryKeyByteObject : RealmObject
    {
        [PrimaryKey]
        public byte ByteProperty { get; set; }
    }

    [Preserve(AllMembers = true)]
    public class PrimaryKeyInt16Object : RealmObject
    {
        [PrimaryKey]
        public short Int16Property { get; set; }
    }

    [Preserve(AllMembers = true)]
    public class PrimaryKeyInt32Object : RealmObject
    {
        [PrimaryKey]
        public int Int32Property { get; set; }
    }

    [Preserve(AllMembers = true)]
    public class PrimaryKeyInt64Object : RealmObject
    {
        [PrimaryKey]
        public long Int64Property { get; set; }
    }

    [Preserve(AllMembers = true)]
    public class PrimaryKeyStringObject : RealmObject
    {
        [PrimaryKey]
        public string StringProperty { get; set; }
    }

    [Preserve(AllMembers = true)]
    public class ClassWithUnqueryableMembers : RealmObject
    {
        public string RealPropertyToSatisfyWeaver { get; set; }

        public string PublicField;

        public string PublicMethod()
        {
            return null; 
        }

        [Ignored]
        public string IgnoredProperty { get; set; }

        public string NonAutomaticProperty => null;

        public Person RealmObjectProperty { get; set; }
    }
}
