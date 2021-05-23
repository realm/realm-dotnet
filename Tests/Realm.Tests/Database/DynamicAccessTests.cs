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
        private void RunTestInAllModes(Action<Realm, bool> test)
        {
            foreach (var isDynamic in new[] { true, false })
            {
                var config = new RealmConfiguration(Guid.NewGuid().ToString())
                {
                    ObjectClasses = new[] { typeof(AllTypesObject), typeof(IntPropertyObject) },
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
                    var ato = (RealmObject)(object)realm.DynamicApi.CreateObject("AllTypesObject", null);
                    if (isDynamic)
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

                if (TestHelpers.IsUnity)
                {
                    return;
                }

                var dynamicAto = realm.Write(() =>
                {
                    dynamic ato = realm.DynamicApi.CreateObject("AllTypesObject", null);
                    if (isDynamic)
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

                Assert.That((char)dynamicAto.CharProperty, Is.EqualTo('F'));
                Assert.That((char)dynamicAto.NullableCharProperty, Is.EqualTo('o'));
                Assert.That(dynamicAto.StringProperty, Is.EqualTo("o"));
            });
        }

#if !UNITY // Unity doesn't support generic test cases
        [TestCaseSource(typeof(AccessTests), nameof(AccessTests.SetAndGetValueCases))]
#endif
        public void SetAndGetValue<T>(string propertyName, T propertyValue)
        {
            RunTestInAllModes((realm, isDynamic) =>
            {
                object allTypesObject;
                using (var transaction = realm.BeginWrite())
                {
                    allTypesObject = realm.DynamicApi.CreateObject("AllTypesObject", null);

                    InvokeSetter(allTypesObject, propertyName, propertyValue, isDynamic);
                    transaction.Commit();
                }

                Assert.That((T)InvokeGetter(allTypesObject, propertyName, isDynamic), Is.EqualTo(propertyValue));
            });
        }

        [TestCaseSource(typeof(AccessTests), nameof(AccessTests.SetAndGetValueCases))]
        public void SetAndGetValue_NewAPI(string propertyName, object propertyValue)
        {
            var realmValue = Operator.Convert<RealmValue>(propertyValue);

            RunTestInAllModes((realm, mode) =>
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

#if !UNITY // Unity doesn't support generic test cases
        [TestCaseSource(typeof(AccessTests), nameof(AccessTests.SetAndReplaceWithNullCases))]
#endif
        public void SetValueAndReplaceWithNull<T>(string propertyName, T propertyValue)
        {
            RunTestInAllModes((realm, mode) =>
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

            RunTestInAllModes((realm, mode) =>
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
            RunTestInAllModes((realm, mode) =>
            {
                var allTypesObject = realm.Write(() =>
                {
                    var ato = (RealmObject)(object)realm.DynamicApi.CreateObject(nameof(AllTypesObject), null);

                    ato.DynamicApi.Set(nameof(AllTypesObject.RealmValueProperty), rv);

                    return ato;
                });

                Assert.That(allTypesObject.DynamicApi.Get<RealmValue>(nameof(AllTypesObject.RealmValueProperty)), Is.EqualTo(rv));

                if (TestHelpers.IsUnity)
                {
                    return;
                }

                var dynamicAto = realm.Write(() =>
                {
                    dynamic ato = realm.DynamicApi.CreateObject(nameof(AllTypesObject), null);

                    ato.RealmValueProperty = rv;

                    return ato;
                });

                Assert.That((RealmValue)dynamicAto.RealmValueProperty, Is.EqualTo(rv));
            });
        }

        [Test]
        public void RealmValueTests_WithObject()
        {
            RunTestInAllModes((realm, mode) =>
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

                if (TestHelpers.IsUnity)
                {
                    return;
                }

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
            });
        }

        private static object InvokeGetter(object o, string propertyName, bool isDynamic)
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
