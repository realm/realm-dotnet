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
using Realms.Tests.Database;

namespace Realms.Tests
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

        public ObjectId ObjectIdProperty { get; set; }

        public Guid GuidProperty { get; set; }

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

        public ObjectId? NullableObjectIdProperty { get; set; }

        public Guid? NullableGuidProperty { get; set; }
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

        public IList<decimal> DecimalList { get; }

        public IList<Decimal128> Decimal128List { get; }

        public IList<ObjectId> ObjectIdList { get; }

        public IList<Guid> GuidList { get; }

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

        public IList<decimal?> NullableDecimalList { get; }

        public IList<Decimal128?> NullableDecimal128List { get; }

        public IList<ObjectId?> NullableObjectIdList { get; }

        public IList<Guid?> NullableGuidList { get; }

        public IList<RealmInteger<byte>> ByteCounterList { get; }

        public IList<RealmInteger<short>> Int16CounterList { get; }

        public IList<RealmInteger<int>> Int32CounterList { get; }

        public IList<RealmInteger<long>> Int64CounterList { get; }

        public IList<RealmInteger<byte>?> NullableByteCounterList { get; }

        public IList<RealmInteger<short>?> NullableInt16CounterList { get; }

        public IList<RealmInteger<int>?> NullableInt32CounterList { get; }

        public IList<RealmInteger<long>?> NullableInt64CounterList { get; }
    }

    public class SetsObject : RealmObject
    {
        public ISet<char> CharSet { get; }

        public ISet<byte> ByteSet { get; }

        public ISet<short> Int16Set { get; }

        public ISet<int> Int32Set { get; }

        public ISet<long> Int64Set { get; }

        public ISet<float> SingleSet { get; }

        public ISet<double> DoubleSet { get; }

        public ISet<bool> BooleanSet { get; }

        public ISet<decimal> DecimalSet { get; }

        public ISet<Decimal128> Decimal128Set { get; }

        public ISet<ObjectId> ObjectIdSet { get; }

        [Required]
        public ISet<string> StringSet { get; }

        public ISet<string> NullableStringSet { get; }

        public ISet<byte[]> ByteArraySet { get; }

        public ISet<DateTimeOffset> DateTimeOffsetSet { get; }

        public ISet<char?> NullableCharSet { get; }

        public ISet<byte?> NullableByteSet { get; }

        public ISet<short?> NullableInt16Set { get; }

        public ISet<int?> NullableInt32Set { get; }

        public ISet<long?> NullableInt64Set { get; }

        public ISet<float?> NullableSingleSet { get; }

        public ISet<double?> NullableDoubleSet { get; }

        public ISet<bool?> NullableBooleanSet { get; }

        public ISet<DateTimeOffset?> NullableDateTimeOffsetSet { get; }

        public ISet<decimal?> NullableDecimalSet { get; }

        public ISet<Decimal128?> NullableDecimal128Set { get; }

        public ISet<ObjectId?> NullableObjectIdSet { get; }

        public ISet<RealmInteger<byte>> ByteCounterSet { get; }

        public ISet<RealmInteger<short>> Int16CounterSet { get; }

        public ISet<RealmInteger<int>> Int32CounterSet { get; }

        public ISet<RealmInteger<long>> Int64CounterSet { get; }

        public ISet<RealmInteger<byte>?> NullableByteCounterSet { get; }

        public ISet<RealmInteger<short>?> NullableInt16CounterSet { get; }

        public ISet<RealmInteger<int>?> NullableInt32CounterSet { get; }

        public ISet<RealmInteger<long>?> NullableInt64CounterSet { get; }
    }

    public class CounterObject : RealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
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

    public class ObjectIdPrimaryKeyWithValueObject : RealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        public string StringValue { get; set; }
    }

    public class IntPrimaryKeyWithValueObject : RealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public int Id { get; set; }

        public string StringValue { get; set; }
    }

    public class PrimaryKeyCharObject : RealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public char CharProperty { get; set; }
    }

    public class PrimaryKeyByteObject : RealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public byte ByteProperty { get; set; }
    }

    public class PrimaryKeyInt16Object : RealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public short Int16Property { get; set; }
    }

    public class PrimaryKeyInt32Object : RealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public int Int32Property { get; set; }
    }

    public class PrimaryKeyInt64Object : RealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public long Int64Property { get; set; }
    }

    public class PrimaryKeyStringObject : RealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public string StringProperty { get; set; }

        public string Value { get; set; }
    }

    public class RequiredPrimaryKeyStringObject : RealmObject
    {
        [PrimaryKey]
        [Required]
        [MapTo("_id")]
        public string StringProperty { get; set; }

        public string Value { get; set; }
    }

    public class PrimaryKeyObjectIdObject : RealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public ObjectId ObjectIdProperty { get; set; }
    }

    public class PrimaryKeyGuidObject : RealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public Guid GuidProperty { get; set; }
    }

    public class PrimaryKeyNullableCharObject : RealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public char? CharProperty { get; set; }
    }

    public class PrimaryKeyNullableByteObject : RealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public byte? ByteProperty { get; set; }
    }

    public class PrimaryKeyNullableInt16Object : RealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public short? Int16Property { get; set; }
    }

    public class PrimaryKeyNullableInt32Object : RealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public int? Int32Property { get; set; }
    }

    public class PrimaryKeyNullableInt64Object : RealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public long? Int64Property { get; set; }
    }

    public class PrimaryKeyNullableObjectIdObject : RealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public ObjectId? ObjectIdProperty { get; set; }
    }

    public class PrimaryKeyNullableGuidObject : RealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public Guid? GuidProperty { get; set; }
    }

    public class ClassWithUnqueryableMembers : RealmObject
    {
        public string RealPropertyToSatisfyWeaver { get; set; }

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
        [MapTo("_id")]
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

    public class ObjectWithEmbeddedProperties : RealmObject
    {
        [PrimaryKey]
        public int PrimaryKey { get; set; }

        public EmbeddedAllTypesObject AllTypesObject { get; set; }

        public IList<EmbeddedAllTypesObject> ListOfAllTypesObjects { get; }

        public EmbeddedLevel1 RecursiveObject { get; set; }
    }

    public class EmbeddedAllTypesObject : EmbeddedObject
    {
        public char CharProperty { get; set; }

        public byte ByteProperty { get; set; }

        public short Int16Property { get; set; }

        public int Int32Property { get; set; }

        public long Int64Property { get; set; }

        public float SingleProperty { get; set; }

        public double DoubleProperty { get; set; }

        public decimal DecimalProperty { get; set; }

        public Decimal128 Decimal128Property { get; set; }

        public bool BooleanProperty { get; set; }

        public string StringProperty { get; set; }

        public DateTimeOffset DateTimeOffsetProperty { get; set; }

        public char? NullableCharProperty { get; set; }

        public byte? NullableByteProperty { get; set; }

        public short? NullableInt16Property { get; set; }

        public int? NullableInt32Property { get; set; }

        public long? NullableInt64Property { get; set; }

        public float? NullableSingleProperty { get; set; }

        public double? NullableDoubleProperty { get; set; }

        public bool? NullableBooleanProperty { get; set; }

        public decimal? NullableDecimalProperty { get; set; }

        public Decimal128? NullableDecimal128Property { get; set; }

        public DateTimeOffset? NullableDateTimeOffsetProperty { get; set; }

        public RealmInteger<byte> ByteCounterProperty { get; set; }

        public RealmInteger<short> Int16CounterProperty { get; set; }

        public RealmInteger<int> Int32CounterProperty { get; set; }

        public RealmInteger<long> Int64CounterProperty { get; set; }

        public RealmInteger<byte>? NullableByteCounterProperty { get; set; }

        public RealmInteger<short>? NullableInt16CounterProperty { get; set; }

        public RealmInteger<int>? NullableInt32CounterProperty { get; set; }

        public RealmInteger<long>? NullableInt64CounterProperty { get; set; }

        [Backlink(nameof(ObjectWithEmbeddedProperties.AllTypesObject))]
        public IQueryable<ObjectWithEmbeddedProperties> ContainersObjects { get; }
    }

    public class EmbeddedLevel1 : EmbeddedObject
    {
        public string String { get; set; }

        public EmbeddedLevel2 Child { get; set; }

        public IList<EmbeddedLevel2> Children { get; }
    }

    public class EmbeddedLevel2 : EmbeddedObject
    {
        public string String { get; set; }

        public EmbeddedLevel3 Child { get; set; }

        public IList<EmbeddedLevel3> Children { get; }
    }

    public class EmbeddedLevel3 : EmbeddedObject
    {
        public string String { get; set; }
    }
}
