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
using System.Linq;
using BenchmarkDotNet.Attributes;
using Bogus;
using Realms;

namespace PerformanceTests
{
    public class QueryTests : BenchmarkBase
    {
        private int _executionCounter;

        [Params(10, 100, 1000)]
        public int ObjectCount { get; set; }

        protected override void SeedData()
        {
            var dataset = new DataSet();
            _realm.Write(() =>
            {
                for (var i = 0; i < ObjectCount; i++)
                {
                    _realm.Add(new QueryClass
                    {
                        StringValue = dataset.Random.Utf16String(40, 40),
                        BoolValue = i % 2 == 0,
                        IntValue = dataset.Random.Int()
                    });
                }
            });
        }

        [Benchmark]
        public int Count()
        {
            var expectedBool = _executionCounter++ % 2 == 0;
            return _realm.All<QueryClass>().Where(c => c.BoolValue == expectedBool).Count();
        }

        [Benchmark]
        public object Enumerate()
        {
            var expectedBool = _executionCounter++ % 2 == 0;
            return _realm.All<QueryClass>().Where(c => c.BoolValue == expectedBool).ToArray();
        }

        private class QueryClass : RealmObject
        {
            public string StringValue { get; set; }

            public bool BoolValue { get; set; }

            public int IntValue { get; set; }

            public DateTimeOffset DateValue { get; set; }
        }
    }
}
