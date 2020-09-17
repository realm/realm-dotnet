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
using NUnit.Framework;
using Realms.Dynamic;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class DynamicAccessTests : RealmInstanceTest
    {
        protected override RealmConfiguration CreateConfiguration(string path)
        {
            return new RealmConfiguration(path)
            {
                ObjectClasses = new[] { typeof(AllTypesObject) },
                IsDynamic = true
            };
        }

        [Test]
        public void SimpleTest()
        {
            dynamic allTypesObject;
            using (var transaction = _realm.BeginWrite())
            {
                allTypesObject = _realm.DynamicApi.CreateObject("AllTypesObject", null);
                Assert.That(allTypesObject, Is.InstanceOf<DynamicRealmObject>());

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

                CreateDynamicSetter<T>(propertyName).Invoke(allTypesObject, propertyValue);
                transaction.Commit();
            }

            Assert.That((T)CreateDynamicGetter(propertyName).Invoke(allTypesObject), Is.EqualTo(propertyValue));
        }

        [TestCaseSource(typeof(AccessTests), nameof(AccessTests.SetAndReplaceWithNullCases))]
        public void SetValueAndReplaceWithNull<T>(string propertyName, T propertyValue)
        {
            var getter = CreateDynamicGetter(propertyName);

            object allTypesObject;
            using (var transaction = _realm.BeginWrite())
            {
                allTypesObject = _realm.DynamicApi.CreateObject("AllTypesObject", null);

                CreateDynamicSetter<T>(propertyName).Invoke(allTypesObject, propertyValue);
                transaction.Commit();
            }

            Assert.That((T)getter(allTypesObject), Is.EqualTo(propertyValue));

            using (var transaction = _realm.BeginWrite())
            {
                CreateDynamicSetter<object>(propertyName).Invoke(allTypesObject, null);
                transaction.Commit();
            }

            Assert.That(getter(allTypesObject), Is.EqualTo(null));
        }

        private static Func<object, dynamic> CreateDynamicGetter(string propertyName)
        {
            var binder = Binder.GetMember(CSharpBinderFlags.None, propertyName, typeof(DynamicAccessTests), new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
            var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);
            return (self) => callsite.Target(callsite, self);
        }

        private static Action<object, T> CreateDynamicSetter<T>(string propertyName)
        {
            var binder = Binder.SetMember(CSharpBinderFlags.None, propertyName, typeof(DynamicAccessTests), new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null) });
            var callsite = CallSite<Func<CallSite, object, T, object>>.Create(binder);
            return (self, value) => callsite.Target(callsite, self, value);
        }
    }
}