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
using System.Reflection;
using BenchmarkDotNet.Attributes;
using Realms;

namespace PerformanceTests
{
    public partial class StringTests : BenchmarkBase
    {
        private const int ObjectCount = 100;

        private StringClass _robject = null!;
        private string[] _newPropertyValues = null!;
        private string[] _primaryKeys = null!;
        private Transaction _transaction = null!;
        private string _temp = null!;
        private IRealmObject? _tempObj;

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
                        Value = _faker.Random.Utf16String(StringSize, StringSize),
                        Value0 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value1 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value2 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value3 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value4 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value5 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value6 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value7 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value8 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value9 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value10 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value11 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value12 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value13 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value14 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value15 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value16 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value17 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value18 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value19 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value20 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value21 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value22 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value23 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value24 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value25 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value26 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value27 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value28 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value29 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value30 = _faker.Random.Utf16String(StringSize, StringSize),
                        Value31 = _faker.Random.Utf16String(StringSize, StringSize),
                    });
                }
            });

            _robject = _realm.All<StringClass>().ElementAt(_faker.Random.Int(0, ObjectCount - 1));

            _primaryKeys = new string[32];
            for (var i = 0; i < _primaryKeys.Length; i++)
            {
                _primaryKeys[i] = _realm.All<StringClass>().ElementAt(_faker.Random.Int(0, ObjectCount - 1)).Value;
            }

            _newPropertyValues = new string[32];
            for (var i = 0; i < _newPropertyValues.Length; i++)
            {
                var prop = typeof(StringClass).GetProperty($"Value{i}", BindingFlags.Public | BindingFlags.Instance)!;
                _newPropertyValues[i] = (string)prop.GetValue(_robject)!;
            }

            _transaction = _realm.BeginWrite();
        }

        protected override void CleanupCore()
        {
            _transaction.Rollback();
        }

        [Benchmark(OperationsPerInvoke = 32, Description = "Time to get a property of type String")]
        public string GetPropertyValue()
        {
            _temp = _robject.Value0;
            _temp = _robject.Value1;
            _temp = _robject.Value2;
            _temp = _robject.Value3;
            _temp = _robject.Value4;
            _temp = _robject.Value5;
            _temp = _robject.Value6;
            _temp = _robject.Value7;
            _temp = _robject.Value8;
            _temp = _robject.Value9;
            _temp = _robject.Value10;
            _temp = _robject.Value11;
            _temp = _robject.Value12;
            _temp = _robject.Value13;
            _temp = _robject.Value14;
            _temp = _robject.Value15;
            _temp = _robject.Value16;
            _temp = _robject.Value17;
            _temp = _robject.Value18;
            _temp = _robject.Value19;
            _temp = _robject.Value20;
            _temp = _robject.Value21;
            _temp = _robject.Value22;
            _temp = _robject.Value23;
            _temp = _robject.Value24;
            _temp = _robject.Value25;
            _temp = _robject.Value26;
            _temp = _robject.Value27;
            _temp = _robject.Value28;
            _temp = _robject.Value29;
            _temp = _robject.Value30;
            _temp = _robject.Value31;
            return _temp;
        }

        [Benchmark(OperationsPerInvoke = 32, Description = "Time to set a property of type String")]
        public void SetPropertyValue()
        {
            _robject.Value0 = _newPropertyValues[0];
            _robject.Value1 = _newPropertyValues[1];
            _robject.Value2 = _newPropertyValues[2];
            _robject.Value3 = _newPropertyValues[3];
            _robject.Value4 = _newPropertyValues[4];
            _robject.Value5 = _newPropertyValues[5];
            _robject.Value6 = _newPropertyValues[6];
            _robject.Value7 = _newPropertyValues[7];
            _robject.Value8 = _newPropertyValues[8];
            _robject.Value9 = _newPropertyValues[9];
            _robject.Value10 = _newPropertyValues[10];
            _robject.Value11 = _newPropertyValues[11];
            _robject.Value12 = _newPropertyValues[12];
            _robject.Value13 = _newPropertyValues[13];
            _robject.Value14 = _newPropertyValues[14];
            _robject.Value15 = _newPropertyValues[15];
            _robject.Value16 = _newPropertyValues[16];
            _robject.Value17 = _newPropertyValues[17];
            _robject.Value18 = _newPropertyValues[18];
            _robject.Value19 = _newPropertyValues[19];
            _robject.Value20 = _newPropertyValues[20];
            _robject.Value21 = _newPropertyValues[21];
            _robject.Value22 = _newPropertyValues[22];
            _robject.Value23 = _newPropertyValues[23];
            _robject.Value24 = _newPropertyValues[24];
            _robject.Value25 = _newPropertyValues[25];
            _robject.Value26 = _newPropertyValues[26];
            _robject.Value27 = _newPropertyValues[27];
            _robject.Value28 = _newPropertyValues[28];
            _robject.Value29 = _newPropertyValues[29];
            _robject.Value30 = _newPropertyValues[30];
            _robject.Value31 = _newPropertyValues[31];
        }

        [Benchmark(OperationsPerInvoke = 32, Description = "Time to lookup an object with a String PK")]
        public IRealmObject? LookupByPK()
        {
            _tempObj = _realm.Find<StringClass>(_primaryKeys[0]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[1]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[2]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[3]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[4]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[5]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[6]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[7]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[8]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[9]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[10]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[11]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[12]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[13]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[14]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[15]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[16]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[17]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[18]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[19]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[20]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[21]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[22]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[23]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[24]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[25]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[26]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[27]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[28]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[29]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[30]);
            _tempObj = _realm.Find<StringClass>(_primaryKeys[31]);

            return _tempObj;
        }

        private partial class StringClass : IRealmObject
        {
            [PrimaryKey]
            public string Value { get; set; } = null!;

            public string Value0 { get; set; } = null!;

            public string Value1 { get; set; } = null!;

            public string Value2 { get; set; } = null!;

            public string Value3 { get; set; } = null!;

            public string Value4 { get; set; } = null!;

            public string Value5 { get; set; } = null!;

            public string Value6 { get; set; } = null!;

            public string Value7 { get; set; } = null!;

            public string Value8 { get; set; } = null!;

            public string Value9 { get; set; } = null!;

            public string Value10 { get; set; } = null!;

            public string Value11 { get; set; } = null!;

            public string Value12 { get; set; } = null!;

            public string Value13 { get; set; } = null!;

            public string Value14 { get; set; } = null!;

            public string Value15 { get; set; } = null!;

            public string Value16 { get; set; } = null!;

            public string Value17 { get; set; } = null!;

            public string Value18 { get; set; } = null!;

            public string Value19 { get; set; } = null!;

            public string Value20 { get; set; } = null!;

            public string Value21 { get; set; } = null!;

            public string Value22 { get; set; } = null!;

            public string Value23 { get; set; } = null!;

            public string Value24 { get; set; } = null!;

            public string Value25 { get; set; } = null!;

            public string Value26 { get; set; } = null!;

            public string Value27 { get; set; } = null!;

            public string Value28 { get; set; } = null!;

            public string Value29 { get; set; } = null!;

            public string Value30 { get; set; } = null!;

            public string Value31 { get; set; } = null!;
        }
    }
}
