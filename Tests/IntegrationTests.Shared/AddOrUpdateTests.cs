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

namespace IntegrationTests.Shared
{
    [TestFixture, Preserve(AllMembers = true)]
    public class AddOrUpdateTests
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

        [Test]
        public void AddOrUpdate_WhenDoesntExist_ShouldAdd()
        {
            var standalone = new PrimaryKeyObject
            {
                Id = 1,
                StringValue = "bla"
            };

            _realm.Write(() =>
            {
                _realm.Manage(standalone, update: true);
            });

            Assert.IsTrue(standalone.IsManaged);

            var queried = _realm.ObjectForPrimaryKey<PrimaryKeyObject>(1);
            Assert.AreEqual(standalone, queried);
        }

        [Test]
        public void AddOrUpdate_WhenExists_ShouldUpdate()
        {
            var first = new PrimaryKeyObject
            {
                Id = 1,
                StringValue = "first"
            };

            _realm.Write(() =>
            {
                _realm.Manage(first, update: true);
            });

            var second = new PrimaryKeyObject
            {
                Id = 1,
                StringValue = "second"
            };

            _realm.Write(() =>
            {
                _realm.Manage(second, update: true);
            });

            var queried = _realm.ObjectForPrimaryKey<PrimaryKeyObject>(1);
            Assert.AreEqual("second", queried.StringValue);
            Assert.AreEqual("second", first.StringValue);
            Assert.AreEqual("second", second.StringValue);
        }

        [Test]
        public void AddOrUpdate_WhenNoPrimaryKey_ShouldAdd()
        {
            var noPK = new NonPrimaryKeyObject
            {
                StringValue = "123"
            };

            _realm.Write(() =>
            {
                _realm.Manage(noPK, update: true);
            });

            var noPK2 = new NonPrimaryKeyObject
            {
                StringValue = "123"
            };

            _realm.Write(() =>
            {
                _realm.Manage(noPK2, update: true);
            });

            Assert.AreNotEqual(noPK, noPK2);
            Assert.AreEqual(2, _realm.All<NonPrimaryKeyObject>().Count());
        }

        [Test]
        public void AddOrUpdate_WhenParentAndChildDontExist_ShouldAddBoth()
        {
            var first = new PrimaryKeyWithPKRelation
            {
                Id = 1,
                StringValue = "parent",
                OtherObject = new PrimaryKeyObject
                {
                    Id = 1,
                    StringValue = "child"
                }
            };

            _realm.Write(() =>
            {
                _realm.Manage(first, update: true);
            });

            var queried = _realm.ObjectForPrimaryKey<PrimaryKeyWithPKRelation>(1);
            Assert.IsNotNull(queried.OtherObject);
            Assert.AreEqual(queried.StringValue, "parent");
            Assert.AreEqual(queried.OtherObject.StringValue, "child");
        }

        [Test]
        public void AddOrUpdate_WhenParentExistsChildDoesnt_ShouldAddChild()
        {
            var first = new PrimaryKeyWithPKRelation
            {
                Id = 1,
                StringValue = "parent",
                OtherObject = new PrimaryKeyObject
                {
                    Id = 1,
                    StringValue = "child"
                }
            };

            _realm.Write(() =>
            {
                _realm.Manage(first, update: true);
            });

            var second = new PrimaryKeyWithPKRelation
            {
                Id = 1,
                StringValue = "parent",
                OtherObject = new PrimaryKeyObject
                {
                    Id = 2,
                    StringValue = "child2"
                }
            };

            _realm.Write(() =>
            {
                _realm.Manage(second, update: true);
            });

            var queried = _realm.ObjectForPrimaryKey<PrimaryKeyWithPKRelation>(1);
            Assert.IsNotNull(queried.OtherObject);
            Assert.AreEqual("parent", queried.StringValue);
            Assert.AreEqual("child2", queried.OtherObject.StringValue);

            var child1 = _realm.ObjectForPrimaryKey<PrimaryKeyObject>(1);
            Assert.AreEqual("child", child1.StringValue);

            var child2 = _realm.ObjectForPrimaryKey<PrimaryKeyObject>(2);
            Assert.AreEqual("child2", child2.StringValue);
        }

        [Test]
        public void AddOrUpdate_WhenParentAndChildExist_ShouldUpdateBoth()
        {
            var first = new PrimaryKeyWithPKRelation
            {
                Id = 1,
                StringValue = "parent",
                OtherObject = new PrimaryKeyObject
                {
                    Id = 1,
                    StringValue = "child"
                }
            };

            _realm.Write(() =>
            {
                _realm.Manage(first, update: true);
            });

            var second = new PrimaryKeyWithPKRelation
            {
                Id = 1,
                StringValue = "parent2",
                OtherObject = new PrimaryKeyObject
                {
                    Id = 1,
                    StringValue = "child2"
                }
            };

            _realm.Write(() =>
            {
                _realm.Manage(second, update: true);
            });

            var queried = _realm.ObjectForPrimaryKey<PrimaryKeyWithPKRelation>(1);
            Assert.IsNotNull(queried.OtherObject);
            Assert.AreEqual("parent2", queried.StringValue);
            Assert.AreEqual("child2", queried.OtherObject.StringValue);

            var child1 = _realm.ObjectForPrimaryKey<PrimaryKeyObject>(1);
            Assert.AreEqual("child2", child1.StringValue);
        }

        [Test]
        public void AddOrUpdate_WhenChildHasNoPK_ShouldAddChild()
        {
            var first = new PrimaryKeyWithNonPKRelation
            {
                Id = 1,
                StringValue = "parent",
                OtherObject = new NonPrimaryKeyObject
                {
                    StringValue = "child"
                }
            };

            _realm.Write(() =>
            {
                _realm.Manage(first, update: true);
            });

            var second = new PrimaryKeyWithNonPKRelation
            {
                Id = 2,
                StringValue = "parent2",
                OtherObject = new NonPrimaryKeyObject
                {
                    StringValue = "child"
                }
            };

            _realm.Write(() =>
            {
                _realm.Manage(second, update: true);
            });

            Assert.AreNotEqual(first.OtherObject, second.OtherObject);
            Assert.AreEqual(2, _realm.All<NonPrimaryKeyObject>().Count());
        }

        [Test]
        public void AddOrUpdate_WhenChildHasPK_ShouldUpdateChild()
        {
            var first = new PrimaryKeyWithPKRelation
            {
                Id = 1,
                StringValue = "parent",
                OtherObject = new PrimaryKeyObject
                {
                    Id = 1,
                    StringValue = "child"
                }
            };

            _realm.Write(() =>
            {
                _realm.Manage(first, update: true);
            });

            var second = new PrimaryKeyWithPKRelation
            {
                Id = 2,
                StringValue = "parent2",
                OtherObject = new PrimaryKeyObject
                {
                    Id = 1,
                    StringValue = "child2"
                }
            };

            _realm.Write(() =>
            {
                _realm.Manage(second, update: true);
            });

            Assert.AreEqual(first.OtherObject, second.OtherObject);
            Assert.AreEqual(1, _realm.All<PrimaryKeyObject>().Count());
            Assert.AreEqual("child2", _realm.ObjectForPrimaryKey<PrimaryKeyObject>(1).StringValue);
        }

        private class NonPrimaryKeyObject : RealmObject
        {
            public string StringValue { get; set; }
        }

        private class PrimaryKeyObject : RealmObject
        {
            [PrimaryKey]
            public long Id { get; set; }

            public string StringValue { get; set; }
        }

        private class PrimaryKeyWithPKRelation : RealmObject
        {
            [PrimaryKey]
            public long Id { get; set; }

            public string StringValue { get; set; }

            public PrimaryKeyObject OtherObject { get; set; }
        }

        private class PrimaryKeyWithNonPKRelation : RealmObject
        {
            [PrimaryKey]
            public long Id { get; set; }

            public string StringValue { get; set; }

            public NonPrimaryKeyObject OtherObject { get; set; }
        }
    }
}