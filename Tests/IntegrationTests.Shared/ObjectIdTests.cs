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
using System.Linq;
using NUnit.Framework;
using System.Threading;
using Realms;

namespace IntegrationTests.Shared
{
    // using classes from TestObjects.cs
    [TestFixture, Preserve(AllMembers = true)]
    public class ObjectIdTests
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
        public void FindByCharId()
        {
            _realm.Write(() => {
                var obj = _realm.CreateObject<ObjectIdCharObject>();
                obj.CharProperty = 'x';
            });

            var foundObj = _realm.ObjectById<ObjectIdCharObject>('x');
            Assert.IsNotNull(foundObj);
            Assert.That(foundObj.CharProperty, Is.EqualTo('x'));
        }

        [Test]
        public void FindByByteId()
        {
            _realm.Write(() => {
                var obj = _realm.CreateObject<ObjectIdByteObject>();
                obj.ByteProperty = 42;
            });

            var foundObj = _realm.ObjectById<ObjectIdByteObject>(42);
            Assert.IsNotNull(foundObj);
            Assert.That(foundObj.ByteProperty, Is.EqualTo(42));
        }

        [Test]
        public void FindByInt16Id()
        {
            _realm.Write(() => {
                var obj = _realm.CreateObject<ObjectIdInt16Object>();
                obj.Int16Property = 4242;
            });

            var foundObj = _realm.ObjectById<ObjectIdInt16Object>(4242);
            Assert.IsNotNull(foundObj);
            Assert.That(foundObj.Int16Property, Is.EqualTo(4242));
        }

        [Test]
        public void FindByInt32Id()
        {
            _realm.Write(() => {
                var obj = _realm.CreateObject<ObjectIdInt32Object>();
                obj.Int32Property = 42000042;
            });

            var foundObj = _realm.ObjectById<ObjectIdInt32Object>(42000042);
            Assert.IsNotNull(foundObj);
            Assert.That(foundObj.Int32Property, Is.EqualTo(42000042));
        }


        [Test]
        public void FindByInt64Id()
        {
            _realm.Write(() => {
                var obj = _realm.CreateObject<ObjectIdInt64Object>();
                obj.Int64Property = 42000042;
            });

            var foundObj = _realm.ObjectById<ObjectIdInt64Object>(42000042);
            Assert.IsNotNull(foundObj);
            Assert.That(foundObj.Int64Property, Is.EqualTo(42000042));
        }

        [Test]
        public void FindByStringId()
        {
            _realm.Write(() => {
                var obj = _realm.CreateObject<ObjectIdStringObject>();
                obj.StringProperty = "Zaphod";
            });

            var foundObj = _realm.ObjectById<ObjectIdStringObject>("Zaphod");
            Assert.IsNotNull(foundObj);
            Assert.That(foundObj.StringProperty, Is.EqualTo("Zaphod"));
        }


        [Test]
        public void ExceptionIfNoIdDeclared()
        {
            Assert.Throws<RealmClassLacksObjectIdException>( () => {
                var foundObj = _realm.ObjectById<Person>("Zaphod");
            });
        }



        [Test]
        public void GetByIdDifferentThreads()
        {
            _realm.Write(() => {
                var obj = _realm.CreateObject<ObjectIdInt64Object>();
                obj.Int64Property = 42000042;
            });

            Int64 foundValue = 0;
            // Act
            var t = new Thread(() =>
            {
                using (var realm2 = Realm.GetInstance()) {
                    var foundObj = realm2.ObjectById<ObjectIdInt64Object>(42000042);
                    foundValue = foundObj.Int64Property;
                }
            });
            t.Start();
            t.Join();

            Assert.That(foundValue, Is.EqualTo(42000042));
        }
    
    }
}

