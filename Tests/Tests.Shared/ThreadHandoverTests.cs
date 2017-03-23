////////////////////////////////////////////////////////////////////////////
//
// Copyright 2017 Realm Inc.
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
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using Realms;
using Realms.Exceptions;

namespace IntegrationTests.Shared
{
    [TestFixture, Preserve(AllMembers = true)]
    public class ThreadHandoverTests : RealmInstanceTest
    {
        [Test]
        public void ObjectHandover_ShouldWork()
        {
            AsyncContext.Run(async () =>
            {
                var obj = new IntPropertyObject { Int = 12 };

                _realm.Write(() => _realm.Add(obj));

                var objReference = ThreadSafeReference.Create(obj);

                await Task.Run(() =>
                {
                    using (var otherRealm = Realm.GetInstance(_realm.Config))
                    {
                        var otherObj = otherRealm.ResolveReference(objReference);

                        Assert.That(otherObj.IsManaged);
                        Assert.That(otherObj.IsValid);
                        Assert.That(otherObj.Int, Is.EqualTo(12));
                    }
                });
            });
        }

        [Test]
        public void ListHandover_ShouldWork()
        {
            AsyncContext.Run(async () =>
            {
                var obj = new Owner();
                obj.Dogs.Add(new Dog { Name = "1" });
                obj.Dogs.Add(new Dog { Name = "2" });

                _realm.Write(() => _realm.Add(obj));

                var listReference = ThreadSafeReference.Create(obj.Dogs);

                await Task.Run(() =>
                {
                    using (var otherRealm = Realm.GetInstance(_realm.Config))
                    {
                        var otherList = otherRealm.ResolveReference(listReference);

                        Assert.That(otherList, Is.InstanceOf(typeof(RealmList<Dog>)));
                        var dogNames = otherList.Select(d => d.Name);
                        Assert.That(dogNames, Is.EqualTo(new[] { "1", "2" }));
                    }
                });
            });
        }

        [Test]
        public void QueryHandover_WhenNoQueryApplied_ShouldWork()
        {
            AsyncContext.Run(async () =>
            {
                _realm.Write(() =>
                {
                    _realm.Add(new IntPropertyObject { Int = 1 });
                    _realm.Add(new IntPropertyObject { Int = 2 });
                    _realm.Add(new IntPropertyObject { Int = 3 });
                    _realm.Add(new IntPropertyObject { Int = 4 });
                });

                var query = _realm.All<IntPropertyObject>();
                var queryReference = ThreadSafeReference.Create(query);

                await Task.Run(() =>
                {
                    using (var otherRealm = Realm.GetInstance(_realm.Config))
                    {
                        var otherQuery = otherRealm.ResolveReference(queryReference);

                        Assert.That(otherQuery, Is.InstanceOf(typeof(RealmResults<IntPropertyObject>)));
                        var values = otherQuery.ToArray().Select(q => q.Int);
                        Assert.That(values, Is.EqualTo(new[] { 1, 2, 3, 4 }));
                    }
                });
            });
        }

        [Test]
        public void ThreadSafeReference_CanOnlyBeConsumedOnce()
        {
            AsyncContext.Run(async () =>
            {
                var obj = new IntPropertyObject();

                _realm.Write(() => _realm.Add(obj));

                var objReference = ThreadSafeReference.Create(obj);
                await Task.Run(() =>
                {
                    using (var otherRealm = Realm.GetInstance(_realm.Config))
                    {
                        otherRealm.ResolveReference(objReference);
                        Assert.That(() => otherRealm.ResolveReference(objReference), Throws.InstanceOf<RealmException>().And.Message.Contains("Can only resolve a thread safe reference once."));
                    }
                });
            });
        }

        [Test]
        public void ThreadSafeReference_WhenAnObjectIsUnmanaged_ShouldFail()
        {
            var obj = new IntPropertyObject();

            Assert.That(() => ThreadSafeReference.Create(obj), Throws.InstanceOf<RealmException>().And.Message.Contains("unmanaged object"));
        }

        [Test]
        public void ThreadSafeReference_WhenObjectIsDeleted_ShouldFail()
        {
            var obj = new IntPropertyObject();
            _realm.Write(() => _realm.Add(obj));
            _realm.Write(() => _realm.Remove(obj));

            Assert.That(() => ThreadSafeReference.Create(obj), Throws.InstanceOf<RealmException>().And.Message.Contains("invalidated object"));
        }

        [Test]
        public void ThreadSafeReference_CanBeResolvedOnTheSameThread()
        {
            var obj = new IntPropertyObject { Int = 12 };

            _realm.Write(() => _realm.Add(obj));

            var objReference = ThreadSafeReference.Create(obj);

            using (var otherRealm = Realm.GetInstance(_realm.Config))
            {
                var otherObj = otherRealm.ResolveReference(objReference);

                Assert.That(otherObj.IsManaged);
                Assert.That(otherObj.IsValid);
                Assert.That(otherObj.Int, Is.EqualTo(12));
            }
        }

        [Test]
        public void ThreadSafeReference_CanBeResolvedByTheSameRealm()
        {
            var obj = new IntPropertyObject { Int = 12 };

            _realm.Write(() => _realm.Add(obj));

            var objReference = ThreadSafeReference.Create(obj);

            var otherObj = _realm.ResolveReference(objReference);

            Assert.That(otherObj.IsManaged);
            Assert.That(otherObj.IsValid);
            Assert.That(otherObj.Int, Is.EqualTo(12));
        }

        [Test]
        public void ListReference_WhenListIsNotRealmList_ShouldFail()
        {
            IList<Dog> unmanagedDogs = new List<Dog>();
            unmanagedDogs.Add(new Dog());

            Assert.That(() => ThreadSafeReference.Create(unmanagedDogs), Throws.InstanceOf<InvalidCastException>());

            _realm.Write(() => _realm.Add(new Dog()));

            IList<Dog> managedDogs = _realm.All<Dog>().ToList();

            Assert.That(() => ThreadSafeReference.Create(managedDogs), Throws.InstanceOf<InvalidCastException>());
        }

        [Test]
        public void QueryReference_WhenQueryIsNotRealmResults_ShouldFail()
        {
            var unmanagedDogs = new[] { new Dog() };
            Assert.That(() => ThreadSafeReference.Create(unmanagedDogs.AsQueryable()), Throws.InstanceOf<InvalidCastException>());

            _realm.Write(() => _realm.Add(new Dog()));

            var managedDogs = _realm.All<Dog>().ToArray().AsQueryable();
            Assert.That(() => ThreadSafeReference.Create(managedDogs), Throws.InstanceOf<InvalidCastException>());
        }

        [Test]
        public void ThreadReference_WhenResolvedWithDifferentConfiguration_ShouldFail()
        {
            AsyncContext.Run(async () =>
            {
                var obj = new IntPropertyObject();

                _realm.Write(() => _realm.Add(obj));

                var objReference = ThreadSafeReference.Create(obj);

                await Task.Run(() =>
                {
                    var otherRealm = Realm.GetInstance("other.realm");

                    Assert.That(() => otherRealm.ResolveReference(objReference),
                                Throws.InstanceOf<RealmException>().And.Message.Contains("different configuration"));

                    otherRealm.Dispose();
                    Realm.DeleteRealm(otherRealm.Config);
                });
            });
        }

        [Test]
        public void ThreadSafeReference_WhenTargetRealmInTransaction_ShouldFail()
        {
            AsyncContext.Run(async () =>
            {
                var obj = new IntPropertyObject { Int = 12 };

                _realm.Write(() => _realm.Add(obj));

                var objReference = ThreadSafeReference.Create(obj);

                await Task.Run(() =>
                {
                    using (var otherRealm = Realm.GetInstance(_realm.Config))
                    {
                        otherRealm.Write(() =>
                        {
                            Assert.That(() => otherRealm.ResolveReference(objReference),
                                        Throws.InstanceOf<RealmException>().And.Message.Contains("write transaction"));
                        });
                    }
                });
            });
        }

        [Test]
        public void ObjectReference_WhenSourceRealmInTransaction_ShouldFail()
        {
            _realm.Write(() =>
            {
                var obj = _realm.Add(new IntPropertyObject());

                Assert.That(() => ThreadSafeReference.Create(obj), Throws.InstanceOf<RealmInvalidTransactionException>());
            });
        }

        [Test]
        [NUnit.Framework.Explicit("Assertion failure - investigating")]
        public void ObjectReference_ResolveDeletedObject()
        {
            AsyncContext.Run(async () =>
            {
                var obj = new IntPropertyObject { Int = 12 };
                _realm.Write(() => _realm.Add(obj));

                var ref1 = ThreadSafeReference.Create(obj);
                var ref2 = ThreadSafeReference.Create(obj);

                _realm.Write(() => _realm.Remove(obj));

                await Task.Run(() =>
                {
                    using (var otherRealm = Realm.GetInstance(_realm.Config))
                    {
                        var otherObj1 = otherRealm.ResolveReference(ref1);
                        Assert.That(otherObj1.IsManaged);
                        Assert.That(otherObj1.IsValid);
                        Assert.That(otherObj1.Int, Is.EqualTo(12));

                        otherRealm.Refresh();

                        var otherObj2 = otherRealm.ResolveReference(ref2);

                        Assert.That(otherObj2, Is.Null);
                    }
                });
            });
        }
    }
}
