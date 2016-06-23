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
using System.Text;
using IntegrationTests.Shared;
using NUnit.Framework;
using Realms;

namespace IntegrationTests
{
    [TestFixture]
    class ObjectIdAndUniqueTests
    {
        private Realm _realm;

        [SetUp]
        public void SetUp()
        {
            var conf = new RealmConfiguration("ObjectIdandUnique.realm");
            // Realm.DeleteRealm(conf);  // uncomment to leave all the records created by these tests
            _realm = Realm.GetInstance(conf);
        }

        [TearDown]
        public void TearDown()
        {
            _realm.Close();
            // leave file on disk so can check with browser, delete before next run
        }

        [Test]
        public void ObjectIdCharObjectIsUnique()
        {
            _realm.Write(() => {
                var o1 = _realm.CreateObject<ObjectIdCharObject>();
                o1.CharProperty = 'a';
                o1.Created = DateTimeOffset.Now;
                o1.CountCreated = 1;
            });
            _realm.Write(() => {
                var o2 = _realm.CreateObject<ObjectIdCharObject>();
                o2.CharProperty = 'a'; // deliberately reuse id
                o2.Created = DateTimeOffset.Now;
                o2.CountCreated = 2;
            });
            Assert.That(_realm.All<ObjectIdCharObject>().Count(), Is.EqualTo(1));
        }


        [Test]
        public void ObjectIdByteObjectIsUnique()
        {
            _realm.Write(() => {
                var o1 = _realm.CreateObject<ObjectIdByteObject>();
                o1.ByteProperty = 42;
                o1.Created = DateTimeOffset.Now;
                o1.CountCreated = 1;
                });
            _realm.Write(() => {
                var o2 = _realm.CreateObject<ObjectIdByteObject>();
                o2.ByteProperty = 42; // deliberately reuse id
                o2.Created = DateTimeOffset.Now;
                o2.CountCreated = 2;
            });
            Assert.That(_realm.All<ObjectIdByteObject>().Count(), Is.EqualTo(1));
        }


        [Test]
        public void ObjectIdInt16ObjectIsUnique()
        {
            _realm.Write(() => {
                var o1 = _realm.CreateObject<ObjectIdInt16Object>();
                o1.Int16Property = 4242;
                o1.Created = DateTimeOffset.Now;
                o1.CountCreated = 1;
            });
            _realm.Write(() => {
                var o2 = _realm.CreateObject<ObjectIdInt16Object>();
                o2.Int16Property = 4242; // deliberately reuse id
                o2.Created = DateTimeOffset.Now;
                o2.CountCreated = 2;
            });
            Assert.That(_realm.All<ObjectIdInt16Object>().Count(), Is.EqualTo(1));
        }


        [Test]
        public void ObjectIdInt32ObjectIsUnique()
        {
            _realm.Write(() => {
                var o1 = _realm.CreateObject<ObjectIdInt32Object>();
                o1.Int32Property = 42042;
                o1.Created = DateTimeOffset.Now;
                o1.CountCreated = 1;
            });
            _realm.Write(() => {
                var o2 = _realm.CreateObject<ObjectIdInt32Object>();
                o2.Int32Property = 42042; // deliberately reuse id
                o2.Created = DateTimeOffset.Now;
                o2.CountCreated = 2;
            });
            Assert.That(_realm.All<ObjectIdInt32Object>().Count(), Is.EqualTo(1));
        }


        [Test]
        public void ObjectIdInt64ObjectIsUnique()
        {
            _realm.Write(() => {
                var o1 = _realm.CreateObject<ObjectIdInt64Object>();
                o1.Int64Property = 424242;
                o1.Created = DateTimeOffset.Now;
                o1.CountCreated = 1;
            });
            _realm.Write(() => {
                var o2 = _realm.CreateObject<ObjectIdInt64Object>();
                o2.Int64Property = 424242; // deliberately reuse id
                o2.Created = DateTimeOffset.Now;
                o2.CountCreated = 2;
            });
            Assert.That(_realm.All<ObjectIdInt64Object>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void ObjectIdStringObjectIsUnique()
        {
            _realm.Write(() => {
                var o1 = _realm.CreateObject<ObjectIdStringObject>();
                o1.StringProperty = "Zaphod";
                o1.Created = DateTimeOffset.Now;
                o1.CountCreated = 1;
            });
            _realm.Write(() => {
                var o2 = _realm.CreateObject<ObjectIdStringObject>();
                o2.StringProperty = "Zaphod"; // deliberately reuse id
                o2.Created = DateTimeOffset.Now;
                o2.CountCreated = 2;
            });
            Assert.That(_realm.All<ObjectIdStringObject>().Count(), Is.EqualTo(1));
        }

    }
}
