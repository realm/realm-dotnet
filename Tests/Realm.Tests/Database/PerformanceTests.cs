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
using NUnit.Framework;
using TestExplicitAttribute = NUnit.Framework.ExplicitAttribute;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class PerformanceTests : RealmInstanceTest
    {
        [TestCase(1000000, 100), TestExplicit]
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
                    var hangOntoObjectsUntilCommit = new List<RealmObjectBase>();
                    for (var iTrans = 0; iTrans < recsPerTrans; ++iTrans)
                    {
                        var p = _realm.Add(new Person
                        {
                            FirstName = s,
                            IsInteresting = true
                        });
                        hangOntoObjectsUntilCommit.Add(p);
                    }

                    trans.Commit();
                }
            }

            sw.Stop();

            Console.WriteLine("Time spent: " + sw.Elapsed);
            Console.WriteLine("Kilo-iterations per second: {0:0.00}", (numRecs / 1000) / sw.Elapsed.TotalSeconds);
        }

        [TestCase(1000000, 1000), TestExplicit]
        public void BindingCreateObjectPerformanceTest(int totalRecs, int recsPerTrans)
        {
            Console.WriteLine($"Binding-based performance check for {totalRecs:n} entries at {recsPerTrans} ops per transaction: CreateObject -------------");

            var sw = Stopwatch.StartNew();
            var numRecs = totalRecs / recsPerTrans;
            for (var rowIndex = 0; rowIndex < numRecs; rowIndex++)
            {
                using (var trans = _realm.BeginWrite())
                {
                    var hangOntoObjectsUntilCommit = new List<RealmObjectBase>();
                    for (var iTrans = 0; iTrans < recsPerTrans; ++iTrans)
                    {
                        var p = _realm.Add(new Person());
                        hangOntoObjectsUntilCommit.Add(p);
                    }

                    trans.Commit();
                }
            }

            sw.Stop();

            Console.WriteLine("Time spent: " + sw.Elapsed);
            Console.WriteLine("Kilo-iterations per second: {0:0.00}", (numRecs / 1000) / sw.Elapsed.TotalSeconds);
        }

        [TestCase(1000000), TestExplicit]
        public void BindingSetValuePerformanceTest(int count)
        {
            Console.WriteLine($"Binding-based performance check for {count:n} entries: Set value -------------");

            var s = "String value";

            var sw = Stopwatch.StartNew();
            using (var trans = _realm.BeginWrite())
            {
                var p = _realm.Add(new Person());

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
            Console.WriteLine("Kilo-iterations per second: {0:0.00}", (count / 1000) / sw.Elapsed.TotalSeconds);
        }

        [TestCase(100000), TestExplicit]
        public void ManageSmallObjectPerformanceTest(int count)
        {
            var objects = new List<MiniPerson>();
            for (var i = 0; i < count; i++)
            {
                objects.Add(new MiniPerson
                {
                    Name = "Name" + i,
                    IsInteresting = true
                });
            }

            var sw = new Stopwatch();
            sw.Start();

            _realm.Write(() =>
            {
                foreach (var obj in objects)
                {
                    _realm.Add(obj);
                }
            });
            sw.Stop();
            Console.WriteLine($"{count} objects managed for {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            _realm.Write(() =>
            {
                for (var i = 0; i < count; i++)
                {
                    _realm.Add(new MiniPerson
                    {
                        Name = objects[i].Name,
                        IsInteresting = objects[i].IsInteresting
                    });
                }
            });

            Console.WriteLine($"{count} objects created for {sw.ElapsedMilliseconds} ms");
        }

        [TestCase(100000), TestExplicit]
        public void ManageLargeObjectPerformanceTest(int count)
        {
            var objects = new List<Person>();
            for (var i = 0; i < count; i++)
            {
                objects.Add(new Person
                {
                    FirstName = "Name" + i,
                    IsInteresting = true
                });
            }

            var sw = new Stopwatch();
            sw.Start();

            _realm.Write(() =>
            {
                foreach (var obj in objects)
                {
                    _realm.Add(obj);
                }
            });
            sw.Stop();
            Console.WriteLine($"{count} objects managed for {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            _realm.Write(() =>
            {
                for (var i = 0; i < count; i++)
                {
                    _realm.Add(new Person
                    {
                        FirstName = objects[i].FirstName,
                        IsInteresting = objects[i].IsInteresting
                    });
                }
            });

            Console.WriteLine($"{count} objects created for {sw.ElapsedMilliseconds} ms");
        }
    }

    public class MiniPerson : RealmObject
    {
        public string Name { get; set; }

        public bool IsInteresting { get; set; }
    }
}