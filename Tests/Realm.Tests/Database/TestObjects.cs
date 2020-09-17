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
using MongoDB.Bson;

namespace Realms.Tests.Database
{
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

        public decimal DecimalProperty { get; set; }

        public Decimal128 Decimal128Property { get; set; }

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

        public decimal? NullableDecimalProperty { get; set; }

        public Decimal128? NullableDecimal128Property { get; set; }
    }

    public class DecimalsObject : RealmObject
    {
        public decimal DecimalValue { get; set; }

        public Decimal128 Decimal128Value { get; set; }
    }

    public class ListsObject : RealmObject
    {
        public IList<char> CharList { get; }

        public IList<byte> ByteList { get; }

        public IList<short> Int16List { get; }

        public IList<int> Int32List { get; }

        public IList<long> Int64List { get; }

        public IList<float> SingleList { get; }

        public IList<double> DoubleList { get; }

        public IList<bool> BooleanList { get; }

        public IList<string> StringList { get; }

        public IList<byte[]> ByteArrayList { get; }

        public IList<DateTimeOffset> DateTimeOffsetList { get; }

        public IList<char?> NullableCharList { get; }

        public IList<byte?> NullableByteList { get; }

        public IList<short?> NullableInt16List { get; }

        public IList<int?> NullableInt32List { get; }

        public IList<long?> NullableInt64List { get; }

        public IList<float?> NullableSingleList { get; }

        public IList<double?> NullableDoubleList { get; }

        public IList<bool?> NullableBooleanList { get; }

        public IList<DateTimeOffset?> NullableDateTimeOffsetList { get; }

        public IList<RealmInteger<byte>> ByteCounterList { get; }

        public IList<RealmInteger<short>> Int16CounterList { get; }

        public IList<RealmInteger<int>> Int32CounterList { get; }

        public IList<RealmInteger<long>> Int64CounterList { get; }

        public IList<RealmInteger<byte>?> NullableByteCounterList { get; }

        public IList<RealmInteger<short>?> NullableInt16CounterList { get; }

        public IList<RealmInteger<int>?> NullableInt32CounterList { get; }

        public IList<RealmInteger<long>?> NullableInt64CounterList { get; }
    }

    public class CounterObject : RealmObject
    {
        [PrimaryKey]
        public int Id { get; set; }

        public RealmInteger<byte> ByteProperty { get; set; }

        public RealmInteger<short> Int16Property { get; set; }

        public RealmInteger<int> Int32Property { get; set; }

        public RealmInteger<long> Int64Property { get; set; }

        public RealmInteger<byte>? NullableByteProperty { get; set; }

        public RealmInteger<short>? NullableInt16Property { get; set; }

        public RealmInteger<int>? NullableInt32Property { get; set; }

        public RealmInteger<long>? NullableInt64Property { get; set; }

        public override string ToString() => Id.ToString();
    }

    public class IntPrimaryKeyWithValueObject : RealmObject
    {
        [PrimaryKey]
        public int Id { get; set; }

        public string StringValue { get; set; }
    }

    public class PrimaryKeyCharObject : RealmObject
    {
        [PrimaryKey]
        public char CharProperty { get; set; }
    }

    public class PrimaryKeyByteObject : RealmObject
    {
        [PrimaryKey]
        public byte ByteProperty { get; set; }
    }

    public class PrimaryKeyInt16Object : RealmObject
    {
        [PrimaryKey]
        public short Int16Property { get; set; }
    }

    public class PrimaryKeyInt32Object : RealmObject
    {
        [PrimaryKey]
        public int Int32Property { get; set; }
    }

    public class PrimaryKeyInt64Object : RealmObject
    {
        [PrimaryKey]
        public long Int64Property { get; set; }
    }

    public class PrimaryKeyStringObject : RealmObject
    {
        [PrimaryKey]
        public string StringProperty { get; set; }

        public string Value { get; set; }
    }

    public class RequiredPrimaryKeyStringObject : RealmObject
    {
        [PrimaryKey]
        [Required]
        public string StringProperty { get; set; }

        public string Value { get; set; }
    }

    public class PrimaryKeyNullableCharObject : RealmObject
    {
        [PrimaryKey]
        public char? CharProperty { get; set; }
    }

    public class PrimaryKeyNullableByteObject : RealmObject
    {
        [PrimaryKey]
        public byte? ByteProperty { get; set; }
    }

    public class PrimaryKeyNullableInt16Object : RealmObject
    {
        [PrimaryKey]
        public short? Int16Property { get; set; }
    }

    public class PrimaryKeyNullableInt32Object : RealmObject
    {
        [PrimaryKey]
        public int? Int32Property { get; set; }
    }

    public class PrimaryKeyNullableInt64Object : RealmObject
    {
        [PrimaryKey]
        public long? Int64Property { get; set; }
    }

    public class ClassWithUnqueryableMembers : RealmObject
    {
        public string RealPropertyToSatisfyWeaver { get; set; }

        [SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "This is intentionally a public field.")]
        public string PublicField;

        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "This is intentionally an instance method.")]
        public string PublicMethod()
        {
            return null;
        }

        [Ignored]
        public string IgnoredProperty { get; set; }

        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "This is intentionally an instance property.")]
        public string NonAutomaticProperty => null;

        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "This is intentionally an instance property.")]
        public string PropertyWithOnlyGet
        {
            get
            {
                return null;
            }
        }

        public Person RealmObjectProperty { get; set; }

        public IList<Person> RealmListProperty { get; }

        public string FirstName { get; set; }

        [Backlink(nameof(UnqueryableBacklinks.Parent))]
        public IQueryable<UnqueryableBacklinks> BacklinkProperty { get; }
    }

    public class UnqueryableBacklinks : RealmObject
    {
        public ClassWithUnqueryableMembers Parent { get; set; }
    }

    public class Dog : RealmObject
    {
        public string Name { get; set; }

        public string Color { get; set; }

        public bool Vaccinated { get; set; }

        [Backlink(nameof(Owner.Dogs))]
        public IQueryable<Owner> Owners { get; }
    }

    public class Owner : RealmObject
    {
        public string Name { get; set; }

        public Dog TopDog { get; set; }

        public IList<Dog> Dogs { get; }
    }

    // A copy of Owner that verifies that different objects referring to the same type (Dog)
    // results in the correct backlink count being calculated
    public class Walker : RealmObject
    {
        public string Name { get; set; }

        public Dog TopDog { get; set; }

        public IList<Dog> Dogs { get; }
    }

    public class RequiredStringObject : RealmObject
    {
        [Required]
        public string String { get; set; }
    }

    public class ContainerObject : RealmObject
    {
        public IList<IntPropertyObject> Items { get; }
    }

    public class IntPropertyObject : RealmObject
    {
        public int Int { get; set; }
    }

    public class RecursiveBacklinksObject : RealmObject
    {
        public int Id { get; set; }

        public RecursiveBacklinksObject Parent { get; set; }

        [Backlink(nameof(Parent))]
        public IQueryable<RecursiveBacklinksObject> Children { get; }
    }

    public class RemappedPropertiesObject : RealmObject
    {
        [PrimaryKey]
        [MapTo("id")]
        public int Id { get; set; }

        [MapTo("name")]
        public string Name { get; set; }
    }

    [MapTo("__RemappedTypeObject")]
    public class RemappedTypeObject : RealmObject
    {
        [PrimaryKey]
        [MapTo("__id")]
        public int Id { get; set; }

        public string StringValue { get; set; }

        public RemappedTypeObject NormalLink { get; set; }

        [MapTo("__mappedLink")]
        public RemappedTypeObject MappedLink { get; set; }

        public IList<RemappedTypeObject> NormalList { get; }

        [MapTo("__mappedList")]
        public IList<RemappedTypeObject> MappedList { get; }

        [Backlink(nameof(NormalLink))]
        public IQueryable<RemappedTypeObject> NormalBacklink { get; }

        [Backlink(nameof(MappedLink))]
        [MapTo("__mappedBacklink")]
        public IQueryable<RemappedTypeObject> MappedBacklink { get; }
    }

    public class ObjectWithRequiredStringList : RealmObject
    {
        [Required]
        public IList<string> Strings { get; }
    }
}
