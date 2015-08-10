using NUnit.Framework;
using RealmNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InteropShared;

namespace Tests
{
    [TestFixture]
    public class MockQueryTestsBase
    {
        protected class TestEntity : RealmObject
        {
            public string NameStr { get; set; }
            public int IntNum { get; set; }
            public double DoubleNum { get; set; }
        }

        protected Realm realm;
        protected List<String> providerLog;
        protected RealmQuery<TestEntity> testEntities;

        [SetUp]
        public void Setup()
        {
            // use a mock where we're going to collate all the calls
            providerLog = new List<String>();  
            Realm.ActiveCoreProvider = new MockCoreProvider((msg) => { providerLog.Add(msg); });
            realm = Realm.GetInstance();
            testEntities = realm.All<TestEntity>();
            PrepareForQueries();
        }

        private void PrepareForQueries()
        {
            using (var writeWith = realm.BeginWrite())  // not strictly necessary for Mock backend
            {
                var te1 = realm.CreateObject<TestEntity>();
                te1.NameStr = "John";
                te1.IntNum = 1;
                te1.DoubleNum = 0.99;
                var te2 = realm.CreateObject<TestEntity>();
                te2.NameStr = "Peter";
                te2.IntNum = 2;
                te2.DoubleNum = 999.99;
                // TODO we should make more idiomatic syntax work such as 
                //var autoAdded = new TestEntity {Str = "Johnnie", Number = 3};
                //autoAdded = new TestEntity { Str = "Xanh Li", Number = 4 };
                var te3 = realm.CreateObject<TestEntity>();
                te3.NameStr = "Johnnie";
                te3.IntNum = 3;
                te3.DoubleNum = 3.1415;
                var te4 = realm.CreateObject<TestEntity>();
                te4.NameStr = "Xanh Li";
                te4.IntNum = 9;
                te4.DoubleNum = -5.0e6;
                writeWith.Commit();
            }
        }

        /*
        // Idiomatic C# using Linq to Objects and the anonymous objects and array init inference of C# 3
        var People = new[]  {  // infers consistent type if all init sequences have same names
	        new {NameStr = "John",    IntNum = 1},
	        new {NameStr = "Peter",   IntNum = 2},
	        new {NameStr = "Johnnie", IntNum = 3},
	        new {NameStr = "Xanh Li", IntNum = 4}
        };

        var query = from p in People select p;

        var linqed = query.ToList();
        */

    }
} 