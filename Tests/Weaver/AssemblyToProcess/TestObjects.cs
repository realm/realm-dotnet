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

        public string StringProperty { get; set; }

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
        public IList<char> CharList { get; }

        public IList<byte> ByteList { get; }

        public IList<short> Int16List { get; }

        public IList<int> Int32List { get; }

        public IList<long> Int64List { get; }

        public IList<float> SingleList { get; }

        public IList<double> DoubleList { get; }

        public IList<bool> BooleanList { get; }

        public IList<string> StringList { get; }

        public IList<DateTimeOffset> DateTimeOffsetList { get; }

        public IList<decimal> DecimalList { get; }

        public IList<Decimal128> Decimal128List { get; }

        public IList<ObjectId> ObjectIdList { get; }

        public IList<Guid> GuidList { get; }

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

        public ISet<string> StringSet { get; }

        public ISet<DateTimeOffset> DateTimeOffsetSet { get; }

        public ISet<decimal> DecimalSet { get; }

        public ISet<Decimal128> Decimal128Set { get; }

        public ISet<ObjectId> ObjectIdSet { get; }

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

    public class DictionariesObject : RealmObject
    {
        public IDictionary<string, char> CharDictionary { get; }

        public IDictionary<string, byte> ByteDictionary { get; }

        public IDictionary<string, short> Int16Dictionary { get; }

        public IDictionary<string, int> Int32Dictionary { get; }

        public IDictionary<string, long> Int64Dictionary { get; }

        public IDictionary<string, float> SingleDictionary { get; }

        public IDictionary<string, double> DoubleDictionary { get; }

        public IDictionary<string, bool> BooleanDictionary { get; }

        public IDictionary<string, string> StringDictionary { get; }

        public IDictionary<string, DateTimeOffset> DateTimeOffsetDictionary { get; }

        public IDictionary<string, decimal> DecimalDictionary { get; }

        public IDictionary<string, Decimal128> Decimal128Dictionary { get; }

        public IDictionary<string, ObjectId> ObjectIdDictionary { get; }

        public IDictionary<string, char?> NullableCharDictionary { get; }

        public IDictionary<string, byte?> NullableByteDictionary { get; }

        public IDictionary<string, short?> NullableInt16Dictionary { get; }

        public IDictionary<string, int?> NullableInt32Dictionary { get; }

        public IDictionary<string, long?> NullableInt64Dictionary { get; }

        public IDictionary<string, float?> NullableSingleDictionary { get; }

        public IDictionary<string, double?> NullableDoubleDictionary { get; }

        public IDictionary<string, bool?> NullableBooleanDictionary { get; }

        public IDictionary<string, DateTimeOffset?> NullableDateTimeOffsetDictionary { get; }

        public IDictionary<string, decimal?> NullableDecimalDictionary { get; }

        public IDictionary<string, Decimal128?> NullableDecimal128Dictionary { get; }

        public IDictionary<string, ObjectId?> NullableObjectIdDictionary { get; }

        public IDictionary<string, RealmInteger<byte>> ByteCounterDictionary { get; }

        public IDictionary<string, RealmInteger<short>> Int16CounterDictionary { get; }

        public IDictionary<string, RealmInteger<int>> Int32CounterDictionary { get; }

        public IDictionary<string, RealmInteger<long>> Int64CounterDictionary { get; }

        public IDictionary<string, RealmInteger<byte>?> NullableByteCounterDictionary { get; }

        public IDictionary<string, RealmInteger<short>?> NullableInt16CounterDictionary { get; }

        public IDictionary<string, RealmInteger<int>?> NullableInt32CounterDictionary { get; }

        public IDictionary<string, RealmInteger<long>?> NullableInt64CounterDictionary { get; }
    }

    public class MixOfCollectionsObject : RealmObject
    {
        [PrimaryKey]
        public ObjectId Id { get; set; }

        public IList<int> IntegersList { get; }

        public IList<AllTypesObject> ObjectList { get; }

        public IList<EmbeddedAllTypesObject> EmbeddedList { get; }

        public ISet<int> IntegersSet { get; }

        public ISet<AllTypesObject> ObjectSet { get; }

        public ISet<EmbeddedAllTypesObject> EmbeddedSet { get; }

        public IDictionary<string, int> IntegersDictionary { get; }

        public IDictionary<string, AllTypesObject> ObjectDictionary { get; }

        public IDictionary<string, EmbeddedAllTypesObject> EmbeddedDictionary { get; }
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
        public EmbeddedAllTypesObject AllTypesObject { get; set; }

        public IList<EmbeddedAllTypesObject> ListOfAllTypesObjects { get; }
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
}
