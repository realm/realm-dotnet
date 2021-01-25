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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MongoDB.Bson;
using NUnit.Framework;
using Realms.Exceptions;

namespace Realms.Tests.Database
{
    // using classes from TestObjects.cs
    [TestFixture, Preserve(AllMembers = true)]
    public class PrimaryKeyTests : RealmInstanceTest
    {
        public enum PKType
        {
            Int,
            String,
            ObjectId,
            Guid
        }

        public static object[] PKTestCases =
        {
            new object[] { typeof(PrimaryKeyCharObject), 'x', PKType.Int },
            new object[] { typeof(PrimaryKeyNullableCharObject), 'x', PKType.Int },
            new object[] { typeof(PrimaryKeyNullableCharObject), null, PKType.Int },
            new object[] { typeof(PrimaryKeyByteObject), (byte)42, PKType.Int },
            new object[] { typeof(PrimaryKeyNullableByteObject), (byte)42, PKType.Int },
            new object[] { typeof(PrimaryKeyNullableByteObject), null, PKType.Int },
            new object[] { typeof(PrimaryKeyInt16Object), (short)4242, PKType.Int },
            new object[] { typeof(PrimaryKeyNullableInt16Object), (short)4242, PKType.Int },
            new object[] { typeof(PrimaryKeyNullableInt16Object), null, PKType.Int },
            new object[] { typeof(PrimaryKeyInt32Object), 42000042, PKType.Int },
            new object[] { typeof(PrimaryKeyNullableInt32Object), 42000042, PKType.Int },
            new object[] { typeof(PrimaryKeyNullableInt32Object), null, PKType.Int },
            new object[] { typeof(PrimaryKeyInt64Object), 42000042L, PKType.Int },
            new object[] { typeof(PrimaryKeyNullableInt64Object), 42000042L, PKType.Int },
            new object[] { typeof(PrimaryKeyNullableInt64Object), null, PKType.Int },
            new object[] { typeof(PrimaryKeyStringObject), "key", PKType.String },
            new object[] { typeof(PrimaryKeyStringObject), null, PKType.String },
            new object[] { typeof(PrimaryKeyStringObject), string.Empty, PKType.String },
            new object[] { typeof(RequiredPrimaryKeyStringObject), "key", PKType.String },
            new object[] { typeof(RequiredPrimaryKeyStringObject), string.Empty, PKType.String },
            new object[] { typeof(PrimaryKeyObjectIdObject), new ObjectId("5f64cd9f1691c361b2451d96"), PKType.ObjectId },
            new object[] { typeof(PrimaryKeyNullableObjectIdObject), new ObjectId("5f64cd9f1691c361b2451d96"), PKType.ObjectId },
            new object[] { typeof(PrimaryKeyNullableObjectIdObject), null, PKType.ObjectId },
            new object[] { typeof(PrimaryKeyGuidObject), Guid.Parse("{C4EC8CEF-D62A-405E-83BB-B0A3D8DABB36}"), PKType.Guid },
            new object[] { typeof(PrimaryKeyNullableGuidObject), Guid.Parse("{C4EC8CEF-D62A-405E-83BB-B0A3D8DABB36}"), PKType.Guid },
            new object[] { typeof(PrimaryKeyNullableGuidObject), null, PKType.Guid },
        };

        private readonly IEnumerable<dynamic> _primaryKeyValues = new dynamic[] { "42", 123L, ObjectId.GenerateNewId(), Guid.NewGuid() };

        [TestCaseSource(nameof(PKTestCases))]
        public void FindByPrimaryKeyDynamicTests(Type type, object primaryKeyValue, PKType pkType)
        {
            var obj = (RealmObject)Activator.CreateInstance(type);
            var pkProperty = type.GetProperties().Single(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);
            pkProperty.SetValue(obj, primaryKeyValue);

            _realm.Write(() => _realm.Add(obj));

            var foundObj = FindByPKDynamic(type, primaryKeyValue, pkType);

            Assert.That(foundObj, Is.Not.Null);
            Assert.That(pkProperty.GetValue(foundObj), Is.EqualTo(primaryKeyValue));
        }

        [TestCaseSource(nameof(PKTestCases))]
        public void FailToFindByPrimaryKeyDynamicTests(Type type, object primaryKeyValue, PKType pkType)
        {
            var foundObj = FindByPKDynamic(type, primaryKeyValue, pkType);
            Assert.That(foundObj, Is.Null);
        }

        [TestCaseSource(nameof(PKTestCases))]
        public void CreateObject_WhenPKExists_ShouldFail(Type type, object primaryKeyValue, PKType _)
        {
            _realm.Write(() => _realm.DynamicApi.CreateObject(type.Name, primaryKeyValue));

            Assert.That(() =>
            {
                _realm.Write(() => _realm.DynamicApi.CreateObject(type.Name, primaryKeyValue));
            }, Throws.TypeOf<RealmDuplicatePrimaryKeyValueException>());
        }

        [TestCaseSource(nameof(PKTestCases))]
        public void ManageObject_WhenPKExists_ShouldFail(Type type, object primaryKeyValue, PKType _)
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

        private RealmObjectBase FindByPKDynamic(Type type, object primaryKeyValue, PKType pkType)
        {
            switch (pkType)
            {
                case PKType.Int:
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

                case PKType.String:
                    return _realm.DynamicApi.Find(type.Name, (string)primaryKeyValue);

                case PKType.ObjectId:
                    return _realm.DynamicApi.Find(type.Name, (ObjectId?)primaryKeyValue);

                case PKType.Guid:
                    return _realm.DynamicApi.Find(type.Name, (Guid?)primaryKeyValue);

                default:
                    throw new NotSupportedException($"Unsupported pk type: {pkType}");
            }
        }

        [TestCaseSource(nameof(PKTestCases))]
        public void FindByPrimaryKeyGenericTests(Type type, object primaryKeyValue, PKType pkType)
        {
            var obj = (RealmObject)Activator.CreateInstance(type);
            var pkProperty = type.GetProperties().Single(p => p.GetCustomAttribute<PrimaryKeyAttribute>() != null);
            pkProperty.SetValue(obj, primaryKeyValue);

            _realm.Write(() => _realm.Add(obj));

            var foundObj = FindByPKGeneric(type, primaryKeyValue, pkType);

            Assert.That(foundObj, Is.Not.Null);
            Assert.That(pkProperty.GetValue(foundObj), Is.EqualTo(primaryKeyValue));
        }

        [TestCaseSource(nameof(PKTestCases))]
        public void FailToFindByPrimaryKeyGenericTests(Type type, object primaryKeyValue, PKType pkType)
        {
            var foundObj = FindByPKGeneric(type, primaryKeyValue, pkType);
            Assert.That(foundObj, Is.Null);
        }

        private RealmObjectBase FindByPKGeneric(Type type, object primaryKeyValue, PKType pkType)
        {
            var genericArgument = pkType switch
            {
                PKType.Int => typeof(long?),
                PKType.String => typeof(string),
                PKType.ObjectId => typeof(ObjectId?),
                PKType.Guid => typeof(Guid?),
                _ => throw new NotSupportedException(),
            };

            var genericMethod = _realm.GetType().GetMethod(nameof(Realm.Find), new[] { genericArgument });
            if (pkType == PKType.Int && primaryKeyValue != null)
            {
                primaryKeyValue = Convert.ToInt64(primaryKeyValue);
            }

            return (RealmObjectBase)genericMethod.MakeGenericMethod(type).Invoke(_realm, new[] { primaryKeyValue });
        }

        [Test]
        public void RealmFind_WhenPKIsIntAndArgumentIsNot_Throws() => RealmFind_IncorrectPKArgument_Throws<IntPrimaryKeyWithValueObject>();

        [Test]
        public void RealmFind_WhenPKIsCharAndArgumentIsNot_Throws() => RealmFind_IncorrectPKArgument_Throws<PrimaryKeyCharObject>();

        [Test]
        public void RealmFind_WhenPKIsByteAndArgumentIsNot_Throws() => RealmFind_IncorrectPKArgument_Throws<PrimaryKeyByteObject>();

        [Test]
        public void RealmFind_WhenPKIsInt16AndArgumentIsNot_Throws() => RealmFind_IncorrectPKArgument_Throws<PrimaryKeyInt16Object>();

        [Test]
        public void RealmFind_WhenPKIsInt32AndArgumentIsNot_Throws() => RealmFind_IncorrectPKArgument_Throws<PrimaryKeyInt32Object>();

        [Test]
        public void RealmFind_WhenPKIsInt64AndArgumentIsNot_Throws() => RealmFind_IncorrectPKArgument_Throws<PrimaryKeyInt64Object>();

        [Test]
        public void RealmFind_WhenPKIsStringAndArgumentIsNot_Throws() => RealmFind_IncorrectPKArgument_Throws<PrimaryKeyStringObject>();

        [Test]
        public void RealmFind_WhenPKIsGuidAndArgumentIsNot_Throws() => RealmFind_IncorrectPKArgument_Throws<PrimaryKeyGuidObject>();

        [Test]
        public void RealmFind_WhenPKIsObjectIdAndArgumentIsNot_Throws() => RealmFind_IncorrectPKArgument_Throws<PrimaryKeyObjectIdObject>();

        /// <summary>
        /// In <see cref="RealmFind_IncorrectPKArgument_Throws"/> we're using reflection to check if the primary
        /// key type and the property type match. Since Realm stores all integral properties as Int64, we
        /// want to treat the narrower types as "long" to match the value type in <see cref="_primaryKeyValues"/>.
        /// </summary>
        private static Type GetDatabaseType(Type toConvert)
        {
            var intBackedTypes = new Type[] { typeof(int), typeof(long), typeof(short), typeof(byte), typeof(char) };
            return intBackedTypes.Contains(toConvert) ? typeof(long) : toConvert;
        }

        private void RealmFind_IncorrectPKArgument_Throws<T>()
            where T : RealmObject
        {
            var pkInClass = typeof(T).GetProperties().Single(prop => Attribute.IsDefined(prop, typeof(PrimaryKeyAttribute)));
            var pkType = GetDatabaseType(pkInClass.PropertyType);

            var keysWithIncorrectType = _primaryKeyValues.Where(pk => pk.GetType() != pkType);
            foreach (var pk in keysWithIncorrectType)
            {
                Assert.That(() => _realm.Find<T>(pk), Throws.TypeOf<RealmException>().With.Message.Contains("Property type mismatch"));
                Assert.That(() => _realm.DynamicApi.Find(typeof(T).Name, pk), Throws.TypeOf<RealmException>().With.Message.Contains("Property type mismatch"));
            }
        }

        [Test]
        public void ExceptionIfNoPrimaryKeyDeclared()
        {
            Assert.That(() => _realm.Find<Person>("Zaphod"), Throws.TypeOf<RealmClassLacksPrimaryKeyException>());
            Assert.That(() => _realm.Find<Person>(42), Throws.TypeOf<RealmClassLacksPrimaryKeyException>());
            Assert.That(() => _realm.Find<Person>(ObjectId.GenerateNewId()), Throws.TypeOf<RealmClassLacksPrimaryKeyException>());
            Assert.That(() => _realm.Find<Person>(Guid.NewGuid()), Throws.TypeOf<RealmClassLacksPrimaryKeyException>());
        }

        [Test]
        public void ExceptionIfNoDynamicPrimaryKeyDeclared()
        {
            Assert.That(() => _realm.DynamicApi.Find("Person", "Zaphod"), Throws.TypeOf<RealmClassLacksPrimaryKeyException>());
            Assert.That(() => _realm.DynamicApi.Find("Person", 23), Throws.TypeOf<RealmClassLacksPrimaryKeyException>());
            Assert.That(() => _realm.DynamicApi.Find("Person", ObjectId.GenerateNewId()), Throws.TypeOf<RealmClassLacksPrimaryKeyException>());
            Assert.That(() => _realm.DynamicApi.Find("Person", Guid.NewGuid()), Throws.TypeOf<RealmClassLacksPrimaryKeyException>());
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
                    using var realm2 = GetRealm(_configuration);
                    var foundObj = realm2.Find<PrimaryKeyInt64Object>(42000042);
                    foundValue = foundObj.Int64Property;
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
            var conf = ((RealmConfiguration)RealmConfiguration.DefaultConfiguration).ConfigWithPath(Guid.NewGuid().ToString());
            conf.ObjectClasses = new[] { typeof(Person) };

            using var skinny = GetRealm(conf);
            Assert.That(() => skinny.Find<PrimaryKeyInt64Object>(42), Throws.TypeOf<KeyNotFoundException>());
        }

        [Test]
        public void StringPrimaryKey_WhenRequiredDoesntAllowNull()
        {
            Assert.That(() => _realm.Write(() => _realm.Add(new RequiredPrimaryKeyStringObject())), Throws.TypeOf<ArgumentException>());
        }
    }
}