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
using System.Threading;
using NUnit.Framework;
using Realms;

namespace IntegrationTests.Shared
{
    // using classes from TestObjects.cs
    [TestFixture, Preserve(AllMembers = true)]
    public class PrimaryKeyTests
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
        public void FindByCharPrimaryKey()
        {
            _realm.Write(() =>
            {
                var obj = _realm.CreateObject<PrimaryKeyCharObject>();
                obj.CharProperty = 'x';
            });

            var foundObj = _realm.ObjectForPrimaryKey<PrimaryKeyCharObject>('x');
            Assert.IsNotNull(foundObj);
            Assert.That(foundObj.CharProperty, Is.EqualTo('x'));
        }

        [Test]
        public void FindByBytePrimaryKey()
        {
            _realm.Write(() =>
            {
                var obj = _realm.CreateObject<PrimaryKeyByteObject>();
                obj.ByteProperty = 42;
            });

            var foundObj = _realm.ObjectForPrimaryKey<PrimaryKeyByteObject>(42);
            Assert.IsNotNull(foundObj);
            Assert.That(foundObj.ByteProperty, Is.EqualTo(42));
        }

        [Test]
        public void FindByInt16PrimaryKey()
        {
            _realm.Write(() =>
            {
                var obj = _realm.CreateObject<PrimaryKeyInt16Object>();
                obj.Int16Property = 4242;
            });

            var foundObj = _realm.ObjectForPrimaryKey<PrimaryKeyInt16Object>(4242);
            Assert.IsNotNull(foundObj);
            Assert.That(foundObj.Int16Property, Is.EqualTo(4242));
        }

        [Test]
        public void FindByInt32PrimaryKey()
        {
            _realm.Write(() =>
            {
                var obj = _realm.CreateObject<PrimaryKeyInt32Object>();
                obj.Int32Property = 42000042;
            });

            var foundObj = _realm.ObjectForPrimaryKey<PrimaryKeyInt32Object>(42000042);
            Assert.IsNotNull(foundObj);
            Assert.That(foundObj.Int32Property, Is.EqualTo(42000042));
        }


        [Test]
        public void FindByInt64PrimaryKey()
        {
            _realm.Write(() =>
            {
                var obj = _realm.CreateObject<PrimaryKeyInt64Object>();
                obj.Int64Property = 42000042;
            });

            var foundObj = _realm.ObjectForPrimaryKey<PrimaryKeyInt64Object>(42000042);
            Assert.IsNotNull(foundObj);
            Assert.That(foundObj.Int64Property, Is.EqualTo(42000042));
        }


        [Test]
        public void DontFindByInt64PrimaryKey()
        {
            var foundObj = _realm.ObjectForPrimaryKey<PrimaryKeyInt64Object>(3);
            Assert.IsNull(foundObj);
        }


        [Test]
        public void FindByStringPrimaryKey()
        {
            _realm.Write(() =>
            {
                var obj = _realm.CreateObject<PrimaryKeyStringObject>();
                obj.StringProperty = "Zaphod";
            });

            var foundObj = _realm.ObjectForPrimaryKey<PrimaryKeyStringObject>("Zaphod");
            Assert.IsNotNull(foundObj);
            Assert.That(foundObj.StringProperty, Is.EqualTo("Zaphod"));
        }


        [Test]
        public void DontFindByStringPrimaryKey()
        {
            var foundObj = _realm.ObjectForPrimaryKey<PrimaryKeyStringObject>("Ford");
            Assert.IsNull(foundObj);
        }


        [Test]
        public void FindDynamicByInt64PrimaryKey()
        {
            _realm.Write(() =>
            {
                var obj = _realm.CreateObject("PrimaryKeyInt64Object");
                obj.Int64Property = 42000042;
            });

            dynamic foundObj = _realm.ObjectForPrimaryKey("PrimaryKeyInt64Object", 42000042);
            Assert.IsNotNull(foundObj);
            Assert.That(foundObj.Int64Property, Is.EqualTo(42000042));
        }


        [Test]
        public void DontFindDynamicByInt64PrimaryKey()
        {
            dynamic foundObj = _realm.ObjectForPrimaryKey("PrimaryKeyInt64Object", 33);
            Assert.IsNull(foundObj);
        }


        [Test]
        public void FindDynamicByStringPrimaryKey()
        {
            _realm.Write(() =>
            {
                var obj = _realm.CreateObject("PrimaryKeyStringObject");
                obj.StringProperty = "Zaphod";
            });

            dynamic foundObj = _realm.ObjectForPrimaryKey("PrimaryKeyStringObject", "Zaphod");
            Assert.IsNotNull(foundObj);
            Assert.That(foundObj.StringProperty, Is.EqualTo("Zaphod"));
        }


        [Test]
        public void DontFindDynamicByStringPrimaryKey()
        {
            dynamic foundObj = _realm.ObjectForPrimaryKey("PrimaryKeyStringObject", "Dent");
            Assert.IsNull(foundObj);
        }


        [Test]
        public void ExceptionIfNoPrimaryKeyDeclared()
        {
            Assert.Throws<RealmClassLacksPrimaryKeyException>(() =>
            {
                var foundObj = _realm.ObjectForPrimaryKey<Person>("Zaphod");
            });
        }


        [Test]
        public void ExceptionIfNoDynamicIPrimaryKeyDeclared()
        {
            Assert.Throws<RealmClassLacksPrimaryKeyException>(() =>
            {
                var foundObj = _realm.ObjectForPrimaryKey("Person", "Zaphod");
            });
        }



        [Test]
        public void GetByPrimaryKeyDifferentThreads()
        {
            _realm.Write(() =>
            {
                var obj = _realm.CreateObject<PrimaryKeyInt64Object>();
                obj.Int64Property = 42000042;
            });

            long foundValue = 0;
            // Act
            var t = new Thread(() =>
            {
                using (var realm2 = Realm.GetInstance())
                {
                    var foundObj = realm2.ObjectForPrimaryKey<PrimaryKeyInt64Object>(42000042);
                    foundValue = foundObj.Int64Property;
                }
            });
            t.Start();
            t.Join();

            Assert.That(foundValue, Is.EqualTo(42000042));
        }


        [Test]
        public void PrimaryKeyStringObjectIsUnique()
        {
            _realm.Write(() =>
            {
                var o1 = _realm.CreateObject<PrimaryKeyStringObject>();
                o1.StringProperty = "Zaphod";
            });

            Assert.Throws<RealmDuplicatePrimaryKeyValueException>(() =>
            {
                _realm.Write(() =>
                {
                    var o2 = _realm.CreateObject<PrimaryKeyStringObject>();
                    o2.StringProperty = "Zaphod"; // deliberately reuse id
                });
            });
        }


        [Test]
        public void PrimaryKeyIntObjectIsUnique()
        {
            _realm.Write(() =>
            {
                var o1 = _realm.CreateObject<PrimaryKeyInt64Object>();
                o1.Int64Property = 9999000;
            });

            Assert.Throws<RealmDuplicatePrimaryKeyValueException>(() =>
            {
                _realm.Write(() =>
                {
                    var o2 = _realm.CreateObject<PrimaryKeyInt64Object>();
                    o2.Int64Property = 9999000; // deliberately reuse id
                });
            });
        }


        [Test]
        public void PrimaryKeyFailsIfClassNotinRealm()
        {
            var conf = RealmConfiguration.DefaultConfiguration.ConfigWithPath("Skinny");
            conf.ObjectClasses = new[] { typeof(Person) };
            var skinny = Realm.GetInstance(conf);
            Assert.Throws<KeyNotFoundException>(() =>
            {
                var obj = skinny.ObjectForPrimaryKey<PrimaryKeyInt64Object>(42);
            });
        }

    }
}

