////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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

using System.Linq;
using NUnit.Framework;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class WriteOverloads : RealmInstanceTest
    {
        [Test]
        public void Write_ShouldReturnPrimitive()
        {
            var primitive = _realm.Write(() =>
            {
                return 2;
            });

            Assert.That(primitive, Is.EqualTo(2));
        }

        [Test]
        public void Write_ShouldReturnRealmObject()
        {
            var pko = _realm.Write(() =>
            {
                return _realm.Add(new IntPrimaryKeyWithValueObject
                {
                    Id = 1,
                    StringValue = "bla"
                });
            });

            var queried = _realm.Find<IntPrimaryKeyWithValueObject>(1);
            Assert.That(queried, Is.EqualTo(pko));
        }

        [Test]
        public void Write_ShouldReturnQueryable()
        {
            var queryable = _realm.Write(() =>
            {
                _realm.Add(new IntPrimaryKeyWithValueObject
                {
                    Id = 1,
                    StringValue = "bla"
                });

                _realm.Add(new IntPrimaryKeyWithValueObject
                {
                    Id = 2,
                    StringValue = "ble"
                });

                return _realm.All<IntPrimaryKeyWithValueObject>();
            });

            var queried = _realm.All<IntPrimaryKeyWithValueObject>();
            Assert.That(queried, Is.EqualTo(queryable));
        }

        [Test]
        public void Write_ShouldReturnCollection()
        {
            var dogs = _realm.Write(() =>
            {
                var owner = _realm.Add(new Owner
                {
                    Name = "Owen"
                });

                owner.ListOfDogs.Add(new Dog
                {
                    Name = "Puppy1"
                });

                owner.ListOfDogs.Add(new Dog
                {
                    Name = "Puppy2"
                });

                return owner.ListOfDogs;
            });

            var queried = _realm.All<Owner>().First().ListOfDogs;
            Assert.That(queried, Is.EqualTo(dogs));
        }
    }
}
