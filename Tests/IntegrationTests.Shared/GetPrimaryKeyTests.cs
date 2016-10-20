﻿////////////////////////////////////////////////////////////////////////////
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
using System.Reflection;
using NUnit.Framework;
using Realms;

namespace IntegrationTests.Shared
{
    [TestFixture, Preserve(AllMembers = true)]
    public class GetPrimaryKeyTests
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
        public void GetPrimaryKey_WhenManagedAndDoesNotHavePK_ShouldReturnFalse()
        {
            var obj = new AllTypesObject
            {
                BooleanProperty = true
            };

            _realm.Write(() =>
            {
                _realm.Manage(obj);
            });

            object pk;
            var success = obj.ObjectMetadata.Helper.TryGetPrimaryKeyValue(obj, out pk);

            Assert.IsFalse(success);
            Assert.IsNull(pk);
        }

        [Test]
        public void GetPrimaryKey_WhenNotManagedAndDoesNotHavePK_ShouldReturnFalse()
        {
            var obj = new AllTypesObject
            {
                BooleanProperty = true
            };

            object pk;
            var success = _realm.Metadata[obj.GetType().Name].Helper.TryGetPrimaryKeyValue(obj, out pk);

            Assert.IsFalse(success);
            Assert.IsNull(pk);
        }

        [TestCase(typeof(PrimaryKeyCharObject), 'a')]
        [TestCase(typeof(PrimaryKeyByteObject), (byte)5)]
        [TestCase(typeof(PrimaryKeyInt16Object), (short)13)]
        [TestCase(typeof(PrimaryKeyInt32Object), 42)]
        [TestCase(typeof(PrimaryKeyInt64Object), (long)76)]
        [TestCase(typeof(PrimaryKeyStringObject), "lorem ipsum")]
        public void GetPrimaryKey_WhenClassManagedAndHasPK_ShouldReturnPK(Type objectType, object pkValue)
        {
            var obj = (RealmObject)Activator.CreateInstance(objectType);
            var pkProperty = objectType.GetProperties().Single(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);
            pkProperty.SetValue(obj, pkValue);

            _realm.Write(() =>
            {
                _realm.Manage(obj);
            });

            object pk;
            var success = obj.ObjectMetadata.Helper.TryGetPrimaryKeyValue(obj, out pk);

            Assert.IsTrue(success);
            Assert.AreEqual(pkValue, pk);
        }

        [TestCase(typeof(PrimaryKeyCharObject), 'a')]
        [TestCase(typeof(PrimaryKeyByteObject), (byte)5)]
        [TestCase(typeof(PrimaryKeyInt16Object), (short)13)]
        [TestCase(typeof(PrimaryKeyInt32Object), 42)]
        [TestCase(typeof(PrimaryKeyInt64Object), (long)76)]
        [TestCase(typeof(PrimaryKeyStringObject), "lorem ipsum")]
        public void GetPrimaryKey_WhenClassNotManagedAndHasPK_ShouldReturnPK(Type objectType, object pkValue)
        {
            var obj = (RealmObject)Activator.CreateInstance(objectType);
            var pkProperty = objectType.GetProperties().Single(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);
            pkProperty.SetValue(obj, pkValue);

            object pk;
            var success = _realm.Metadata[objectType.Name].Helper.TryGetPrimaryKeyValue(obj, out pk);

            Assert.IsTrue(success);
            Assert.AreEqual(pkValue, pk);
        }
    }
}