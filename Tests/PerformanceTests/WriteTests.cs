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
    public class WriteTests : BenchmarkBase
    {
        private const int MaxObjectCount = 100;
        private const int Bytes_128B = 128;
        private const int Bytes_1KB = 1024;
        private const int Bytes_1MB = 1024 * 1024;

        private IDictionary<int, InsertClass[]> _stringObjects;
        private IDictionary<int, InsertClass[]> _binaryObjects;

        protected override void SeedData()
        {
            _stringObjects = new[] { 20, 200 }.ToDictionary(o => o, GenerateStringObjects);
            _binaryObjects = new[] { Bytes_128B, Bytes_1KB, Bytes_1MB }.ToDictionary(o => o, GenerateBinaryObjects);
        }

        [Benchmark]
        public void EmptyWrite()
        {
            using var transaction = _realm.BeginWrite();
            transaction.Commit();
        }

        [Benchmark]
        [Arguments(10, 20)]
        [Arguments(100, 20)]
        [Arguments(10, 200)]
        [Arguments(100, 200)]
        public void Strings(int objectCount, int stringLength)
        {
            var objects = _stringObjects[stringLength];
            _realm.Write(() =>
            {
                _realm.Add(_stringObjects[stringLength].Take(objectCount));
            });
        }

        [Benchmark]
        [Arguments(10, Bytes_128B)]
        [Arguments(100, Bytes_128B)]
        [Arguments(10, Bytes_1KB)]
        [Arguments(100, Bytes_1KB)]
        [Arguments(10, Bytes_1MB)]
        [Arguments(100, Bytes_1MB)]
        public void Binary(int objectCount, int binarySize)
        {
            _realm.Write(() =>
            {
                _realm.Add(_binaryObjects[binarySize].Take(objectCount));
            });
        }

        private InsertClass[] GenerateStringObjects(int stringSize)
            => Enumerable.Range(0, MaxObjectCount)
            .Select(_ => new InsertClass { StringValue = _faker.Random.Utf16String(stringSize, stringSize) })
            .ToArray();

        private InsertClass[] GenerateBinaryObjects(int binarySize)
            => Enumerable.Range(0, MaxObjectCount)
            .Select(_ => new InsertClass { Bytes = _faker.Random.Bytes(binarySize) })
            .ToArray();

        private class InsertClass : RealmObject
        {
            public string StringValue { get; set; }

            public bool BoolValue { get; set; }

            public int IntValue { get; set; }

            public DateTimeOffset DateValue { get; set; }

            public byte[] Bytes { get; set; }
        }
    }
}
