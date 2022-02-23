////////////////////////////////////////////////////////////////////////////
//
// Copyright 2021 Realm Inc.
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
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using MongoDB.Bson;
using Realms;
using Realms.Schema;

namespace PerformanceTests
{
    public class SchemaTests : BenchmarkBase
    {
        private Type[] _schemaClasses;
        private string _basePath;

        protected override void SeedData()
        {
            _schemaClasses = typeof(SchemaTests).GetNestedTypes().Where(t => typeof(RealmObjectBase).IsAssignableFrom(t)).ToArray();
            _basePath = Path.Combine(Path.GetTempPath(), "schema-bench");
            Directory.CreateDirectory(_basePath);
        }

        [Benchmark(Description = "Time to convert C# classes containing all possible property types to Realm Schema")]
        public RealmSchema CreateSchema()
        {
            return new RealmSchema.Builder(_schemaClasses).Build();
        }

        [Benchmark(Description = "Time to open a Realm with a schema containing all possible property types")]
        public Realm OpenRealm()
        {
            var config = new RealmConfiguration(Path.Combine(_basePath, Guid.NewGuid().ToString()))
            {
                Schema = _schemaClasses
            };

            using var realm = Realm.GetInstance(config);
            return realm;
        }

        protected override void CleanupCore()
        {
            base.CleanupCore();
        }

        [Explicit]
        public class Embedded : EmbeddedObject
        {
            public string SomeProperty { get; set; }

            [Backlink(nameof(LinkTypesObject.EmbeddedLink))]
            public IQueryable<LinkTypesObject> BacklinksLink { get; }
        }

        [Explicit]
        public class CounterTypesObject : RealmObject
        {
            public RealmInteger<byte> ByteCounter { get; set; }

            public RealmInteger<short> ShortCounter { get; set; }

            public RealmInteger<int> IntCounter { get; set; }

            public RealmInteger<long> LongCounter { get; set; }

            public RealmInteger<byte>? OptionalByteCounter { get; set; }

            public RealmInteger<short>? OptionalShortCounter { get; set; }

            public RealmInteger<int>? OptionalIntCounter { get; set; }

            public RealmInteger<long>? OptionalLongCounter { get; set; }
        }

        [Explicit]
        public class LinkTypesObject : RealmObject
        {
            public LinkTypesObject SingleLink { get; set; }

            public IList<RequiredTypesObject> ListLink { get; }

            public ISet<RequiredTypesObject> SetLink { get; }

            public IDictionary<string, RequiredTypesObject> DictionaryLink { get; }

            [Backlink(nameof(SingleLink))]
            public IQueryable<LinkTypesObject> BacklinksLink { get; }

            public Embedded EmbeddedLink { get; set; }

            public IList<Embedded> EmbeddedList { get; }

            public IDictionary<string, Embedded> EmbeddedDict { get; }

            public RealmValue RealmValueProp { get; set; }

            public IList<RealmValue> RealmValueList { get; }

            public ISet<RealmValue> RealmValueSet { get; }

            public IDictionary<string, RealmValue> RealmValueDict { get; }
        }

        [Explicit]
        public class RequiredTypesObject : RealmObject
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
            public string StringProperty { get; set; }

            [Required]
            public byte[] ByteArrayProperty { get; set; }
        }

        [Explicit]
        public class RequiredListTypesObject : RealmObject
        {
            public IList<char> CharProperty { get; }

            public IList<byte> ByteProperty { get; }

            public IList<short> Int16Property { get; }

            public IList<int> Int32Property { get; }

            public IList<long> Int64Property { get; }

            public IList<float> SingleProperty { get; }

            public IList<double> DoubleProperty { get; }

            public IList<bool> BooleanProperty { get; }

            public IList<DateTimeOffset> DateTimeOffsetProperty { get; }

            public IList<decimal> DecimalProperty { get; }

            public IList<Decimal128> Decimal128Property { get; }

            public IList<ObjectId> ObjectIdProperty { get; }

            public IList<Guid> GuidProperty { get; }

            [Required]
            public IList<string> StringProperty { get; }

            [Required]
            public IList<byte[]> ByteArrayProperty { get; }
        }

        [Explicit]
        public class RequiredSetTypesObject : RealmObject
        {
            public ISet<char> CharProperty { get; }

            public ISet<byte> ByteProperty { get; }

            public ISet<short> Int16Property { get; }

            public ISet<int> Int32Property { get; }

            public ISet<long> Int64Property { get; }

            public ISet<float> SingleProperty { get; }

            public ISet<double> DoubleProperty { get; }

            public ISet<bool> BooleanProperty { get; }

            public ISet<DateTimeOffset> DateTimeOffsetProperty { get; }

            public ISet<decimal> DecimalProperty { get; }

            public ISet<Decimal128> Decimal128Property { get; }

            public ISet<ObjectId> ObjectIdProperty { get; }

            public ISet<Guid> GuidProperty { get; }

            [Required]
            public ISet<string> StringProperty { get; }

            [Required]
            public ISet<byte[]> ByteArrayProperty { get; }
        }

        [Explicit]
        public class RequiredDictionaryTypesObject : RealmObject
        {
            public IDictionary<string, char> CharProperty { get; }

            public IDictionary<string, byte> ByteProperty { get; }

            public IDictionary<string, short> Int16Property { get; }

            public IDictionary<string, int> Int32Property { get; }

            public IDictionary<string, long> Int64Property { get; }

            public IDictionary<string, float> SingleProperty { get; }

            public IDictionary<string, double> DoubleProperty { get; }

            public IDictionary<string, bool> BooleanProperty { get; }

            public IDictionary<string, DateTimeOffset> DateTimeOffsetProperty { get; }

            public IDictionary<string, decimal> DecimalProperty { get; }

            public IDictionary<string, Decimal128> Decimal128Property { get; }

            public IDictionary<string, ObjectId> ObjectIdProperty { get; }

            public IDictionary<string, Guid> GuidProperty { get; }

            [Required]
            public IDictionary<string, string> StringProperty { get; }

            [Required]
            public IDictionary<string, byte[]> ByteArrayProperty { get; }
        }

        [Explicit]
        public class OptionalTypesObject : RealmObject
        {
            public char? CharProperty { get; set; }

            public byte? ByteProperty { get; set; }

            public short? Int16Property { get; set; }

            public int? Int32Property { get; set; }

            public long? Int64Property { get; set; }

            public float? SingleProperty { get; set; }

            public double? DoubleProperty { get; set; }

            public bool? BooleanProperty { get; set; }

            public DateTimeOffset? DateTimeOffsetProperty { get; set; }

            public decimal? DecimalProperty { get; set; }

            public Decimal128? Decimal128Property { get; set; }

            public ObjectId? ObjectIdProperty { get; set; }

            public Guid? GuidProperty { get; set; }

            public string StringProperty { get; set; }

            public byte[] ByteArrayProperty { get; set; }
        }

        [Explicit]
        public class OptionalListTypesObject : RealmObject
        {
            public IList<char?> CharProperty { get; }

            public IList<byte?> ByteProperty { get; }

            public IList<short?> Int16Property { get; }

            public IList<int?> Int32Property { get; }

            public IList<long?> Int64Property { get; }

            public IList<float?> SingleProperty { get; }

            public IList<double?> DoubleProperty { get; }

            public IList<bool?> BooleanProperty { get; }

            public IList<DateTimeOffset?> DateTimeOffsetProperty { get; }

            public IList<decimal?> DecimalProperty { get; }

            public IList<Decimal128?> Decimal128Property { get; }

            public IList<ObjectId?> ObjectIdProperty { get; }

            public IList<Guid?> GuidProperty { get; }

            public IList<string> StringProperty { get; }

            public IList<byte[]> ByteArrayProperty { get; }
        }

        [Explicit]
        public class OptionalSetTypesObject : RealmObject
        {
            public ISet<char?> CharProperty { get; }

            public ISet<byte?> ByteProperty { get; }

            public ISet<short?> Int16Property { get; }

            public ISet<int?> Int32Property { get; }

            public ISet<long?> Int64Property { get; }

            public ISet<float?> SingleProperty { get; }

            public ISet<double?> DoubleProperty { get; }

            public ISet<bool?> BooleanProperty { get; }

            public ISet<DateTimeOffset?> DateTimeOffsetProperty { get; }

            public ISet<decimal?> DecimalProperty { get; }

            public ISet<Decimal128?> Decimal128Property { get; }

            public ISet<ObjectId?> ObjectIdProperty { get; }

            public ISet<Guid?> GuidProperty { get; }

            public ISet<string> StringProperty { get; }

            public ISet<byte[]> ByteArrayProperty { get; }
        }

        [Explicit]
        public class OptionalDictionaryTypesObject : RealmObject
        {
            public IDictionary<string, char?> CharProperty { get; }

            public IDictionary<string, byte?> ByteProperty { get; }

            public IDictionary<string, short?> Int16Property { get; }

            public IDictionary<string, int?> Int32Property { get; }

            public IDictionary<string, long?> Int64Property { get; }

            public IDictionary<string, float?> SingleProperty { get; }

            public IDictionary<string, double?> DoubleProperty { get; }

            public IDictionary<string, bool?> BooleanProperty { get; }

            public IDictionary<string, DateTimeOffset?> DateTimeOffsetProperty { get; }

            public IDictionary<string, decimal?> DecimalProperty { get; }

            public IDictionary<string, Decimal128?> Decimal128Property { get; }

            public IDictionary<string, ObjectId?> ObjectIdProperty { get; }

            public IDictionary<string, Guid?> GuidProperty { get; }

            public IDictionary<string, string> StringProperty { get; }

            public IDictionary<string, byte[]> ByteArrayProperty { get; }
        }
    }
}
