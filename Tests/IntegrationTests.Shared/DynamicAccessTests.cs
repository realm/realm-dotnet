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
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Realms;
using System.Linq.Expressions;
using Microsoft.CSharp.RuntimeBinder;
using System.Dynamic;
using System.Runtime.CompilerServices;

namespace IntegrationTests.Shared
{
    [TestFixture]
    public class DynamicAccessTests
    {
        protected Realm _realm;

        [SetUp]
        public void Setup()
        {
            var config = new RealmConfiguration();
            config.Schema = RealmSchema.CreateSchemaForClasses(new[] { typeof(AllTypesObject) }).DynamicClone();
            Realm.DeleteRealm(config);
            _realm = Realm.GetInstance(config);
        }

        [TearDown]
        public void TearDown()
        {
            _realm.Close();
            Realm.DeleteRealm(_realm.Config);
        }

        [Test]
        public void SimpleTest()
        {
            dynamic allTypesObject;
            using (var transaction = _realm.BeginWrite())
            {
                allTypesObject = _realm.CreateObject("AllTypesObject");
                Assert.That(allTypesObject, Is.InstanceOf<Realms.Dynamic.DynamicRealmObject>());

                allTypesObject.CharProperty = 'F';
                allTypesObject.NullableCharProperty = 'o';
                allTypesObject.StringProperty = "o";

                transaction.Commit();
            }

            Assert.That((char)(allTypesObject.CharProperty), Is.EqualTo('F'));
            Assert.That((char)(allTypesObject.NullableCharProperty), Is.EqualTo('o'));
            Assert.That(allTypesObject.StringProperty, Is.EqualTo("o"));
        }

        [TestCaseSource(typeof(AccessTests), nameof(AccessTests.SetAndGetValueCases))]
        public void SetAndGetValue(string propertyName, object propertyValue)
        {
            var getter = CreateDynamicGetter(propertyName);
            var setter = CreateDynamicSetter(propertyName);

            object allTypesObject;
            using (var transaction = _realm.BeginWrite())
            {
                allTypesObject = _realm.CreateObject("AllTypesObject");

                setter(allTypesObject, propertyValue);
                transaction.Commit();
            }

            Assert.That(Convert.ChangeType(getter(allTypesObject), propertyValue.GetType()), Is.EqualTo(propertyValue));
        }

        [TestCaseSource(typeof(AccessTests), nameof(AccessTests.SetAndReplaceWithNullCases))]
        public void SetValueAndReplaceWithNull(string propertyName, object propertyValue)
        {
            var getter = CreateDynamicGetter(propertyName);
            var setter = CreateDynamicSetter(propertyName);

            object allTypesObject;
            using (var transaction = _realm.BeginWrite())
            {
                allTypesObject = _realm.CreateObject("AllTypesObject");

                setter(allTypesObject, propertyValue);
                transaction.Commit();
            }

            Assert.That(Convert.ChangeType(getter(allTypesObject), propertyValue.GetType()), Is.EqualTo(propertyValue));

            using (var transaction = _realm.BeginWrite())
            {
                setter(allTypesObject, null);
                transaction.Commit();
            }

            Assert.That(getter(allTypesObject), Is.EqualTo(null));
        }

        private static Func<object, object> CreateDynamicGetter(string propertyName)
        {
            var binder = Binder.GetMember(CSharpBinderFlags.None, propertyName, typeof(DynamicAccessTests), new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
            var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);
            return (self) => callsite.Target(callsite, self);
        }

        private static Action<object, object> CreateDynamicSetter(string propertyName)
        {
            var binder = Binder.SetMember(CSharpBinderFlags.None, propertyName, typeof(DynamicAccessTests), new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
            var callsite = CallSite<Func<CallSite, object, object, object>>.Create(binder);
            return (self, value) => callsite.Target(callsite, self, value);
        }
    }
}
