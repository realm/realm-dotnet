////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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
using System.Linq;
using BenchmarkDotNet.Attributes;
using Realms;

namespace PerformanceTests
{
    public abstract partial class WriteTests : BenchmarkBase
    {
        [Params(10, 100)]
        public int ObjectCount { get; set; }

        protected const int MaxObjectCount = 100;

        protected IDictionary<int, InsertClass[]> _testObjects = null!;

        protected abstract int[] ObjectSizes { get; }

        protected override void SeedData()
        {
            _testObjects = ObjectSizes.ToDictionary(o => o, GenerateTestObjects);
        }

        protected abstract InsertClass[] GenerateTestObjects(int size);

        protected abstract int GetCurrentSize();

        [Benchmark(Description = "Time to write %ObjectCount% objects of particular size/length and commit the transaction")]
        public void Write()
        {
            _realm.Write(() =>
            {
                _realm.Add(_testObjects[GetCurrentSize()].Take(ObjectCount));
            });
        }

        protected partial class InsertClass : IRealmObject
        {
            public string? StringValue { get; set; }

            public bool BoolValue { get; set; }

            public int IntValue { get; set; }

            public DateTimeOffset DateValue { get; set; }

            public byte[]? Bytes { get; set; }
        }
    }

    public class EmptyWriteTests : BenchmarkBase
    {
        [Benchmark(Description = "Time to commit an empty write transaction")]
        public void Write()
        {
            using var transaction = _realm.BeginWrite();
            transaction.Commit();
        }
    }

    public class BinaryWriteTests : WriteTests
    {
        private const int Bytes_128B = 128;
        private const int Bytes_1KB = 1024;
        private const int Bytes_1MB = 1024 * 1024;

        [Params(Bytes_128B, Bytes_1KB, Bytes_1MB)]
        public int BinarySize { get; set; }

        protected override int[] ObjectSizes => new[] { Bytes_128B, Bytes_1KB, Bytes_1MB };

        protected override InsertClass[] GenerateTestObjects(int size)
            => Enumerable.Range(0, MaxObjectCount)
            .Select(_ => new InsertClass { Bytes = _faker.Random.Bytes(size) })
            .ToArray();

        protected override int GetCurrentSize() => BinarySize;
    }

    public class StringWriteTests : WriteTests
    {
        [Params(20, 200)]
        public int StringLength { get; set; }

        protected override int[] ObjectSizes => new[] { 20, 200 };

        protected override InsertClass[] GenerateTestObjects(int size)
            => Enumerable.Range(0, MaxObjectCount)
            .Select(_ => new InsertClass { StringValue = _faker.Random.Utf16String(size, size) })
            .ToArray();

        protected override int GetCurrentSize() => StringLength;
    }
}
