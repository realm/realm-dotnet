using System.Linq;
using BenchmarkDotNet.Attributes;
using Bogus;
using Realms;

namespace PerformanceTests
{
    public class StringTests : BenchmarkBase
    {
        private StringClass _robject;
        private string _newPropertyValue;
        private string _primaryKey;

        [Params(20, 100, 1000)]
        public int StringSize;

        [Params(10, 100)]
        public int ObjectCount;

        [Params(true, false)]
        public bool IsUtf16;

        protected override void SeedData()
        {
            base.SeedData();

            var dataset = new DataSet(locale: IsUtf16 ? "ja" : "en");
            _realm.Write(() =>
            {
                for (var i = 0; i < ObjectCount; i++)
                {
                    _realm.Add(new StringClass
                    {
                        Value = dataset.Random.Utf16String(StringSize, StringSize)
                    });
                }
            });

            _robject = _realm.All<StringClass>().ElementAt(dataset.Random.Int(0, ObjectCount - 1));
            _newPropertyValue = dataset.Random.Utf16String(StringSize, StringSize);
            _primaryKey = _robject.Value;
        }

        [Benchmark]
        public string GetPropertyValue()
        {
            return _robject.Value;
        }

        [Benchmark]
        public void SetPropertyValue()
        {
            _realm.Write(() =>
            {
                _robject.NonPKValue = _newPropertyValue;
            });
        }

        [Benchmark]
        public RealmObject LookupByPK()
        {
            return _realm.Find<StringClass>(_primaryKey);
        }

        private class StringClass : RealmObject
        {
            [PrimaryKey]
            public string Value { get; set; }

            public string NonPKValue { get; set; }
        }
    }
}
