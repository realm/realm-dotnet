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
using Realms;

namespace PerformanceTests
{
    public partial class QueryTests : BenchmarkBase
    {
        [Params(10, 100, 1000)]
        public int ObjectCount { get; set; }

        protected override void SeedData()
        {
            _realm.Write(() =>
            {
                for (var i = 0; i < ObjectCount; i++)
                {
                    _realm.Add(new QueryClass
                    {
                        StringValue = _faker.Random.Utf16String(40, 40),
                        BoolValue = i % 2 == 0,
                        IntValue = _faker.Random.Int()
                    });
                }
            });
        }

        [Benchmark(Description = "Time to execute Count query that matches half of %ObjectCount% elements")]
        public int Count()
        {
            var expectedBool = _faker.Random.Bool();
            return _realm.All<QueryClass>().Where(c => c.BoolValue == expectedBool).Count();
        }

        [Benchmark(Description = "Time to execute enumerate results of a query that matches half of %ObjectCount% elements")]
        public object Enumerate()
        {
            var expectedBool = _faker.Random.Bool();
            return _realm.All<QueryClass>().Where(c => c.BoolValue == expectedBool).ToArray();
        }

        private partial class QueryClass : IRealmObject
        {
            public string? StringValue { get; set; }

            public bool BoolValue { get; set; }

            public int IntValue { get; set; }

            public DateTimeOffset DateValue { get; set; }
        }
    }
}
