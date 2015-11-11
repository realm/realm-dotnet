/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using NUnit.Framework;
using RealmNet;
using RealmNet.Interop;
using System.Collections.Generic;
using System.Linq;



// TODO
// Work out a nicer way of swapping from using Linq to Objects to using Realm backend


namespace Tests
{
    [TestFixture]
    public class QueryTestsBase
    {
        // TODO share this with MockQueryTestsBase to avoid duplication

        protected class TestEntity : RealmObject  // if not using Realm core will be standalone objects
        {
            public string NameStr { get; set; }
            public bool IsCool { get; set; }
            public int IntNum { get; set; }
            public double FloatNum { get; set; }
            public double DoubleNum { get; set; }
            // TODO add Binary
            // TODO add DateTime
        }

#if USING_REALM_CORE
        protected Realm realm;
        protected RealmQuery<TestEntity> testEntities;


        [SetUp]
        public void Setup()
        {
            // use a mock where we're going to collate all the calls
            Realm.ActiveCoreProvider = new CoreProvider();
            realm = Realm.GetInstance(System.IO.Path.GetTempFileName());
            testEntities = realm.All<TestEntity>();

            using (var writeWith = realm.BeginWrite())
            {
                var te1 = realm.CreateObject<TestEntity>();
                te1.NameStr = "John";
                te1.IsCool = false;
                te1.IntNum = 1;
                // not until #67 te1.FloatNum = 0.99;
                // not until #67 te1.DoubleNum = 0.99;
                var te2 = realm.CreateObject<TestEntity>();
                te2.NameStr = "Peter";
                te2.IsCool = false;
                te2.IntNum = 2;
                // not until #67 te2.FloatNum = 999.99;
                // not until #67 te2.DoubleNum = 999.99;

                // alternative, simpler creation syntax
                var te3 = new TestEntity {NameStr = "Johnnie", IntNum = 3, IsCool = true /* not until #67, FloatNum = 3.1415, DoubleNum = 3.1415 */};
                var te4 = new TestEntity {NameStr = "Xanh Li", IntNum = 9, IsCool = false /* not until #67, FloatNum = -5.0e6, DoubleNum = -5.0e6 */ };
                writeWith.Commit();
            }
        }

#else
        protected IEnumerable<TestEntity> testEntities;

        [SetUp]
        public void Setup()
        {
            // Idiomatic C# using Linq to Objects and the anonymous objects and array init inference of C# 3
            var People = new[]  {  // infers consistent type if all init sequences have same names
                new TestEntity {NameStr = "John",    IsCool = false, IntNum = 1, FloatNum = 0.99,   DoubleNum = 0.99},
                new TestEntity {NameStr = "Peter",   IsCool = false, IntNum = 2, FloatNum = 999.99, DoubleNum = 999.99},
                new TestEntity {NameStr = "Johnnie", IsCool = true,  IntNum = 3, FloatNum = 3.1415, DoubleNum = 3.1415},
                new TestEntity {NameStr = "Xanh Li", IsCool = false,  IntNum = 4, FloatNum = -5.0e6, DoubleNum = -5.0e6}
            };

            testEntities =  from p in People select p;

        }
#endif  // using Realm or simple objects

    }
} 