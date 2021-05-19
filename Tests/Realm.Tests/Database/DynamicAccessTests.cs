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
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;
using MongoDB.Bson;
using NUnit.Framework;
using Realms.Dynamic;
using Realms.Helpers;

namespace Realms.Tests.Database
{
    [TestFixture]
    [Preserve(AllMembers = true)]
    public class DynamicAccessTests : RealmInstanceTest
    {
        private static readonly DynamicTestObjectType[] _testModes = new[]
        {
            DynamicTestObjectType.DynamicRealmObject,
            DynamicTestObjectType.RealmObject
        };

        private void RunTestsWithParameters(Action<Realm, DynamicTestObjectType> test)
        {
            foreach (var mode in _testModes)
            {
                var config = new RealmConfiguration(Guid.NewGuid().ToString())
                {
                    ObjectClasses = new[] { typeof(AllTypesObject), typeof(IntPropertyObject) },
                    IsDynamic = mode == DynamicTestObjectType.DynamicRealmObject
                };

                using var realm = GetRealm(config);
                test(realm, mode);
            }
        }

        [Test]
        public void SimpleTest()
        {
            TestHelpers.IgnoreOnUnity();

            RunTestsWithParameters((realm, mode) =>
            {
                var allTypesObject = realm.Write(() =>
                {
                    dynamic ato = realm.DynamicApi.CreateObject("AllTypesObject", null);
                    if (mode == DynamicTestObjectType.DynamicRealmObject)
                    {
                        Assert.That(ato, Is.InstanceOf<DynamicRealmObject>());
                    }
                    else
                    {
                        Assert.That(ato, Is.InstanceOf<AllTypesObject>());
                    }

                    ato.CharProperty = 'F';
                    ato.NullableCharProperty = 'o';
                    ato.StringProperty = "o";

                    return ato;
                });

                Assert.That((char)allTypesObject.CharProperty, Is.EqualTo('F'));
                Assert.That((char)allTypesObject.NullableCharProperty, Is.EqualTo('o'));
                Assert.That(allTypesObject.StringProperty, Is.EqualTo("o"));
            });
        }

        [Test]
        public void SimpleTest_NewAPI()
        {
            RunTestsWithParameters((realm, mode) =>
            {
                var allTypesObject = realm.Write(() =>
                {
                    var ato = (RealmObject)(object)realm.DynamicApi.CreateObject("AllTypesObject", null);
                    if (mode == DynamicTestObjectType.DynamicRealmObject)
                    {
                        Assert.That(ato, Is.InstanceOf<DynamicRealmObject>());
                    }
                    else
                    {
                        Assert.That(ato, Is.InstanceOf<AllTypesObject>());
                    }

                    ato.DynamicApi.Set(nameof(AllTypesObject.CharProperty), 'F');
                    ato.DynamicApi.Set(nameof(AllTypesObject.NullableCharProperty), 'o');
                    ato.DynamicApi.Set(nameof(AllTypesObject.StringProperty), "o");

                    return ato;
                });

                Assert.That(allTypesObject.DynamicApi.Get<char>(nameof(AllTypesObject.CharProperty)), Is.EqualTo('F'));
                Assert.That(allTypesObject.DynamicApi.Get<char?>(nameof(AllTypesObject.NullableCharProperty)), Is.EqualTo('o'));
                Assert.That(allTypesObject.DynamicApi.Get<string>(nameof(AllTypesObject.StringProperty)), Is.EqualTo("o"));
            });
        }

#if !UNITY
        [TestCaseSource(typeof(AccessTests), nameof(AccessTests.SetAndGetValueCases))]
#endif
        public void SetAndGetValue<T>(string propertyName, T propertyValue)
        {
            TestHelpers.IgnoreOnUnity();

            RunTestsWithParameters((realm, mode) =>
            {
                object allTypesObject;
                using (var transaction = realm.BeginWrite())
                {
                    allTypesObject = realm.DynamicApi.CreateObject("AllTypesObject", null);

                    InvokeSetter(allTypesObject, propertyName, propertyValue, mode);
                    transaction.Commit();
                }

                Assert.That((T)InvokeGetter(allTypesObject, propertyName, mode), Is.EqualTo(propertyValue));
            });
        }

        [TestCaseSource(typeof(AccessTests), nameof(AccessTests.SetAndGetValueCases))]
        public void SetAndGetValue_NewAPI(string propertyName, object propertyValue)
        {
            var realmValue = Operator.Convert<RealmValue>(propertyValue);

            RunTestsWithParameters((realm, mode) =>
            {
                var allTypesObject = realm.Write(() =>
                {
                    var ato = (RealmObject)(object)realm.DynamicApi.CreateObject(nameof(AllTypesObject), null);

                    ato.DynamicApi.Set(propertyName, realmValue);

                    return ato;
                });

                Assert.That(allTypesObject.DynamicApi.Get<RealmValue>(propertyName), Is.EqualTo(realmValue));
            });
        }

#if !UNITY
        [TestCaseSource(typeof(AccessTests), nameof(AccessTests.SetAndReplaceWithNullCases))]
#endif
        public void SetValueAndReplaceWithNull<T>(string propertyName, T propertyValue)
        {
            RunTestsWithParameters((realm, mode) =>
            {
                var allTypesObject = realm.Write(() =>
                {
                    dynamic ato = realm.DynamicApi.CreateObject("AllTypesObject", null);

                    InvokeSetter(ato, propertyName, propertyValue, mode);

                    return ato;
                });

                Assert.That((T)InvokeGetter(allTypesObject, propertyName, mode), Is.EqualTo(propertyValue));

                realm.Write(() =>
                {
                    InvokeSetter<object>(allTypesObject, propertyName, null, mode);
                });

                Assert.That(InvokeGetter(allTypesObject, propertyName, mode), Is.EqualTo(null));
            });
        }

        [TestCaseSource(typeof(AccessTests), nameof(AccessTests.SetAndReplaceWithNullCases))]
        public void SetValueAndReplaceWithNull_NewAPI(string propertyName, object propertyValue)
        {
            var realmValue = Operator.Convert<RealmValue>(propertyValue);

            RunTestsWithParameters((realm, mode) =>
            {
                var allTypesObject = realm.Write(() =>
                {
                    var ato = (RealmObject)(object)realm.DynamicApi.CreateObject(nameof(AllTypesObject), null);

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
            RunTestsWithParameters((realm, mode) =>
            {
                var allTypesObject = realm.Write(() =>
                {
                    var ato = (RealmObject)(object)realm.DynamicApi.CreateObject(nameof(AllTypesObject), null);

                    ato.DynamicApi.Set(nameof(AllTypesObject.RealmValueProperty), rv);

                    return ato;
                });

                Assert.That(allTypesObject.DynamicApi.Get<RealmValue>(nameof(AllTypesObject.RealmValueProperty)), Is.EqualTo(rv));

#if !UNITY
                var dynamicAto = realm.Write(() =>
                {
                    dynamic ato = realm.DynamicApi.CreateObject(nameof(AllTypesObject), null);

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
            RunTestsWithParameters((realm, mode) =>
            {
                var (ato, rv) = realm.Write(() =>
                {
                    var intObject = (RealmObject)(object)realm.DynamicApi.CreateObject(nameof(IntPropertyObject), ObjectId.GenerateNewId());
                    intObject.DynamicApi.Set(nameof(IntPropertyObject.Int), 10);

                    var allTypesObject = (RealmObject)(object)realm.DynamicApi.CreateObject(nameof(AllTypesObject), null);
                    allTypesObject.DynamicApi.Set(nameof(AllTypesObject.RealmValueProperty), intObject);
                    return (allTypesObject, (RealmValue)intObject);
                });

                Assert.That(ato.DynamicApi.Get<RealmValue>(nameof(AllTypesObject.RealmValueProperty)), Is.EqualTo(rv));

#if !UNITY
                var (dynamicAto, dynamicRV) = realm.Write(() =>
                {
                    dynamic intObject = realm.DynamicApi.CreateObject(nameof(IntPropertyObject), ObjectId.GenerateNewId());
                    intObject.Int = 10;
                    RealmValue intObjectRV = intObject;

                    dynamic ato = realm.DynamicApi.CreateObject(nameof(AllTypesObject), null);
                    ato.RealmValueProperty = intObjectRV;
                    return (ato, intObjectRV);
                });

                Assert.That((RealmValue)dynamicAto.RealmValueProperty, Is.EqualTo(dynamicRV));
#endif
            });
        }

        private static object InvokeGetter(object o, string propertyName, DynamicTestObjectType mode)
        {
            if (mode == DynamicTestObjectType.DynamicRealmObject)
            {
                var binder = Binder.GetMember(CSharpBinderFlags.None, propertyName, typeof(DynamicAccessTests), new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
                var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);
                return callsite.Target(callsite, o);
            }

            return TestHelpers.GetPropertyValue(o, propertyName);
        }

        private static void InvokeSetter<T>(object o, string propertyName, T propertyValue, DynamicTestObjectType mode)
        {
            if (mode == DynamicTestObjectType.DynamicRealmObject)
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
