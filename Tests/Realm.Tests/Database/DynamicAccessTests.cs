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
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;
using MongoDB.Bson;
using NUnit.Framework;
using Realms.Dynamic;
using Realms.Exceptions;
using Realms.Helpers;

namespace Realms.Tests.Database
{
    [TestFixture]
    [Preserve(AllMembers = true)]
    public class DynamicAccessTests : RealmInstanceTest
    {
        private void RunTestInAllModes(Action<Realm, bool> test)
        {
            foreach (var isDynamic in new[] { true, false })
            {
                var config = new RealmConfiguration(Guid.NewGuid().ToString())
                {
                    Schema = new[]
                    {
                        typeof(AllTypesObject),
                        typeof(IntPropertyObject),
                        typeof(SyncCollectionsObject),
                        typeof(ObjectWithObjectProperties),
                        typeof(EmbeddedIntPropertyObject)
                    },
                    IsDynamic = isDynamic
                };

                using var realm = GetRealm(config);
                test(realm, isDynamic);
            }
        }

        [Test]
        public void SimpleTest()
        {
            RunTestInAllModes((realm, isDynamic) =>
            {
                var allTypesObject = realm.Write(() =>
                {
                    var ato = realm.DynamicApi.CreateObject(nameof(AllTypesObject));
                    Assert.That(ato, isDynamic ? Is.InstanceOf<DynamicRealmObject>() : Is.InstanceOf<AllTypesObject>());

                    ato.DynamicApi.Set(nameof(AllTypesObject.CharProperty), 'F');
                    ato.DynamicApi.Set(nameof(AllTypesObject.NullableCharProperty), 'o');
                    ato.DynamicApi.Set(nameof(AllTypesObject.StringProperty), "o");

                    return ato;
                });

                Assert.That(allTypesObject.DynamicApi.Get<char>(nameof(AllTypesObject.CharProperty)), Is.EqualTo('F'));
                Assert.That(allTypesObject.DynamicApi.Get<char?>(nameof(AllTypesObject.NullableCharProperty)), Is.EqualTo('o'));
                Assert.That(allTypesObject.DynamicApi.Get<string>(nameof(AllTypesObject.StringProperty)), Is.EqualTo("o"));

#if !UNITY
                var dynamicAto = realm.Write(() =>
                {
                    dynamic ato = realm.DynamicApi.CreateObject(nameof(AllTypesObject));
                    Assert.That(ato, isDynamic ? Is.InstanceOf<DynamicRealmObject>() : Is.InstanceOf<AllTypesObject>());

                    ato.CharProperty = 'F';
                    ato.NullableCharProperty = 'o';
                    ato.StringProperty = "o";

                    return ato;
                });

                Assert.That((char)dynamicAto.CharProperty, Is.EqualTo('F'));
                Assert.That((char)dynamicAto.NullableCharProperty, Is.EqualTo('o'));
                Assert.That(dynamicAto.StringProperty, Is.EqualTo("o"));
#endif
            });
        }

        [Test]
        public void DynamicApi_WhenObjectIsUnmanaged_Throws()
        {
            var ato = new AllTypesObject();
            Assert.That(() => _ = ato.DynamicApi, Throws.TypeOf<NotSupportedException>());
        }

#if !UNITY // Unity doesn't support generic test cases
        [TestCaseSource(typeof(AccessTests), nameof(AccessTests.SetAndGetValueCases))]
#endif
        public void SetAndGetValue<T>(string propertyName, T propertyValue)
        {
            RunTestInAllModes((realm, isDynamic) =>
            {
                dynamic allTypesObject;
                using (var transaction = realm.BeginWrite())
                {
                    allTypesObject = realm.DynamicApi.CreateObject(nameof(AllTypesObject));

                    InvokeSetter(allTypesObject, propertyName, propertyValue, isDynamic);
                    transaction.Commit();
                }

                Assert.That(Operator.Convert<T>(InvokeGetter(allTypesObject, propertyName, isDynamic)), Is.EqualTo(propertyValue));
            });
        }

        [TestCaseSource(typeof(AccessTests), nameof(AccessTests.SetAndGetValueCases))]
        public void SetAndGetValue_NewAPI(string propertyName, object propertyValue)
        {
            var realmValue = Operator.Convert<RealmValue>(propertyValue);

            RunTestInAllModes((realm, _) =>
            {
                var allTypesObject = realm.Write(() =>
                {
                    var ato = realm.DynamicApi.CreateObject(nameof(AllTypesObject));

                    ato.DynamicApi.Set(propertyName, realmValue);

                    return ato;
                });

                Assert.That(allTypesObject.DynamicApi.Get<RealmValue>(propertyName), Is.EqualTo(realmValue));
                Assert.That(allTypesObject.DynamicApi.Get(propertyName), Is.EqualTo(realmValue));
            });
        }

#if !UNITY // Unity doesn't support generic test cases
        [TestCaseSource(typeof(AccessTests), nameof(AccessTests.SetAndReplaceWithNullCases))]
#endif
        public void SetValueAndReplaceWithNull<T>(string propertyName, T propertyValue)
        {
            RunTestInAllModes((realm, isDynamic) =>
            {
                var allTypesObject = realm.Write(() =>
                {
                    dynamic ato = realm.DynamicApi.CreateObject(nameof(AllTypesObject));

                    InvokeSetter(ato, propertyName, propertyValue, isDynamic);

                    return ato;
                });

                Assert.That((T)InvokeGetter(allTypesObject, propertyName, isDynamic), Is.EqualTo(propertyValue));

                realm.Write(() =>
                {
                    InvokeSetter<object>(allTypesObject, propertyName, null, isDynamic);
                });

                Assert.That(InvokeGetter(allTypesObject, propertyName, isDynamic), Is.EqualTo(null));
            });
        }

        [TestCaseSource(typeof(AccessTests), nameof(AccessTests.SetAndReplaceWithNullCases))]
        public void SetValueAndReplaceWithNull_NewAPI(string propertyName, object propertyValue)
        {
            var realmValue = Operator.Convert<RealmValue>(propertyValue);

            RunTestInAllModes((realm, _) =>
            {
                var allTypesObject = realm.Write(() =>
                {
                    var ato = realm.DynamicApi.CreateObject(nameof(AllTypesObject));

                    ato.DynamicApi.Set(propertyName, realmValue);

                    return ato;
                });

                Assert.That(allTypesObject.DynamicApi.Get<RealmValue>(propertyName), Is.EqualTo(realmValue));

                realm.Write(() =>
                {
                    allTypesObject.DynamicApi.Set(propertyName, RealmValue.Null);
                });

                Assert.That(allTypesObject.DynamicApi.Get<object>(propertyName), Is.EqualTo(null));
            });
        }

        public static RealmValue[] RealmValues = new[]
        {
            RealmValue.Null,
            RealmValue.Create(10, RealmValueType.Int),
            RealmValue.Create(true, RealmValueType.Bool),
            RealmValue.Create("abc", RealmValueType.String),
            RealmValue.Create(new byte[] { 0, 1, 2 }, RealmValueType.Data),
            RealmValue.Create(DateTimeOffset.FromUnixTimeSeconds(1616137641), RealmValueType.Date),
            RealmValue.Create(1.5f, RealmValueType.Float),
            RealmValue.Create(2.5d, RealmValueType.Double),
            RealmValue.Create(5m, RealmValueType.Decimal128),
            RealmValue.Create(new ObjectId("5f63e882536de46d71877979"), RealmValueType.ObjectId),
            RealmValue.Create(new Guid("{F2952191-A847-41C3-8362-497F92CB7D24}"), RealmValueType.Guid),
        };

        [Test]
        public void RealmValueTests([ValueSource(nameof(RealmValues))] RealmValue rv)
        {
            RunTestInAllModes((realm, _) =>
            {
                var allTypesObject = realm.Write(() =>
                {
                    var ato = realm.DynamicApi.CreateObject(nameof(AllTypesObject));

                    ato.DynamicApi.Set(nameof(AllTypesObject.RealmValueProperty), rv);

                    return ato;
                });

                Assert.That(allTypesObject.DynamicApi.Get<RealmValue>(nameof(AllTypesObject.RealmValueProperty)), Is.EqualTo(rv));

#if !UNITY
                var dynamicAto = realm.Write(() =>
                {
                    dynamic ato = realm.DynamicApi.CreateObject(nameof(AllTypesObject));

                    ato.RealmValueProperty = rv;

                    return ato;
                });

                Assert.That((RealmValue)dynamicAto.RealmValueProperty, Is.EqualTo(rv));
#endif
            });
        }

        [Test]
        public void RealmValueTests_WithObject()
        {
            RunTestInAllModes((realm, _) =>
            {
                var (ato, rv) = realm.Write(() =>
                {
                    var intObject = realm.DynamicApi.CreateObject(nameof(IntPropertyObject), ObjectId.GenerateNewId());
                    intObject.DynamicApi.Set(nameof(IntPropertyObject.Int), 10);

                    var allTypesObject = realm.DynamicApi.CreateObject(nameof(AllTypesObject));
                    allTypesObject.DynamicApi.Set(nameof(AllTypesObject.RealmValueProperty), RealmValue.Object(intObject));
                    return (allTypesObject, RealmValue.Object(intObject));
                });

                Assert.That(ato.DynamicApi.Get<RealmValue>(nameof(AllTypesObject.RealmValueProperty)), Is.EqualTo(rv));

#if !UNITY
                var (dynamicAto, dynamicRV) = realm.Write(() =>
                {
                    dynamic intObject = realm.DynamicApi.CreateObject(nameof(IntPropertyObject), ObjectId.GenerateNewId());
                    intObject.Int = 10;
                    RealmValue intObjectRV = RealmValue.Object(intObject);

                    dynamic ato = realm.DynamicApi.CreateObject(nameof(AllTypesObject));
                    ato.RealmValueProperty = intObjectRV;
                    return (ato, intObjectRV);
                });

                Assert.That((RealmValue)dynamicAto.RealmValueProperty, Is.EqualTo(dynamicRV));
#endif
            });
        }

        #region Dynamic.Get<T>

        [Test]
        public void GetProperty_WhenIncorrectType_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                var allTypesObject = realm.Write(() => realm.DynamicApi.CreateObject(nameof(AllTypesObject)));

                Assert.Throws<InvalidCastException>(() => allTypesObject.DynamicApi.Get<string>(nameof(AllTypesObject.Int32Property)));
            });
        }

        [Test]
        public void GetProperty_WhenUsingRealmValue_Succeeds()
        {
            RunTestInAllModes((realm, _) =>
            {
                var allTypesObject = realm.Write(() =>
                {
                    var result = realm.DynamicApi.CreateObject(nameof(AllTypesObject));
                    result.DynamicApi.Set(nameof(AllTypesObject.Int32Property), 123);
                    return result;
                });

                var value = allTypesObject.DynamicApi.Get<RealmValue>(nameof(AllTypesObject.Int32Property));

                Assert.That(value.Type, Is.EqualTo(RealmValueType.Int));
                Assert.That(value.AsAny(), Is.EqualTo(123));
            });
        }

        [Test]
        public void GetProperty_WhenUsingSystemObject_Succeeds()
        {
            RunTestInAllModes((realm, _) =>
            {
                var allTypesObject = realm.Write(() =>
                {
                    var result = realm.DynamicApi.CreateObject(nameof(AllTypesObject));
                    result.DynamicApi.Set(nameof(AllTypesObject.Int32Property), 123);
                    return result;
                });

                var value = allTypesObject.DynamicApi.Get<object>(nameof(AllTypesObject.Int32Property));

                Assert.That(value, Is.EqualTo(123));
            });
        }

        [Test]
        public void GetProperty_WhenUsingConvertibleType_Succeeds()
        {
            RunTestInAllModes((realm, _) =>
            {
                var allTypesObject = realm.Write(() =>
                {
                    var result = realm.DynamicApi.CreateObject(nameof(AllTypesObject));
                    result.DynamicApi.Set(nameof(AllTypesObject.Int32Property), 123);
                    return result;
                });

                var longValue = allTypesObject.DynamicApi.Get<long>(nameof(AllTypesObject.Int32Property));
                Assert.That(longValue, Is.EqualTo(123L));

                var shortValue = allTypesObject.DynamicApi.Get<short>(nameof(AllTypesObject.Int32Property));
                Assert.That(shortValue, Is.EqualTo((short)123));
            });
        }

        [Test]
        public void GetRealmObjectProperty_WhenCastToRealmObject_Succeeds()
        {
            RunTestInAllModes((realm, _) =>
            {
                var allTypesObject = realm.Write(() =>
                {
                    var intPropObject = realm.DynamicApi.CreateObject(nameof(IntPropertyObject), ObjectId.GenerateNewId());
                    intPropObject.DynamicApi.Set(nameof(IntPropertyObject.Int), 456);

                    var result = realm.DynamicApi.CreateObject(nameof(ObjectWithObjectProperties));
                    result.DynamicApi.Set(nameof(ObjectWithObjectProperties.StandaloneObject), RealmValue.Object(intPropObject));

                    return result;
                });

                var intObj = allTypesObject.DynamicApi.Get<IRealmObject>(nameof(ObjectWithObjectProperties.StandaloneObject));
                Assert.That(intObj, Is.Not.Null);

                Assert.That(intObj.DynamicApi.Get<long>(nameof(IntPropertyObject.Int)), Is.EqualTo(456));
            });
        }

        [Test]
        public void GetRealmObjectProperty_WhenCastToRealmObjectBase_Succeeds()
        {
            RunTestInAllModes((realm, _) =>
            {
                var allTypesObject = realm.Write(() =>
                {
                    var intPropObject = realm.DynamicApi.CreateObject(nameof(IntPropertyObject), ObjectId.GenerateNewId());
                    intPropObject.DynamicApi.Set(nameof(IntPropertyObject.Int), 456);

                    var result = realm.DynamicApi.CreateObject(nameof(ObjectWithObjectProperties));
                    result.DynamicApi.Set(nameof(ObjectWithObjectProperties.StandaloneObject), RealmValue.Object(intPropObject));

                    return result;
                });

                var intObj = allTypesObject.DynamicApi.Get<IRealmObjectBase>(nameof(ObjectWithObjectProperties.StandaloneObject));
                Assert.That(intObj, Is.Not.Null);

                Assert.That(intObj.DynamicApi.Get<long>(nameof(IntPropertyObject.Int)), Is.EqualTo(456));
            });
        }

        [Test]
        public void GetRealmObjectProperty_WhenCastToEmbeddedObject_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                var allTypesObject = realm.Write(() =>
                {
                    var intPropObject = realm.DynamicApi.CreateObject(nameof(IntPropertyObject), ObjectId.GenerateNewId());
                    intPropObject.DynamicApi.Set(nameof(IntPropertyObject.Int), 456);

                    var result = realm.DynamicApi.CreateObject(nameof(ObjectWithObjectProperties));
                    result.DynamicApi.Set(nameof(ObjectWithObjectProperties.StandaloneObject), RealmValue.Object(intPropObject));

                    return result;
                });

                Assert.Throws<InvalidCastException>(() => allTypesObject.DynamicApi.Get<IEmbeddedObject>(nameof(ObjectWithObjectProperties.StandaloneObject)));
            });
        }

        [Test]
        public void GetEmbeddedObjectProperty_WhenCastToEmbeddedObject_Succeeds()
        {
            RunTestInAllModes((realm, _) =>
            {
                var allTypesObject = realm.Write(() =>
                {
                    var result = realm.DynamicApi.CreateObject(nameof(ObjectWithObjectProperties));
                    var intPropObject = realm.DynamicApi.CreateEmbeddedObjectForProperty(result, nameof(ObjectWithObjectProperties.EmbeddedObject));
                    intPropObject.DynamicApi.Set(nameof(EmbeddedIntPropertyObject.Int), 456);

                    return result;
                });

                var intObj = allTypesObject.DynamicApi.Get<IEmbeddedObject>(nameof(ObjectWithObjectProperties.EmbeddedObject));
                Assert.That(intObj, Is.Not.Null);

                Assert.That(intObj.DynamicApi.Get<long>(nameof(EmbeddedIntPropertyObject.Int)), Is.EqualTo(456));
            });
        }

        [Test]
        public void GetEmbeddedObjectProperty_WhenCastToRealmObjectBase_Succeeds()
        {
            RunTestInAllModes((realm, _) =>
            {
                var allTypesObject = realm.Write(() =>
                {
                    var result = realm.DynamicApi.CreateObject(nameof(ObjectWithObjectProperties));
                    var intPropObject = realm.DynamicApi.CreateEmbeddedObjectForProperty(result, nameof(ObjectWithObjectProperties.EmbeddedObject));
                    intPropObject.DynamicApi.Set(nameof(EmbeddedIntPropertyObject.Int), 456);

                    return result;
                });

                var intObj = allTypesObject.DynamicApi.Get<IEmbeddedObject>(nameof(ObjectWithObjectProperties.EmbeddedObject));
                Assert.That(intObj, Is.Not.Null);

                Assert.That(intObj.DynamicApi.Get<long>(nameof(EmbeddedIntPropertyObject.Int)), Is.EqualTo(456));
            });
        }

        [Test]
        public void GetEmbeddedObjectProperty_WhenCastToRealmObject_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                var allTypesObject = realm.Write(() =>
                {
                    var result = realm.DynamicApi.CreateObject(nameof(ObjectWithObjectProperties));
                    var intPropObject = realm.DynamicApi.CreateEmbeddedObjectForProperty(result, nameof(ObjectWithObjectProperties.EmbeddedObject));
                    intPropObject.DynamicApi.Set(nameof(EmbeddedIntPropertyObject.Int), 456);

                    return result;
                });

                Assert.Throws<InvalidCastException>(() => allTypesObject.DynamicApi.Get<IRealmObject>(nameof(ObjectWithObjectProperties.EmbeddedObject)));
            });
        }

        [Test]
        public void GetProperty_WhenPropertyIsBacklinks_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                var allTypesObject = realm.Write(() => realm.DynamicApi.CreateObject(nameof(IntPropertyObject), ObjectId.GenerateNewId()));

                var ex = Assert.Throws<NotSupportedException>(() => allTypesObject.DynamicApi.Get<RealmValue>(nameof(IntPropertyObject.ContainingCollections)))!;
                Assert.That(ex.Message, Does.Contain("IQueryable<SyncCollectionsObject>").And.Contains("GetBacklinks"));
            });
        }

        [Test]
        public void GetProperty_WhenPropertyIsList_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                var allTypesObject = realm.Write(() => realm.DynamicApi.CreateObject(nameof(SyncCollectionsObject), ObjectId.GenerateNewId()));

                var ex = Assert.Throws<NotSupportedException>(() => allTypesObject.DynamicApi.Get<RealmValue>(nameof(SyncCollectionsObject.ObjectIdList)))!;
                Assert.That(ex.Message, Does.Contain("IList<ObjectId>").And.Contains("GetList"));
            });
        }

        [Test]
        public void GetProperty_WhenPropertyIsSet_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                var allTypesObject = realm.Write(() => realm.DynamicApi.CreateObject(nameof(SyncCollectionsObject), ObjectId.GenerateNewId()));

                var ex = Assert.Throws<NotSupportedException>(() => allTypesObject.DynamicApi.Get<RealmValue>(nameof(SyncCollectionsObject.BooleanSet)))!;
                Assert.That(ex.Message, Does.Contain("ISet<Boolean>").And.Contains("GetSet"));
            });
        }

        [Test]
        public void GetProperty_WhenPropertyIsDictionary_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                var allTypesObject = realm.Write(() => realm.DynamicApi.CreateObject(nameof(SyncCollectionsObject), ObjectId.GenerateNewId()));

                var ex = Assert.Throws<NotSupportedException>(() => allTypesObject.DynamicApi.Get<RealmValue>(nameof(SyncCollectionsObject.DecimalDict)))!;
                Assert.That(ex.Message, Does.Contain("IDictionary<string, Decimal128>").And.Contains("GetDictionary"));
            });
        }

        #endregion Dynamic.Get<T>

        #region Dynamic.Set

        [Test]
        public void SetProperty_WhenIncorrectType_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                realm.Write(() =>
                {
                    var ato = realm.DynamicApi.CreateObject(nameof(AllTypesObject));
                    Assert.Throws<ArgumentException>(() => ato.DynamicApi.Set(nameof(AllTypesObject.Int32Property), "abc"));
                });
            });
        }

        [Test]
        public void SetProperty_WhenUsingConvertibleType_Succeeds()
        {
            RunTestInAllModes((realm, _) =>
            {
                realm.Write(() =>
                {
                    var ato = realm.DynamicApi.CreateObject(nameof(AllTypesObject));
                    ato.DynamicApi.Set(nameof(AllTypesObject.Int32Property), 123);
                    ato.DynamicApi.Set(nameof(AllTypesObject.Int32Property), 9999L);
                    ato.DynamicApi.Set(nameof(AllTypesObject.Int32Property), (short)5);
                });
            });
        }

        [Test]
        public void SetProperty_WhenPropertyIsBacklinks_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                realm.Write(() =>
                {
                    var ato = realm.DynamicApi.CreateObject(nameof(IntPropertyObject), ObjectId.GenerateNewId());
                    var ex = Assert.Throws<NotSupportedException>(() => ato.DynamicApi.Set(nameof(IntPropertyObject.ContainingCollections), 123))!;
                    Assert.That(ex.Message, Does.Contain("IQueryable<SyncCollectionsObject>").And.Contains("can't be set directly"));
                });
            });
        }

        [Test]
        public void SetProperty_WhenPropertyIsList_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                realm.Write(() =>
                {
                    var ato = realm.DynamicApi.CreateObject(nameof(SyncCollectionsObject), ObjectId.GenerateNewId());

                    var ex = Assert.Throws<NotSupportedException>(() => ato.DynamicApi.Set(nameof(SyncCollectionsObject.ObjectIdList), 123))!;
                    Assert.That(ex.Message, Does.Contain("IList<ObjectId>").And.Contains("can't be set directly"));
                });
            });
        }

        [Test]
        public void SetProperty_WhenPropertyIsSet_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                realm.Write(() =>
                {
                    var ato = realm.DynamicApi.CreateObject(nameof(SyncCollectionsObject), ObjectId.GenerateNewId());
                    var ex = Assert.Throws<NotSupportedException>(() => ato.DynamicApi.Set(nameof(SyncCollectionsObject.BooleanSet), 123))!;
                    Assert.That(ex.Message, Does.Contain("ISet<Boolean>").And.Contains("can't be set directly"));
                });
            });
        }

        [Test]
        public void SetProperty_WhenPropertyIsDictionary_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                realm.Write(() =>
                {
                    var ato = realm.DynamicApi.CreateObject(nameof(SyncCollectionsObject), ObjectId.GenerateNewId());
                    var ex = Assert.Throws<NotSupportedException>(() => ato.DynamicApi.Set(nameof(SyncCollectionsObject.DecimalDict), 123))!;
                    Assert.That(ex.Message, Does.Contain("IDictionary<string, Decimal128>").And.Contains("can't be set directly"));
                });
            });
        }

        [Test]
        public void SetPropertyToNull_WhenPropertyIsNotNullable_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                realm.Write(() =>
                {
                    var ato = realm.DynamicApi.CreateObject(nameof(AllTypesObject));

                    var ex = Assert.Throws<ArgumentException>(() => ato.DynamicApi.Set(nameof(AllTypesObject.RequiredStringProperty), RealmValue.Null))!;
                    Assert.That(ex.Message.Contains("not nullable"));
                });
            });
        }

        #endregion Dynamic.Set

        #region Dynamic.GetBacklinks

        [Test]
        public void GetBacklinks_WhenPropertyIsMissing_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                var obj = realm.Write(() => realm.DynamicApi.CreateObject(nameof(IntPropertyObject), ObjectId.GenerateNewId()));

                Assert.Throws<MissingMemberException>(() => obj.DynamicApi.GetBacklinks("idontexist"));
            });
        }

        [Test]
        public void GetBacklinks_WhenPropertyIsWrongType_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                var obj = realm.Write(() => realm.DynamicApi.CreateObject(nameof(IntPropertyObject), ObjectId.GenerateNewId()));

                var ex = Assert.Throws<ArgumentException>(() => obj.DynamicApi.GetBacklinks(nameof(IntPropertyObject.Int)))!;
                Assert.That(ex.Message, Does.Contain("Int64").And.Contains(nameof(DynamicObjectApi.GetBacklinks)));
            });
        }

        [Test]
        public void GetBacklinks_WhenPropertyExists_ReturnsBacklinks()
        {
            RunTestInAllModes((realm, _) =>
            {
                var obj = realm.Write(() =>
                {
                    var intPropObj = (IRealmObject)realm.DynamicApi.CreateObject(nameof(IntPropertyObject), ObjectId.GenerateNewId());

                    var collectionsObj = realm.DynamicApi.CreateObject(nameof(SyncCollectionsObject), ObjectId.GenerateNewId());
                    var objectList = collectionsObj.DynamicApi.GetList<IRealmObject>(nameof(SyncCollectionsObject.ObjectList));
                    objectList.Add(intPropObj);

                    return intPropObj;
                });

                var backlinks = obj.DynamicApi.GetBacklinks(nameof(IntPropertyObject.ContainingCollections));

                Assert.That(backlinks, Is.Not.Null);
                Assert.That(backlinks.Count(), Is.EqualTo(1));

                var collectionsObj = realm.DynamicApi.All(nameof(SyncCollectionsObject)).Single();
                Assert.That(backlinks.Single(), Is.EqualTo(collectionsObj));
            });
        }

        [Test]
        public void GetBacklinksFromType_WhenFromTypeIsInvalid_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                var obj = realm.Write(() => realm.DynamicApi.CreateObject(nameof(IntPropertyObject), ObjectId.GenerateNewId()));

                var ex = Assert.Throws<ArgumentException>(() => obj.DynamicApi.GetBacklinksFromType("idontexist", "someprop"))!;
                Assert.That(ex.Message, Does.Contain("idontexist"));
            });
        }

        [Test]
        public void GetBacklinksFromType_WhenFromPropertyIsMissing_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                var obj = realm.Write(() => realm.DynamicApi.CreateObject(nameof(IntPropertyObject), ObjectId.GenerateNewId()));

                var ex = Assert.Throws<MissingMemberException>(() => obj.DynamicApi.GetBacklinksFromType(nameof(SyncCollectionsObject), "someprop"))!;
                Assert.That(ex.Message, Does.Contain("someprop"));
            });
        }

        [Test]
        public void GetBacklinksFromType_WhenFromPropertyExists_Succeeds()
        {
            RunTestInAllModes((realm, _) =>
            {
                var obj = realm.Write(() =>
                {
                    var intPropObj = (IRealmObject)realm.DynamicApi.CreateObject(nameof(IntPropertyObject), ObjectId.GenerateNewId());

                    var collectionsObj = realm.DynamicApi.CreateObject(nameof(SyncCollectionsObject), ObjectId.GenerateNewId());
                    var objectSet = collectionsObj.DynamicApi.GetSet<IRealmObject>(nameof(SyncCollectionsObject.ObjectSet));
                    objectSet.Add(intPropObj);

                    return intPropObj;
                });

                var backlinks = obj.DynamicApi.GetBacklinksFromType(nameof(SyncCollectionsObject), nameof(SyncCollectionsObject.ObjectSet));
                Assert.That(backlinks, Is.Not.Null);
                Assert.That(backlinks.Count(), Is.EqualTo(1));

                var collectionsObj = realm.DynamicApi.All(nameof(SyncCollectionsObject)).Single();
                Assert.That(backlinks.Single(), Is.EqualTo(collectionsObj));
            });
        }

        #endregion Dynamic.GetBacklinks

        #region Dynamic.GetList

        [Test]
        public void GetList_WhenPropertyIsNull_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                var obj = realm.Write(() => realm.DynamicApi.CreateObject(nameof(SyncCollectionsObject), ObjectId.GenerateNewId()));

                var ex = Assert.Throws<ArgumentNullException>(() => obj.DynamicApi.GetList<RealmValue>(null!))!;
                Assert.That(ex.ParamName, Is.EqualTo("propertyName"));
            });
        }

        [Test]
        public void GetList_WhenPropertyIsMissing_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                var obj = realm.Write(() => realm.DynamicApi.CreateObject(nameof(SyncCollectionsObject), ObjectId.GenerateNewId()));

                var ex = Assert.Throws<MissingMemberException>(() => obj.DynamicApi.GetList<RealmValue>("someprop"))!;
                Assert.That(ex.Message, Does.Contain("someprop"));
            });
        }

        [Test]
        public void GetList_WhenPropertyIsNotList_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                var obj = realm.Write(() => realm.DynamicApi.CreateObject(nameof(SyncCollectionsObject), ObjectId.GenerateNewId()));

                var ex = Assert.Throws<ArgumentException>(() => obj.DynamicApi.GetList<RealmValue>(nameof(SyncCollectionsObject.BooleanSet)))!;
                Assert.That(ex.Message, Does.Contain("ISet<Boolean>").And.Contain($"can't be accessed using {nameof(DynamicObjectApi.GetList)}"));
            });
        }

        [Test]
        public void GetList_WhenCastToRealmValue_Works()
        {
            RunTestInAllModes((realm, _) =>
            {
                var obj = realm.Write(() => realm.DynamicApi.CreateObject(nameof(SyncCollectionsObject), ObjectId.GenerateNewId()));

                var stringList = obj.DynamicApi.GetList<RealmValue>(nameof(SyncCollectionsObject.StringList));

                realm.Write(() =>
                {
                    stringList.Add("abc");
                });

                Assert.That(stringList.Count, Is.EqualTo(1));
                Assert.That(stringList[0].Type, Is.EqualTo(RealmValueType.String));
                Assert.That(stringList[0].AsAny(), Is.EqualTo("abc"));

                realm.Write(() =>
                {
                    var ex = Assert.Throws<RealmException>(() => stringList.Add(123))!;
                    Assert.That(ex.Message, Does.Contain("type mismatch"));
                });
            });
        }

        [Test]
        public void GetList_WhenCastToWrongValue_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                var obj = realm.Write(() => realm.DynamicApi.CreateObject(nameof(SyncCollectionsObject), ObjectId.GenerateNewId()));

                var stringList = obj.DynamicApi.GetList<int>(nameof(SyncCollectionsObject.StringList));

                realm.Write(() =>
                {
                    var ex = Assert.Throws<RealmException>(() => stringList.Add(123))!;
                    Assert.That(ex.Message, Does.Contain("type mismatch"));
                });
            });
        }

        #endregion Dynamic.GetList

        #region Dynamic.GetSet

        [Test]
        public void GetSet_WhenPropertyIsNull_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                var obj = realm.Write(() => realm.DynamicApi.CreateObject(nameof(SyncCollectionsObject), ObjectId.GenerateNewId()));

                var ex = Assert.Throws<ArgumentNullException>(() => obj.DynamicApi.GetSet<RealmValue>(null!))!;
                Assert.That(ex.ParamName, Is.EqualTo("propertyName"));
            });
        }

        [Test]
        public void GetSet_WhenPropertyIsMissing_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                var obj = realm.Write(() => realm.DynamicApi.CreateObject(nameof(SyncCollectionsObject), ObjectId.GenerateNewId()));

                var ex = Assert.Throws<MissingMemberException>(() => obj.DynamicApi.GetSet<RealmValue>("someprop"))!;
                Assert.That(ex.Message, Does.Contain("someprop"));
            });
        }

        [Test]
        public void GetSet_WhenPropertyIsNotSet_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                var obj = realm.Write(() => realm.DynamicApi.CreateObject(nameof(SyncCollectionsObject), ObjectId.GenerateNewId()));

                var ex = Assert.Throws<ArgumentException>(() => obj.DynamicApi.GetSet<RealmValue>(nameof(SyncCollectionsObject.StringList)))!;
                Assert.That(ex.Message, Does.Contain("IList<String>").And.Contain($"can't be accessed using {nameof(DynamicObjectApi.GetSet)}"));
            });
        }

        [Test]
        public void GetSet_WhenCastToRealmValue_Works()
        {
            RunTestInAllModes((realm, _) =>
            {
                var obj = realm.Write(() => realm.DynamicApi.CreateObject(nameof(SyncCollectionsObject), ObjectId.GenerateNewId()));

                var stringSet = obj.DynamicApi.GetSet<RealmValue>(nameof(SyncCollectionsObject.StringSet));

                realm.Write(() =>
                {
                    stringSet.Add("abc");
                });

                Assert.That(stringSet.Count, Is.EqualTo(1));
                Assert.That(stringSet.Single().Type, Is.EqualTo(RealmValueType.String));
                Assert.That(stringSet.Single().AsAny(), Is.EqualTo("abc"));

                realm.Write(() =>
                {
                    var ex = Assert.Throws<RealmException>(() => stringSet.Add(123))!;
                    Assert.That(ex.Message, Does.Contain("type mismatch"));
                });
            });
        }

        [Test]
        public void GetSet_WhenCastToWrongValue_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                var obj = realm.Write(() => realm.DynamicApi.CreateObject(nameof(SyncCollectionsObject), ObjectId.GenerateNewId()));

                var stringSet = obj.DynamicApi.GetSet<int>(nameof(SyncCollectionsObject.StringSet));

                realm.Write(() =>
                {
                    var ex = Assert.Throws<RealmException>(() => stringSet.Add(123))!;
                    Assert.That(ex.Message, Does.Contain("type mismatch"));
                });
            });
        }

        #endregion Dynamic.GetSet

        #region Dynamic.GetDictionary

        [Test]
        public void GetDictionary_WhenPropertyIsNull_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                var obj = realm.Write(() => realm.DynamicApi.CreateObject(nameof(SyncCollectionsObject), ObjectId.GenerateNewId()));

                var ex = Assert.Throws<ArgumentNullException>(() => obj.DynamicApi.GetDictionary<RealmValue>(null!))!;
                Assert.That(ex.ParamName, Is.EqualTo("propertyName"));
            });
        }

        [Test]
        public void GetDictionary_WhenPropertyIsMissing_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                var obj = realm.Write(() => realm.DynamicApi.CreateObject(nameof(SyncCollectionsObject), ObjectId.GenerateNewId()));

                var ex = Assert.Throws<MissingMemberException>(() => obj.DynamicApi.GetDictionary<RealmValue>("someprop"))!;
                Assert.That(ex.Message, Does.Contain("someprop"));
            });
        }

        [Test]
        public void GetDictionary_WhenPropertyIsNotDictionary_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                var obj = realm.Write(() => realm.DynamicApi.CreateObject(nameof(SyncCollectionsObject), ObjectId.GenerateNewId()));

                var ex = Assert.Throws<ArgumentException>(() => obj.DynamicApi.GetDictionary<RealmValue>(nameof(SyncCollectionsObject.StringList)))!;
                Assert.That(ex.Message, Does.Contain("IList<String>").And.Contain($"can't be accessed using {nameof(DynamicObjectApi.GetDictionary)}"));
            });
        }

        [Test]
        public void GetDictionary_WhenCastToRealmValue_Works()
        {
            RunTestInAllModes((realm, _) =>
            {
                var obj = realm.Write(() => realm.DynamicApi.CreateObject(nameof(SyncCollectionsObject), ObjectId.GenerateNewId()));

                var stringDictionary = obj.DynamicApi.GetDictionary<RealmValue>(nameof(SyncCollectionsObject.StringDict));

                realm.Write(() =>
                {
                    stringDictionary.Add("abc", "cde");
                });

                Assert.That(stringDictionary.Count, Is.EqualTo(1));
                Assert.That(stringDictionary.Single().Value.Type, Is.EqualTo(RealmValueType.String));
                Assert.That(stringDictionary.Single().Value.AsAny(), Is.EqualTo("cde"));
                Assert.That(stringDictionary.Single().Key, Is.EqualTo("abc"));

                realm.Write(() =>
                {
                    var ex = Assert.Throws<RealmException>(() => stringDictionary.Add("cde", 123))!;
                    Assert.That(ex.Message, Does.Contain("type mismatch"));
                });
            });
        }

        [Test]
        public void GetDictionary_WhenCastToWrongValue_Throws()
        {
            RunTestInAllModes((realm, _) =>
            {
                var obj = realm.Write(() => realm.DynamicApi.CreateObject(nameof(SyncCollectionsObject), ObjectId.GenerateNewId()));

                var stringDictionary = obj.DynamicApi.GetDictionary<int>(nameof(SyncCollectionsObject.StringDict));

                realm.Write(() =>
                {
                    var ex = Assert.Throws<RealmException>(() => stringDictionary.Add("abc", 123))!;
                    Assert.That(ex.Message, Does.Contain("type mismatch"));
                });
            });
        }

        #endregion

        #region Flexible schema

        [Test]
        public void FlexibleSchema_BaseTest()
        {
            _configuration.RelaxedSchema = true;
            var realm = GetRealm(_configuration);

            var person = realm.Write(() =>
            {
                return realm.Add(new Person());
            });

            var testObj = new Person { FirstName = "Luigi" };
            var testList = new List<RealmValue> { 1, "test", true };

            //Additional properties should be empty in the beginning
            Assert.That(person.ExtendedObjectSchema.ExtraProperties, Is.Empty);

            Assert.That(person.ExtendedObjectSchema.HasProperty("propString"), Is.False);

            // Basic set/get
            realm.Write(() =>
            {
                person.DynamicApi.Set("propString", "testval");
                person.DynamicApi.Set("propInt", 10);
                person.DynamicApi.Set("propObj", testObj);
                person.DynamicApi.Set("propList", testList);
                person.DynamicApi.Set("propNull", RealmValue.Null);
            });

            Assert.That(person.DynamicApi.Get<string>("propString"), Is.EqualTo("testval"));
            Assert.That(person.DynamicApi.Get<int>("propInt"), Is.EqualTo(10));
            Assert.That(person.DynamicApi.Get<Person>("propObj"), Is.EqualTo(testObj));
            Assert.That(person.DynamicApi.Get<IList<RealmValue>>("propList"), Is.EqualTo(testList));
            Assert.That(person.DynamicApi.Get<RealmValue>("propNull"), Is.EqualTo(RealmValue.Null));

            Assert.That(person.ExtendedObjectSchema.HasProperty("propString"), Is.True);

            bool found;

            found = person.DynamicApi.TryGet<string>("propString", out var stringVal);
            Assert.That(found, Is.True);
            Assert.That(stringVal, Is.EqualTo("testval"));

            found = person.DynamicApi.TryGet<IList<RealmValue>>("propList", out var listVal);
            Assert.That(found, Is.True);
            Assert.That(listVal, Is.EqualTo(testList));

            // Change type
            realm.Write(() =>
            {
                person.DynamicApi.Set("propString", 23);
            });

            Assert.That(person.DynamicApi.Get<int>("propString"), Is.EqualTo(23));

            // Get unknown property
            Assert.That(() => person.DynamicApi.Get<int>("unknonProp"), Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Property not found: unknonProp"));
            Assert.That(() => person.DynamicApi.Get("unknonProp"), Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Property not found: unknonProp"));

            // TryGet unknown property
            found = person.DynamicApi.TryGet("unknonProp", out var rvUnKnownValue);
            Assert.That(found, Is.False);
            Assert.That(rvUnKnownValue, Is.EqualTo(RealmValue.Null));

            found = person.DynamicApi.TryGet<int>("unknonProp", out var intUnknownVal);
            Assert.That(found, Is.False);
            Assert.That(intUnknownVal, Is.EqualTo(default(int)));

            found = person.DynamicApi.TryGet<IList<RealmValue>>("unknonProp", out var listUnknonwVal);
            Assert.That(found, Is.False);
            Assert.That(listUnknonwVal, Is.EqualTo(default(IList<RealmValue>)));

            // Unset property
            realm.Write(() =>
            {
                person.DynamicApi.Unset("propString");
            });
            Assert.That(() => person.DynamicApi.Get("propString"), Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Property not found: propString"));

            Assert.That(() => realm.Write(() =>
            {
                person.DynamicApi.Unset("propString");
            }), Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Could not erase property: propString"));

            Assert.That(person.ExtendedObjectSchema.HasProperty("propString"), Is.False);

            // Unset property in schema
            Assert.That(() => realm.Write(() =>
            {
                person.DynamicApi.Unset("FirstName");
            }), Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Could not erase property: FirstName"));

            // TryUnset property
            realm.Write(() =>
            {
                bool unsetVal = person.DynamicApi.TryUnset("propInt");
                Assert.That(unsetVal, Is.True);
            });
            Assert.That(() => person.DynamicApi.Get("propInt"), Throws.TypeOf<ArgumentException>().With.Message.EqualTo("Property not found: propInt"));

            realm.Write(() =>
            {
                bool unsetVal = person.DynamicApi.TryUnset("propInt");
                Assert.That(unsetVal, Is.False);
            });

            // TryUnset property in schema
            // We need to get a new core method to check if a certain property is in the extra
            // properties
            //realm.Write(() =>
            //{
            //    bool unsetVal = person.DynamicApi.TryUnset("FirstName");
            //    Assert.That(unsetVal, Is.False);
            //});

            // Get all extra properties keys
            Assert.That(person.ExtendedObjectSchema.ExtraProperties.Select(p => p.Name), Is.EquivalentTo(new[] { "propObj", "propList", "propNull" }));
        }

        #endregion

        [Test]
        public void GetPrimaryKey_WhenPrivate_Works()
        {
            var config = new RealmConfiguration(Guid.NewGuid().ToString())
            {
                Schema = new[] { typeof(PrivatePrimaryKeyObject) }
            };

            var newObj = new PrivatePrimaryKeyObject();
            var id = newObj.GetId();
            using (var realm = GetRealm(config))
            {
                realm.Write(() =>
                {
                    realm.Add(newObj);
                });
            }

            config.IsDynamic = true;

            using var dynamicRealm = GetRealm(config);
            var dynamicObj = dynamicRealm.DynamicApi.All(nameof(PrivatePrimaryKeyObject)).Single();

            Assert.That(dynamicRealm.Metadata.TryGetValue(nameof(PrivatePrimaryKeyObject), out var meta), Is.True);
            Assert.That(meta!.Helper.TryGetPrimaryKeyValue(dynamicObj, out var foundPk), Is.True);

            Assert.That(foundPk.AsString(), Is.EqualTo(id));
        }

        private static object? InvokeGetter(object o, string propertyName, bool isDynamic)
        {
            if (isDynamic)
            {
                var binder = Binder.GetMember(CSharpBinderFlags.None, propertyName, typeof(DynamicAccessTests), new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
                var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);
                return callsite.Target(callsite, o);
            }

            return TestHelpers.GetPropertyValue(o, propertyName);
        }

        private static void InvokeSetter<T>(object o, string propertyName, T propertyValue, bool isDynamic)
        {
            if (isDynamic)
            {
                var binder = Binder.SetMember(CSharpBinderFlags.None, propertyName, typeof(DynamicAccessTests), new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null) });
                var callsite = CallSite<Func<CallSite, object, T, object>>.Create(binder);
                callsite.Target(callsite, o, propertyValue);
            }
            else
            {
                TestHelpers.SetPropertyValue(o, propertyName, propertyValue);
            }
        }
    }
}
