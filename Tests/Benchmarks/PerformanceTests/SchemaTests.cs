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
    public partial class SchemaTests : BenchmarkBase
    {
        private Type[] _schemaClasses = null!;
        private string _basePath = null!;

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
        public partial class Embedded : IEmbeddedObject
        {
            public string? SomeProperty { get; set; }

            [Backlink(nameof(LinkTypesObject.EmbeddedLink))]
            public IQueryable<LinkTypesObject> BacklinksLink { get; } = null!;
        }

        [Explicit]
        public partial class CounterTypesObject : IRealmObject
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
        public partial class LinkTypesObject : IRealmObject
        {
            public LinkTypesObject? SingleLink { get; set; }

            public IList<RequiredTypesObject> ListLink { get; } = null!;

            public ISet<RequiredTypesObject> SetLink { get; } = null!;

            public IDictionary<string, RequiredTypesObject?> DictionaryLink { get; } = null!;

            [Backlink(nameof(SingleLink))]
            public IQueryable<LinkTypesObject> BacklinksLink { get; } = null!;

            public Embedded? EmbeddedLink { get; set; }

            public IList<Embedded> EmbeddedList { get; } = null!;

            public IDictionary<string, Embedded?> EmbeddedDict { get; } = null!;

            public RealmValue RealmValueProp { get; set; }

            public IList<RealmValue> RealmValueList { get; } = null!;

            public ISet<RealmValue> RealmValueSet { get; } = null!;

            public IDictionary<string, RealmValue> RealmValueDict { get; } = null!;
        }

        [Explicit]
        public partial class RequiredTypesObject : IRealmObject
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

            public string StringProperty { get; set; } = null!;

            public byte[] ByteArrayProperty { get; set; } = null!;
        }

        [Explicit]
        public class RequiredListTypesObject : RealmObject
        {
            public IList<char> CharProperty { get; } = null!;

            public IList<byte> ByteProperty { get; } = null!;

            public IList<short> Int16Property { get; } = null!;

            public IList<int> Int32Property { get; } = null!;

            public IList<long> Int64Property { get; } = null!;

            public IList<float> SingleProperty { get; } = null!;

            public IList<double> DoubleProperty { get; } = null!;

            public IList<bool> BooleanProperty { get; } = null!;

            public IList<DateTimeOffset> DateTimeOffsetProperty { get; } = null!;

            public IList<decimal> DecimalProperty { get; } = null!;

            public IList<Decimal128> Decimal128Property { get; } = null!;

            public IList<ObjectId> ObjectIdProperty { get; } = null!;

            public IList<Guid> GuidProperty { get; } = null!;

            public IList<string> StringProperty { get; } = null!;

            public IList<byte[]> ByteArrayProperty { get; } = null!;
        }

        [Explicit]
        public partial class RequiredSetTypesObject : IRealmObject
        {
            public ISet<char> CharProperty { get; } = null!;

            public ISet<byte> ByteProperty { get; } = null!;

            public ISet<short> Int16Property { get; } = null!;

            public ISet<int> Int32Property { get; } = null!;

            public ISet<long> Int64Property { get; } = null!;

            public ISet<float> SingleProperty { get; } = null!;

            public ISet<double> DoubleProperty { get; } = null!;

            public ISet<bool> BooleanProperty { get; } = null!;

            public ISet<DateTimeOffset> DateTimeOffsetProperty { get; } = null!;

            public ISet<decimal> DecimalProperty { get; } = null!;

            public ISet<Decimal128> Decimal128Property { get; } = null!;

            public ISet<ObjectId> ObjectIdProperty { get; } = null!;

            public ISet<Guid> GuidProperty { get; } = null!;

            public ISet<string> StringProperty { get; } = null!;

            public ISet<byte[]> ByteArrayProperty { get; } = null!;
        }

        [Explicit]
        public partial class RequiredDictionaryTypesObject : IRealmObject
        {
            public IDictionary<string, char> CharProperty { get; } = null!;

            public IDictionary<string, byte> ByteProperty { get; } = null!;

            public IDictionary<string, short> Int16Property { get; } = null!;

            public IDictionary<string, int> Int32Property { get; } = null!;

            public IDictionary<string, long> Int64Property { get; } = null!;

            public IDictionary<string, float> SingleProperty { get; } = null!;

            public IDictionary<string, double> DoubleProperty { get; } = null!;

            public IDictionary<string, bool> BooleanProperty { get; } = null!;

            public IDictionary<string, DateTimeOffset> DateTimeOffsetProperty { get; } = null!;

            public IDictionary<string, decimal> DecimalProperty { get; } = null!;

            public IDictionary<string, Decimal128> Decimal128Property { get; } = null!;

            public IDictionary<string, ObjectId> ObjectIdProperty { get; } = null!;

            public IDictionary<string, Guid> GuidProperty { get; } = null!;

            public IDictionary<string, string> StringProperty { get; } = null!;

            public IDictionary<string, byte[]> ByteArrayProperty { get; } = null!;
        }

        [Explicit]
        public partial class OptionalTypesObject : IRealmObject
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

            public string? StringProperty { get; set; }

            public byte[]? ByteArrayProperty { get; set; }
        }

        [Explicit]
        public partial class OptionalListTypesObject : IRealmObject
        {
            public IList<char?> CharProperty { get; } = null!;

            public IList<byte?> ByteProperty { get; } = null!;

            public IList<short?> Int16Property { get; } = null!;

            public IList<int?> Int32Property { get; } = null!;

            public IList<long?> Int64Property { get; } = null!;

            public IList<float?> SingleProperty { get; } = null!;

            public IList<double?> DoubleProperty { get; } = null!;

            public IList<bool?> BooleanProperty { get; } = null!;

            public IList<DateTimeOffset?> DateTimeOffsetProperty { get; } = null!;

            public IList<decimal?> DecimalProperty { get; } = null!;

            public IList<Decimal128?> Decimal128Property { get; } = null!;

            public IList<ObjectId?> ObjectIdProperty { get; } = null!;

            public IList<Guid?> GuidProperty { get; } = null!;

            public IList<string?> StringProperty { get; } = null!;

            public IList<byte[]?> ByteArrayProperty { get; } = null!;
        }

        [Explicit]
        public partial class OptionalSetTypesObject : IRealmObject
        {
            public ISet<char?> CharProperty { get; } = null!;

            public ISet<byte?> ByteProperty { get; } = null!;

            public ISet<short?> Int16Property { get; } = null!;

            public ISet<int?> Int32Property { get; } = null!;

            public ISet<long?> Int64Property { get; } = null!;

            public ISet<float?> SingleProperty { get; } = null!;

            public ISet<double?> DoubleProperty { get; } = null!;

            public ISet<bool?> BooleanProperty { get; } = null!;

            public ISet<DateTimeOffset?> DateTimeOffsetProperty { get; } = null!;

            public ISet<decimal?> DecimalProperty { get; } = null!;

            public ISet<Decimal128?> Decimal128Property { get; } = null!;

            public ISet<ObjectId?> ObjectIdProperty { get; } = null!;

            public ISet<Guid?> GuidProperty { get; } = null!;

            public ISet<string?> StringProperty { get; } = null!;

            public ISet<byte[]?> ByteArrayProperty { get; } = null!;
        }

        [Explicit]
        public partial class OptionalDictionaryTypesObject : IRealmObject
        {
            public IDictionary<string, char?> CharProperty { get; } = null!;

            public IDictionary<string, byte?> ByteProperty { get; } = null!;

            public IDictionary<string, short?> Int16Property { get; } = null!;

            public IDictionary<string, int?> Int32Property { get; } = null!;

            public IDictionary<string, long?> Int64Property { get; } = null!;

            public IDictionary<string, float?> SingleProperty { get; } = null!;

            public IDictionary<string, double?> DoubleProperty { get; } = null!;

            public IDictionary<string, bool?> BooleanProperty { get; } = null!;

            public IDictionary<string, DateTimeOffset?> DateTimeOffsetProperty { get; } = null!;

            public IDictionary<string, decimal?> DecimalProperty { get; } = null!;

            public IDictionary<string, Decimal128?> Decimal128Property { get; } = null!;

            public IDictionary<string, ObjectId?> ObjectIdProperty { get; } = null!;

            public IDictionary<string, Guid?> GuidProperty { get; } = null!;

            public IDictionary<string, string?> StringProperty { get; } = null!;

            public IDictionary<string, byte[]?> ByteArrayProperty { get; } = null!;
        }
    }
}
