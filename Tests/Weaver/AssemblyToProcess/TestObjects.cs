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
using MongoDB.Bson;
using Realms;

namespace AssemblyToProcess
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

        public decimal DecimalProperty { get; set; }

        public Decimal128 Decimal128Property { get; set; }

        public bool BooleanProperty { get; set; }

        public string? StringProperty { get; set; }

        public DateTimeOffset DateTimeOffsetProperty { get; set; }

        public ObjectId ObjectIdProperty { get; set; }

        public Guid GuidProperty { get; set; }

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

        public ObjectId? NullableObjectIdProperty { get; set; }

        public Guid? NullableGuidProperty { get; set; }
    }

    public class ListsObject : RealmObject
    {
        public IList<char> CharList { get; } = null!;

        public IList<byte> ByteList { get; } = null!;

        public IList<short> Int16List { get; } = null!;

        public IList<int> Int32List { get; } = null!;

        public IList<long> Int64List { get; } = null!;

        public IList<float> SingleList { get; } = null!;

        public IList<double> DoubleList { get; } = null!;

        public IList<bool> BooleanList { get; } = null!;

        public IList<string?> StringList { get; } = null!;

        public IList<DateTimeOffset> DateTimeOffsetList { get; } = null!;

        public IList<decimal> DecimalList { get; } = null!;

        public IList<Decimal128> Decimal128List { get; } = null!;

        public IList<ObjectId> ObjectIdList { get; } = null!;

        public IList<Guid> GuidList { get; } = null!;

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
    }

    public class SetsObject : RealmObject
    {
        public ISet<char> CharSet { get; } = null!;

        public ISet<byte> ByteSet { get; } = null!;

        public ISet<short> Int16Set { get; } = null!;

        public ISet<int> Int32Set { get; } = null!;

        public ISet<long> Int64Set { get; } = null!;

        public ISet<float> SingleSet { get; } = null!;

        public ISet<double> DoubleSet { get; } = null!;

        public ISet<bool> BooleanSet { get; } = null!;

        public ISet<string?> StringSet { get; } = null!;

        public ISet<DateTimeOffset> DateTimeOffsetSet { get; } = null!;

        public ISet<decimal> DecimalSet { get; } = null!;

        public ISet<Decimal128> Decimal128Set { get; } = null!;

        public ISet<ObjectId> ObjectIdSet { get; } = null!;

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
    }

    public class DictionariesObject : RealmObject
    {
        public IDictionary<string, char> CharDictionary { get; } = null!;

        public IDictionary<string, byte> ByteDictionary { get; } = null!;

        public IDictionary<string, short> Int16Dictionary { get; } = null!;

        public IDictionary<string, int> Int32Dictionary { get; } = null!;

        public IDictionary<string, long> Int64Dictionary { get; } = null!;

        public IDictionary<string, float> SingleDictionary { get; } = null!;

        public IDictionary<string, double> DoubleDictionary { get; } = null!;

        public IDictionary<string, bool> BooleanDictionary { get; } = null!;

        [Required]
        public IDictionary<string, string> StringDictionary { get; } = null!;

        public IDictionary<string, string?> NullableStringDictionary { get; } = null!;

        public IDictionary<string, DateTimeOffset> DateTimeOffsetDictionary { get; } = null!;

        public IDictionary<string, decimal> DecimalDictionary { get; } = null!;

        public IDictionary<string, Decimal128> Decimal128Dictionary { get; } = null!;

        public IDictionary<string, ObjectId> ObjectIdDictionary { get; } = null!;

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
    }

    public class MixOfCollectionsObject : RealmObject
    {
        [PrimaryKey]
        public ObjectId Id { get; set; }

        public IList<int> IntegersList { get; } = null!;

        public IList<AllTypesObject> ObjectList { get; } = null!;

        public IList<EmbeddedAllTypesObject> EmbeddedList { get; } = null!;

        public ISet<int> IntegersSet { get; } = null!;

        public ISet<AllTypesObject> ObjectSet { get; } = null!;

        public ISet<EmbeddedAllTypesObject> EmbeddedSet { get; } = null!;

        public IDictionary<string, int> IntegersDictionary { get; } = null!;

        public IDictionary<string, AllTypesObject?> ObjectDictionary { get; } = null!;

        public IDictionary<string, EmbeddedAllTypesObject?> EmbeddedDictionary { get; } = null!;
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
        public string? StringProperty { get; set; }
    }

    public class PrimaryKeyObjectIdObject : RealmObject
    {
        [PrimaryKey]
        public ObjectId ObjectIdProperty { get; set; }
    }

    public class PrimaryKeyGuidObject : RealmObject
    {
        [PrimaryKey]
        public Guid GuidProperty { get; set; }
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

    public class PrimaryKeyNullableObjectIdObject : RealmObject
    {
        [PrimaryKey]
        public ObjectId? ObjectIdProperty { get; set; }
    }

    public class PrimaryKeyNullableGuidObject : RealmObject
    {
        [PrimaryKey]
        public Guid? GuidProperty { get; set; }
    }

    public class GetterOnlyUnsupportedProperty : RealmObject
    {
        public int IntPropety { get; set; }

        public MyEnum EnumValue { get; }

        public MyEnum LambdaEnum => (MyEnum)IntPropety;

        public enum MyEnum
        {
        }
    }

    public class ObjectWithEmbeddedProperties : RealmObject
    {
        public EmbeddedAllTypesObject? AllTypesObject { get; set; }

        public IList<EmbeddedAllTypesObject> ListOfAllTypesObjects { get; } = null!;
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

        public string? StringProperty { get; set; }

        public DateTimeOffset DateTimeOffsetProperty { get; set; }

        public ObjectId ObjectIdProperty { get; set; }

        public Guid GuidProperty { get; set; }

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

        public ObjectId? NullableObjectIdProperty { get; set; }

        public Guid? NullableGuidProperty { get; set; }
    }

    public class ResearchFacility : RealmObject
    {
        [PrimaryKey, MapTo("_id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        public IList<Sensor> SensorsList { get; } = null!;

        public ISet<Sensor> SensorsSet { get; } = null!;
    }
}
