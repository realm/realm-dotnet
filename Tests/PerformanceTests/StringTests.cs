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

using System.Linq;
using BenchmarkDotNet.Attributes;
using Realms;

namespace PerformanceTests
{
    public class StringTests : BenchmarkBase
    {
        private const int ObjectCount = 100;

        private StringClass _robject;
        private string _newPropertyValue;
        private string _primaryKey;

        [Params(20, 100, 1000)]
        public int StringSize { get; set; }

        protected override void SeedData()
        {
            base.SeedData();

            _realm.Write(() =>
            {
                for (var i = 0; i < ObjectCount; i++)
                {
                    _realm.Add(new StringClass
                    {
                        Value = _faker.Random.Utf16String(StringSize, StringSize)
                    });
                }
            });

            _robject = _realm.All<StringClass>().ElementAt(_faker.Random.Int(0, ObjectCount - 1));
            _newPropertyValue = _faker.Random.Utf16String(StringSize, StringSize);
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
