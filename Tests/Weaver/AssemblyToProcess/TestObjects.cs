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
using MongoDB.Bson;
using Realms;

namespace AssemblyToProcess
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
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
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
    public class GetterOnlyUnsupportedProperty : RealmObject
    {
        public int IntPropety { get; set; }

        public MyEnum EnumValue { get; }

        public MyEnum LambdaEnum => (MyEnum)IntPropety;

        public enum MyEnum
        {
        }
    }
}