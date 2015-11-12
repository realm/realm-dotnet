/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using NUnit.Framework;
using RealmNet;
using System;
using System.Collections.Generic;
using InteropShared;

namespace Tests
{
    [TestFixture]
    public class MockQueryTestsBase
    {
        protected class TestEntity : RealmObject
        {
            public string NameStr { get; set; }
            public bool IsCool { get; set; }
            public int IntNum { get; set; }
            public double FloatNum { get; set; }
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
        }

    }
} 