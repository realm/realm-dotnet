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
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;
using MongoDB.Bson;
using NUnit.Framework;
using Realms.Dynamic;

namespace Realms.Tests.Database
{
    [TestFixture(DynamicTestObjectType.RealmObject)]
    [TestFixture(DynamicTestObjectType.DynamicRealmObject)]
    [Preserve(AllMembers = true)]
    public class DynamicAccessTests : RealmInstanceTest
    {
        private readonly DynamicTestObjectType _mode;

        public DynamicAccessTests(DynamicTestObjectType mode)
        {
            _mode = mode;
        }

        protected override RealmConfiguration CreateConfiguration(string path)
        {
            return new RealmConfiguration(path)
            {
                ObjectClasses = new[] { typeof(AllTypesObject), typeof(IntPropertyObject) },
                IsDynamic = _mode == DynamicTestObjectType.DynamicRealmObject
            };
        }

        [Test]
        public void SimpleTest()
        {
            dynamic allTypesObject;
            using (var transaction = _realm.BeginWrite())
            {
                allTypesObject = _realm.DynamicApi.CreateObject("AllTypesObject", null);
                if (_mode == DynamicTestObjectType.DynamicRealmObject)
                {
                    Assert.That(allTypesObject, Is.InstanceOf<DynamicRealmObject>());
                }
                else
                {
                    Assert.That(allTypesObject, Is.InstanceOf<AllTypesObject>());
                }

                allTypesObject.CharProperty = 'F';
                allTypesObject.NullableCharProperty = 'o';
                allTypesObject.StringProperty = "o";

                transaction.Commit();
            }

            Assert.That((char)allTypesObject.CharProperty, Is.EqualTo('F'));
            Assert.That((char)allTypesObject.NullableCharProperty, Is.EqualTo('o'));
            Assert.That(allTypesObject.StringProperty, Is.EqualTo("o"));
        }

        [TestCaseSource(typeof(AccessTests), nameof(AccessTests.SetAndGetValueCases))]
        public void SetAndGetValue<T>(string propertyName, T propertyValue)
        {
            object allTypesObject;
            using (var transaction = _realm.BeginWrite())
            {
                allTypesObject = _realm.DynamicApi.CreateObject("AllTypesObject", null);

                InvokeSetter(allTypesObject, propertyName, propertyValue);
                transaction.Commit();
            }

            Assert.That((T)InvokeGetter(allTypesObject, propertyName), Is.EqualTo(propertyValue));
        }

        [TestCaseSource(typeof(AccessTests), nameof(AccessTests.SetAndReplaceWithNullCases))]
        public void SetValueAndReplaceWithNull<T>(string propertyName, T propertyValue)
        {
            object allTypesObject;
            using (var transaction = _realm.BeginWrite())
            {
                allTypesObject = _realm.DynamicApi.CreateObject("AllTypesObject", null);

                InvokeSetter(allTypesObject, propertyName, propertyValue);
                transaction.Commit();
            }

            Assert.That((T)InvokeGetter(allTypesObject, propertyName), Is.EqualTo(propertyValue));

            using (var transaction = _realm.BeginWrite())
            {
                InvokeSetter<object>(allTypesObject, propertyName, null);
                transaction.Commit();
            }

            Assert.That(InvokeGetter(allTypesObject, propertyName), Is.EqualTo(null));
        }

        public static IEnumerable<RealmValue> RealmValues = new[]
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

        [TestCaseSource(nameof(RealmValues))]
        public void RealmValueTests(RealmValue rv)
        {
            dynamic ato = null;

            _realm.Write(() =>
            {
                ato = _realm.DynamicApi.CreateObject("AllTypesObject", null);

                ato.RealmValueProperty = rv;
            });

            Assert.That(TestHelpers.RealmValueContentEqual((RealmValue)ato.RealmValueProperty, rv), Is.True);
        }

        [Test]
        public void RealmValueTests_WithObject()
        {
            dynamic ato = null;
            RealmValue rv = RealmValue.Null;

            _realm.Write(() =>
            {
                var intObject = _realm.DynamicApi.CreateObject("IntPropertyObject", null);
                intObject.Int = 10;
                rv = intObject;

                ato = _realm.DynamicApi.CreateObject("AllTypesObject", null);
                ato.RealmValueProperty = rv;
            });

            Assert.That(TestHelpers.RealmValueContentEqual((RealmValue)ato.RealmValueProperty, rv), Is.True);
        }

        private object InvokeGetter(object o, string propertyName)
        {
            if (_mode == DynamicTestObjectType.DynamicRealmObject)
            {
                var binder = Binder.GetMember(CSharpBinderFlags.None, propertyName, typeof(DynamicAccessTests), new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
                var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);
                return callsite.Target(callsite, o);
            }
            else
            {
                return TestHelpers.GetPropertyValue(o, propertyName);
            }
        }

        private void InvokeSetter<T>(object o, string propertyName, T propertyValue)
        {
            if (_mode == DynamicTestObjectType.DynamicRealmObject)
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