// ////////////////////////////////////////////////////////////////////////////
// //
// // Copyright 2022 Realm Inc.
// //
// // Licensed under the Apache License, Version 2.0 (the "License")
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// // http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //
// ////////////////////////////////////////////////////////////////////////////

using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using Realms;

namespace PerformanceTests
{
    public class IntTests : BenchmarkBase
    {
        private const int ObjectCount = 100;

        private IntClass _robject;
        private int[] _newPropertyValues;
        private int[] _primaryKeys;
        private Transaction _transaction;
        private int _temp;
        private RealmObject _tempObj;

        protected override void SeedData()
        {
            base.SeedData();

            _realm.Write(() =>
            {
                for (var i = 0; i < ObjectCount; i++)
                {
                    _realm.Add(new IntClass
                    {
                        Value = _faker.Random.Int(),
                        Value0 = _faker.Random.Int(),
                        Value1 = _faker.Random.Int(),
                        Value2 = _faker.Random.Int(),
                        Value3 = _faker.Random.Int(),
                        Value4 = _faker.Random.Int(),
                        Value5 = _faker.Random.Int(),
                        Value6 = _faker.Random.Int(),
                        Value7 = _faker.Random.Int(),
                        Value8 = _faker.Random.Int(),
                        Value9 = _faker.Random.Int(),
                        Value10 = _faker.Random.Int(),
                        Value11 = _faker.Random.Int(),
                        Value12 = _faker.Random.Int(),
                        Value13 = _faker.Random.Int(),
                        Value14 = _faker.Random.Int(),
                        Value15 = _faker.Random.Int(),
                        Value16 = _faker.Random.Int(),
                        Value17 = _faker.Random.Int(),
                        Value18 = _faker.Random.Int(),
                        Value19 = _faker.Random.Int(),
                        Value20 = _faker.Random.Int(),
                        Value21 = _faker.Random.Int(),
                        Value22 = _faker.Random.Int(),
                        Value23 = _faker.Random.Int(),
                        Value24 = _faker.Random.Int(),
                        Value25 = _faker.Random.Int(),
                        Value26 = _faker.Random.Int(),
                        Value27 = _faker.Random.Int(),
                        Value28 = _faker.Random.Int(),
                        Value29 = _faker.Random.Int(),
                        Value30 = _faker.Random.Int(),
                        Value31 = _faker.Random.Int(),
                    });
                }
            });

            _robject = _realm.All<IntClass>().ElementAt(_faker.Random.Int(0, ObjectCount - 1));

            _primaryKeys = new int[32];
            for (var i = 0; i < _primaryKeys.Length; i++)
            {
                _primaryKeys[i] = _realm.All<IntClass>().ElementAt(_faker.Random.Int(0, ObjectCount - 1)).Value;
            }

            _newPropertyValues = new int[32];
            for (var i = 0; i < _newPropertyValues.Length; i++)
            {
                var prop = typeof(IntClass).GetProperty($"Value{i}", BindingFlags.Public | BindingFlags.Instance);
                _newPropertyValues[i] = (int)prop.GetValue(_robject);
            }

            _transaction = _realm.BeginWrite();
        }

        protected override void CleanupCore()
        {
            _transaction.Rollback();
        }

        [Benchmark(OperationsPerInvoke = 32, Description = "Time to get a property of type Int")]
        public int GetPropertyValue()
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

        [Benchmark(OperationsPerInvoke = 32, Description = "Time to set a property of type Int")]
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

        //[Benchmark(OperationsPerInvoke = 32, Description = "Time to lookup an object with a Int PK")]  //TODO Testing
        public RealmObject LookupByPK()
        {
            _tempObj = _realm.Find<IntClass>(_primaryKeys[0]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[1]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[2]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[3]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[4]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[5]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[6]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[7]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[8]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[9]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[10]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[11]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[12]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[13]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[14]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[15]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[16]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[17]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[18]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[19]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[20]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[21]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[22]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[23]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[24]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[25]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[26]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[27]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[28]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[29]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[30]);
            _tempObj = _realm.Find<IntClass>(_primaryKeys[31]);

            return _tempObj;
        }

        private class IntClass : RealmObject
        {
            [PrimaryKey]
            public int Value { get; set; }

            public int Value0 { get; set; }

            public int Value1 { get; set; }

            public int Value2 { get; set; }

            public int Value3 { get; set; }

            public int Value4 { get; set; }

            public int Value5 { get; set; }

            public int Value6 { get; set; }

            public int Value7 { get; set; }

            public int Value8 { get; set; }

            public int Value9 { get; set; }

            public int Value10 { get; set; }

            public int Value11 { get; set; }

            public int Value12 { get; set; }

            public int Value13 { get; set; }

            public int Value14 { get; set; }

            public int Value15 { get; set; }

            public int Value16 { get; set; }

            public int Value17 { get; set; }

            public int Value18 { get; set; }

            public int Value19 { get; set; }

            public int Value20 { get; set; }

            public int Value21 { get; set; }

            public int Value22 { get; set; }

            public int Value23 { get; set; }

            public int Value24 { get; set; }

            public int Value25 { get; set; }

            public int Value26 { get; set; }

            public int Value27 { get; set; }

            public int Value28 { get; set; }

            public int Value29 { get; set; }

            public int Value30 { get; set; }

            public int Value31 { get; set; }
        }
    }
}
