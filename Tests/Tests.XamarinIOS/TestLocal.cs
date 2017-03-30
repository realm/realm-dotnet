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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Realms;

namespace NUnit.Tests.Simple 
{
    [TestFixture]
    public class TestLocal 
    {
        internal class People : RealmObject
        {
            public string Name { get; set; }
        }

        [Test]
        public void TestMethod()
        {
            // TODO: Add your test code here
            using (var realm = Realm.GetInstance())
            {
                var initialCount = realm.All<People>().Count();

                realm.Write(() =>
                {
                    realm.Add(new People() { Name = "Andy" });
                });
                Assert.That(initialCount + 1, Is.EqualTo(realm.All<People>().Count()));
            }
        }
    }
}
