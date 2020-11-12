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
        public int ObjectCount;

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

        private class QueryClass : RealmObject
        {
            public string StringValue { get; set; }

            public bool BoolValue { get; set; }

            public int IntValue { get; set; }

            public DateTimeOffset DateValue { get; set; }
        }
    }
}
