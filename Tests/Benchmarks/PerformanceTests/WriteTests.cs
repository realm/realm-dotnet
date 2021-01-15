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
    public abstract class WriteTests : BenchmarkBase
    {
        [Params(10, 100)]
        public int ObjectCount { get; set; }

        protected const int MaxObjectCount = 100;

        protected class InsertClass : RealmObject
        {
            public string StringValue { get; set; }

            public bool BoolValue { get; set; }

            public int IntValue { get; set; }

            public DateTimeOffset DateValue { get; set; }

            public byte[] Bytes { get; set; }
        }
    }

    public class EmptyWriteTests : WriteTests
    {
        [Benchmark]
        public void EmptyWrite()
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

        private IDictionary<int, InsertClass[]> _binaryObjects;

        [Params(Bytes_128B, Bytes_1KB, Bytes_1MB)]
        public int BinarySize { get; set; }

        protected override void SeedData()
        {
            _binaryObjects = new[] { Bytes_128B, Bytes_1KB, Bytes_1MB }.ToDictionary(o => o, GenerateBinaryObjects);
        }

        [Benchmark]
        public void Binary()
        {
            _realm.Write(() =>
            {
                _realm.Add(_binaryObjects[BinarySize].Take(ObjectCount));
            });
        }

        private InsertClass[] GenerateBinaryObjects(int binarySize)
            => Enumerable.Range(0, MaxObjectCount)
            .Select(_ => new InsertClass { Bytes = _faker.Random.Bytes(binarySize) })
            .ToArray();
    }

    public class StringWriteTests : WriteTests
    {
        private IDictionary<int, InsertClass[]> _stringObjects;

        [Params(20, 200)]
        public int StringLength { get; set; }

        protected override void SeedData()
        {
            _stringObjects = new[] { 20, 200 }.ToDictionary(o => o, GenerateStringObjects);
        }

        private InsertClass[] GenerateStringObjects(int stringSize)
            => Enumerable.Range(0, MaxObjectCount)
            .Select(_ => new InsertClass { StringValue = _faker.Random.Utf16String(stringSize, stringSize) })
            .ToArray();

        [Benchmark]
        public void Strings()
        {
            var objects = _stringObjects[StringLength];
            _realm.Write(() =>
            {
                _realm.Add(_stringObjects[StringLength].Take(ObjectCount));
            });
        }
    }
}
