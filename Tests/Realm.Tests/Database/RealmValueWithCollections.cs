////////////////////////////////////////////////////////////////////////////
//
// Copyright 2023 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    internal class RealmValueWithCollections : RealmInstanceTest
    {
        [Test]
        public void Test1()
        {
            var rvo = new RealmValueObject();

            rvo.RealmValueProperty = RealmValue.List(new List<RealmValue> { 1, "two", 3 });

            _realm.Write(() =>
            {
                _realm.Add(rvo);
            });

            var savedValue = rvo.RealmValueProperty;
            var list = savedValue.AsList();

            Assert.That(list.Count(), Is.EqualTo(3));

            var firstVal = list[0].AsInt16();
            var secondVal = list[1].AsString();
            var thirdVal = list[2].AsInt16();

            Assert.That(firstVal, Is.EqualTo(1));
            Assert.That(secondVal, Is.EqualTo("two"));
            Assert.That(thirdVal, Is.EqualTo(3));
        }

        [Test]
        public void Test2()
        {
            var rvo = new RealmValueObject();

            rvo.RealmValueProperty = RealmValue.List(new List<RealmValue> { 1, "two", RealmValue.List(new List<RealmValue> { 0, 15 }) });

            _realm.Write(() =>
            {
                _realm.Add(rvo);
            });

            var savedValue = rvo.RealmValueProperty;
            var list = savedValue.AsList();

            Assert.That(list.Count(), Is.EqualTo(3));

            var thirdVal = list[2].AsList();

            var firstEl = thirdVal[0].AsInt16();
            var secondEl = thirdVal[1].AsInt16();

            Assert.That(firstEl, Is.EqualTo(0));
            Assert.That(secondEl, Is.EqualTo(15));
        }
    }
}
