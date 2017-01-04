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
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using Realms;
using Realms.Exceptions;

namespace IntegrationTests
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
        public void CreateObject_WhenPKExists_ShouldFail(Type type, object primaryKeyValue)
        {
            var pkProperty = type.GetProperties().Single(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);

            _realm.Write(() =>
            {
                var first = _realm.CreateObject(type.Name);
                pkProperty.SetValue(first, primaryKeyValue);
            });

            Assert.That(() =>
            {
                _realm.Write(() =>
                {
                    var second = _realm.CreateObject(type.Name);
                    pkProperty.SetValue(second, primaryKeyValue);
                });
            }, Throws.InnerException.TypeOf<RealmDuplicatePrimaryKeyValueException>());
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
        public void ManageObject_WhenPKExists_ShouldFail(Type type, object primaryKeyValue)
        {
            var pkProperty = type.GetProperties().Single(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);
            var first = (RealmObject)Activator.CreateInstance(type);
            pkProperty.SetValue(first, primaryKeyValue);

            _realm.Write(() =>
            {
                _realm.Add(first);
            });

            Assert.That(() =>
            {
                var second = (RealmObject)Activator.CreateInstance(type);
                pkProperty.SetValue(second, primaryKeyValue);
                _realm.Write(() =>
                {
                    _realm.Add(second);
                });
            }, Throws.TypeOf<RealmDuplicatePrimaryKeyValueException>());
        }

        private RealmObject FindByPKDynamic(Type type, object primaryKeyValue, bool isIntegerPK)
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

                return _realm.Find(type.Name, castPKValue);
            }

            return _realm.Find(type.Name, (string)primaryKeyValue);
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
        public void FailToFindByPrimaryKeyGenericTests(Type type, object primaryKeyValue, bool isIntegerPK)
        {
            var foundObj = FindByPKGeneric(type, primaryKeyValue, isIntegerPK);
            Assert.That(foundObj, Is.Null);
        }

        private RealmObject FindByPKGeneric(Type type, object primaryKeyValue, bool isIntegerPK)
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

            return (RealmObject)genericMethod.MakeGenericMethod(type).Invoke(_realm, new[] { castPKValue });
        }

        [Test]
        public void ExceptionIfNoPrimaryKeyDeclared()
        {
            Assert.Throws<RealmClassLacksPrimaryKeyException>(() =>
            {
                var foundObj = _realm.Find<Person>("Zaphod");
            });
        }

        [Test]
        public void ExceptionIfNoDynamicPrimaryKeyDeclared()
        {
            Assert.Throws<RealmClassLacksPrimaryKeyException>(() =>
            {
                var foundObj = _realm.Find("Person", "Zaphod");
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
                using (var realm2 = Realm.GetInstance(_configuration))
                {
                    var foundObj = realm2.Find<PrimaryKeyInt64Object>(42000042);
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
        public void NullPrimaryKeyStringObjectThrows()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _realm.Write(() =>
                {
                    var o2 = _realm.CreateObject<PrimaryKeyStringObject>();
                    o2.StringProperty = null;
                });
            });
        }

        [Test]
        public void NullAndNotNullIntPKsWorkTogether()
        {
            _realm.Write(() =>
            {
                var o1 = _realm.CreateObject<PrimaryKeyNullableInt64Object>();
                o1.Int64Property = null;
                var o2 = _realm.CreateObject<PrimaryKeyNullableInt64Object>();
                o2.Int64Property = 123;
            });

            Assert.That(_realm.All<PrimaryKeyNullableInt64Object>().Count, Is.EqualTo(2));
        }

        [Test]
        public void PrimaryKeyFailsIfClassNotinRealm()
        {
            var conf = RealmConfiguration.DefaultConfiguration.ConfigWithPath("Skinny");
            conf.ObjectClasses = new[] { typeof(Person) };
            var skinny = Realm.GetInstance(conf);
            Assert.Throws<KeyNotFoundException>(() =>
            {
                var obj = skinny.Find<PrimaryKeyInt64Object>(42);
            });
        }
    }
}