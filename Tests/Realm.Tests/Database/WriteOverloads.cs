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

                owner.Dogs.Add(new Dog
                {
                    Name = "Puppy1"
                });

                owner.Dogs.Add(new Dog
                {
                    Name = "Puppy2"
                });

                return owner.Dogs;
            });

            var queried = _realm.All<Owner>().First().Dogs;
            Assert.That(queried, Is.EqualTo(dogs));
        }

        [Test]
        public void WriteAsync_ShouldReturnPrimitive()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var primitive = await _realm.WriteAsync(realm =>
                {
                    return 2;
                });
                Assert.That(primitive, Is.EqualTo(2));
            });
        }

        [Test]
        public void WriteAsync_ShouldReturnRealmObject()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var pko = await _realm.WriteAsync(realm =>
                {
                    return realm.Add(new IntPrimaryKeyWithValueObject
                    {
                        Id = 1,
                        StringValue = "bla"
                    });
                });

                var queried = _realm.Find<IntPrimaryKeyWithValueObject>(1);
                Assert.That(queried, Is.EqualTo(pko));
            });
        }

        [Test]
        public void WriteAsync_ShouldReturnUnmanagedRealmObject()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var pko = new IntPrimaryKeyWithValueObject
                {
                    Id = 1,
                    StringValue = "bla"
                };

                var writeResult = await _realm.WriteAsync(realm =>
                {
                    return pko;
                });

                Assert.That(writeResult, Is.EqualTo(pko));
            });
        }

        [Test]
        public void WriteAsync_ShouldReturnQueryable()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var queryable = await _realm.WriteAsync(realm =>
                   {
                       realm.Add(new IntPrimaryKeyWithValueObject
                       {
                           Id = 1,
                           StringValue = "bla"
                       });

                       realm.Add(new IntPrimaryKeyWithValueObject
                       {
                           Id = 2,
                           StringValue = "ble"
                       });

                       return realm.All<IntPrimaryKeyWithValueObject>();
                   });

                var queried = _realm.All<IntPrimaryKeyWithValueObject>();
                Assert.That(queried, Is.EqualTo(queryable));
            });
        }

        [Test]
        public void WriteAsync_ShouldReturnUnmanagedQueryable()
        {
            var pko1 = new IntPrimaryKeyWithValueObject
            {
                Id = 1,
                StringValue = "bla"
            };

            var pko2 = new IntPrimaryKeyWithValueObject
            {
                Id = 2,
                StringValue = "ble"
            };

            var queryable = new IntPrimaryKeyWithValueObject[] { pko1, pko2 }.AsQueryable();

            TestHelpers.RunAsyncTest(async () =>
            {
                var queryableResult = await _realm.WriteAsync(realm =>
                {
                    return queryable;
                });

                Assert.That(queryable, Is.EqualTo(queryableResult));
            });
        }

        [Test]
        public void WriteAsync_ShouldReturnCollection()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var dogs = await _realm.WriteAsync(realm =>
                    {
                        var owner = realm.Add(new Owner
                        {
                            Name = "Owen"
                        });

                        owner.Dogs.Add(new Dog
                        {
                            Name = "Puppy1"
                        });

                        owner.Dogs.Add(new Dog
                        {
                            Name = "Puppy2"
                        });

                        return owner.Dogs;
                    });

                var queried = _realm.All<Owner>().First().Dogs;
                Assert.That(queried, Is.EqualTo(dogs));
            });
        }

        [Test]
        public void WriteAsync_ShouldReturnUnmanagedCollection()
        {
            var owner = new Owner
            {
                Name = "Owen"
            };

            owner.Dogs.Add(new Dog
            {
                Name = "Puppy1"
            });

            owner.Dogs.Add(new Dog
            {
                Name = "Puppy2"
            });

            TestHelpers.RunAsyncTest(async () =>
            {
                var collectionResult = await _realm.WriteAsync(realm =>
                {
                    return owner.Dogs;
                });

                Assert.That(owner.Dogs, Is.EqualTo(collectionResult));
            });
        }

        [Test]
        public void WriteAsync_WhenReturningManagedObjectIndirectly_ShouldThrow()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var pko = await _realm.WriteAsync(realm =>
                {
                    var ipo = realm.Add(new IntPrimaryKeyWithValueObject
                    {
                        Id = 1,
                        StringValue = "bla"
                    });

                    return new
                    {
                        IPO = ipo,
                    };
                });

                Assert.Throws<Exceptions.RealmClosedException>(() => { _ = pko.IPO.Id != 3; });
            });
        }
    }
}
