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
    public partial class ObjectTests : BenchmarkBase
    {
        private const int ObjectCount = 100;

        private ObjectClass _robject = null!;
        private DummyClass[] _newPropertyValues = null!;
        private Transaction _transaction = null!;
        private DummyClass? _temp;

        protected override void SeedData()
        {
            base.SeedData();

            _realm.Write(() =>
            {
                for (var i = 0; i < ObjectCount; i++)
                {
                    _realm.Add(new ObjectClass
                    {
                        Value0 = new DummyClass(),
                        Value1 = new DummyClass(),
                        Value2 = new DummyClass(),
                        Value3 = new DummyClass(),
                        Value4 = new DummyClass(),
                        Value5 = new DummyClass(),
                        Value6 = new DummyClass(),
                        Value7 = new DummyClass(),
                        Value8 = new DummyClass(),
                        Value9 = new DummyClass(),
                        Value10 = new DummyClass(),
                        Value11 = new DummyClass(),
                        Value12 = new DummyClass(),
                        Value13 = new DummyClass(),
                        Value14 = new DummyClass(),
                        Value15 = new DummyClass(),
                        Value16 = new DummyClass(),
                        Value17 = new DummyClass(),
                        Value18 = new DummyClass(),
                        Value19 = new DummyClass(),
                        Value20 = new DummyClass(),
                        Value21 = new DummyClass(),
                        Value22 = new DummyClass(),
                        Value23 = new DummyClass(),
                        Value24 = new DummyClass(),
                        Value25 = new DummyClass(),
                        Value26 = new DummyClass(),
                        Value27 = new DummyClass(),
                        Value28 = new DummyClass(),
                        Value29 = new DummyClass(),
                        Value30 = new DummyClass(),
                        Value31 = new DummyClass(),
                    });
                }
            });

            _robject = _realm.All<ObjectClass>().ElementAt(_faker.Random.Int(0, ObjectCount - 1));

            _newPropertyValues = new DummyClass[32];
            for (var i = 0; i < _newPropertyValues.Length; i++)
            {
                var prop = typeof(ObjectClass).GetProperty($"Value{i}", BindingFlags.Public | BindingFlags.Instance)!;
                _newPropertyValues[i] = (DummyClass)prop.GetValue(_robject)!;
            }

            _transaction = _realm.BeginWrite();
        }

        protected override void CleanupCore()
        {
            _transaction.Rollback();
        }

        [Benchmark(OperationsPerInvoke = 32, Description = "Time to get a property of type RealmObject")]
        public object? GetPropertyValue()
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

        [Benchmark(OperationsPerInvoke = 32, Description = "Time to set a property of type RealmObject")]
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

        private partial class ObjectClass : IRealmObject
        {
            public DummyClass? Value0 { get; set; }

            public DummyClass? Value1 { get; set; }

            public DummyClass? Value2 { get; set; }

            public DummyClass? Value3 { get; set; }

            public DummyClass? Value4 { get; set; }

            public DummyClass? Value5 { get; set; }

            public DummyClass? Value6 { get; set; }

            public DummyClass? Value7 { get; set; }

            public DummyClass? Value8 { get; set; }

            public DummyClass? Value9 { get; set; }

            public DummyClass? Value10 { get; set; }

            public DummyClass? Value11 { get; set; }

            public DummyClass? Value12 { get; set; }

            public DummyClass? Value13 { get; set; }

            public DummyClass? Value14 { get; set; }

            public DummyClass? Value15 { get; set; }

            public DummyClass? Value16 { get; set; }

            public DummyClass? Value17 { get; set; }

            public DummyClass? Value18 { get; set; }

            public DummyClass? Value19 { get; set; }

            public DummyClass? Value20 { get; set; }

            public DummyClass? Value21 { get; set; }

            public DummyClass? Value22 { get; set; }

            public DummyClass? Value23 { get; set; }

            public DummyClass? Value24 { get; set; }

            public DummyClass? Value25 { get; set; }

            public DummyClass? Value26 { get; set; }

            public DummyClass? Value27 { get; set; }

            public DummyClass? Value28 { get; set; }

            public DummyClass? Value29 { get; set; }

            public DummyClass? Value30 { get; set; }

            public DummyClass? Value31 { get; set; }
        }

        private partial class DummyClass : IRealmObject
        {
            public int Int { get; set; }
        }
    }
}
