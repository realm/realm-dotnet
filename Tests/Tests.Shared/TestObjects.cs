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
using System.Linq;
using Realms;

namespace Tests
{
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
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

        [Required]
        public string RequiredStringProperty { get; set; }

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

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class CounterObject : RealmObject
    {
        public RealmInteger<byte> ByteProperty { get; set; }

        public RealmInteger<short> Int16Property { get; set; }

        public RealmInteger<int> Int32Property { get; set; }

        public RealmInteger<long> Int64Property { get; set; }

        public RealmInteger<byte>? NullableByteProperty { get; set; }

        public RealmInteger<short>? NullableInt16Property { get; set; }

        public RealmInteger<int>? NullableInt32Property { get; set; }

        public RealmInteger<long>? NullableInt64Property { get; set; }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class IntPrimaryKeyWithValueObject : RealmObject
    {
        [PrimaryKey]
        public int Id { get; set; }

        public string StringValue { get; set; }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class PrimaryKeyCharObject : RealmObject
    {
        [PrimaryKey]
        public char CharProperty { get; set; }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class PrimaryKeyByteObject : RealmObject
    {
        [PrimaryKey]
        public byte ByteProperty { get; set; }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class PrimaryKeyInt16Object : RealmObject
    {
        [PrimaryKey]
        public short Int16Property { get; set; }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class PrimaryKeyInt32Object : RealmObject
    {
        [PrimaryKey]
        public int Int32Property { get; set; }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class PrimaryKeyInt64Object : RealmObject
    {
        [PrimaryKey]
        public long Int64Property { get; set; }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class PrimaryKeyStringObject : RealmObject
    {
        [PrimaryKey]
        public string StringProperty { get; set; }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class PrimaryKeyNullableCharObject : RealmObject
    {
        [PrimaryKey]
        public char? CharProperty { get; set; }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class PrimaryKeyNullableByteObject : RealmObject
    {
        [PrimaryKey]
        public byte? ByteProperty { get; set; }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class PrimaryKeyNullableInt16Object : RealmObject
    {
        [PrimaryKey]
        public short? Int16Property { get; set; }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class PrimaryKeyNullableInt32Object : RealmObject
    {
        [PrimaryKey]
        public int? Int32Property { get; set; }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class PrimaryKeyNullableInt64Object : RealmObject
    {
        [PrimaryKey]
        public long? Int64Property { get; set; }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
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

        public string PropertyWithOnlyGet
        {
            get
            {
                return null;
            }
        }

        public Person RealmObjectProperty { get; set; }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class Dog : RealmObject
    {
        public string Name { get; set; }

        public string Color { get; set; }

        public bool Vaccinated { get; set; }

        [Backlink(nameof(Owner.Dogs))]
        public IQueryable<Owner> Owners { get; }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class Owner : RealmObject
    {
        public string Name { get; set; }

        public Dog TopDog { get; set; }

        public IList<Dog> Dogs { get; }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class RequiredStringObject : RealmObject
    {
        [Required]
        public string String { get; set; }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class ContainerObject : RealmObject
    {
        public IList<IntPropertyObject> Items { get; }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class IntPropertyObject : RealmObject
    {
        public int Int { get; set; }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class RecursiveBacklinksObject : RealmObject
    {
        public int Id { get; set; }

        public RecursiveBacklinksObject Parent { get; set; }

        [Backlink(nameof(Parent))]
        public IQueryable<RecursiveBacklinksObject> Children { get; }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class RemappedPropertiesObject : RealmObject
    {
        [PrimaryKey]
        [MapTo("id")]
        public int Id { get; set; }

        [MapTo("name")]
        public string Name { get; set; }
    }
}