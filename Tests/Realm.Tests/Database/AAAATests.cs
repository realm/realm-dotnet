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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class AAAATests : RealmInstanceTest
    {
        private static string GetDebugView(Expression exp)
        {
            if (exp == null)
            {
                return null;
            }

            var propertyInfo = typeof(Expression).GetProperty("DebugView", BindingFlags.Instance | BindingFlags.NonPublic);
            return propertyInfo.GetValue(exp) as string;
        }

        [Test]
        public void SimpleTest()
        {
            var query = _realm.All<Person>().Where(p => p.Score == 2);
            var debugView = GetDebugView(query.Expression);
            Console.WriteLine(debugView);
            _ = query.ToArray();
        }

        //[Test]
        //public void Ordering()
        //{
        //    var query = _realm.All<Person>()
        //        .Where(p => p.FirstName.StartsWith("abc") && p.IsInteresting)
        //        .Where(p => p.Birthday < System.DateTimeOffset.UtcNow)
        //        .OrderBy(p => p.FirstName)
        //        .ThenByDescending(p => p.Birthday);

        //    _ = query.ToArray();
        //}

        //[Test]
        //public void DictTest()
        //{
        //    var query = _realm.All<CollectionsObject>().Where(a => a.BooleanDict.Any(kvp => kvp.Key.StartsWith("abc")));
        //    var debugView = GetDebugView(query.Expression);
        //    Console.WriteLine(debugView);
        //    _ = query.ToArray();
        //}

        //[Test]
        //public void ListTest()
        //{
        //    var query = _realm.All<CollectionsObject>().Where(a => a.BooleanList.Count > 5);
        //    _ = query.ToArray();
        //}

        //[Test]
        //public void Iteration()
        //{
        //    _realm.Write(() =>
        //    {
        //        for (var i = 0; i < 10; i++)
        //        {
        //            _realm.Add(new IntPropertyObject
        //            {
        //                Int = i
        //            });
        //        }
        //    });

        //    var query = _realm.All<IntPropertyObject>().Where(a => a.Int > 5);
        //    foreach (var item in query)
        //    {
        //        System.Console.WriteLine(item.Int);
        //    }

        //    for (var i = 0; i < query.Count(); i++)
        //    {
        //        System.Console.WriteLine(query.ElementAt(i).Int);
        //    }
        //}
    }
}
