/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Realms;

namespace IntegrationTests
{
    public class PerformanceTests
    {
        protected string _databasePath;
        protected Realm _realm;

        [SetUp]
        public void Setup()
        {
            _databasePath = Path.GetTempFileName();
            _realm = Realm.GetInstance(_databasePath);
        }

        [TearDown]
        public void TearDown() 
        {
            _realm.Close();
            Realm.DeleteRealm(_realm.Config);
        }


        [TestCase(1000000, 100), Explicit]
        public void BindingPerformanceTest(int totalRecs, int recsPerTrans)
        {
            Console.WriteLine($"Binding-based performance check for {totalRecs:n} entries at {recsPerTrans} ops per transaction -------------");

            var s = "String value";

            var sw = Stopwatch.StartNew();
            var numRecs = totalRecs / recsPerTrans;
            for (var rowIndex = 0; rowIndex < numRecs; rowIndex++)
            {
                using (var trans = _realm.BeginWrite())
                {         
                    var hangOntoObjectsUntilCommit = new List<RealmObject>();
                    for (var iTrans = 0; iTrans < recsPerTrans; ++iTrans)
                    {
                        var p = _realm.CreateObject<Person>();
                        p.FirstName = s;
                        p.IsInteresting = true;
                        hangOntoObjectsUntilCommit.Add(p);
                    }
                    trans.Commit();
                }
            }
            sw.Stop();

            Console.WriteLine("Time spent: " + sw.Elapsed);
            Console.WriteLine("Kilo-iterations per second: {0:0.00}", ((numRecs/1000) / sw.Elapsed.TotalSeconds));
        }
            

        [TestCase(1000000, 1000), Explicit]
        public void BindingCreateObjectPerformanceTest(int totalRecs, int recsPerTrans)
        {
            Console.WriteLine($"Binding-based performance check for {totalRecs:n} entries at {recsPerTrans} ops per transaction: CreateObject -------------");

            var s = "String value";

            var sw = Stopwatch.StartNew();
            var numRecs = totalRecs / recsPerTrans;
            for (var rowIndex = 0; rowIndex < numRecs; rowIndex++)
            {
                using (var trans = _realm.BeginWrite())
                {
                    var hangOntoObjectsUntilCommit = new List<RealmObject>();
                    for (var iTrans = 0; iTrans < recsPerTrans; ++iTrans)
                    {
                        var p = _realm.CreateObject<Person>();
                        hangOntoObjectsUntilCommit.Add(p);
                    }
                    trans.Commit();
                }
            }
            sw.Stop();

            Console.WriteLine("Time spent: " + sw.Elapsed);
            Console.WriteLine("Kilo-iterations per second: {0:0.00}", ((numRecs/1000) / sw.Elapsed.TotalSeconds));
        }
            

        [TestCase(1000000), Explicit]
        public void BindingSetValuePerformanceTest(int count)
        {
            Console.WriteLine($"Binding-based performance check for {count:n} entries: Set value -------------");

            var s = "String value";

            var sw = Stopwatch.StartNew();
            using (var trans = _realm.BeginWrite())
            {
                var p = _realm.CreateObject<Person>();
                // inner loop this time to rewrite the value many times without committing
                for (var rowIndex = 0; rowIndex < count; rowIndex++)
                {
                    p.FirstName = s;
                    p.IsInteresting = true;
                }
                trans.Commit();
            }
            sw.Stop();

            Console.WriteLine("Time spent: " + sw.Elapsed);
            Console.WriteLine("Kilo-iterations per second: {0:0.00}", ((count/1000) / sw.Elapsed.TotalSeconds));
        }

    }
}

