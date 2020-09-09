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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using Realms.Exceptions;

namespace Realms.Tests.Database
{
    // using classes from TestObjects.cs
    [TestFixture, Preserve(AllMembers = true)]
    public class PrimaryKeyTests : RealmInstanceTest
    {
        [TestCase(typeof(PrimaryKeyCharObject), 'x', true)]
        [TestCase(typeof(PrimaryKeyNullableCharObject), 'x', true)]
        [TestCase(typeof(PrimaryKeyNullableCharObject), null, true)]
        [TestCase(typeof(PrimaryKeyByteObject), (byte)42, true)]
        [TestCase(typeof(PrimaryKeyNullableByteObject), (byte)42, true)]
        [TestCase(typeof(PrimaryKeyNullableByteObject), null, true)]
        [TestCase(typeof(PrimaryKeyInt16Object), (short)4242, true)]
        [TestCase(typeof(PrimaryKeyNullableInt16Object), (short)4242, true)]
        [TestCase(typeof(PrimaryKeyNullableInt16Object), null, true)]
        [TestCase(typeof(PrimaryKeyInt32Object), 42000042, true)]
        [TestCase(typeof(PrimaryKeyNullableInt32Object), 42000042, true)]
        [TestCase(typeof(PrimaryKeyNullableInt32Object), null, true)]
        [TestCase(typeof(PrimaryKeyInt64Object), 42000042L, true)]
        [TestCase(typeof(PrimaryKeyNullableInt64Object), 42000042L, true)]
        [TestCase(typeof(PrimaryKeyNullableInt64Object), null, true)]
        [TestCase(typeof(PrimaryKeyStringObject), "key", false)]
        [TestCase(typeof(PrimaryKeyStringObject), null, false)]
        [TestCase(typeof(PrimaryKeyStringObject), "", false)]
        [TestCase(typeof(RequiredPrimaryKeyStringObject), "key", false)]
        [TestCase(typeof(RequiredPrimaryKeyStringObject), "", false)]
        public void FindByPrimaryKeyDynamicTests(Type type, object primaryKeyValue, bool isIntegerPK)
        {
            var obj = (RealmObject)Activator.CreateInstance(type);
            var pkProperty = type.GetProperties().Single(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);
            pkProperty.SetValue(obj, primaryKeyValue);

            _realm.Write(() => _realm.Add(obj));

            var foundObj = FindByPKDynamic(type, primaryKeyValue, isIntegerPK);

            Assert.That(foundObj, Is.Not.Null);
            Assert.That(pkProperty.GetValue(foundObj), Is.EqualTo(primaryKeyValue));
        }

        [TestCase(typeof(PrimaryKeyCharObject), 'x', true)]
        [TestCase(typeof(PrimaryKeyNullableCharObject), 'x', true)]
        [TestCase(typeof(PrimaryKeyNullableCharObject), null, true)]
        [TestCase(typeof(PrimaryKeyByteObject), (byte)42, true)]
        [TestCase(typeof(PrimaryKeyNullableByteObject), (byte)42, true)]
        [TestCase(typeof(PrimaryKeyNullableByteObject), null, true)]
        [TestCase(typeof(PrimaryKeyInt16Object), (short)4242, true)]
        [TestCase(typeof(PrimaryKeyNullableInt16Object), (short)4242, true)]
        [TestCase(typeof(PrimaryKeyNullableInt16Object), null, true)]
        [TestCase(typeof(PrimaryKeyInt32Object), 42000042, true)]
        [TestCase(typeof(PrimaryKeyNullableInt32Object), 42000042, true)]
        [TestCase(typeof(PrimaryKeyNullableInt32Object), null, true)]
        [TestCase(typeof(PrimaryKeyInt64Object), 42000042L, true)]
        [TestCase(typeof(PrimaryKeyNullableInt64Object), 42000042L, true)]
        [TestCase(typeof(PrimaryKeyNullableInt64Object), null, true)]
        [TestCase(typeof(PrimaryKeyStringObject), "key", false)]
        [TestCase(typeof(PrimaryKeyStringObject), null, false)]
        [TestCase(typeof(PrimaryKeyStringObject), "", false)]
        [TestCase(typeof(RequiredPrimaryKeyStringObject), "key", false)]
        [TestCase(typeof(RequiredPrimaryKeyStringObject), "", false)]
        public void FailToFindByPrimaryKeyDynamicTests(Type type, object primaryKeyValue, bool isIntegerPK)
        {
            var foundObj = FindByPKDynamic(type, primaryKeyValue, isIntegerPK);
            Assert.That(foundObj, Is.Null);
        }

        [TestCase(typeof(PrimaryKeyCharObject), 'x')]
        [TestCase(typeof(PrimaryKeyNullableCharObject), 'x')]
        [TestCase(typeof(PrimaryKeyNullableCharObject), null)]
        [TestCase(typeof(PrimaryKeyByteObject), (byte)42)]
        [TestCase(typeof(PrimaryKeyNullableByteObject), (byte)42)]
        [TestCase(typeof(PrimaryKeyNullableByteObject), null)]
        [TestCase(typeof(PrimaryKeyInt16Object), (short)4242)]
        [TestCase(typeof(PrimaryKeyNullableInt16Object), (short)4242)]
        [TestCase(typeof(PrimaryKeyNullableInt16Object), null)]
        [TestCase(typeof(PrimaryKeyInt32Object), 42000042)]
        [TestCase(typeof(PrimaryKeyNullableInt32Object), 42000042)]
        [TestCase(typeof(PrimaryKeyNullableInt32Object), null)]
        [TestCase(typeof(PrimaryKeyInt64Object), 42000042L)]
        [TestCase(typeof(PrimaryKeyNullableInt64Object), 42000042L)]
        [TestCase(typeof(PrimaryKeyNullableInt64Object), null)]
        [TestCase(typeof(PrimaryKeyStringObject), "key")]
        [TestCase(typeof(PrimaryKeyStringObject), null)]
        [TestCase(typeof(PrimaryKeyStringObject), "")]
        [TestCase(typeof(RequiredPrimaryKeyStringObject), "key")]
        [TestCase(typeof(RequiredPrimaryKeyStringObject), "")]
        public void CreateObject_WhenPKExists_ShouldFail(Type type, object primaryKeyValue)
        {
            _realm.Write(() => _realm.DynamicApi.CreateObject(type.Name, primaryKeyValue));

            Assert.That(() =>
            {
                _realm.Write(() => _realm.DynamicApi.CreateObject(type.Name, primaryKeyValue));
            }, Throws.TypeOf<RealmDuplicatePrimaryKeyValueException>());
        }

        [TestCase(typeof(PrimaryKeyCharObject), 'x')]
        [TestCase(typeof(PrimaryKeyNullableCharObject), 'x')]
        [TestCase(typeof(PrimaryKeyNullableCharObject), null)]
        [TestCase(typeof(PrimaryKeyByteObject), (byte)42)]
        [TestCase(typeof(PrimaryKeyNullableByteObject), (byte)42)]
        [TestCase(typeof(PrimaryKeyNullableByteObject), null)]
        [TestCase(typeof(PrimaryKeyInt16Object), (short)4242)]
        [TestCase(typeof(PrimaryKeyNullableInt16Object), (short)4242)]
        [TestCase(typeof(PrimaryKeyNullableInt16Object), null)]
        [TestCase(typeof(PrimaryKeyInt32Object), 42000042)]
        [TestCase(typeof(PrimaryKeyNullableInt32Object), 42000042)]
        [TestCase(typeof(PrimaryKeyNullableInt32Object), null)]
        [TestCase(typeof(PrimaryKeyInt64Object), 42000042L)]
        [TestCase(typeof(PrimaryKeyNullableInt64Object), 42000042L)]
        [TestCase(typeof(PrimaryKeyNullableInt64Object), null)]
        [TestCase(typeof(PrimaryKeyStringObject), "key")]
        [TestCase(typeof(PrimaryKeyStringObject), null)]
        [TestCase(typeof(PrimaryKeyStringObject), "")]
        [TestCase(typeof(RequiredPrimaryKeyStringObject), "key")]
        [TestCase(typeof(RequiredPrimaryKeyStringObject), "")]
        public void ManageObject_WhenPKExists_ShouldFail(Type type, object primaryKeyValue)
        {
            var pkProperty = type.GetProperties().Single(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);
            var first = (RealmObject)Activator.CreateInstance(type);
            pkProperty.SetValue(first, primaryKeyValue);

            _realm.Write(() => _realm.Add(first));

            Assert.That(() =>
            {
                var second = (RealmObject)Activator.CreateInstance(type);
                pkProperty.SetValue(second, primaryKeyValue);
                _realm.Write(() => _realm.Add(second));
            }, Throws.TypeOf<RealmDuplicatePrimaryKeyValueException>());
        }

        private RealmObjectBase FindByPKDynamic(Type type, object primaryKeyValue, bool isIntegerPK)
        {
            if (isIntegerPK)
            {
                long? castPKValue;
                if (primaryKeyValue == null)
                {
                    castPKValue = null;
                }
                else
                {
                    castPKValue = Convert.ToInt64(primaryKeyValue);
                }

                return _realm.DynamicApi.Find(type.Name, castPKValue);
            }

            return _realm.DynamicApi.Find(type.Name, (string)primaryKeyValue);
        }

        [TestCase(typeof(PrimaryKeyCharObject), 'x', true)]
        [TestCase(typeof(PrimaryKeyNullableCharObject), 'x', true)]
        [TestCase(typeof(PrimaryKeyNullableCharObject), null, true)]
        [TestCase(typeof(PrimaryKeyByteObject), (byte)42, true)]
        [TestCase(typeof(PrimaryKeyNullableByteObject), (byte)42, true)]
        [TestCase(typeof(PrimaryKeyNullableByteObject), null, true)]
        [TestCase(typeof(PrimaryKeyInt16Object), (short)4242, true)]
        [TestCase(typeof(PrimaryKeyNullableInt16Object), (short)4242, true)]
        [TestCase(typeof(PrimaryKeyNullableInt16Object), null, true)]
        [TestCase(typeof(PrimaryKeyInt32Object), 42000042, true)]
        [TestCase(typeof(PrimaryKeyNullableInt32Object), 42000042, true)]
        [TestCase(typeof(PrimaryKeyNullableInt32Object), null, true)]
        [TestCase(typeof(PrimaryKeyInt64Object), 42000042L, true)]
        [TestCase(typeof(PrimaryKeyNullableInt64Object), 42000042L, true)]
        [TestCase(typeof(PrimaryKeyNullableInt64Object), null, true)]
        [TestCase(typeof(PrimaryKeyStringObject), "key", false)]
        [TestCase(typeof(PrimaryKeyStringObject), null, false)]
        [TestCase(typeof(PrimaryKeyStringObject), "", false)]
        [TestCase(typeof(RequiredPrimaryKeyStringObject), "key", false)]
        [TestCase(typeof(RequiredPrimaryKeyStringObject), "", false)]
        public void FindByPrimaryKeyGenericTests(Type type, object primaryKeyValue, bool isIntegerPK)
        {
            var obj = (RealmObject)Activator.CreateInstance(type);
            var pkProperty = type.GetProperties().Single(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);
            pkProperty.SetValue(obj, primaryKeyValue);

            _realm.Write(() => _realm.Add(obj));

            var foundObj = FindByPKGeneric(type, primaryKeyValue, isIntegerPK);

            Assert.That(foundObj, Is.Not.Null);
            Assert.That(pkProperty.GetValue(foundObj), Is.EqualTo(primaryKeyValue));
        }

        [TestCase(typeof(PrimaryKeyCharObject), 'x', true)]
        [TestCase(typeof(PrimaryKeyNullableCharObject), 'x', true)]
        [TestCase(typeof(PrimaryKeyNullableCharObject), null, true)]
        [TestCase(typeof(PrimaryKeyByteObject), (byte)42, true)]
        [TestCase(typeof(PrimaryKeyNullableByteObject), (byte)42, true)]
        [TestCase(typeof(PrimaryKeyNullableByteObject), null, true)]
        [TestCase(typeof(PrimaryKeyInt16Object), (short)4242, true)]
        [TestCase(typeof(PrimaryKeyNullableInt16Object), (short)4242, true)]
        [TestCase(typeof(PrimaryKeyNullableInt16Object), null, true)]
        [TestCase(typeof(PrimaryKeyInt32Object), 42000042, true)]
        [TestCase(typeof(PrimaryKeyNullableInt32Object), 42000042, true)]
        [TestCase(typeof(PrimaryKeyNullableInt32Object), null, true)]
        [TestCase(typeof(PrimaryKeyInt64Object), 42000042L, true)]
        [TestCase(typeof(PrimaryKeyNullableInt64Object), 42000042L, true)]
        [TestCase(typeof(PrimaryKeyNullableInt64Object), null, true)]
        [TestCase(typeof(PrimaryKeyStringObject), "key", false)]
        [TestCase(typeof(PrimaryKeyStringObject), null, false)]
        [TestCase(typeof(PrimaryKeyStringObject), "", false)]
        [TestCase(typeof(RequiredPrimaryKeyStringObject), "key", false)]
        [TestCase(typeof(RequiredPrimaryKeyStringObject), "", false)]
        public void FailToFindByPrimaryKeyGenericTests(Type type, object primaryKeyValue, bool isIntegerPK)
        {
            var foundObj = FindByPKGeneric(type, primaryKeyValue, isIntegerPK);
            Assert.That(foundObj, Is.Null);
        }

        private RealmObjectBase FindByPKGeneric(Type type, object primaryKeyValue, bool isIntegerPK)
        {
            var genericArgument = isIntegerPK ? typeof(long?) : typeof(string);
            var genericMethod = _realm.GetType().GetMethod(nameof(Realm.Find), new[] { genericArgument });

            object castPKValue;
            if (isIntegerPK)
            {
                if (primaryKeyValue == null)
                {
                    castPKValue = (long?)null;
                }
                else
                {
                    castPKValue = Convert.ToInt64(primaryKeyValue);
                }
            }
            else
            {
                castPKValue = (string)primaryKeyValue;
            }

            return (RealmObjectBase)genericMethod.MakeGenericMethod(type).Invoke(_realm, new[] { castPKValue });
        }

        [Test]
        public void ExceptionIfNoPrimaryKeyDeclared()
        {
            Assert.That(() => _realm.Find<Person>("Zaphod"), Throws.TypeOf<RealmClassLacksPrimaryKeyException>());
        }

        [Test]
        public void ExceptionIfNoDynamicPrimaryKeyDeclared()
        {
            Assert.That(() => _realm.DynamicApi.Find("Person", "Zaphod"), Throws.TypeOf<RealmClassLacksPrimaryKeyException>());
        }

        [Test]
        public void GetByPrimaryKeyDifferentThreads()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                _realm.Write(() =>
                {
                    _realm.Add(new PrimaryKeyInt64Object { Int64Property = 42000042 });
                });

                long foundValue = 0;

                // Act
                await Task.Run(() =>
                {
                    using (var realm2 = Realm.GetInstance(_configuration))
                    {
                        var foundObj = realm2.Find<PrimaryKeyInt64Object>(42000042);
                        foundValue = foundObj.Int64Property;
                    }
                });

                Assert.That(foundValue, Is.EqualTo(42000042));
            });
        }

        [Test]
        public void PrimaryKeyStringObjectIsUnique()
        {
            _realm.Write(() =>
            {
                _realm.Add(new PrimaryKeyStringObject { StringProperty = "Zaphod" });
            });

            Assert.That(() =>
            {
                _realm.Write(() =>
                {
                    _realm.Add(new PrimaryKeyStringObject { StringProperty = "Zaphod" }); // deliberately reuse id
                });
            }, Throws.TypeOf<RealmDuplicatePrimaryKeyValueException>());
        }

        [Test]
        public void NullAndNotNullIntPKsWorkTogether()
        {
            _realm.Write(() =>
            {
                _realm.Add(new PrimaryKeyNullableInt64Object { Int64Property = null });
                _realm.Add(new PrimaryKeyNullableInt64Object { Int64Property = 123 });
            });

            Assert.That(_realm.All<PrimaryKeyNullableInt64Object>().Count, Is.EqualTo(2));

            _realm.Write(() =>
            {
                _realm.Add(new PrimaryKeyStringObject { StringProperty = "123" });
                _realm.Add(new PrimaryKeyStringObject { StringProperty = null });
            });

            Assert.That(_realm.All<PrimaryKeyStringObject>().Count, Is.EqualTo(2));
        }

        [Test]
        public void PrimaryKeyFailsIfClassNotInRealm()
        {
            var conf = ((RealmConfiguration)RealmConfiguration.DefaultConfiguration).ConfigWithPath(Path.GetTempFileName());
            conf.ObjectClasses = new[] { typeof(Person) };
            try
            {
                using (var skinny = Realm.GetInstance(conf))
                {
                    Assert.That(() => skinny.Find<PrimaryKeyInt64Object>(42), Throws.TypeOf<KeyNotFoundException>());
                }
            }
            finally
            {
                Realm.DeleteRealm(conf);
            }
        }

        [Test]
        public void StringPrimaryKey_WhenRequiredDoesntAllowNull()
        {
            Assert.That(() => _realm.Write(() => _realm.Add(new RequiredPrimaryKeyStringObject())), Throws.TypeOf<ArgumentException>());
        }
    }
}