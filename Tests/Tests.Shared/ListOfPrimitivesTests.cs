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
using NUnit.Framework;
using Realms;

namespace Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class ListOfPrimitivesTests : RealmInstanceTest
    {
        [Test]
        public void TestIntegers()
        {
            var obj = new ListsObject();
            _realm.Write(() => _realm.Add(obj));
            var items = obj.NullableInt32List;
            _realm.Write(() =>
            {
                items.Add(1);
                items.Add(null);
                items.Insert(0, 2);
                items.Insert(2, null);
            });

            var test1 = items[0];
            foreach (var item in items)
            {
                System.Console.WriteLine(item);
            }

            var test = items.IndexOf(2);
            var test5 = items.IndexOf(null);

            var counters = obj.NullableInt32CounterList;

            _realm.Write(() =>
            {
                counters.Add(1);
                counters.Add(null);
                counters.Insert(0, 2);
                counters.Insert(2, null);
            });

            var test2 = counters[0];
            foreach (var item in counters)
            {
                System.Console.WriteLine(item);
            }

            var test3 = counters.IndexOf(2);
            var test6 = counters.IndexOf(null);
        }
    }
}
