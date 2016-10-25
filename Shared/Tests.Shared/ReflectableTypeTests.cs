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
using System.Reflection;
using NUnit.Framework;
using Realms;

namespace IntegrationTests.Shared
{
    [TestFixture, Preserve(AllMembers = true)]
    public class ReflectableTypeTests
    {
        protected Realm _realm;

        [SetUp]
        public void Setup()
        {
            Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);
            _realm = Realm.GetInstance();
        }

        [TearDown]
        public void TearDown()
        {
            _realm.Close();
            Realm.DeleteRealm(_realm.Config);
        }

        [Test]
        public void ReflectedGetter_WhenObjectIsRemoved_ShouldReturnNull()
        {
            var owner = new Owner
            {
                TopDog = new Dog
                {
                    Name = "Sharo"
                },
                Name = "Peter"
            };

            _realm.Write(() =>
            {
                _realm.Manage(owner);
            });

            var typeInfo = owner.GetTypeInfo();
            var topDogProperty = typeInfo.GetDeclaredProperty("TopDog");
            var getter = topDogProperty.GetMethod;

            var topDogBeforeRemove = getter.Invoke(owner, null) as Dog;
            Assert.That(topDogBeforeRemove, Is.Not.Null);
            Assert.That(topDogBeforeRemove.Name, Is.EqualTo("Sharo"));

            _realm.Write(() =>
            {
                _realm.Remove(owner);
            });

            var topDogAfterRemove = getter.Invoke(owner, null);
            Assert.That(topDogAfterRemove, Is.Null);
            Assert.Throws<RealmInvalidObjectException>(() =>
            {
                var directDog = owner.TopDog;
            });
        }
    }
}