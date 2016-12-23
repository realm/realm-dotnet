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
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Realms;
using Realms.Weaving;

namespace IntegrationTests
{
    [TestFixture, Preserve(AllMembers = true)]
    public class GetPrimaryKeyTests : RealmInstanceTest
    {
        [Test]
        public void GetPrimaryKey_WhenManagedAndDoesNotHavePK_ShouldReturnFalse()
        {
            var obj = new AllTypesObject
            {
                BooleanProperty = true,
                RequiredStringProperty = string.Empty
            };

            _realm.Write(() =>
            {
                _realm.Add(obj);
            });

            object pk;
            var success = GetHelper(obj).TryGetPrimaryKeyValue(obj, out pk);
            Assert.That(success, Is.False);
            Assert.That(pk, Is.Null);
        }

        [Test]
        public void GetPrimaryKey_WhenNotManagedAndDoesNotHavePK_ShouldReturnFalse()
        {
            var obj = new AllTypesObject
            {
                BooleanProperty = true
            };

            object pk;
            var success = GetHelper(obj.GetType()).TryGetPrimaryKeyValue(obj, out pk);

            Assert.That(success, Is.False);
            Assert.That(pk, Is.Null);
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
                _realm.Add(obj);
            });

            object pk;
            var success = GetHelper(obj).TryGetPrimaryKeyValue(obj, out pk);

            Assert.That(success, Is.True);
            Assert.That(pk, Is.EqualTo(pkValue));
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
            var success = GetHelper(objectType).TryGetPrimaryKeyValue(obj, out pk);

            Assert.That(success, Is.True);
            Assert.That(pk, Is.EqualTo(pkValue));
        }

        private IRealmObjectHelper GetHelper(RealmObject obj)
        {
#if ENABLE_INTERNAL_NON_PCL_TESTS
            return obj.ObjectMetadata.Helper;
#else
            return GetHelper(obj.GetType());
#endif
        }

        private IRealmObjectHelper GetHelper(Type type)
        {
#if ENABLE_INTERNAL_NON_PCL_TESTS
            return _realm.Metadata[type.Name].Helper;
#else
            var attribute = type.GetCustomAttribute<WovenAttribute>();
            var helperType = (Type)typeof(WovenAttribute).GetProperty("HelperType", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(attribute);
            return (IRealmObjectHelper)Activator.CreateInstance(helperType);
#endif
        }
    }
}