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
using MongoDB.Bson;
using Realms;

namespace AssemblyToProcess
{
    public class NonNullableProperties : RealmObject
    {
        public string? String { get; set; }

        public char Char { get; set; }

        public byte Byte { get; set; }

        public short Int16 { get; set; }

        public int Int32 { get; set; }

        public long Int64 { get; set; }

        public float Single { get; set; }

        public double Double { get; set; }

        public bool Boolean { get; set; }

        public byte[]? ByteArray { get; set; }
    }

    public class DateTimeOffsetProperty : RealmObject
    {
        public DateTimeOffset DateTimeOffset { get; set; }
    }

    public class DecimalProperty : RealmObject
    {
        public decimal Decimal { get; set; }
    }

    public class Decimal128Property : RealmObject
    {
        public Decimal128 Decimal128 { get; set; }
    }

    public class ObjectIdProperty : RealmObject
    {
        public ObjectId ObjectId { get; set; }
    }

    public class GuidProperty : RealmObject
    {
        public Guid Guid { get; set; }
    }

    public class NullableProperties : RealmObject
    {
        public char? Char { get; set; }

        public byte? NullableByte { get; set; }

        public short? NullableInt16 { get; set; }

        public int? NullableInt32 { get; set; }

        public long? NullableInt64 { get; set; }

        public float? Single { get; set; }

        public double? Double { get; set; }

        public DateTimeOffset? DateTimeOffset { get; set; }

        public decimal? Decimal { get; set; }

        public Decimal128? Decimal128 { get; set; }

        public bool? Boolean { get; set; }

        public ObjectId? ObjectId { get; set; }

        public Guid? Guid { get; set; }

    }

    public class PKObjectOne : RealmObject
    {
        public string Foo { get; set; } = Guid.NewGuid().ToString();

        [PrimaryKey]
        public int Id { get; set; }
    }

    public class PKObjectTwo : RealmObject
    {
        [PrimaryKey]
        public int Id { get; set; }

        public string Foo { get; set; } = Guid.NewGuid().ToString();
    }

    public class PKObjectThree : RealmObject
    {
        public string Foo { get; set; } = Guid.NewGuid().ToString();

        [PrimaryKey]
        public int Id { get; set; }

        public string Bar { get; set; } = Guid.NewGuid().ToString();
    }

    public class RequiredObject : RealmObject
    {
        [Required]
        public string String { get; set; } = null!;
    }

    public class NonRequiredObject : RealmObject
    {
        public string? String { get; set; }
    }
}
