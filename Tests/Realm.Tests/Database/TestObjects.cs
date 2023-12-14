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
#if TEST_WEAVER
using TestEmbeddedObject = Realms.EmbeddedObject;
using TestRealmObject = Realms.RealmObject;
#else
using TestEmbeddedObject = Realms.IEmbeddedObject;
using TestRealmObject = Realms.IRealmObject;
#endif

namespace Realms.Tests
{
    public partial class AllTypesObject : TestRealmObject
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

#if TEST_WEAVER
        [Required]
#endif
        public string RequiredStringProperty { get; set; } = string.Empty;

        public string? StringProperty { get; set; }

        public byte[]? ByteArrayProperty { get; set; }

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

        public RealmInteger<byte> ByteCounterProperty { get; set; }

        public RealmInteger<short> Int16CounterProperty { get; set; }

        public RealmInteger<int> Int32CounterProperty { get; set; }

        public RealmInteger<long> Int64CounterProperty { get; set; }

        public RealmValue RealmValueProperty { get; set; }
    }

    public partial class DecimalsObject : TestRealmObject
    {
        public decimal DecimalValue { get; set; }

        public Decimal128 Decimal128Value { get; set; }
    }

    public partial class ListsObject : TestRealmObject
    {
        public IList<char> CharList { get; } = null!;

        public IList<byte> ByteList { get; } = null!;

        public IList<short> Int16List { get; } = null!;

        public IList<int> Int32List { get; } = null!;

        public IList<long> Int64List { get; } = null!;

        public IList<float> SingleList { get; } = null!;

        public IList<double> DoubleList { get; } = null!;

        public IList<bool> BooleanList { get; } = null!;

        public IList<decimal> DecimalList { get; } = null!;

        public IList<Decimal128> Decimal128List { get; } = null!;

        public IList<ObjectId> ObjectIdList { get; } = null!;

        public IList<Guid> GuidList { get; } = null!;

#if TEST_WEAVER
        [Required]
#endif
        public IList<string> StringList { get; } = null!;

#if TEST_WEAVER
        [Required]
#endif
        public IList<byte[]> ByteArrayList { get; } = null!;

        public IList<DateTimeOffset> DateTimeOffsetList { get; } = null!;

        public IList<char?> NullableCharList { get; } = null!;

        public IList<byte?> NullableByteList { get; } = null!;

        public IList<short?> NullableInt16List { get; } = null!;

        public IList<int?> NullableInt32List { get; } = null!;

        public IList<long?> NullableInt64List { get; } = null!;

        public IList<float?> NullableSingleList { get; } = null!;

        public IList<double?> NullableDoubleList { get; } = null!;

        public IList<bool?> NullableBooleanList { get; } = null!;

        public IList<DateTimeOffset?> NullableDateTimeOffsetList { get; } = null!;

        public IList<decimal?> NullableDecimalList { get; } = null!;

        public IList<Decimal128?> NullableDecimal128List { get; } = null!;

        public IList<ObjectId?> NullableObjectIdList { get; } = null!;

        public IList<Guid?> NullableGuidList { get; } = null!;

        public IList<string?> NullableStringList { get; } = null!;

        public IList<byte[]?> NullableByteArrayList { get; } = null!;

        public IList<RealmValue> RealmValueList { get; } = null!;
    }

    public partial class CollectionsObject : TestRealmObject
    {
        public ISet<char> CharSet { get; } = null!;

        public ISet<byte> ByteSet { get; } = null!;

        public ISet<short> Int16Set { get; } = null!;

        public ISet<int> Int32Set { get; } = null!;

        public ISet<long> Int64Set { get; } = null!;

        public ISet<float> SingleSet { get; } = null!;

        public ISet<double> DoubleSet { get; } = null!;

        public ISet<bool> BooleanSet { get; } = null!;

        public ISet<decimal> DecimalSet { get; } = null!;

        public ISet<Decimal128> Decimal128Set { get; } = null!;

        public ISet<ObjectId> ObjectIdSet { get; } = null!;

#if TEST_WEAVER
        [Required]
#endif
        public ISet<string> StringSet { get; } = null!;

        public ISet<string?> NullableStringSet { get; } = null!;

#if TEST_WEAVER
        [Required]
#endif
        public ISet<byte[]> ByteArraySet { get; } = null!;

        public ISet<byte[]?> NullableByteArraySet { get; } = null!;

        public ISet<DateTimeOffset> DateTimeOffsetSet { get; } = null!;

        public ISet<char?> NullableCharSet { get; } = null!;

        public ISet<byte?> NullableByteSet { get; } = null!;

        public ISet<short?> NullableInt16Set { get; } = null!;

        public ISet<int?> NullableInt32Set { get; } = null!;

        public ISet<long?> NullableInt64Set { get; } = null!;

        public ISet<float?> NullableSingleSet { get; } = null!;

        public ISet<double?> NullableDoubleSet { get; } = null!;

        public ISet<bool?> NullableBooleanSet { get; } = null!;

        public ISet<DateTimeOffset?> NullableDateTimeOffsetSet { get; } = null!;

        public ISet<decimal?> NullableDecimalSet { get; } = null!;

        public ISet<Decimal128?> NullableDecimal128Set { get; } = null!;

        public ISet<ObjectId?> NullableObjectIdSet { get; } = null!;

        public ISet<IntPropertyObject> ObjectSet { get; } = null!;

        public ISet<RealmValue> RealmValueSet { get; } = null!;

        public IList<char> CharList { get; } = null!;

        public IList<byte> ByteList { get; } = null!;

        public IList<short> Int16List { get; } = null!;

        public IList<int> Int32List { get; } = null!;

        public IList<long> Int64List { get; } = null!;

        public IList<float> SingleList { get; } = null!;

        public IList<double> DoubleList { get; } = null!;

        public IList<bool> BooleanList { get; } = null!;

        public IList<decimal> DecimalList { get; } = null!;

        public IList<Decimal128> Decimal128List { get; } = null!;

        public IList<ObjectId> ObjectIdList { get; } = null!;

#if TEST_WEAVER
        [Required]
#endif
        public IList<string> StringList { get; } = null!;

        public IList<string?> NullableStringList { get; } = null!;

#if TEST_WEAVER
        [Required]
#endif
        public IList<byte[]> ByteArrayList { get; } = null!;

        public IList<byte[]?> NullableByteArrayList { get; } = null!;

        public IList<DateTimeOffset> DateTimeOffsetList { get; } = null!;

        public IList<char?> NullableCharList { get; } = null!;

        public IList<byte?> NullableByteList { get; } = null!;

        public IList<short?> NullableInt16List { get; } = null!;

        public IList<int?> NullableInt32List { get; } = null!;

        public IList<long?> NullableInt64List { get; } = null!;

        public IList<float?> NullableSingleList { get; } = null!;

        public IList<double?> NullableDoubleList { get; } = null!;

        public IList<bool?> NullableBooleanList { get; } = null!;

        public IList<DateTimeOffset?> NullableDateTimeOffsetList { get; } = null!;

        public IList<decimal?> NullableDecimalList { get; } = null!;

        public IList<Decimal128?> NullableDecimal128List { get; } = null!;

        public IList<ObjectId?> NullableObjectIdList { get; } = null!;

        public IList<IntPropertyObject> ObjectList { get; } = null!;

        public IList<EmbeddedIntPropertyObject> EmbeddedObjectList { get; } = null!;

        public IList<RealmValue> RealmValueList { get; } = null!;

        public IDictionary<string, char> CharDict { get; } = null!;

        public IDictionary<string, byte> ByteDict { get; } = null!;

        public IDictionary<string, short> Int16Dict { get; } = null!;

        public IDictionary<string, int> Int32Dict { get; } = null!;

        public IDictionary<string, long> Int64Dict { get; } = null!;

        public IDictionary<string, float> SingleDict { get; } = null!;

        public IDictionary<string, double> DoubleDict { get; } = null!;

        public IDictionary<string, bool> BooleanDict { get; } = null!;

        public IDictionary<string, decimal> DecimalDict { get; } = null!;

        public IDictionary<string, Decimal128> Decimal128Dict { get; } = null!;

        public IDictionary<string, ObjectId> ObjectIdDict { get; } = null!;

#if TEST_WEAVER
        [Required]
#endif
        public IDictionary<string, string> StringDict { get; } = null!;

        public IDictionary<string, string?> NullableStringDict { get; } = null!;

#if TEST_WEAVER
        [Required]
#endif
        public IDictionary<string, byte[]> ByteArrayDict { get; } = null!;

        public IDictionary<string, byte[]?> NullableByteArrayDict { get; } = null!;

        public IDictionary<string, DateTimeOffset> DateTimeOffsetDict { get; } = null!;

        public IDictionary<string, char?> NullableCharDict { get; } = null!;

        public IDictionary<string, byte?> NullableByteDict { get; } = null!;

        public IDictionary<string, short?> NullableInt16Dict { get; } = null!;

        public IDictionary<string, int?> NullableInt32Dict { get; } = null!;

        public IDictionary<string, long?> NullableInt64Dict { get; } = null!;

        public IDictionary<string, float?> NullableSingleDict { get; } = null!;

        public IDictionary<string, double?> NullableDoubleDict { get; } = null!;

        public IDictionary<string, bool?> NullableBooleanDict { get; } = null!;

        public IDictionary<string, DateTimeOffset?> NullableDateTimeOffsetDict { get; } = null!;

        public IDictionary<string, decimal?> NullableDecimalDict { get; } = null!;

        public IDictionary<string, Decimal128?> NullableDecimal128Dict { get; } = null!;

        public IDictionary<string, ObjectId?> NullableObjectIdDict { get; } = null!;

        public IDictionary<string, IntPropertyObject?> ObjectDict { get; } = null!;

        public IDictionary<string, RealmValue> RealmValueDict { get; } = null!;
    }

    // This is a stripped-down version of SetsObject because Sync doesn't support
    // collections of nullable primitives
    public partial class SyncCollectionsObject : TestRealmObject
    {
        [MapTo("_id")]
        [PrimaryKey]
        public ObjectId Id { get; private set; } = ObjectId.GenerateNewId();

        public Guid GuidProperty { get; set; }

        public IList<char> CharList { get; } = null!;

        public IList<byte> ByteList { get; } = null!;

        public IList<short> Int16List { get; } = null!;

        public IList<int> Int32List { get; } = null!;

        public IList<long> Int64List { get; } = null!;

        public IList<float> FloatList { get; } = null!;

        public IList<double> DoubleList { get; } = null!;

        public IList<bool> BooleanList { get; } = null!;

        public IList<decimal> DecimalList { get; } = null!;

        public IList<Decimal128> Decimal128List { get; } = null!;

        public IList<ObjectId> ObjectIdList { get; } = null!;

#if TEST_WEAVER
        [Required]
#endif
        public IList<string> StringList { get; } = null!;

#if TEST_WEAVER
        [Required]
#endif
        public IList<byte[]> ByteArrayList { get; } = null!;

        public IList<DateTimeOffset> DateTimeOffsetList { get; } = null!;

        public IList<IntPropertyObject> ObjectList { get; } = null!;

        public IList<EmbeddedIntPropertyObject> EmbeddedObjectList { get; } = null!;

        public IList<RealmValue> RealmValueList { get; } = null!;

        public ISet<char> CharSet { get; } = null!;

        public ISet<byte> ByteSet { get; } = null!;

        public ISet<short> Int16Set { get; } = null!;

        public ISet<int> Int32Set { get; } = null!;

        public ISet<long> Int64Set { get; } = null!;

        public ISet<float> FloatSet { get; } = null!;

        public ISet<double> DoubleSet { get; } = null!;

        public ISet<bool> BooleanSet { get; } = null!;

        public ISet<decimal> DecimalSet { get; } = null!;

        public ISet<Decimal128> Decimal128Set { get; } = null!;

        public ISet<ObjectId> ObjectIdSet { get; } = null!;

#if TEST_WEAVER
        [Required]
#endif
        public ISet<string> StringSet { get; } = null!;

#if TEST_WEAVER
        [Required]
#endif
        public ISet<byte[]> ByteArraySet { get; } = null!;

        public ISet<DateTimeOffset> DateTimeOffsetSet { get; } = null!;

        public ISet<IntPropertyObject> ObjectSet { get; } = null!;

        public ISet<RealmValue> RealmValueSet { get; } = null!;

        public IDictionary<string, char> CharDict { get; } = null!;

        public IDictionary<string, byte> ByteDict { get; } = null!;

        public IDictionary<string, short> Int16Dict { get; } = null!;

        public IDictionary<string, int> Int32Dict { get; } = null!;

        public IDictionary<string, long> Int64Dict { get; } = null!;

        public IDictionary<string, float> FloatDict { get; } = null!;

        public IDictionary<string, double> DoubleDict { get; } = null!;

        public IDictionary<string, bool> BooleanDict { get; } = null!;

        public IDictionary<string, decimal> DecimalDict { get; } = null!;

        public IDictionary<string, Decimal128> Decimal128Dict { get; } = null!;

        public IDictionary<string, ObjectId> ObjectIdDict { get; } = null!;

#if TEST_WEAVER
        [Required]
#endif
        public IDictionary<string, string> StringDict { get; } = null!;

#if TEST_WEAVER
        [Required]
#endif
        public IDictionary<string, byte[]> ByteArrayDict { get; } = null!;

        public IDictionary<string, DateTimeOffset> DateTimeOffsetDict { get; } = null!;

        public IDictionary<string, IntPropertyObject?> ObjectDict { get; } = null!;

        public IDictionary<string, EmbeddedIntPropertyObject?> EmbeddedObjectDict { get; } = null!;

        public IDictionary<string, RealmValue> RealmValueDict { get; } = null!;
    }

    // This is a stripped-down version of SetsObject because Sync doesn't support
    // collections of nullable primitives
    public partial class SyncAllTypesObject : TestRealmObject
    {
        [MapTo("_id")]
        [PrimaryKey]
        public ObjectId Id { get; private set; } = ObjectId.GenerateNewId();

        public char CharProperty { get; set; }

        public byte ByteProperty { get; set; }

        public short Int16Property { get; set; }

        public int Int32Property { get; set; }

        public long Int64Property { get; set; }

        public float FloatProperty { get; set; }

        public double DoubleProperty { get; set; }

        public bool BooleanProperty { get; set; }

        public DateTimeOffset DateTimeOffsetProperty { get; set; }

        public decimal DecimalProperty { get; set; }

        public Decimal128 Decimal128Property { get; set; }

        public ObjectId ObjectIdProperty { get; set; }

        public Guid GuidProperty { get; set; }

        public string? StringProperty { get; set; }

        public byte[]? ByteArrayProperty { get; set; }

        public RealmValue RealmValueProperty { get; set; }

        public IntPropertyObject? ObjectProperty { get; set; }

        public EmbeddedIntPropertyObject? EmbeddedObjectProperty { get; set; }
    }

    public partial class DictionariesObject : TestRealmObject
    {
        public IDictionary<string, char> CharDictionary { get; } = null!;

        public IDictionary<string, byte> ByteDictionary { get; } = null!;

        public IDictionary<string, short> Int16Dictionary { get; } = null!;

        public IDictionary<string, int> Int32Dictionary { get; } = null!;

        public IDictionary<string, long> Int64Dictionary { get; } = null!;

        public IDictionary<string, float> SingleDictionary { get; } = null!;

        public IDictionary<string, double> DoubleDictionary { get; } = null!;

        public IDictionary<string, bool> BooleanDictionary { get; } = null!;

        public IDictionary<string, decimal> DecimalDictionary { get; } = null!;

        public IDictionary<string, Decimal128> Decimal128Dictionary { get; } = null!;

        public IDictionary<string, ObjectId> ObjectIdDictionary { get; } = null!;

#if TEST_WEAVER
        [Required]
#endif
        public IDictionary<string, string> StringDictionary { get; } = null!;

        public IDictionary<string, string?> NullableStringDictionary { get; } = null!;

        public IDictionary<string, byte[]> ByteArrayDictionary { get; } = null!;

        public IDictionary<string, DateTimeOffset> DateTimeOffsetDictionary { get; } = null!;

        public IDictionary<string, char?> NullableCharDictionary { get; } = null!;

        public IDictionary<string, byte?> NullableByteDictionary { get; } = null!;

        public IDictionary<string, short?> NullableInt16Dictionary { get; } = null!;

        public IDictionary<string, int?> NullableInt32Dictionary { get; } = null!;

        public IDictionary<string, long?> NullableInt64Dictionary { get; } = null!;

        public IDictionary<string, float?> NullableSingleDictionary { get; } = null!;

        public IDictionary<string, double?> NullableDoubleDictionary { get; } = null!;

        public IDictionary<string, bool?> NullableBooleanDictionary { get; } = null!;

        public IDictionary<string, DateTimeOffset?> NullableDateTimeOffsetDictionary { get; } = null!;

        public IDictionary<string, decimal?> NullableDecimalDictionary { get; } = null!;

        public IDictionary<string, Decimal128?> NullableDecimal128Dictionary { get; } = null!;

        public IDictionary<string, ObjectId?> NullableObjectIdDictionary { get; } = null!;

        public IDictionary<string, byte[]?> NullableBinaryDictionary { get; } = null!;

#if TEST_WEAVER
        [Required]
#endif
        public IDictionary<string, byte[]> BinaryDictionary { get; } = null!;

        public IDictionary<string, IntPropertyObject?> ObjectDictionary { get; } = null!;

        public IDictionary<string, EmbeddedIntPropertyObject?> EmbeddedObjectDictionary { get; } = null!;

        public IDictionary<string, RealmValue> RealmValueDictionary { get; } = null!;
    }

    public partial class CounterObject : TestRealmObject
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

    public partial class ObjectIdPrimaryKeyWithValueObject : TestRealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        public string? StringValue { get; set; }
    }

    public partial class IntPrimaryKeyWithValueObject : TestRealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public int Id { get; set; }

        public string? StringValue { get; set; }
    }

    public partial class PrimaryKeyCharObject : TestRealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public char Id { get; set; }
    }

    public partial class PrimaryKeyByteObject : TestRealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public byte Id { get; set; }
    }

    public partial class PrimaryKeyInt16Object : TestRealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public short Id { get; set; }
    }

    public partial class PrimaryKeyInt32Object : TestRealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public int Id { get; set; }
    }

    public partial class PrimaryKeyInt64Object : TestRealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public long Id { get; set; }
    }

    public partial class PrimaryKeyStringObject : TestRealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public string? Id { get; set; }

        public string? Value { get; set; }
    }

    public partial class RequiredPrimaryKeyStringObject : TestRealmObject
    {
        [PrimaryKey]
#if TEST_WEAVER
        [Required]
#endif
        [MapTo("_id")]
        public string Id { get; set; } = null!;

        public string? Value { get; set; }
    }

    public partial class PrimaryKeyObjectIdObject : TestRealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public ObjectId Id { get; set; }
    }

    public partial class PrimaryKeyGuidObject : TestRealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public Guid Id { get; set; }
    }

    public partial class PrimaryKeyNullableCharObject : TestRealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public char? Id { get; set; }
    }

    public partial class PrimaryKeyNullableByteObject : TestRealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public byte? Id { get; set; }
    }

    public partial class PrimaryKeyNullableInt16Object : TestRealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public short? Id { get; set; }
    }

    public partial class PrimaryKeyNullableInt32Object : TestRealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public int? Id { get; set; }
    }

    public partial class PrimaryKeyNullableInt64Object : TestRealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public long? Id { get; set; }
    }

    public partial class PrimaryKeyNullableObjectIdObject : TestRealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public ObjectId? Id { get; set; }
    }

    public partial class PrimaryKeyNullableGuidObject : TestRealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public Guid? Id { get; set; }
    }

    public partial class ClassWithUnqueryableMembers : TestRealmObject
    {
        public string? RealPropertyToSatisfyWeaver { get; set; }

        public string? PublicField;

        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "This is intentionally an instance method.")]
        public string? PublicMethod()
        {
            return null;
        }

        [Ignored]
        public string? IgnoredProperty { get; set; }

        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "This is intentionally an instance property.")]
        public string? NonAutomaticProperty => null;

        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "This is intentionally an instance property.")]
        public string? PropertyWithOnlyGet
        {
            get
            {
                return null;
            }
        }

        public Person? RealmObjectProperty { get; set; }

        public IList<Person> RealmListProperty { get; } = null!;

        public string? FirstName { get; set; }

        [Backlink(nameof(UnqueryableBacklinks.Parent))]
        public IQueryable<UnqueryableBacklinks> BacklinkProperty { get; } = null!;

        public static string? StaticProperty { get; set; }
    }

    public partial class UnqueryableBacklinks : TestRealmObject
    {
        public ClassWithUnqueryableMembers? Parent { get; set; }
    }

    public partial class Dog : TestRealmObject
    {
        public string? Name { get; set; }

        public string? Color { get; set; }

        public bool Vaccinated { get; set; }

        public int Age { get; set; }

        [Backlink(nameof(Owner.ListOfDogs))]
        public IQueryable<Owner> Owners { get; } = null!;
    }

    public partial class Owner : TestRealmObject
    {
        public string? Name { get; set; }

        public Dog? TopDog { get; set; }

        public IList<Dog> ListOfDogs { get; } = null!;

        public ISet<Dog> SetOfDogs { get; } = null!;

        public IDictionary<string, Dog?> DictOfDogs { get; } = null!;
    }

    // A copy of Owner that verifies that different objects referring to the same type (Dog)
    // results in the correct backlink count being calculated
    public partial class Walker : TestRealmObject
    {
        public string? Name { get; set; }

        public Dog? TopDog { get; set; }

        public IList<Dog> ListOfDogs { get; } = null!;

        public ISet<Dog> SetOfDogs { get; } = null!;
    }

    public partial class RequiredStringObject : TestRealmObject
    {
#if TEST_WEAVER
        [Required]
#endif
        public string String { get; set; } = null!;
    }

    public partial class ContainerObject : TestRealmObject
    {
        public IList<IntPropertyObject> Items { get; } = null!;
    }

    public partial class IntPropertyObject : TestRealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public ObjectId Id { get; private set; } = ObjectId.GenerateNewId();

        public int Int { get; set; }

        public Guid GuidProperty { get; set; }

        [Backlink(nameof(SyncCollectionsObject.ObjectList))]
        public IQueryable<SyncCollectionsObject> ContainingCollections { get; } = null!;

        public override string ToString() => $"Int: {Int}";
    }

    public partial class ObjectWithObjectProperties : TestRealmObject
    {
        public IntPropertyObject? StandaloneObject { get; set; }

        public EmbeddedIntPropertyObject? EmbeddedObject { get; set; }
    }

    public partial class EmbeddedIntPropertyObject : TestEmbeddedObject
    {
        public int Int { get; set; }

        public override string ToString() => $"Int: {Int}";
    }

    public partial class RecursiveBacklinksObject : TestRealmObject
    {
        public int Id { get; set; }

        public RecursiveBacklinksObject? Parent { get; set; }

        [Backlink(nameof(Parent))]
        public IQueryable<RecursiveBacklinksObject> Children { get; } = null!;
    }

    public partial class RemappedPropertiesObject : TestRealmObject
    {
        [PrimaryKey]
        [MapTo("id")]
        public int Id { get; set; }

        [MapTo("name")]
        public string? Name { get; set; }
    }

    [MapTo("__RemappedTypeObject")]
    public partial class RemappedTypeObject : TestRealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public int Id { get; set; }

        public string? StringValue { get; set; }

        public RemappedTypeObject? NormalLink { get; set; }

        [MapTo("__mappedLink")]
        public RemappedTypeObject? MappedLink { get; set; }

        public IList<RemappedTypeObject> NormalList { get; } = null!;

        [MapTo("__mappedList")]
        public IList<RemappedTypeObject> MappedList { get; } = null!;

        [Backlink(nameof(NormalLink))]
        public IQueryable<RemappedTypeObject> NormalBacklink { get; } = null!;

        [Backlink(nameof(MappedLink))]
        [MapTo("__mappedBacklink")]
        public IQueryable<RemappedTypeObject> MappedBacklink { get; } = null!;
    }

    public partial class ObjectWithRequiredStringList : TestRealmObject
    {
#if TEST_WEAVER
        [Required]
#endif
        public IList<string> Strings { get; } = null!;
    }

    public partial class ObjectWithEmbeddedProperties : TestRealmObject
    {
        [PrimaryKey]
        public int PrimaryKey { get; set; }

        public EmbeddedAllTypesObject? AllTypesObject { get; set; }

        public EmbeddedLevel1? RecursiveObject { get; set; }

        public IList<EmbeddedAllTypesObject> ListOfAllTypesObjects { get; } = null!;

        public IDictionary<string, EmbeddedAllTypesObject?> DictionaryOfAllTypesObjects { get; } = null!;
    }

    public partial class EmbeddedAllTypesObject : TestEmbeddedObject
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

        public string? StringProperty { get; set; }

        public DateTimeOffset DateTimeOffsetProperty { get; set; }

        public ObjectId ObjectIdProperty { get; set; }

        public Guid GuidProperty { get; set; }

        public byte[]? ByteArrayProperty { get; set; }

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
        public IQueryable<ObjectWithEmbeddedProperties> ContainersObjects { get; } = null!;
    }

    public partial class EmbeddedLevel1 : TestEmbeddedObject
    {
        public string? String { get; set; }

        public EmbeddedLevel2? Child { get; set; }

        public IList<EmbeddedLevel2> Children { get; } = null!;
    }

    public partial class EmbeddedLevel2 : TestEmbeddedObject
    {
        public string? String { get; set; }

        public EmbeddedLevel3? Child { get; set; }

        public IList<EmbeddedLevel3> Children { get; } = null!;
    }

    public partial class EmbeddedLevel3 : TestEmbeddedObject
    {
        public string? String { get; set; }
    }

    public partial class HugeSyncObject : TestRealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        public byte[]? Data { get; set; }

        public HugeSyncObject()
        {
        }

        public HugeSyncObject(int dataSize)
        {
            var data = new byte[dataSize];
            TestHelpers.Random.NextBytes(data);
            Data = data;
        }
    }

    public partial class RealmValueObject : TestRealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public int Id { get; set; } = TestHelpers.Random.Next();

        public RealmValue RealmValueProperty { get; set; }

        public IList<RealmValue> RealmValueList { get; } = null!;

        public ISet<RealmValue> RealmValueSet { get; } = null!;

        public IDictionary<string, RealmValue> RealmValueDictionary { get; } = null!;

        public IDictionary<string, int> TestDict { get; } = null!;
    }

    public partial class LinksObject : TestRealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        public string Id { get; private set; }

        public int Value { get; set; }

        public LinksObject? Link { get; set; }

        public IList<LinksObject> List { get; } = null!;

        public ISet<LinksObject> Set { get; } = null!;

        public IDictionary<string, LinksObject?> Dictionary { get; } = null!;

        public LinksObject(string id)
        {
            Id = id;
        }
    }

    public partial class ObjectWithFtsIndex : TestRealmObject
    {
        [PrimaryKey]
#if TEST_WEAVER
        [Required]
        public string Title { get; set; } = null!;
#else
        public string Title { get; set; }
#endif

#if TEST_WEAVER
        [Required]
#endif
        [Indexed(IndexType.FullText)]
        public string Summary { get; set; } = string.Empty;

        [Indexed(IndexType.FullText)]
        public string? NullableSummary { get; set; }

        public ObjectWithFtsIndex(string title, string summary)
        {
            Title = title;
            Summary = summary;
            NullableSummary = summary;
        }

#if TEST_WEAVER
        private ObjectWithFtsIndex()
        {
        }
#endif
    }

    [Explicit]
    public partial class PrivatePrimaryKeyObject : TestRealmObject
    {
        [PrimaryKey]
        [MapTo("_id")]
        private string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public string? Value { get; set; }

        public string GetId() => Id;
    }
}
