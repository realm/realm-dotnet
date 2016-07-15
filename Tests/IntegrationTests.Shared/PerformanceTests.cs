////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
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
    [TestFixture, Preserve(AllMembers = true)]
    public class PerformanceTests
    {
        protected Realm _realm;

        [SetUp]
        public void Setup()
        {
            Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);
            _realm = Realm.GetInstance();
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

